using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickVisual : MonoBehaviour
{
    [SerializeField] private StickData stickData;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    void Awake()
    {
        ApplyVisual();
    }
    public void ApplyVisual()
    {
        if (meshFilter != null && stickData != null && stickData.stickMesh != null)
            meshFilter.mesh = stickData.stickMesh;

        if (meshRenderer != null && stickData != null && stickData.stickMaterial != null)
            meshRenderer.material = stickData.stickMaterial; 
    }
}
