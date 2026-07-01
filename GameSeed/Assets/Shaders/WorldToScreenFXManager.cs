using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public enum MidLifeAnimation
{
    None,
    Pulse,      // scales up and down (yoyo)
    SlideUp,    // floats up and back down (yoyo)
    Bounce,     // like SlideUp but with a bouncy ease
    Shake,      // rapid jittery position shake
    Sway        // gentle side-to-side rotation (yoyo)
}

[System.Serializable]
public class FXProfile
{
    public string fxName = "Heal";
    public Sprite iconSprite;
    public Color elementColor = Color.white;
    public Vector3 worldOffset = new Vector3(0, 2f, 0);

    [Header("Keyframe 1: Intro Pop")]
    public float startScale = 0f;
    public float targetScale = 1f;
    public float introDuration = 0.25f;
    public Ease introEase = Ease.OutBack;

    [Header("Keyframe 2: Mid-Life Behaviour")]
    [Tooltip("How long to wait/hold after the intro before the outro plays. If a Mid-Life Animation is chosen below, this is extra time added AFTER that animation finishes (set to 0 if you want the animation to fill the whole mid-life).")]
    public float holdDuration = 0.2f;

    [Tooltip("What the icon does while it's held in place. 'None' just waits for Hold Duration with no movement.")]
    public MidLifeAnimation midLifeAnimation = MidLifeAnimation.None;

    [Tooltip("Magnitude of the effect. Meaning depends on the animation chosen:\n" +
             "Pulse = target scale to pulse to\n" +
             "SlideUp / Bounce = distance in pixels to float up\n" +
             "Shake = jitter strength in pixels\n" +
             "Sway = rotation angle in degrees")]
    public float midLifeAmount = 1.25f;

    [Tooltip("Duration of a single cycle of the animation (ignored by Shake, which uses this * Loops as its total duration).")]
    public float midLifeDuration = 0.15f;

    [Tooltip("Number of yoyo loops for Pulse / SlideUp / Bounce / Sway. For Shake this instead multiplies with Mid-Life Duration to get total shake time.")]
    public int midLifeLoops = 2;

    [Tooltip("Ease used for Pulse / SlideUp / Bounce / Sway. Ignored by Shake.")]
    public Ease midLifeEase = Ease.InOutSine;

    [Header("Keyframe 3: Outro Exit")]
    public float floatUpDistance = 60f;
    public bool fadeOut = true;
    public float outroDuration = 0.35f;
    public Ease outroEase = Ease.InQuad;

    [Header("Screen Vignette (optional)")]
    [Tooltip("If true, a screen vignette will pulse on while this FX plays, then fade back off.")]
    public bool doVignette = false;
    public float vignetteIntensity = 0.4f;
    public float vignetteFadeInDuration = 0.15f;
    public float vignetteFadeOutDuration = 0.35f;
}

public class WorldToScreenFXManager : MonoBehaviour
{
    public static WorldToScreenFXManager Instance { get; private set; }

    [Header("Pool Configurations")]
    public WorldToScreenFXElement fxPrefab;
    public int initialPoolSize = 15;
    public Transform poolParent;

    [Header("Registered Effects")]
    public List<FXProfile> fxProfiles = new List<FXProfile>();

    [Header("Vignette (URP Global Volume)")]
    [Tooltip("Drag the Global Volume GameObject that holds your Vignette override here.")]
    public Volume fxVolume;

    private Queue<WorldToScreenFXElement> pool = new Queue<WorldToScreenFXElement>();
    private Camera mainCam;
    private Vignette vignette;
    private Tween vignetteTween;

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;

        // DIAGNOSTIC 0: Check Vignette Volume
        if (fxVolume == null)
        {
            Debug.LogWarning("[FX Manager] WARNING: 'Fx Volume' is not assigned. Vignette effects will be skipped.");
        }
        else if (fxVolume.profile == null || !fxVolume.profile.TryGet(out vignette))
        {
            Debug.LogWarning("[FX Manager] WARNING: Assigned Volume has no Vignette override on its profile. Vignette effects will be skipped.");
        }
        else
        {
            // Make sure it starts fully off, regardless of what was left in the editor.
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0f;
        }
        
        // DIAGNOSTIC 1: Check Camera
        if (mainCam == null)
        {
            Debug.LogError("[FX Manager] CRITICAL: Camera.main is NULL! Make sure your main scene camera is tagged 'MainCamera' in the Inspector.");
        }
        else
        {
            Debug.Log($"[FX Manager] Camera successfully found: {mainCam.name}");
        }

        // DIAGNOSTIC 2: Check Canvas Parent
        if (GetComponentInParent<Canvas>() == null)
        {
            Debug.LogWarning("[FX Manager] WARNING: This script or its parents do not have a Canvas component! UI elements will be invisible.");
        }

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPoolInstance();
        }
        Debug.Log($"[FX Manager] Pre-warmed pool with {pool.Count} instances.");
    }

    private WorldToScreenFXElement CreateNewPoolInstance()
    {
        var instance = Instantiate(fxPrefab, poolParent != null ? poolParent : transform);
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
        return instance;
    }

    public void PlayFX(string fxName, Transform target)
    {
        Debug.Log($"[FX Manager] PlayFX requested for event name: '{fxName}' on target: {target.name}");

        if (target == null) return;

        FXProfile profile = fxProfiles.Find(p => p.fxName.Equals(fxName, System.StringComparison.OrdinalIgnoreCase));
        if (profile == null)
        {
            Debug.LogError($"[FX Manager] ERROR: Profile '{fxName}' not found in the Registered Effects list!");
            return;
        }

        if (pool.Count == 0)
        {
            CreateNewPoolInstance();
        }

        WorldToScreenFXElement element = pool.Dequeue();
        element.gameObject.SetActive(true);

        TriggerVignetteIn(profile);

        Debug.Log($"[FX Manager] Successfully spawning element from pool for '{fxName}'.");
        element.Play(target, profile, mainCam, (finishedElement) =>
        {
            TriggerVignetteOut(profile);
            ReturnToPool(finishedElement);
        });
    }

    private void ReturnToPool(WorldToScreenFXElement element)
    {
        element.gameObject.SetActive(false);
        pool.Enqueue(element);
    }

    private void TriggerVignetteIn(FXProfile profile)
    {
        if (!profile.doVignette || vignette == null) return;

        vignetteTween?.Kill();
        vignetteTween = DOTween.To(
            () => vignette.intensity.value,
            x => vignette.intensity.value = x,
            profile.vignetteIntensity,
            profile.vignetteFadeInDuration
        );
    }

    private void TriggerVignetteOut(FXProfile profile)
    {
        if (!profile.doVignette || vignette == null) return;

        vignetteTween?.Kill();
        vignetteTween = DOTween.To(
            () => vignette.intensity.value,
            x => vignette.intensity.value = x,
            0f,
            profile.vignetteFadeOutDuration
        );
    }
}