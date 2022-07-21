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

    private void Start()
    {
        _worldObjectsManager = WorldObjectsManager.instance;
    }

    public string SaveMapData()
    {
        int count = 0;
        int length = _worldObjectsManager.worldObjects.Count;
        MapMetaData[] mapMetaDatas = new MapMetaData[length];
        foreach (var obstacle in _worldObjectsManager.worldObjects)
        {
            MapMetaData metaData = new MapMetaData();
            metaData.idx = count;
            metaData.model_label = obstacle.gameObject.name;
            var position = obstacle.gameObject.transform.position;
            metaData.pos_x = position.x;
            metaData.pos_y = position.y;
            metaData.pos_z = position.z;

            var rotation = obstacle.transform.rotation;
            metaData.rotation_x = rotation.x;
            metaData.rotation_y = rotation.y;
            metaData.rotation_z = rotation.z;

            var localScale = obstacle.transform.localScale;
            metaData.scale_x = localScale.x;
            metaData.scale_y = localScale.y;
            metaData.scale_z = localScale.z;
            mapMetaDatas[count] = metaData;
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

    private void CreateMapData()
    {
        
    }
}
