using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using MultiWindow.EventSystems;
using MultiWindow.UI.Collections;

namespace MultiWindow.UI
{
	[AddComponentMenu("MultiWindow/Event/Graphic Raycaster")]
	[RequireComponent( typeof( Canvas))]
	public class GraphicRaycaster : BaseRaycaster
	{
		protected const int kNoEventMaskSet = -1;
		
		public enum BlockingObjects
		{
			None = 0,
			TwoD = 1,
			ThreeD = 2,
			All = 3,
		}
		public override int sortOrderPriority
		{
			get
			{
				if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					return canvas.sortingOrder;
				}
				return base.sortOrderPriority;
			}
		}
		public override int renderOrderPriority
		{
			get
			{
				if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					return canvas.rootCanvas.renderOrder;
				}
				return base.renderOrderPriority;
			}
		}
		[SerializeField, FormerlySerializedAs("ignoreReversedGraphics")]
		private bool m_IgnoreReversedGraphics = true;
		[SerializeField, FormerlySerializedAs("blockingObjects")]
		private BlockingObjects m_BlockingObjects = BlockingObjects.None;
		
		public bool ignoreReversedGraphics
		{
			get {return m_IgnoreReversedGraphics; }
			set { m_IgnoreReversedGraphics = value; }
		}
		public BlockingObjects blockingObjects
		{
			get {return m_BlockingObjects; }
			set { m_BlockingObjects = value; }
		}
		[SerializeField]
		protected LayerMask m_BlockingMask = kNoEventMaskSet;
		
		public LayerMask blockingMask { get { return m_BlockingMask; } set { m_BlockingMask = value; } }
		private Canvas m_Canvas;
		
		protected GraphicRaycaster()
		{
		}
		private Canvas canvas
		{
			get
			{
				if( m_Canvas != null)
				{
					return m_Canvas;
				}
				m_Canvas = GetComponent<Canvas>();
				return m_Canvas;
			}
		}
		[NonSerialized]
		readonly List<Graphic> m_RaycastResults = new();
		
		public override void Raycast( PointerEventData eventData, List<RaycastResult> resultAppendList)
		{
			if( canvas == null)
			{
				return;
			}
			var canvasGraphics = GetRaycastableGraphicsForCanvas( canvas);
			if (canvasGraphics == null || canvasGraphics.Count == 0)
			{
				return;
			}
			int displayIndex;
			var currentEventCamera = eventCamera;
			
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || currentEventCamera == null)
			{
				displayIndex = canvas.targetDisplay;
			}
			else
			{
				displayIndex = currentEventCamera.targetDisplay;
			}
			Vector3 eventPosition = MultipleDisplayUtilities.GetRelativeMousePositionForRaycast( eventData);
/*			
			if ((int) eventPosition.z != displayIndex)
			{
				return;
			}
*/
			Vector2 pos;
			
			if( currentEventCamera == null)
			{
				float w = Screen.width;
				float h = Screen.height;
				
				if (displayIndex > 0 && displayIndex < Display.displays.Length)
				{
				#if UNITY_ANDROID
					w = Display.displays[ displayIndex].renderingWidth;
					h = Display.displays[ displayIndex].renderingHeight;
				#else
					w = Display.displays[ displayIndex].systemWidth;
					h = Display.displays[ displayIndex].systemHeight;
				#endif
				}
				pos = new Vector2( eventPosition.x / w, eventPosition.y / h);
			}
			else
			{
				pos = currentEventCamera.ScreenToViewportPoint(eventPosition);
			}
			if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
			{
				return;
			}
			float hitDistance = float.MaxValue;
			var ray = new Ray();
			
			if( currentEventCamera != null)
			{
				ray = currentEventCamera.ScreenPointToRay( eventPosition);
			}
			if( canvas.renderMode != RenderMode.ScreenSpaceOverlay && blockingObjects != BlockingObjects.None)
			{
				float distanceToClipPlane = 100.0f;
				
				if( currentEventCamera != null)
				{
					float projectionDirection = ray.direction.z;
					distanceToClipPlane = Mathf.Approximately( 0.0f, projectionDirection)
						? Mathf.Infinity
						: Mathf.Abs((currentEventCamera.farClipPlane - currentEventCamera.nearClipPlane) / projectionDirection);
				}
			#if PACKAGE_PHYSICS
				if( blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)
				{
					if( ReflectionMethodsCache.Singleton.raycast3D != null)
					{
						if (ReflectionMethodsCache.Singleton.raycast3D(ray, out RaycastHit hit, distanceToClipPlane, (int)m_BlockingMask))
						{
							hitDistance = hit.distance;
						}
					}
				}
			#endif
			#if PACKAGE_PHYSICS2D
				if( blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
				{
					if( ReflectionMethodsCache.Singleton.raycast2D != null)
					{
						var hits = ReflectionMethodsCache.Singleton.getRayIntersectionAll( ray, distanceToClipPlane, (int)m_BlockingMask);
						if( hits.Length > 0)
						{
							hitDistance = hits[ 0].distance;
						}
					}
				}
			#endif
			}
			m_RaycastResults.Clear();
			Raycast( canvas, currentEventCamera, eventPosition, canvasGraphics, m_RaycastResults);
			
			int totalCount = m_RaycastResults.Count;
			
			for( var index = 0; index < totalCount; index++)
			{
				var go = m_RaycastResults[index].gameObject;
				bool appendGraphic = true;
				
				if( ignoreReversedGraphics)
				{
					if (currentEventCamera == null)
					{
						var dir = go.transform.rotation * Vector3.forward;
						appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
					}
					else
					{
						var cameraForward = currentEventCamera.transform.rotation * Vector3.forward * currentEventCamera.nearClipPlane;
						appendGraphic = Vector3.Dot(go.transform.position - currentEventCamera.transform.position - cameraForward, go.transform.forward) >= 0;
					}
				}
				if (appendGraphic)
				{
					float distance = 0;
					Transform trans = go.transform;
					Vector3 transForward = trans.forward;
					
					if (currentEventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
					{
						distance = 0;
					}
					else
					{
						distance = (Vector3.Dot(transForward, trans.position - ray.origin) / Vector3.Dot(transForward, ray.direction));
						if (distance < 0)
						{
							continue;
						}
					}
					if (distance >= hitDistance)
					{
						continue;
					}
					var castResult = new RaycastResult
					{
						gameObject = go,
						module = this,
						distance = distance,
						screenPosition = eventPosition,
						displayIndex = displayIndex,
						index = resultAppendList.Count,
						depth = m_RaycastResults[index].depth,
						sortingLayer = canvas.sortingLayerID,
						sortingOrder = canvas.sortingOrder,
						worldPosition = ray.origin + ray.direction * distance,
						worldNormal = -transForward
					};
					resultAppendList.Add(castResult);
				}
			}
		}
		public override Camera eventCamera
		{
			get
			{
				var canvas = this.canvas;
				var renderMode = canvas.renderMode;
				
				if( renderMode == RenderMode.ScreenSpaceOverlay
				||	(renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
				{
					return null;
				}
				return canvas.worldCamera ?? Camera.main;
			}
		}
		[NonSerialized]
		static readonly List<Graphic> s_SortedGraphics = new();
		
		static void Raycast( Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
		{
			int totalCount = foundGraphics.Count;
			
			for (int i = 0; i < totalCount; ++i)
			{
				Graphic graphic = foundGraphics[i];
				
				if( !graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
				{
					continue;
				}
				if( !RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera, graphic.raycastPadding))
				{
					continue;
				}
				if( eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
				{
					continue;
				}
				if( graphic.Raycast(pointerPosition, eventCamera))
				{
					s_SortedGraphics.Add(graphic);
				}
			}
			s_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
			totalCount = s_SortedGraphics.Count;
			
			for (int i = 0; i < totalCount; ++i)
			{
				results.Add(s_SortedGraphics[i]);
			}
			s_SortedGraphics.Clear();
		}
		
		
		
		
		static readonly List<Graphic> s_EmptyList = new();
		readonly Dictionary<Canvas, IndexedSet<Graphic>> m_Graphics = new();
		readonly Dictionary<Canvas, IndexedSet<Graphic>> m_RaycastableGraphics = new();
		
		public void RegisterGraphicForCanvas( Canvas c, Graphic graphic)
		{
			if( c == null || graphic == null)
			{
				return;
			}
			IndexedSet<Graphic> graphics;
			m_Graphics.TryGetValue( c, out graphics);
			
			if (graphics != null)
			{
				graphics.AddUnique( graphic);
				RegisterRaycastGraphicForCanvas( c, graphic);
				return;
			}
			graphics = new IndexedSet<Graphic>
			{
				graphic
			};
			m_Graphics.Add( c, graphics);
			RegisterRaycastGraphicForCanvas( c, graphic);
		}
		public void RegisterRaycastGraphicForCanvas( Canvas c, Graphic graphic)
		{
			if( c == null || graphic == null || !graphic.raycastTarget)
			{
				return;
			}
			IndexedSet<Graphic> graphics;
			m_RaycastableGraphics.TryGetValue( c, out graphics);
			
			if( graphics != null)
			{
				graphics.AddUnique( graphic);
				return;
			}
			graphics = new IndexedSet<Graphic>
			{
				graphic
			};
			m_RaycastableGraphics.Add( c, graphics);
		}
		public void UnregisterGraphicForCanvas( Canvas c, Graphic graphic)
		{
			if( c == null || graphic == null)
			{
				return;
			}
			IndexedSet<Graphic> graphics;
			
			if( m_Graphics.TryGetValue( c, out graphics))
			{
				graphics.Remove( graphic);
				
				if( graphics.Capacity == 0)
				{
					m_Graphics.Remove( c);
				}
				UnregisterRaycastGraphicForCanvas( c, graphic);
			}
		}
		public void UnregisterRaycastGraphicForCanvas( Canvas c, Graphic graphic)
		{
			if( c == null || graphic == null)
			{
				return;
			}
			IndexedSet<Graphic> graphics;
			
			if( m_RaycastableGraphics.TryGetValue( c, out graphics))
			{
				graphics.Remove( graphic);
				
				if( graphics.Count == 0)
				{
					m_RaycastableGraphics.Remove( c);
				}
			}
		}
		public void DisableGraphicForCanvas( Canvas c, Graphic graphic)
		{
			if( c == null)
			{
				return;
			}
			IndexedSet<Graphic> graphics;
			
			if( m_Graphics.TryGetValue( c, out graphics))
			{
				graphics.DisableItem( graphic);
				
				if( graphics.Capacity == 0)
				{
					m_Graphics.Remove(　c);
				}
				DisableRaycastGraphicForCanvas( c, graphic);
			}
		}
		public void DisableRaycastGraphicForCanvas( Canvas c, Graphic graphic)
		{
			if( c == null || !graphic.raycastTarget)
			{
				return;
			}
			IndexedSet<Graphic> graphics;
			
			if( m_RaycastableGraphics.TryGetValue( c, out graphics))
			{
				graphics.DisableItem(graphic);
				
				if( graphics.Capacity == 0)
				{
					m_RaycastableGraphics.Remove( c);
				}
			}
		}
		public IList<Graphic> GetGraphicsForCanvas( Canvas canvas)
		{
			IndexedSet<Graphic> graphics;
			
			if( m_Graphics.TryGetValue( canvas, out graphics))
			{
				return graphics;
			}
			return s_EmptyList;
		}
		public IList<Graphic> GetRaycastableGraphicsForCanvas( Canvas canvas)
		{
			IndexedSet<Graphic> graphics;
			
			if( m_RaycastableGraphics.TryGetValue( canvas, out graphics))
			{
				return graphics;
			}
			return s_EmptyList;
		}
	}
}
