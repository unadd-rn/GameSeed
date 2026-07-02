using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetDatabase", menuName = "Gadget/Database")]
public class GadgetDatabase : ScriptableObject
{
    public BaseGadget[] allGadgets;

    public BaseGadget GetByUniqueName(string UN)
    {
        foreach (var g in allGadgets)
            if (g != null && g.uniqueName == UN)
                return g;

        Debug.LogWarning($"Gadget dengan unique name {UN} gak ketemu di database!");
        return null;
    }
}
