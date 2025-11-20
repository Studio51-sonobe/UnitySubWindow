
using System;
using UnityEngine.Rendering.Universal;

namespace SubWindows
{
	[DisallowMultipleRendererFeature( "Flip Vertical Sync")]
	public sealed class FlipVerticalSync : ScriptableRendererFeature
	{
		public override void Create()
		{
			m_Pass = new FlipVerticalSyncPass()
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
			};
			m_Pass.ConfigureInput( ScriptableRenderPassInput.Color);
		}
		public override void AddRenderPasses( ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass( m_Pass);
		}
		[NonSerialized]
		FlipVerticalSyncPass m_Pass;
	}
}
