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
    public UIDocument floorUIDocument;
    public Button floorButton;
    public Button floorTypeButton;

    public UIState UIState = UIState.None;

    // Start is called before the first frame update
    void Start()
    {
        var mainUIRoot = mainUIDocument.GetComponent<UIDocument>().rootVisualElement;
        var floorUIRoot = floorUIDocument.GetComponent<UIDocument>().rootVisualElement;

        floorButton = mainUIRoot.Q<Button>("FloorBtn");
        floorTypeButton = floorUIRoot.Q<Button>("Floor01");

        floorButton.clicked += FloorButtonPressed;
        floorTypeButton.clicked += FloorTypeButtonPressed;
        
        mainUIGO.SetActive(true);
        floorUIGO.SetActive(false);
        UIState = UIState.MainUI;
    }

    private void FloorTypeButtonPressed()
    {
        Debug.Log("FloorTypeButtonPressed!!");

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