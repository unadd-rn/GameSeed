using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class tesst : MonoBehaviour
{
    public enum AnimationType
    {
        Slide,
        SpinZoom,
        Wipe,
        Bounce,
        Shake,
        Fade
    }

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
        [HideInInspector] public string uniqueTweenId;
        [HideInInspector] public string uniqueTweenId2; 
        [HideInInspector] public Vector2 wipeMaskFullSize;
        [HideInInspector] public Vector2 wipeMaskHiddenSize;
        [HideInInspector] public Vector3 originalScale = Vector3.one; 
    }

    public enum WipeDirection { Left, Right, Up, Down }

    [Header("Portraits Setup")]
    public List<PortraitAnimationData> portraits = new List<PortraitAnimationData>();

    [Header("Toggle Button (optional)")]
    [Tooltip("Assign a UI Button to auto-wire the toggle. Leave empty to call PlayToggle() manually.")]
    public Button toggleButton;

    private bool _isShownState = false;
    
    // Shader Property IDs
    private static readonly int TrailOffsetID = Shader.PropertyToID("_TrailOffset");
    private static readonly int TrailOffset2ID = Shader.PropertyToID("_TrailOffset2");
    private static readonly int TrailColorID = Shader.PropertyToID("_TrailColor");

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────

    void Awake()
    {
        InitializeMaterials();
        InitializeWipeMasks();
        CacheOriginalScales(); 
    }

    void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(PlayToggle);

        ResetToHiddenState();
    }

    void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(PlayToggle);
    }

    // ─────────────────────────────────────────────
    // Initialization & State Management
    // ─────────────────────────────────────────────

    private void CacheOriginalScales()
    {
        foreach (var data in portraits)
        {
            if (data.uiPortrait != null)
            {
                if (data.uiPortrait.localScale.sqrMagnitude < 0.001f)
                    data.originalScale = Vector3.one;
                else
                    data.originalScale = data.uiPortrait.localScale;
            }
        }
    }

    private void InitializeMaterials()
    {
        for (int i = 0; i < portraits.Count; i++)
            SetupPortraitMaterial(portraits[i], i);
    }

    private void InitializeWipeMasks()
    {
        foreach (var data in portraits)
        {
            if (data.wipeMask == null) continue;
            data.wipeMaskFullSize = data.wipeMask.sizeDelta;
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
                    data.uiPortrait.localScale = data.originalScale * Mathf.Max(0.01f, data.zoomInScale);
                    data.uiPortrait.localEulerAngles = new Vector3(0f, 0f, -data.spinDegrees);
                    break;
                case AnimationType.Wipe:
                    data.uiPortrait.anchoredPosition = data.onScreenPosition;
                    if (data.wipeMask != null) data.wipeMask.sizeDelta = data.wipeMaskHiddenSize;
                    break;
                case AnimationType.Bounce:
                    data.uiPortrait.anchoredPosition = data.onScreenPosition;
                    data.uiPortrait.localScale = data.originalScale * Mathf.Max(0.01f, data.bounceStartScale);
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

    private void SetupPortraitMaterial(PortraitAnimationData data, int index)
    {
        if (data.uiPortrait == null) return;

        var img = data.uiPortrait.GetComponent<Image>();
        if (img != null && img.material != null)
        {
            data.uniqueMaterial = Instantiate(img.material);
            data.uniqueMaterial.SetColor(TrailColorID, data.trailColor); 
            img.material = data.uniqueMaterial;
        }

        data.uniqueTweenId = $"trailTween_{index}_{data.uiPortrait.gameObject.GetInstanceID()}";
        data.uniqueTweenId2 = $"trailTween2_{index}_{data.uiPortrait.gameObject.GetInstanceID()}";
    }

    private void EnsureMaterialReady(PortraitAnimationData data, int index)
    {
        if (data.uniqueMaterial != null) return;

        var img = data.uiPortrait.GetComponent<Image>();
        if (img != null)
        {
            data.uniqueMaterial = Instantiate(img.material);
            data.uniqueMaterial.SetColor(TrailColorID, data.trailColor); 
            img.material = data.uniqueMaterial;
        }

        data.uniqueTweenId = $"trailTween_{index}_{data.uiPortrait.gameObject.GetInstanceID()}";
        data.uniqueTweenId2 = $"trailTween2_{index}_{data.uiPortrait.gameObject.GetInstanceID()}";
    }

    // ─────────────────────────────────────────────
    // Shared Helpers
    // ─────────────────────────────────────────────

    private void KillTweens(PortraitAnimationData data)
    {
        data.uiPortrait.DOKill();
        data.canvasGroup?.DOKill();
        DOTween.Kill(data.uniqueTweenId);
        DOTween.Kill(data.uniqueTweenId2);
        data.wipeMask?.DOKill();
    }

    private void ResetTrailToFixedEnd(PortraitAnimationData data)
    {
        if (data.uniqueMaterial == null) return;
        data.uniqueMaterial.SetColor(TrailColorID, data.trailColor);
        data.uniqueMaterial.SetVector(TrailOffsetID, data.finalTrailOffset);
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

    private void AnimateTrails(PortraitAnimationData data, Vector4 from1, Vector4 to1, Vector4 from2, Vector4 to2,
                               float duration, Ease ease1, Ease ease2, float delay)
    {
        if (data.uniqueMaterial == null) return;
        
        // Dynamically update the color in case you changed it in the inspector
        data.uniqueMaterial.SetColor(TrailColorID, data.trailColor);

        // --- TRAIL 1 ---
        data.uniqueMaterial.SetVector(TrailOffsetID, from1);
        DOTween.To(
                () => data.uniqueMaterial.GetVector(TrailOffsetID),
                v => data.uniqueMaterial.SetVector(TrailOffsetID, v),
                to1, duration)
            .SetEase(ease1)
            .SetDelay(delay)
            .SetId(data.uniqueTweenId);

        // --- TRAIL 2 ---
        data.uniqueMaterial.SetVector(TrailOffset2ID, from2);
        DOTween.To(
                () => data.uniqueMaterial.GetVector(TrailOffset2ID),
                v => data.uniqueMaterial.SetVector(TrailOffset2ID, v),
                to2, duration)
            .SetEase(ease2) 
            .SetDelay(delay)
            .SetId(data.uniqueTweenId2);
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

    // ─────────────────────────────────────────────
    // Single Toggle Entry Point
    // ─────────────────────────────────────────────

    [ContextMenu("Play Toggle")]
    public void PlayToggle()
    {
        if (_isShownState) PlayAllOut();
        else PlayAllIn();

        _isShownState = !_isShownState;
    }

    public void SetState(bool shown)
    {
        _isShownState = shown;
    }

    // ─────────────────────────────────────────────
    // Dispatch
    // ─────────────────────────────────────────────

    private void PlayAllIn()
    {
        float cumulativeDelay = 0f;

        for (int i = 0; i < portraits.Count; i++)
        {
            var data = portraits[i];
            if (data.uiPortrait == null) continue;

            float delay = cumulativeDelay;
            cumulativeDelay += data.followDelay;

            PlayPortraitIn(data, i, delay);
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

            PlayPortraitOut(data, i, delay);
        }
    }

    private void PlayPortraitIn(PortraitAnimationData data, int index, float delay)
    {
        EnsureMaterialReady(data, index);
        KillTweens(data);

        // Global Trail Lag configuration triggered for ALL animation types incoming
        float duration = GetAnimationDuration(data);
        var startLag1 = new Vector4(data.maximumTrailLag.x, data.maximumTrailLag.y, 0f, 0f);
        var startLag2 = new Vector4(data.maximumTrailLag2.x, data.maximumTrailLag2.y, 0f, 0f);
        AnimateTrails(data, startLag1, data.finalTrailOffset, startLag2, data.finalTrailOffset, duration, Ease.OutQuint, Ease.OutQuad, delay);

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

    private void PlayPortraitOut(PortraitAnimationData data, int index, float delay)
    {
        KillTweens(data);

        // Global Trail Lag configuration triggered for ALL animation types outgoing
        float duration = GetAnimationDuration(data);
        var targetLag1 = new Vector4(-data.maximumTrailLag.x, data.maximumTrailLag.y, 0f, 0f);
        var targetLag2 = new Vector4(-data.maximumTrailLag2.x, data.maximumTrailLag2.y, 0f, 0f);
        AnimateTrails(data, data.finalTrailOffset, targetLag1, data.finalTrailOffset, targetLag2, duration, Ease.InQuint, Ease.InQuad, delay);

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

    // ─────────────────────────────────────────────
    // Slide
    // ─────────────────────────────────────────────

    private void SlideIn(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.offScreenPosition;

        data.uiPortrait.DOAnchorPos(data.onScreenPosition, data.slideDuration)
            .SetEase(Ease.OutQuint)
            .SetDelay(delay);
    }

    private void SlideOut(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;

        data.uiPortrait.DOAnchorPos(data.offScreenPosition, data.slideDuration)
            .SetEase(Ease.InQuint)
            .SetDelay(delay);
    }

    // ─────────────────────────────────────────────
    // Spin & Zoom
    // ─────────────────────────────────────────────

    private void SpinZoomIn(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.uiPortrait.localScale = data.originalScale * Mathf.Max(0.01f, data.zoomInScale);
        data.uiPortrait.localEulerAngles = new Vector3(0f, 0f, -data.spinDegrees);

        data.uiPortrait.DOScale(data.originalScale, data.spinZoomDuration)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);

        data.uiPortrait.DOLocalRotate(Vector3.zero, data.spinZoomDuration, RotateMode.Fast)
            .SetEase(Ease.OutQuint)
            .SetDelay(delay);
    }

    private void SpinZoomOut(PortraitAnimationData data, float delay)
    {
        Vector3 targetScale = data.originalScale * Mathf.Max(0.01f, data.zoomInScale); 

        data.uiPortrait.DOScale(targetScale, data.spinZoomDuration)
            .SetEase(Ease.InBack)
            .SetDelay(delay);

        data.uiPortrait.DOLocalRotate(new Vector3(0f, 0f, data.spinDegrees), data.spinZoomDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuint)
            .SetDelay(delay);
    }

    // ─────────────────────────────────────────────
    // Wipe
    // ─────────────────────────────────────────────

    private void WipeIn(PortraitAnimationData data, float delay)
    {
        if (data.wipeMask == null) return;

        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.wipeMask.sizeDelta = data.wipeMaskHiddenSize;

        data.wipeMask.DOSizeDelta(data.wipeMaskFullSize, data.wipeDuration)
            .SetEase(Ease.OutQuart)
            .SetDelay(delay);
    }

    private void WipeOut(PortraitAnimationData data, float delay)
    {
        if (data.wipeMask == null) return;

        data.wipeMask.sizeDelta = data.wipeMaskFullSize;

        data.wipeMask.DOSizeDelta(data.wipeMaskHiddenSize, data.wipeDuration)
            .SetEase(Ease.InQuart)
            .SetDelay(delay);
    }

    // ─────────────────────────────────────────────
    // Bounce
    // ─────────────────────────────────────────────

    private void BounceIn(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;
        data.uiPortrait.localScale = data.originalScale * Mathf.Max(0.01f, data.bounceStartScale);

        Sequence seq = DOTween.Sequence().SetDelay(delay).SetUpdate(UpdateType.Normal, false);
        seq.Append(data.uiPortrait.DOScale(data.originalScale * data.bounceOvershoot, data.bounceDuration * 0.55f).SetEase(Ease.OutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale * 0.90f,               data.bounceDuration * 0.20f).SetEase(Ease.InOutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale * 1.05f,               data.bounceDuration * 0.15f).SetEase(Ease.InOutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale,                        data.bounceDuration * 0.10f).SetEase(Ease.InOutQuad)); 
    }

    private void BounceOut(PortraitAnimationData data, float delay)
    {
        Sequence seq = DOTween.Sequence().SetDelay(delay).SetUpdate(UpdateType.Normal, false);
        seq.Append(data.uiPortrait.DOScale(data.originalScale * 1.10f, data.bounceDuration * 0.20f).SetEase(Ease.OutQuad));
        seq.Append(data.uiPortrait.DOScale(data.originalScale * Mathf.Max(0.01f, data.bounceStartScale), data.bounceDuration * 0.80f).SetEase(Ease.InQuad));
    }

    // ─────────────────────────────────────────────
    // Shake
    // ─────────────────────────────────────────────

    private void Shake(PortraitAnimationData data, float delay)
    {
        data.uiPortrait.anchoredPosition = data.onScreenPosition;

        data.uiPortrait.DOShakeAnchorPos(
                data.shakeDuration,
                data.shakeStrength,
                data.shakeVibrato,
                data.shakeRandomness,
                snapping: false,
                fadeOut: true)
            .SetDelay(delay)
            .OnComplete(() => data.uiPortrait.anchoredPosition = data.onScreenPosition);
    }

    // ─────────────────────────────────────────────
    // Fade
    // ─────────────────────────────────────────────

    private void FadeIn(PortraitAnimationData data, float delay)
    {
        if (data.canvasGroup == null) return;

        data.uiPortrait.anchoredPosition = data.onScreenPosition; 
        data.canvasGroup.alpha = 0f;

        data.canvasGroup.DOFade(1f, data.fadeDuration)
            .SetEase(Ease.OutQuad)
            .SetDelay(delay);
    }

    private void FadeOut(PortraitAnimationData data, float delay)
    {
        if (data.canvasGroup == null) return;

        data.canvasGroup.DOFade(0f, data.fadeDuration)
            .SetEase(Ease.InQuad)
            .SetDelay(delay);
    }

    // ─────────────────────────────────────────────
    // Context Menu shortcuts
    // ─────────────────────────────────────────────

    [ContextMenu("Force Play In")]
    private void DebugForceIn()  { _isShownState = false; PlayToggle(); }

    [ContextMenu("Force Play Out")]
    private void DebugForceOut() { _isShownState = true;  PlayToggle(); }
}