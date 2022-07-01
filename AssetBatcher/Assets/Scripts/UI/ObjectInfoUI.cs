using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class ObjectInfoUI : MonoBehaviour
{
    public UIDocument RightUIDocument;
    
    private VisualElement _rightPanelUIRoot;
    private Label _objectNameLabel;
    private Label _objectPostionLabel;
    private Label _objectRotationLabel;
    private Label _objectScaleLabel;

    [FormerlySerializedAs("UnitCursor")] public MouseCursor mouseCursor;

    private void OnEnable()
    {
        _rightPanelUIRoot = RightUIDocument.GetComponent<UIDocument>().rootVisualElement;

        _objectNameLabel = _rightPanelUIRoot.Q<Label>("NameLabel");
        _objectPostionLabel = _rightPanelUIRoot.Q<Label>("PositionLabel");
        _objectRotationLabel = _rightPanelUIRoot.Q<Label>("RotationLabel");
        _objectScaleLabel = _rightPanelUIRoot.Q<Label>("ScaleLabel");
        
        // UnitCursor.OnSelectedObject += OnSelectedObject;
    }
    private void OnDisable()
    {
        
    }  

    private void OnSelectedObject(GameObject selectedObject)
    {
        string name = selectedObject.name;
        _objectNameLabel.text = name;

        double posX = Math.Round(selectedObject.transform.position.x, 3);
        double posY = Math.Round(selectedObject.transform.position.y, 3);
        double posZ = Math.Round(selectedObject.transform.position.z, 3);
        string pos = String.Format("X {0} Y {1} Z {2}", posX, posY, posZ);
        _objectPostionLabel.text = pos;
        
        string rotation = String.Format("X {0} Y {1} Z {2}", selectedObject.transform.rotation.x,
            selectedObject.transform.rotation.y, selectedObject.transform.rotation.z);
        _objectRotationLabel.text = rotation;
        
        string scale = String.Format("X {0} Y {1} Z {2}", selectedObject.transform.localScale.x,
            selectedObject.transform.localScale.y, selectedObject.transform.localScale.z);
        _objectScaleLabel.text = scale;
    }


}
