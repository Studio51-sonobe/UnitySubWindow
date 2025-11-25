
using UnityEngine;
using System.Collections.Generic;

namespace MultiWindow
{
	[RequireComponent( typeof( Camera))]
	public partial class Window : MonoBehaviour
	{
		public bool IsCreated
		{
			get{ return m_SubWindowIndex >= 0; }
		}
		public bool Create( int x = -1, int y = -1)
		{
			if( m_Camera != null && m_SubWindowIndex < 0)
			{
				if( m_RenderTexture == null)
				{
					m_RenderTexture = new RenderTexture( m_Width, m_Height, 
						UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
						UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt, 1);
				}
				if( m_RenderTexture != null)
				{
					m_Camera.targetTexture = m_RenderTexture;
					m_SubWindowIndex = CreateSubWindow( m_RenderTexture.GetNativeTexturePtr(),
						x, y, m_RenderTexture.width, m_RenderTexture.height, OnSubWindowEventCallback);
					if( m_SubWindowIndex >= 0)
					{
						m_InputModule?.input.SetWindow( this);
						return true;
					}
				}
				if( m_RenderTexture != null)
				{
					m_Camera.targetTexture = null;
					m_RenderTexture.Release();
					m_RenderTexture = null;
				}
			}
			return false;
		}
		public void Dispose()
		{
			if( m_SubWindowIndex >= 0)
			{
				m_InputModule?.input.SetWindow( null);
				DisposeSubWindow( m_SubWindowIndex);
				m_SubWindowIndex = -1;
			}
			if( m_RenderTexture != null)
			{
				m_Camera.targetTexture = null;
				m_RenderTexture.Release();
				m_RenderTexture = null;
			}
		}
		public void Move( int x, int y)
		{
			if( m_SubWindowIndex >= 0)
			{
				MoveSubWindow( m_SubWindowIndex, x, y);
			}
		}
		public bool TryGetPosition( out Vector2Int position)
		{
			if( m_SubWindowIndex >= 0)
			{
				TPoint point = GetSubWindowPoint( m_SubWindowIndex);
				position = new Vector2Int( point.x, point.y);
				return true;
			}
			position = Vector2Int.zero;
			return false;
		}
		internal bool GetMouseButton( int button)
		{
			return (m_MouseButton & (1 << button)) != 0;
		}
		internal bool GetMouseButtonUp( int button)
		{
			return (m_MouseButtonUp & (1 << button)) != 0;
		}
		internal bool GetMouseButtonDown( int button)
		{
			return (m_MouseButtonDown & (1 << button)) != 0;
		}
		internal Vector2 GetMousePosition()
		{
			return m_MousePosition;
		}
		internal Vector2 GetMouseScrollDelta()
		{
			return m_MouseScrollDelta;
		}
		[AOT.MonoPInvokeCallback( typeof( InputEventCallback))]
		internal static void OnSubWindowEventCallback( InputEvent msg)
		{
			OnInputEvent?.Invoke( msg);
		}
		void Awake()
		{
			OnInputEvent += OnInputEventMethod;
		}
		void OnDestroy()
		{
			OnInputEvent -= OnInputEventMethod;
		}
		void Update()
		{
			m_MouseButton |= m_MouseButtonDown;
			m_MouseButton &= ~m_MouseButtonUp;
		}
		void LateUpdate()
		{
			m_MouseButtonUp = 0;
			m_MouseButtonDown = 0;
			m_MouseScrollDelta = Vector2.zero;
		}
		void OnInputEventMethod( InputEvent ev)
		{
			if( ev.index == m_SubWindowIndex)
			{
				switch( ev.msg)
				{
					case 0x0200: /* WM_MOUSEMOVE */
					{
						m_MousePosition = new Vector2( ev.x, m_Height - ev.y);
						break;
					}
					case 0x0201: /* WM_LBUTTONDOWN */
					{
						m_MouseButtonDown |= 1 << 0;
						break;
					}
					case 0x0202: /* WM_LBUTTONUP */
					{
						m_MouseButtonUp |= 1 << 0;
						break;
					}
					case 0x0204: /* WM_RBUTTONDOWN */
					{
						m_MouseButtonDown |= 1 << 1;
						break;
					}
					case 0x0205: /* WM_RBUTTONUP */
					{
						m_MouseButtonUp |= 1 << 1;
						break;
					}
					case 0x0207: /* WM_MBUTTONDOWN */
					{
						m_MouseButtonDown |= 1 << 2;
						break;
					}
					case 0x0208: /* WM_MBUTTONUP */
					{
						m_MouseButtonUp |= 1 << 2;
						break;
					}
					case 0x20a: /* WM_MOUSEWHEEL */
					{
						m_MouseScrollDelta.x = 0;
						m_MouseScrollDelta.y = ev.w / 120.0f;
						break;
					}
					case 0x0100: /* WM_KEYDOWN */
					{
						m_InputModule?.AddKeyDown( GetKeyCode( ev.x));
						break;
					}
					case 0x0101: /* WM_KEYUP */
					{
						m_InputModule?.AddKeyUp( GetKeyCode( ev.x));
						break;
					}
					case 0x0102: /* WM_CHAR */
					{
						m_InputModule?.AddChar( (char)ev.x);
						break;
					}
					case 0x0281: /* WM_IME_CHAR */
					{
						// InjectChar( (char)ev.x);
						break;
					}
				}
			}
		}
		static KeyCode GetKeyCode( int vk)
		{
			return vk switch
			{
				0x08 => KeyCode.Backspace,
				0x09 => KeyCode.Tab,
				0x0D => KeyCode.Return,
				0x10 => KeyCode.LeftShift,
				0x25 => KeyCode.LeftArrow,
				0x26 => KeyCode.UpArrow,
				0x27 => KeyCode.RightArrow,
				0x28 => KeyCode.DownArrow,
				0x2E => KeyCode.Delete,
				0x2A => KeyCode.Asterisk,
				0x2B => KeyCode.Plus,
				0x41 => KeyCode.A,
				0x42 => KeyCode.B,
				0x43 => KeyCode.C,
				0x44 => KeyCode.D,
				0x45 => KeyCode.E,
				0x46 => KeyCode.F,
				0x47 => KeyCode.G,
				0x48 => KeyCode.H,
				0x49 => KeyCode.I,
				0x4A => KeyCode.J,
				0x4B => KeyCode.K,
				0x4C => KeyCode.L,
				0x4D => KeyCode.M,
				0x4E => KeyCode.N,
				0x4F => KeyCode.O,
				0x50 => KeyCode.P,
				0x51 => KeyCode.Q,
				0x52 => KeyCode.R,
				0x53 => KeyCode.S,
				0x54 => KeyCode.T,
				0x55 => KeyCode.U,
				0x56 => KeyCode.V,
				0x57 => KeyCode.W,
				0x58 => KeyCode.X,
				0x59 => KeyCode.Y,
				0x5A => KeyCode.Z,
				_ => KeyCode.None,
			};
		}
	#if !UNITY_EDITOR
		void Reset()
		{
			m_Camera = GetComponent<Camera>();
			m_EventSystem = GetComponentInChildren<EventSystems.EventSystem>();
			m_InputModule = GetComponentInChildren<EventSystems.BaseInputModule>();
		}
	#endif
		static event System.Action<Window.InputEvent> OnInputEvent;
		
		[SerializeField, Range( 128, 4096)]
		int m_Width = 512;
		[SerializeField, Range( 128, 4096)]
		int m_Height = 512;
		[SerializeField]
		Camera m_Camera;
		[SerializeField]
		EventSystems.EventSystem m_EventSystem;
		[SerializeField]
		EventSystems.BaseInputModule m_InputModule;
		[System.NonSerialized]
		int m_SubWindowIndex = -1;
		[System.NonSerialized]
		RenderTexture m_RenderTexture;
		
		int m_MouseButton;
		int m_MouseButtonUp;
		int m_MouseButtonDown;
		Vector2 m_MousePosition;
		Vector2 m_MouseScrollDelta;
	}
}
