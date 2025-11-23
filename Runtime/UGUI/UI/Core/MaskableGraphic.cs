using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace MultiWindow.UI
{
	public abstract class MaskableGraphic : Graphic, UnityEngine.UI.IClippable, UnityEngine.UI.IMaskable, UnityEngine.UI.IMaterialModifier
	{
		[NonSerialized]
		protected bool m_ShouldRecalculateStencil = true;
		[NonSerialized]
		protected Material m_MaskMaterial;
		[NonSerialized]
		private RectMask2D m_ParentMask;
		[SerializeField]
		private bool m_Maskable = true;
		
		private bool m_IsMaskingGraphic = false;
		
		[NonSerialized]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[Obsolete("Not used anymore.", true)]
		protected bool m_IncludeForMasking = false;

		[Serializable]
		public class CullStateChangedEvent : UnityEvent<bool> {}

		// Event delegates triggered on click.
		[SerializeField]
		private CullStateChangedEvent m_OnCullStateChanged = new CullStateChangedEvent();

		/// <summary>
		/// Callback issued when culling changes.
		/// </summary>
		/// <remarks>
		/// Called whene the culling state of this MaskableGraphic either becomes culled or visible. You can use this to control other elements of your UI as culling happens.
		/// </remarks>
		public CullStateChangedEvent onCullStateChanged
		{
			get { return m_OnCullStateChanged; }
			set { m_OnCullStateChanged = value; }
		}

		/// <summary>
		/// Does this graphic allow masking.
		/// </summary>
		public bool maskable
		{
			get { return m_Maskable; }
			set
			{
				if (value == m_Maskable)
					return;
				m_Maskable = value;
				m_ShouldRecalculateStencil = true;
				SetMaterialDirty();
			}
		}


		/// <summary>
		/// Is this graphic the graphic on the same object as a Mask that is enabled.
		/// </summary>
		/// <remarks>
		/// If toggled ensure to call MaskUtilities.NotifyStencilStateChanged(this); manually as it changes how stenciles are calculated for this image.
		/// </remarks>
		public bool isMaskingGraphic
		{
			get { return m_IsMaskingGraphic; }
			set
			{
				if (value == m_IsMaskingGraphic)
					return;

				m_IsMaskingGraphic = value;
			}
		}

		[NonSerialized]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[Obsolete("Not used anymore", true)]
		protected bool m_ShouldRecalculate = true;

		[NonSerialized]
		protected int m_StencilValue;

		/// <summary>
		/// See IMaterialModifier.GetModifiedMaterial
		/// </summary>
		public virtual Material GetModifiedMaterial(Material baseMaterial)
		{
			var toUse = baseMaterial;

			if (m_ShouldRecalculateStencil)
			{
				if (maskable)
				{
					var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
					m_StencilValue = MaskUtilities.GetStencilDepth(transform, rootCanvas);
				}
				else
					m_StencilValue = 0;

				m_ShouldRecalculateStencil = false;
			}

			// if we have a enabled Mask component then it will
			// generate the mask material. This is an optimization
			// it adds some coupling between components though :(
			if (m_StencilValue > 0 && !isMaskingGraphic)
			{
				var maskMat = UnityEngine.UI.StencilMaterial.Add(toUse, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
				UnityEngine.UI.StencilMaterial.Remove(m_MaskMaterial);
				m_MaskMaterial = maskMat;
				toUse = m_MaskMaterial;
			}
			return toUse;
		}

		/// <summary>
		/// See IClippable.Cull
		/// </summary>
		public virtual void Cull(Rect clipRect, bool validRect)
		{
			var cull = !validRect || !clipRect.Overlaps(rootCanvasRect, true);
			UpdateCull(cull);
		}

		private void UpdateCull(bool cull)
		{
			if (canvasRenderer.cull != cull)
			{
				canvasRenderer.cull = cull;
				UISystemProfilerApi.AddMarker("MaskableGraphic.cullingChanged", this);
				m_OnCullStateChanged.Invoke(cull);
				OnCullingChanged();
			}
		}

		/// <summary>
		/// See IClippable.SetClipRect
		/// </summary>
		public virtual void SetClipRect(Rect clipRect, bool validRect)
		{
			if (validRect)
				canvasRenderer.EnableRectClipping(clipRect);
			else
				canvasRenderer.DisableRectClipping();
		}

		public virtual void SetClipSoftness(Vector2 clipSoftness)
		{
			canvasRenderer.clippingSoftness = clipSoftness;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			m_ShouldRecalculateStencil = true;
			UpdateClipParent();
			SetMaterialDirty();

			if (isMaskingGraphic)
			{
				MaskUtilities.NotifyStencilStateChanged(this);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			m_ShouldRecalculateStencil = true;
			SetMaterialDirty();
			UpdateClipParent();
			UnityEngine.UI.StencilMaterial.Remove(m_MaskMaterial);
			m_MaskMaterial = null;

			if (isMaskingGraphic)
			{
				MaskUtilities.NotifyStencilStateChanged(this);
			}
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			m_ShouldRecalculateStencil = true;
			UpdateClipParent();
			SetMaterialDirty();
		}

#endif

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();

			if (!isActiveAndEnabled)
				return;

			m_ShouldRecalculateStencil = true;
			UpdateClipParent();
			SetMaterialDirty();
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[Obsolete("Not used anymore.", true)]
		public virtual void ParentMaskStateChanged() {}

		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged();

			if (!isActiveAndEnabled)
				return;

			m_ShouldRecalculateStencil = true;
			UpdateClipParent();
			SetMaterialDirty();
		}

		readonly Vector3[] m_Corners = new Vector3[4];
		private Rect rootCanvasRect
		{
			get
			{
				rectTransform.GetWorldCorners(m_Corners);

				if (canvas)
				{
					Matrix4x4 mat = canvas.rootCanvas.transform.worldToLocalMatrix;
					for (int i = 0; i < 4; ++i)
						m_Corners[i] = mat.MultiplyPoint(m_Corners[i]);
				}

				// bounding box is now based on the min and max of all corners (case 1013182)

				Vector2 min = m_Corners[0];
				Vector2 max = m_Corners[0];
				for (int i = 1; i < 4; i++)
				{
					min.x = Mathf.Min(m_Corners[i].x, min.x);
					min.y = Mathf.Min(m_Corners[i].y, min.y);
					max.x = Mathf.Max(m_Corners[i].x, max.x);
					max.y = Mathf.Max(m_Corners[i].y, max.y);
				}

				return new Rect(min, max - min);
			}
		}

		private void UpdateClipParent()
		{
			var newParent = (maskable && IsActive()) ? MaskUtilities.GetRectMaskForClippable(this) : null;

			// if the new parent is different OR is now inactive
			if (m_ParentMask != null && (newParent != m_ParentMask || !newParent.IsActive()))
			{
				m_ParentMask.RemoveClippable(this);
				UpdateCull(false);
			}

			// don't re-add it if the newparent is inactive
			if (newParent != null && newParent.IsActive())
				newParent.AddClippable(this);

			m_ParentMask = newParent;
		}

		/// <summary>
		/// See IClippable.RecalculateClipping
		/// </summary>
		public virtual void RecalculateClipping()
		{
			UpdateClipParent();
		}

		/// <summary>
		/// See IMaskable.RecalculateMasking
		/// </summary>
		public virtual void RecalculateMasking()
		{
			// Remove the material reference as either the graphic of the mask has been enable/ disabled.
			// This will cause the material to be repopulated from the original if need be. (case 994413)
			UnityEngine.UI.StencilMaterial.Remove(m_MaskMaterial);
			m_MaskMaterial = null;
			m_ShouldRecalculateStencil = true;
			SetMaterialDirty();
		}

		/// <inheritdoc/>
		public override bool Raycast(Vector2 sp, Camera eventCamera) => Raycast(sp, eventCamera, !maskable);
	}
}
