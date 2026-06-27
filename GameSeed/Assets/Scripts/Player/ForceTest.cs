using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ForceTest : MonoBehaviour
{
    [SerializeField] private int forceValue = 10;
    [SerializeField] private Image forceBarTracker;
    // public Button forceController;
    public float barSpeed = 0.6f;
    private float barDirection = 1f;

    public StickThrowTest stickThrowTest;
    // Inside ForceTest.cs
    [SerializeField] private StickData stickData;

    [Header("Tracker Settings")]
    [SerializeField] private RectTransform triangleTracker;
    [SerializeField] private bool isVertical = true;

    [SerializeField] private float minYPosition = 10f; 
    [SerializeField] private float maxYPosition = 190f;

    public void ChangeForce()
    {
        float newValue = forceBarTracker.fillAmount + barDirection * barSpeed * Time.deltaTime;
        if (newValue >= 1f) { newValue = 1f; barDirection = -1f; }
        if (newValue <= 0f) { newValue = 0; barDirection = 1f; }
        forceBarTracker.fillAmount = newValue;
        stickData.velocityScale = newValue;
        if (triangleTracker != null)
        {
            UpdateTrackerPosition(newValue);
        }
    }

    private void UpdateTrackerPosition(float fillAmount)
    {
        Vector2 newPosition = triangleTracker.anchoredPosition;

        if (isVertical)
        {
            newPosition.y = Mathf.Lerp(minYPosition, maxYPosition, fillAmount);
        }

        triangleTracker.anchoredPosition = newPosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
