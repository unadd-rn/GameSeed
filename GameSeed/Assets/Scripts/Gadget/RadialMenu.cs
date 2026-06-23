using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialMenu : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform[] items;
    public float radius = 150f;
    public float activeAngleDeg = -45f;
    public float animDuration = 0.25f;

    private int activeIndex = 0;
    private float currentRotationOffset = 0f;
    private float targetRotationOffset = 0f;
    private Coroutine animCoroutine;

    void Start()
    {
        PositionItems(0f);
    }

    public void RotateRight()
    {
        activeIndex = (activeIndex + 1) % items.Length;
        targetRotationOffset += (360f / items.Length);
        StartSpin();
    }

    public void RotateLeft()
    {
        activeIndex = (activeIndex - 1 + items.Length) % items.Length;
        targetRotationOffset -= (360f / items.Length);
        StartSpin();
    }

    public void SelectItem(int index)
    {
        int delta = index - activeIndex;
        if (delta > items.Length / 2)  delta -= items.Length;
        if (delta < -items.Length / 2) delta += items.Length;

        activeIndex = index;
        targetRotationOffset += delta * (360f / items.Length);
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
    }

    void PositionItems(float rotationOffset)
    {
        int n = items.Length;
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

            bool isActive = (i == 0);
            float targetScale = isActive ? 1.2f : 1f;
            items[i].localScale = Vector3.one * targetScale;
        }
    }

    public int GetActiveIndex() => activeIndex;
}