using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class tesst : MonoBehaviour
{
    public enum AnimationType { Slide, SpinZoom, Wipe, Bounce, Shake, Fade }
    public enum WipeDirection { Left, Right, Up, Down }

    [System.Serializable]
    public class PortraitAnimationData
    {
        public string label = "Portrait Element";
        public RectTransform uiPortrait;

        [Header("Animation Type")]
        public AnimationType animationType = AnimationType.Slide;

        [Header("Canvas Group (Required for Fade)")]
        [Tooltip("Assign a CanvasGroup component on or above the portrait to handle fading.")]
        public CanvasGroup canvasGroup;

        [Header("Position")]
        public Vector2 offScreenPosition = new Vector2(-1200f, 0f);
        public Vector2 onScreenPosition = Vector2.zero;

        [Header("Trail Settings (Global Settings)")]
        [ColorUsage(true, true)] public Color trailColor = Color.white;
        public Vector2 maximumTrailLag = new Vector2(0.08f, 0.02f);
        [Tooltip("Secondary trail lag (should be higher/different to look slower).")]
        public Vector2 maximumTrailLag2 = new Vector2(0.15f, 0.04f);
        [Tooltip("Trail offset remaining when the tween is fully complete.")]
        public Vector4 finalTrailOffset = Vector4.zero;

        [Header("Timing")]
        public float followDelay = 0.12f;

        [Header("Slide Settings")]
        public float slideDuration = 0.45f;

        [Header("Spin & Zoom Settings")]
        public float spinDegrees = 360f;
        public float zoomInScale = 0.01f;
        public float zoomOutScale = 2f;
        public float spinZoomDuration = 0.6f;

        [Header("Wipe Settings")]
        [Tooltip("Assign the RectTransform of a RectMask2D parent GameObject.")]
        public RectTransform wipeMask;
        public WipeDirection wipeDirection = WipeDirection.Left;
        public float wipeDuration = 0.5f;

        [Header("Bounce Settings")]
        public float bounceStartScale = 0.01f;
        public float bounceOvershoot = 1.25f;
        public float bounceDuration = 0.55f;

        [Header("Shake Settings")]
        public float shakeStrength = 30f;
        public int shakeVibrato = 20;
        public float shakeRandomness = 90f;
        public float shakeDuration = 0.6f;

        [Header("Fade Settings")]
        public float fadeDuration = 0.4f;

        [HideInInspector] public Material uniqueMaterial;
        [HideInInspector] public int materialInstanceId;
        [HideInInspector] public Vector2 wipeMaskFullSize;
        [HideInInspector] public Vector2 wipeMaskHiddenSize;
        [HideInInspector] public Vector3 originalScale = Vector3.one;

        // --- FIX: Coroutine handle replaces DOVector tweens ---
        [HideInInspector] public Coroutine trailCoroutine;
    }

    [Header("Portraits Setup")]
    public List<PortraitAnimationData> portraits = new List<PortraitAnimationData>();

    [Header("Toggle Button (optional)")]
    public Button toggleButton;

    private bool _isShownState = false;

    // Cached property IDs
    private static readonly int TrailOffsetID  = Shader.PropertyToID("_TrailOffset");
    private static readonly int TrailOffset2ID = Shader.PropertyToID("_TrailOffset2");
    private static readonly int TrailColorID   = Shader.PropertyToID("_TrailColor");

    // --- FIX: Reusable ease curves matching the original DOVector ease pairs ---
    // Trail1 used Ease.OutQuint (in) / Ease.InQuint (out)
    // Trail2 used Ease.OutQuad  (in) / Ease.InQuad  (out)
    // Keeping them separate is what creates the visible lag between the two trails.
    private static readonly AnimationCurve _easeOutQuint = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 5f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    private static readonly AnimationCurve _easeInQuint = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(1f, 1f, 5f, 0f)
    );
    private static readonly AnimationCurve _easeOutQuad = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    private static readonly AnimationCurve _easeInQuad = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(1f, 1f, 2f, 0f)
    );

    void Awake()
    {
        WarmUpShaders();
        InitializeWipeMasks();
        CacheOriginalScales();
    }

    void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(PlayToggle);

        ResetToHiddenState();
                PlayToggle();

    }

    void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(PlayToggle);
    }

    /// <summary>
    /// Instantiates materials and forces WebGL/GPU compilation BEFORE gameplay.
    /// Call this while your loading bar is visible.
    /// </summary>
    public void WarmUpShaders()
    {
        for (int i = 0; i < portraits.Count; i++)
        {
            var data = portraits[i];
            if (data.uiPortrait == null) continue;

            var img = data.uiPortrait.GetComponent<Image>();
            if (img != null && img.material != null)
            {
                data.uniqueMaterial = Instantiate(img.material);
                data.uniqueMaterial.SetColor(TrailColorID, data.trailColor);
                data.materialInstanceId = data.uniqueMaterial.GetInstanceID();
                img.material = data.uniqueMaterial;

                data.uniqueMaterial.SetVector(TrailOffsetID,  data.finalTrailOffset);
                data.uniqueMaterial.SetVector(TrailOffset2ID, data.finalTrailOffset);
            }
        }
    }

    private void CacheOriginalScales()
    {
        foreach (var data in portraits)
        {
            if (data.uiPortrait != null)
                data.originalScale = (data.uiPortrait.localScale.sqrMagnitude < 0.001f)
                    ? Vector3.one
                    : data.uiPortrait.localScale;
        }
    }

    private void InitializeWipeMasks()
    {
        foreach (var data in portraits)
        {
            if (data.wipeMask == null) continue;
            data.wipeMaskFullSize   = data.wipeMask.sizeDelta;
            data.wipeMaskHiddenSize = GetWipeHiddenSize(data);
        }
    }

    public void ResetToHiddenState()
    {
        _isShownState = false;
        foreach (var data in portraits)
        {
            if (data.uiPortrait == null) continue;
            KillTweens(data);
            ResetTrailToFixedEnd(data);

            switch (data.animationType)
            {
                case AnimationType.Slide:
                    data.uiPortrait.anchoredPosition = data.offScreenPosition;
                    break;
                case AnimationType.SpinZoom:
                    data.uiPortrait.anchoredPosition = data.onScreenPosition;
                    data.uiPortrait.localScale       = data.originalScale * Mathf.Max(0.01f, data.zoomInScale);
                    data.uiPortrait.localEulerAngles = new Vector3(0f, 0f, -data.spinDegrees);
                    break;
                case AnimationType.Wipe:
                    data.uiPortrait.anchoredPosition = data.onScreenPosition;
                    if (data.wipeMask != null) data.wipeMask.sizeDelta = data.wipeMaskHiddenSize;
                    break;
                case AnimationType.Bounce:
                    data.uiPortrait.anchoredPosition = data.onScreenPosition;
                    data.uiPortrait.localScale       = data.originalScale * Mathf.Max(0.01f, data.bounceStartScale);
                    break;
                case AnimationType.Shake:
                    data.uiPortrait.anchoredPosition = data.onScreenPosition;
                    break;
                case AnimationType.Fade:
                    data.uiPortrait.anchoredPosition = data.onScreenPosition;
                    if (data.canvasGroup != null) data.canvasGroup.alpha = 0f;
                    break;
            }
        }
    }

    private void KillTweens(PortraitAnimationData data)
    {
        data.uiPortrait.DOKill();
        data.canvasGroup?.DOKill();
        data.wipeMask?.DOKill();

        // --- FIX: Stop the coroutine instead of killing DOVector tweens ---
        if (data.trailCoroutine != null)
        {
            StopCoroutine(data.trailCoroutine);
            data.trailCoroutine = null;
        }
    }

    private void ResetTrailToFixedEnd(PortraitAnimationData data)
    {
        if (data.uniqueMaterial == null) return;
        data.uniqueMaterial.SetColor(TrailColorID,   data.trailColor);
        data.uniqueMaterial.SetVector(TrailOffsetID,  data.finalTrailOffset);
        data.uniqueMaterial.SetVector(TrailOffset2ID, data.finalTrailOffset);
    }

    private float GetAnimationDuration(PortraitAnimationData data)
    {
        return data.animationType switch
        {
            AnimationType.Slide    => data.slideDuration,
            AnimationType.SpinZoom => data.spinZoomDuration,
            AnimationType.Wipe     => data.wipeDuration,
            AnimationType.Bounce   => data.bounceDuration,
            AnimationType.Shake    => data.shakeDuration,
            AnimationType.Fade     => data.fadeDuration,
            _                      => 0.5f
        };
    }

    // --- FIX: Coroutine replaces DOVector — stays fully inside wasm, no JS bridge crossing ---
    // Trail1 uses OutQuint/InQuint, Trail2 uses OutQuad/InQuad.
    // Using DIFFERENT curves per trail is what makes trail2 lag behind trail1 visually.
    private IEnumerator AnimateTrailCoroutine(
        PortraitAnimationData data,
        Vector4 from1, Vector4 to1,
        Vector4 from2, Vector4 to2,
        float duration,
        bool easeOut,   // true = in-animation (Out curves), false = out-animation (In curves)
        float delay)
    {
        if (data.uniqueMaterial == null) yield break;

        data.uniqueMaterial.SetColor(TrailColorID,   data.trailColor);
        data.uniqueMaterial.SetVector(TrailOffsetID,  from1);
        data.uniqueMaterial.SetVector(TrailOffset2ID, from2);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // Trail1 = sharper/faster curve (Quint), Trail2 = softer/slower curve (Quad)
        // The difference in speed between them is exactly what creates the visible trail gap.
        AnimationCurve curve1 = easeOut ? _easeOutQuint : _easeInQuint;
        AnimationCurve curve2 = easeOut ? _easeOutQuad  : _easeInQuad;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            data.uniqueMaterial.SetVector(TrailOffsetID,  Vector4.LerpUnclamped(from1, to1, curve1.Evaluate(t)));
            data.uniqueMaterial.SetVector(TrailOffset2ID, Vector4.LerpUnclamped(from2, to2, curve2.Evaluate(t)));

            yield return null;
        }

        // Snap to final values cleanly
        data.uniqueMaterial.SetVector(TrailOffsetID,  to1);
        data.uniqueMaterial.SetVector(TrailOffset2ID, to2);
        data.trailCoroutine = null;
    }

    private void StartTrailAnimation(PortraitAnimationData data,
                                     Vector4 from1, Vector4 to1,
                                     Vector4 from2, Vector4 to2,
                                     float duration, bool easeOut, float delay)
    {
        if (data.uniqueMaterial == null) return;
        if (data.trailCoroutine != null)
        {
            StopCoroutine(data.trailCoroutine);
            data.trailCoroutine = null;
        }
        data.trailCoroutine = StartCoroutine(
            AnimateTrailCoroutine(data, from1, to1, from2, to2, duration, easeOut, delay));
    }

    private Vector2 GetWipeHiddenSize(PortraitAnimationData data)
    {
        return data.wipeDirection switch
        {
            WipeDirection.Left  => new Vector2(0f, data.wipeMaskFullSize.y),
            WipeDirection.Right => new Vector2(0f, data.wipeMaskFullSize.y),
            WipeDirection.Up    => new Vector2(data.wipeMaskFullSize.x, 0f),
            WipeDirection.Down  => new Vector2(data.wipeMaskFullSize.x, 0f),
            _                   => Vector2.zero
        };
    }

    [ContextMenu("Play Toggle")]
    public void PlayToggle()
    {
        if (_isShownState) PlayAllOut();
        else PlayAllIn();

        _isShownState = !_isShownState;
    }

    private void PlayAllIn()
    {
        float cumulativeDelay = 0f;
        for (int i = 0; i < portraits.Count; i++)
        {
            var data = portraits[i];
            if (data.uiPortrait == null) continue;

            float delay = cumulativeDelay;
            cumulativeDelay += data.followDelay;

            PlayPortraitIn(data, delay);
        }
    }

    private void PlayAllOut()
    {
        float cumulativeDelay = 0f;
        for (int i = portraits.Count - 1; i >= 0; i--)
        {
            var data = portraits[i];
            if (data.uiPortrait == null) continue;

            float delay = cumulativeDelay;
            cumulativeDelay += data.followDelay;

            PlayPortraitOut(data, delay);
        }
    }

    private void PlayPortraitIn(PortraitAnimationData data, float delay)
    {
        KillTweens(data);

        float duration = GetAnimationDuration(data);

        var startLag1 = new Vector4( data.maximumTrailLag.x,  data.maximumTrailLag.y,  0f, 0f);
        var startLag2 = new Vector4( data.maximumTrailLag2.x, data.maximumTrailLag2.y, 0f, 0f);

        // --- FIX: Coroutine call instead of DOVector ---
        StartTrailAnimation(data,
            startLag1, data.finalTrailOffset,
            startLag2, data.finalTrailOffset,
            duration, easeOut: true, delay);

        switch (data.animationType)
        {
            case AnimationType.Slide:    SlideIn(data, delay);    break;
            case AnimationType.SpinZoom: SpinZoomIn(data, delay); break;
            case AnimationType.Wipe:     WipeIn(data, delay);     break;
            case AnimationType.Bounce:   BounceIn(data, delay);   break;
            case AnimationType.Shake:    Shake(data, delay);      break;
            case AnimationType.Fade:     FadeIn(data, delay);     break;
        }
    }

    private void PlayPortraitOut(PortraitAnimationData data, float delay)
    {
        KillTweens(data);

        float duration = GetAnimationDuration(data);

        var targetLag1 = new Vector4(-data.maximumTrailLag.x,  data.maximumTrailLag.y,  0f, 0f);
        var targetLag2 = new Vector4(-data.maximumTrailLag2.x, data.maximumTrailLag2.y, 0f, 0f);

        // --- FIX: Coroutine call instead of DOVector ---
        StartTrailAnimation(data,
            data.finalTrailOffset, targetLag1,
            data.finalTrailOffset, targetLag2,
            duration, easeOut: false, delay);

        switch (data.animationType)
        {
            case AnimationType.Slide:    SlideOut(data, delay);    break;
            case AnimationType.SpinZoom: SpinZoomOut(data, delay); break;
            case AnimationType.Wipe:     WipeOut(data, delay);     break;
            case AnimationType.Bounce:   BounceOut(data, delay);   break;
            case AnimationType.Shake:    Shake(data, delay);       break;
            case AnimationType.Fade:     FadeOut(data, delay);     break;
        }
    }

    private void SlideIn(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.offScreenPosition;
        data.uiPortrait.DOAnchorPos(data.onScreenPosition, data.slideDuration)
            .SetEase(Ease.OutQuint).SetDelay(delay);
    }

    private void SlideOut(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.uiPortrait.DOAnchorPos(data.offScreenPosition, data.slideDuration)
            .SetEase(Ease.InQuint).SetDelay(delay);
    }

    private void SpinZoomIn(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.uiPortrait.localScale       = data.originalScale * Mathf.Max(0.01f, data.zoomInScale);
        data.uiPortrait.localEulerAngles = new Vector3(0f, 0f, -data.spinDegrees);

        data.uiPortrait.DOScale(data.originalScale, data.spinZoomDuration)
            .SetEase(Ease.OutBack).SetDelay(delay);
        data.uiPortrait.DOLocalRotate(Vector3.zero, data.spinZoomDuration, RotateMode.Fast)
            .SetEase(Ease.OutQuint).SetDelay(delay);
    }

    private void SpinZoomOut(PortraitAnimationData data, float delay)
    {
        Vector3 targetScale = data.originalScale * Mathf.Max(0.01f, data.zoomInScale);
        data.uiPortrait.DOScale(targetScale, data.spinZoomDuration)
            .SetEase(Ease.InBack).SetDelay(delay);
        data.uiPortrait.DOLocalRotate(new Vector3(0f, 0f, data.spinDegrees), data.spinZoomDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuint).SetDelay(delay);
    }

    private void WipeIn(PortraitAnimationData data, float delay)
    {
        if (data.wipeMask == null) return;
        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.wipeMask.sizeDelta = data.wipeMaskHiddenSize;
        data.wipeMask.DOSizeDelta(data.wipeMaskFullSize, data.wipeDuration)
            .SetEase(Ease.OutQuart).SetDelay(delay);
    }

    private void WipeOut(PortraitAnimationData data, float delay)
    {
        if (data.wipeMask == null) return;
        data.wipeMask.sizeDelta = data.wipeMaskFullSize;
        data.wipeMask.DOSizeDelta(data.wipeMaskHiddenSize, data.wipeDuration)
            .SetEase(Ease.InQuart).SetDelay(delay);
    }

    private void BounceIn(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.uiPortrait.localScale       = data.originalScale * Mathf.Max(0.01f, data.bounceStartScale);

        Sequence seq = DOTween.Sequence().SetDelay(delay);
        seq.Append(data.uiPortrait.DOScale(data.originalScale * data.bounceOvershoot, data.bounceDuration * 0.55f).SetEase(Ease.OutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale * 0.90f,               data.bounceDuration * 0.20f).SetEase(Ease.InOutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale * 1.05f,               data.bounceDuration * 0.15f).SetEase(Ease.InOutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale,                        data.bounceDuration * 0.10f).SetEase(Ease.InOutQuad));
    }

    private void BounceOut(PortraitAnimationData data, float delay)
    {
        Sequence seq = DOTween.Sequence().SetDelay(delay);
        seq.Append(data.uiPortrait.DOScale(data.originalScale * 1.10f,                                      data.bounceDuration * 0.20f).SetEase(Ease.OutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale * Mathf.Max(0.01f, data.bounceStartScale),    data.bounceDuration * 0.80f).SetEase(Ease.InQuad));
    }

    private void Shake(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;

        // --- FIX: Cache the reset position to avoid lambda GC allocation ---
        Vector2 resetPos = data.onScreenPosition;
        data.uiPortrait.DOShakeAnchorPos(
            data.shakeDuration,
            data.shakeStrength,
            data.shakeVibrato,
            data.shakeRandomness,
            snapping: false,
            fadeOut: true)
            .SetDelay(delay)
            .OnComplete(() => data.uiPortrait.anchoredPosition = resetPos);
    }

    private void FadeIn(PortraitAnimationData data, float delay)
    {
        if (data.canvasGroup == null) return;
        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.canvasGroup.alpha = 0f;
        data.canvasGroup.DOFade(1f, data.fadeDuration)
            .SetEase(Ease.OutQuad).SetDelay(delay);
    }

    private void FadeOut(PortraitAnimationData data, float delay)
    {
        if (data.canvasGroup == null) return;
        data.canvasGroup.DOFade(0f, data.fadeDuration)
            .SetEase(Ease.InQuad).SetDelay(delay);
    }
}