using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CommandUndoRedo;
using UnityEngine.Rendering;

namespace RuntimeGizmos
{
	//To be safe, if you are changing any transforms hierarchy, such as parenting an object to something,
	//you should call ClearTargets before doing so just to be sure nothing unexpected happens... as well as call UndoRedoManager.Clear()
	//For example, if you select an object that has children, move the children elsewhere, deselect the original object, then try to add those old children to the selection, I think it wont work.

	[RequireComponent(typeof(Camera))]
	public class TransformGizmo : MonoBehaviour
	{
		public TransformSpace space = TransformSpace.Global;
		public TransformType transformType = TransformType.Move;
		public TransformPivot pivot = TransformPivot.Pivot;
		public CenterType centerType = CenterType.All;
		public ScaleType scaleType = ScaleType.FromPoint;

		//These are the same as the unity editor hotkeys
		public KeyCode SetMoveType = KeyCode.W;
		public KeyCode SetRotateType = KeyCode.E;
		public KeyCode SetScaleType = KeyCode.R;
		//public KeyCode SetRectToolType = KeyCode.T;
		public KeyCode SetAllTransformType = KeyCode.Y;
		public KeyCode SetSpaceToggle = KeyCode.X;
		public KeyCode SetPivotModeToggle = KeyCode.Z;
		public KeyCode SetCenterTypeToggle = KeyCode.C;
		public KeyCode SetScaleTypeToggle = KeyCode.S;
		public KeyCode translationSnapping = KeyCode.LeftControl;
		public KeyCode AddSelection = KeyCode.LeftShift;
		public KeyCode RemoveSelection = KeyCode.LeftControl;
		public KeyCode ActionKey = KeyCode.LeftShift; //Its set to shift instead of control so that while in the editor we dont accidentally undo editor changes =/

		/// <summary>
		/// 사용 안함
		/// </summary>
		public Color xColor = new Color(1, 0, 0, 0.8f);
		public Color yColor = new Color(0, 1, 0, 0.8f);
		public Color zColor = new Color(0, 0, 1, 0.8f);
		public Color allColor = new Color(.7f, .7f, .7f, 0.8f);
		public Color selectedColor = new Color(1, 1, 0, 0.8f);
		public Color hoverColor = new Color(1, .75f, 0, 0.8f);

		public Material xMaterial;
		public Material yMaterial;
		public Material zMaterial;
		public Material allMaterial;

		public float movementSnap = .25f;
		public float rotationSnap = 15f;
		public float scaleSnap = 1f;

		public float handleLength = .25f;
		public float handleWidth = .003f;
		public float planeSize = .035f;
		public float triangleSize = .03f;
		public float boxSize = .03f;
		public int circleDetail = 40;
		public float allMoveHandleLengthMultiplier = 1f;
		public float allRotateHandleLengthMultiplier = 1.4f;
		public float allScaleHandleLengthMultiplier = 1.6f;
		public float minSelectedDistanceCheck = .01f;
		public float moveSpeedMultiplier = 1f;
		public float scaleSpeedMultiplier = 1f;
		public float rotateSpeedMultiplier = 1f;
		public float allRotateSpeedMultiplier = 20f;

		public bool useFirstSelectedAsMain = true;
		
		public bool circularRotationMethod;

		public bool forceUpdatePivotPointOnChange = true;

		public LayerMask selectionMask = Physics.DefaultRaycastLayers;

		public Camera myCamera {get; private set;}

		public bool isTransforming {get; private set;}
		public float totalScaleAmount {get; private set;}
		public Quaternion totalRotationAmount {get; private set;}
		public Axis translatingAxis {get {return nearAxis;}}
		public Axis translatingAxisPlane {get {return planeAxis;}}
		public bool hasTranslatingAxisPlane {get {return translatingAxisPlane != Axis.None && translatingAxisPlane != Axis.Any;}}
		public TransformType transformingType {get {return translatingType;}}

		public Vector3 pivotPoint {get; private set;}
		Vector3 totalCenterPivotPoint;
		
		// 선택한 오브젝트
		public Transform mainTargetRoot {get {return (targetRootsOrdered.Count > 0) ? (useFirstSelectedAsMain) ? targetRootsOrdered[0] : targetRootsOrdered[targetRootsOrdered.Count - 1] : null;}}

		AxisInfo axisInfo;
		Axis nearAxis = Axis.None;
		Axis planeAxis = Axis.None;
		TransformType translatingType;

		AxisVectors handleLines = new AxisVectors();
		AxisVectors handlePlanes = new AxisVectors();
		AxisVectors handleTriangles = new AxisVectors();
		AxisVectors handleSquares = new AxisVectors();
		AxisVectors circlesLines = new AxisVectors();

		List<Transform> targetRootsOrdered = new List<Transform>();
		Dictionary<Transform, TargetInfo> targetRoots = new Dictionary<Transform, TargetInfo>();
		HashSet<Renderer> highlightedRenderers = new HashSet<Renderer>();
		HashSet<Transform> children = new HashSet<Transform>();

		List<Transform> childrenBuffer = new List<Transform>();
		List<Renderer> renderersBuffer = new List<Renderer>();
		List<Material> materialsBuffer = new List<Material>();

		WaitForEndOfFrame waitForEndOFFrame = new WaitForEndOfFrame();
		Coroutine forceUpdatePivotCoroutine;

		static Material lineMaterial;
		static Material outlineMaterial;

		public GameObject Arrow;

		void Awake()
		{
			myCamera = GetComponent<Camera>();
			SetMaterial();
		}

		void OnEnable()
		{
			forceUpdatePivotCoroutine = StartCoroutine(ForceUpdatePivotPointAtEndOfFrame());

			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
		}

		private void OnBeginContextRendering(ScriptableRenderContext arg1, List<Camera> arg2)
		{
			if (mainTargetRoot == null)
			{
				return;
			}
			DrawQuads(handleLines.z, zMaterial);
			DrawQuads(handleLines.x, xMaterial);
			DrawQuads(handleLines.y, yMaterial);

			DrawTriangles(handleTriangles.x, xMaterial);
			DrawTriangles(handleTriangles.y, yMaterial);
			DrawTriangles(handleTriangles.z, zMaterial);

			DrawQuads(handlePlanes.z, zMaterial);
			DrawQuads(handlePlanes.x, xMaterial);
			DrawQuads(handlePlanes.y, yMaterial);

			DrawQuads(handleSquares.x, xMaterial);
			DrawQuads(handleSquares.y, yMaterial);
			DrawQuads(handleSquares.z, zMaterial);
			DrawQuads(handleSquares.all, allMaterial);
			
			DrawQuads(circlesLines.all, allMaterial);
			DrawQuads(circlesLines.x, xMaterial);
			DrawQuads(circlesLines.y, yMaterial);
			DrawQuads(circlesLines.z, zMaterial);
		}

		void OnDisable()
		{
			ClearTargets(); //Just so things gets cleaned up, such as removing any materials we placed on objects.

			StopCoroutine(forceUpdatePivotCoroutine);
		}

		void OnDestroy()
		{
			ClearAllHighlightedRenderers();
		}

		void Update()
		{
			SetSpaceAndType();


			SetNearAxis();

			GetTarget();

			if (mainTargetRoot == null)
			{
				return;
			}
			TransformSelected();
		}

		void LateUpdate()
		{
			if (mainTargetRoot == null)
			{
				return;
			}
			
			SetAxisInfo();
			
			SetLines();
		}

		#region TransformSpace
		public TransformSpace GetProperTransformSpace()
		{
			return transformType == TransformType.Scale ? TransformSpace.Local : space;
		}

		public bool TransformTypeContains(TransformType type)
		{
			return TransformTypeContains(transformType, type);
		}
		public bool TranslatingTypeContains(TransformType type, bool checkIsTransforming = true)
		{
			TransformType transType = !checkIsTransforming || isTransforming ? translatingType : transformType;
			return TransformTypeContains(transType, type);
		}
		public bool TransformTypeContains(TransformType mainType, TransformType type)
		{
			return ExtTransformType.TransformTypeContains(mainType, type, GetProperTransformSpace());
		}
		#endregion

		
		public float GetHandleLength(TransformType type, Axis axis = Axis.None, bool multiplyDistanceMultiplier = true)
		{
			float length = handleLength;
			if(transformType == TransformType.All)
			{
				if(type == TransformType.Move) length *= allMoveHandleLengthMultiplier;
				if(type == TransformType.Rotate) length *= allRotateHandleLengthMultiplier;
				if(type == TransformType.Scale) length *= allScaleHandleLengthMultiplier;
			}

			if(multiplyDistanceMultiplier) length *= GetDistanceMultiplier();

			if(type == TransformType.Scale && isTransforming && (translatingAxis == axis || translatingAxis == Axis.Any)) length += totalScaleAmount;

			return length;
		}

		void SetSpaceAndType()
		{
			if(Input.GetKey(ActionKey)) return;

			if(Input.GetKeyDown(SetMoveType)) transformType = TransformType.Move;
			else if(Input.GetKeyDown(SetRotateType)) transformType = TransformType.Rotate;
			else if(Input.GetKeyDown(SetScaleType)) transformType = TransformType.Scale;
			else if(Input.GetKeyDown(SetAllTransformType)) transformType = TransformType.All;

			if(!isTransforming) translatingType = transformType;

			if(Input.GetKeyDown(SetPivotModeToggle))
			{
				if(pivot == TransformPivot.Pivot) pivot = TransformPivot.Center;
				else if(pivot == TransformPivot.Center) pivot = TransformPivot.Pivot;

				SetPivotPoint();
			}

			if(Input.GetKeyDown(SetCenterTypeToggle))
			{
				if(centerType == CenterType.All) centerType = CenterType.Solo;
				else if(centerType == CenterType.Solo) centerType = CenterType.All;

				SetPivotPoint();
			}

			if(Input.GetKeyDown(SetSpaceToggle))
			{
				if(space == TransformSpace.Global) space = TransformSpace.Local;
				else if(space == TransformSpace.Local) space = TransformSpace.Global;
			}

			if(Input.GetKeyDown(SetScaleTypeToggle))
			{
				if(scaleType == ScaleType.FromPoint) scaleType = ScaleType.FromPointOffset;
				else if(scaleType == ScaleType.FromPointOffset) scaleType = ScaleType.FromPoint;
			}

			if(transformType == TransformType.Scale)
			{
				if(pivot == TransformPivot.Pivot) scaleType = ScaleType.FromPoint;
			}
		}

		#region TransformSelected
		void TransformSelected()
		{
			if(mainTargetRoot != null)
			{
				if(nearAxis != Axis.None && Input.GetMouseButtonDown(0))
				{
					StartCoroutine(TransformSelected(translatingType));
				}
			}
		}
		
		IEnumerator TransformSelected(TransformType transType)
		{
			isTransforming = true;
			totalScaleAmount = 0;
			totalRotationAmount = Quaternion.identity;

			Vector3 originalPivot = pivotPoint;

			Vector3 otherAxis1, otherAxis2;
			Vector3 axis = GetNearAxisDirection(out otherAxis1, out otherAxis2);
			Vector3 planeNormal = hasTranslatingAxisPlane ? axis : (transform.position - originalPivot).normalized;
			Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
			Vector3 previousMousePosition = Vector3.zero;

			Vector3 currentSnapMovementAmount = Vector3.zero;
			float currentSnapRotationAmount = 0;
			float currentSnapScaleAmount = 0;

			while(!Input.GetMouseButtonUp(0))
			{
				Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
				Debug.Log("planeNormal : " + planeNormal);
				Vector3 mousePosition = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalPivot, planeNormal);
				bool isSnapping = Input.GetKey(translationSnapping);

				if(previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
				{
					if(transType == TransformType.Move)
					{
						currentSnapMovementAmount = CurrentSnapMovementAmount(mousePosition, previousMousePosition, projectedAxis, axis, isSnapping, currentSnapMovementAmount, otherAxis1, otherAxis2);
					}
					else if(transType == TransformType.Scale)
					{
						currentSnapScaleAmount = CurrentSnapScaleAmount(projectedAxis, mousePosition, previousMousePosition, isSnapping, currentSnapScaleAmount, axis, originalPivot);
					}
					else if(transType == TransformType.Rotate)
					{
						currentSnapRotationAmount = CurrentSnapRotationAmount(axis, previousMousePosition, originalPivot, mousePosition, planeNormal, isSnapping, currentSnapRotationAmount);
					}
				}

				previousMousePosition = mousePosition;

				yield return null;
			}

			totalRotationAmount = Quaternion.identity;
			totalScaleAmount = 0;
			isTransforming = false;
			SetTranslatingAxis(transformType, Axis.None);

			SetPivotPoint();
		}

		private float CurrentSnapRotationAmount(Vector3 axis, Vector3 previousMousePosition, Vector3 originalPivot,
			Vector3 mousePosition, Vector3 planeNormal, bool isSnapping, float currentSnapRotationAmount)
		{
			float rotateAmount = 0;
			Vector3 rotationAxis = axis;

			if (nearAxis == Axis.Any)
			{
				Vector3 rotation =
					transform.TransformDirection(new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0));
				Quaternion.Euler(rotation).ToAngleAxis(out rotateAmount, out rotationAxis);
				rotateAmount *= allRotateSpeedMultiplier;
			}
			else
			{
				if (circularRotationMethod)
				{
					float angle = Vector3.SignedAngle(previousMousePosition - originalPivot, mousePosition - originalPivot,
						axis);
					rotateAmount = angle * rotateSpeedMultiplier;
				}
				else
				{
					Vector3 projected = (nearAxis == Axis.Any || ExtVector3.IsParallel(axis, planeNormal))
						? planeNormal
						: Vector3.Cross(axis, planeNormal);
					rotateAmount =
						(ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projected) *
						 (rotateSpeedMultiplier * 100f)) / GetDistanceMultiplier();
				}
			}

			if (isSnapping && rotationSnap > 0)
			{
				currentSnapRotationAmount += rotateAmount;
				rotateAmount = 0;

				float remainder;
				float snapAmount = CalculateSnapAmount(rotationSnap, currentSnapRotationAmount, out remainder);

				if (snapAmount != 0)
				{
					rotateAmount = snapAmount;
					currentSnapRotationAmount = remainder;
				}
			}

			for (int i = 0; i < targetRootsOrdered.Count; i++)
			{
				Transform target = targetRootsOrdered[i];

				if (pivot == TransformPivot.Pivot)
				{
					target.Rotate(rotationAxis, rotateAmount, Space.World);
				}
				else if (pivot == TransformPivot.Center)
				{
					target.RotateAround(originalPivot, rotationAxis, rotateAmount);
				}
			}

			totalRotationAmount *= Quaternion.Euler(rotationAxis * rotateAmount);
			return currentSnapRotationAmount;
		}

		private float CurrentSnapScaleAmount(Vector3 projectedAxis, Vector3 mousePosition, Vector3 previousMousePosition,
			bool isSnapping, float currentSnapScaleAmount, Vector3 axis, Vector3 originalPivot)
		{
			Vector3 projected = (nearAxis == Axis.Any) ? transform.right : projectedAxis;
			float scaleAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projected) *
			                    scaleSpeedMultiplier;

			if (isSnapping && scaleSnap > 0)
			{
				currentSnapScaleAmount += scaleAmount;
				scaleAmount = 0;

				float remainder;
				float snapAmount = CalculateSnapAmount(scaleSnap, currentSnapScaleAmount, out remainder);

				if (snapAmount != 0)
				{
					scaleAmount = snapAmount;
					currentSnapScaleAmount = remainder;
				}
			}

			//WARNING - There is a bug in unity 5.4 and 5.5 that causes InverseTransformDirection to be affected by scale which will break negative scaling. Not tested, but updating to 5.4.2 should fix it - https://issuetracker.unity3d.com/issues/transformdirection-and-inversetransformdirection-operations-are-affected-by-scale
			Vector3 localAxis = (GetProperTransformSpace() == TransformSpace.Local && nearAxis != Axis.Any)
				? mainTargetRoot.InverseTransformDirection(axis)
				: axis;

			Vector3 targetScaleAmount = Vector3.one;
			if (nearAxis == Axis.Any) targetScaleAmount = (ExtVector3.Abs(mainTargetRoot.localScale.normalized) * scaleAmount);
			else targetScaleAmount = localAxis * scaleAmount;

			for (int i = 0; i < targetRootsOrdered.Count; i++)
			{
				Transform target = targetRootsOrdered[i];

				Vector3 targetScale = target.localScale + targetScaleAmount;

				if (pivot == TransformPivot.Pivot)
				{
					target.localScale = targetScale;
				}
				else if (pivot == TransformPivot.Center)
				{
					if (scaleType == ScaleType.FromPoint)
					{
						target.SetScaleFrom(originalPivot, targetScale);
					}
					else if (scaleType == ScaleType.FromPointOffset)
					{
						target.SetScaleFromOffset(originalPivot, targetScale);
					}
				}
			}

			totalScaleAmount += scaleAmount;
			return currentSnapScaleAmount;
		}
		
		/// <summary>
		/// Snap Move
		/// </summary>
		/// <param name="mousePosition"></param>
		/// <param name="previousMousePosition"></param>
		/// <param name="projectedAxis"></param>
		/// <param name="axis"></param>
		/// <param name="isSnapping"></param>
		/// <param name="currentSnapMovementAmount"></param>
		/// <param name="otherAxis1"></param>
		/// <param name="otherAxis2"></param>
		/// <returns></returns>
		private Vector3 CurrentSnapMovementAmount(Vector3 mousePosition, Vector3 previousMousePosition, Vector3 projectedAxis,
			Vector3 axis, bool isSnapping, Vector3 currentSnapMovementAmount, Vector3 otherAxis1, Vector3 otherAxis2)
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
					float snapAmount = CalculateSnapAmount(movementSnap, currentSnapMovementAmount.magnitude, out remainder);

					if (snapAmount != 0)
					{
						movement = currentSnapMovementAmount.normalized * snapAmount;
						currentSnapMovementAmount = currentSnapMovementAmount.normalized * remainder;
					}
				}
			}

			for (int i = 0; i < targetRootsOrdered.Count; i++)
			{
				Transform target = targetRootsOrdered[i];

				target.Translate(movement, Space.World);
			}

			SetPivotPointOffset(movement);
			return currentSnapMovementAmount;
		}


		#endregion

		float CalculateSnapAmount(float snapValue, float currentAmount, out float remainder)
		{
			remainder = 0;
			if(snapValue <= 0) return currentAmount;

			float currentAmountAbs = Mathf.Abs(currentAmount);
			if(currentAmountAbs > snapValue)
			{
				remainder = currentAmountAbs % snapValue;
				return snapValue * (Mathf.Sign(currentAmount) * Mathf.Floor(currentAmountAbs / snapValue));
			}

			return 0;
		}

		Vector3 GetNearAxisDirection(out Vector3 otherAxis1, out Vector3 otherAxis2)
		{
			otherAxis1 = otherAxis2 = Vector3.zero;

			if(nearAxis != Axis.None)
			{
				if(nearAxis == Axis.X)
				{
					otherAxis1 = axisInfo.yDirection;
					otherAxis2 = axisInfo.zDirection;
					return axisInfo.xDirection;
				}
				if(nearAxis == Axis.Y)
				{
					otherAxis1 = axisInfo.xDirection;
					otherAxis2 = axisInfo.zDirection;
					return axisInfo.yDirection;
				}
				if(nearAxis == Axis.Z)
				{
					otherAxis1 = axisInfo.xDirection;
					otherAxis2 = axisInfo.yDirection;
					return axisInfo.zDirection;
				}
				if(nearAxis == Axis.Any)
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
		void GetTarget()
		{
			if(nearAxis == Axis.None && Input.GetMouseButtonDown(0))
			{
				bool isAdding = Input.GetKey(AddSelection);
				bool isRemoving = Input.GetKey(RemoveSelection);

				RaycastHit hitInfo; 
				if(Physics.Raycast(myCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, selectionMask))
				{
					Transform target = hitInfo.transform;

					if(isAdding)
					{
						AddTarget(target);
					}
					else if(isRemoving)
					{
						RemoveTarget(target);
					}
					else if(!isAdding && !isRemoving)
					{
						ClearAndAddTarget(target);
					}
				}
				else
				{
					if(!isAdding && !isRemoving)
					{
						ClearTargets();
					}
				}
			}
		}

		public void AddTarget(Transform target, bool addCommand = true)
		{
			if(target != null)
			{
				Debug.Log("Add Target");
				if(targetRoots.ContainsKey(target)) return;
				if(children.Contains(target)) return;

				if(addCommand) UndoRedoManager.Insert(new AddTargetCommand(this, target, targetRootsOrdered));

				AddTargetRoot(target);
				AddTargetHighlightedRenderers(target);

				SetPivotPoint();
			}
		}

		public void RemoveTarget(Transform target, bool addCommand = true)
		{
			if(target != null)
			{
				if(!targetRoots.ContainsKey(target)) return;

				if(addCommand) UndoRedoManager.Insert(new RemoveTargetCommand(this, target));

				RemoveTargetHighlightedRenderers(target);
				RemoveTargetRoot(target);

				SetPivotPoint();
			}
		}

		public void ClearTargets(bool addCommand = true)
		{
			if(addCommand) UndoRedoManager.Insert(new ClearTargetsCommand(this, targetRootsOrdered));

			ClearAllHighlightedRenderers();
			targetRoots.Clear();
			targetRootsOrdered.Clear();
			children.Clear();
		}

		void ClearAndAddTarget(Transform target)
		{
			UndoRedoManager.Insert(new ClearAndAddTargetCommand(this, target, targetRootsOrdered));

			ClearTargets(false);
			AddTarget(target, false);
		}

		void AddTargetHighlightedRenderers(Transform target)
		{
			if(target != null)
			{
				GetTargetRenderers(target, renderersBuffer);

				for(int i = 0; i < renderersBuffer.Count; i++)
				{
					Renderer render = renderersBuffer[i];

					if(!highlightedRenderers.Contains(render))
					{
						materialsBuffer.Clear();
						materialsBuffer.AddRange(render.sharedMaterials);

						if(!materialsBuffer.Contains(outlineMaterial))
						{
							materialsBuffer.Add(outlineMaterial);
							render.materials = materialsBuffer.ToArray();
						}

						highlightedRenderers.Add(render);
					}
				}

				materialsBuffer.Clear();
			}
		}

		void GetTargetRenderers(Transform target, List<Renderer> renderers)
		{
			renderers.Clear();
			if(target != null)
			{
				target.GetComponentsInChildren<Renderer>(true, renderers);
			}
		}

		void ClearAllHighlightedRenderers()
		{
			foreach(var target in targetRoots)
			{
				RemoveTargetHighlightedRenderers(target.Key);
			}

			//In case any are still left, such as if they changed parents or what not when they were highlighted.
			renderersBuffer.Clear();
			renderersBuffer.AddRange(highlightedRenderers);
			RemoveHighlightedRenderers(renderersBuffer);
		}

		void RemoveTargetHighlightedRenderers(Transform target)
		{
			GetTargetRenderers(target, renderersBuffer);

			RemoveHighlightedRenderers(renderersBuffer);
		}

		void RemoveHighlightedRenderers(List<Renderer> renderers)
		{
			for(int i = 0; i < renderersBuffer.Count; i++)
			{
				Renderer render = renderersBuffer[i];
				if(render != null)
				{
					materialsBuffer.Clear();
					materialsBuffer.AddRange(render.sharedMaterials);

					if(materialsBuffer.Contains(outlineMaterial))
					{
						materialsBuffer.Remove(outlineMaterial);
						render.materials = materialsBuffer.ToArray();
					}
				}

				highlightedRenderers.Remove(render);
			}

			renderersBuffer.Clear();
		}

		void AddTargetRoot(Transform targetRoot)
		{
			targetRoots.Add(targetRoot, new TargetInfo());
			targetRootsOrdered.Add(targetRoot);

			AddAllChildren(targetRoot);
		}
		void RemoveTargetRoot(Transform targetRoot)
		{
			if(targetRoots.Remove(targetRoot))
			{
				targetRootsOrdered.Remove(targetRoot);

				RemoveAllChildren(targetRoot);
			}
		}
		#endregion


		void AddAllChildren(Transform target)
		{
			childrenBuffer.Clear();
			target.GetComponentsInChildren<Transform>(true, childrenBuffer);
			childrenBuffer.Remove(target);

			for(int i = 0; i < childrenBuffer.Count; i++)
			{
				Transform child = childrenBuffer[i];
				children.Add(child);
				RemoveTargetRoot(child); //We do this in case we selected child first and then the parent.
			}

			childrenBuffer.Clear();
		}
		void RemoveAllChildren(Transform target)
		{
			childrenBuffer.Clear();
			target.GetComponentsInChildren<Transform>(true, childrenBuffer);
			childrenBuffer.Remove(target);

			for(int i = 0; i < childrenBuffer.Count; i++)
			{
				children.Remove(childrenBuffer[i]);
			}

			childrenBuffer.Clear();
		}

		#region SetPivot
		/// <summary>
		/// Pivot을 설정합니다. 
		/// </summary>
		public void SetPivotPoint()
		{
			if(mainTargetRoot != null)
			{
				if(pivot == TransformPivot.Pivot)
				{
					pivotPoint = mainTargetRoot.position;
				}
				else if(pivot == TransformPivot.Center)
				{
					totalCenterPivotPoint = Vector3.zero;

					Dictionary<Transform, TargetInfo>.Enumerator targetsEnumerator = targetRoots.GetEnumerator(); //We avoid foreach to avoid garbage.
					while(targetsEnumerator.MoveNext())
					{
						Transform target = targetsEnumerator.Current.Key;
						TargetInfo info = targetsEnumerator.Current.Value;
						info.centerPivotPoint = target.GetCenter(centerType);

						totalCenterPivotPoint += info.centerPivotPoint;
					}

					totalCenterPivotPoint /= targetRoots.Count;

					if(centerType == CenterType.Solo)
					{
						pivotPoint = targetRoots[mainTargetRoot].centerPivotPoint;
					}
					else if(centerType == CenterType.All)
					{
						pivotPoint = totalCenterPivotPoint;
					}
				}

				Arrow.transform.position = pivotPoint;
			}
		}
		void SetPivotPointOffset(Vector3 offset)
		{
			pivotPoint += offset;
			totalCenterPivotPoint += offset;
		}
		

		#endregion

		
		IEnumerator ForceUpdatePivotPointAtEndOfFrame()
		{
			while(this.enabled)
			{
				ForceUpdatePivotPointOnChange();
				yield return waitForEndOFFrame;
			}
		}

		void ForceUpdatePivotPointOnChange()
		{
			if(forceUpdatePivotPointOnChange)
			{
				if(mainTargetRoot != null && !isTransforming)
				{
					Debug.Log("PivotPointChange");
					bool hasSet = false;
					Dictionary<Transform, TargetInfo>.Enumerator targets = targetRoots.GetEnumerator();
					while(targets.MoveNext())
					{
						if(!hasSet)
						{
							if(targets.Current.Value.previousPosition != Vector3.zero && targets.Current.Key.position != targets.Current.Value.previousPosition)
							{
								SetPivotPoint();
								hasSet = true;
							}
						}

						targets.Current.Value.previousPosition = targets.Current.Key.position;
					}
				}
			}
		}

		public void SetTranslatingAxis(TransformType type, Axis axis, Axis planeAxis = Axis.None)
		{
			this.translatingType = type;
			this.nearAxis = axis;
			this.planeAxis = planeAxis;
		}

		public AxisInfo GetAxisInfo()
		{
			AxisInfo currentAxisInfo = axisInfo;

			if(isTransforming && GetProperTransformSpace() == TransformSpace.Global && translatingType == TransformType.Rotate)
			{
				currentAxisInfo.xDirection = totalRotationAmount * Vector3.right;
				currentAxisInfo.yDirection = totalRotationAmount * Vector3.up;
				currentAxisInfo.zDirection = totalRotationAmount * Vector3.forward;
			}

			return currentAxisInfo;
		}

		void SetNearAxis()
		{
			if(isTransforming) return;

			SetTranslatingAxis(transformType, Axis.None);

			if(mainTargetRoot == null) return;

			float distanceMultiplier = GetDistanceMultiplier();
			float handleMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + handleWidth) * distanceMultiplier;

			if(nearAxis == Axis.None && (TransformTypeContains(TransformType.Move) || TransformTypeContains(TransformType.Scale)))
			{
				if(nearAxis == Axis.None && TransformTypeContains(TransformType.Scale))
				{
					float tipMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + boxSize) * distanceMultiplier;
					HandleNearestPlanes(TransformType.Scale, handleSquares, tipMinSelectedDistanceCheck);
				}

				if(nearAxis == Axis.None && TransformTypeContains(TransformType.Move))
				{
					float planeMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + planeSize) * distanceMultiplier;
					HandleNearestPlanes(TransformType.Move, handlePlanes, planeMinSelectedDistanceCheck);
						
					if(nearAxis != Axis.None)
					{
						planeAxis = nearAxis;
					}
					else
					{
						float tipMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + triangleSize) * distanceMultiplier;
						HandleNearestLines(TransformType.Move, handleTriangles, tipMinSelectedDistanceCheck);
					}
				}

				if(nearAxis == Axis.None)
				{
					TransformType transType = transformType == TransformType.All ? TransformType.Move : transformType;
					HandleNearestLines(transType, handleLines, handleMinSelectedDistanceCheck);
				}
			}
			
			if(nearAxis == Axis.None && TransformTypeContains(TransformType.Rotate))
			{
				HandleNearestLines(TransformType.Rotate, circlesLines, handleMinSelectedDistanceCheck);
			}
		}

		void HandleNearestLines(TransformType type, AxisVectors axisVectors, float minSelectedDistanceCheck)
		{
			float xClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.x);
			float yClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.y);
			float zClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.z);
			float allClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.all);

			HandleNearest(type, xClosestDistance, yClosestDistance, zClosestDistance, allClosestDistance, minSelectedDistanceCheck);
		}

		void HandleNearestPlanes(TransformType type, AxisVectors axisVectors, float minSelectedDistanceCheck)
		{
			float xClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.x);
			float yClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.y);
			float zClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.z);
			float allClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.all);

			HandleNearest(type, xClosestDistance, yClosestDistance, zClosestDistance, allClosestDistance, minSelectedDistanceCheck);
		}

		void HandleNearest(TransformType type, float xClosestDistance, float yClosestDistance, float zClosestDistance, float allClosestDistance, float minSelectedDistanceCheck)
		{
			if(type == TransformType.Scale && allClosestDistance <= minSelectedDistanceCheck) SetTranslatingAxis(type, Axis.Any);
			else if(xClosestDistance <= minSelectedDistanceCheck && xClosestDistance <= yClosestDistance && xClosestDistance <= zClosestDistance) SetTranslatingAxis(type, Axis.X);
			else if(yClosestDistance <= minSelectedDistanceCheck && yClosestDistance <= xClosestDistance && yClosestDistance <= zClosestDistance) SetTranslatingAxis(type, Axis.Y);
			else if(zClosestDistance <= minSelectedDistanceCheck && zClosestDistance <= xClosestDistance && zClosestDistance <= yClosestDistance) SetTranslatingAxis(type, Axis.Z);
			else if(type == TransformType.Rotate && mainTargetRoot != null)
			{
				Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
				Vector3 mousePlaneHit = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, pivotPoint, (transform.position - pivotPoint).normalized);
				if((pivotPoint - mousePlaneHit).sqrMagnitude <= (GetHandleLength(TransformType.Rotate)).Squared()) SetTranslatingAxis(type, Axis.Any);
			}
		}

		float ClosestDistanceFromMouseToLines(List<Vector3> lines)
		{
			Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

			float closestDistance = float.MaxValue;
			for(int i = 0; i + 1 < lines.Count; i++)
			{
				IntersectPoints points = Geometry.ClosestPointsOnSegmentToLine(lines[i], lines[i + 1], mouseRay.origin, mouseRay.direction);
				float distance = Vector3.Distance(points.first, points.second);
				if(distance < closestDistance)
				{
					closestDistance = distance;
				}
			}
			return closestDistance;
		}

		float ClosestDistanceFromMouseToPlanes(List<Vector3> planePoints)
		{
			float closestDistance = float.MaxValue;

			if(planePoints.Count >= 4)
			{
				Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

				for(int i = 0; i < planePoints.Count; i += 4)
				{
					Plane plane = new Plane(planePoints[i], planePoints[i + 1], planePoints[i + 2]);

					float distanceToPlane;
					if(plane.Raycast(mouseRay, out distanceToPlane))
					{
						Vector3 pointOnPlane = mouseRay.origin + (mouseRay.direction * distanceToPlane);
						Vector3 planeCenter = (planePoints[0] + planePoints[1] + planePoints[2] + planePoints[3]) / 4f;

						float distance = Vector3.Distance(planeCenter, pointOnPlane);
						if(distance < closestDistance)
						{
							closestDistance = distance;
						}
					}
				}
			}

			return closestDistance;
		}

		void SetAxisInfo()
		{
			if(mainTargetRoot != null)
			{
				axisInfo.Set(mainTargetRoot, pivotPoint, GetProperTransformSpace());
			}
		}
		
		public float GetDistanceMultiplier()
		{
			if(mainTargetRoot == null) return 0f;

			if(myCamera.orthographic) return Mathf.Max(.01f, myCamera.orthographicSize * 2f);
			return Mathf.Max(.01f, Mathf.Abs(ExtVector3.MagnitudeInDirection(pivotPoint - transform.position, myCamera.transform.forward)));
		}
		
		/// <summary>
		/// 라인을 그리는 함수
		/// </summary>
		void SetLines()
		{
			SetHandleLines();
			SetHandlePlanes();
			SetHandleTriangles();
			SetHandleSquares();
			SetCircles(GetAxisInfo(), circlesLines);
		}

		void SetHandleLines()
		{
			handleLines.Clear();

			if(TranslatingTypeContains(TransformType.Move) || TranslatingTypeContains(TransformType.Scale))
			{
				float lineWidth = handleWidth * GetDistanceMultiplier();

				float xLineLength = 0;
				float yLineLength = 0;
				float zLineLength = 0;
				if(TranslatingTypeContains(TransformType.Move))
				{
					xLineLength = yLineLength = zLineLength = GetHandleLength(TransformType.Move);
				}
				else if(TranslatingTypeContains(TransformType.Scale))
				{
					xLineLength = GetHandleLength(TransformType.Scale, Axis.X);
					yLineLength = GetHandleLength(TransformType.Scale, Axis.Y);
					zLineLength = GetHandleLength(TransformType.Scale, Axis.Z);
				}

				AddQuads(pivotPoint, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, xLineLength, lineWidth, handleLines.x);
				AddQuads(pivotPoint, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, yLineLength, lineWidth, handleLines.y);
				AddQuads(pivotPoint, axisInfo.zDirection, axisInfo.xDirection, axisInfo.yDirection, zLineLength, lineWidth, handleLines.z);
			}
		}

		void SetHandlePlanes()
		{
			handlePlanes.Clear();

			if(TranslatingTypeContains(TransformType.Move))
			{
				Vector3 pivotToCamera = myCamera.transform.position - pivotPoint;
				float cameraXSign = Mathf.Sign(Vector3.Dot(axisInfo.xDirection, pivotToCamera));
				float cameraYSign = Mathf.Sign(Vector3.Dot(axisInfo.yDirection, pivotToCamera));
				float cameraZSign = Mathf.Sign(Vector3.Dot(axisInfo.zDirection, pivotToCamera));

				float planeSize = this.planeSize;
				if(transformType == TransformType.All) { planeSize *= allMoveHandleLengthMultiplier; }
				planeSize *= GetDistanceMultiplier();

				Vector3 xDirection = (axisInfo.xDirection * planeSize) * cameraXSign;
				Vector3 yDirection = (axisInfo.yDirection * planeSize) * cameraYSign;
				Vector3 zDirection = (axisInfo.zDirection * planeSize) * cameraZSign;

				Vector3 xPlaneCenter = pivotPoint + (yDirection + zDirection);
				Vector3 yPlaneCenter = pivotPoint + (xDirection + zDirection);
				Vector3 zPlaneCenter = pivotPoint + (xDirection + yDirection);

				AddQuad(xPlaneCenter, axisInfo.yDirection, axisInfo.zDirection, planeSize, handlePlanes.x);
				AddQuad(yPlaneCenter, axisInfo.xDirection, axisInfo.zDirection, planeSize, handlePlanes.y);
				AddQuad(zPlaneCenter, axisInfo.xDirection, axisInfo.yDirection, planeSize, handlePlanes.z);
			}
		}

		void SetHandleTriangles()
		{
			handleTriangles.Clear();

			if(TranslatingTypeContains(TransformType.Move))
			{
				float triangleLength = triangleSize * GetDistanceMultiplier();
				AddTriangles(axisInfo.GetXAxisEnd(GetHandleLength(TransformType.Move)), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, triangleLength, handleTriangles.x);
				AddTriangles(axisInfo.GetYAxisEnd(GetHandleLength(TransformType.Move)), axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, triangleLength, handleTriangles.y);
				AddTriangles(axisInfo.GetZAxisEnd(GetHandleLength(TransformType.Move)), axisInfo.zDirection, axisInfo.yDirection, axisInfo.xDirection, triangleLength, handleTriangles.z);
			}
		}

		void AddTriangles(Vector3 axisEnd, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
		{
			Vector3 endPoint = axisEnd + (axisDirection * (size * 2f));
			Square baseSquare = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, size / 2f);

			resultsBuffer.Add(baseSquare.bottomLeft);
			resultsBuffer.Add(baseSquare.topLeft);
			resultsBuffer.Add(baseSquare.topRight);
			resultsBuffer.Add(baseSquare.topLeft);
			resultsBuffer.Add(baseSquare.bottomRight);
			resultsBuffer.Add(baseSquare.topRight);

			for(int i = 0; i < 4; i++)
			{
				resultsBuffer.Add(baseSquare[i]);
				resultsBuffer.Add(baseSquare[i + 1]);
				resultsBuffer.Add(endPoint);
			}
		}

		void SetHandleSquares()
		{
			handleSquares.Clear();

			if(TranslatingTypeContains(TransformType.Scale))
			{
				float boxSize = this.boxSize * GetDistanceMultiplier();
				AddSquares(axisInfo.GetXAxisEnd(GetHandleLength(TransformType.Scale, Axis.X)), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxSize, handleSquares.x);
				AddSquares(axisInfo.GetYAxisEnd(GetHandleLength(TransformType.Scale, Axis.Y)), axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, boxSize, handleSquares.y);
				AddSquares(axisInfo.GetZAxisEnd(GetHandleLength(TransformType.Scale, Axis.Z)), axisInfo.zDirection, axisInfo.xDirection, axisInfo.yDirection, boxSize, handleSquares.z);
				AddSquares(pivotPoint - (axisInfo.xDirection * (boxSize * .5f)), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxSize, handleSquares.all);
			}
		}

		void AddSquares(Vector3 axisStart, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
		{
			AddQuads(axisStart, axisDirection, axisOtherDirection1, axisOtherDirection2, size, size * .5f, resultsBuffer);
		}
		void AddQuads(Vector3 axisStart, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float length, float width, List<Vector3> resultsBuffer)
		{
			Vector3 axisEnd = axisStart + (axisDirection * length);
			AddQuads(axisStart, axisEnd, axisOtherDirection1, axisOtherDirection2, width, resultsBuffer);
		}
		void AddQuads(Vector3 axisStart, Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float width, List<Vector3> resultsBuffer)
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

			for(int i = 0; i < 4; i++)
			{
				resultsBuffer.Add(baseRectangle[i]);
				resultsBuffer.Add(baseRectangleEnd[i]);
				resultsBuffer.Add(baseRectangleEnd[i + 1]);
				resultsBuffer.Add(baseRectangle[i + 1]);
			}
		}

		void AddQuad(Vector3 axisStart, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float width, List<Vector3> resultsBuffer)
		{
			Square baseRectangle = GetBaseSquare(axisStart, axisOtherDirection1, axisOtherDirection2, width);

			resultsBuffer.Add(baseRectangle.bottomLeft);
			resultsBuffer.Add(baseRectangle.topLeft);
			resultsBuffer.Add(baseRectangle.topRight);
			resultsBuffer.Add(baseRectangle.bottomRight);
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

		void SetCircles(AxisInfo axisInfo, AxisVectors axisVectors)
		{
			axisVectors.Clear();

			if(TranslatingTypeContains(TransformType.Rotate))
			{
				float circleLength = GetHandleLength(TransformType.Rotate);
				AddCircle(pivotPoint, axisInfo.xDirection, circleLength, axisVectors.x);
				AddCircle(pivotPoint, axisInfo.yDirection, circleLength, axisVectors.y);
				AddCircle(pivotPoint, axisInfo.zDirection, circleLength, axisVectors.z);
				AddCircle(pivotPoint, (pivotPoint - transform.position).normalized, circleLength, axisVectors.all, false);
			}
		}

		void AddCircle(Vector3 origin, Vector3 axisDirection, float size, List<Vector3> resultsBuffer, bool depthTest = true)
		{
			Vector3 up = axisDirection.normalized * size;
			Vector3 forward = Vector3.Slerp(up, -up, .5f);
			Vector3 right = Vector3.Cross(up, forward).normalized * size;
		
			Matrix4x4 matrix = new Matrix4x4();
		
			matrix[0] = right.x;
			matrix[1] = right.y;
			matrix[2] = right.z;
		
			matrix[4] = up.x;
			matrix[5] = up.y;
			matrix[6] = up.z;
		
			matrix[8] = forward.x;
			matrix[9] = forward.y;
			matrix[10] = forward.z;
		
			Vector3 lastPoint = origin + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
			Vector3 nextPoint = Vector3.zero;
			float multiplier = 360f / circleDetail;

			Plane plane = new Plane((transform.position - pivotPoint).normalized, pivotPoint);

			float circleHandleWidth = handleWidth * GetDistanceMultiplier();

			for(int i = 0; i < circleDetail + 1; i++)
			{
				nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
				nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
				nextPoint.y = 0;
			
				nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);
			
				if(!depthTest || plane.GetSide(lastPoint))
				{
					Vector3 centerPoint = (lastPoint + nextPoint) * .5f;
					Vector3 upDirection = (centerPoint - origin).normalized;
					AddQuads(lastPoint, nextPoint, upDirection, axisDirection, circleHandleWidth, resultsBuffer);
				}

				lastPoint = nextPoint;
			}
		}
		
		void DrawTriangles(List<Vector3> lines, Material material)
		{
			if(lines.Count == 0) return;

			for(int i = 0; i < lines.Count; i += 3)
			{
				Mesh m = new Mesh();
				Vector3[] VerteicesArray = new Vector3[3];
				int[] trianglesArray = new int[3];

				VerteicesArray[0] = lines[i];
				VerteicesArray[1] = lines[i + 1];
				VerteicesArray[2] = lines[i + 2];

				trianglesArray[0] = 0;
				trianglesArray[1] = 1;
				trianglesArray[2] = 2;
				
				m.vertices = VerteicesArray;
				m.triangles = trianglesArray;

				Graphics.DrawMesh(m, Vector3.zero, Quaternion.identity, material, 0);
			}
		}
		void DrawQuads(List<Vector3> lines, Material material)
		{
			if(lines.Count == 0) return;

			for(int i = 0; i < lines.Count; i += 4)
			{
				Mesh m1 = new Mesh();
				Mesh m2 = new Mesh();
				Vector3[] VerteicesArray1 = new Vector3[3];
				int[] trianglesArray1 = new int[3];
				
				Vector3[] VerteicesArray2 = new Vector3[3];
				int[] trianglesArray2 = new int[3];

				VerteicesArray1[0] = lines[i];
				VerteicesArray1[1] = lines[i + 1];
				VerteicesArray1[2] = lines[i + 2];
				
				VerteicesArray2[0] = lines[i + 1];
				VerteicesArray2[1] = lines[i + 2];
				VerteicesArray2[2] = lines[i + 3];

				trianglesArray1[0] = 0;
				trianglesArray1[1] = 1;
				trianglesArray1[2] = 2;
				
				trianglesArray2[0] = 0;
				trianglesArray2[1] = 1;
				trianglesArray2[2] = 2;

				m1.vertices = VerteicesArray1;
				m1.triangles = trianglesArray1;
				
				m2.vertices = VerteicesArray2;
				m2.triangles = trianglesArray2;

				Graphics.DrawMesh(m1, Vector3.zero, Quaternion.identity, material, 0);
				Graphics.DrawMesh(m2, Vector3.zero, Quaternion.identity, material, 0);
			}
		}

		void SetMaterial()
		{
			if(lineMaterial == null)
			{
				lineMaterial = new Material(Shader.Find("Custom/Lines"));
				outlineMaterial = new Material(Shader.Find("Obliy/BasicOutline"));
			}
		}
	}
}
