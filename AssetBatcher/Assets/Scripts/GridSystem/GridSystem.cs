using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.Serialization;

public class GridSystem : MonoBehaviour
{
    private UnitGrid _unitGrid;

    public Camera _camera;

    public GameObject GridGO;

    public int widthGrid = 5;
    public int heightGrid = 5;
    public float cellSize = 1f;

    public bool GridActive = false;

    public GridSystemType _gridSystemType = GridSystemType.None;

    private void Awake()
    {
        SnapSwitch(false);
    }
    
    void Start()
    {
        _camera = Camera.main;
        
        CreateGrid(widthGrid, heightGrid);
    }

    public void CreateGrid(int width, int height)
    {
        _unitGrid = new UnitGrid(width, height, cellSize);
    }

    public Vector3 SnapCoordinateToGrid(Vector3 WorldPosition)
    {
        Vector3 snapPos = Vector3.zero;
        switch (_gridSystemType)
        {
            case GridSystemType.Tile:
                snapPos = CalculateTileSnap(WorldPosition);
                break;
            case GridSystemType.Wall:
                snapPos = CalculateWallSnap(WorldPosition);
                _unitGrid.EdgeStateSwitch(EdgeState.Horizontal);
                break;
        }
        
        return snapPos;
    }

    private Vector3 CalculateWallSnap(Vector3 worldPositon)
    {
        Vector3Int cellPost = _unitGrid.WorldToEdgeCell(worldPositon);
        return _unitGrid.GetCellEdgeCenterWorld(cellPost);
    }
    
    private Vector3 CalculateTileSnap(Vector3 worldPositon)
    {
        Vector3Int cellPost = _unitGrid.WorldToCenterCell(worldPositon);
        return _unitGrid.GetCellCenterWorld(cellPost);
    }
    
    /// <summary>
    /// Snap효과를 끄고 키고, 하는 역할을 함
    /// </summary>
    /// <param name="onGrid"></param>
    public void SnapSwitch(bool onGrid)
    {
        GridGO.SetActive(onGrid);
        GridActive = onGrid;
        if (onGrid)
        {
            
        }
        else
        {
            
        }
    }

    public void SwitchGridType(GridSystemType gridType)
    {
        _gridSystemType = gridType;
        switch (gridType)
        {
            case GridSystemType.None:
                
                break;
            case GridSystemType.Tile:
                
                break;
            case GridSystemType.Wall:
                
                break;
        }
    }
}

public enum GridSystemType
{
    None,
    Tile,
    Wall
}
