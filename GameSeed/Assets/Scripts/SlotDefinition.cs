using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDefinition
{
    public int slotId;
    public Vector3 localPosition; // posisi di stick
    public GadgetInstance occupant;

    [System.NonSerialized]
    public GameObject spawnedVisual;

}
