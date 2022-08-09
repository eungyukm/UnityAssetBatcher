using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BatcherObj : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public UnityAction<int, Vector2> OnDragAction;
    public UnityAction<int> OnTapDownAction, OnTapReleaseAction;
    
    public Image portraitImage;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag!!");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointerUP!!");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown!!");
    }
    
    public void ChangeActiveState(bool isActive)
    {
        canvasGroup.alpha = (isActive) ? .05f : 1f;
    }
}
