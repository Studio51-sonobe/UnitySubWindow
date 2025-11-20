
using UnityEngine;
using UnityEngine.UI;

namespace SubWindows
{
	public partial class SubWindow : MonoBehaviour
	{
		public bool IsCreated
		{
			get{ return m_InputModule.SubWindowIndex >= 0; }
		}
		public bool Create()
		{
			if( m_Camera != null && m_InputModule != null && m_InputModule.SubWindowIndex < 0)
			{
				RenderTexture renderTexture = m_Camera.targetTexture;
					
				if( renderTexture != null)
				{
					m_InputModule.SubWindowIndex = CreateSubWindow( renderTexture.GetNativeTexturePtr(),
						renderTexture.width, renderTexture.height, WindowsInputModule.OnSubWindowEventCallback);
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
		void Update()
		{
			if( m_InputModule != null && m_InputModule.SubWindowIndex >= 0
			&&	m_Raycaster != null && m_Camera.targetTexture != null)
			{
				m_InputModule.Process( m_Raycaster, m_Camera.targetTexture);
			}
		}
		[SerializeField]
		Camera m_Camera;
		[SerializeField]
		GraphicRaycaster m_Raycaster;
		[SerializeField]
		internal WindowsInputModule m_InputModule;
	}
}
