using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unit이 선택되었음을 표시하는 Cursor
/// </summary>
public class UnitCursor : MonoBehaviour
{
    [SerializeField] private LayerMask _layerUnit;

    private Camera _mainCamera;

    public InputReader _InputReader;

    public List<Obstacle> PlaceableDatas = new List<Obstacle>();

    public WorldObjectsManager WorldObjectsManager;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _InputReader.OnMouseCursorClickAction += OnMouseCursorClicked;
    }

    private void OnDisable()
    {
        _InputReader.OnMouseCursorClickAction -= OnMouseCursorClicked;
    }

    private void OnMouseCursorClicked(Vector2 value)
    {
        RaycastHit hit;
        Ray ray = _mainCamera.ScreenPointToRay(value);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerUnit))
        {
            Debug.Log("hit name : " + hit.transform.name);
            Obstacle placeableData = hit.transform.GetComponent<Obstacle>();
            PlaceableDatas.Add(placeableData);
            WorldObjectsManager.worldObjects.Add(placeableData);
        }
        else
        {
            // Debug.Log("not hit!!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _InputReader.ModeSwitch(InputMode.UnitCursor);
    }
}
