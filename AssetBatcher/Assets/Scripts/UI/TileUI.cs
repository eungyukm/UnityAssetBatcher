using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class TileUI : MonoBehaviour
{
    public UIDocument floorUIDocument;
    public Button tileTypeButton;
    public Button floorTypeButton02;

    public Button PreviousButton;
    

    public CardManager CardManager;

    public UnityAction OnClosePanel;

    private VisualElement tileUIRoot;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        tileUIRoot = floorUIDocument.GetComponent<UIDocument>().rootVisualElement;
        tileTypeButton = tileUIRoot.Q<Button>("Floor01");
        PreviousButton = tileUIRoot.Q<Button>("PreviousBtn");
        
        tileTypeButton.clicked += TileTypeButtonPressed;

        floorTypeButton02 = tileUIRoot.Q<Button>("Floor02");
        floorTypeButton02.clicked += () =>
        {
        };

        PreviousButton.clickable.clicked += () =>
        {
            Debug.Log("PreviousButton Clicked!!");
            OnClosePanel?.Invoke();
        };
    }

    private void OnDisable()
    {
        tileTypeButton.clicked -= TileTypeButtonPressed;
    }

    private void TileTypeButtonPressed()
    {
        CardManager.ActivateCard();
    }
}
