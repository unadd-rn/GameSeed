using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickSlot : MonoBehaviour
{
    [Header("Needed Element")]
    public float stickLength = 2f;

    [Header("Gadget-Related")]
    public float sizeX;
    public float sizeZ;
    public SlotDefinition[] frontSlots = new SlotDefinition[5]; // ada 5 di sepanjang stik
    public SlotDefinition[] backSlots = new SlotDefinition[5];
}
