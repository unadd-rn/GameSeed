using System;
using UnityEngine;

[Serializable]
public class BodyInstance
{
    public string id;
    public BodyType data; // reference to the SO blueprint
    public bool isEquipped;
    public int currentDurability;

    public BodyInstance(BodyType baseData)
    {   
        Debug.Log("masukin body di sini");
        this.id = System.Guid.NewGuid().ToString();
        this.data = baseData;
        this.isEquipped = false;
        this.currentDurability = baseData.durability;
    }
}