using System;
#if UNITY_EDITOR
	using System.Reflection;
#endif
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Events;
using UnityEngine.Serialization;
using MultiWindow.UI.CoroutineTween;

namespace MultiWindow.UI
{
	[DisallowMultipleComponent]
	[RequireComponent( typeof( RectTransform))]
	[ExecuteAlways]
	public abstract class Graphic : UnityEngine.EventSystems.UIBehaviour, UnityEngine.UI.ICanvasElement
	{
		public static Material defaultGraphicMaterial
		{
			get
			{
				if( s_DefaultUI == null)
				{
					s_DefaultUI = Canvas.GetDefaultCanvasMaterial();
				}
				return s_DefaultUI;
			}
		}
		public RectTransform rectTransform
		{
			get
			{
				if( ReferenceEquals(m_RectTransform, null))
				{
					m_RectTransform = GetComponent<RectTransform>();
				}
				return m_RectTransform;
			}
		}
		public Canvas canvas
		{
			get
			{
				if( m_Canvas == null)
				{
					CacheCanvas();
				}
				return m_Canvas;
			}
		}
		public virtual Color color
		{
			get { return m_Color; }
			set { if (SetPropertyUtility.SetColor(ref m_Color, value)) SetVerticesDirty(); }
		}
		public int depth
		{
			get { return canvasRenderer.absoluteDepth; }
		}
		public virtual bool raycastTarget
		{
			get{ return m_RaycastTarget; }
			set
			{
				if( value != m_RaycastTarget)
				{
					if( m_RaycastTarget != false)
					{
						GraphicRaycaster?.UnregisterRaycastGraphicForCanvas( canvas, this);
					}
					m_RaycastTarget = value;
					
					if( m_RaycastTarget && isActiveAndEnabled)
					{
						GraphicRaycaster?.RegisterRaycastGraphicForCanvas( canvas, this);
					}
				}
				m_RaycastTargetCache = value;
			}
		}
		public Vector4 raycastPadding
		{
			get { return m_RaycastPadding; }
			set
			{
				m_RaycastPadding = value;
			}
		}
		protected bool useLegacyMeshGeneration
		{
			get;
			set;
		}
		protected GraphicRaycaster GraphicRaycaster
		{
			get
			{
				if( m_GraphicRaycaster == null)
				{
					m_GraphicRaycaster = GetComponentInParent<GraphicRaycaster>();
				}
				return m_GraphicRaycaster;
			}
		}
		protected Graphic()
		{
			if( m_ColorTweenRunner == null)
			{
				m_ColorTweenRunner = new TweenRunner<ColorTween>();
			}
			m_ColorTweenRunner.Init( this);
			useLegacyMeshGeneration = true;
		}
		public virtual void SetAllDirty()
		{
			if( m_SkipLayoutUpdate)
			{
				m_SkipLayoutUpdate = false;
			}
			else
			{
				SetLayoutDirty();
			}
			if( m_SkipMaterialUpdate)
			{
				m_SkipMaterialUpdate = false;
			}
			else
			{
				SetMaterialDirty();
			}
			SetVerticesDirty();
			SetRaycastDirty();
		}
		public virtual void SetLayoutDirty()
		{
			if( !IsActive())
			{
				return;
			}
			UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild( rectTransform);
			
			if( m_OnDirtyLayoutCallback != null)
			{
				m_OnDirtyLayoutCallback();
			}
		}
		public virtual void SetVerticesDirty()
		{
			if( !IsActive())
			{
				return;
			}
			m_VertsDirty = true;
			UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild( this);
			
			if( m_OnDirtyVertsCallback != null)
			{
				m_OnDirtyVertsCallback();
			}
		}
		public virtual void SetMaterialDirty()
		{
			if( !IsActive())
			{
				return;
			}
			m_MaterialDirty = true;
			UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild( this);
			
			if( m_OnDirtyMaterialCallback != null)
			{
				m_OnDirtyMaterialCallback();
			}
		}
		public void SetRaycastDirty()
		{
			if( m_RaycastTargetCache != m_RaycastTarget)
			{
				if( m_RaycastTarget != false && isActiveAndEnabled != false)
				{
					GraphicRaycaster?.RegisterRaycastGraphicForCanvas( canvas, this);
				}
				else if( m_RaycastTarget == false)
				{
					GraphicRaycaster?.UnregisterRaycastGraphicForCanvas( canvas, this);
				}
			}
			m_RaycastTargetCache = m_RaycastTarget;
		}
		protected override void OnRectTransformDimensionsChange()
		{
			if( gameObject.activeInHierarchy != false)
			{
				if( UnityEngine.UI.CanvasUpdateRegistry.IsRebuildingLayout())
				{
					SetVerticesDirty();
				}
				else
				{
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}
		protected override void OnBeforeTransformParentChanged()
		{
			GraphicRaycaster?.UnregisterGraphicForCanvas( canvas, this);
			UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			
			m_Canvas = null;
			
			if (!IsActive())
			{
				return;
			}
			CacheCanvas();
			GraphicRaycaster?.RegisterGraphicForCanvas( canvas, this);
			SetAllDirty();
		}
		private void CacheCanvas()
		{
			var list = ListPool<Canvas>.Get();
			gameObject.GetComponentsInParent( false, list);
			
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; ++i)
				{
					if (list[i].isActiveAndEnabled)
					{
						m_Canvas = list[i];
						break;
					}
					if (i == list.Count - 1)
					{
						m_Canvas = null;
					}
				}
			}
			else
			{
				m_Canvas = null;
			}
			ListPool<Canvas>.Release( list);
		}
		public CanvasRenderer canvasRenderer
		{
			get
			{
				if (ReferenceEquals(m_CanvasRenderer, null))
				{
					m_CanvasRenderer = GetComponent<CanvasRenderer>();
					
					if (ReferenceEquals(m_CanvasRenderer, null))
					{
						m_CanvasRenderer = gameObject.AddComponent<CanvasRenderer>();
					}
				}
				return m_CanvasRenderer;
			}
		}
		public virtual Material defaultMaterial
		{
			get { return defaultGraphicMaterial; }
		}
		public virtual Material material
		{
			get{ return (m_Material != null) ? m_Material : defaultMaterial; }
			set
			{
				if( m_Material == value)
				{
					return;
				}
				m_Material = value;
				SetMaterialDirty();
			}
		}
		public virtual Material materialForRendering
		{
			get
			{
				var components = ListPool<UnityEngine.UI.IMaterialModifier>.Get();
				GetComponents<UnityEngine.UI.IMaterialModifier>( components);
				var currentMat = material;
				
				for( var i = 0; i < components.Count; i++)
				{
					currentMat = (components[i] as UnityEngine.UI.IMaterialModifier).GetModifiedMaterial( currentMat);
				}
				ListPool<UnityEngine.UI.IMaterialModifier>.Release( components);
				return currentMat;
			}
		}
		public virtual Texture mainTexture
		{
			get{ return s_WhiteTexture; }
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			CacheCanvas();
			GraphicRaycaster?.RegisterGraphicForCanvas( canvas, this);
		#if UNITY_EDITOR
			GraphicRebuildTracker.TrackGraphic(this);
		#endif
			if( s_WhiteTexture == null)
			{
				s_WhiteTexture = Texture2D.whiteTexture;
			}
			SetAllDirty();
		}
		protected override void OnDisable()
		{
		#if UNITY_EDITOR
			GraphicRebuildTracker.UnTrackGraphic( this);
		#endif
			GraphicRaycaster?.DisableGraphicForCanvas( canvas, this);
			UnityEngine.UI.CanvasUpdateRegistry.DisableCanvasElementForRebuild( this);
			
			if (canvasRenderer != null)
			{
				canvasRenderer.Clear();
			}
			UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild( rectTransform);
			base.OnDisable();
		}
		protected override void OnDestroy()
		{
		#if UNITY_EDITOR
			GraphicRebuildTracker.UnTrackGraphic( this);
		#endif
			GraphicRaycaster?.UnregisterGraphicForCanvas( canvas, this);
			UnityEngine.UI.CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild( this);
			
			if( m_CachedMesh)
			{
				Destroy(m_CachedMesh);
			}
			m_CachedMesh = null;
			base.OnDestroy();
		}
		protected override void OnCanvasHierarchyChanged()
		{
			Canvas currentCanvas = m_Canvas;
			m_Canvas = null;
			
			if( !IsActive())
			{
				GraphicRaycaster?.UnregisterGraphicForCanvas( currentCanvas, this);
				return;
			}
			CacheCanvas();
			
			if( currentCanvas != m_Canvas)
			{
				GraphicRaycaster?.UnregisterGraphicForCanvas( currentCanvas, this);
				
				if (IsActive())
				{
					GraphicRaycaster?.RegisterGraphicForCanvas( canvas, this);
				}
			}
		}
		public virtual void OnCullingChanged()
		{
			if( !canvasRenderer.cull && (m_VertsDirty || m_MaterialDirty))
			{
				UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
			}
		}
		public virtual void Rebuild( UnityEngine.UI.CanvasUpdate update)
		{
			if( canvasRenderer == null || canvasRenderer.cull)
			{
				return;
			}
			switch( update)
			{
				case UnityEngine.UI.CanvasUpdate.PreRender:
				{
					if( m_VertsDirty)
					{
						UpdateGeometry();
						m_VertsDirty = false;
					}
					if( m_MaterialDirty)
					{
						UpdateMaterial();
						m_MaterialDirty = false;
					}
					break;
				}
			}
		}
		public virtual void LayoutComplete()
		{
		}
		public virtual void GraphicUpdateComplete()
		{
		}
		protected virtual void UpdateMaterial()
		{
			if (!IsActive())
			{
				return;
			}
			canvasRenderer.materialCount = 1;
			canvasRenderer.SetMaterial( materialForRendering, 0);
			canvasRenderer.SetTexture( mainTexture);
		}
		protected virtual void UpdateGeometry()
		{
			if( useLegacyMeshGeneration)
			{
				DoLegacyMeshGeneration();
			}
			else
			{
				DoMeshGeneration();
			}
		}
		private void DoMeshGeneration()
		{
			if( rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
			{
				OnPopulateMesh( s_VertexHelper);
			}
			else
			{
				s_VertexHelper.Clear();
			}
			var components = ListPool<Component>.Get();
			GetComponents( typeof(UnityEngine.UI.IMeshModifier), components);
			
			for( var i = 0; i < components.Count; i++)
			{
				((UnityEngine.UI.IMeshModifier)components[ i]).ModifyMesh( s_VertexHelper);
			}
			ListPool<Component>.Release( components);
			s_VertexHelper.FillMesh( workerMesh);
			canvasRenderer.SetMesh( workerMesh);
		}
		private void DoLegacyMeshGeneration()
		{
			if( rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
			{
			#pragma warning disable 618
				OnPopulateMesh( workerMesh);
			#pragma warning restore 618
			}
			else
			{
				workerMesh.Clear();
			}
			var components = ListPool<Component>.Get();
			GetComponents(typeof(UnityEngine.UI.IMeshModifier), components);
			
			for (var i = 0; i < components.Count; i++)
			{
			#pragma warning disable 618
				((UnityEngine.UI.IMeshModifier)components[i]).ModifyMesh(workerMesh);
			#pragma warning restore 618
			}
			ListPool<Component>.Release(components);
			canvasRenderer.SetMesh(workerMesh);
		}
		protected static Mesh workerMesh
		{
			get
			{
				if( s_Mesh == null)
				{
					s_Mesh = new Mesh
					{
						name = "Shared UI Mesh"
					};
				}
				return s_Mesh;
			}
		}
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[Obsolete("Use OnPopulateMesh instead.", true)]
		protected virtual void OnFillVBO(System.Collections.Generic.List<UIVertex> vbo) {}
		
		[Obsolete("Use OnPopulateMesh(VertexHelper vh) instead.", false)]
		protected virtual void OnPopulateMesh(Mesh m)
		{
			OnPopulateMesh(s_VertexHelper);
			s_VertexHelper.FillMesh(m);
		}
		protected virtual void OnPopulateMesh( UnityEngine.UI.VertexHelper vh)
		{
			var r = GetPixelAdjustedRect();
			var v = new Vector4( r.x, r.y, r.x + r.width, r.y + r.height);
			Color32 color32 = color;
			
			vh.Clear();
			vh.AddVert( new Vector3( v.x, v.y), color32, new Vector2( 0f, 0f));
			vh.AddVert( new Vector3( v.x, v.w), color32, new Vector2( 0f, 1f));
			vh.AddVert( new Vector3( v.z, v.w), color32, new Vector2( 1f, 1f));
			vh.AddVert( new Vector3( v.z, v.y), color32, new Vector2( 1f, 0f));
			vh.AddTriangle( 0, 1, 2);
			vh.AddTriangle( 2, 3, 0);
		}
	#if UNITY_EDITOR
		public virtual void OnRebuildRequested()
		{
			m_SkipLayoutUpdate = true;
			var mbs = gameObject.GetComponents<MonoBehaviour>();
			
			foreach (var mb in mbs)
			{
				if( mb == null)
				{
					continue;
				}
				var methodInfo = mb.GetType().GetMethod( "OnValidate", 
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				
				if (methodInfo != null)
				{
					methodInfo.Invoke(mb, null);
				}
			}
			m_SkipLayoutUpdate = false;
		}
		protected override void Reset()
		{
			SetAllDirty();
		}
	#endif
		protected override void OnDidApplyAnimationProperties()
		{
			SetAllDirty();
		}
		public virtual void SetNativeSize()
		{
		}
		public virtual bool Raycast(Vector2 sp, Camera eventCamera)
		{
			return Raycast( sp, eventCamera, false);
		}
		protected bool Raycast(Vector2 sp, Camera eventCamera, bool ignoreMasks)
		{
			if (!isActiveAndEnabled)
			{
				return false;
			}
			var t = transform;
			var components = ListPool<Component>.Get();
			bool ignoreParentGroups = false;
			bool continueTraversal = true;
			
			while( t != null)
			{
				t.GetComponents(components);
				
				for (var i = 0; i < components.Count; i++)
				{
					var canvas = components[i] as Canvas;
					
					if (canvas != null && canvas.overrideSorting)
					{
						continueTraversal = false;
					}
					var filter = components[i] as ICanvasRaycastFilter;
					
					if (filter == null)
					{
						continue;
					}
					if (ignoreMasks && components[i] is Mask or RectMask2D)
					{
						continue;
					}
					var raycastValid = true;
					var group = components[i] as CanvasGroup;
					
					if (group != null)
					{
						if (!group.enabled)
						{
							continue;
						}
						if (ignoreParentGroups == false && group.ignoreParentGroups)
						{
							ignoreParentGroups = true;
							raycastValid = filter.IsRaycastLocationValid(sp, eventCamera);
						}
						else if (!ignoreParentGroups)
						{
							raycastValid = filter.IsRaycastLocationValid(sp, eventCamera);
						}
					}
					else
					{
						raycastValid = filter.IsRaycastLocationValid(sp, eventCamera);
					}
					if (!raycastValid)
					{
						ListPool<Component>.Release(components);
						return false;
					}
				}
				t = continueTraversal ? t.parent : null;
			}
			ListPool<Component>.Release(components);
			return true;
		}
	#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			SetAllDirty();
		}
	#endif
		public Vector2 PixelAdjustPoint(Vector2 point)
		{
			if (!canvas || canvas.renderMode == RenderMode.WorldSpace || canvas.scaleFactor == 0.0f || !canvas.pixelPerfect)
			{
				return point;
			}
			else
			{
				return RectTransformUtility.PixelAdjustPoint(point, transform, canvas);
			}
		}
		public Rect GetPixelAdjustedRect()
		{
			if (!canvas || canvas.renderMode == RenderMode.WorldSpace || canvas.scaleFactor == 0.0f || !canvas.pixelPerfect)
			{
				return rectTransform.rect;
			}
			else
			{
				return RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
			}
		}
		public virtual void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
		{
			CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, true);
		}
		public virtual void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
		{
			if (canvasRenderer == null || (!useRGB && !useAlpha))
			{
				return;
			}
			Color currentColor = canvasRenderer.GetColor();
			
			if (currentColor.Equals(targetColor))
			{
				m_ColorTweenRunner.StopTween();
				return;
			}
			ColorTween.ColorTweenMode mode = (useRGB && useAlpha)?
				ColorTween.ColorTweenMode.All :
				(useRGB ? ColorTween.ColorTweenMode.RGB : ColorTween.ColorTweenMode.Alpha);
			
			var colorTween = new ColorTween {duration = duration, startColor = canvasRenderer.GetColor(), targetColor = targetColor};
			colorTween.AddOnChangedCallback(canvasRenderer.SetColor);
			colorTween.ignoreTimeScale = ignoreTimeScale;
			colorTween.tweenMode = mode;
			m_ColorTweenRunner.StartTween(colorTween);
		}
		static private Color CreateColorFromAlpha(float alpha)
		{
			var alphaColor = Color.black;
			alphaColor.a = alpha;
			return alphaColor;
		}
		public virtual void CrossFadeAlpha( float alpha, float duration, bool ignoreTimeScale)
		{
			CrossFadeColor( CreateColorFromAlpha( alpha), duration, ignoreTimeScale, true, false);
		}
		public void RegisterDirtyLayoutCallback(UnityAction action)
		{
			m_OnDirtyLayoutCallback += action;
		}
		public void UnregisterDirtyLayoutCallback(UnityAction action)
		{
			m_OnDirtyLayoutCallback -= action;
		}
		public void RegisterDirtyVerticesCallback(UnityAction action)
		{
			m_OnDirtyVertsCallback += action;
		}
		public void UnregisterDirtyVerticesCallback(UnityAction action)
		{
			m_OnDirtyVertsCallback -= action;
		}
		public void RegisterDirtyMaterialCallback(UnityAction action)
		{
			m_OnDirtyMaterialCallback += action;
		}
		public void UnregisterDirtyMaterialCallback(UnityAction action)
		{
			m_OnDirtyMaterialCallback -= action;
		}
		static protected Material s_DefaultUI = null;
		static protected Texture2D s_WhiteTexture = null;
		
		[SerializeField, FormerlySerializedAs( "m_Mat")]
		protected Material m_Material;
		[SerializeField]
		private Color m_Color = Color.white;
		[NonSerialized]
		protected bool m_SkipLayoutUpdate;
		[NonSerialized]
		protected bool m_SkipMaterialUpdate;
		[SerializeField]
		bool m_RaycastTarget = true;
		[SerializeField]
		Vector4 m_RaycastPadding;
		
		[NonSerialized]
		GraphicRaycaster m_GraphicRaycaster;
		[NonSerialized]
		bool m_RaycastTargetCache = true;
		
		[NonSerialized]
		RectTransform m_RectTransform;
		[NonSerialized]
		CanvasRenderer m_CanvasRenderer;
		[NonSerialized]
		Canvas m_Canvas;
		
		[NonSerialized]
		bool m_VertsDirty;
		[NonSerialized]
		bool m_MaterialDirty;
		
		[NonSerialized]
		protected UnityAction m_OnDirtyLayoutCallback;
		[NonSerialized]
		protected UnityAction m_OnDirtyVertsCallback;
		[NonSerialized]
		protected UnityAction m_OnDirtyMaterialCallback;
		
		[NonSerialized]
		protected static Mesh s_Mesh;
		[NonSerialized]
		static readonly UnityEngine.UI.VertexHelper s_VertexHelper = new();
		
		[NonSerialized]
		protected Mesh m_CachedMesh;
		[NonSerialized]
		protected Vector2[] m_CachedUvs;
		
		[NonSerialized]
		private readonly TweenRunner<ColorTween> m_ColorTweenRunner;
	}
}
