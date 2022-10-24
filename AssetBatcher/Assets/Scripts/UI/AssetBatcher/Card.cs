using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 카드의 Drag And Drop의 이벤트를 가지고 있음
/// 카드의 마우스 위치를 반환하는 InputReader를 가지고 있음
/// </summary>
public class Card : MonoBehaviour
{
    [HideInInspector] public int cardId;
    [HideInInspector] public CardData cardData;

    public Image portraitImage; //Inspector-set reference
    private CanvasGroup canvasGroup;

    public InputReader inputReader;
    
    public UnityAction OnLeftMouseClickAction = delegate {  };
    public UnityAction OnRightMouseClickAction = delegate {  };

    public Vector2 MousePos = new Vector2(0, 0);

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        MousePos = inputReader.GetMousePos();
    }

    private void OnEnable()
    {
        inputReader.OnMouseLeftClickedAction += OnLeftMouseClicked;
        inputReader.OnMouseRightClickAction += OnRightMouseClicked;
    }

    private void OnDisable()
    {
        inputReader.OnMouseLeftClickedAction -= OnLeftMouseClicked;
        inputReader.OnMouseRightClickAction -= OnRightMouseClicked;
    }

    //called by CardManager, it feeds CardData so this card can display the placeable's portrait
    public void InitialiseWithData(CardData cData)
    {
        cardData = cData;
        portraitImage.sprite = cardData.cardImage;
    }

    public void OnLeftMouseClicked(Vector2 mousePos)
    {
        Debug.Log("On Mouse");
        OnLeftMouseClickAction?.Invoke();
    }

    public void ChangeActiveState(bool isActive)
    {
        canvasGroup.alpha = (isActive) ? .01f : 1f;
    }

    public void OnRightMouseClicked()
    {
        OnRightMouseClickAction?.Invoke();
    }
}
