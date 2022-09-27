using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

// 배치할 수 있는 에셋의 종류를 나타내는 UI
public class AssetCategoryUI : MonoBehaviour
{
    public UIDocument mainUIDocument;
    private Button _titleButton;
    private Button _wallButton;
    private Button _propButton;

    public UnityAction<UIState> OnUISateChanged;

    private void OnEnable()
    {
        var mainUIRoot = mainUIDocument.GetComponent<UIDocument>().rootVisualElement;
        _titleButton = mainUIRoot.Q<Button>("FloorBtn");
        _wallButton = mainUIRoot.Q<Button>("WallBtn");
        _propButton = mainUIRoot.Q<Button>("PropBtn");
        
        
        _titleButton.clicked += TitleButtonPressed;
        _wallButton.clicked += WallButtonPressed;
        _propButton.clicked += PropButtonPressed;
    }

    private void OnDisable()
    {
        _titleButton.clicked -= TitleButtonPressed;
        _wallButton.clicked -= WallButtonPressed;
        _propButton.clicked -= PropButtonPressed;
    }

    private void TitleButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.TileUI);
    }

    private void WallButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.WallUI);
    }

    private void PropButtonPressed()
    {
        OnUISateChanged?.Invoke(UIState.PropUI);
    }
}
