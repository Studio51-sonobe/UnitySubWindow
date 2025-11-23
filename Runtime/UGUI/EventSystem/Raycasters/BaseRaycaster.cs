using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiWindow.EventSystems
{
	public abstract class BaseRaycaster : UnityEngine.EventSystems.UIBehaviour
	{
		public abstract void Raycast( PointerEventData eventData, List<RaycastResult> resultAppendList);
		
		public abstract Camera eventCamera
		{
			get;
		}
		[Obsolete( "Please use sortOrderPriority and renderOrderPriority", false)]
		public virtual int priority
		{
			get { return 0; }
		}
		public virtual int sortOrderPriority
		{
			get { return int.MinValue; }
		}
		public virtual int renderOrderPriority
		{
			get { return int.MinValue; }
		}
		public EventSystem EventSystem
		{
			get
			{
				if( m_EventSystem == null)
				{
					m_EventSystem = GetComponentInParent<EventSystem>();
				}
				return m_EventSystem;
			}
		}
		public BaseRaycaster rootRaycaster
		{
			get
			{
				if( m_RootRaycaster == null)
				{
					var baseRaycasters = GetComponentsInParent<BaseRaycaster>();
					
					if( baseRaycasters.Length != 0)
					{
						m_RootRaycaster = baseRaycasters[ baseRaycasters.Length - 1];
					}
				}
				return m_RootRaycaster;
			}
		}
		public override string ToString()
		{
			return "Name: " + gameObject + "\n" +
				"eventCamera: " + eventCamera + "\n" +
				"sortOrderPriority: " + sortOrderPriority + "\n" +
				"renderOrderPriority: " + renderOrderPriority;
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			
			if( m_EventSystem == null)
			{
				m_EventSystem = GetComponentInParent<EventSystem>();
			}
			m_EventSystem?.AddRaycaster( this);
		}
		protected override void OnDisable()
		{
			m_EventSystem?.RemoveRaycasters( this);
			base.OnDisable();
		}
		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged();
			m_EventSystem?.RemoveRaycasters( this);
			m_EventSystem = GetComponentInParent<EventSystem>();
			m_EventSystem?.AddRaycaster( this);
			m_RootRaycaster = null;
		}
		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			m_EventSystem?.RemoveRaycasters( this);
			m_EventSystem = GetComponentInParent<EventSystem>();
			m_EventSystem?.AddRaycaster( this);
			m_RootRaycaster = null;
		}
		EventSystem m_EventSystem;
		BaseRaycaster m_RootRaycaster;
	}
}
