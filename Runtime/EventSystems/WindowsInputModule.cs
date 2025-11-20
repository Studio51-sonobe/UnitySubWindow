
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace SubWindows
{
	public class WindowsInputModule : BaseInputModule
	{
		internal int SubWindowIndex
		{
			get;
			set;
		} = -1;
		
		protected override void Awake()
		{
			base.Awake();
			OnEnable();
			OnInputEvent += OnInputEventMethod;
		}
		protected override void OnDestroy()
		{
			OnInputEvent -= OnInputEventMethod;
			base.OnDestroy();
		}
		public override void Process()
		{
		}
		internal void Process( GraphicRaycaster raycaster, RenderTexture renderTexture)
		{
			var pointerEventData = new PointerEventData(eventSystem)
			{
				position = new Vector2( m_MousePosition.x, renderTexture.height - m_MousePosition.y)
			};
			var results = new List<RaycastResult>();
			raycaster.Raycast( pointerEventData, results);
			
			GameObject hit = results.Count > 0 ? results[ 0].gameObject : null;
			
			if( (m_MouseClickFlags & 0x01) != 0)
			{
				pointerEventData.pointerPressRaycast = results.Count > 0 ? results[0] : new RaycastResult();
				ExecuteEvents.Execute( hit, pointerEventData, ExecuteEvents.pointerDownHandler);
				pointerEventData.pointerPress = hit;
				m_MouseClickFlags &= ~0x01;
			}
			if( (m_MouseClickFlags & 0x02) != 0)
			{
				ExecuteEvents.Execute( hit, pointerEventData, ExecuteEvents.pointerUpHandler);
				
				if (pointerEventData.pointerPress == hit)
				{
					ExecuteEvents.Execute(hit, pointerEventData, ExecuteEvents.pointerClickHandler);
				}
				pointerEventData.pointerPress = null;
				m_MouseClickFlags &= ~0x02;
			}
			ExecuteEvents.Execute( hit, pointerEventData, ExecuteEvents.pointerMoveHandler);
		}
		void OnInputEventMethod( SubWindow.InputEvent ev)
		{
			if( ev.index == SubWindowIndex)
			{
				switch( ev.msg)
				{
					case 0x0200: /* WM_MOUSEMOVE */
					{
						m_MousePosition = new Vector2( ev.x, ev.y);
						break;
					}
					case 0x0201: /* WM_LBUTTONDOWN */
					{
						m_MouseClickFlags |= 0x01;
						break;
					}
					case 0x0202: /* WM_LBUTTONUP */
					{
						m_MouseClickFlags |= 0x02;
						break;
					}
				}
			}
		}
		[AOT.MonoPInvokeCallback( typeof( SubWindow.InputEventCallback))]
		internal static void OnSubWindowEventCallback( SubWindow.InputEvent msg)
		{
			OnInputEvent?.Invoke( msg);
		}
		static event System.Action<SubWindow.InputEvent> OnInputEvent;
		
		[System.NonSerialized]
		Vector2 m_MousePosition;
		[System.NonSerialized]
		int m_MouseClickFlags;
	}
}
