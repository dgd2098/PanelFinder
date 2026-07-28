using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityCulling : MonoBehaviour
{
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnBecameVisible()
    {
        if (rend != null)
            rend.forceRenderingOff = false;
    }

    private void OnBecameInvisible()
    {
        if (rend != null)
            rend.forceRenderingOff = true;
    }
}
