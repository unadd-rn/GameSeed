using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GadgetSaveData
{
    public string gadgetId;
    public bool isEquipped;
    public int slotIdx;
    public int currentDurability;
}

[System.Serializable]
public class GadgetSaveWrapper
{
    public List<GadgetSaveData> gadgets = new List<GadgetSaveData>();
}