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
    public Sprite model; // bentukannya(?)
    

    [Header("Stats")]
    public int durability;
    public int weight;

    [Header("Position")]
    public float sizeX;
    public float sizeZ;

    /* Others */
    [HideInInspector] public bool isEquipped = false;
    public abstract void Apply(GameObject target);
}