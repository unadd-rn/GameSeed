using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// basically buat apply si gadgetnya, which means hrs ditaruh di lobby bagian modify?
public class GadgetManager : MonoBehaviour
{
    // ide: sebelum dia main, kalau gadgetOwned udah 10, ingetin dulu dia gabisa ngeclaim stik musuh, jadi gak ribet nampilin inventory lagi
    [Header("Arrays")]
    public GadgetInstance[] gadgetOwned; // maksimal 10
    // public BaseGadget[] gadgetEquipped;
    // kayaknya gadgetEquipped gausah ada soalnya nnti pusing lagi gimana ngapusnya

    [Header("Data")]
    public StickData data;
    GameObject player;

    [Header("Spawned Reference")]
    public Transform stickBodyTransform;

    public void AttachGadget(GadgetInstance gadget, int slotIndex)
    {
        if(slotIndex < 0 || slotIndex >= data.frontSlots.Length) return;
        Transform parentTransform = stickBodyTransform != null ? stickBodyTransform : transform;

        SlotDefinition frontSlot = data.frontSlots[slotIndex];
        GameObject frontVisual = Instantiate(gadget.data.prefab, parentTransform);

        frontVisual.transform.localPosition = frontSlot.localPosition;
        frontVisual.transform.localRotation = Quaternion.identity; // bikin dia lagi ngadep ke depan
        SetGadgetScale(frontVisual, gadget.data);

        SlotDefinition backSlot = data.backSlots[slotIndex];
        GameObject backVisual = Instantiate(gadget.data.prefab, parentTransform);

        backVisual.transform.localPosition = backSlot.localPosition;
        backVisual.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        SetGadgetScale(backVisual, gadget.data);

        gadget.data.Apply(player);

        frontSlot.spawnedVisual = frontVisual;
        backSlot.spawnedVisual = backVisual;

        frontSlot.occupant = gadget;
        backSlot.occupant = gadget;

        gadget.isEquipped = true;
    }

    private void SetGadgetScale(GameObject go, BaseGadget gadgetData)
    {
        go.transform.localScale = new Vector3(
            gadgetData.sizeX,
            go.transform.localScale.y,
            gadgetData.sizeZ
        );
    }

    public void DetachGadget(GadgetInstance gadget, int slotIndex)
    {
        SlotDefinition frontSlot = data.frontSlots[slotIndex];
        SlotDefinition backSlot = data.backSlots[slotIndex];

        GadgetInstance detachedGadget = frontSlot.occupant;

        if(frontSlot.spawnedVisual != null) Destroy(frontSlot.spawnedVisual);
        if(backSlot.spawnedVisual != null) Destroy(backSlot.spawnedVisual);

        detachedGadget.data.Remove(player);
        detachedGadget.isEquipped = false;
        
        frontSlot.occupant = null;
        backSlot.occupant = null;
        data.frontSlots[slotIndex] = null;
        data.backSlots[slotIndex] = null;
    }

    public void RemoveGadgetFromInventory(int slotIndex, StickData data)
    {
        if (gadgetOwned[slotIndex].isEquipped)
        {
            gadgetOwned[slotIndex].data.Remove(player);

        }
        for(int i = gadgetOwned.Length - 1; i > slotIndex; i++)
        {
            gadgetOwned[i-1] = gadgetOwned[i];
        }
        gadgetOwned[gadgetOwned.Length - 1] = null;
    }
}