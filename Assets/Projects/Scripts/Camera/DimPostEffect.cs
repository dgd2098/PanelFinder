using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DimPostEffect : MonoBehaviour
{
    [Range(0f, 1f)] public float dimAmount = 0.5f;
    private Material mat;

    void Start()
    {
        Shader shader = Shader.Find("Custom/DimEffect");
        if (shader == null)
        {
            Debug.LogError("Custom/DimEffect shader not found!");
            return;
        }
        mat = new Material(shader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (mat != null)
        {
            mat.SetFloat("_DimAmount", dimAmount);
            Graphics.Blit(src, dest, mat);
        }
        else
            Graphics.Blit(src, dest);
    }
}
