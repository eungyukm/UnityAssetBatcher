using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeGizmos;

[RequireComponent(typeof(Camera))]
public class GizmoTransform : MonoBehaviour
{
	public TransformType transformType = TransformType.Move;
    
    public KeyCode translationSnapping = KeyCode.LeftControl;
    
    public float movementSnap = .25f;
    
    public float handleLength = .25f;
    public float handleWidth = .003f;

    public float minSelectedDistanceCheck = .01f;
    public float moveSpeedMultiplier = 1f;
    
    public bool forceUpdatePivotPointOnChange = true;

    public LayerMask selectionMask = Physics.DefaultRaycastLayers;
    public Camera myCamera {get; private set;}

    public bool isTransforming {get; private set;}
		
    public Axis translatingAxisPlane {get {return planeAxis;}}
    public bool hasTranslatingAxisPlane {get {return translatingAxisPlane != Axis.None && translatingAxisPlane != Axis.Any;}}

    public Vector3 pivotPoint {get; private set;}

    // 선택한 오브젝트
    public Transform mainTargetRoot;

    AxisInfo axisInfo;
    Axis nearAxis = Axis.None;
    Axis planeAxis = Axis.None;
    TransformType translatingType;

    AxisVectors handleLines = new AxisVectors();

    WaitForEndOfFrame waitForEndOFFrame = new WaitForEndOfFrame();
    Coroutine forceUpdatePivotCoroutine;
    
    public GameObject Arrow;
    public GridSystem GridSystem;
    public InputReader InputReader;
    
    private bool isMoseLeftClicked = false;
    private bool isKeyboardCtrlPressed = false;

    void Awake()
    {
        myCamera = GetComponent<Camera>();
    }

    void OnEnable()
    {
        forceUpdatePivotCoroutine = StartCoroutine(ForceUpdatePivotPointAtEndOfFrame());

        InputReader.OnMouseLeftClickDownAction += MouseLeftDown;
        InputReader.OnMouseLeftClickUPAction += MouseLeftUP;
        InputReader.OnMouseCursorClickAction += GetTarget;

        InputReader.OnCtrlDownAction += KeyboardCtrlDown;
        InputReader.OnCtrlUPAction += KeyboardCtrlUP;
    }

    void OnDisable()
    {
	    StopCoroutine(forceUpdatePivotCoroutine);
	    
	    InputReader.OnMouseLeftClickDownAction -= MouseLeftDown;
	    InputReader.OnMouseLeftClickUPAction -= MouseLeftUP;
	    InputReader.OnMouseCursorClickAction -= GetTarget;
    }
    
    void Update()
    {
        SetNearAxis();
    }

    void LateUpdate()
    {
        if (mainTargetRoot == null)
        {
            return;
        }
			
        SetAxisInfo();
			
        SetHandleLines();
        SetSnapSize();
    }

    public float GetHandleLength(bool multiplyDistanceMultiplier = true)
    {
	    float length = handleLength;

	    if (multiplyDistanceMultiplier)
	    {
		    length *= GetDistanceMultiplier();
	    }

	    return length;
    }

    #region TransformSelected
    
    private void MouseLeftDown()
    {
	    isMoseLeftClicked = true;
	    TransformSelected();
    }

    private void MouseLeftUP()
    {
	    isMoseLeftClicked = false;
    }

    private void KeyboardCtrlDown()
    {
	    isKeyboardCtrlPressed = true;
    }

    private void KeyboardCtrlUP()
    {
	    isKeyboardCtrlPressed = false;
    }

    private void TransformSelected()
    {
	    if (mainTargetRoot != null)
	    {
		    if (nearAxis != Axis.None)
		    {
			    StartCoroutine(TransformSelected(translatingType));
		    }
	    }
    }
    
    IEnumerator TransformSelected(TransformType transType)
    {
	    isTransforming = true;

	    Vector3 originalPivot = pivotPoint;

	    Vector3 otherAxis1, otherAxis2;
	    Vector3 axis = GetNearAxisDirection(out otherAxis1, out otherAxis2);
	    Vector3 planeNormal = hasTranslatingAxisPlane ? axis : (transform.position - originalPivot).normalized;
	    Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
	    Vector3 previousMousePosition = Vector3.zero;

	    Vector3 currentSnapMovementAmount = Vector3.zero;
	    float currentSnapRotationAmount = 0;
	    float currentSnapScaleAmount = 0;

	    while (isMoseLeftClicked)
	    {
		    Ray mouseRay = myCamera.ScreenPointToRay(InputReader.MousePos);
		    Vector3 mousePosition =
			    Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalPivot, planeNormal);
		    bool isSnapping = isKeyboardCtrlPressed;
		    Debug.Log("isSnap : " + isSnapping);

		    if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
		    {
			    if (transType == TransformType.Move)
			    {
				    currentSnapMovementAmount = CurrentSnapMovementAmount(mousePosition, previousMousePosition,
					    projectedAxis, axis, isSnapping, currentSnapMovementAmount, otherAxis1, otherAxis2);
			    }
		    }

		    previousMousePosition = mousePosition;

		    yield return null;
	    }

	    isTransforming = false;
	    SetTranslatingAxis(transformType, Axis.None);

	    SetPivotPoint();
    }

    private Vector3 CurrentSnapMovementAmount(Vector3 mousePosition, Vector3 previousMousePosition,
	    Vector3 projectedAxis, Vector3 axis, bool isSnapping, Vector3 currentSnapMovementAmount, Vector3 otherAxis1,
	    Vector3 otherAxis2)
    {
	    Vector3 movement = Vector3.zero;

	    if (hasTranslatingAxisPlane)
	    {
		    movement = mousePosition - previousMousePosition;
	    }
	    else
	    {
		    float moveAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) *
		                       moveSpeedMultiplier;
		    movement = axis * moveAmount;
	    }

	    if (isSnapping && movementSnap > 0)
	    {
		    currentSnapMovementAmount += movement;
		    movement = Vector3.zero;

		    if (hasTranslatingAxisPlane)
		    {
			    float amountInAxis1 = ExtVector3.MagnitudeInDirection(currentSnapMovementAmount, otherAxis1);
			    float amountInAxis2 = ExtVector3.MagnitudeInDirection(currentSnapMovementAmount, otherAxis2);

			    float remainder1;
			    float snapAmount1 = CalculateSnapAmount(movementSnap, amountInAxis1, out remainder1);
			    float remainder2;
			    float snapAmount2 = CalculateSnapAmount(movementSnap, amountInAxis2, out remainder2);

			    if (snapAmount1 != 0)
			    {
				    Vector3 snapMove = (otherAxis1 * snapAmount1);
				    movement += snapMove;
				    currentSnapMovementAmount -= snapMove;
			    }

			    if (snapAmount2 != 0)
			    {
				    Vector3 snapMove = (otherAxis2 * snapAmount2);
				    movement += snapMove;
				    currentSnapMovementAmount -= snapMove;
			    }
		    }
		    else
		    {
			    float remainder;
			    float snapAmount =
				    CalculateSnapAmount(movementSnap, currentSnapMovementAmount.magnitude, out remainder);

			    if (snapAmount != 0)
			    {
				    movement = currentSnapMovementAmount.normalized * snapAmount;
				    currentSnapMovementAmount = currentSnapMovementAmount.normalized * remainder;
			    }
		    }
	    }

	    Transform target = mainTargetRoot;

	    target.Translate(movement, Space.World);

	    SetPivotPointOffset(movement);
	    return currentSnapMovementAmount;
    }

    #endregion

    float CalculateSnapAmount(float snapValue, float currentAmount, out float remainder)
    {
	    remainder = 0;
	    if (snapValue <= 0) return currentAmount;

	    float currentAmountAbs = Mathf.Abs(currentAmount);
	    if (currentAmountAbs > snapValue)
	    {
		    remainder = currentAmountAbs % snapValue;
		    return snapValue * (Mathf.Sign(currentAmount) * Mathf.Floor(currentAmountAbs / snapValue));
	    }

	    return 0;
    }

    Vector3 GetNearAxisDirection(out Vector3 otherAxis1, out Vector3 otherAxis2)
    {
	    otherAxis1 = otherAxis2 = Vector3.zero;

	    if (nearAxis != Axis.None)
	    {
		    if (nearAxis == Axis.X)
		    {
			    otherAxis1 = axisInfo.yDirection;
			    otherAxis2 = axisInfo.zDirection;
			    return axisInfo.xDirection;
		    }

		    if (nearAxis == Axis.Y)
		    {
			    otherAxis1 = axisInfo.xDirection;
			    otherAxis2 = axisInfo.zDirection;
			    return axisInfo.yDirection;
		    }

		    if (nearAxis == Axis.Z)
		    {
			    otherAxis1 = axisInfo.xDirection;
			    otherAxis2 = axisInfo.yDirection;
			    return axisInfo.zDirection;
		    }

		    if (nearAxis == Axis.Any)
		    {
			    return Vector3.one;
		    }
	    }

	    return Vector3.zero;
    }

    #region SetTarget
    /// <summary>
    /// 선택한 Target을 추가 삭제 함
    /// </summary>
    void GetTarget(Vector2 pos)
    {
	    if (nearAxis == Axis.None)
	    {
		    RaycastHit hitInfo;
		    if (Physics.Raycast(myCamera.ScreenPointToRay(InputReader.MousePos), out hitInfo, Mathf.Infinity,
			        selectionMask))
		    {
			    Transform target = hitInfo.transform;
			    ClearAndAddTarget(target);
		    }
	    }
    }

    void ClearAndAddTarget(Transform target)
    {
	    mainTargetRoot = target;
	    SetPivotPoint();
    }
    #endregion

    #region SetPivot
    /// <summary>
    /// Pivot을 설정합니다. 
    /// </summary>
    public void SetPivotPoint()
    {
	    if (mainTargetRoot != null)
	    {
		    pivotPoint = mainTargetRoot.position;
		    Arrow.transform.position = pivotPoint;
	    }
    }

    void SetPivotPointOffset(Vector3 offset)
    {
	    pivotPoint += offset;
	    Arrow.transform.position = pivotPoint;
    }
    #endregion

    IEnumerator ForceUpdatePivotPointAtEndOfFrame()
    {
	    while (enabled)
	    {
		    if (forceUpdatePivotPointOnChange)
		    {
			    if (mainTargetRoot != null && !isTransforming)
			    {
				    SetPivotPoint();
			    }
		    }
		    yield return waitForEndOFFrame;
	    }
    }

    public void SetTranslatingAxis(TransformType type, Axis axis, Axis planeAxis = Axis.None)
    {
	    translatingType = type;
	    nearAxis = axis;
	    this.planeAxis = planeAxis;
    }

    void SetNearAxis()
    {
	    if (isTransforming) return;

	    SetTranslatingAxis(transformType, Axis.None);

	    if (mainTargetRoot == null) return;

	    float distanceMultiplier = GetDistanceMultiplier();
	    float handleMinSelectedDistanceCheck = (minSelectedDistanceCheck + handleWidth) * distanceMultiplier;

	    if (nearAxis == Axis.None)
	    {
		    planeAxis = nearAxis;
		    HandleNearestLines(transformType, handleLines, handleMinSelectedDistanceCheck);
	    }
    }
    
    # region Line Check
    void HandleNearestLines(TransformType type, AxisVectors axisVectors, float minSelectedDistanceCheck)
    {
	    float xClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.x);
	    float yClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.y);
	    float zClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.z);

	    HandleNearest(type, xClosestDistance, yClosestDistance, zClosestDistance, minSelectedDistanceCheck);
    }

    void HandleNearest(TransformType type, float xClosestDistance, float yClosestDistance, float zClosestDistance,
	    float minSelectedDistanceCheck)
    {
	    if (xClosestDistance <= minSelectedDistanceCheck && xClosestDistance <= yClosestDistance &&
	        xClosestDistance <= zClosestDistance)
	    {
		    SetTranslatingAxis(type, Axis.X);
	    }
	    else if (yClosestDistance <= minSelectedDistanceCheck && yClosestDistance <= xClosestDistance &&
	             yClosestDistance <= zClosestDistance)
	    {
		    SetTranslatingAxis(type, Axis.Y);
	    }
	    else if (zClosestDistance <= minSelectedDistanceCheck && zClosestDistance <= xClosestDistance &&
	             zClosestDistance <= yClosestDistance)
	    {
		    SetTranslatingAxis(type, Axis.Z);
	    }
    }

    float ClosestDistanceFromMouseToLines(List<Vector3> lines)
    {
	    Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

	    float closestDistance = float.MaxValue;
	    for (int i = 0; i + 1 < lines.Count; i++)
	    {
		    IntersectPoints points =
			    Geometry.ClosestPointsOnSegmentToLine(lines[i], lines[i + 1], mouseRay.origin, mouseRay.direction);
		    float distance = Vector3.Distance(points.first, points.second);
		    if (distance < closestDistance)
		    {
			    closestDistance = distance;
		    }
	    }

	    return closestDistance;
    }
    #endregion
    
    # region Set Info
    void SetAxisInfo()
    {
	    if (mainTargetRoot != null)
	    {
		    axisInfo.Set(mainTargetRoot, pivotPoint);
	    }
    }

    private void SetSnapSize()
    {
	    movementSnap = GridSystem.cellSize;
    }
    #endregion
    public float GetDistanceMultiplier()
    {
	    if (mainTargetRoot == null)
	    {
		    return 0f;
	    }

	    if (myCamera.orthographic)
	    {
		    return Mathf.Max(.01f, myCamera.orthographicSize * 2f);
	    }

	    return Mathf.Max(.01f,
		    Mathf.Abs(ExtVector3.MagnitudeInDirection(pivotPoint - transform.position, myCamera.transform.forward)));
    }

    #region SetHandle
    void SetHandleLines()
    {
	    handleLines.Clear();

	    if (transformType == TransformType.Move)
	    {
		    float lineWidth = handleWidth * GetDistanceMultiplier();

		    float xLineLength = 0;
		    float yLineLength = 0;
		    float zLineLength = 0;
		    if (transformType == TransformType.Move)
		    {
			    xLineLength = yLineLength = zLineLength = GetHandleLength();
		    }

		    AddQuads(pivotPoint, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, xLineLength, lineWidth,
			    handleLines.x);
		    AddQuads(pivotPoint, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, yLineLength, lineWidth,
			    handleLines.y);
		    AddQuads(pivotPoint, axisInfo.zDirection, axisInfo.xDirection, axisInfo.yDirection, zLineLength, lineWidth,
			    handleLines.z);
	    }
    }

    void AddQuads(Vector3 axisStart, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2,
	    float length, float width, List<Vector3> resultsBuffer)
    {
	    Vector3 axisEnd = axisStart + (axisDirection * length);
	    AddQuads(axisStart, axisEnd, axisOtherDirection1, axisOtherDirection2, width, resultsBuffer);
    }

    void AddQuads(Vector3 axisStart, Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2,
	    float width, List<Vector3> resultsBuffer)
    {
	    Square baseRectangle = GetBaseSquare(axisStart, axisOtherDirection1, axisOtherDirection2, width);
	    Square baseRectangleEnd = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, width);

	    resultsBuffer.Add(baseRectangle.bottomLeft);
	    resultsBuffer.Add(baseRectangle.topLeft);
	    resultsBuffer.Add(baseRectangle.topRight);
	    resultsBuffer.Add(baseRectangle.bottomRight);

	    resultsBuffer.Add(baseRectangleEnd.bottomLeft);
	    resultsBuffer.Add(baseRectangleEnd.topLeft);
	    resultsBuffer.Add(baseRectangleEnd.topRight);
	    resultsBuffer.Add(baseRectangleEnd.bottomRight);

	    for (int i = 0; i < 4; i++)
	    {
		    resultsBuffer.Add(baseRectangle[i]);
		    resultsBuffer.Add(baseRectangleEnd[i]);
		    resultsBuffer.Add(baseRectangleEnd[i + 1]);
		    resultsBuffer.Add(baseRectangle[i + 1]);
	    }
    }

    Square GetBaseSquare(Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size)
    {
	    Square square;
	    Vector3 offsetUp = ((axisOtherDirection1 * size) + (axisOtherDirection2 * size));
	    Vector3 offsetDown = ((axisOtherDirection1 * size) - (axisOtherDirection2 * size));
	    square.bottomLeft = axisEnd + offsetDown;
	    square.topLeft = axisEnd + offsetUp;
	    square.bottomRight = axisEnd - offsetUp;
	    square.topRight = axisEnd - offsetDown;
	    return square;
    }
    #endregion

    public enum TransformType
    {
	    Move,
	    Rotate,
	    Scale
    }

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
