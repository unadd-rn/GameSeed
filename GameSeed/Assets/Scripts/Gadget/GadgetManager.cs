using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// basically buat apply si gadgetnya, which means hrs ditaruh di lobby bagian modify?
public class GadgetManager : MonoBehaviour
{
    [Header("Arrays")]
    public BaseGadget[] gadgetOwned;
    public BaseGadget[] gadgetEquipped;

    [Header("Data")]
    public StickData data;

    public void AttachGadget(GadgetInstance gadget, int slotIndex)
    {
        SlotDefinition slot = data.slots[slotIndex];
        GameObject go = Instantiate(gadget.data.prefab, transform);
        go.transform.localPosition = slot.localPosition;
        go.transform.localScale = new Vector3(
            gadget.data.sizeX,
            go.transform.localScale.y,
            gadget.data.sizeZ
        );

        gadget.data.Apply(go);
        slot.occupant = gadget;
        gadget.isEquipped = true;
    }
}