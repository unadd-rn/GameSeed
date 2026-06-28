using System;

[Serializable]
public class BodyInstance
{
    public string id;
    public BodyType data; // reference to the SO blueprint
    public bool isEquipped;

    public BodyInstance(BodyType baseData)
    {
        this.id = System.Guid.NewGuid().ToString();
        this.data = baseData;
        this.isEquipped = false;
    }
}