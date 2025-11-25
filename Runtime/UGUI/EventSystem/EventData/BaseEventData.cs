
using UnityEngine;

namespace MultiWindow.EventSystems
{
	public abstract class AbstractEventData
	{
		protected bool m_Used;
		
		public virtual void Reset()
		{
			m_Used = false;
		}
		public virtual void Use()
		{
			m_Used = true;
		}
		public virtual bool used
		{
			get { return m_Used; }
		}
	}
	public class BaseEventData : AbstractEventData
	{
		public BaseEventData( EventSystem eventSystem)
		{
			m_EventSystem = eventSystem;
		}
		public EventSystem EventSystem
		{
			get{ return m_EventSystem; }
		}
		public BaseInputModule currentInputModule
		{
			get { return m_EventSystem.currentInputModule; }
		}
		public GameObject selectedObject
		{
			get { return m_EventSystem.currentSelectedGameObject; }
			set { m_EventSystem.SetSelectedGameObject( value, this); }
		}
		readonly EventSystem m_EventSystem;
	}
}
