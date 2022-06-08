using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

/// <summary>
/// Unit들을 배치하기 위한 Grid
/// </summary>
public class UnitGrid
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] gridArray;
    
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs {
        public int x;
        public int z;
    }
    
    public UnitGrid(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new int[width, height];
        // Debug.Log(width + " : " + height);
        
        TextMesh[,] debugTextArray = new TextMesh[width, height];
        
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {
                debugTextArray[x, z] = UtilsClass.CreateWorldText(gridArray[x, z].ToString(), null, GetWorldCenterPosition(x, z), Mathf.FloorToInt(cellSize * 5), Color.white, TextAnchor.MiddleCenter);
                // Debug.Log(x + " : " + z);

                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z +1), Color.blue, 100f);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.blue, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width,height), Color.blue, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width,height), Color.blue, 100f);
        
        OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) => {
            debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z].ToString();
        };
    }

    private Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x,0, z) * cellSize;
    }

    public void SetValue(int x, int z, int value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            // Debug.Log("Value : " + value);
            gridArray[x, z] += value;
            if (OnGridValueChanged != null)
            {
                OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, z = z });             
            }
        }
    }

    private void GetXZ(Vector3 wolrdPositon, out int x, out int z)
    {
        Debug.Log("wolrd x vlaue : " + wolrdPositon.x + " z: " + wolrdPositon.z + "y : " + wolrdPositon.y);
        x = Mathf.FloorToInt(wolrdPositon.x / cellSize);
        z = Mathf.FloorToInt(wolrdPositon.z / cellSize);
        // Debug.Log("world Pos X : " + x + "Pos Z" + z);
    }

    public void SetValue(Vector3 worldPositon, int value)
    {
        int x, z;
        GetXZ(worldPositon, out x, out z);
        Debug.Log("Get X : " + x + " Get Z : " + z);
        SetValue(x,z, value);
    }

    /// <summary>
    /// World Postion을 Cell Positon으로 변경하여 반환 함
    /// TODO : 높이 값에 따른 반환 추가
    /// </summary>
    /// <param name="worldPositon"></param>
    /// <returns></returns>
    public Vector3Int WorldToCell(Vector3 worldPositon)
    {
        int x, z;
        GetXZ(worldPositon, out x, out z);
        Vector3Int cellVector = new Vector3Int(x, 0, z);
        return cellVector;
    }
    
    /// <summary>
    /// cell positon을 입력 받으면, cell position의 Center 값 반환
    /// TODO : 높이 계산
    /// </summary>
    /// <param name="cellPosition"></param>
    /// <returns></returns>
    public Vector3 GetCellCenterWorld(Vector3Int cellPosition)
    {
        Vector3 cellCenter = GetWorldCenterPosition(cellPosition.x,cellPosition.z);
        return cellCenter;
    }
    
    /// <summary>
    /// X와 Z값을 주면 Cell의 중심을 찾는 메서드
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private Vector3 GetWorldCenterPosition(int x, int z)
    {
        return new Vector3( x,0,  z) * cellSize + new Vector3(cellSize, 0, cellSize) * 0.5f;
    }
}
