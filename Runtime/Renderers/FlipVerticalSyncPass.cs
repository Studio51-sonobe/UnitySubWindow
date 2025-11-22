
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace MultiWindow
{
	public sealed class FlipVerticalSyncPass : ScriptableRenderPass
	{
		public FlipVerticalSyncPass()
		{
		}
		public override void RecordRenderGraph( RenderGraph renderGraph, ContextContainer frameData)
		{
			var cameraData = frameData.Get<UniversalCameraData>();
			
			if( (cameraData.camera.cameraType & kInvalidCameraType) != 0)
			{
				return;
			}
			UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
			TextureHandle cameraTexture = resourceData.activeColorTexture;
			
			if( cameraTexture.IsValid() != false)
			{
				var tempDesc = renderGraph.GetTextureDesc( cameraTexture);
				tempDesc.name = kTempColorTarget;
				TextureHandle tempTexture = renderGraph.CreateTexture( tempDesc);
				
				if( tempTexture.IsValid() != false)
				{
					renderGraph.AddBlitPass( cameraTexture, tempTexture, 
						new Vector2( 1.0f, -1.0f), new Vector2( 0.0f, 1.0f));
					
					if( renderPassEvent <= RenderPassEvent.BeforeRenderingPostProcessing)
					{
						resourceData.cameraColor = tempTexture;
					}
					else
					{
						renderGraph.AddCopyPass( tempTexture, cameraTexture);
					}
				}
			}
		}
		const CameraType kInvalidCameraType = CameraType.SceneView | CameraType.Preview;
		const string kTempColorTarget = "_FlipVerticalTarget";
	}
}
