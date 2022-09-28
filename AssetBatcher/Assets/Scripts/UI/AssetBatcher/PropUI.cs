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
    private Button _propTypeButton02;
    private Button _propTypeButton03;
    private Button _propTypeButton04;
    private Button _propTypeButton05;
    
    public UnityAction<UIState> OnClosePanel;

    private void OnEnable()
    {
        _propUIRoot = propUIDocument.rootVisualElement;
        _previousButton = _propUIRoot.Q<Button>("PreviousBtn");
        
        _propTypeButton01 = _propUIRoot.Q<Button>("Prop01");
        _propTypeButton02 = _propUIRoot.Q<Button>("Prop02");
        _propTypeButton03 = _propUIRoot.Q<Button>("Prop03");
        _propTypeButton04 = _propUIRoot.Q<Button>("Prop04");
        _propTypeButton05 = _propUIRoot.Q<Button>("Prop05");

        _previousButton.clicked += ClosePanel;
        
        _propTypeButton01.clicked += PropTypeButton01Pressed;
        _propTypeButton02.clicked += PropTypeButton02Pressed;
        _propTypeButton03.clicked += PropTypeButton03Pressed;
        _propTypeButton04.clicked += PropTypeButton04Pressed;
        _propTypeButton05.clicked += PropTypeButton05Pressed;
    }

    private void OnDisable()
    {
        _previousButton.clicked -= ClosePanel;
        
        _propTypeButton01.clicked -= PropTypeButton01Pressed;
        _propTypeButton02.clicked -= PropTypeButton02Pressed;
        _propTypeButton03.clicked -= PropTypeButton03Pressed;
        _propTypeButton04.clicked -= PropTypeButton04Pressed;
        _propTypeButton05.clicked -= PropTypeButton05Pressed;
    }

    private void ClosePanel()
    {
        UIState goToPanel = UIState.MainUI;
        OnClosePanel?.Invoke(goToPanel);
    }

    private void PropTypeButton01Pressed()
    {
        StartCoroutine(CardChangeRoutine(0));
    }
    
    private void PropTypeButton02Pressed()
    {
        StartCoroutine(CardChangeRoutine(1));
    }
    
    private void PropTypeButton03Pressed()
    {
        StartCoroutine(CardChangeRoutine(2));
    }
    
    private void PropTypeButton04Pressed()
    {
        StartCoroutine(CardChangeRoutine(3));
    }
    
    private void PropTypeButton05Pressed()
    {
        StartCoroutine(CardChangeRoutine(4));
    }

    private IEnumerator CardChangeRoutine(int index)
    {
        cardManager.ChangeCard(index);
        yield return new WaitForSeconds(.3f);
        cardManager.ActivateCard();    
    }
}
