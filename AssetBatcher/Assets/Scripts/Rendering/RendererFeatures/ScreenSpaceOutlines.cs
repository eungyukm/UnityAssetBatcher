using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{
    [System.Serializable]
    private class ViewSpaceNormalsTextureSetting
    {
        
    }
    
    private class ViewSpaceNormalsTexturePss : ScriptableRenderPass
    {
        private readonly RenderTargetHandle normals;

        public ViewSpaceNormalsTexturePss(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
            normals.Init("_SceneViewSpaceNormals");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTex
            cmd.GetTemporaryRT(normals.id, cameraTextureDescriptor, FilterMode.Point);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
        }
    }
    public override void Create()
    {
        throw new System.NotImplementedException();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        throw new System.NotImplementedException();
    }
}
