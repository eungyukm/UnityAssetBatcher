using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;


public class PanelManager : MonoBehaviour
{
    public GameObject mainUIGO;
    public GameObject floorUIGO;
    public GameObject wallUIGO;
    public GameObject propUIGO;
    public GameObject mapSaveUIGO;
    
    public TileUI TileUI;
    public MainUI MainUI;
    public WallUI WallUI;
    public PropUI PropUI;
    public MapSaveUI MapSaveUI;
    public TopPanelUI TopPanelUI;

    public UIState UIState = UIState.None;

    [SerializeField] private CardManager cardManager;
    [SerializeField] private GridSystem gridSystem;

    private void OnEnable()
    {
        MainUI.OnUISateChanged += PanelSwitch;
        TileUI.OnClosePanel += PanelSwitch;
        WallUI.OnClosePanel += PanelSwitch;
        PropUI.OnClosePanel += PanelSwitch;
        MapSaveUI.onClosePanel += PanelSwitch;
        TopPanelUI.OnSaveAction += PanelSwitch;
    }

    private void OnDisable()
    {
        TileUI.OnClosePanel -= PanelSwitch;
        WallUI.OnClosePanel -= PanelSwitch;
        PropUI.OnClosePanel -= PanelSwitch;
        MapSaveUI.onClosePanel -= PanelSwitch;
        TopPanelUI.OnSaveAction -= PanelSwitch;
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

    private void PanelSwitch(UIState uiState)
    {
        UIState = uiState;
        switch (UIState)
        {
            case UIState.None:
                gridSystem.SwitchGridType(GridSystemType.None);
                break;
            case UIState.MainUI:
                AllUISetActiveFalse();
                mainUIGO.SetActive(true);
                gridSystem.SwitchGridType(GridSystemType.None);
                break;
            case UIState.TileUI:
                AllUISetActiveFalse();
                floorUIGO.SetActive(true);
                cardManager.LoadDeck(DeckType.Tile);
                gridSystem.SwitchGridType(GridSystemType.Tile);
                break;
            case UIState.WallUI:
                AllUISetActiveFalse();
                wallUIGO.SetActive(true);
                cardManager.LoadDeck(DeckType.Wall);
                gridSystem.SwitchGridType(GridSystemType.Wall);
                break;
            case UIState.PropUI:
                AllUISetActiveFalse();
                propUIGO.SetActive(true);
                cardManager.LoadDeck(DeckType.Prop);
                gridSystem.SwitchGridType(GridSystemType.None);
                break;
            case UIState.MapSaveUI:
                AllUISetActiveFalse();
                mapSaveUIGO.SetActive(true);
                break;
        }
    }

    void AllUISetActiveFalse()
    {
        mainUIGO.SetActive(false);
        floorUIGO.SetActive(false);
        wallUIGO.SetActive(false);
        mapSaveUIGO.SetActive(false);
    }
}

public enum UIState
{
    None,
    MainUI,
    TileUI,
    WallUI,
    PropUI,
    MapSaveUI,
}