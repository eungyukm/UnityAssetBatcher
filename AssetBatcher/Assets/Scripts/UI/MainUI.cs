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
    public Button WallButton;

    public UnityAction<UIState> OnUISateChanged;
    
    void Awake()
    {

    }

    private void OnEnable()
    {
        var mainUIRoot = MainUIDocument.GetComponent<UIDocument>().rootVisualElement;
        TitleButton = mainUIRoot.Q<Button>("FloorBtn");
        WallButton = mainUIRoot.Q<Button>("WallBtn");
        
        TitleButton.clicked += TitleButtonPressed;
        WallButton.clicked += WallButtonPressed;
    }

    private void OnDisable()
    {
        TitleButton.clicked -= TitleButtonPressed;
        WallButton.clicked -= WallButtonPressed;
    }

    private void TitleButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.TileUI);
    }

    private void WallButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.WallUI);
    }
}
