using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ToneMappingRenderFeature : ScriptableRendererFeature
{
    private static readonly int IsTone = Shader.PropertyToID("_IsTone");
    
    public RenderPassEvent RenderPassEvent;
    [Serializable]
    public struct ToneMappingSetting
    {
        public Material Material;
        public bool isTone;
    }
    
    public ToneMappingSetting Setting;
    
    // 外部开关
    public void SetToneMappingEnabledInPass(bool isEnabled)
    {
        Setting.isTone = isEnabled;
    }

    class ToneMappingRenderPass : ScriptableRenderPass
    {
        private Material _material;
        private bool _isTone;
        
        public void Setup(ToneMappingSetting setting)
        {
            this._material = setting.Material;
            this._isTone = setting.isTone;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if (cameraData.cameraType == CameraType.Preview) return;
            if (_material == null) return;
            var renderer = cameraData.renderer;
            
            var loadAction = RenderBufferLoadAction.DontCare;
            if (!cameraData.isSceneViewCamera && !cameraData.isDefaultViewport)
                loadAction = RenderBufferLoadAction.Load;
            
            CommandBuffer cmd = CommandBufferPool.Get("ToneMapping");
            
            var sourceHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            // Blitter.BlitCameraTexture(cmd, sourceHandle, renderer.PiuGetCameraColorFrontBuffer(cmd), _material, 0);
            // renderer.PiuSwapColorBuffer(cmd);

            var cameraTarget = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
            RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTarget);
            var cameraTargetHandle = RTHandleStaticHelpers.s_RTHandleWrapper;
            
            _material.SetFloat(IsTone, _isTone ? 1 : 0);
            
            RenderingUtils.FinalBlit(cmd, ref cameraData, sourceHandle, cameraTargetHandle, loadAction, RenderBufferStoreAction.Store, _material, 0);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
    }
    
    ToneMappingRenderPass m_ScriptablePass;
    public override void Create()
    {
        m_ScriptablePass = new ToneMappingRenderPass();
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


