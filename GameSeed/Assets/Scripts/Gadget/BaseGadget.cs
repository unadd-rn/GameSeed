using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gadget", menuName = "Gadgets/Gadget Effect")]
public abstract class BaseGadget : ScriptableObject
{
    [Header("General Info")]
    public string gadgetName = "Default";
    [TextArea(3,5)]
    public string description = "This is the description of the gadget";
    public Sprite model; // icon buat gambar
    public GameObject prefab; // bentuk 3dny
    

    [Header("Stats")]
    public int durability;
    public int weight;

    [Header("Position")]
    public float sizeX;
    public float sizeZ;

    /* Others */
    public abstract void Apply(GameObject target);
    public abstract void Remove(GameObject target);
}

public class GadgetInstance
{
    public BaseGadget data;
    public bool isEquipped;
    public int currentDurability;
}