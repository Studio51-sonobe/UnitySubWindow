
using UnityEngine;
using UnityEngine.UI;

namespace MultiWindow
{
	public partial class Window : MonoBehaviour
	{
		public bool IsCreated
		{
			get{ return m_InputModule.SubWindowIndex >= 0; }
		}
		public bool Create( int x = -1, int y = -1)
		{
			if( m_Camera != null && m_InputModule != null && m_InputModule.SubWindowIndex < 0)
			{
				RenderTexture renderTexture = m_Camera.targetTexture;
					
				if( renderTexture != null)
				{
					m_InputModule.SubWindowIndex = CreateSubWindow( renderTexture.GetNativeTexturePtr(),
						x, y, renderTexture.width, renderTexture.height, WindowInputModule.OnSubWindowEventCallback);
					if( m_InputModule.SubWindowIndex >= 0)
					{
						return true;
					}
				}
			}
			return false;
		}
		public void Dispose()
		{
			if( m_InputModule != null)
			{
				if( m_InputModule.SubWindowIndex >= 0)
				{
					DisposeSubWindow( m_InputModule.SubWindowIndex);
					m_InputModule.SubWindowIndex = -1;
				}
			}
		}
		public void Move( int x, int y)
		{
			int index = m_InputModule?.SubWindowIndex ?? -1;
			if( index >= 0)
			{
				MoveSubWindow( m_InputModule.SubWindowIndex, x, y);
			}
		}
		public bool TryGetPosition( out Vector2Int position)
		{
			int index = m_InputModule?.SubWindowIndex ?? -1;
			if( index >= 0)
			{
				TPoint point = GetSubWindowPoint( index);
				position = new Vector2Int( point.x, point.y);
				return true;
			}
			position = Vector2Int.zero;
			return false;
		}
		void Update()
		{
			if( m_InputModule != null && m_InputModule.SubWindowIndex >= 0)
			{
				m_InputModule.Process( m_Camera, m_Raycaster);
			}
		}
		[SerializeField]
		Camera m_Camera;
		[SerializeField]
		GraphicRaycaster m_Raycaster;
		[SerializeField]
		internal WindowInputModule m_InputModule;
	}
}
