using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TopPanelUI : MonoBehaviour
{
    public UIDocument TopPanelDocument;

    public Button AlignmentButton;
    public GridSystem GridSystem;

    private bool _onSnap = false;

    private void OnEnable()
    {
        var floorUIRoot = TopPanelDocument.GetComponent<UIDocument>().rootVisualElement;
        AlignmentButton = floorUIRoot.Q<Button>("AlignmentBtn");
        if (AlignmentButton == null)
        {
            Debug.LogError("AlignmentButton is null!!");
        }
        else
        {
            // Debug.Log("FloorType Button Call!!");
            // Debug.Log("floor Type name : " + floorTypeButton.name);
            AlignmentButton.clicked += AlignmentButtonPressed;
        }
    }

    private void AlignmentButtonPressed()
    {
        Debug.Log("OnClicked Snap!!");
        _onSnap = !_onSnap;
        GridSystem.SnapSwitch(_onSnap);
    }
}
