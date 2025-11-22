
using UnityEngine.Rendering.Universal;

namespace MultiWindow
{
	[DisallowMultipleRendererFeature]
	public sealed class FlipVerticalSync : ScriptableRendererFeature
	{
		public override void Create()
		{
		#if !UNITY_EDITOR
			m_Pass = new FlipVerticalSyncPass()
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
			};
		#endif
		}
		public override void AddRenderPasses( ScriptableRenderer renderer, ref RenderingData renderingData)
		{
		#if !UNITY_EDITOR
			renderer.EnqueuePass( m_Pass);
		#endif
		}
	#if !UNITY_EDITOR
		FlipVerticalSyncPass m_Pass;
	#endif
	}
}
