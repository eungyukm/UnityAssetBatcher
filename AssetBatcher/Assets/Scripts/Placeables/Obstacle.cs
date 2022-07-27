using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Placeable
{
    private void Awake()
    {
        pType = Placeable.PlaceableType.Obstacle;
    }
}
