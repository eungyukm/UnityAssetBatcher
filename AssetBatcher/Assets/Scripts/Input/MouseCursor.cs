using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// Unit이 선택되었음을 표시하는 Cursor
/// Cursor의 Mode를 결정합니다.
/// </summary>
public class MouseCursor : MonoBehaviour
{
    public InputReader _InputReader;

    public GameTransformMode transformMode = GameTransformMode.None;

    public GameObject GizmoArrow;
    public GameObject GizmoRotation;

    protected Axis rotateAxis;
    protected Axis nearAxis = Axis.None;
    
    // 선택한 오브젝트
    protected Transform mainTargetRoot;
    
    public Camera myCamera {get; set;}
    public LayerMask selectionMask = Physics.DefaultRaycastLayers;
    
    public Vector3 pivotPoint {get; set;}

    public UnityAction<GameTransformMode> changeCursorMode;
    
    public TransformType transformType = TransformType.Move;

    protected virtual void Awake()
    {
        myCamera = Camera.main;
    }

    #region Action 연결
    public virtual void OnEnable()
    {
        if (_InputReader == null)
        {
            Debug.Log("Input Reader is null!!");
        }

        _InputReader.OnMouseLeftClickDownAction += MouseLeftDown;
        _InputReader.OnMouseLeftClickUPAction += MouseLeftUP;
        
        // 마우스 왼쪽 클릭
        _InputReader.OnMouseLeftClickedAction += GetTarget;

        _InputReader.OnGrapPresseAction += OnGrapMode;
        _InputReader.OnScaleAction += OnScaleMode;
        _InputReader.OnRotationAction += OnRotationMode;

        _InputReader.OnXKeyAction += OnRotationXMode;
        _InputReader.OnYKeyAction += OnRotationYMode;
        _InputReader.OnZKeyAction += OnRotationZMode;
        
    }
    public virtual void OnDisable()
    {
        _InputReader.OnMouseLeftClickDownAction -= MouseLeftDown;
        _InputReader.OnMouseLeftClickUPAction -= MouseLeftUP;
        
        // 마우스 왼쪽 클릭
        _InputReader.OnMouseLeftClickedAction -= GetTarget;
        
        _InputReader.OnGrapPresseAction -= OnGrapMode;
        _InputReader.OnScaleAction -= OnScaleMode;
        _InputReader.OnRotationAction -= OnRotationMode;

        _InputReader.OnXKeyAction -= OnRotationXMode;
        _InputReader.OnYKeyAction -= OnRotationYMode;
        _InputReader.OnZKeyAction -= OnRotationZMode;
    }
    #endregion

    
    void Start()
    {
        _InputReader.ModeSwitch(InputMode.UnitCursor);

        InitGizmo();
    }
    
    // Gizm를 초기화 합니다.
    private void InitGizmo()
    {
        GizmoArrow.SetActive(false);
        GizmoRotation.SetActive(false);
    }
    
    // 마우스의 커서 모드를 변경하는 로직
    public void SwitchCursorMode(GameTransformMode gameTransformMode)
    {
        ClearAndAddTarget(null);
        transformMode = gameTransformMode;
        switch (gameTransformMode)
        {
            case GameTransformMode.SelectMode:
                break;
            case GameTransformMode.MoveMode:
                transformType = TransformType.Move;
                break;
            case GameTransformMode.ScaleMode:
                transformType = TransformType.Scale;
                break;
            case GameTransformMode.RotationMode:
                transformType = TransformType.Rotate;
                break;
        }

        SendCursorModeToUI(transformMode);
    }
    
    // 마우스의 커서 모드가 변경되었을 경우, UI에 변경된 커서 모드를 전달하는 로직
    private void SendCursorModeToUI(GameTransformMode gameTransformMode)
    {
        changeCursorMode?.Invoke(gameTransformMode);
    }
    
    // TODO : EGK 아래 함수는 블렌더와 같은 회전을 고려하고 만든 함수 입니다.
    private bool EndRotationMode()
    {
        if (transformMode == GameTransformMode.RotationMode)
        {
            transformMode = GameTransformMode.SelectMode;
            return true;
        }

        return false;
    }

    #region OutLineSetup

    private void ClearOutline()
    {
        if (mainTargetRoot != null)
        {
            DeSelectOutline();
        }
    }
    protected void SetOutline()
    {
        if (transformMode == GameTransformMode.SelectMode)
        {
            SelectOutline();
        }
        else
        {
            DeSelectOutline();
        }
    }
    
    private void DeSelectOutline()
    {
        if (mainTargetRoot != null)
        {
            mainTargetRoot.gameObject.layer = LayerMask.NameToLayer("Unit");
        }
    }

    private void SelectOutline()
    {
        // 오브젝트에 선택 OutLine 표현
        if (mainTargetRoot != null)
        {
            mainTargetRoot.gameObject.layer = LayerMask.NameToLayer("OutLine");
        }
    }
    #endregion


    protected virtual void OnGrapMode()
    {
        SwitchCursorMode(GameTransformMode.MoveMode);
    }

    protected virtual void OnScaleMode()
    {
        SwitchCursorMode(GameTransformMode.ScaleMode);
    }

    protected virtual void OnRotationMode()
    {
        SwitchCursorMode(GameTransformMode.RotationMode);
        rotateAxis = Axis.X;
    }

    private void OnRotationXMode()
    {
        if (transformMode != GameTransformMode.RotationMode)
        {
            return;
        }
        rotateAxis = Axis.X;
    }

    private void OnRotationYMode()
    {
        if (transformMode != GameTransformMode.RotationMode)
        {
            return;
        }
        rotateAxis = Axis.Y;
    }

    private void OnRotationZMode()
    {
        if (transformMode != GameTransformMode.RotationMode)
        {
            return;
        }
        rotateAxis = Axis.Z;
    }

    protected virtual void MouseLeftDown()
    {
        
    }

    protected virtual void MouseLeftUP()
    {
        
    }

    protected virtual void GetTarget(Vector2 pos)
    {
        if (nearAxis == Axis.None)
        {
            RaycastHit hitInfo;
            if (myCamera == null)
            {
                Debug.LogError("myCamera is null!");
            }
            if (Physics.Raycast(myCamera.ScreenPointToRay(_InputReader.MousePos), out hitInfo, Mathf.Infinity,
                    selectionMask))
            {
                Transform target = hitInfo.transform;
                ClearAndAddTarget(target);
            }
        }
    }
    
    void ClearAndAddTarget(Transform target)
    {
        ClearOutline();
        mainTargetRoot = target;
        SetPivotPoint();
        SetOutline();
    }
    
    /// <summary>
    /// Pivot을 설정합니다. 
    /// </summary>
    public void SetPivotPoint()
    {
        if (mainTargetRoot != null)
        {
            pivotPoint = mainTargetRoot.position;
            SetArrowGizmo();
        }
        else
        {
            GizmoArrow.SetActive(false);
            GizmoRotation.SetActive(false);
        }
    }
    
    public void SetArrowGizmo()
    {
        if (transformMode == GameTransformMode.MoveMode || transformMode == GameTransformMode.ScaleMode)
        {
            GizmoArrow.SetActive(true);
            GizmoArrow.transform.position = pivotPoint;
        }

        if (transformMode == GameTransformMode.RotationMode)
        {
            GizmoRotation.SetActive(true);
        }
    }

    public Vector3 GetSelectedObjectPos()
    {
        if (mainTargetRoot == null)
        {
            return Vector3.zero;
        }
        else
        {
            return mainTargetRoot.position;
        }
    }
    
    // 
    public void RotateSelectedObject()
    {
        if (mainTargetRoot == null)
        {
            Debug.Log("hit object is null");
        }
        else
        {
            mainTargetRoot.transform.rotation *= Quaternion.Euler(Vector3.up * 30f);
        }
    }

    [Serializable]
    public enum GameTransformMode
    {
        None,
        SelectMode,
        MoveMode,
        ScaleMode,
        RotationMode
    }
    
    [Serializable]
    public enum TransformType
    {
        Move,
        Rotate,
        Scale,
    }
    
    [Serializable]
    public enum Axis
    {
        None,
        X,
        Y,
        Z,
        Any
    }

    public struct AxisInfo
    {
        public Vector3 pivot;
        public Vector3 xDirection;
        public Vector3 yDirection;
        public Vector3 zDirection;

        public void Set(Transform target, Vector3 pivot)
        {
            xDirection = Vector3.right;
            yDirection = Vector3.up;
            zDirection = Vector3.forward;

            this.pivot = pivot;
        }
    }

    public class AxisVectors
    {
        public List<Vector3> x = new List<Vector3>();
        public List<Vector3> y = new List<Vector3>();
        public List<Vector3> z = new List<Vector3>();
        public List<Vector3> all = new List<Vector3>();

        public void Clear()
        {
            x.Clear();
            y.Clear();
            z.Clear();
            all.Clear();
        }
    }
}