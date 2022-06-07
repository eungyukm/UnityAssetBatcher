using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FloorUI : MonoBehaviour
{
    public UIDocument floorUIDocument;
    public Button floorTypeButton;
    public Button floorTypeButton02;
    

    public CardManager CardManager;

    private void OnEnable()
    {
        var floorUIRoot = floorUIDocument.GetComponent<UIDocument>().rootVisualElement;
        floorTypeButton = floorUIRoot.Q<Button>("Floor01");
        if (floorTypeButton == null)
        {
            Debug.LogError("floorTypeButton is null!!");
        }
        else
        {
            Debug.Log("FloorType Button Call!!");
            Debug.Log("floor Type name : " + floorTypeButton.name);
            floorTypeButton.clicked += FloorTypeButtonPressed;
        }

        floorTypeButton02 = floorUIRoot.Q<Button>("Floor02");
        floorTypeButton02.clicked += () =>
        {
            Debug.Log("!!!!!");
        };
    }

    private void FloorTypeButtonPressed()
    {
        Debug.Log("FloorTypeButtonPressed!!");
        CardManager.ActivateCard();
    }
}
