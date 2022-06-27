using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeGizmos;

[RequireComponent(typeof(Camera))]
public class GizmoTransform : MonoBehaviour
{
	public TransformType transformType = TransformType.Move;

	public float movementSnap = .25f;
	public float scaleSnap = 0.25f;
    
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

        if (mainTargetRoot == null)
        {
	        return;
        }
        
        Vector2 mo = new Vector2(InputReader.MousePos.x, InputReader.MousePos.y);

        float theta = CalculateRotationAmount(Axis.X, mo, mainTargetRoot);

        mainTargetRoot.transform.localRotation = Quaternion.Euler(Vector3.right * theta);
    }

    private float CalculateRotationAmount(Axis axis, Vector2 mousePos, Transform target)
    {
	    Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
	    Debug.DrawRay(mouseRay.origin, mouseRay.direction * 1000f, Color.red);
	    float mouseRayLength = mouseRay.direction.magnitude;
	    float targetVectorLength = 0f;

	    switch (axis)
	    {
		    case Axis.X:
			    // targetVectorLength = target.up.magnitude;
			    targetVectorLength = Vector3.up.magnitude;
			    break;
		    case Axis.Y:
			    targetVectorLength = target.right.magnitude;
			    break;
		    case Axis.Z:
			    targetVectorLength = target.forward.magnitude;
			    break;
	    }
	    // float dot = Vector3.Dot(mainTargetRoot.up, -mouseRay.direction);
	    float dot = Vector3.Dot(Vector3.up, -mouseRay.direction);
	    float radian = Mathf.Acos(dot / mouseRayLength / targetVectorLength);
	    float theta = radian * Mathf.Rad2Deg;
	    
	    Debug.Log("up Vector : " + target.up);
	    Debug.Log("theta : " + theta);
	    return theta;
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
	    
	    Vector3 axis = GetNearAxisDirection();
	    Vector3 planeNormal = hasTranslatingAxisPlane ? axis : (transform.position - originalPivot).normalized;
	    Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
	    Vector3 previousMousePosition = Vector3.zero;

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

    float CalculateSnapAmount(float snapValue, float currentAmount, out float remainder)
    {
	    Debug.Log("currentAmount : " + currentAmount);
	    remainder = 0;
	    if (snapValue <= 0)
	    {
		    return currentAmount;
	    }

	    float currentAmountAbs = Mathf.Abs(currentAmount);
	    if (currentAmountAbs > snapValue)
	    {
		    remainder = currentAmountAbs % snapValue;
		    Debug.Log("1 : " + snapValue);
		    Debug.Log("2 : " + (Mathf.Sign(currentAmount)));
		    Debug.Log("3 : " + Mathf.Floor(currentAmountAbs / snapValue));
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
	    Debug.Log("localScale : " + localScale);
	    Debug.Log("currentAmount : " + currentAmount);
	    if (currentAmountAbs > localScale + snapValue)
	    {
		    Debug.Log("1 : " + currentAmountAbs);
		    Debug.Log("2 : " + (Mathf.Sign(currentAmount)));
		    Debug.Log("3 : " + Mathf.Floor(currentAmountAbs / snapValue));
		    return snapValue;
	    }

	    return 0;
    }
    
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
	    else if (transformType == TransformType.Rotate)
	    {
		    
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
