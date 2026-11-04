using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ReflectEffectRenderFeature : ScriptableRendererFeature
{
    public RenderPassEvent RenderPassEvent;
    
    [Serializable]
    public struct ReflectEffectSetting
    {
        [Range(0, 4)] public int DownSample;
    }
    public ReflectEffectSetting setting;
    class ReflectEffectRenderPass : ScriptableRenderPass
    {
        private int _downSample;
        private int _CustomCameraColorTextureID = Shader.PropertyToID("_CustomCameraColorTexture");
        
        public void Setup(ReflectEffectSetting setting)
        {
            this._downSample = setting.DownSample;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) { }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            if (renderingData.cameraData.isPreviewCamera) return;
            var renderer = renderingData.cameraData.renderer;
            CommandBuffer cmd = CommandBufferPool.Get("BlitAfterPostProcessingTexture");
            
            var sourceHandle = renderer.cameraColorTargetHandle;
            Blitter.BlitCameraTexture(cmd, sourceHandle, renderer.PiuGetCameraColorFrontBuffer(cmd));
            cmd.SetGlobalTexture(_CustomCameraColorTextureID, sourceHandle);
            renderer.PiuSwapColorBuffer(cmd);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
        public void Dispose() { }
    }
    
    ReflectEffectRenderPass m_ScriptablePass;
    public override void Create()
    {
        m_ScriptablePass = new ReflectEffectRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent;
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_ScriptablePass.Setup(setting);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        m_ScriptablePass.Dispose();
    }
}


