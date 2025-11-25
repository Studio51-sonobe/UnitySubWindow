using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MultiWindow.EventSystems
{
	[AddComponentMenu("MultiWindow/Event/Event Trigger")]
	public class EventTrigger :
		MonoBehaviour,
		IPointerEnterHandler,
		IPointerExitHandler,
		IPointerDownHandler,
		IPointerUpHandler,
		IPointerClickHandler,
		IInitializePotentialDragHandler,
		IBeginDragHandler,
		IDragHandler,
		IEndDragHandler,
		IDropHandler,
		IScrollHandler,
		IUpdateSelectedHandler,
		ISelectHandler,
		IDeselectHandler,
		IMoveHandler,
		ISubmitHandler,
		ICancelHandler
	{
		[Serializable]
		public class TriggerEvent : UnityEvent<BaseEventData>
		{
		}
		[Serializable]
		public class Entry
		{
			public UnityEngine.EventSystems.EventTriggerType eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
			public TriggerEvent callback = new TriggerEvent();
		}
		[FormerlySerializedAs("delegates")]
		[SerializeField]
		private List<Entry> m_Delegates;
		
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[Obsolete("Please use triggers instead (UnityUpgradable) -> triggers", true)]
		public List<Entry> delegates { get { return triggers; } set { triggers = value; } }
		
		protected EventTrigger()
		{
		}
		public List<Entry> triggers
		{
			get
			{
				if( m_Delegates == null)
				{
					m_Delegates = new List<Entry>();
				}
				return m_Delegates;
			}
			set { m_Delegates = value; }
		}
		private void Execute( UnityEngine.EventSystems.EventTriggerType id, BaseEventData eventData)
		{
			for (int i = 0; i < triggers.Count; ++i)
			{
				var ent = triggers[i];
				if (ent.eventID == id && ent.callback != null)
					ent.callback.Invoke(eventData);
			}
		}
		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.PointerEnter, eventData);
		}
		public virtual void OnPointerExit(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.PointerExit, eventData);
		}
		public virtual void OnDrag(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Drag, eventData);
		}
		public virtual void OnDrop(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Drop, eventData);
		}
		public virtual void OnPointerDown(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.PointerDown, eventData);
		}
		public virtual void OnPointerUp(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.PointerUp, eventData);
		}
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.PointerClick, eventData);
		}
		public virtual void OnSelect(BaseEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Select, eventData);
		}
		public virtual void OnDeselect(BaseEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Deselect, eventData);
		}
		public virtual void OnScroll(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Scroll, eventData);
		}
		public virtual void OnMove(AxisEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Move, eventData);
		}
		public virtual void OnUpdateSelected(BaseEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.UpdateSelected, eventData);
		}
		public virtual void OnInitializePotentialDrag(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.InitializePotentialDrag, eventData);
		}
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.BeginDrag, eventData);
		}
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.EndDrag, eventData);
		}
		public virtual void OnSubmit(BaseEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Submit, eventData);
		}
		public virtual void OnCancel(BaseEventData eventData)
		{
			Execute( UnityEngine.EventSystems.EventTriggerType.Cancel, eventData);
		}
	}
}
