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
    public GadgetInstance(BaseGadget baseData) // for spawning purposes
    {
        this.id = System.Guid.NewGuid().ToString();
        this.data = baseData;
        this.isEquipped = true;
        this.currentDurability = baseData.durability; // Starts at max durability
    }
}