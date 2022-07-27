using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
