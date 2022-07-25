using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using Program;
using Newtonsoft.Json;

public class MapSaveManager : MonoBehaviour
{
    private WorldObjectsManager _worldObjectsManager;
    public StringEventChannelSO StringEventChannelSo = default;
    public VoidEventChannelSO SaveButtonEventChannelSo = default;

    private void Awake()
    {
        _worldObjectsManager = GetComponent<WorldObjectsManager>();
    }

    private void OnEnable()
    {
        SaveButtonEventChannelSo.OnEventRaise += SaveButtonClicked;
    }

    private void OnDisable()
    {
        SaveButtonEventChannelSo.OnEventRaise -= SaveButtonClicked;
    }

    private void SaveButtonClicked()
    {
        string mapData = SaveMapDataToJson();
        StringEventChannelSo.RaiseEvent(mapData);
    }

    public string SaveMapDataToJson()
    {
        int count = 0;
        int length = _worldObjectsManager.WorldObjectSo.GetCount();
        MapMetaData[] mapMetaDatas = new MapMetaData[length];
        foreach (var placeableData in _worldObjectsManager.WorldObjectSo.PlaceableDatas)
        {
            MapMetaData data = placeableData.MetaData;
            mapMetaDatas[count] = data;
            count++;
        }

        string json = JsonConvert.SerializeObject(mapMetaDatas);
        if (!string.IsNullOrEmpty(json))
        {
            return json;
        }
        else
        {
            return "None";
        }
    }
}
