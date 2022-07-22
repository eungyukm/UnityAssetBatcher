using System.Collections;
using System.Collections.Generic;
using Program;
using UnityEngine;


public class ObjectSO : MonoBehaviour
{
    [SerializeField] private MapMetaData _metaData;

    public ObjectSO(GameObject mapObject, int idx)
    {
        MapMetaData metaData = new MapMetaData();
        metaData.idx = idx;
        metaData.model_label = mapObject.name;
        
        var position = mapObject.transform.position;
        
        metaData.pos_x = SetRoundValue(position.x);
        metaData.pos_y = SetRoundValue(position.y);
        metaData.pos_z = SetRoundValue(position.z);

        var scale = mapObject.transform.localScale;
        
        metaData.scale_x = SetRoundValue(scale.x);
        metaData.scale_y = SetRoundValue(scale.y);
        metaData.scale_z = SetRoundValue(scale.z);

        var rotation = mapObject.transform.rotation;
        metaData.rotation_x = SetRoundValue(rotation.x);
        metaData.rotation_y = SetRoundValue(rotation.y);
        metaData.rotation_z = SetRoundValue(rotation.z);

        _metaData = metaData;
    }

    private float SetRoundValue(float value)
    {
        return (float)System.Math.Round(value, 3);
    }
    
    
}
