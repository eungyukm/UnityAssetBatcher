using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class MainUI : MonoBehaviour
{
    public UIDocument MainUIDocument;
    private Button TitleButton;
    private Button WallButton;
    private Button PropButton;

    public UnityAction<UIState> OnUISateChanged;
    
    void Awake()
    {

    }

    private void OnEnable()
    {
        var mainUIRoot = MainUIDocument.GetComponent<UIDocument>().rootVisualElement;
        TitleButton = mainUIRoot.Q<Button>("FloorBtn");
        WallButton = mainUIRoot.Q<Button>("WallBtn");
        PropButton = mainUIRoot.Q<Button>("PropBtn");
        
        
        TitleButton.clicked += TitleButtonPressed;
        WallButton.clicked += WallButtonPressed;
        PropButton.clicked += PropButtonPressed;
    }

    private void OnDisable()
    {
        TitleButton.clicked -= TitleButtonPressed;
        WallButton.clicked -= WallButtonPressed;
        PropButton.clicked -= PropButtonPressed;
    }

    private void TitleButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.TileUI);
    }

    private void WallButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.WallUI);
    }

    private void PropButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.PropUI);
    }
}
