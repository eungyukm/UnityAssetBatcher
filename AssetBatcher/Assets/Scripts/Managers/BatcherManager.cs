using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BatcherManager : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask playingFieldMask;
    public GameObject cardPrefab;
    // TODO : DeckData 추가
    public MeshRenderer forbiddenAreaRenderer;
    
    public UnityAction<BatcherObj, Vector3, Placeable.Faction> OnCardUsed;
    
    private BatcherObj[] _batcherobjs;

    private void Awake()
    {
        _batcherobjs = new BatcherObj[3];
    }

    private void CardDragged(int cardId, Vector2 dragAmount)
    {
        _batcherobjs[cardId].transform.Translate(dragAmount);

        RaycastHit hit;
        // Ray ray = 
    }
}
