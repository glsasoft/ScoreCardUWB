using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WraithVoidEffectFeature : ScriptableRendererFeature
{
    public RenderPassEvent RenderPassEvent;

    [Serializable]
    public struct WraithVoidEffectSetting
    {
        public Material Material;
    }

    public WraithVoidEffectSetting Setting;
    class VoidEffectRenderPass : ScriptableRenderPass
    {
        private Material _Material;

        public void Setup(WraithVoidEffectSetting Setting)
        {
            this._Material = Setting.Material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if (cameraData.cameraType == CameraType.Preview || _Material == null)
                return;
            var renderer = cameraData.renderer;

            CommandBuffer cmd = CommandBufferPool.Get("WraithVoidEffect");

            var sourceHandle = cameraData.renderer.cameraColorTargetHandle;
            // 颜色
            Blitter.BlitCameraTexture(cmd, sourceHandle, renderer.PiuGetCameraColorFrontBuffer(cmd), _Material, 0);
            //Blitter.BlitCameraTexture(cmd, renderer.PiuGetCameraColorFrontBuffer(cmd), sourceHandle, _Material, 1);
            
            renderer.PiuSwapColorBuffer(cmd);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    VoidEffectRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new VoidEffectRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent;
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_ScriptablePass.Setup(Setting);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


