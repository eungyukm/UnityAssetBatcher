using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TopPanelUI : MonoBehaviour
{
    public UIDocument TopPanelDocument;
    
    private Button SelectButtom;
    private Button MoveButton;
    private Button RotationButton;
    private Button ScaleButton;
    private Button AlignmentButton;

    public GridSystem GridSystem;
    public UnitCursor UnitCursor;

    private bool _onSnap = false;

    private void OnEnable()
    {
        var topPanelUIRoot = TopPanelDocument.GetComponent<UIDocument>().rootVisualElement;

        SelectButtom = topPanelUIRoot.Q<Button>("SelectBtn");
        SelectButtom.clicked += SelectButtonPressed;

        MoveButton = topPanelUIRoot.Q<Button>("MoveBtn");
        MoveButton.clicked += MoveButtonPressed;

        RotationButton = topPanelUIRoot.Q<Button>("RotationBtn");
        RotationButton.clicked += RotationButtonPressed;
        
        ScaleButton = topPanelUIRoot.Q<Button>("ScaleBtn");
        ScaleButton.clicked += ScaleButtonPressed;
        
        AlignmentButton = topPanelUIRoot.Q<Button>("AlignmentBtn");
        AlignmentButton.clicked += AlignmentButtonPressed;
    }
    
    private void SelectButtonPressed()
    {
        UnitCursor.SwitchMode(GameTransformMode.SelectMode);
    }

    private void MoveButtonPressed()
    {
        UnitCursor.SwitchMode(GameTransformMode.MoveMode);

        UnitCursor.OnMoveButtonPressed?.Invoke();
    }

    private void RotationButtonPressed()
    {
        UnitCursor.SwitchMode(GameTransformMode.RotationMode);
    }

    private void ScaleButtonPressed()
    {
        UnitCursor.SwitchMode(GameTransformMode.ScaleMode);
    }

    private void AlignmentButtonPressed()
    {
        Debug.Log("OnClicked Snap!!");
        _onSnap = !_onSnap;
        GridSystem.SnapSwitch(_onSnap);
    }
}
