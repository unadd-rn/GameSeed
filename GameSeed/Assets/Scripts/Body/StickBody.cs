using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickBody : MonoBehaviour
{
    public float stickLength = 2f;
    [Header("Info Visual")]
    public string stickName;
    public string description;
    public Sprite stickIcon; // Gambar untuk di UI Inventory
    public Mesh stickMesh;
    public Material stickMaterial;
}