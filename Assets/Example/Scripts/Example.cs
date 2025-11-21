
using UnityEngine;
using MultiWindow;

public class Example : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSplashScreen)]
	static void Initialize()
	{
		Window.Initialize( 2);
	}
	void OnDestroy()
	{
		Window.Terminate();
	}
	void Update()
	{
		if( Input.GetMouseButtonDown( 2) != false)
		{
			m_MousePosition = Window.GetCursorPos();
			m_WindowPosition = Window.GetMainPosition();
		}
		else if( Input.GetMouseButton( 2) != false)
		{
			Vector2Int currentPosition = Window.GetCursorPos();
			float x = m_WindowPosition.x + (currentPosition.x - m_MousePosition.x);
			float y = m_WindowPosition.y + (currentPosition.y - m_MousePosition.y);
			Window.MoveMain( (int)x, (int)y);
		}
		if( Input.GetMouseButtonUp( 0) != false)
		{
			if( m_Window1.IsCreated == false)
			{
				m_Window1.Create();
			}
			else
			{
				m_Window1.Dispose();
			}
		}
		else if( Input.GetMouseButtonUp( 1) != false)
		{
			if( m_Window2.IsCreated == false)
			{
				m_Window2.Create();
			}
			else
			{
				m_Window2.Dispose();
			}
		}
	}
	[SerializeField]
	Window m_Window1;
	[SerializeField]
	Window m_Window2;
	
	Vector2 m_WindowPosition;
	Vector2Int m_MousePosition;
}

