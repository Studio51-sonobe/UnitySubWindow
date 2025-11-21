
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace MultiWindow
{
	public sealed class WindowInputModule : BaseInputModule
	{
		internal int SubWindowIndex
		{
			get;
			set;
		} = -1;
		
		protected override void Awake()
		{
			base.Awake();
			OnInputEvent += OnInputEventMethod;
			pointerData = new PointerEventData( eventSystem);
		}
		protected override void OnDestroy()
		{
			OnInputEvent -= OnInputEventMethod;
			base.OnDestroy();
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			pointerData = new PointerEventData( eventSystem);
		}
		protected override void OnDisable()
		{
			base.OnDisable();
		}
		public override bool IsPointerOverGameObject( int pointerId)
		{
			return raycastResults.Count > 0 && raycastResults[ 0].gameObject != null;
		}
		public override void Process()
		{
		}
		public int pixelDragThreshold = 5;
		public float multiClickTime = 0.3f;
		
		GraphicRaycaster windowRaycaster;
		PointerEventData pointerData; // single mouse pointer per window
		readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
		bool lastLeftPressed;
		bool lastRightPressed;
		bool lastMiddlePressed;
		float injectedScrollDelta;
		int clickCount;
		float lastClickTime;
		GameObject lastClickObject;
		Vector2 injectedPosition = Vector2.zero;
		bool hasInjectedPosition = false;
		bool injectLeftDown = false;
		bool injectLeftUp = false;
		bool leftIsDown = false;
		bool injectRightDown = false;
		bool injectRightUp = false;
		bool rightIsDown = false;
		bool injectMiddleDown = false;
		bool injectMiddleUp = false;
		bool middleIsDown = false;
		readonly Queue<KeyEvent> injectedKeys = new Queue<KeyEvent>();
		bool hasFocus = true; // whether this window currently has focus (native should update)
		
		struct KeyEvent
		{
			public KeyCode key;
			public bool down;
		}
		public void InjectScroll( float scrollDelta)
		{
			injectedScrollDelta += scrollDelta;
		}
		public void InjectKey( KeyCode key, bool down)
		{
			injectedKeys.Enqueue( new KeyEvent { key = key, down = down });
		}
		public void InjectFocus( bool focused)
		{
			hasFocus = focused;
		}
		void ProcessInjectedKeys()
		{
			if( injectedKeys.Count == 0)
			{
				return;
			}
			while( injectedKeys.Count > 0)
			{
				var k = injectedKeys.Dequeue();
				var selected = eventSystem.currentSelectedGameObject;
				
				if( selected == null)
				{
					continue;
				}
				var ped = new BaseEventData( eventSystem);
				
				if( k.down != false)
				{
					if( k.key == KeyCode.Return || k.key == KeyCode.KeypadEnter)
					{
						ExecuteEvents.Execute( selected, ped, ExecuteEvents.submitHandler);
					}
					else if( k.key == KeyCode.Escape)
					{
						ExecuteEvents.Execute( selected, ped, ExecuteEvents.cancelHandler);
					}
					else
					{
						ExecuteEvents.Execute<IWindowKeyHandler>( selected, ped, (handler, data) => handler.OnKey( k.key, true));
					}
				}
				else
				{
					ExecuteEvents.Execute<IWindowKeyHandler>( selected, new BaseEventData( eventSystem), (handler, data) => handler.OnKey( k.key, false));
				}
			}
		}
		void ProcessPointerDown( PointerEventData.InputButton button, PointerEventData ped, List<RaycastResult> results)
		{
			ped.button = button;
			ped.pressPosition = ped.position;
			
			GameObject newPressed = null;
			
			if( results.Count > 0)
			{
				newPressed = results[ 0].gameObject;
			}
			ped.pointerPressRaycast = (results.Count > 0)? results[ 0] : new RaycastResult();
			
			GameObject handler = ExecuteEvents.ExecuteHierarchy( newPressed, ped, ExecuteEvents.pointerDownHandler);
			if( handler == null)
			{
				handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>( newPressed);
			}
			ped.pointerPress = handler;
			ped.rawPointerPress = newPressed;
			ped.eligibleForClick = true;
			ped.delta = Vector2.zero;
			
			if( newPressed == lastClickObject && Time.unscaledTime - lastClickTime < multiClickTime)
			{
				clickCount++;
			}
			else
			{
				clickCount = 1;
			}
			ped.clickCount = clickCount;
			lastClickTime = Time.unscaledTime;
			lastClickObject = newPressed;
			
			var potentialDrag = ExecuteEvents.GetEventHandler<IDragHandler>( newPressed);
			ped.pointerDrag = potentialDrag;
			ped.dragging = false;
			ped.useDragThreshold = true;
		}
		void ProcessPointerUp( PointerEventData.InputButton button, PointerEventData ped, List<RaycastResult> results)
		{
			ped.button = button;
			
			GameObject currentOverGo = (results.Count > 0) ? results[ 0].gameObject : null;
			
			ExecuteEvents.Execute( ped.pointerPress, ped, ExecuteEvents.pointerUpHandler);
			var clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>( currentOverGo);
			
			if( ped.pointerPress == clickHandler && ped.eligibleForClick != false)
			{
				ExecuteEvents.Execute( ped.pointerPress, ped, ExecuteEvents.pointerClickHandler);
			}
			else
			{
			}
			if( ped.dragging != false && ped.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy( currentOverGo, ped, ExecuteEvents.endDragHandler);
				ExecuteEvents.Execute( ped.pointerDrag, ped, ExecuteEvents.endDragHandler);
			}
			ped.eligibleForClick = false;
			ped.pointerPress = null;
			ped.rawPointerPress = null;
			ped.pointerDrag = null;
			ped.dragging = false;
		}
		internal void Process( Camera camera, GraphicRaycaster raycaster)
		{
			RenderTexture renderTexture = camera?.targetTexture ;
			windowRaycaster = raycaster;
			
			if( renderTexture == null || windowRaycaster == null)
			{
				return;
			}
			if( hasInjectedPosition != false)
			{
				float y = renderTexture.height - injectedPosition.y;
				pointerData.position = new Vector2( injectedPosition.x, y);
				pointerData.delta = pointerData.position - pointerData.pressPosition;
				hasInjectedPosition = false;
			}
			raycastResults.Clear();
			
			if( windowRaycaster != null)
			{
				windowRaycaster.Raycast(pointerData, raycastResults);
			}
			GameObject currentOverGo = (raycastResults.Count > 0) ? raycastResults[ 0].gameObject : null;
			
			HandlePointerExitAndEnter( pointerData, currentOverGo);
			
			if( Mathf.Abs( injectedScrollDelta) > float.Epsilon)
			{
				pointerData.scrollDelta = new Vector2(0, injectedScrollDelta);
				
				if( currentOverGo != null)
				{
					ExecuteEvents.ExecuteHierarchy( currentOverGo, pointerData, ExecuteEvents.scrollHandler);
				}
				injectedScrollDelta = 0;
				pointerData.scrollDelta = Vector2.zero;
			}
			if( injectLeftDown != false)
			{
				injectLeftDown = false;
				leftIsDown = true;
				ProcessPointerDown( PointerEventData.InputButton.Left, pointerData, raycastResults);
			}
			if( injectLeftUp != false)
			{
				injectLeftUp = false;
				leftIsDown = false;
				ProcessPointerUp( PointerEventData.InputButton.Left, pointerData, raycastResults);
			}
			if( injectRightDown != false)
			{
				injectRightDown = false;
				rightIsDown = true;
				ProcessPointerDown( PointerEventData.InputButton.Right, pointerData, raycastResults);
			}
			if( injectRightUp != false)
			{
				injectRightUp = false;
				rightIsDown = false;
				ProcessPointerUp( PointerEventData.InputButton.Right, pointerData, raycastResults);
			}
			if( injectMiddleDown != false)
			{
				injectMiddleDown = false;
				middleIsDown = true;
				ProcessPointerDown( PointerEventData.InputButton.Middle, pointerData, raycastResults);
			}
			if( injectMiddleUp != false)
			{
				injectMiddleUp = false;
				middleIsDown = false;
				ProcessPointerUp( PointerEventData.InputButton.Middle, pointerData, raycastResults);
			}
			ProcessInjectedKeys();
		}
		void LateUpdate()
		{
			if( leftIsDown != false || rightIsDown != false || middleIsDown != false)
			{
				ProcessMoveAndDrag( pointerData, leftIsDown || rightIsDown || middleIsDown);
			}
		}
		void ProcessMoveAndDrag( PointerEventData ped, bool pressed)
		{
			raycastResults.Clear();
			
			if( windowRaycaster != null)
			{
				windowRaycaster.Raycast( ped, raycastResults);
			}
			GameObject currentOverGo = (raycastResults.Count > 0)? raycastResults[ 0].gameObject : null;
			
			if( ped.pointerDrag != null && ped.dragging == false)
			{
				if( ShouldStartDrag( ped.pressPosition, ped.position, pixelDragThreshold, ped.useDragThreshold) != false)
				{
					ExecuteEvents.Execute( ped.pointerDrag, ped, ExecuteEvents.beginDragHandler);
					ped.dragging = true;
				}
			}
			if( ped.dragging != false && ped.pointerDrag != null)
			{
				ExecuteEvents.Execute( ped.pointerDrag, ped, ExecuteEvents.dragHandler);
			}
		}
		static bool ShouldStartDrag( Vector2 pressPos, Vector2 currentPos, int threshold, bool useDragThreshold)
		{
			if( useDragThreshold == false)
			{
				return true;
			}
			return (pressPos - currentPos).sqrMagnitude >= (threshold * threshold);
		}
		public interface IWindowKeyHandler : IEventSystemHandler
		{
			void OnKey( KeyCode key, bool down);
		}
		void OnInputEventMethod( Window.InputEvent ev)
		{
			if( ev.index == SubWindowIndex)
			{
				switch( ev.msg)
				{
					case 0x0200: /* WM_MOUSEMOVE */
					{
						injectedPosition = new Vector2( ev.x, ev.y);
						hasInjectedPosition = true;
						break;
					}
					case 0x0201: /* WM_LBUTTONDOWN */
					{
						injectLeftDown = true;
						break;
					}
					case 0x0202: /* WM_LBUTTONUP */
					{
						injectLeftUp = true;
						break;
					}
					case 0x0204: /* WM_RBUTTONDOWN */
					{
						injectRightDown = true;
						break;
					}
					case 0x0205: /* WM_RBUTTONUP */
					{
						injectRightUp = true;
						break;
					}
					case 0x0207: /* WM_MBUTTONDOWN */
					{
						injectMiddleDown = true;
						break;
					}
					case 0x0208: /* WM_MBUTTONUP */
					{
						injectMiddleUp = true;
						break;
					}
				}
			}
		}
		[AOT.MonoPInvokeCallback( typeof( Window.InputEventCallback))]
		internal static void OnSubWindowEventCallback( Window.InputEvent msg)
		{
			OnInputEvent?.Invoke( msg);
		}
		static event System.Action<Window.InputEvent> OnInputEvent;
	}
}
