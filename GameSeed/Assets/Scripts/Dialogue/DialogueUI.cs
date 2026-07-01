using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string knotName = "";
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
        if (!dialogueManager.IsDialoguePlaying)
        {
            dialogueManager.EnterDialogue(knotName);
        }
        else
        {
            dialogueManager.ContinueStory();
        }
    }
}
