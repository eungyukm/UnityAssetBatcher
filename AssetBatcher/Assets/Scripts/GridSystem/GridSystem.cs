using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class GridSystem : MonoBehaviour
{
    private UnitGrid _unitGrid;

    public Camera _camera;

    public InputReader _InputReader;

    public GameObject GridGO;

    private void Awake()
    {
        SnapSwitch(false);
    }

    private void OnEnable()
    {
        _InputReader.OnMouseLeftClickedAction += LeftMouseClick;
    }

    private void LeftMouseClick(Vector2 mousePos)
    {
        Debug.Log("mouse Pos Y : " + mousePos.x + "mouse Pox Y" + mousePos.y);
        Vector3 worldPositon = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _camera.transform.position.y));
        Debug.Log("World Pos X : " + worldPositon.x + "World Pos Y : " + worldPositon.y + "World Pos Z : " + worldPositon.z);
        _unitGrid.SetValue(worldPositon, 1);
        
        // Test Code
        // Vector3 snapPos = SnapCoordinateToGrid(worldPositon);
        // GameObject go = new GameObject();
        // Instantiate(go, snapPos, Quaternion.identity);
    }

    void Start()
    {
        _camera = Camera.main;
        
        // TODO : Grid Size 조절
        _unitGrid = new UnitGrid(5, 5, 2f);
    }

    public Vector3 SnapCoordinateToGrid(Vector3 WorldPosition)
    {
        Vector3Int cellPost = _unitGrid.WorldToCell(WorldPosition);
        Vector3 snapPos = _unitGrid.GetCellCenterWorld(cellPost);
        return snapPos;
    }
    
    /// <summary>
    /// Snap효과를 끄고 키고, 하는 역할을 함
    /// </summary>
    /// <param name="onGrid"></param>
    public void SnapSwitch(bool onGrid)
    {
        GridGO.SetActive(onGrid);
        if (onGrid)
        {
            
        }
        else
        {
            
        }
    }
}
