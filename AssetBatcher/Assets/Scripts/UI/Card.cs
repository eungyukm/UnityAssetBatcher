using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 카드의 Drag And Drop의 이벤트를 가지고 있음
/// 카드의 마우스 위치를 반환하는 InputReader를 가지고 있음
/// </summary>
public class Card : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public UnityAction<int, Vector2> OnDragAction;
    public UnityAction<int> OnTapDownAction, OnTapReleaseAction;

    [HideInInspector] public int cardId;
    [HideInInspector] public CardData cardData;

    public Image portraitImage; //Inspector-set reference
    private CanvasGroup canvasGroup;

    public InputReader inputReader;
    
    public UnityAction OnLeftMouseClickAction = delegate {  };

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
    }

    private void OnDisable()
    {
        inputReader.OnMouseLeftClickedAction -= OnLeftMouseClicked;
    }

    //called by CardManager, it feeds CardData so this card can display the placeable's portrait
    public void InitialiseWithData(CardData cData)
    {
        cardData = cData;
        portraitImage.sprite = cardData.cardImage;
    }

    public void OnPointerDown(PointerEventData pointerEvent)
    {
        if(OnTapDownAction != null)
            OnTapDownAction(cardId);
    }

    public void OnDrag(PointerEventData pointerEvent)
    {
        Debug.Log("[Card] Draged!!");
        if(OnDragAction != null)
            OnDragAction(cardId, pointerEvent.delta);
    }

    public void OnPointerUp(PointerEventData pointerEvent)
    {
        if(OnTapReleaseAction != null)
            OnTapReleaseAction(cardId);
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

    public void InputToggle(bool isActive)
    {
        if (isActive)
        {
            inputReader.ModeSwitch(InputMode.Deploy);
        }
        else
        {
            inputReader.ModeSwitch(InputMode.UnitCursor);
        }
    }
}
