using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

public class DrawEffectRenderFeature : ScriptableRendererFeature
{
    public class DrawEffectRenderPass : ScriptableRenderPass
    {
        FilteringSettings m_FilteringSettings;
        public static readonly int _Bloom_Tex = Shader.PropertyToID("_BloomBlitTex");
        
        ProfilingSampler m_ProfilingSampler;
        RenderStateBlock m_RenderStateBlock;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>()
        {
            new ShaderTagId("Piu_Effect"),
        };

        public DrawEffectRenderPass(RenderQueueRange renderQueueRange, LayerMask layerMask)
        {
            m_ProfilingSampler = new ProfilingSampler("Piu_DrawEffect");
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var depthAttachment = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            cmd.GetTemporaryRT(_Bloom_Tex, descriptor);
            
            ConfigureClear(ClearFlag.Color, Color.clear);
            //一直报错，废弃Feature，注释掉
            //ConfigureTarget(_Bloom_Tex, depthAttachment);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                var sortFlags = SortingCriteria.CommonTransparent;
                var filterSettings = m_FilteringSettings;
#if UNITY_EDITOR
                if (renderingData.cameraData.isPreviewCamera)
                {
                    filterSettings.layerMask = -1;
                }
#endif

                DrawingSettings drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref m_RenderStateBlock);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_Bloom_Tex);
        }
    }

    DrawEffectRenderPass m_ScriptablePass;

    public LayerMask layerMask;

    public override void Create()
    {
        m_ScriptablePass = new DrawEffectRenderPass(RenderQueueRange.transparent, layerMask);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


