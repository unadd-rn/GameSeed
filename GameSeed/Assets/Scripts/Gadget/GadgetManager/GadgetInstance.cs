using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GadgetInstance
{
    public string id;
    public BaseGadget data;
    public bool isEquipped;
    public int currentDurability;
    public int slotIdx; // -1 if unequipped
    public GadgetInstance(BaseGadget baseData) // for spawning purposes
    {
        this.id = System.Guid.NewGuid().ToString();
        this.data = baseData;
        this.isEquipped = false;
        this.currentDurability = baseData.durability; // Starts at max durability
        this.slotIdx = -1;
    }

}