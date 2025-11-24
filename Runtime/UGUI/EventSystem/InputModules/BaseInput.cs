using UnityEngine;

namespace MultiWindow.EventSystems
{
	public class BaseInput : UnityEngine.EventSystems.UIBehaviour
	{
		public virtual string compositionString
		{
			get
			{
				var ret = Input.compositionString;
				if( string.IsNullOrEmpty( ret) == false)
				{
					Debug.LogError( ret);
				}
				return ret;
			}
		}
		public virtual IMECompositionMode imeCompositionMode
		{
			get { return Input.imeCompositionMode; }
			set { Input.imeCompositionMode = value; }
		}
		public virtual Vector2 compositionCursorPos
		{
			get { return Input.compositionCursorPos; }
			set { Input.compositionCursorPos = value; }
		}
		public virtual bool mousePresent
		{
			get { return Input.mousePresent; }
		}
		public virtual bool GetMouseButtonDown( int button)
		{
			if( m_Window != null)
			{
				bool ret = m_Window.GetMouseButtonDown( button);
				// Debug.LogError( $"{gameObject.name}: down = {ret}");
				return ret;
			}
			return Input.GetMouseButtonDown( button);
		}
		public virtual bool GetMouseButtonUp( int button)
		{
			if( m_Window != null)
			{
				bool ret = m_Window.GetMouseButtonUp( button);
				// Debug.LogError( $"{gameObject.name}: up = {ret}");
				return ret;
			}
			return Input.GetMouseButtonUp( button);
		}
		public virtual bool GetMouseButton( int button)
		{
			if( m_Window != null)
			{
				bool ret = m_Window.GetMouseButton( button);
				// Debug.LogError( $"{gameObject.name}: btn = {ret}");
				return ret;
			}
			return Input.GetMouseButton( button);
		}
		public virtual Vector2 mousePosition
		{
			get
			{
				if( m_Window != null)
				{
					Vector2 r = m_Window.GetMousePosition();
					// Debug.LogError( $"!{r}, {gameObject.name}");
					return r;
				}
				Vector2 ret = Input.mousePosition;
				// Debug.LogError( $"#{ret}, {gameObject.name}");
				return ret;
			}
		}
		public virtual Vector2 mouseScrollDelta
		{
			get
			{
				if( m_Window != null)
				{
					return m_Window.GetMouseScrollDelta();
				}
				return Input.mouseScrollDelta;
			}
		}
		public virtual float mouseScrollDeltaPerTick
		{
			get { return 1.0f; }
		}
		public virtual bool touchSupported
		{
			get { return Input.touchSupported; }
		}
		public virtual int touchCount
		{
			get { return Input.touchCount; }
		}
		public virtual Touch GetTouch( int index)
		{
			return Input.GetTouch( index);
		}
		public virtual float GetAxisRaw( string axisName)
		{
			return Input.GetAxisRaw( axisName);
		}
		public virtual bool GetButtonDown( string buttonName)
		{
			return Input.GetButtonDown( buttonName);
		}
		internal void SetWindow( Window window)
		{
			m_Window = window;
		}
		Window m_Window;
	}
}
