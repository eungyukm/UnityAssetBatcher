using UnityEngine;
using UnityEngine.Serialization;

// Asset Batcher의 전체 UI를 가지고 있는 Manager
public class AssetBatcherUIManager : MonoBehaviour
{
    public GameObject mainUIGO;
    public GameObject floorUIGO;
    public GameObject wallUIGO;
    public GameObject propUIGO;
    public GameObject mapSaveUIGO;
    
    public AssetCategoryUI assetCategoryUI;
    public TileUI tileUI;
    public WallUI wallUI;
    public PropUI propUI;
    
    public MapSaveUI mapSaveUI;
    [FormerlySerializedAs("topPanelUI")] public ManipulateUI manipulateUI;

    public UIState uiState = UIState.None;

    [SerializeField] private CardManager cardManager;
    [SerializeField] private GridSystem gridSystem;

    private void OnEnable()
    {
        assetCategoryUI.OnUISateChanged += PanelSwitch;
        tileUI.OnClosePanel += PanelSwitch;
        wallUI.OnClosePanel += PanelSwitch;
        propUI.OnClosePanel += PanelSwitch;
        mapSaveUI.onClosePanel += PanelSwitch;
        manipulateUI.OnSaveAction += PanelSwitch;

        mapSaveUI.onSaveComplate += MapSave;
    }

    private void OnDisable()
    {
        assetCategoryUI.OnUISateChanged -= PanelSwitch;
        tileUI.OnClosePanel -= PanelSwitch;
        wallUI.OnClosePanel -= PanelSwitch;
        propUI.OnClosePanel -= PanelSwitch;
        mapSaveUI.onClosePanel -= PanelSwitch;
        manipulateUI.OnSaveAction -= PanelSwitch;
        
        mapSaveUI.onSaveComplate -= MapSave;
    }
    
    void Start()
    {
        InitUI();
    }

    private void InitUI()
    {
        mainUIGO.SetActive(true);
        floorUIGO.SetActive(false);
        uiState = UIState.MainUI;
    }

    private void MapSave()
    {
        PanelSwitch(UIState.MainUI);
    }

    private void PanelSwitch(UIState uiState)
    {
        this.uiState = uiState;
        switch (this.uiState)
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