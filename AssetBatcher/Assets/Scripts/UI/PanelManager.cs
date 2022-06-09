using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class PanelManager : MonoBehaviour
{
    public GameObject mainUIGO;
    public GameObject floorUIGO;
    
    public TileUI TileUI;
    public MainUI MainUI;

    public UIState UIState = UIState.None;

    private void OnEnable()
    {
        MainUI.OnUISateChanged += PanelSwitch;
        TileUI.OnClosePanel += TileButtonClosed;
    }

    private void OnDisable()
    {
        TileUI.OnClosePanel -= TileButtonClosed;
    }
    
    void Start()
    {
        InitUI();
    }

    private void InitUI()
    {
        mainUIGO.SetActive(true);
        floorUIGO.SetActive(false);
        UIState = UIState.MainUI;
    }

    private void TileButtonClosed()
    {
        PanelSwitch(UIState.MainUI);
    }

    private void PanelSwitch(UIState uiState)
    {
        UIState = uiState;
        switch (UIState)
        {
            case UIState.None:
                break;
            case UIState.MainUI:
                AllUISetActiveFalse();
                mainUIGO.SetActive(true);
                break;
            case UIState.TileUI:
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
    TileUI
}