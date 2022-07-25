using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectsManager : MonoBehaviour
{
    [SerializeField] private WorldObjectSO _currentWorldObject = default;

    public WorldObjectSO WorldObjectSo => _currentWorldObject;

    [SerializeField] private ObjectEventChannelSO _addObjectEvent = default;
    [SerializeField] private ObjectEventChannelSO _removeObjectEvent = default;

    private void OnEnable()
    {
        _addObjectEvent.OnEventRaised += AddObjec;
        _removeObjectEvent.OnEventRaised += RemoveObject;
    }

    private void OnDisable()
    {
        _addObjectEvent.OnEventRaised -= AddObjec;
        _removeObjectEvent.OnEventRaised -= RemoveObject;        
    }

    private void AddObjec(ObjectSO objectSo)
    {
        _currentWorldObject.Add(objectSo);
    }

    private void RemoveObject(ObjectSO objectSo)
    {
        _currentWorldObject.Remove(objectSo);
    }
}
