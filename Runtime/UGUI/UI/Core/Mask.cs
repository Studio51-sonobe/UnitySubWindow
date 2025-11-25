using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace MultiWindow.UI
{
	[AddComponentMenu("MultiWindowUI/Mask", 13)]
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	[DisallowMultipleComponent]
	public class Mask : UIBehaviour, ICanvasRaycastFilter, UnityEngine.UI.IMaterialModifier
	{
		[NonSerialized]
		private RectTransform m_RectTransform;
		public RectTransform rectTransform
		{
			get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
		}
		[SerializeField]
		private bool m_ShowMaskGraphic = true;
		
		public bool showMaskGraphic
		{
			get { return m_ShowMaskGraphic; }
			set
			{
				if (m_ShowMaskGraphic == value)
				{
					return;
				}
				m_ShowMaskGraphic = value;
				
				if (graphic != null)
				{
					graphic.SetMaterialDirty();
				}
			}
		}
		[NonSerialized]
		private Graphic m_Graphic;
		
		public Graphic graphic
		{
			get { return m_Graphic ?? (m_Graphic = GetComponent<Graphic>()); }
		}
		[NonSerialized]
		private Material m_MaskMaterial;
		[NonSerialized]
		private Material m_UnmaskMaterial;
		
		protected Mask()
		{
		}
		public virtual bool MaskEnabled() { return IsActive() && graphic != null; }
		
		[Obsolete("Not used anymore.")]
		public virtual void OnSiblingGraphicEnabledDisabled() {}
		
		protected override void OnEnable()
		{
			base.OnEnable();
			if (graphic != null)
			{
				graphic.canvasRenderer.hasPopInstruction = true;
				graphic.SetMaterialDirty();
				
				if (graphic is MaskableGraphic)
				{
					(graphic as MaskableGraphic).isMaskingGraphic = true;
				}
			}
			MaskUtilities.NotifyStencilStateChanged(this);
		}
		protected override void OnDisable()
		{
			base.OnDisable();
			
			if (graphic != null)
			{
				graphic.SetMaterialDirty();
				graphic.canvasRenderer.hasPopInstruction = false;
				graphic.canvasRenderer.popMaterialCount = 0;
				
				if (graphic is MaskableGraphic)
				{
					(graphic as MaskableGraphic).isMaskingGraphic = false;
				}
			}
			UnityEngine.UI.StencilMaterial.Remove(m_MaskMaterial);
			m_MaskMaterial = null;
			UnityEngine.UI.StencilMaterial.Remove(m_UnmaskMaterial);
			m_UnmaskMaterial = null;
			MaskUtilities.NotifyStencilStateChanged(this);
		}
	#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			
			if (!IsActive())
			{
				return;
			}
			if (graphic != null)
			{
				if (graphic is MaskableGraphic)
				{
					(graphic as MaskableGraphic).isMaskingGraphic = true;
				}
				graphic.SetMaterialDirty();
			}
			MaskUtilities.NotifyStencilStateChanged(this);
		}
	#endif
		public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			if (!isActiveAndEnabled)
			{
				return true;
			}
			return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
		}
		public virtual Material GetModifiedMaterial(Material baseMaterial)
		{
			if (!MaskEnabled())
			{
				return baseMaterial;
			}
			var rootSortCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
			var stencilDepth = MaskUtilities.GetStencilDepth(transform, rootSortCanvas);
			if (stencilDepth >= 8)
			{
				Debug.LogWarning("Attempting to use a stencil mask with depth > 8", gameObject);
				return baseMaterial;
			}
			int desiredStencilBit = 1 << stencilDepth;
			
			if (desiredStencilBit == 1)
			{
				var maskMaterial = UnityEngine.UI.StencilMaterial.Add(baseMaterial, 1, StencilOp.Replace, CompareFunction.Always, m_ShowMaskGraphic ? ColorWriteMask.All : 0);
				UnityEngine.UI.StencilMaterial.Remove(m_MaskMaterial);
				m_MaskMaterial = maskMaterial;
				
				var unmaskMaterial = UnityEngine.UI.StencilMaterial.Add(baseMaterial, 1, StencilOp.Zero, CompareFunction.Always, 0);
				UnityEngine.UI.StencilMaterial.Remove(m_UnmaskMaterial);
				m_UnmaskMaterial = unmaskMaterial;
				graphic.canvasRenderer.popMaterialCount = 1;
				graphic.canvasRenderer.SetPopMaterial(m_UnmaskMaterial, 0);
				return m_MaskMaterial;
			}
			var maskMaterial2 = UnityEngine.UI.StencilMaterial.Add(baseMaterial, desiredStencilBit | (desiredStencilBit - 1), StencilOp.Replace, CompareFunction.Equal, m_ShowMaskGraphic ? ColorWriteMask.All : 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
			UnityEngine.UI.StencilMaterial.Remove(m_MaskMaterial);
			m_MaskMaterial = maskMaterial2;
			
			graphic.canvasRenderer.hasPopInstruction = true;
			var unmaskMaterial2 = UnityEngine.UI.StencilMaterial.Add(baseMaterial, desiredStencilBit - 1, StencilOp.Replace, CompareFunction.Equal, 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
			UnityEngine.UI.StencilMaterial.Remove(m_UnmaskMaterial);
			m_UnmaskMaterial = unmaskMaterial2;
			graphic.canvasRenderer.popMaterialCount = 1;
			graphic.canvasRenderer.SetPopMaterial(m_UnmaskMaterial, 0);
			return m_MaskMaterial;
		}
	}
}
