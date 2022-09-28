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
    private Button _wallTypeButton02;
    private Button _wallTypeButton03;
    
    public UnityAction<UIState> OnClosePanel;

    private void OnEnable()
    {
        _wallUIRoot = wallUIDocument.rootVisualElement;
        _previousButton = _wallUIRoot.Q<Button>("PreviousBtn");
        _wallTypeButton01 = _wallUIRoot.Q<Button>("Wall01");
        _wallTypeButton02 = _wallUIRoot.Q<Button>("Wall02");
        _wallTypeButton03 = _wallUIRoot.Q<Button>("Wall03");

        _previousButton.clicked += ClosePanel;
        _wallTypeButton01.clicked += WallTypeButton01Pressed;
        _wallTypeButton02.clicked += WallTypeButton02Pressed;
        _wallTypeButton03.clicked += WallTypeButton03Pressed;
    }

    private void OnDisable()
    {
        _previousButton.clicked -= ClosePanel;
        _wallTypeButton01.clicked -= WallTypeButton01Pressed;
        _wallTypeButton02.clicked -= WallTypeButton02Pressed;
        _wallTypeButton03.clicked -= WallTypeButton03Pressed;
    }

    private void ClosePanel()
    {
        UIState goToPanel = UIState.MainUI;
        OnClosePanel?.Invoke(goToPanel);
    }

    private void WallTypeButton01Pressed()
    {
        StartCoroutine(CardChangeRoutine(0));
    }

    private void WallTypeButton02Pressed()
    {
        StartCoroutine(CardChangeRoutine(1));
    }
    
    private void WallTypeButton03Pressed()
    {
        StartCoroutine(CardChangeRoutine(2));
    }

    private IEnumerator CardChangeRoutine(int index)
    {
        cardManager.ChangeCard(index);
        yield return new WaitForSeconds(.3f);
        cardManager.ActivateCard();    
    }
}
