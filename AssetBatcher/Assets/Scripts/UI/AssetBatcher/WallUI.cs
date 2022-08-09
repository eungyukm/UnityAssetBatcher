using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class WallUI : MonoBehaviour
{
    [SerializeField] private UIDocument wallUIDocument;
    [SerializeField] private CardManager cardManager;
    
    private Button _previousButton;

    private VisualElement _wallUIRoot;
    
    private Button _wallTypeButton01;
    
    public UnityAction<UIState> OnClosePanel;

    private void OnEnable()
    {
        _wallUIRoot = wallUIDocument.rootVisualElement;
        _previousButton = _wallUIRoot.Q<Button>("PreviousBtn");
        _wallTypeButton01 = _wallUIRoot.Q<Button>("Wall01");

        _previousButton.clicked += ClosePanel;
        _wallTypeButton01.clicked += WallTypeButton01Pressed;
    }

    private void OnDisable()
    {
        _previousButton.clicked -= ClosePanel;
        _wallTypeButton01.clicked -= WallTypeButton01Pressed;
    }

    private void ClosePanel()
    {
        UIState goToPanel = UIState.MainUI;
        OnClosePanel?.Invoke(goToPanel);
    }

    private void WallTypeButton01Pressed()
    {
        cardManager.ActivateCard();
    }
}
