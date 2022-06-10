using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Unit이 선택되었음을 표시하는 Cursor
/// </summary>
public class UnitCursor : MonoBehaviour
{
    [SerializeField] private LayerMask _layerUnit;
    [SerializeField] private LayerMask _layerOutLine;

    private Camera _mainCamera;

    public InputReader _InputReader;

    public List<Obstacle> PlaceableDatas = new List<Obstacle>();

    public WorldObjectsManager WorldObjectsManager;

    [FormerlySerializedAs("Mode")] public GameTransformMode transformMode;

    private GameObject hitObj;

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

    // Start is called before the first frame update
    void Start()
    {
        _InputReader.ModeSwitch(InputMode.UnitCursor);
    }

    public void SwitchMode(GameTransformMode gameTransformMode)
    {
        transformMode = gameTransformMode;
        switch (gameTransformMode)
        {
            case GameTransformMode.SelectMode:
                break;
            case GameTransformMode.MoveMode:
                break;
            case GameTransformMode.ScaleMode:
                break;
            case GameTransformMode.RotationMode:
                break;
        }
    }
    
    private void OnMouseCursorClicked(Vector2 value)
    {
        RaycastHit hit;
        Ray ray = _mainCamera.ScreenPointToRay(value);

        LayerMask layer = (_layerUnit | _layerOutLine);
        // Debug.Log("layer : " + layer);
        // Debug.DrawRay(_mainCamera.transform.position, ray.direction * 1000, Color.red);
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
        {
            Obstacle placeableData = hit.transform.GetComponent<Obstacle>();
            PlaceableDatas.Add(placeableData);
            // WorldObjectsManager.worldObjects.Add(placeableData);

            SelectGameObject(hit);
        }
        else
        {
            Debug.Log("not hit!!");
            DeSelectGameObject();
        }
    }
    // Cusor가 GameTransfrom Mode에 따른 분기 처리
    // private void 

    // Select Button Press할 경우, RayCast 쏴서 오브젝트 선택
    private void SelectGameObject(RaycastHit hit)
    {
        hitObj = hit.transform.gameObject;
        string hitObjectName = hitObj.name;
        Debug.Log("hit object name : " + hitObjectName);
        
        // 오브젝트에 선택 OutLine 표현
        hitObj.layer = LayerMask.NameToLayer("OutLine");
        
        // 선택 했을 경우, 정보 표현
    }

    private void DeSelectGameObject()
    {
        hitObj.layer = LayerMask.NameToLayer("Unit");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            gameObject.AddComponent<SelectOutline>();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Destroy(gameObject.GetComponent<SelectOutline>());
        }
    }
}

public enum GameTransformMode
{
    SelectMode,
    MoveMode,
    ScaleMode,
    RotationMode
}