using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlindFogRenderFeature : ScriptableRendererFeature
{
    public RenderPassEvent RenderPassEvent;
    [Serializable]
    public struct BlindFogSetting
    {
        public Material Material;
        public float FogStart;
        public float FogEnd;
        [Range(0, 3)]public float FogDensity;
        public Color FogColor;
    }

    public BlindFogSetting Setting;
    class BlindFogRenderPass : ScriptableRenderPass
    {
        private int RT_Tmp0;
        private Material _material;

        private float _fogStart;
        private float _fogEnd;
        private float _fogDensity;
        private Color _fogColor;
        
        public void Setup(BlindFogSetting setting)
        {
            this._material = setting.Material;
            this._fogStart = setting.FogStart;
            this._fogEnd = setting.FogEnd;
            this._fogColor = setting.FogColor;
            this._fogDensity = setting.FogDensity;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.mipCount = 0;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            
            RT_Tmp0 = Shader.PropertyToID("RT_Tmp0_BlindFog");
            cmd.GetTemporaryRT(RT_Tmp0, descriptor, FilterMode.Bilinear);
            
            // 配置输入：需要读取 相机的颜色纹理（Camera Color Texture）
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!_material) return;
            
            var renderer = renderingData.cameraData.renderer;   // 渲染器实例
            CommandBuffer cmd = CommandBufferPool.Get("BlindFog");
            
            var sourceHandle = renderer.cameraColorTargetHandle;    // 当前颜色缓冲
            
            // 传数据
            _material.SetFloat("_FogStart", _fogStart);
            _material.SetFloat("_FogEnd", _fogEnd);
            _material.SetFloat("_FogDensity", _fogDensity);
            _material.SetColor("_FogColor", _fogColor);
         
            // RenderTargetIdentifier source = sourceHandle.nameID;
            // cmd.Blit(source, RT_Tmp0, _material, 0);
            // cmd.Blit(RT_Tmp0, source);
            
            Blitter.BlitCameraTexture(cmd, sourceHandle, renderer.PiuGetCameraColorFrontBuffer(cmd), _material, 0);
            // Blitter.BlitCameraTexture(cmd, renderer.PiuGetCameraColorFrontBuffer(cmd), sourceHandle);
            
            renderer.PiuSwapColorBuffer(cmd);// 交换颜色缓冲区
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(RT_Tmp0);
        }
    }
    
    BlindFogRenderPass m_ScriptablePass;
    public override void Create()
    {
        m_ScriptablePass = new BlindFogRenderPass();
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


