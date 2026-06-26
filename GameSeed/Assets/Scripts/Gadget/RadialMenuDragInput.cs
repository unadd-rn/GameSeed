using UnityEngine;
using UnityEngine.EventSystems;

public class RadialMenuDragInput : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RadialMenu radialMenu;
    public float dragThreshold = 50f;

    private float dragAccumulator = 0f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragAccumulator = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragAccumulator += eventData.delta.x;
        Debug.Log($"Lagi di-drag! Nilai akumulator saat ini: {dragAccumulator}");

        if (dragAccumulator >= dragThreshold)
        {
            radialMenu.RotateRight();
            dragAccumulator -= dragThreshold;
        }
        else if (dragAccumulator <= -dragThreshold)
        {
            radialMenu.RotateLeft();
            dragAccumulator += dragThreshold;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragAccumulator = 0f;
    }
}