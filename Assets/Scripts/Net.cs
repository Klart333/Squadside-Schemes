using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour
{
    private static int ColorName = Shader.PropertyToID("_TheColor");

    private MaterialPropertyBlock block;
    private new MeshRenderer renderer;

    private Color originalColor;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        renderer = GetComponent<MeshRenderer>();
        originalColor = renderer.material.GetColor(ColorName);
    }

    public void Highlight()
    {
        renderer.GetPropertyBlock(block);
        block.SetColor(ColorName, originalColor * 0.45f);
        renderer.SetPropertyBlock(block);
    }

    public void ResetNet()
    {
        renderer.GetPropertyBlock(block);
        block.SetColor(ColorName, originalColor * 0.2f);
        renderer.SetPropertyBlock(block);
    }
}
