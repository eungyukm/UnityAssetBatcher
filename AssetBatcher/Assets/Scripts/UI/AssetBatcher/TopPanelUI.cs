using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    private Button _saveButton;

    public GridSystem gridSystem;
    public MouseCursor mouseCursor;

    private bool _onSnap = false;

    private const string iconSelectStyle = "Icon_Selected";
    private const string labelSelectStyle = "Label_Selected";

    public InputReader InputReader;

    public UnityAction<UIState> OnSaveAction;

    private LocationExit _locationExit;

    public UnityEngine.UI.Button RotateButton;

    private void Awake()
    {
        _locationExit = GetComponent<LocationExit>();
    }

    private void OnEnable()
    {
        topPanelUIRoot = topPanelDocument.GetComponent<UIDocument>().rootVisualElement;

        _selectButtom = topPanelUIRoot.Q<Button>("SelectBtn");
        _moveButton = topPanelUIRoot.Q<Button>("MoveBtn");
        _rotationButton = topPanelUIRoot.Q<Button>("RotationBtn");
        _scaleButton = topPanelUIRoot.Q<Button>("ScaleBtn");
        _alignmentButton = topPanelUIRoot.Q<Button>("AlignmentBtn");
        _playButton = topPanelUIRoot.Q<Button>("PlayBtn");
        _saveButton = topPanelUIRoot.Q<Button>("SaveBtn");

        #region Action 처리
        InputReader.OnWKeyAction += SelectButtonPressed;
        InputReader.OnGrapPresseAction += MoveButtonPressed;
        InputReader.OnRotationAction += RotationButtonPressed;
        InputReader.OnScaleAction += ScaleButtonPressed;
        
        _selectButtom.clicked += SelectButtonPressed;
        _moveButton.clicked += MoveButtonPressed;
        _rotationButton.clicked += RotationButtonPressed;
        _scaleButton.clicked += ScaleButtonPressed;
        _alignmentButton.clicked += AlignmentButtonPressed;
        _playButton.clicked += PlayButtonPressed;
        _saveButton.clicked += MapSaveButtonPressed;
        
        // Cursor의 모드를 전달받는 코드
        mouseCursor.changeCursorMode += ReciveCursorMode;
        #endregion
    }

    private void OnDisable()
    {
        #region Action 처리
        InputReader.OnWKeyAction -= SelectButtonPressed;
        InputReader.OnGrapPresseAction -= MoveButtonPressed;
        InputReader.OnRotationAction -= RotationButtonPressed;
        InputReader.OnScaleAction -= ScaleButtonPressed;
        
        _selectButtom.clicked -= SelectButtonPressed;
        _moveButton.clicked -= MoveButtonPressed;
        _rotationButton.clicked -= RotationButtonPressed;
        _scaleButton.clicked -= ScaleButtonPressed;
        _alignmentButton.clicked -= AlignmentButtonPressed;
        _playButton.clicked -= PlayButtonPressed;
        _saveButton.clicked -= MapSaveButtonPressed;
        
        mouseCursor.changeCursorMode += ReciveCursorMode;
        #endregion
    }

    // 클래스 내부에서 Transform을 변경하는 코드
    // Transform Mode를 변경하는 코드
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
    
    // 커서 모드의 모드가 변경 시 커서의 모드를 전달받는 함수 
    private void ReciveCursorMode(MouseCursor.GameTransformMode gameTransformMode)
    {
        mouseCursor.transformMode = gameTransformMode;
        SwitchTransformMode();
    }

    #region UI Controll
    
    private void SelectButtonPressed()
    {
        mouseCursor.SwitchCursorMode(MouseCursor.GameTransformMode.SelectMode);
        SwitchTransformMode();
    }

    private void MoveButtonPressed()
    {
        mouseCursor.SwitchCursorMode(MouseCursor.GameTransformMode.MoveMode);
        SwitchTransformMode();
    }

    private void RotationButtonPressed()
    {
        mouseCursor.SwitchCursorMode(MouseCursor.GameTransformMode.RotationMode);
        SwitchTransformMode();
    }

    private void ScaleButtonPressed()
    {
        mouseCursor.SwitchCursorMode(MouseCursor.GameTransformMode.ScaleMode);
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
        _locationExit.LoadNextScene();
    }

    private void MapSaveButtonPressed()
    {
        OnSaveAction?.Invoke(UIState.MapSaveUI);
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

    public void PressedRotationButton()
    {
        mouseCursor.RotateSelectedObject();

        Debug.Log(mouseCursor.GetSelectedObjectPos().x);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(mouseCursor.GetSelectedObjectPos());
        Vector2 vector2Pos = new Vector2(screenPos.x, screenPos.y);
        RotateButton.GetComponent<RectTransform>().anchoredPosition = vector2Pos;
    }
    #endregion

}
