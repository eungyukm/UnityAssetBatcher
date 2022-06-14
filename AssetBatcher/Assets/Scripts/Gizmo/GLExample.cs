using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLExample : MonoBehaviour
{
    public Material mat;

    public Mesh mesh;
    public Material material;
    public void Update() {
        // will make the mesh appear in the Scene at origin position
        Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
    }
}
