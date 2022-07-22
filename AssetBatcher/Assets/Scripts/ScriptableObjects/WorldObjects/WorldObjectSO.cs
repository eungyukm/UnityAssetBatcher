using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldObject", menuName = "SO/WorldObjectSO")]
public class WorldObjectSO : ScriptableObject
{
    private List<ObjectSO> _placeableDatas = new List<ObjectSO>();
    public List<ObjectSO> PlaceableDatas => _placeableDatas;

    public void Add(ObjectSO objectSo, int count = 1)
    {
        if (count <= 0)
        {
            return;
        }
        Debug.Log("Add!");
        PlaceableDatas.Add(objectSo);
    }

    public void Remove(ObjectSO objectSo, int count = 1)
    {
        if (count <= 0)
        {
            return;
        }

        PlaceableDatas.Remove(objectSo);
    }
}
