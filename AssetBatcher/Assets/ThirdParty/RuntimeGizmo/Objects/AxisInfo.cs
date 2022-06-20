using System;
using UnityEngine;

namespace RuntimeGizmos
{
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
}
