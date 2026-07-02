using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (!dialogueManager.IsDialoguePlaying) return;

        Debug.Log($"Tap detected | IsWaitingForAction: {dialogueManager.IsWaitingForAction}");

        if (dialogueManager.IsWaitingForAction) return;

        dialogueManager.ContinueStory();
    }
}