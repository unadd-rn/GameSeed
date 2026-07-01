using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickBody : MonoBehaviour
{
    [Header("Component Reference")]
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public float stickLength = 2f;
    [Header("Info Visual")]
    public string stickName;
    public string description;
    public Sprite stickIcon; // Gambar untuk di UI Inventory
    public Mesh stickMesh;
    public Material stickMaterial;

    [Header("Body Information")]
    [SerializeField] BodyType[] bodies;

    void Start()
    {
        if (bodies.Length > 0) {
            int randIndex = Random.Range(0, bodies.Length);
            Debug.Log(bodies.Length);
            BodyType tempBodyType = bodies[randIndex];
            ApplyPreview(tempBodyType);
        }
    }

    public void ApplyPreview(BodyType body)
    {
        if(body == null) return;

        this.stickName = body.stickName;
        this.description = body.description;
        this.stickIcon = body.stickIcon;

        if(meshFilter != null)
            meshFilter.mesh = body.stickMesh;
        
        if(meshRenderer != null)
            meshRenderer.material = body.stickMaterial;
    }
}