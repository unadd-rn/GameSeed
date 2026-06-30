using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickSlot : MonoBehaviour
{
    public float stickLength = 2f;

    [Header("Gadget-Related")]
    public float sizeX;
    public float sizeZ;
    public SlotDefinition[] frontSlots = new SlotDefinition[5]; // ada 5 di sepanjang stik
    public SlotDefinition[] backSlots = new SlotDefinition[5];

    // private void OnDrawGizmos()
    // {
    //     if (frontSlots == null) return;

    //     foreach (var slot in frontSlots)
    //     {
    //         Vector3 worldPos = transform.TransformPoint(slot.localPosition);
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawWireSphere(worldPos, 0.05f);
    //     }
    // }
}