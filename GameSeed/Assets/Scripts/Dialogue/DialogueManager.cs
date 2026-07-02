using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using TMPro;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;

public class DialogueManager : MonoBehaviour
{
    [Header("Story")]
    [SerializeField] private TextAsset inkJson;

    [Header("DialoguePanel")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("ActorPanel")]
    [SerializeField] private GameObject actorPanel;
    [SerializeField] private TextMeshProUGUI actorText;
    
    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;

    [Header("TypewriterSpeed")]
    [SerializeField] private float typingSpeed = 0.005f;
    [SerializeField] private int charsPerTick = 3;

    private const string HIGHLIGHT_TAG = "highlight";
    private const string HIGHLIGHT_NONE = "none";
    private const string ACTOR_TAG = "actor";
    private const string NARRATOR_TAG = "narrator";
    private const string WAIT_TAG = "wait";
    private Story story;
    private bool dialoguePlaying = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    public bool IsDialoguePlaying => dialoguePlaying;
    public bool IsWaitingForAction => waitingForAction;
    private bool waitingForAction = false;
    private bool ignoreTapThisFrame = false;
    private bool tapLocked = false;
    public event System.Action OnDialogueEnd;
    public event System.Action<string> OnHighlightTag;
    public bool IsTapLocked => tapLocked;

    private void Awake()
    {
        Debug.Log("DialogueManager Awake running");
        story = new Story(inkJson.text);
        Debug.Log("Story created successfully");
    }

    public void SetWaitForAction(bool waiting)
    {
        waitingForAction = waiting;
    }

    public void IgnoreTapThisFrame()
    {
        ignoreTapThisFrame = true;
    }

    public void LockTapForDuration(float duration)
    {
        StartCoroutine(TapLockCoroutine(duration));
    }

    private IEnumerator TapLockCoroutine(float duration)
    {
        tapLocked = true;
        yield return new WaitForSeconds(duration);
        tapLocked = false;
    }

    private void Update()
    {
        if (ignoreTapThisFrame)
        {
            ignoreTapThisFrame = false;
            return;
        }
        if (!dialoguePlaying)
        {
            return;
        }
        if (waitingForAction)
        {
            return;
        }
        if (tapLocked)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            ContinueStory();
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        return EventSystem.current.IsPointerOverGameObject();

    }

    public void EnterDialogue(string knotName)
    {
        if (dialoguePlaying)
        {
            Debug.LogWarning($"EnterDialogue('{knotName}') called while dialogue is already playing. Ignoring.");
            return;
        }       
        
        Debug.Log($"story null? {story == null}, panel null? {dialoguePanel == null}, text null? {dialogueText == null}");

        dialoguePlaying = true;
        dialoguePanel.SetActive(true);

        if (!string.IsNullOrEmpty(knotName))
        {
            story.ChoosePathString(knotName);
        }
        else
        {
            Debug.LogWarning("Knot not found");
        }

        ContinueStory();
    }

    public void EnterDialogueWithDelay(string knotName, float delay)
    {
        StartCoroutine(DelayedEnterDialogue(knotName, delay));
    }

    private IEnumerator DelayedEnterDialogue(string knotName, float delay)
    {
        yield return new WaitForSeconds(delay);
        EnterDialogue(knotName);
    }

    public void ContinueStory()
    {
        if (isTyping)
        {
            CompleteLine();
            return;
        }
        if (story.canContinue)
        {
            string nextLine = story.Continue();

            HandleTags(story.currentTags);
            
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeLine(nextLine));

            LockTapForDuration(1f);
        }
        else
        {
            ExitStory();
        }
    }

    public void ContinueStoryWithDelay(float delay)
    {
        StartCoroutine(DelayedContinueStory(delay));
    }
    
    private IEnumerator DelayedContinueStory(float delay)
    {
        yield return new WaitForSeconds(delay);
        ContinueStory();
    }

    private void HandleTags(List<string> tags)
    {
        // Reset per-line state.
        bool actorTagSeen = false;

        if (tags == null || tags.Count == 0)
        {
            if (actorPanel != null) actorPanel.SetActive(false);
            return;
        }

        foreach (string tag in tags)
        {
            string trimmedTag = tag.Trim().ToLower();

            if (trimmedTag == WAIT_TAG)
            {
                waitingForAction = true;
                Debug.Log("waitingForAction is doin the thing");
                continue;
            }

            string[] splitTag = tag.Split('=');

            if (splitTag.Length != 2)
            {
                continue;
            }

            string tagKey = splitTag[0].Trim().ToLower();
            string tagValue = splitTag[1].Trim();

            switch (tagKey)
            {
                case "actor":
                    actorTagSeen = true;
                    if (actorPanel != null)
                        {
                            if (tagValue.ToLower() == NARRATOR_TAG)
                            {
                                actorPanel.SetActive(false);
                            }
                            else
                            {
                                actorPanel.SetActive(true);
                                actorText.text = tagValue;  
                            }
                        }
                    break;    

                case HIGHLIGHT_TAG:
                    if (tagValue.ToLower() == HIGHLIGHT_NONE)
                    {
                        tutorialManager.ShowFullDim();
                    }
                    else
                    {
                        tutorialManager.ShowTutorial(tagValue);
                        OnHighlightTag?.Invoke(tagValue);
                    }
                    break;
            }
        }

        // If this line carried tags but none of them was an #actor = tag,
        // default the actor panel to hidden instead of leaving whatever
        if (!actorTagSeen && actorPanel != null)
        {
            actorPanel.SetActive(false);
        }
    }
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        while (visibleCount < totalVisibleCharacters)
        {
            visibleCount = Mathf.Min(visibleCount + charsPerTick, totalVisibleCharacters);
            dialogueText.maxVisibleCharacters = visibleCount;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        isTyping = false;
    }

    private void ExitStory()
    {
        Debug.Log("Exiting dialogue");
        dialoguePlaying = false;
        dialoguePanel.SetActive(false);
        if (actorPanel != null) actorPanel.SetActive(false);
        dialogueText.text = "";
        dialogueText.maxVisibleCharacters = 0;
        story.ResetState();

        OnDialogueEnd?.Invoke();
    }
}