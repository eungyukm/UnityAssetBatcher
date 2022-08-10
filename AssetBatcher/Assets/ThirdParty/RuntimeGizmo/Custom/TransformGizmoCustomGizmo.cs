using System;
using UnityEngine;

namespace RuntimeGizmos
{
	public class TransformGizmoCustomGizmo : MonoBehaviour
	{
		public bool autoFindTransformGizmo = true;
		public TransformGizmo transformGizmo;

		public CustomTransformGizmos customTranslationGizmos = new CustomTransformGizmos();
		public CustomTransformGizmos customRotationGizmos = new CustomTransformGizmos();
		public CustomTransformGizmos customScaleGizmos = new CustomTransformGizmos();

		public bool scaleBasedOnDistance = true;
		public float scaleMultiplier = .4f;

		public int gizmoLayer = 2; //2 is the ignoreRaycast layer. Set to whatever you want.

		LayerMask mask;

		void Awake()
		{
			if(transformGizmo == null && autoFindTransformGizmo)
			{
				transformGizmo = GameObject.FindObjectOfType<TransformGizmo>();
			}

			mask = LayerMask.GetMask(LayerMask.LayerToName(gizmoLayer));

			customTranslationGizmos.Init(gizmoLayer);
			customRotationGizmos.Init(gizmoLayer);
			customScaleGizmos.Init(gizmoLayer);
		}
	}

	[Serializable]
	public class CustomTransformGizmos
	{
		public Transform xAxisGizmo;
		public Transform yAxisGizmo;
		public Transform zAxisGizmo;
		public Transform anyAxisGizmo;

		Collider xAxisGizmoCollider;
		Collider yAxisGizmoCollider;
		Collider zAxisGizmoCollider;
		Collider anyAxisGizmoCollider;

		Vector3 originalXAxisScale;
		Vector3 originalYAxisScale;
		Vector3 originalZAxisScale;
		Vector3 originalAnyAxisScale;

		public void Init(int layer)
		{
			if(xAxisGizmo != null)
			{
				SetLayerRecursively(xAxisGizmo.gameObject, layer);
				xAxisGizmoCollider = xAxisGizmo.GetComponentInChildren<Collider>();
				originalXAxisScale = xAxisGizmo.localScale;
			}
			if(yAxisGizmo != null)
			{
				SetLayerRecursively(yAxisGizmo.gameObject, layer);
				yAxisGizmoCollider = yAxisGizmo.GetComponentInChildren<Collider>();
				originalYAxisScale = yAxisGizmo.localScale;
			}
			if(zAxisGizmo != null)
			{
				SetLayerRecursively(zAxisGizmo.gameObject, layer);
				zAxisGizmoCollider = zAxisGizmo.GetComponentInChildren<Collider>();
				originalZAxisScale = zAxisGizmo.localScale;
			}
			if(anyAxisGizmo != null)
			{
				SetLayerRecursively(anyAxisGizmo.gameObject, layer);
				anyAxisGizmoCollider = anyAxisGizmo.GetComponentInChildren<Collider>();
				originalAnyAxisScale = anyAxisGizmo.localScale;
			}
		}

		public void SetEnable(bool enable)
		{
			if(xAxisGizmo != null && xAxisGizmo.gameObject.activeSelf != enable) xAxisGizmo.gameObject.SetActive(enable);
			if(yAxisGizmo != null && yAxisGizmo.gameObject.activeSelf != enable) yAxisGizmo.gameObject.SetActive(enable);
			if(zAxisGizmo != null && zAxisGizmo.gameObject.activeSelf != enable) zAxisGizmo.gameObject.SetActive(enable);
			if(anyAxisGizmo != null && anyAxisGizmo.gameObject.activeSelf != enable) anyAxisGizmo.gameObject.SetActive(enable);
		}

		public void SetAxis(AxisInfo axisInfo)
		{
			Quaternion lookRotation = Quaternion.LookRotation(axisInfo.zDirection, axisInfo.yDirection);

			if(xAxisGizmo != null) xAxisGizmo.rotation = lookRotation;
			if(yAxisGizmo != null) yAxisGizmo.rotation = lookRotation;
			if(zAxisGizmo != null) zAxisGizmo.rotation = lookRotation;
			if(anyAxisGizmo != null) anyAxisGizmo.rotation = lookRotation;
		}

		public void SetPosition(Vector3 position)
		{
			if(xAxisGizmo != null) xAxisGizmo.position = position;
			if(yAxisGizmo != null) yAxisGizmo.position = position;
			if(zAxisGizmo != null) zAxisGizmo.position = position;
			if(anyAxisGizmo != null) anyAxisGizmo.position = position;
		}

		public void ScaleMultiply(Vector4 scaleMultiplier)
		{
			if(xAxisGizmo != null) xAxisGizmo.localScale = Vector3.Scale(originalXAxisScale, new Vector3(scaleMultiplier.w + scaleMultiplier.x, scaleMultiplier.w, scaleMultiplier.w));
			if(yAxisGizmo != null) yAxisGizmo.localScale = Vector3.Scale(originalYAxisScale, new Vector3(scaleMultiplier.w, scaleMultiplier.w + scaleMultiplier.y, scaleMultiplier.w));
			if(zAxisGizmo != null) zAxisGizmo.localScale = Vector3.Scale(originalZAxisScale, new Vector3(scaleMultiplier.w, scaleMultiplier.w, scaleMultiplier.w + scaleMultiplier.z));
			if(anyAxisGizmo != null) anyAxisGizmo.localScale = originalAnyAxisScale * scaleMultiplier.w;
		}

		public Axis GetSelectedAxis(Collider selectedCollider)
		{
			if(xAxisGizmoCollider != null && xAxisGizmoCollider == selectedCollider) return Axis.X;
			if(yAxisGizmoCollider != null && yAxisGizmoCollider == selectedCollider) return Axis.Y;
			if(zAxisGizmoCollider != null && zAxisGizmoCollider == selectedCollider) return Axis.Z;
			if(anyAxisGizmoCollider != null && anyAxisGizmoCollider == selectedCollider) return Axis.Any;

			return Axis.None;
		}

		void SetLayerRecursively(GameObject gameObject, int layer)
		{
			Transform[] selfAndChildren = gameObject.GetComponentsInChildren<Transform>(true);

			for(int i = 0; i < selfAndChildren.Length; i++)
			{
				selfAndChildren[i].gameObject.layer = layer;
			}
		}
	}
}
