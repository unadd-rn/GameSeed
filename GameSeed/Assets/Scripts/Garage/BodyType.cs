using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyType", menuName = "Body Type/Body")]
public abstract class BodyType : ScriptableObject
{
    [Header("Info Visual")]
    public string stickName;
    public string description;
    public Sprite stickIcon; // Gambar untuk di UI Inventory
    public GameObject stickBody; // model(?) 3dnya

    [Header("Stats")]
    public float damage;
    public float weight;

    [Header("Important")]
    public float length = 2f;
}