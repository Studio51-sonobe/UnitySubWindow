
using UnityEngine.Rendering.Universal;

namespace MultiWindow
{
	[DisallowMultipleRendererFeature]
	public sealed class FlipVerticalSync : ScriptableRendererFeature
	{
		public override void Create()
		{
			m_Pass = new FlipVerticalSyncPass()
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
			};
			// m_Pass.ConfigureInput( ScriptableRenderPassInput.Color);
		}
		public override void AddRenderPasses( ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass( m_Pass);
		}
		FlipVerticalSyncPass m_Pass;
	}
}
