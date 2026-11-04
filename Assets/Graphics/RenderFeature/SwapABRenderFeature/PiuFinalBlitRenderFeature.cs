using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PiuFinalBlitRenderFeature : ScriptableRendererFeature
{
    public RenderPassEvent RenderPassEvent;

    [Serializable]
    public struct PiuFinalBlitSetting
    {
        public Material Material;
    }
    
    public PiuFinalBlitSetting Setting;
    
    class PiuFinalBlitRenderPass : ScriptableRenderPass
    {
        private Material _material;
        // 标记是否已经记录过该材质的日志，防止每帧重复打印
        private bool _materialLogged = false;

        public void Setup(PiuFinalBlitSetting setting)
        {
            // 只在材质首次设置或者材质实际变化时记录一次日志，避免每帧重复输出
            var newMat = setting.Material;
            if (newMat == null)
            {
                // 重置状态以便后续如果设置了材质能再次记录
                _material = null;
                _materialLogged = false;
                // 仅在编辑器中打印错误以避免在设备/发布版本中产生大量日志
#if UNITY_EDITOR
                Debug.LogError("PiuFinalBlitRenderFeature.Setup: Setting.Material is null. Please assign a Material in the Renderer Feature settings (SwapABRenderFeature -> Setting.Material).");
#endif
            }
            else
            {
                // 若材质未变化且已经记录过日志，则直接返回（避免重复日志）
                if (_material == newMat && _materialLogged)
                {
                    // 保持 _material 引用一致
                    _material = newMat;
                    return;
                }

                // 更新材质引用并记录日志
                _material = newMat;
                try
                {
#if UNITY_EDITOR
                    var path = UnityEditor.AssetDatabase.GetAssetPath(_material);
                    Debug.Log($"PiuFinalBlitRenderFeature.Setup: Using material '{_material.name}' at '{path}'");
#endif
                }
                catch (Exception)
                {
#if UNITY_EDITOR
                    Debug.Log($"PiuFinalBlitRenderFeature.Setup: Using material '{_material.name}'");
#endif
                }

                _materialLogged = true;
            }
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if (cameraData.cameraType == CameraType.Preview) return;

            var renderer = cameraData.renderer;
            var loadAction = RenderBufferLoadAction.DontCare;
            if (!cameraData.isSceneViewCamera && !cameraData.isDefaultViewport)
                loadAction = RenderBufferLoadAction.Load;

            CommandBuffer cmd = CommandBufferPool.Get("PiuFinalBlit");

            // Blit 当前帧颜色到 A/B 缓冲前帧位置
            var sourceHandle = renderer.cameraColorTargetHandle;
            
            var cameraTarget = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
            RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTarget);
            var cameraTargetHandle = RTHandleStaticHelpers.s_RTHandleWrapper;
            
            if (_material == null)
            {
#if UNITY_EDITOR
                Debug.LogError("PiuFinalBlitRenderFeature.Execute: Material is null, skipping final blit. Assign a Material to the Render Feature to enable this pass.");
#endif
            }
            else
            {
                RenderingUtils.FinalBlit(cmd, ref cameraData, sourceHandle, cameraTargetHandle, loadAction, RenderBufferStoreAction.Store, _material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

    PiuFinalBlitRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new PiuFinalBlitRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent;
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_ScriptablePass.Setup(Setting);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (Setting.Material == null)
        {
            return;
        }

        renderer.EnqueuePass(m_ScriptablePass);
    }
}