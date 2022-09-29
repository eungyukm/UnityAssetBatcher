using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class FurnitureUI : MonoBehaviour
{
    [SerializeField] private UIDocument furnitureUIDocument;
    [SerializeField] private CardManager cardManager;
    
    private Button _previousButton;
    private VisualElement _propUIRoot;
    
    private Button _furnitureTypeButton01;
    private Button _furnitureTypeButton02;
    private Button _furnitureTypeButton03;
    private Button _furnitureTypeButton04;
    private Button _furnitureTypeButton05;
    
    public UnityAction<UIState> OnClosePanel;

    private void OnEnable()
    {
        _propUIRoot = furnitureUIDocument.rootVisualElement;
        _previousButton = _propUIRoot.Q<Button>("PreviousBtn");
        
        _previousButton.clicked += ClosePanel;
        
        _furnitureTypeButton01 = _propUIRoot.Q<Button>("Furniture01");
        _furnitureTypeButton02 = _propUIRoot.Q<Button>("Furniture02");
        _furnitureTypeButton03 = _propUIRoot.Q<Button>("Furniture03");
        _furnitureTypeButton04 = _propUIRoot.Q<Button>("Furniture04");
        _furnitureTypeButton05 = _propUIRoot.Q<Button>("Furniture05");
        
        _previousButton.clicked += ClosePanel;
        
        _furnitureTypeButton01.clicked += FurnitureTypeButton01Pressed;
        _furnitureTypeButton02.clicked += FurnitureTypeButton02Pressed;
        _furnitureTypeButton03.clicked += FurnitureTypeButton03Pressed;
        _furnitureTypeButton04.clicked += FurnitureTypeButton04Pressed;
        _furnitureTypeButton05.clicked += FurnitureTypeButton05Pressed;
    }

    private void OnDisable()
    {
        _previousButton.clicked -= ClosePanel;

        _furnitureTypeButton01.clicked -= FurnitureTypeButton01Pressed;
        _furnitureTypeButton02.clicked -= FurnitureTypeButton02Pressed;
        _furnitureTypeButton03.clicked -= FurnitureTypeButton03Pressed;
        _furnitureTypeButton04.clicked -= FurnitureTypeButton04Pressed;
        _furnitureTypeButton05.clicked -= FurnitureTypeButton05Pressed;
    }
    
    private void ClosePanel()
    {
        UIState goToPanel = UIState.MainUI;
        OnClosePanel?.Invoke(goToPanel);
    }
    
    private void FurnitureTypeButton01Pressed()
    {
        StartCoroutine(CardChangeRoutine(0));
    }
    
    private void FurnitureTypeButton02Pressed()
    {
        StartCoroutine(CardChangeRoutine(1));
    }
    
    private void FurnitureTypeButton03Pressed()
    {
        StartCoroutine(CardChangeRoutine(2));
    }
    
    private void FurnitureTypeButton04Pressed()
    {
        StartCoroutine(CardChangeRoutine(3));
    }
    
    private void FurnitureTypeButton05Pressed()
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
