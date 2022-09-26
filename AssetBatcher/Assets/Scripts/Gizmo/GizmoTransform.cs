using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeGizmos;

[RequireComponent(typeof(Camera))]
public class GizmoTransform : MouseCursor
{
	// Snape 시 이동 범위
	public float movementSnap = .25f;
	public float scaleSnap = 0.25f;
    
	// 생성되는 Handle의 길이 및 너비
    public float handleLength = .1f;
    public float handleWidth = .003f;
    private float _handleScaleMultipler = 0.5f;
    
    
    public float minSelectedDistanceCheck = .01f;
    public float moveSpeedMultiplier = 1f;
    
    public bool forceUpdatePivotPointOnChange = true;

    public bool isTransforming {get; private set;}
		
    public Axis translatingAxisPlane {get {return planeAxis;}}
    public bool hasTranslatingAxisPlane {get {return translatingAxisPlane != Axis.None && translatingAxisPlane != Axis.Any;}}

    private AxisInfo axisInfo;
    private Axis planeAxis = Axis.None;
    private TransformType translatingType;

    AxisVectors handleLines = new AxisVectors();

    WaitForEndOfFrame waitForEndOFFrame = new WaitForEndOfFrame();
    Coroutine forceUpdatePivotCoroutine;
    
    public GridSystem GridSystem;

    private bool isMoseLeftClicked = false;
    private bool isKeyboardCtrlPressed = false;

    private Vector3 originRotation;

    protected override void Awake()
    {
	    base.Awake();
    }

    public override void OnEnable()
    {
	    base.OnEnable();
        forceUpdatePivotCoroutine = StartCoroutine(ForceUpdatePivotPointAtEndOfFrame());
        
        _InputReader.OnCtrlDownAction += KeyboardCtrlDown;
        _InputReader.OnCtrlUPAction += KeyboardCtrlUP;
    }

    public override void OnDisable()
    {
	    base.OnDisable();
	    StopCoroutine(forceUpdatePivotCoroutine);
    }
    
    
    void Update()
    {
        SetNearAxis();
        
        // TODO : EGK 회전
        //SetRotate();
    }

    #region Object Rotation
    private void SetRotate()
    {
	    if (transformMode != GameTransformMode.RotationMode)
	    {
		    return;
	    }

	    if (mainTargetRoot == null)
	    {
		    return;
	    }
	    Vector3 originMousePos = Camera.main.WorldToScreenPoint(mainTargetRoot.transform.position);
	    Vector2 mousePos = new Vector2(_InputReader.MousePos.x, _InputReader.MousePos.y);

	    float theta = CalculateRotationAmount(originMousePos, mousePos, mainTargetRoot);
	    
	    Vector3 rotation = Vector3.zero;

	    switch (rotateAxis)
	    {
		    case Axis.X:
			    rotation = originRotation + Vector3.right * theta;
			    break;
		    case Axis.Y:
			    rotation = originRotation + Vector3.up * theta;
			    break;
		    case Axis.Z:
			    rotation = originRotation + Vector3.forward * theta;
			    break;
	    }
	    mainTargetRoot.transform.localRotation = Quaternion.Euler(rotation);
    }

    private float CalculateRotationAmount(Vector2 originMousePos, Vector2 currentMousePos, Transform target)
    {
	    float theta = 0f;

	    float rotationAngle = Mathf.Atan2(currentMousePos.y - originMousePos.y, currentMousePos.x - originMousePos.x) * Mathf.Rad2Deg;

	    // 시계 방향 회전하기 위해 -값으로 반환
	    theta = -rotationAngle;
	    return theta;
    }
    #endregion


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

	    SetCursor(length);
	    
	    return length;
    }

    private void SetCursor(float length)
    {
	    GizmoArrow.transform.localScale = new Vector3(length, length, length);
    }

    #region TransformSelected
    
    protected override void MouseLeftDown()
    {
	    isMoseLeftClicked = true;
	    TransformSelected();
    }

    protected override void MouseLeftUP()
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
	    
	    Vector3 axis = GetNearAxisDirection();
	    Vector3 planeNormal = hasTranslatingAxisPlane ? axis : (transform.position - originalPivot).normalized;
	    Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
	    Vector3 previousMousePosition = Vector3.zero;

	    while (isMoseLeftClicked)
	    {
		    Ray mouseRay = myCamera.ScreenPointToRay(_InputReader.MousePos);
		    Vector3 mousePosition =
			    Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalPivot, planeNormal);
		    bool isSnapping = isKeyboardCtrlPressed;

		    if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
		    {
			    if (transType == TransformType.Move)
			    {
				    CurrentSnapMovementAmount(mousePosition, previousMousePosition,
					    projectedAxis, axis, isSnapping);
			    }
			    else if (transType == TransformType.Rotate)
			    {
				    
			    }
			    else if(transType == TransformType.Scale)
			    {
				    CurrentSnapScaleAmount(mousePosition, previousMousePosition,
					    projectedAxis, axis, isSnapping);				    
			    }
		    }

		    previousMousePosition = mousePosition;

		    yield return null;
	    }

	    isTransforming = false;
	    SetTranslatingAxis(transformType, Axis.None);

	    SetPivotPoint();
    }
    /// <summary>
    /// 움직임 값을 연산합니다.
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <param name="previousMousePosition"></param>
    /// <param name="projectedAxis"></param>
    /// <param name="axis"></param>
    /// <param name="isSnapping"></param>
    private void CurrentSnapMovementAmount(Vector3 mousePosition, Vector3 previousMousePosition,
	    Vector3 projectedAxis, Vector3 axis, bool isSnapping)
    {
	    Vector3 movement = Vector3.zero;
	    Vector3 currentSnapMovementAmount = Vector3.zero;
	    
	    float moveAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) *
	                       moveSpeedMultiplier;
	    movement = axis * moveAmount;

	    if (isSnapping && movementSnap > 0)
	    {
		    movement = Vector3.zero;
		    
		    moveAmount = ExtVector3.MagnitudeInDirection(mousePosition - mainTargetRoot.transform.position, projectedAxis) *
		                 moveSpeedMultiplier;
		    currentSnapMovementAmount += axis * moveAmount;

		    float remainder;
		    float snapAmount = CalculateSnapAmount(movementSnap, currentSnapMovementAmount.magnitude, out remainder);

		    if (snapAmount != 0)
		    {
			    movement = currentSnapMovementAmount.normalized * snapAmount;
		    }
	    }

	    Transform target = mainTargetRoot;

	    target.Translate(movement, Space.World);

	    SetPivotPointOffset(movement);
    }

    /// <summary>
    /// Scale 조절하는 코드
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <param name="previousMousePosition"></param>
    /// <param name="projectedAxis"></param>
    /// <param name="axis"></param>
    /// <param name="isSnapping"></param>
    private void CurrentSnapScaleAmount(Vector3 mousePosition, Vector3 previousMousePosition,
	    Vector3 projectedAxis, Vector3 axis, bool isSnapping)
    {
	    Transform target = mainTargetRoot;
	    
	    float scale = 0f;
	    float currentScaleAmount = 0f;

	    float scaleAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) *
	                        moveSpeedMultiplier;

	    if (isSnapping && scaleSnap > 0)
	    {
		    currentScaleAmount = ExtVector3.MagnitudeInDirection(mousePosition - mainTargetRoot.transform.position , projectedAxis) *
		                         moveSpeedMultiplier;;
		    scaleAmount = 0f;
		    
		    float localScale = (axis.x * target.transform.localScale.x) + 
		                       (axis.y * target.transform.localScale.y) +
		                       (axis.z * target.transform.localScale.z);
		    
		    float remainder;
		    float snapAmount =
			    CalculateScaleSnapAmount(scaleSnap, localScale, currentScaleAmount);

		    if (snapAmount != 0)
		    {
			    scaleAmount = snapAmount;
		    }
		    
		    target.transform.localScale += axis * scaleAmount;
	    }
	    else
	    {
		    target.transform.localScale += axis * scaleAmount;
	    }
	    
    }
    #endregion

    #region CacluateAmount
    float CalculateSnapAmount(float snapValue, float currentAmount, out float remainder)
    {
	    remainder = 0;
	    if (snapValue <= 0)
	    {
		    return currentAmount;
	    }

	    float currentAmountAbs = Mathf.Abs(currentAmount);
	    if (currentAmountAbs > snapValue)
	    {
		    remainder = currentAmountAbs % snapValue;

		    return snapValue * (Mathf.Sign(currentAmount) * Mathf.Floor(currentAmountAbs / snapValue));
	    }

	    return 0;
    }
    
    float CalculateScaleSnapAmount(float snapValue, float localScale, float currentAmount)
    {
	    if (snapValue <= 0)
	    {
		    return currentAmount;
	    }

	    float currentAmountAbs = Mathf.Abs(currentAmount);

	    if (currentAmountAbs > localScale + snapValue)
	    {
		    Debug.Log("1 : " + currentAmountAbs);
		    Debug.Log("2 : " + (Mathf.Sign(currentAmount)));
		    Debug.Log("3 : " + Mathf.Floor(currentAmountAbs / snapValue));
		    return snapValue;
	    }

	    return 0;
    }
    #endregion

    
    /// <summary>
    /// 근접한 Axis를 Retrun 합니다.
    /// </summary>
    /// <returns></returns>
    Vector3 GetNearAxisDirection()
    {
	    if (nearAxis != Axis.None)
	    {
		    if (nearAxis == Axis.X)
		    {
			    return axisInfo.xDirection;
		    }

		    if (nearAxis == Axis.Y)
		    {
			    return axisInfo.yDirection;
		    }

		    if (nearAxis == Axis.Z)
		    {
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
    protected override void GetTarget(Vector2 pos)
    {
	    base.GetTarget(pos);

    }
    #endregion

    #region SetPivot
    private void SetPivotPointOffset(Vector3 offset)
    {
	    pivotPoint += offset;
	    SetArrowGizmo();
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
	    if (transformMode == GameTransformMode.None || transformMode == GameTransformMode.SelectMode)
	    {
		    return;
	    }

	    if (isTransforming)
	    {
		    return;
	    }

	    SetTranslatingAxis(transformType, Axis.None);

	    if (mainTargetRoot == null)
	    {
		    return;
	    }

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
    
    // 카메라와의 거리에 따라 길이를 반환해주는 로직
    public float GetDistanceMultiplier()
    {
	    if (mainTargetRoot == null)
	    {
		    return 0f;
	    }

	    return Mathf.Max(.01f,
		    Mathf.Abs(ExtVector3.MagnitudeInDirection(pivotPoint - transform.position, myCamera.transform.forward))) * 0.5f;
    }

    #region SetHandle
    void SetHandleLines()
    {
	    handleLines.Clear();

	    if (transformType == TransformType.Move || transformType == TransformType.Scale)
	    {
		    float lineWidth = handleWidth * GetDistanceMultiplier();

		    float xLineLength = 0;
		    float yLineLength = 0;
		    float zLineLength = 0;
		    xLineLength = yLineLength = zLineLength = GetHandleLength();

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
    
    # region Move Rotation Scale Setup
    protected override void OnGrapMode()
    {
	    base.OnGrapMode();

	    transformType = TransformType.Move;
    }

    protected override void OnScaleMode()
    {
	    base.OnScaleMode();

	    transformType = TransformType.Scale;
    }

    protected override void OnRotationMode()
    {
	    base.OnRotationMode();

	    transformType = TransformType.Rotate;
    }
    # endregion
}
