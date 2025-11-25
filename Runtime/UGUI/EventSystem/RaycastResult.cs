
using UnityEngine;

namespace MultiWindow.EventSystems
{
	public struct RaycastResult
	{
		public GameObject gameObject
		{
			get { return m_GameObject; }
			set { m_GameObject = value; }
		}
		public bool isValid
		{
			get { return module != null && gameObject != null; }
		}
		public void Clear()
		{
			gameObject = null;
			module = null;
			distance = 0;
			index = 0;
			depth = 0;
			sortingLayer = 0;
			sortingOrder = 0;
			origin = Vector3.zero;
			worldNormal = Vector3.up;
			worldPosition = Vector3.zero;
			screenPosition = Vector3.zero;
			displayIndex = 0;
		#if PACKAGE_UITOOLKIT
			document = null;
			element = null;
		#endif
		}
		public override string ToString()
		{
			if (!isValid)
			{
				return "";
			}
			return "Name: " + gameObject + "\n" +
				"module: " + module + "\n" +
				"distance: " + distance + "\n" +
				"index: " + index + "\n" +
				"depth: " + depth + "\n" +
				"worldNormal: " + worldNormal + "\n" +
				"worldPosition: " + worldPosition + "\n" +
				"screenPosition: " + screenPosition + "\n" +
				"module.sortOrderPriority: " + module.sortOrderPriority + "\n" +
				"module.renderOrderPriority: " + module.renderOrderPriority + "\n" +
				"sortingLayer: " + sortingLayer + "\n" +
				"sortingOrder: " + sortingOrder;
		}
		public BaseRaycaster module;
		public float distance;
		public float index;
		public int depth;
		public int sortingGroupID;
		public int sortingGroupOrder;
		public int sortingLayer;
		public int sortingOrder;
		public Vector3 origin;
		public Vector3 worldPosition;
		public Vector3 worldNormal;
		public Vector2 screenPosition;
		public int displayIndex;
	#if PACKAGE_UITOOLKIT
		public UIDocument document;
		public VisualElement element;
	#endif
		GameObject m_GameObject;
	}
}
