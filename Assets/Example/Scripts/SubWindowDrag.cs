
using UnityEngine;
using MultiWindow;
using MultiWindow.EventSystems;

public class SubWindowDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	public void OnBeginDrag( PointerEventData eventData)
	{
		if( m_Window.TryGetPosition( out Vector2Int position) != false)
		{
			m_CursorPosition = Window.GetCursorPos();
			m_WindowPosition = new Vector2( position.x, position.y);
		}
	}
	public void OnDrag( PointerEventData eventData)
	{
		if( m_WindowPosition.HasValue != false)
		{
			Vector2 cursorDelta = Window.GetCursorPos() - m_CursorPosition;
			Vector2 windowPosition = new(
				m_WindowPosition.Value.x + cursorDelta.x,
				m_WindowPosition.Value.y + cursorDelta.y);
			m_Window.Move( 
				(int)windowPosition.x, 
				(int)windowPosition.y);
		}
	}
	public void OnEndDrag( PointerEventData eventData)
	{
		m_WindowPosition = null;
	}
	[SerializeField]
	Window m_Window;
	[System.NonSerialized]
	Vector2 m_CursorPosition;
	[System.NonSerialized]
	Vector2? m_WindowPosition;
}
