using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RadialMenu : MonoBehaviour
{
    [Header("Settings")]
    public List<RectTransform> items = new List<RectTransform>();
    public float radius = 150f;
    public float activeAngleDeg = -45f;
    public float animDuration = 0.25f;

    private int activeIndex = 0;
    private float currentRotationOffset = 0f;
    private float targetRotationOffset = 0f;
    private Coroutine animCoroutine;

    void Start()
    {
    }

    void Awake()
    {
        items = new List<RectTransform>(); 
    }

    public void ClearMenu()
    {
        if (items == null) 
        {
            items = new List<RectTransform>();
        }

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        
        foreach (var item in items)
        {
            if (item != null) Destroy(item.gameObject);
        }
        
        items.Clear();
        activeIndex = 0;
        currentRotationOffset = 0f;
        targetRotationOffset = 0f;
    }

    public void RotateRight()
    {
        if (items.Count == 0) return;
        activeIndex = (activeIndex + 1) % items.Count;
        targetRotationOffset -= (360f / items.Count);
        StartSpin();
    }

    public void RotateLeft()
    {
        if (items.Count == 0) return;
        activeIndex = (activeIndex - 1 + items.Count) % items.Count;
        targetRotationOffset += (360f / items.Count);
        StartSpin();
    }

    public void SelectItem(int index)
    {
        if (items.Count == 0) return;
        int delta = index - activeIndex;
        if (delta > items.Count / 2)  delta -= items.Count;
        if (delta < -items.Count / 2) delta += items.Count;

        activeIndex = index;
        targetRotationOffset += delta * (360f / items.Count);
        StartSpin();
    }

    void StartSpin()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateSpin());
    }

    IEnumerator AnimateSpin()
    {
        float startOffset = currentRotationOffset;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            // Ease out
            t = 1f - Mathf.Pow(1f - t, 3f);

            currentRotationOffset = Mathf.Lerp(startOffset, targetRotationOffset, t);
            PositionItems(currentRotationOffset);
            yield return null;
        }

        currentRotationOffset = targetRotationOffset;
        PositionItems(currentRotationOffset);

        RadialGadgetController controller = FindObjectOfType<RadialGadgetController>();
        if (controller != null)
        {
            controller.UpdateUIHighlight();
        }
    }

    void PositionItems(float rotationOffset)
    {
        if (items == null || items.Count == 0) return;
        int n = items.Count;
        if (n == 0) return;
        float stepAngle = 360f / n;

        for (int i = 0; i < n; i++)
        {
            float angleDeg = activeAngleDeg + (i * stepAngle) + rotationOffset;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius
            );
            items[i].anchoredPosition = pos;

            bool isActive = (i == activeIndex);
            float targetScale = isActive ? 1.2f : 1f;
            items[i].localScale = Vector3.one * targetScale;
        }
    }

    public int GetActiveIndex() => activeIndex;
}