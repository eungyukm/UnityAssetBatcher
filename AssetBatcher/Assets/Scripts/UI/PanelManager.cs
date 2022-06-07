using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PanelManager : MonoBehaviour
{
    public GameObject mainUIGO;
    public GameObject floorUIGO;
    
    public UIDocument mainUIDocument;

    public Button floorButton;

    public UIState UIState = UIState.None;


    // Start is called before the first frame update
    void Start()
    {
        var mainUIRoot = mainUIDocument.GetComponent<UIDocument>().rootVisualElement;


        floorButton = mainUIRoot.Q<Button>("FloorBtn");


        floorButton.clicked += FloorButtonPressed;


        mainUIGO.SetActive(true);
        floorUIGO.SetActive(false);
        UIState = UIState.MainUI;
        
        // floorButton
        
        // TODO : DragAndDrop UIElement로 변경
        // DragAndDropManipulator manipulator =
        //     new DragAndDropManipulator(floorUIDocument.rootVisualElement.Q<VisualElement>("object"));
    }

    void FloorButtonPressed()
    {
        Debug.Log("floorButton Pressed!!");
        UIState = UIState.FloorUI;
        PanelSwitch();
    }

    void PanelSwitch()
    {
        switch (UIState)
        {
            case UIState.None:
                break;
            case UIState.MainUI:
                AllUISetActiveFalse();
                mainUIGO.SetActive(true);
                break;
            case UIState.FloorUI:
                Debug.Log("Floor UI Called!!");
                AllUISetActiveFalse();
                mainUIDocument.enabled = false;
                floorUIGO.SetActive(true);
                break;
        }
    }

    void AllUISetActiveFalse()
    {
        mainUIGO.SetActive(false);
        floorUIGO.SetActive(false);
    }
}

public enum UIState
{
    None,
    MainUI,
    FloorUI
}