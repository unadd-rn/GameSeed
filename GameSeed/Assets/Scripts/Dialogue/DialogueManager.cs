using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.SearchService;
using System;

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

    [Header("TypewriterSpeed")]
    [SerializeField] private float typingSpeed = 0.03f;

    private const string NARRATOR_TAG = "narrator";
    private Story story;
    private bool dialoguePlaying = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    public bool IsDialoguePlaying => dialoguePlaying;

    private void Awake()
    {
        Debug.Log("DialogueManager Awake running");
        story = new Story(inkJson.text);
        Debug.Log("Story created successfully");
    }

    private void Update()
    {
        if (!dialoguePlaying)
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
        }
        else
        {
            ExitStory();
        }
    }

    private void HandleTags(List<string> tags)
    {
        if (tags == null || tags.Count == 0)
        {
            actorPanel.SetActive(false);
            return;
        }

        foreach (string tag in tags)
        {
            string[] splitTag = tag.Split('=');

            if (splitTag.Length != 2)
            {
                continue;
            }

            string tagKey = splitTag[0].Trim().ToLower();
            string tagValue = splitTag[1].Trim();

            if (tagKey == "actor")
            {
                if (tagValue.ToLower() == NARRATOR_TAG)
                {
                    actorPanel.SetActive(false);
                    actorText.text = "";
                }
                else
                {
                    actorPanel.SetActive(true);
                    actorText.text = tagValue;
                }
            }
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
            visibleCount++;
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
        actorPanel.SetActive(false);
        dialogueText.text = "";
        dialogueText.maxVisibleCharacters = 0;
        story.ResetState();
    }
}
