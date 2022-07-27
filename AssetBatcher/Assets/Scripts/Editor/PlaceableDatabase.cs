using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaceableDatabase : EditorWindow
{
    private Sprite _defaultItemIcon;
    private static List<PlaceableData> _placeableDatas = new List<PlaceableData>();
    
    private VisualElement _itemsTab;
    private static VisualTreeAsset _itemRowTemplate;
    private ListView _itemListView;
    private float _itemHeight = 60f;

    private ScrollView _detailSection;
    private VisualElement _largeDisplayIcon;
    private TextField _itemName;
    private TextField _description;
    private DropdownField _dropdownField;
    private PlaceableData _activeItem;

    private TextField _assetName;
    private string _name;

    [MenuItem("Item/Placeable Database")]
    public static void Init()
    {
        PlaceableDatabase wnd = GetWindow<PlaceableDatabase>();
        wnd.titleContent = new GUIContent("Placeable Database");

        Vector2 size = new Vector2(800, 475);
        wnd.minSize = size;
        wnd.maxSize = size;
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ART/UI_Toolkit/UIDocument/Editor/PlaceableDatabase.uxml");
        VisualElement rootFromUXML = visualTree.Instantiate();
        rootVisualElement.Add(rootFromUXML);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ART/UI_Toolkit/UIDocument/Editor/PlaceableDatabase.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        _itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ART/UI_Toolkit/UIDocument/Editor/ItemRowTemplate.uxml");
        _defaultItemIcon =
            (Sprite) AssetDatabase.LoadAssetAtPath("Assets/ART/UI/Editor/UnknownIcon.png", typeof(Sprite));

        LoadAllItems();
        
        _itemsTab = rootVisualElement.Q<VisualElement>("ItemsTab");
        GenerateListView();
        
        _detailSection = rootVisualElement.Q<ScrollView>("ScrollView_Details");
        _detailSection.style.visibility = Visibility.Hidden;
        _largeDisplayIcon = _detailSection.Q<VisualElement>("Icon");
        _itemName = _detailSection.Q<TextField>("ItemName");
        _description = _detailSection.Q<TextField>("Description");
        _dropdownField = _detailSection.Q<DropdownField>("ItemType");
        
        rootVisualElement.Q<Button>("Btn_AddItem").clicked += AddItem_OnClick;
        
        _detailSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            _activeItem.FriendlyName = evt.newValue;
            _itemListView.Rebuild();
        });
        
        _detailSection.Q<ObjectField>("IconPicker").RegisterValueChangedCallback(evt =>
        {
            Sprite newSprite = evt.newValue as Sprite;
            
            _activeItem.Icon = newSprite == null ? _defaultItemIcon : newSprite;
            _largeDisplayIcon.style.backgroundImage = newSprite == null ? _defaultItemIcon.texture : newSprite.texture;
            _itemListView.Rebuild();
        });
        
        _detailSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
        {
            _activeItem.Description = evt.newValue; 
            _itemListView.Rebuild();
        });
        
        rootVisualElement.Q<Button>("Btn_DeleteItem").clicked += DeleteItem_OnClick;
        
        List<string> items = new List<string>();
        items.Add("Unit");
        items.Add("Obstacle");
        items.Add("Building");
        _dropdownField.choices = items;

        _assetName = rootVisualElement.Q<TextField>("AssetName");
    }

    private void LoadAllItems()
    {
        _placeableDatas.Clear();
    
        string[] allPaths = Directory.GetFiles("Assets/ScriptableObjects/GameData/04_PlaceableData", "*.asset", SearchOption.AllDirectories);
    
        foreach (var path in allPaths)
        {
            string cleanedPath = path.Replace("\\", "/");
            _placeableDatas.Add((PlaceableData)AssetDatabase.LoadAssetAtPath(cleanedPath,typeof(PlaceableData)));
        }
    }
    
    private void GenerateListView()
    {
        Func<VisualElement> makeItem = () => _itemRowTemplate.CloneTree();
    
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            e.Q<VisualElement>("Icon").style.backgroundImage =
                _placeableDatas[i] == null ? _defaultItemIcon.texture : _placeableDatas[i].Icon.texture;
        
            e.Q<Label>("Name").text = _placeableDatas[i].FriendlyName;
        };
    
        _itemListView = new ListView(_placeableDatas, _itemHeight, makeItem, bindItem);
        _itemListView.selectionType = SelectionType.Single;
        _itemListView.style.height = _placeableDatas.Count * _itemHeight;
        _itemsTab.Add(_itemListView);
        
        _itemListView.onSelectionChange += ListView_OnSelectionChange;
    }

    private void ListView_OnSelectionChange(IEnumerable<object> selectedItems)
    {
        _activeItem = (PlaceableData) selectedItems.First();

        SerializedObject so = new SerializedObject(_activeItem);
        _detailSection.Bind(so);

        if (_activeItem.Icon != null)
        {
            _largeDisplayIcon.style.backgroundImage = _activeItem.Icon.texture;
        }

        if (_activeItem.FriendlyName != null)
        {
            _itemName.value = _activeItem.FriendlyName;
        }

        if (_activeItem.Description != null)
        {
            _description.value = _activeItem.Description;
        }

        _detailSection.style.visibility = Visibility.Visible;
    }

    private void AddItem_OnClick()
    {
        PlaceableData newItem = CreateInstance<PlaceableData>();
        newItem.FriendlyName = $"New Item";
        newItem.Icon = _defaultItemIcon;
        newItem.Description = "";

        _name = _assetName.value;
        AssetDatabase.CreateAsset(newItem, $"Assets/ScriptableObjects/GameData/04_PlaceableData/{_name}.asset");
        
        _placeableDatas.Add(newItem);
        
        _itemListView.Rebuild();
        _itemListView.style.height = _placeableDatas.Count * _itemHeight;
    }

    private void DeleteItem_OnClick()
    {
        string path = AssetDatabase.GetAssetPath(_activeItem);
        AssetDatabase.DeleteAsset(path);

        _placeableDatas.Remove(_activeItem);
        _itemListView.Rebuild();

        _detailSection.style.visibility = Visibility.Hidden;
    }
}
