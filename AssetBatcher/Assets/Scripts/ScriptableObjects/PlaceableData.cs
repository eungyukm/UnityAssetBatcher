using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewPlaceable", menuName = "SO/Placeable Data", order = 1)]
public class PlaceableData : ScriptableObject
{
    public string ID = Guid.NewGuid().ToString().ToUpper();
    public string FriendlyName;
    public string Description;
    [SerializeField]
    public PlaceableType pType;
    public GameObject placeablePrefab;
    public Sprite Icon;
    
    [SerializeField]
    public enum PlaceableType
    {
        Unit,
        Obstacle,
        Building,
    }
}
