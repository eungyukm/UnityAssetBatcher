using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;

public class MapListPanel : MonoBehaviour
{
    [SerializeField] private UIDocument _mapListPanel;
    private VisualElement rootVisualElement;

    private List<MapListItem> _mapListItems = new List<MapListItem>();
    
    private VisualElement _itemsTab;
    [SerializeField] private VisualTreeAsset _itemRowTemplate;
    private ListView _itemListView;
    private float _itemHeight = 100f;

    private ScrollView _detailSection;
    private TextField _idxField;
    private TextField _subjectField;
    private TextField _writerField;
    private TextField _dateField;

    private void OnEnable()
    {
        rootVisualElement = _mapListPanel.rootVisualElement;
        // rootVisualElement.Q<ScrollView>("ScrollView_Details");
        
        LoadAllItems();
        
        _itemsTab = rootVisualElement.Q<VisualElement>("ItemsTab");
        GenerateListView();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddItem();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DeleteItem();
        }
    }
    
    private void LoadAllItems()
    {
        _mapListItems.Clear();
    }

    private void GenerateListView()
    {
        Func<VisualElement> makeItem = () => _itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            e.Q<Label>("Idx").text = i.ToString();
            e.Q<Label>("Subject").text = i.ToString();
            e.Q<Label>("Writer").text = i.ToString();
            e.Q<Label>("Date").text = i.ToString();
        };
        
        _itemListView = new ListView(_mapListItems, _itemHeight, makeItem, bindItem);
        _itemListView.selectionType = SelectionType.Single;
        _itemListView.style.height = _mapListItems.Count * _itemHeight;
        _itemsTab.Add(_itemListView);
        
        _itemListView.onSelectionChange += ListView_OnSelectionChange;
    }

    private void ListView_OnSelectionChange(IEnumerable<object> selectedItems)
    {
        Debug.Log("Change!");
    }

    private void AddItem()
    {
        Debug.Log("Add");
        MapListItem newItem = new MapListItem();
        
        _mapListItems.Add(newItem);
        
        _itemListView.Rebuild();
        _itemListView.style.height = _mapListItems.Count * _itemHeight;
    }

    private void DeleteItem()
    {
        
    }
}

public class MapListItem
{
    public string Idx;
    public string Subject;
    public string Writer;
    public string Date;
}
