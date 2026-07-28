using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeEffect : MonoBehaviour
{
    public Material mat;

    public GameObject EdgeCreate(GameObject go, Transform parent)
    {
        Mesh m = go.GetComponent<MeshFilter>().mesh;

        Mesh r = MeshProcessor.processForOutlineMesh(m);

        GameObject f = new GameObject();
        f.transform.position = go.transform.position;
        f.transform.rotation = go.transform.rotation;
        f.transform.localScale = go.transform.localScale;
        f.transform.parent = parent;
        f.name = " processed outline";

        f.AddComponent<MeshFilter>().mesh = r;
        f.AddComponent<MeshRenderer>().material = mat;
        f.GetComponent<MeshRenderer>().material.SetFloat("_Width", 0.003f / go.transform.localScale.x);

        Renderer fRenderer = f.GetComponent<MeshRenderer>();
        fRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        if (f.gameObject.GetComponent<CameraCulling>() == null)
        {
            f.gameObject.AddComponent<CameraCulling>();
        }

        return f;
    }
}
