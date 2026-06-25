using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyOfStick : MonoBehaviour
{
    [Header("Info Visual")]
    public string stickName;
    public string description;
    public Sprite stickIcon; // Gambar untuk di UI Inventory
    public GameObject stickBody; // basically cuma nerima yang di garage

    [Header("Stats")]
    public float damage = 1f;
    public float weight = 1f;
    public float length = 2f;
}