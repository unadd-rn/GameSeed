using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gadget", menuName = "Gadgets/Gadget Effect")]
public abstract class BaseGadget : ScriptableObject
{
    public string gadgetName = "Default";
    [TextArea(3,5)]
    public string description = "This is the description of the gadget";
    public Sprite model;
    public abstract void Apply(GameObject target);
    public int durability;
    public int weight;

}