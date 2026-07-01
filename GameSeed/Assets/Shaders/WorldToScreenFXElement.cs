using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WorldToScreenFXElement : MonoBehaviour
{
    [Header("Component References")]
    public RectTransform rectTransform;
    public Image fxIcon;
    public CanvasGroup canvasGroup;

    [Header("Debug Settings")]
    [Tooltip("Forces the element to lock directly to the middle of the screen for testing.")]
    public bool forceCenterTest = true;

    private Transform target3D;
    private Camera mainCam;
    private FXProfile currentProfile;
    private Sequence currentSequence;
    private System.Action<WorldToScreenFXElement> onCompleteCallback;
    
    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private bool isTracking = false;
    private Vector3 midLifeOffset = Vector3.zero;

    public void Play(Transform target, FXProfile profile, Camera camera, System.Action<WorldToScreenFXElement> onComplete)
    {
        target3D = target;
        currentProfile = profile;
        mainCam = camera;
        onCompleteCallback = onComplete;

        // Ensure we have a valid root canvas reference
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            rootCanvas = Object.FindAnyObjectByType<Canvas>();
            if (rootCanvas != null) transform.SetParent(rootCanvas.transform, false);
        }
        
        if (rootCanvas != null) canvasRect = rootCanvas.GetComponent<RectTransform>();

        if (fxIcon != null)
        {
            fxIcon.sprite = profile.iconSprite; 
            fxIcon.color = profile.elementColor;
        }

        rectTransform.localScale = Vector3.one * profile.startScale;
        canvasGroup.alpha = 1f;

        midLifeOffset = Vector3.zero;
        isTracking = true;
        UpdatePosition(); 

        // Run the master visual layout inspection
        RunXRayDiagnostic();

        // Build Tween Sequence
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();

        currentSequence.Append(rectTransform.DOScale(Vector3.one * profile.targetScale, profile.introDuration).SetEase(profile.introEase));

        AppendMidLifeAnimation(currentSequence, profile);

        Sequence outroSequence = DOTween.Sequence();
        if (profile.floatUpDistance != 0)
        {
            float outroTargetY = midLifeOffset.y + profile.floatUpDistance;
            outroSequence.Join(DOTween.To(() => midLifeOffset.y, y => midLifeOffset.y = y, outroTargetY, profile.outroDuration).SetEase(profile.outroEase));
        }
        if (profile.fadeOut) outroSequence.Join(canvasGroup.DOFade(0f, profile.outroDuration).SetEase(profile.outroEase));
        currentSequence.Append(outroSequence);

        currentSequence.OnComplete(() =>
        {
            isTracking = false;
            onCompleteCallback?.Invoke(this);
        });
    }

    private void Update()
    {
        if (isTracking) UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (rootCanvas == null || canvasRect == null) return;

        if (forceCenterTest)
        {
            // Lock to center and clear out any weird 3D Z-depth clipping values
            rectTransform.anchoredPosition = (Vector2)midLifeOffset;
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
            return;
        }

        if (target3D == null || mainCam == null || currentProfile == null) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(target3D.position + currentProfile.worldOffset);
        if (screenPos.z <= 0)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        Camera uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + (Vector2)midLifeOffset;
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
        }
    }

    /// <summary>
    /// Builds the mid-life portion of the sequence based on profile.midLifeAnimation.
    /// Each animation type interprets midLifeAmount / midLifeDuration / midLifeLoops / midLifeEase differently
    /// (see tooltips on FXProfile). After the animation, any remaining profile.holdDuration is appended
    /// as a plain wait so you can still pad extra hold time on top of a movement if you want.
    /// </summary>
    private void AppendMidLifeAnimation(Sequence seq, FXProfile profile)
    {
        int loops = Mathf.Max(profile.midLifeLoops, 0);

        switch (profile.midLifeAnimation)
        {
            case MidLifeAnimation.None:
                break;

            case MidLifeAnimation.Pulse:
                if (loops > 0)
                {
                    seq.Append(rectTransform.DOScale(Vector3.one * profile.midLifeAmount, profile.midLifeDuration)
                        .SetEase(profile.midLifeEase)
                        .SetLoops(loops, LoopType.Yoyo));
                }
                break;

            case MidLifeAnimation.SlideUp:
                if (loops > 0)
                {
                    seq.Append(DOTween.To(() => midLifeOffset.y, y => midLifeOffset.y = y, profile.midLifeAmount, profile.midLifeDuration)
                        .SetEase(profile.midLifeEase)
                        .SetLoops(loops, LoopType.Yoyo));
                }
                else
                {
                    seq.Append(DOTween.To(() => midLifeOffset.y, y => midLifeOffset.y = y, profile.midLifeAmount, profile.midLifeDuration)
                        .SetEase(profile.midLifeEase));
                }
                break;

            case MidLifeAnimation.Bounce:
                if (loops > 0)
                {
                    seq.Append(DOTween.To(() => midLifeOffset.y, y => midLifeOffset.y = y, profile.midLifeAmount, profile.midLifeDuration)
                        .SetEase(Ease.OutBounce)
                        .SetLoops(loops, LoopType.Yoyo));
                }
                break;

            case MidLifeAnimation.Shake:
                float totalShakeDuration = profile.midLifeDuration * Mathf.Max(loops, 1);
                seq.Append(DOTween.Shake(() => midLifeOffset, x => midLifeOffset = x, totalShakeDuration, profile.midLifeAmount, vibrato: 10, randomness: 90, fadeOut: true));
                break;

            case MidLifeAnimation.Sway:
                if (loops > 0)
                {
                    seq.Append(rectTransform.DORotate(new Vector3(0, 0, profile.midLifeAmount), profile.midLifeDuration)
                        .SetEase(profile.midLifeEase)
                        .SetLoops(loops, LoopType.Yoyo));
                }
                break;
        }

        if (profile.holdDuration > 0f)
        {
            seq.AppendInterval(profile.holdDuration);
        }
    }

    private void RunXRayDiagnostic()
    {
        Debug.LogWarning("============== [FX ELEMENT X-RAY REPORT] ==============");
        
        // 1. Check Dimensions
        float w = rectTransform.rect.width;
        float h = rectTransform.rect.height;
        Debug.Log($"[DIAGNOSTIC] Size Dimensions: {w}x{h} pixels. {(w == 0 || h == 0 ? "❌ ERROR: Size is 0! It will be completely invisible. Set Width/Height to 100 on your Prefab!" : "✅ OK")}");

        // 2. Check Global Hierarchy scale flattening
        Vector3 globalScale = transform.lossyScale;
        Debug.Log($"[DIAGNOSTIC] Global Scale: {globalScale}. {(globalScale.x == 0 || globalScale.y == 0 ? "❌ ERROR: A parent object in your hierarchy has a scale of 0, flattening this object out of existence!" : "✅ OK")}");

        // 3. Check Graphic Component
        if (fxIcon == null)
        {
            Debug.Log("[DIAGNOSTIC] Image Component: ❌ ERROR: The 'fxIcon' variable assignment slot is empty in the inspector!");
        }
        else
        {
            Debug.Log($"[DIAGNOSTIC] Image Component: Enabled={fxIcon.enabled}, Color={fxIcon.color}, Sprite={(fxIcon.sprite != null ? fxIcon.sprite.name : "None (Solid Color Mode)")}");
        }

        // 4. Check Parent Canvas System
        if (rootCanvas != null)
        {
            Debug.Log($"[DIAGNOSTIC] Render Canvas: Name='{rootCanvas.name}', Enabled={rootCanvas.enabled}, RenderMode={rootCanvas.renderMode}, SortingOrder={rootCanvas.sortingOrder}");
        }
        else
        {
            Debug.Log("[DIAGNOSTIC] Render Canvas: ❌ CRITICAL ERROR: This element is completely missing a Canvas parent component!");
        }

        Debug.LogWarning("=======================================================");
    }

    private void OnDestroy()
    {
        currentSequence?.Kill();
    }
}