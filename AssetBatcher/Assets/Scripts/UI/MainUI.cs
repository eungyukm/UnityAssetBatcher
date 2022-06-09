using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class MainUI : MonoBehaviour
{
    public UIDocument MainUIDocument;
    public Button TitleButton;

    public UnityAction<UIState> OnUISateChanged;
    
    void Awake()
    {

    }

    private void OnEnable()
    {
        var mainUIRoot = MainUIDocument.GetComponent<UIDocument>().rootVisualElement;
        TitleButton = mainUIRoot.Q<Button>("FloorBtn");
        TitleButton.clicked += TitleButtonPressed;
    }

    private void OnDisable()
    {
        TitleButton.clicked -= TitleButtonPressed;
    }

    private void TitleButtonPressed()
    {
        Debug.Log("floorButton Pressed!!");
        OnUISateChanged?.Invoke(UIState.TileUI);
    }
}
