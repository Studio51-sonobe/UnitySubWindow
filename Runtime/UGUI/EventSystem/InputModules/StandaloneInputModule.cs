using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace MultiWindow.EventSystems
{
	[AddComponentMenu("MultiWindow/Event/Standalone Input Module")]
	public class StandaloneInputModule : PointerInputModule
	{
		float m_PrevActionTime;
		Vector2 m_LastMoveVector;
		int m_ConsecutiveMoveCount = 0;
		Vector2 m_LastMousePosition;
		Vector2 m_MousePosition;
		GameObject m_CurrentFocusedGameObject;
		readonly Dictionary<int, PointerEventData> m_InputPointerEvents = new();
		
		const float doubleClickTime = 0.3f;
		
		protected StandaloneInputModule()
		{
		}
		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public enum InputMode
		{
			Mouse,
			Buttons
		}
		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public InputMode inputMode
		{
			get { return InputMode.Mouse; }
		}
		[SerializeField]
		private string m_HorizontalAxis = "Horizontal";
		[SerializeField]
		private string m_VerticalAxis = "Vertical";
		[SerializeField]
		private string m_SubmitButton = "Submit";
		[SerializeField]
		private string m_CancelButton = "Cancel";
		[SerializeField]
		private float m_InputActionsPerSecond = 10;
		[SerializeField]
		private float m_RepeatDelay = 0.5f;
		[SerializeField]
		[FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
		[HideInInspector]
		private bool m_ForceModuleActive;
		
		[Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
		public bool allowActivationOnMobileDevice
		{
			get { return m_ForceModuleActive; }
			set { m_ForceModuleActive = value; }
		}
		[Obsolete("forceModuleActive has been deprecated. There is no need to force the module awake as StandaloneInputModule works for all platforms")]
		public bool forceModuleActive
		{
			get { return m_ForceModuleActive; }
			set { m_ForceModuleActive = value; }
		}
		public float inputActionsPerSecond
		{
			get { return m_InputActionsPerSecond; }
			set { m_InputActionsPerSecond = value; }
		}
		public float repeatDelay
		{
			get { return m_RepeatDelay; }
			set { m_RepeatDelay = value; }
		}
		public string horizontalAxis
		{
			get { return m_HorizontalAxis; }
			set { m_HorizontalAxis = value; }
		}
		public string verticalAxis
		{
			get { return m_VerticalAxis; }
			set { m_VerticalAxis = value; }
		}
		public string submitButton
		{
			get { return m_SubmitButton; }
			set { m_SubmitButton = value; }
		}
		public string cancelButton
		{
			get { return m_CancelButton; }
			set { m_CancelButton = value; }
		}
		private bool ShouldIgnoreEventsOnNoFocus()
		{
		#if UNITY_EDITOR
			return !UnityEditor.EditorApplication.isRemoteConnected;
		#else
			return true;
		#endif
		}
		public override void UpdateModule()
		{
			if( !eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
			{
				ReleasePointerDrags();
				return;
			}
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = input.mousePosition;
		}
		private void ReleasePointerDrags()
		{
			using (ListPool<int>.Get(out var pointerIds))
			{
				foreach (var key in m_InputPointerEvents.Keys)
				{
					pointerIds.Add(key);
				}
				foreach (var pointerId in pointerIds)
				{
					if (!m_InputPointerEvents.TryGetValue(pointerId, out var inputPointerEvent))
					{
						continue;
					}
					if (inputPointerEvent != null && inputPointerEvent.pointerDrag != null && inputPointerEvent.dragging)
					{
						ReleaseMouse(inputPointerEvent, inputPointerEvent.pointerCurrentRaycast.gameObject);
					}
				}
			}
			m_InputPointerEvents.Clear();
		}
		private void ReleaseMouse(PointerEventData pointerEvent, GameObject currentOverGo)
		{
			ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
			var pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
			
			if (pointerEvent.pointerClick == pointerClickHandler && pointerEvent.eligibleForClick)
			{
				ExecuteEvents.Execute(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
			}
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
			}
			pointerEvent.eligibleForClick = false;
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
			pointerEvent.pointerClick = null;
			
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.dragging = false;
			pointerEvent.pointerDrag = null;
			
			if (currentOverGo != pointerEvent.pointerEnter)
			{
				HandlePointerExitAndEnter(pointerEvent, null);
				HandlePointerExitAndEnter(pointerEvent, currentOverGo);
			}
			m_InputPointerEvents[pointerEvent.pointerId] = pointerEvent;
		}
		public override bool ShouldActivateModule()
		{
			if (!base.ShouldActivateModule())
			{
				return false;
			}
			var shouldActivate = m_ForceModuleActive;
			shouldActivate |= input.GetButtonDown(m_SubmitButton);
			shouldActivate |= input.GetButtonDown(m_CancelButton);
			shouldActivate |= !Mathf.Approximately(input.GetAxisRaw(m_HorizontalAxis), 0.0f);
			shouldActivate |= !Mathf.Approximately(input.GetAxisRaw(m_VerticalAxis), 0.0f);
			shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
			shouldActivate |= input.GetMouseButtonDown(0);
			
			if (input.touchCount > 0)
			{
				shouldActivate = true;
			}
			return shouldActivate;
		}
		public override void ActivateModule()
		{
			if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
			{
				return;
			}
			base.ActivateModule();
			m_MousePosition = input.mousePosition;
			m_LastMousePosition = input.mousePosition;
			
			var toSelect = eventSystem.currentSelectedGameObject;
			if( toSelect == null)
			{ 
				toSelect = eventSystem.firstSelectedGameObject;
			}
			eventSystem.SetSelectedGameObject( toSelect, GetBaseEventData());
		}
		public override void DeactivateModule()
		{
			base.DeactivateModule();
			ClearSelection();
		}
		public override void Process()
		{
			if( !eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
			{
				return;
			}
			bool usedEvent = SendUpdateEventToSelectedObject();
			
			if( !ProcessTouchEvents() && input.mousePresent)
			{
				ProcessMouseEvent();
			}
			if( eventSystem.sendNavigationEvents)
			{
				if( !usedEvent)
				{
					usedEvent |= SendMoveEventToSelectedObject();
				}
				if( !usedEvent)
				{
					SendSubmitEventToSelectedObject();
				}
			}
		}
		private bool ProcessTouchEvents()
		{
			for( int i = 0; i < input.touchCount; ++i)
			{
				Touch touch = input.GetTouch(i);
				
				if( touch.type == TouchType.Indirect)
				{
					continue;
				}
				bool released;
				bool pressed;
				var pointer = GetTouchPointerEventData(touch, out pressed, out released);
				
				ProcessTouchPress(pointer, pressed, released);
				
				if( !released)
				{
					ProcessMove(pointer);
					ProcessDrag(pointer);
				}
				else
				{
					RemovePointerData(pointer);
				}
			}
			return input.touchCount > 0;
		}
		protected void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
		{
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
			
			if( pressed)
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
				
				DeselectIfSelectionChanged(currentOverGo, pointerEvent);
				
				if (pointerEvent.pointerEnter != currentOverGo)
				{
					// send a pointer enter to the touched element if it isn't the one to select...
					HandlePointerExitAndEnter(pointerEvent, currentOverGo);
					pointerEvent.pointerEnter = currentOverGo;
				}
				var resetDiffTime = Time.unscaledTime - pointerEvent.clickTime;
				
				if (resetDiffTime >= doubleClickTime)
				{
					pointerEvent.clickCount = 0;
				}
				var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
				var newClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
				
				if (newPressed == null)
				{
					newPressed = newClick;
				}
				float time = Time.unscaledTime;
				
				if (newPressed == pointerEvent.lastPress)
				{
					var diffTime = time - pointerEvent.clickTime;
					if (diffTime < doubleClickTime)
					{
						++pointerEvent.clickCount;
					}
					else
					{
						pointerEvent.clickCount = 1;
					}
					pointerEvent.clickTime = time;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.pointerPress = newPressed;
				pointerEvent.rawPointerPress = currentOverGo;
				pointerEvent.pointerClick = newClick;
				pointerEvent.clickTime = time;
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
				
				if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
				}
			}
			if (released)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				var pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
				
				if (pointerEvent.pointerClick == pointerClickHandler && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
				}
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
				pointerEvent.pointerClick = null;
				
				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
				}
				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;
				
				ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
				pointerEvent.pointerEnter = null;
			}
			m_InputPointerEvents[pointerEvent.pointerId] = pointerEvent;
		}
		protected bool SendSubmitEventToSelectedObject()
		{
			if (eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			var data = GetBaseEventData();
			
			if (input.GetButtonDown(m_SubmitButton))
			{
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
			}
			if (input.GetButtonDown(m_CancelButton))
			{
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
			}
			return data.used;
		}
		private Vector2 GetRawMoveVector()
		{
			Vector2 move = Vector2.zero;
			move.x = input.GetAxisRaw( m_HorizontalAxis);
			move.y = input.GetAxisRaw( m_VerticalAxis);
			
			if( input.GetButtonDown(m_HorizontalAxis))
			{
				if( move.x < 0)
				{
					move.x = -1f;
				}
				if( move.x > 0)
				{
					move.x = 1f;
				}
			}
			if( input.GetButtonDown(m_VerticalAxis))
			{
				if( move.y < 0)
				{
					move.y = -1f;
				}
				if( move.y > 0)
				{
					move.y = 1f;
				}
			}
			return move;
		}
		protected bool SendMoveEventToSelectedObject()
		{
			float time = Time.unscaledTime;
			Vector2 movement = GetRawMoveVector();
			
			if( Mathf.Approximately( movement.x, 0f) && Mathf.Approximately( movement.y, 0f))
			{
				m_ConsecutiveMoveCount = 0;
				return false;
			}
			bool similarDir = Vector2.Dot( movement, m_LastMoveVector) > 0;
			
			if( similarDir && m_ConsecutiveMoveCount == 1)
			{
				if( time <= m_PrevActionTime + m_RepeatDelay)
				{
					return false;
				}
			}
			else
			{
				if( time <= m_PrevActionTime + 1f / m_InputActionsPerSecond)
					return false;
			}
			var axisEventData = GetAxisEventData( movement.x, movement.y, 0.6f);
			
			if( axisEventData.moveDir != UnityEngine.EventSystems.MoveDirection.None)
			{
				ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
				if( !similarDir)
				{
					m_ConsecutiveMoveCount = 0;
				}
				m_ConsecutiveMoveCount++;
				m_PrevActionTime = time;
				m_LastMoveVector = movement;
			}
			else
			{
				m_ConsecutiveMoveCount = 0;
			}
			return axisEventData.used;
		}
		protected void ProcessMouseEvent()
		{
			ProcessMouseEvent( 0);
		}
		[Obsolete("This method is no longer checked, overriding it with return true does nothing!")]
		protected virtual bool ForceAutoSelect()
		{
			return false;
		}
		protected void ProcessMouseEvent(int id)
		{
			var mouseData = GetMousePointerEventData( id);
			var leftButtonData = mouseData.GetButtonState( PointerEventData.InputButton.Left).eventData;
			
			m_CurrentFocusedGameObject = leftButtonData.buttonData.pointerCurrentRaycast.gameObject;
			
			ProcessMousePress( leftButtonData);
			ProcessMove( leftButtonData.buttonData);
			ProcessDrag( leftButtonData.buttonData);
			
			ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Right).eventData);
			ProcessDrag( mouseData.GetButtonState( PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Middle).eventData);
			ProcessDrag( mouseData.GetButtonState( PointerEventData.InputButton.Middle).eventData.buttonData);
			
			if (!Mathf.Approximately( leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
			{
				var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>( leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy( scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
			}
		}
		protected bool SendUpdateEventToSelectedObject()
		{
			if (eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			var data = GetBaseEventData();
			ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
			return data.used;
		}
		protected void ProcessMousePress(MouseButtonEventData data)
		{
			var pointerEvent = data.buttonData;
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
			
			if (data.PressedThisFrame())
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
				
				DeselectIfSelectionChanged(currentOverGo, pointerEvent);
				
				var resetDiffTime = Time.unscaledTime - pointerEvent.clickTime;
				if (resetDiffTime >= doubleClickTime)
				{
					pointerEvent.clickCount = 0;
				}
				var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
				var newClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
				
				if (newPressed == null)
				{
					newPressed = newClick;
				}
				float time = Time.unscaledTime;
				
				if (newPressed == pointerEvent.lastPress)
				{
					var diffTime = time - pointerEvent.clickTime;
					if (diffTime < doubleClickTime)
					{
						++pointerEvent.clickCount;
					}
					else
					{
						pointerEvent.clickCount = 1;
					}
					pointerEvent.clickTime = time;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.pointerPress = newPressed;
				pointerEvent.rawPointerPress = currentOverGo;
				pointerEvent.pointerClick = newClick;
				pointerEvent.clickTime = time;
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
				
				if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
				}
				m_InputPointerEvents[pointerEvent.pointerId] = pointerEvent;
			}
			if (data.ReleasedThisFrame())
			{
				ReleaseMouse(pointerEvent, currentOverGo);
			}
		}
		protected GameObject GetCurrentFocusedGameObject()
		{
			return m_CurrentFocusedGameObject;
		}
	}
}
