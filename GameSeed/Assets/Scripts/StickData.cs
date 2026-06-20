using UnityEngine;

[CreateAssetMenu(fileName = "NewStickData", menuName = "Stick Throw/Stick Data")]
public class StickData : ScriptableObject
{
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
}