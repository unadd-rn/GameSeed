// ═══════════════════════════════════════════════════════════════════════════
//  PortraitAnimator.cs  —  Mobile WebGL–optimised portrait animation system
// ═══════════════════════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#region Enums

public enum AnimationType { Slide, SpinZoom, Wipe, Bounce, Shake, Fade }
public enum WipeDirection  { Left, Right, Up, Down }

#endregion

#region Serialisable data

[System.Serializable]
public class PortraitEventAnimation
{
    public AnimationType animationType = AnimationType.Slide;

    [Tooltip("Seconds before the NEXT portrait starts after this animation triggers.")]
    public float followDelay = 0.12f;

    [ColorUsage(true, true)]
    public Color   trailColor      = Color.white;
    public Vector2 maximumTrailLag  = new Vector2(0.08f, 0.02f);
    public Vector2 maximumTrailLag2 = new Vector2(0.15f, 0.04f);
    public Vector4 finalTrailOffset = Vector4.zero;

    public Vector2 offScreenOffset = new Vector2(-1200f, 0f);
    public float   slideRotation   = 0f;
    public float   slideDuration   = 0.45f;

    public float spinDegrees      = 360f;
    public float zoomInScale      = 0.01f;
    public float spinZoomDuration = 0.6f;

    [Tooltip("RectTransform on a RectMask2D that is a parent of this portrait.")]
    public RectTransform wipeMask;
    public WipeDirection  wipeDirection = WipeDirection.Left;
    public float          wipeDuration  = 0.5f;

    public float bounceStartScale = 0.01f;
    public float bounceOvershoot  = 1.15f;
    public float bounceDuration   = 0.5f;

    public float shakeStrength   = 30f;
    public int   shakeVibrato    = 20;
    public float shakeRandomness = 90f;
    public float shakeDuration   = 0.6f;

    [Tooltip("CanvasGroup on or above the portrait for fading.")]
    public CanvasGroup canvasGroup;
    public float       fadeDuration = 0.4f;

    [HideInInspector] public Vector2 wipeMaskFullSize;
    [HideInInspector] public Vector2 wipeMaskHiddenSize;
}

[System.Serializable]
public class PortraitEventEntry
{
    [Tooltip("Call PlayEventIn(\"MyEvent\") to trigger this.")]
    public string eventName = "Default";

    public PortraitEventAnimation inAnimation  = new PortraitEventAnimation();
    public PortraitEventAnimation outAnimation = new PortraitEventAnimation();
}

[System.Serializable]
public class PortraitData
{
    [Tooltip("Label shown in the Inspector — no effect at runtime.")]
    public string label = "Portrait";

    [Tooltip("The portrait's RectTransform. Can be in ANY parent in the hierarchy.")]
    public RectTransform uiPortrait;

    [Tooltip("One entry per named event.")]
    public List<PortraitEventEntry> events = new List<PortraitEventEntry>();

    // ── Runtime cache ────────────────────────────────────────────────────────
    [HideInInspector] public Material  uniqueMaterial;
    [HideInInspector] public Vector3   originalScale = Vector3.one;
    [HideInInspector] public Vector2   localRestPosition;   // Captured local position on boot
    [HideInInspector] public Coroutine trailCoroutine;
    [HideInInspector] public bool      isVisible;           // Explicit individual state tracking
}

#endregion

public class PortraitAnimator : MonoBehaviour
{
    #region Inspector

    [Header("Portraits")]
    public List<PortraitData> portraits = new List<PortraitData>();

    [Header("Toggle Button (optional)")]
    public Button toggleButton;
    [Tooltip("Which event the toggle button controls.")]
    public string toggleEventName = "Default";

    #endregion

    #region Private state

    private static readonly int TrailOffsetID  = Shader.PropertyToID("_TrailOffset");
    private static readonly int TrailOffset2ID = Shader.PropertyToID("_TrailOffset2");
    private static readonly int TrailColorID   = Shader.PropertyToID("_TrailColor");

    private static readonly AnimationCurve _easeOutQuint = MakeCurve(5f, 0f);
    private static readonly AnimationCurve _easeInQuint  = MakeCurve(0f, 5f);
    private static readonly AnimationCurve _easeOutQuad  = MakeCurve(2f, 0f);
    private static readonly AnimationCurve _easeInQuad   = MakeCurve(0f, 2f);

    #endregion

    #region Unity lifecycle

    private void Awake()
    {
        CacheOriginalScales();
        ResolveRestPositions(); 
        InitWipeMasks();
        WarmUpShaders();        
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(200, 50);
    }

    private void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(OnTogglePressed);

        ResetAll();
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(OnTogglePressed);

        for (int i = 0; i < portraits.Count; i++)
        {
            var p = portraits[i];
            if (p.uniqueMaterial != null)
            {
                Destroy(p.uniqueMaterial);
                p.uniqueMaterial = null;
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Returns whether a specific portrait index is currently animated/active on screen.
    /// </summary>
    public bool IsPortraitVisible(int index)
    {
        if ((uint)index >= (uint)portraits.Count) return false;
        return portraits[index].isVisible;
    }

    public void PlayEventIn(string eventName)
    {
        float delay = 0f;
        for (int i = 0; i < portraits.Count; i++)
        {
            var portrait = portraits[i];
            if (portrait.uiPortrait == null || portrait.isVisible) continue; // Skip if already visible
            
            var entry = FindEntry(portrait, eventName);
            if (entry == null) continue;
            
            PlayIn(portrait, entry.inAnimation, delay);
            delay += entry.inAnimation.followDelay; 
        }
    }

    public void PlayEventOut(string eventName)
    {
        float delay = 0f;
        for (int i = portraits.Count - 1; i >= 0; i--)
        {
            var portrait = portraits[i];
            if (portrait.uiPortrait == null || !portrait.isVisible) continue; // Skip if already hidden
            
            var entry = FindEntry(portrait, eventName);
            if (entry == null) continue;
            
            PlayOut(portrait, entry.outAnimation, delay);
            delay += entry.outAnimation.followDelay; 
        }
    }

    public void PlayToggleEvent(string eventName)
    {
        // Toggle behavior checks if the majority of valid portraits are currently active on screen
        int visibleCount = 0;
        int activePortraits = 0;

        for (int i = 0; i < portraits.Count; i++)
        {
            if (portraits[i].uiPortrait != null)
            {
                activePortraits++;
                if (portraits[i].isVisible) visibleCount++;
            }
        }

        if (visibleCount > activePortraits / 2) 
            PlayEventOut(eventName);
        else                                     
            PlayEventIn(eventName);
    }

    public void PlayPortraitEventIn(int index, string eventName, float delay = 0f)
    {
        if ((uint)index >= (uint)portraits.Count) return;
        var portrait = portraits[index];
        if (portrait.uiPortrait == null || portrait.isVisible) return; // Prevent double In-animations

        var entry = FindEntry(portrait, eventName);
        if (entry == null) return;
        PlayIn(portrait, entry.inAnimation, delay);
    }

    public void PlayPortraitEventOut(int index, string eventName, float delay = 0f)
    {
        if ((uint)index >= (uint)portraits.Count) return;
        var portrait = portraits[index];
        if (portrait.uiPortrait == null || !portrait.isVisible) return; // Prevent double Out-animations

        var entry = FindEntry(portrait, eventName);
        if (entry == null) return;
        PlayOut(portrait, entry.outAnimation, delay);
    }

    public void ResetAll()
    {
        for (int i = 0; i < portraits.Count; i++)
        {
            var p = portraits[i];
            if (p.uiPortrait == null) continue;
            p.isVisible = false; 
            KillAll(p);
            SnapToHidden(p);
        }
    }

    #endregion

    #region Initialisation

    public void WarmUpShaders()
    {
        for (int i = 0; i < portraits.Count; i++)
        {
            var p = portraits[i];
            if (p.uiPortrait == null) continue;

            var img = p.uiPortrait.GetComponent<Image>();
            if (img == null || img.material == null) continue;

            p.uniqueMaterial = Instantiate(img.material);
            img.material     = p.uniqueMaterial;

            p.uniqueMaterial.SetVector(TrailOffsetID,  Vector4.zero);
            p.uniqueMaterial.SetVector(TrailOffset2ID, Vector4.zero);
        }
    }

    private void CacheOriginalScales()
    {
        for (int i = 0; i < portraits.Count; i++)
        {
            var p = portraits[i];
            if (p.uiPortrait == null) continue;
            var s = p.uiPortrait.localScale;
            p.originalScale = (s.sqrMagnitude < 0.001f) ? Vector3.one : s;
        }
    }

    private void ResolveRestPositions()
    {
        for (int i = 0; i < portraits.Count; i++)
        {
            var p = portraits[i];
            if (p.uiPortrait == null) continue;
            p.localRestPosition = p.uiPortrait.anchoredPosition;
        }
    }

    private void InitWipeMasks()
    {
        for (int i = 0; i < portraits.Count; i++)
        {
            var p = portraits[i];
            for (int j = 0; j < p.events.Count; j++)
            {
                CacheWipe(p.events[j].inAnimation);
                CacheWipe(p.events[j].outAnimation);
            }
        }
    }

    private static void CacheWipe(PortraitEventAnimation a)
    {
        if (a.wipeMask == null) return;
        a.wipeMaskFullSize   = a.wipeMask.sizeDelta;
        a.wipeMaskHiddenSize = CalcHiddenSize(a);
    }

    #endregion

    #region Play In / Out

    private void PlayIn(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.isVisible = true; // State updated right away to lock incoming checks
        KillAll(p);
        float dur = Duration(anim);

        var from1 = new Vector4( anim.maximumTrailLag.x,  anim.maximumTrailLag.y,  0f, 0f);
        var from2 = new Vector4( anim.maximumTrailLag2.x, anim.maximumTrailLag2.y, 0f, 0f);

        LaunchTrail(p, anim, from1, anim.finalTrailOffset, from2, anim.finalTrailOffset, dur, true, delay);

        switch (anim.animationType)
        {
            case AnimationType.Slide:    DoSlideIn(p, anim, delay);    break;
            case AnimationType.SpinZoom: DoSpinZoomIn(p, anim, delay); break;
            case AnimationType.Wipe:     DoWipeIn(anim, delay);        break;
            case AnimationType.Bounce:   DoBounceIn(p, anim, delay);   break;
            case AnimationType.Shake:    DoShake(p, anim, delay);      break;
            case AnimationType.Fade:     DoFadeIn(p, anim, delay);     break;
        }
    }

    private void PlayOut(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.isVisible = false; // State updated right away to lock outgoing checks
        KillAll(p);
        float dur = Duration(anim);

        var to1 = new Vector4(-anim.maximumTrailLag.x,  anim.maximumTrailLag.y,  0f, 0f);
        var to2 = new Vector4(-anim.maximumTrailLag2.x, anim.maximumTrailLag2.y, 0f, 0f);

        LaunchTrail(p, anim, anim.finalTrailOffset, to1, anim.finalTrailOffset, to2, dur, false, delay);

        switch (anim.animationType)
        {
            case AnimationType.Slide:    DoSlideOut(p, anim, delay);    break;
            case AnimationType.SpinZoom: DoSpinZoomOut(p, anim, delay); break;
            case AnimationType.Wipe:     DoWipeOut(anim, delay);        break;
            case AnimationType.Bounce:   DoBounceOut(p, anim, delay);   break;
            case AnimationType.Shake:    DoShake(p, anim, delay);       break;
            case AnimationType.Fade:     DoFadeOut(p, anim, delay);     break;
        }
    }

    private void SnapToHidden(PortraitData p)
    {
        if (p.uniqueMaterial != null)
        {
            p.uniqueMaterial.SetVector(TrailOffsetID,  Vector4.zero);
            p.uniqueMaterial.SetVector(TrailOffset2ID, Vector4.zero);
        }

        if (p.events.Count == 0) return;

        var anim = p.events[0].inAnimation;
        switch (anim.animationType)
        {
            case AnimationType.Slide:
                p.uiPortrait.anchoredPosition = p.localRestPosition + anim.offScreenOffset;
                p.uiPortrait.localEulerAngles = new Vector3(0f, 0f, anim.slideRotation);
                break;
            case AnimationType.SpinZoom:
                p.uiPortrait.anchoredPosition = p.localRestPosition;
                p.uiPortrait.localScale       = p.originalScale * Mathf.Max(0.01f, anim.zoomInScale);
                p.uiPortrait.localEulerAngles = new Vector3(0f, 0f, -anim.spinDegrees);
                break;
            case AnimationType.Wipe:
                p.uiPortrait.anchoredPosition = p.localRestPosition;
                if (anim.wipeMask != null) anim.wipeMask.sizeDelta = anim.wipeMaskHiddenSize;
                break;
            case AnimationType.Bounce:
                p.uiPortrait.anchoredPosition = p.localRestPosition;
                p.uiPortrait.localScale       = p.originalScale * Mathf.Max(0.01f, anim.bounceStartScale);
                break;
            case AnimationType.Shake:
                p.uiPortrait.anchoredPosition = p.localRestPosition;
                break;
            case AnimationType.Fade:
                p.uiPortrait.anchoredPosition = p.localRestPosition;
                if (anim.canvasGroup != null) anim.canvasGroup.alpha = 0f;
                break;
        }
    }

    #endregion

    #region Animation Methods

    private void DoSlideIn(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.uiPortrait.anchoredPosition = p.localRestPosition + anim.offScreenOffset;
        p.uiPortrait.localEulerAngles = new Vector3(0f, 0f, anim.slideRotation);

        p.uiPortrait.DOAnchorPos(p.localRestPosition, anim.slideDuration).SetEase(Ease.OutQuint).SetDelay(delay);
        p.uiPortrait.DOLocalRotate(Vector3.zero, anim.slideDuration, RotateMode.Fast).SetEase(Ease.OutQuint).SetDelay(delay);
    }

    private void DoSlideOut(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.uiPortrait.anchoredPosition = p.localRestPosition;
        p.uiPortrait.localEulerAngles = Vector3.zero;

        p.uiPortrait.DOAnchorPos(p.localRestPosition + anim.offScreenOffset, anim.slideDuration).SetEase(Ease.InQuint).SetDelay(delay);
        p.uiPortrait.DOLocalRotate(new Vector3(0f, 0f, anim.slideRotation), anim.slideDuration, RotateMode.Fast).SetEase(Ease.InQuint).SetDelay(delay);
    }

    private void DoSpinZoomIn(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.uiPortrait.anchoredPosition = p.localRestPosition;
        p.uiPortrait.localScale       = p.originalScale * Mathf.Max(0.01f, anim.zoomInScale);
        p.uiPortrait.localEulerAngles = new Vector3(0f, 0f, -anim.spinDegrees);

        p.uiPortrait.DOScale(p.originalScale, anim.spinZoomDuration).SetEase(Ease.OutBack).SetDelay(delay);
        p.uiPortrait.DOLocalRotate(Vector3.zero, anim.spinZoomDuration, RotateMode.Fast).SetEase(Ease.OutQuint).SetDelay(delay);
    }

    private void DoSpinZoomOut(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.uiPortrait.DOScale(p.originalScale * Mathf.Max(0.01f, anim.zoomInScale), anim.spinZoomDuration).SetEase(Ease.InBack).SetDelay(delay);
        p.uiPortrait.DOLocalRotate(new Vector3(0f, 0f, anim.spinDegrees), anim.spinZoomDuration, RotateMode.FastBeyond360).SetEase(Ease.InQuint).SetDelay(delay);
    }

    private static void DoWipeIn(PortraitEventAnimation anim, float delay)
    {
        if (anim.wipeMask == null) return;
        anim.wipeMask.sizeDelta = anim.wipeMaskHiddenSize;
        anim.wipeMask.DOSizeDelta(anim.wipeMaskFullSize, anim.wipeDuration).SetEase(Ease.OutQuart).SetDelay(delay);
    }

    private static void DoWipeOut(PortraitEventAnimation anim, float delay)
    {
        if (anim.wipeMask == null) return;
        anim.wipeMask.sizeDelta = anim.wipeMaskFullSize;
        anim.wipeMask.DOSizeDelta(anim.wipeMaskHiddenSize, anim.wipeDuration).SetEase(Ease.InQuart).SetDelay(delay);
    }

    private void DoBounceIn(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.uiPortrait.anchoredPosition = p.localRestPosition;
        p.uiPortrait.localScale       = p.originalScale * Mathf.Max(0.01f, anim.bounceStartScale);

        Sequence seq = DOTween.Sequence().SetDelay(delay);
        seq.Append(p.uiPortrait.DOScale(p.originalScale * anim.bounceOvershoot, anim.bounceDuration * 0.55f).SetEase(Ease.OutQuad));
        seq.Append(p.uiPortrait.DOScale(p.originalScale * 0.90f, anim.bounceDuration * 0.20f).SetEase(Ease.InOutQuad));
        seq.Append(p.uiPortrait.DOScale(p.originalScale * 1.05f, anim.bounceDuration * 0.15f).SetEase(Ease.InOutQuad));
        seq.Append(p.uiPortrait.DOScale(p.originalScale, anim.bounceDuration * 0.10f).SetEase(Ease.InOutQuad));
        
        seq.OnUpdate(() => p.uiPortrait.anchoredPosition = p.localRestPosition);
    }

    private void DoBounceOut(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.uiPortrait.anchoredPosition = p.localRestPosition;
        
        Sequence seq = DOTween.Sequence().SetDelay(delay);
        seq.Append(p.uiPortrait.DOScale(p.originalScale * 1.10f, anim.bounceDuration * 0.20f).SetEase(Ease.OutQuad));
        seq.Append(p.uiPortrait.DOScale(p.originalScale * Mathf.Max(0.01f, anim.bounceStartScale), anim.bounceDuration * 0.80f).SetEase(Ease.InQuad));
        
        seq.OnUpdate(() => p.uiPortrait.anchoredPosition = p.localRestPosition);
    }

    private void DoShake(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        p.uiPortrait.anchoredPosition = p.localRestPosition;
        Vector2 rest = p.localRestPosition;

        p.uiPortrait.DOShakeAnchorPos(anim.shakeDuration, anim.shakeStrength, anim.shakeVibrato, anim.shakeRandomness, false, true)
            .SetDelay(delay)
            .OnComplete(() => p.uiPortrait.anchoredPosition = rest);
    }

    private void DoFadeIn(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        if (anim.canvasGroup == null) return;
        p.uiPortrait.anchoredPosition = p.localRestPosition;
        anim.canvasGroup.alpha = 0f;
        anim.canvasGroup.DOFade(1f, anim.fadeDuration).SetEase(Ease.OutQuad).SetDelay(delay);
    }

    private void DoFadeOut(PortraitData p, PortraitEventAnimation anim, float delay)
    {
        if (anim.canvasGroup == null) return;
        anim.canvasGroup.DOFade(0f, anim.fadeDuration).SetEase(Ease.InQuad).SetDelay(delay);
    }

    #endregion

    #region Trail coroutine

    private void LaunchTrail(PortraitData p, PortraitEventAnimation anim, Vector4 from1, Vector4 to1, Vector4 from2, Vector4 to2, float duration, bool easeOut, float delay)
    {
        if (p.uniqueMaterial == null) return;

        if (p.trailCoroutine != null)
        {
            StopCoroutine(p.trailCoroutine);
            p.trailCoroutine = null;
        }

        p.trailCoroutine = StartCoroutine(TrailRoutine(p, anim, from1, to1, from2, to2, duration, easeOut, delay));
    }

    private IEnumerator TrailRoutine(PortraitData p, PortraitEventAnimation anim, Vector4 from1, Vector4 to1, Vector4 from2, Vector4 to2, float duration, bool easeOut, float delay)
    {
        var mat = p.uniqueMaterial;
        if (mat == null) yield break;

        mat.SetColor(TrailColorID,   anim.trailColor);
        mat.SetVector(TrailOffsetID,  from1);
        mat.SetVector(TrailOffset2ID, from2);

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        AnimationCurve c1 = easeOut ? _easeOutQuint : _easeInQuint;
        AnimationCurve c2 = easeOut ? _easeOutQuad  : _easeInQuad;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            mat.SetVector(TrailOffsetID,  Vector4.LerpUnclamped(from1, to1, c1.Evaluate(t)));
            mat.SetVector(TrailOffset2ID, Vector4.LerpUnclamped(from2, to2, c2.Evaluate(t)));

            yield return null;
        }

        mat.SetVector(TrailOffsetID,  to1);
        mat.SetVector(TrailOffset2ID, to2);
        p.trailCoroutine = null;
    }

    #endregion

    #region Helpers

    private void OnTogglePressed() => PlayToggleEvent(toggleEventName);

    private void KillAll(PortraitData p)
    {
        p.uiPortrait.DOKill();

        for (int i = 0; i < p.events.Count; i++)
        {
            var ev = p.events[i];
            ev.inAnimation.canvasGroup?.DOKill();
            ev.outAnimation.canvasGroup?.DOKill();
            ev.inAnimation.wipeMask?.DOKill();
            ev.outAnimation.wipeMask?.DOKill();
        }

        if (p.trailCoroutine != null)
        {
            StopCoroutine(p.trailCoroutine);
            p.trailCoroutine = null;
        }
    }

    private static PortraitEventEntry FindEntry(PortraitData p, string eventName)
    {
        PortraitEventEntry fallback = null;
        for (int i = 0; i < p.events.Count; i++)
        {
            var ev = p.events[i];
            if (ev.eventName == eventName) return ev;
            if (ev.eventName == "Default") fallback = ev;
        }
        return fallback;
    }

    private static float Duration(PortraitEventAnimation a) => a.animationType switch
    {
        AnimationType.Slide    => a.slideDuration,
        AnimationType.SpinZoom => a.spinZoomDuration,
        AnimationType.Wipe     => a.wipeDuration,
        AnimationType.Bounce   => a.bounceDuration,
        AnimationType.Shake    => a.shakeDuration,
        AnimationType.Fade     => a.fadeDuration,
        _                      => 0.5f
    };

    private static Vector2 CalcHiddenSize(PortraitEventAnimation a) => a.wipeDirection switch
    {
        WipeDirection.Left  or WipeDirection.Right => new Vector2(0f, a.wipeMaskFullSize.y),
        WipeDirection.Up    or WipeDirection.Down  => new Vector2(a.wipeMaskFullSize.x, 0f),
        _                                          => Vector2.zero
    };

    private static AnimationCurve MakeCurve(float startOut, float endIn) =>
        new AnimationCurve(new Keyframe(0f, 0f, 0f, startOut), new Keyframe(1f, 1f, endIn, 0f));

    #endregion
}