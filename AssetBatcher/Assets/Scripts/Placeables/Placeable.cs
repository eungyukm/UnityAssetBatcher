using UnityEngine;

public class Placeable : MonoBehaviour
{
    public PlaceableType pType;

    public enum PlaceableType
    {
        Unit,
        Obstacle,
        Building,
    }
}
