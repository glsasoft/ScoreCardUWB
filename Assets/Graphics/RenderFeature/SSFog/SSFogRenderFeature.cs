using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SSFogRenderFeature : ScriptableRendererFeature
{
    public RenderPassEvent RenderPassEvent;

    [Serializable]
    public struct SSFogSetting
    {
        public Material material;
    }
    
    public SSFogSetting setting;

    class SSFogRenderPass : ScriptableRenderPass
    {
        private Material _material;

        public void Setup(SSFogSetting setting)
        {
            this._material = setting.material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;

            if (!_material) return;
            
            var renderer = cameraData.renderer;

            CommandBuffer cmd = CommandBufferPool.Get("SSFogEffect");
            
            var sourceHandle = cameraData.renderer.cameraColorTargetHandle;
            
            Blitter.BlitCameraTexture(cmd, sourceHandle, renderer.PiuGetCameraColorFrontBuffer(cmd), _material, 0);
            
            renderer.PiuSwapColorBuffer(cmd);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            
        }
    }
    
    SSFogRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new SSFogRenderPass();
        
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
}


