using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshOutline : MonoBehaviour
{
    public Material outlineMaterial;
    public float outlineScale = 1.03f;

    private Material[] originalMaterials;
    private Renderer objRenderer;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        originalMaterials = objRenderer.materials;
        ApplyOutline();
    }

    public void ApplyOutline()
    {
        var newMaterials = new List<Material>(originalMaterials);
        newMaterials.Add(outlineMaterial);
        objRenderer.materials = newMaterials.ToArray();
    }

    public void RemoveOutline()
    {
        objRenderer.materials = originalMaterials;
    }
}
