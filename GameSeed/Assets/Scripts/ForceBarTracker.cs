using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ForceBarTracker : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private Image bar;
    [SerializeField] private int forceCurrent = 50;
    [SerializeField] private int forceMax = 100;
    [Space]
    [SerializeField] private bool overkillPossible; // idk what does this do let's just move

    [Header("Tracker Settings")]
    [SerializeField] private RectTransform triangleTracker;
    [SerializeField] private bool isVertical = true;

    // Start is called before the first frame update
    private void Start()
    {
        UpdateBarAndForceText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateBarAndForceText()
    {
        if(forceMax == 0){
            bar.fillAmount = 0; return;
        }

        float fillAmount = (float) forceCurrent / forceMax;

        if (triangleTracker != null && bar != null)
        {
            UpdateTrackerPosition(fillAmount);
        }
    }

    private void UpdateTrackerPosition(float fillAmount)
    {
        // Get the local dimensions of the background bar
        RectTransform barRect = bar.rectTransform;
        Vector2 newPosition = triangleTracker.anchoredPosition;

        if (isVertical)
        {
            // Calculate height-based position. Assumes anchor pivot is at the bottom (y=0)
            float barHeight = barRect.rect.height;
            newPosition.y = barHeight * fillAmount;
        }
        else
        {
            // Calculate width-based position. Assumes anchor pivot is at the left (x=0)
            float barWidth = barRect.rect.width;
            newPosition.x = barWidth * fillAmount;
        }

        triangleTracker.anchoredPosition = newPosition;
    }

    public bool ChangeForce(int amount)
    {
        if(!overkillPossible && forceCurrent + amount < 0) return false;
        
        forceCurrent += amount;
        forceCurrent = Mathf.Clamp(value:forceCurrent, min:0, forceMax);

        return true;
    }
}
