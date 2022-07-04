using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class TopPanelUI : MonoBehaviour
{
    [SerializeField]private UIDocument topPanelDocument;

    private VisualElement topPanelUIRoot;
    private Button _selectButtom;
    private Button _moveButton;
    private Button _rotationButton;
    private Button _scaleButton;
    private Button _alignmentButton;
    private Button _playButton;

    public GridSystem gridSystem;
    [FormerlySerializedAs("unitCursor")] public MouseCursor mouseCursor;
    public GizmoTransform gizmoTransform;
    
    
    private bool _onSnap = false;

    private const string iconSelectStyle = "Icon_Selected";
    private const string labelSelectStyle = "Label_Selected";

    public InputReader InputReader;

    private void OnEnable()
    {
        InputReader.OnWKeyAction += SelectButtonPressed;
        InputReader.OnGrapPresseAction += MoveButtonPressed;
        InputReader.OnRotationAction += RotationButtonPressed;
        InputReader.OnScaleAction += ScaleButtonPressed;
        
        
        topPanelUIRoot = topPanelDocument.GetComponent<UIDocument>().rootVisualElement;

        _selectButtom = topPanelUIRoot.Q<Button>("SelectBtn");
        _selectButtom.clicked += SelectButtonPressed;

        _moveButton = topPanelUIRoot.Q<Button>("MoveBtn");
        _moveButton.clicked += MoveButtonPressed;

        _rotationButton = topPanelUIRoot.Q<Button>("RotationBtn");
        _rotationButton.clicked += RotationButtonPressed;
        
        _scaleButton = topPanelUIRoot.Q<Button>("ScaleBtn");
        _scaleButton.clicked += ScaleButtonPressed;
        
        _alignmentButton = topPanelUIRoot.Q<Button>("AlignmentBtn");
        _alignmentButton.clicked += AlignmentButtonPressed;

        _playButton = topPanelUIRoot.Q<Button>("PlayBtn");
        _playButton.clicked += PlayButtonPressed;
    }
    
    private void SelectButtonPressed()
    {
        mouseCursor.SwitchMode(MouseCursor.GameTransformMode.SelectMode);
        SwitchTransformMode();
    }

    private void MoveButtonPressed()
    {
        mouseCursor.SwitchMode(MouseCursor.GameTransformMode.MoveMode);
        SwitchTransformMode();
    }

    private void RotationButtonPressed()
    {
        mouseCursor.SwitchMode(MouseCursor.GameTransformMode.RotationMode);
        SwitchTransformMode();
    }

    private void ScaleButtonPressed()
    {
        mouseCursor.SwitchMode(MouseCursor.GameTransformMode.ScaleMode);
        SwitchTransformMode();
    }

    private void AlignmentButtonPressed()
    {
        _onSnap = !_onSnap;
        gridSystem.SnapSwitch(_onSnap);

        if (_onSnap)
        {
            SelectedButtonStyle("Snap");
        }
        else
        {
            DeSelectedButtonStyle("Snap");
        }
    }

    private void PlayButtonPressed()
    {
        SceneManager.LoadScene("InGame");
    }

    private void SwitchTransformMode()
    {
        AllDeSelectedButton();
        switch (mouseCursor.transformMode)
        {
            case MouseCursor.GameTransformMode.SelectMode:
                SelectedButtonStyle("Select");
                break;
            case MouseCursor.GameTransformMode.MoveMode:
                SelectedButtonStyle("Move");
                break;
            case MouseCursor.GameTransformMode.ScaleMode:
                SelectedButtonStyle("Scale");
                break;
            case MouseCursor.GameTransformMode.RotationMode:
                SelectedButtonStyle("Rotation");
                break;
        }
    }

    private void AllDeSelectedButton()
    {
        var SelectIcon = topPanelUIRoot.Q<VisualElement>("SelectIcon");
        var MoveIcon = topPanelUIRoot.Q<VisualElement>("MoveIcon");
        var RotationIcon = topPanelUIRoot.Q<VisualElement>("RotationIcon");
        var ScaleIcon = topPanelUIRoot.Q<VisualElement>("ScaleIcon");
        
        var SelectLabel = topPanelUIRoot.Q<VisualElement>("SelectLabel");
        var MoveLabel = topPanelUIRoot.Q<VisualElement>("MoveLabel");
        var RotationLabel = topPanelUIRoot.Q<VisualElement>("RotationLabel");
        var ScaleLabel = topPanelUIRoot.Q<VisualElement>("ScaleLabel");
        
        SelectIcon.RemoveFromClassList(iconSelectStyle);
        MoveIcon.RemoveFromClassList(iconSelectStyle);
        RotationIcon.RemoveFromClassList(iconSelectStyle);
        ScaleIcon.RemoveFromClassList(iconSelectStyle);
        
        SelectLabel.RemoveFromClassList(labelSelectStyle);
        MoveLabel.RemoveFromClassList(labelSelectStyle);
        RotationLabel.RemoveFromClassList(labelSelectStyle);
        ScaleLabel.RemoveFromClassList(labelSelectStyle);
    }

    private void SelectedButtonStyle(string name)
    {
        var icon = topPanelUIRoot.Q<VisualElement>(name + "Icon");
        var label = topPanelUIRoot.Q<VisualElement>(name + "Label");

        if (icon == null)
        {
            Debug.Log("icon is null");
        }
        icon.AddToClassList(iconSelectStyle);
        label.AddToClassList(labelSelectStyle);
    }

    private void DeSelectedButtonStyle(string name)
    {
        var icon = topPanelUIRoot.Q<VisualElement>(name + "Icon");
        var label = topPanelUIRoot.Q<VisualElement>(name + "Label");
        icon.RemoveFromClassList(iconSelectStyle);
        label.RemoveFromClassList(labelSelectStyle);
    }
}
