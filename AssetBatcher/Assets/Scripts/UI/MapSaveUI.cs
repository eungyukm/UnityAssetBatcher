using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class MapSaveUI : MonoBehaviour
{
    [SerializeField] private UIDocument MapSaveUIDocument;

    private VisualElement mapUIRoot;

    private Button _SaveButton;

    private Button _closeButton;
    private TextField _mapSubject;
    private TextField _mapData;

    public UnityAction onSaveComplate;
    public UnityAction<UIState> onClosePanel;
    public UnityAction<int> onSaveButtonClicked;

    private MapWebRequest _mapWebRequest;

    [SerializeField] private StringEventChannelSO _stringEventChannelSo = default;
    [SerializeField] private VoidEventChannelSO _saveButtonEventChannelSo = default;

    private void OnEnable()
    {
        var uiRoot = MapSaveUIDocument.GetComponent<UIDocument>().rootVisualElement;
        _SaveButton = uiRoot.Q<Button>("SaveBtn");
        _closeButton = uiRoot.Q<Button>("CloseBtn");

        _mapSubject = uiRoot.Q<TextField>("MapSubject");
        _mapData = uiRoot.Q<TextField>("MapData");

        _SaveButton.clicked += SaveMap;
        _closeButton.clicked += ClosePanel;

        _mapWebRequest = GetComponent<MapWebRequest>();
        onSaveButtonClicked += MapSaveResult;
        _stringEventChannelSo.OnEventRaised += SetMapData;
    }

    private void SetMapData(string mapData)
    {
        // TODO : witer와 info idx 수정
        _mapWebRequest.MapCreate(_mapSubject.text, mapData, 1,1, onSaveButtonClicked);
    }

    private void SaveMap()
    {
        _saveButtonEventChannelSo.RaiseEvent();
    }

    private void MapSaveResult(int code)
    {
        if (code == 200)
        {
            Debug.Log("저장!");
            onSaveComplate?.Invoke();
        }
        else
        {
            Debug.Log("저장 에러!");
            onSaveComplate?.Invoke();
        }
    }

    private void ClosePanel()
    {
        onClosePanel?.Invoke(UIState.MainUI);
    }
}
