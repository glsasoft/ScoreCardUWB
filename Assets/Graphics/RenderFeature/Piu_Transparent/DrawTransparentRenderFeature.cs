using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class DrawTransparentRenderFeature : ScriptableRendererFeature
{
    public class DrawTransparentRenderPass : ScriptableRenderPass
    {
        FilteringSettings m_FilteringSettings;
        ProfilingSampler m_ProfilingSampler;
        RenderStateBlock m_RenderStateBlock;

        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>()
        {
            new ShaderTagId("Piu_Transparent"),
        };

        public DrawTransparentRenderPass(RenderQueueRange renderQueueRange, LayerMask layerMask)
        {
            m_ProfilingSampler = new ProfilingSampler("Piu_DrawTransparent");
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var colorAttachment = renderingData.cameraData.renderer.cameraColorTargetHandle;
            var depthAttachment = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            cmd.ClearRenderTarget(false, true, Color.black);

            ConfigureTarget(colorAttachment, depthAttachment);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {

                Camera camera = renderingData.cameraData.camera;
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
            
        }
    }

    DrawTransparentRenderPass m_ScriptablePass;

    public LayerMask layerMask;

    public override void Create()
    {
        m_ScriptablePass = new DrawTransparentRenderPass(RenderQueueRange.transparent, layerMask);

        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


