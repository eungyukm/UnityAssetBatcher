using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class TopPanelUI : MonoBehaviour
{
    [SerializeField]private UIDocument topPanelDocument;

    private VisualElement _topPanelUIRoot;
    private Button _selectButtom;
    private Button _moveButton;
    private Button _rotationButton;
    private Button _scaleButton;
    private Button _alignmentButton;
    private Button _playButton;
    private Button _saveButton;

    public GridSystem gridSystem;
    public MouseCursor mouseCursor;

    private bool _onSnap = true;

    private const string IconSelectStyle = "Icon_Selected";
    private const string LabelSelectStyle = "Label_Selected";

    public InputReader inputReader;

    public UnityAction<UIState> OnSaveAction;

    private LocationExit _locationExit;

    public UnityEngine.UI.Button RotateButton;

    private void Awake()
    {
        _locationExit = GetComponent<LocationExit>();
    }

    private void OnEnable()
    {
        _topPanelUIRoot = topPanelDocument.GetComponent<UIDocument>().rootVisualElement;

        _selectButtom = _topPanelUIRoot.Q<Button>("SelectBtn");
        _moveButton = _topPanelUIRoot.Q<Button>("MoveBtn");
        _rotationButton = _topPanelUIRoot.Q<Button>("RotationBtn");
        _scaleButton = _topPanelUIRoot.Q<Button>("ScaleBtn");
        _alignmentButton = _topPanelUIRoot.Q<Button>("AlignmentBtn");
        _playButton = _topPanelUIRoot.Q<Button>("PlayBtn");
        _saveButton = _topPanelUIRoot.Q<Button>("SaveBtn");

        #region Action 처리
        inputReader.OnWKeyAction += SelectButtonPressed;
        inputReader.OnGrapPresseAction += MoveButtonPressed;
        inputReader.OnRotationAction += RotationButtonPressed;
        inputReader.OnScaleAction += ScaleButtonPressed;
        
        _selectButtom.clicked += SelectButtonPressed;
        _moveButton.clicked += MoveButtonPressed;
        _rotationButton.clicked += RotationButtonPressed;
        _scaleButton.clicked += ScaleButtonPressed;
        _alignmentButton.clicked += AlignmentButtonPressed;
        _playButton.clicked += PlayButtonPressed;
        _saveButton.clicked += MapSaveButtonPressed;
        
        // Cursor의 모드를 전달받는 코드
        mouseCursor.changeCursorMode += ReciveCursorMode;
        
        // Cursor의 Target Object를 전달받는 코드
        mouseCursor.OnChangeTarget += ReciveSelectedTarget;

        #endregion
    }

    private void OnDisable()
    {
        #region Action 처리
        inputReader.OnWKeyAction -= SelectButtonPressed;
        inputReader.OnGrapPresseAction -= MoveButtonPressed;
        inputReader.OnRotationAction -= RotationButtonPressed;
        inputReader.OnScaleAction -= ScaleButtonPressed;
        
        _selectButtom.clicked -= SelectButtonPressed;
        _moveButton.clicked -= MoveButtonPressed;
        _rotationButton.clicked -= RotationButtonPressed;
        _scaleButton.clicked -= ScaleButtonPressed;
        _alignmentButton.clicked -= AlignmentButtonPressed;
        _playButton.clicked -= PlayButtonPressed;
        _saveButton.clicked -= MapSaveButtonPressed;
        
        mouseCursor.changeCursorMode -= ReciveCursorMode;
        #endregion
    }

    private void Start()
    {
        Init();
    }
    
    // TopPanel의 기본 상태를 초기화 합니다. 
    private void Init()
    {
        InitSnap();
    }
    
    // Snap을 Default인 True로 설정 합니다.
    private void InitSnap()
    {
        _onSnap = true;
        SelectedButtonStyle("Snap");
        gridSystem.SnapSwitch(_onSnap);
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
        var SelectIcon = _topPanelUIRoot.Q<VisualElement>("SelectIcon");
        var MoveIcon = _topPanelUIRoot.Q<VisualElement>("MoveIcon");
        var RotationIcon = _topPanelUIRoot.Q<VisualElement>("RotationIcon");
        var ScaleIcon = _topPanelUIRoot.Q<VisualElement>("ScaleIcon");
        
        var SelectLabel = _topPanelUIRoot.Q<VisualElement>("SelectLabel");
        var MoveLabel = _topPanelUIRoot.Q<VisualElement>("MoveLabel");
        var RotationLabel = _topPanelUIRoot.Q<VisualElement>("RotationLabel");
        var ScaleLabel = _topPanelUIRoot.Q<VisualElement>("ScaleLabel");
        
        SelectIcon.RemoveFromClassList(IconSelectStyle);
        MoveIcon.RemoveFromClassList(IconSelectStyle);
        RotationIcon.RemoveFromClassList(IconSelectStyle);
        ScaleIcon.RemoveFromClassList(IconSelectStyle);
        
        SelectLabel.RemoveFromClassList(LabelSelectStyle);
        MoveLabel.RemoveFromClassList(LabelSelectStyle);
        RotationLabel.RemoveFromClassList(LabelSelectStyle);
        ScaleLabel.RemoveFromClassList(LabelSelectStyle);
    }

    private void SelectedButtonStyle(string name)
    {
        var icon = _topPanelUIRoot.Q<VisualElement>(name + "Icon");
        var label = _topPanelUIRoot.Q<VisualElement>(name + "Label");

        if (icon == null)
        {
            Debug.Log("icon is null");
        }
        icon.AddToClassList(IconSelectStyle);
        label.AddToClassList(LabelSelectStyle);
    }

    private void DeSelectedButtonStyle(string name)
    {
        var icon = _topPanelUIRoot.Q<VisualElement>(name + "Icon");
        var label = _topPanelUIRoot.Q<VisualElement>(name + "Label");
        icon.RemoveFromClassList(IconSelectStyle);
        label.RemoveFromClassList(LabelSelectStyle);
    }
    
    // Rotation Mode에서 오브젝트 선택 되었을 경우, 위치 설정
    public void SetPositionRotationButton()
    {
        mouseCursor.RotateSelectedObject();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(mouseCursor.GetSelectedObjectPos());
        Vector2 vector2Pos = new Vector2(screenPos.x, screenPos.y);
        RotateButton.GetComponent<RectTransform>().anchoredPosition = vector2Pos;
    }
    #endregion

    private void ReciveSelectedTarget()
    {
        
    }
}
