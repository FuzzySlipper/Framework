using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetVertexColorFromShader : MonoBehaviour {

    [SerializeField] private string _shaderColor = "_PrimaryColor";

    void Awake() {
        var filter = GetComponent<MeshFilter>();
        var renderer = GetComponent<MeshRenderer>();
        var count = filter.mesh.vertexCount;
        Color32 color = renderer.sharedMaterial.GetColor(_shaderColor);
        var colors = new List<Color>(count);
        for (int i = 0; i < count; i++) {
            colors.Add(color);
        }
        filter.mesh.SetColors(colors);
    }
}