using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PortraitAnimator portraitAnimator;
    [SerializeField] private Button targetButton;

    [Header("Multiple Events Settings")]
    [Tooltip("Add all the event names you want to trigger at the same time here.")]
    [SerializeField] private List<string> eventNames = new List<string> { "putStick" };

    private bool isUIVisible = false;

    void Start()
    {
        if (targetButton == null) targetButton = GetComponent<Button>();
        if (targetButton != null) targetButton.onClick.AddListener(OnButtonClicked);
    }

    void OnDestroy()
    {
        if (targetButton != null) targetButton.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (portraitAnimator == null) return;

        // Loop through every single event name in the list
        for (int i = 0; i < eventNames.Count; i++)
        {
            string currentEvent = eventNames[i];
            
            if (isUIVisible)
            {
                portraitAnimator.PlayEventOut(currentEvent);
            }
            else
            {
                portraitAnimator.PlayEventIn(currentEvent);
            }
        }

        // Toggle the global visibility tracker state
        isUIVisible = !isUIVisible;
    }
}