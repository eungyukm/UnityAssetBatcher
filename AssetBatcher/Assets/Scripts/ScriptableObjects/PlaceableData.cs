using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaceable", menuName = "SO/Placeable Data")]
public class PlaceableData : ScriptableObject
{
    [Header("Common")]
    public Placeable.PlaceableType pType;
    public GameObject associatedPrefab;
    public GameObject alternatePrefab;
}
