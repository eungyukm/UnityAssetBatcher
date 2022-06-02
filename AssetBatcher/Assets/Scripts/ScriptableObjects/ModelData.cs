using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewModel", menuName = "SO/Model Data")]
public class ModelData : ScriptableObject
{
    [Header("Model graphics")] public Sprite modelImage;

    // [Header("List of Placealbes")
    public Vector3[] relativeOffsets;
}
