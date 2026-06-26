using UnityEngine;

[CreateAssetMenu(fileName = "NewStickData", menuName = "Stick Throw/Stick Data")]
public class StickData : ScriptableObject
{
    [Header("Info Visual")]
    public string stickName;
    public string description;
    public Sprite stickIcon; // Gambar untuk di UI Inventory
    public GameObject stickBody;

    [Header("Stats")]
    public float damage = 1f;
    public float weight = 1f;

    [Header("Launch Settings")]
    public float velocityScale = 0.5f;
    public float launchForce = 10f;
    public float up = 2f;

    [Header("Physics & Spin Settings")]
    public float stickLength = 2f;
    public float spinScale = 500f;
    public LayerMask stickLayer;

    [Header("UI Layout Configuration")]
    public float sliderOffsetY = 1.5f;

    [Header("Gadget-Related")]
    public float sizeX;
    public float sizeZ;
    public SlotDefinition[] frontSlots; // ada 5 di sepanjang stik
    public SlotDefinition[] backSlots;
    public int canTeleport = 0;
    public int canActivateSafeArea = 0;
}