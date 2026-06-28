using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BodyType : ScriptableObject
{
    [Header("Info Visual")]
    public string stickName;
    public string description;
    public Sprite stickIcon; // Gambar untuk di UI Inventory
    // public GameObject stickBody; // model(?) 3dnya
    public Mesh stickMesh;
    public Material stickMaterial;

    // pilih antara: pake mesh dan material trus hrs manual input lagi, atau pake GameObject stickBody, tapi GameObject itu udah harus bentuk jadi (udah punya mesh dan material yang dicombine)

    [Header("Stats")]
    public float damage;
    public float weight = 20; // will be changed later
    public float HP;
}