using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawMesh : MonoBehaviour
{
    Mesh m;

    public Material Material;

    private void OnEnable()
    {
        RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
    }

    private void OnBeginContextRendering(ScriptableRenderContext arg1, List<Camera> arg2)
    {
        m = new Mesh();
        Vector3[] VerteicesArray = new Vector3[3];
        int[] trianglesArray = new int[3];


        VerteicesArray[0] = new Vector3(0, 1, 0);
        VerteicesArray[1] = new Vector3(-1, 0, 0);
        VerteicesArray[2] = new Vector3(1, 0, 0);


        trianglesArray[0] = 0;
        trianglesArray[1] = 1;
        trianglesArray[2] = 2;


        m.vertices = VerteicesArray;
        m.triangles = trianglesArray;
        
        Graphics.DrawMesh(m, Vector3.zero, Quaternion.identity, Material, 0);
    }
}