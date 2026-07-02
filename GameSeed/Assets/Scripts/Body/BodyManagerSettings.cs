using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyManagerSettings", menuName = "Settings/BodyManagerSettings")]
public class BodyManagerSettings : ScriptableObject
{
    public BodyType defaultBody;
}