
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiWindow.EventSystems
{
	[AddComponentMenu("MultiWindow/Event/Event System")]
	[DisallowMultipleComponent]
	public class EventSystem : UnityEngine.EventSystems.UIBehaviour
	{
		public bool sendNavigationEvents
		{
			get { return m_sendNavigationEvents; }
			set { m_sendNavigationEvents = value; }
		}
		public int pixelDragThreshold
		{
			get { return m_DragThreshold; }
			set { m_DragThreshold = value; }
		}
		public BaseInputModule currentInputModule
		{
			get { return m_CurrentInputModule; }
		}
		public GameObject firstSelectedGameObject
		{
			get { return m_FirstSelected; }
			set { m_FirstSelected = value; }
		}
		public GameObject currentSelectedGameObject
		{
			get { return m_CurrentSelected; }
		}
		[Obsolete("lastSelectedGameObject is no longer supported")]
		public GameObject lastSelectedGameObject
		{
			get { return null; }
		}
		public bool isFocused
		{
			get { return m_HasFocus; }
		}
		protected EventSystem()
		{
		}
		public void UpdateModules()
		{
			GetComponents(m_SystemInputModules);
			
			var systemInputModulesCount = m_SystemInputModules.Count;
			
			for( int i = systemInputModulesCount - 1; i >= 0; i--)
			{
				if (m_SystemInputModules[i] && m_SystemInputModules[i].IsActive())
				{
					continue;
				}
				m_SystemInputModules.RemoveAt(i);
			}
		}
		public bool alreadySelecting
		{
			get { return m_SelectionGuard; }
		}
		public void SetSelectedGameObject( GameObject selected, BaseEventData pointer)
		{
			if( m_SelectionGuard)
			{
				Debug.LogError( "Attempting to select " + selected +  "while already selecting an object.");
				return;
			}
			m_SelectionGuard = true;
			
			if( selected == m_CurrentSelected)
			{
				m_SelectionGuard = false;
				return;
			}
			ExecuteEvents.Execute( m_CurrentSelected, pointer, ExecuteEvents.deselectHandler);
			m_CurrentSelected = selected;
			ExecuteEvents.Execute( m_CurrentSelected, pointer, ExecuteEvents.selectHandler);
			m_SelectionGuard = false;
		}
		BaseEventData baseEventDataCache
		{
			get
			{
				if( m_DummyData == null)
				{
					m_DummyData = new BaseEventData( this);
				}
				return m_DummyData;
			}
		}
		public void SetSelectedGameObject( GameObject selected)
		{
			SetSelectedGameObject( selected, baseEventDataCache);
		}
		static int RaycastComparer( RaycastResult lhs, RaycastResult rhs)
		{
			if( lhs.module != rhs.module)
			{
				var lhsEventCamera = lhs.module.eventCamera;
				var rhsEventCamera = rhs.module.eventCamera;
				
				if( lhsEventCamera != null && rhsEventCamera != null && lhsEventCamera.depth != rhsEventCamera.depth)
				{
					if( lhsEventCamera.depth < rhsEventCamera.depth)
					{
						return 1;
					}
					if( lhsEventCamera.depth == rhsEventCamera.depth)
					{
						return 0;
					}
					return -1;
				}
				if( lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
				{
					return rhs.module.sortOrderPriority.CompareTo( lhs.module.sortOrderPriority);
				}
				if( lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
				{
					return rhs.module.renderOrderPriority.CompareTo( lhs.module.renderOrderPriority);
				}
			}
			if( lhs.sortingLayer != rhs.sortingLayer)
			{
				var rid = SortingLayer.GetLayerValueFromID( rhs.sortingLayer);
				var lid = SortingLayer.GetLayerValueFromID( lhs.sortingLayer);
				return rid.CompareTo( lid);
			}
			if( lhs.sortingOrder != rhs.sortingOrder)
			{
				return rhs.sortingOrder.CompareTo( lhs.sortingOrder);
			}
			if( lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster)
			{
				return rhs.depth.CompareTo( lhs.depth);
			}
			if( lhs.distance != rhs.distance)
			{
				return lhs.distance.CompareTo( rhs.distance);
			}
		#if PACKAGE_PHYSICS2D
			if( lhs.sortingGroupID != SortingGroup.invalidSortingGroupID && rhs.sortingGroupID != SortingGroup.invalidSortingGroupID)
			{
				if( lhs.sortingGroupID != rhs.sortingGroupID)
				{
					return lhs.sortingGroupID.CompareTo( rhs.sortingGroupID);
				}
				if( lhs.sortingGroupOrder != rhs.sortingGroupOrder)
				{
					return rhs.sortingGroupOrder.CompareTo( lhs.sortingGroupOrder);
				}
			}
		#endif
			return lhs.index.CompareTo( rhs.index);
		}
		public void RaycastAll( PointerEventData eventData, List<RaycastResult> raycastResults)
		{
			raycastResults.Clear();
			var modules = GetRaycasters();
			var modulesCount = modules.Count;
			
			for (int i = 0; i < modulesCount; ++i)
			{
				var module = modules[i];
				
				if (module == null || !module.IsActive())
				{
					continue;
				}
				module.Raycast( eventData, raycastResults);
			}
			raycastResults.Sort( s_RaycastComparer);
		}
		public bool IsPointerOverGameObject()
		{
			return IsPointerOverGameObject( PointerInputModule.kMouseLeftId);
		}
		public bool IsPointerOverGameObject( int pointerId)
		{
			return m_CurrentInputModule != null && m_CurrentInputModule.IsPointerOverGameObject( pointerId);
		}
		protected override void OnEnable()
		{
			base.OnEnable();
		}
		protected override void OnDisable()
		{
			if (m_CurrentInputModule != null)
			{
				m_CurrentInputModule.DeactivateModule();
				m_CurrentInputModule = null;
			}
			base.OnDisable();
		}
		protected override void Start()
		{
			base.Start();
		}
		private void TickModules()
		{
			var systemInputModulesCount = m_SystemInputModules.Count;
			for (var i = 0; i < systemInputModulesCount; i++)
			{
				if (m_SystemInputModules[i] != null)
				{
					m_SystemInputModules[i].UpdateModule();
				}
			}
		}
		protected virtual void OnApplicationFocus(bool hasFocus)
		{
			m_HasFocus = hasFocus;
			
			if (!m_HasFocus)
			{
				TickModules();
			}
		}
		protected virtual void Update()
		{
			TickModules();
			
			bool changedModule = false;
			var systemInputModulesCount = m_SystemInputModules.Count;
			
			for( int i0 = 0; i0 < systemInputModulesCount; ++i0)
			{
				var module = m_SystemInputModules[ i0];
				if (module.IsModuleSupported() && module.ShouldActivateModule())
				{
					if (m_CurrentInputModule != module)
					{
						ChangeEventModule(module);
						changedModule = true;
					}
					break;
				}
			}
			if( m_CurrentInputModule == null)
			{
				for( int i0 = 0; i0 < systemInputModulesCount; ++i0)
				{
					var module = m_SystemInputModules[ i0];
					
					if( module.IsModuleSupported() != false)
					{
						ChangeEventModule( module);
						changedModule = true;
						break;
					}
				}
			}
			if( changedModule == false && m_CurrentInputModule != null)
			{
				m_CurrentInputModule.Process();
			}
		}
		private void ChangeEventModule(BaseInputModule module)
		{
			if( m_CurrentInputModule == module)
			{
				return;
			}
			if( m_CurrentInputModule != null)
			{
				m_CurrentInputModule.DeactivateModule();
			}
			if( module != null)
			{
				module.ActivateModule();
			}
			m_CurrentInputModule = module;
		}
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("<b>Selected:</b>" + currentSelectedGameObject);
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine(m_CurrentInputModule != null ? m_CurrentInputModule.ToString() : "No module");
			return sb.ToString();
		}
		static readonly Comparison<RaycastResult> s_RaycastComparer = RaycastComparer;
		
		[SerializeField, FormerlySerializedAs( "m_Selected")]
		GameObject m_FirstSelected;
		[SerializeField]
		bool m_sendNavigationEvents = true;
		[SerializeField]
		int m_DragThreshold = 10;
		
		bool m_SelectionGuard;
		bool m_HasFocus = true;
		BaseEventData m_DummyData;
		GameObject m_CurrentSelected;
		BaseInputModule m_CurrentInputModule;
		readonly List<BaseInputModule> m_SystemInputModules = new();
		
		internal void AddRaycaster( BaseRaycaster baseRaycaster)
		{
			if( m_Raycasters.Contains( baseRaycaster) == false)
			{
				m_Raycasters.Add( baseRaycaster);
			}
		}
		internal void RemoveRaycasters( BaseRaycaster baseRaycaster)
		{
			if( m_Raycasters.Contains( baseRaycaster) != false)
			{
				m_Raycasters.Remove( baseRaycaster);
			}
		}
		public List<BaseRaycaster> GetRaycasters()
		{
			return m_Raycasters;
		}
		readonly List<BaseRaycaster> m_Raycasters = new();
		
	}
}
