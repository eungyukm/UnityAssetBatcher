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

    public UnityAction onSaveAction;
    public UnityAction<UIState> onClosePanel;

    private MapWebRequest _mapWebRequest;

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
        _mapWebRequest.OnCreateMap += MapSaveResult;
    }

    private void SaveMap()
    {
        // TODO : witer와 info idx 수정
        _mapWebRequest.MapCreate(_mapSubject.text, _mapData.text, 1,1);
        
    }

    private void MapSaveResult(int code)
    {
        if (code == 200)
        {
            Debug.Log("저장!");
            onSaveAction?.Invoke();
        }
        else
        {
            Debug.Log("저장 에러!");
            onSaveAction?.Invoke();
        }
    }

    private void ClosePanel()
    {
        onClosePanel?.Invoke(UIState.MainUI);
    }
}
