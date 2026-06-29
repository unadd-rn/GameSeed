using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public enum AnimTypeIn
{
    SlideUp,
    FadeIn,
    ZoomIn
}

public enum AnimTypeOut
{
    SlideDown,
    FadeOut,
    ZoomOut
}

[System.Serializable]
public class TransitionProfile
{
    [Tooltip("Name used to call this transition (e.g., 'Circle', 'Logo')")]
    public string transitionName = "Default";
    public RectTransform transitionRect;
    public CanvasGroup canvasGroup;

    [Header("Animation Styles")]
    public AnimTypeIn animIn = AnimTypeIn.SlideUp;
    public AnimTypeOut animOut = AnimTypeOut.FadeOut;

    [Header("Timing Overrides")]
    public float durationIn = 0.4f;
    public float holdDuration = 0.15f;
    public float durationOut = 0.4f;
}

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Transitions Setup")]
    [SerializeField] private List<TransitionProfile> transitions;

    [Header("Global Curves & Easing")]
    [SerializeField] Ease easeIn = Ease.OutCubic;
    [SerializeField] Ease easeOut = Ease.OutQuad;

    private bool _isTransitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-fetch CanvasGroups if they exist but weren't assigned
        foreach (var t in transitions)
        {
            if (t.transitionRect != null && t.canvasGroup == null)
            {
                t.canvasGroup = t.transitionRect.GetComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        ResetAllTransitionVisuals();
    }

    private void ResetAllTransitionVisuals()
    {
        foreach (var t in transitions)
        {
            if (t.transitionRect != null)
            {
                t.transitionRect.gameObject.SetActive(false);
                // Reset to neutral states
                t.transitionRect.anchoredPosition = new Vector2(0f, -3000f);
                t.transitionRect.localScale = Vector3.one;
                if (t.canvasGroup != null) t.canvasGroup.alpha = 1f;
            }
        }
    }

    public void GoToScene(string sceneName, string transitionName = "Default")
    {
        if (_isTransitioning) return;
        StartCoroutine(RunTransition(sceneName, transitionName));
    }

    IEnumerator RunTransition(string sceneName, string transitionName)
    {
        _isTransitioning = true;

        TransitionProfile activeProfile = transitions.Find(t => t.transitionName == transitionName);
        if (activeProfile == null && transitions.Count > 0) activeProfile = transitions[0];

        if (activeProfile == null || activeProfile.transitionRect == null)
        {
            SceneManager.LoadScene(sceneName);
            _isTransitioning = false;
            yield break;
        }

        if (MenuBackground.Instance != null) MenuBackground.Instance.StartAnim();
        yield return new WaitForSecondsRealtime(1.3f);

        // 1. SETUP STARTING STATE BASED ON 'ANIM IN' SELECTION
        activeProfile.transitionRect.DOKill();
        if (activeProfile.canvasGroup != null) activeProfile.canvasGroup.DOKill();

        // Neutralize first
        activeProfile.transitionRect.anchoredPosition = Vector2.zero;
        activeProfile.transitionRect.localScale = Vector3.one;
        if (activeProfile.canvasGroup != null) activeProfile.canvasGroup.alpha = 1f;

        switch (activeProfile.animIn)
        {
            case AnimTypeIn.SlideUp:
                activeProfile.transitionRect.anchoredPosition = new Vector2(0f, -3000f);
                break;
            case AnimTypeIn.FadeIn:
                if (activeProfile.canvasGroup != null) activeProfile.canvasGroup.alpha = 0f;
                break;
            case AnimTypeIn.ZoomIn:
                activeProfile.transitionRect.localScale = Vector3.zero;
                break;
        }

        activeProfile.transitionRect.gameObject.SetActive(true);

        // 2. PLAY 'ANIM IN'
        Tween tweenIn = null;
        switch (activeProfile.animIn)
        {
            case AnimTypeIn.SlideUp:
                tweenIn = activeProfile.transitionRect.DOAnchorPosY(0f, activeProfile.durationIn);
                break;
            case AnimTypeIn.FadeIn:
                if (activeProfile.canvasGroup != null) tweenIn = activeProfile.canvasGroup.DOFade(1f, activeProfile.durationIn);
                break;
            case AnimTypeIn.ZoomIn:
                tweenIn = activeProfile.transitionRect.DOScale(Vector3.one, activeProfile.durationIn);
                break;
        }

        if (tweenIn != null)
        {
            bool inDone = false;
            tweenIn.SetEase(easeIn).SetUpdate(UpdateType.Normal, true).OnComplete(() => inDone = true);
            while (!inDone) yield return null;
        }
        else
        {
            yield return new WaitForSecondsRealtime(activeProfile.durationIn);
        }

        yield return new WaitForSecondsRealtime(activeProfile.holdDuration);
        if (MenuBackground.Instance != null) MenuBackground.Instance.StopAnim();

        // 3. ASYNC SCENE LOAD
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        ao.allowSceneActivation = false;
        while (ao.progress < 0.9f) yield return null;
        
        ao.allowSceneActivation = true;
        while (!ao.isDone) yield return null;

        // 4. PLAY 'ANIM OUT' BASED ON SELECTION
        Tween tweenOut = null;
        switch (activeProfile.animOut)
        {
            case AnimTypeOut.SlideDown:
                tweenOut = activeProfile.transitionRect.DOAnchorPosY(-3000f, activeProfile.durationOut);
                break;
            case AnimTypeOut.FadeOut:
                if (activeProfile.canvasGroup != null) tweenOut = activeProfile.canvasGroup.DOFade(0f, activeProfile.durationOut);
                break;
            case AnimTypeOut.ZoomOut:
                tweenOut = activeProfile.transitionRect.DOScale(Vector3.zero, activeProfile.durationOut);
                break;
        }

        if (tweenOut != null)
        {
            bool outDone = false;
            tweenOut.SetEase(easeOut).SetUpdate(UpdateType.Normal, true).OnComplete(() => outDone = true);
            while (!outDone) yield return null;
        }
        else
        {
            yield return new WaitForSecondsRealtime(activeProfile.durationOut);
        }

        // 5. CLEAN UP
        ResetAllTransitionVisuals();
        _isTransitioning = false;
    }
}