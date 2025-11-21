
using UnityEngine;
using UnityEngine.EventSystems;
using MultiWindow;

public class SubWindowDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	public void OnBeginDrag( PointerEventData eventData)
	{
		if( m_Window.TryGetPosition( out Vector2Int position) != false)
		{
			m_WindowPosition = new Vector2( position.x, position.y);
		}
	}
	public void OnDrag( PointerEventData eventData)
	{
		if( m_WindowPosition.HasValue != false)
		{
			m_WindowPosition = new Vector2(
				m_WindowPosition.Value.x + eventData.delta.x,
				m_WindowPosition.Value.y - eventData.delta.y);
			m_Window.Move( 
				(int)m_WindowPosition.Value.x, 
				(int)m_WindowPosition.Value.y);
		}
	}
	public void OnEndDrag( PointerEventData eventData)
	{
		m_WindowPosition = null;
	}
	[SerializeField]
	Window m_Window;
	[System.NonSerialized]
	Vector2? m_WindowPosition;
}
