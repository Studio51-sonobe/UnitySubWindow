
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
	#if false
		public static EventSystem current
		{
			get { return s_EventSystems.Count > 0 ? s_EventSystems[0] : null; }
			set
			{
				int index = s_EventSystems.IndexOf( value);
				if (index > 0)
				{
					s_EventSystems.RemoveAt( index);
					s_EventSystems.Insert(0, value);
				}
				else if (index < 0)
				{
					Debug.LogError("Failed setting EventSystem.current to unknown EventSystem " + value);
				}
			}
		}
	#endif
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
			var modules = RaycasterManager.GetRaycasters();
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
	#if PACKAGE_UITOOLKIT
		[SerializeField, HideInInspector]
		private UIToolkitInteroperabilityBridge m_UIToolkitInterop = new ();
		
		internal UIToolkitInteroperabilityBridge uiToolkitInterop => m_UIToolkitInterop;
	#endif
		internal bool isOverridingUIToolkitEvents
		{
			get
			{
			#if PACKAGE_UITOOLKIT
				return uiToolkitInterop.overrideUIToolkitEvents && UIDocument.EnabledDocumentCount > 0;
			#else
				return false;
			#endif
			}
		}
	#if PACKAGE_UITOOLKIT
		private struct UIToolkitOverrideConfigOld
		{
			public EventSystem activeEventSystem;
			public bool sendEvents;
			public bool createPanelGameObjectsOnStart;
		}
		private static UIToolkitOverrideConfigOld? s_UIToolkitOverrideConfigOld = null;
	#endif
		[Obsolete("Use PanelInputConfiguration component instead.")]
		public static void SetUITookitEventSystemOverride(EventSystem activeEventSystem, bool sendEvents = true, bool createPanelGameObjectsOnStart = true)
		{
		#if PACKAGE_UITOOLKIT
			s_UIToolkitOverrideConfigOld = activeEventSystem == null && sendEvents && createPanelGameObjectsOnStart ? null : new UIToolkitOverrideConfigOld
			{
				activeEventSystem = activeEventSystem,
				sendEvents = sendEvents,
				createPanelGameObjectsOnStart = createPanelGameObjectsOnStart,
			};
			var eventSystem = activeEventSystem != null ? activeEventSystem : EventSystem.current;
			
			if (UIElementsRuntimeUtility.activeEventSystem != null && UIElementsRuntimeUtility.activeEventSystem != eventSystem)
			{
				((EventSystem)UIElementsRuntimeUtility.activeEventSystem).uiToolkitInterop.overrideUIToolkitEvents = false;
			}
			if (eventSystem != null && eventSystem.isActiveAndEnabled)
			{
				eventSystem.uiToolkitInterop.overrideUIToolkitEvents = sendEvents;
				eventSystem.uiToolkitInterop.handlerTypes = createPanelGameObjectsOnStart ? (UIToolkitInteroperabilityBridge.EventHandlerTypes)~0 : 0;
			}
		#endif
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			s_EventSystems.Add(this);
		#if PACKAGE_UITOOLKIT
			if( s_UIToolkitOverrideConfigOld != null)
			{
				m_UIToolkitInterop = new();
				
				if (!s_UIToolkitOverrideConfigOld.Value.sendEvents)
				{
					m_UIToolkitInterop.overrideUIToolkitEvents = false;
				}
				if (!s_UIToolkitOverrideConfigOld.Value.createPanelGameObjectsOnStart)
				{
					m_UIToolkitInterop.handlerTypes = 0;
				}
			}
			m_UIToolkitInterop.eventSystem = this;
			m_UIToolkitInterop.OnEnable();
		#endif
		}
		protected override void OnDisable()
		{
		#if PACKAGE_UITOOLKIT
			m_UIToolkitInterop.OnDisable();
		#endif
			if (m_CurrentInputModule != null)
			{
				m_CurrentInputModule.DeactivateModule();
				m_CurrentInputModule = null;
			}
			s_EventSystems.Remove(this);
			base.OnDisable();
		}
		protected override void Start()
		{
			base.Start();
		#if PACKAGE_UITOOLKIT
			m_UIToolkitInterop.Start();
		#endif
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
		#if PACKAGE_UITOOLKIT
			m_UIToolkitInterop.Update();
		#endif
		#if false
			if (current != this)
			{
				return;
			}
		#endif
			TickModules();
			
			bool changedModule = false;
			var systemInputModulesCount = m_SystemInputModules.Count;
			
			for (var i = 0; i < systemInputModulesCount; i++)
			{
				var module = m_SystemInputModules[i];
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
			if (m_CurrentInputModule == null)
			{
				for (var i = 0; i < systemInputModulesCount; i++)
				{
					var module = m_SystemInputModules[i];
					if (module.IsModuleSupported())
					{
						ChangeEventModule(module);
						changedModule = true;
						break;
					}
				}
			}
			if (!changedModule && m_CurrentInputModule != null)
			{
				m_CurrentInputModule.Process();
			}
	#if false
		#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				int eventSystemCount = 0;
				for (int i = 0; i < m_EventSystems.Count; i++)
				{
					if (m_EventSystems[i].GetType() == typeof(EventSystem))
					{
						eventSystemCount++;
					}
				}
				if (eventSystemCount > 1)
				{
					Debug.LogWarning("There are " + eventSystemCount + " event systems in the scene. Please ensure there is always exactly one event system in the scene");
				}
			}
		#endif
	#endif
		}
		private void ChangeEventModule(BaseInputModule module)
		{
			if (m_CurrentInputModule == module)
			{
				return;
			}
			if (m_CurrentInputModule != null)
			{
				m_CurrentInputModule.DeactivateModule();
			}
			if (module != null)
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
		static readonly List<EventSystem> s_EventSystems = new();
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
		List<BaseInputModule> m_SystemInputModules = new();
	}
}
