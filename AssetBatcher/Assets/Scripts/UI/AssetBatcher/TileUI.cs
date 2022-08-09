using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class TileUI : MonoBehaviour
{
    [SerializeField] private UIDocument floorUIDocument;
    [SerializeField] private CardManager cardManager;
    
    private Button _previousButton;
    
    private Button _floorTypeButton01;
    private Button _floorTypeButton02;

    public UnityAction<UIState> OnClosePanel;

    private VisualElement _tileUIRoot;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        _tileUIRoot = floorUIDocument.rootVisualElement;
        _previousButton = _tileUIRoot.Q<Button>("PreviousBtn");
        _floorTypeButton01 = _tileUIRoot.Q<Button>("Floor01");
        
        
        _previousButton.clickable.clicked += ClosePanel;
        _floorTypeButton01.clicked += FloorTypeButton01Pressed;

        _floorTypeButton02 = _tileUIRoot.Q<Button>("Floor02");
        _floorTypeButton02.clicked += () =>
        {
        };
    }

    private void OnDisable()
    {
        _previousButton.clickable.clicked += ClosePanel;
        _floorTypeButton01.clicked -= FloorTypeButton01Pressed;
    }
    
    private void ClosePanel()
    {
        UIState goToPanel = UIState.MainUI;
        OnClosePanel?.Invoke(goToPanel);
    }

    private void FloorTypeButton01Pressed()
    {
        cardManager.ActivateCard();
    }
}
