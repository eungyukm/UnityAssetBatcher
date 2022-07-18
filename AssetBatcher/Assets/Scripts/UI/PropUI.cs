using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


public class PropUI : MonoBehaviour
{
    [SerializeField] private UIDocument propUIDocument;
    [SerializeField] private CardManager cardManager;

    private Button _previousButton;
    private VisualElement _propUIRoot;
    private Button _propTypeButton01;
    public UnityAction<UIState> OnClosePanel;

    private void OnEnable()
    {
        _propUIRoot = propUIDocument.rootVisualElement;
        _previousButton = _propUIRoot.Q<Button>("PreviousBtn");
        _propTypeButton01 = _propUIRoot.Q<Button>("Prop01");

        _previousButton.clicked += ClosePanel;
        _propTypeButton01.clicked += PropTypeButton01Pressed;
    }

    private void OnDisable()
    {
        _previousButton.clicked -= ClosePanel;
        _propTypeButton01.clicked -= PropTypeButton01Pressed;
    }

    private void ClosePanel()
    {
        UIState goToPanel = UIState.MainUI;
        OnClosePanel?.Invoke(goToPanel);
    }

    private void PropTypeButton01Pressed()
    {
        cardManager.ActivateCard();
    }
}
