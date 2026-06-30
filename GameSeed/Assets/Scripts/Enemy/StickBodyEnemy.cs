using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickBodyENemy : MonoBehaviour
{
    [Header("Component Reference")]
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    [SerializeField] BodyType[] bodies;
    public BodyType CurrentBodyData { get; private set; }

    void Start()
    {
        if (bodies.Length > 0) {
            int randIndex = Random.Range(0, bodies.Length);
            CurrentBodyData = bodies[randIndex];
            ApplyPreview(CurrentBodyData);
        }
    }

    public void ApplyPreview(BodyType body)
    {
        if(body == null) return;

        if(meshFilter != null)
            meshFilter.mesh = body.stickMesh;
        
        if(meshRenderer != null)
            meshRenderer.material = body.stickMaterial;
    }
}