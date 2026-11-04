using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class EffPostProcessing : ScriptableRendererFeature
{
    /// <summary>要执行这些 Pass 的相机类型。预览窗口（Preview）、反射探针等不在其中。</summary>
    [Flags]
    public enum CameraScope
    {
        None = 0,
        GameView = 1 << 0,
        SceneView = 1 << 1,
        GameAndSceneView = GameView | SceneView,
    }

    [Serializable]
    public class Settings
    {
        [Header("截屏贴图ID   _ScreenCopyTexture")]
        [Header("LightMode  “Eff")]
        
        
        [Space(10)]
        [Header("作用相机")]
        [Tooltip("GameView = 运行时的游戏相机；SceneView = 编辑器 Scene 视图相机。")]
        public CameraScope cameraScope = CameraScope.GameAndSceneView;

        [Header("拷贝 Pass")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        [Tooltip("下采样倍率，1 = 原分辨率。")]
        [Range(1, 8)] public int downsample = 1;

        public FilterMode filterMode = FilterMode.Bilinear;

        [Header("Eff 绘制 Pass（紧跟拷贝 Pass 之后）")]
        [Tooltip("总开关：关掉就只拷贝不画 Eff。")]
        public bool drawEffPass = true;

        [Tooltip("绘制哪个队列的 Eff 物体。故障片是 Transparent 队列，保持 Transparent；调成 All 可以连不透明队列的一起画。")]
        public RenderQueueRange effRenderQueueRange = RenderQueueRange.transparent;

        [Tooltip("只画这些 Layer 上的 Eff 物体。")]
        public LayerMask effLayerMask = ~0;

        [Tooltip("透明物体的排序方式。CommonTransparent = 按距离，和 URP 默认一致。")]
        public SortingCriteria effSortingCriteria = SortingCriteria.CommonTransparent;
    }

    public Settings settings = new Settings();
    private ScreenCopyPass screenCopyPass;
    private DrawEffPass drawEffPass;

    public override void Create()
    {
        screenCopyPass = new ScreenCopyPass(settings);
        drawEffPass = new DrawEffPass(settings);
    }

    /// <summary>
    /// 每台相机渲染前都会调到这里，把"这台相机当前的颜色目标"交给 Pass。
    /// 必须在这里 Setup：AddRenderPasses 时相机的颜色目标还没准备好，
    /// 原来代码里 Setup 从来没被调用过，Blit 的 source 一直是默认值（空标识），这就是个隐藏 bug。
    /// </summary>
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        if (colorTarget == null)
            return;

        // Setup 里同步一次设置，这样 Inspector 上改 renderPassEvent / 队列 / LayerMask 不用重启 Play 就能生效
        screenCopyPass.Setup(settings, colorTarget);
        drawEffPass.Setup(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!IsCameraTypeEnabled(renderingData.cameraData.cameraType))
            return;

        // 顺序很重要：拷贝在前，画 Eff 在后。
        // 两个 Pass 的 renderPassEvent 相同，URP 排序用的是稳定排序（ScriptableRenderer.SortStable），
        // 事件相同的 Pass 会完全保持这里的入队顺序，所以 Eff 一定在拷贝之后执行 —— 它采样到的就是本帧本相机的画面。
        renderer.EnqueuePass(screenCopyPass);

        if (settings.drawEffPass)
            renderer.EnqueuePass(drawEffPass);
    }

    private bool IsCameraTypeEnabled(CameraType cameraType)
    {
        switch (cameraType)
        {
            case CameraType.Game:
                return (settings.cameraScope & CameraScope.GameView) != 0;
            case CameraType.SceneView:
                return (settings.cameraScope & CameraScope.SceneView) != 0;
            default:
                // Preview / Reflection 这类内部相机不需要，直接跳过
                return false;
        }
    }

    public class ScreenCopyPass : ScriptableRenderPass
    {
        private Settings settings;
        private RTHandle source;
        private int copyRTId = Shader.PropertyToID("_ScreenCopyRT");
        private static readonly int screenCopyTexId = Shader.PropertyToID("_ScreenCopyTexture");

        public ScreenCopyPass(Settings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
        }

        public void Setup(Settings settings, RTHandle source)
        {
            this.settings = settings;
            this.source = source;
            this.renderPassEvent = settings.renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.width = Mathf.Max(1, desc.width / settings.downsample);
            desc.height = Mathf.Max(1, desc.height / settings.downsample);
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.useMipMap = false;

            // 用【这台相机自己的】描述符分配临时 RT，Scene 视图和 Game 视图各拿各的尺寸
            cmd.GetTemporaryRT(copyRTId, desc, settings.filterMode);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (source == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("ScreenCopyPass");

            // 拷贝当前相机已经画好的画面。
            // 用 cmd.Blit 而不是 Blitter.BlitCameraTexture：目标是用 GetTemporaryRT 分配的临时 RT（int 句柄），
            // 而 Blitter.BlitCameraTexture 的目标参数是 RTHandle，类型对不上。
            // RTHandle -> RenderTargetIdentifier 有隐式转换，所以 source 可以直接传。
            cmd.Blit(source, copyRTId);

            // 关键：全局贴图由【正在渲染的这台相机】绑定成自己那份拷贝。
            // 这样 Scene 视图渲染时绑定的就是 Scene 视图自己的画面，不会再沿用 Game 视图的那一份。
            cmd.SetGlobalTexture(screenCopyTexId, copyRTId);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(copyRTId);
        }
    }

    /// <summary>
    /// 绘制 LightMode = "Eff" 的物体。
    /// 这就是原来由 <see cref="DrawEffectRenderFeature"/> / Assets/Graphics/RenderFeature/Piu_Effect 做的事，
    /// 现在收进这个 Feature 里，和 _ScreenCopyTexture 的拷贝成对管理：拷贝 → 立刻画 Eff，保证同一帧同一台相机。
    /// 注意：一个片如果同时被这里的 Pass 和别处的 Feature 绘制，会画两遍（Blend Off 时会重复覆盖），
    /// 所以启用这个 Pass 后，请把场景里原有的 Eff 绘制 Feature 关掉。
    /// </summary>
    public class DrawEffPass : ScriptableRenderPass
    {
        /// <summary>GlitchCombined.shader 里那个 Pass 的 LightMode，就是靠这个 tag 被挑出来的。</summary>
        private static readonly ShaderTagId k_EffShaderTag = new ShaderTagId("Eff");

        private static readonly List<ShaderTagId> k_ShaderTagIdList = new List<ShaderTagId> { k_EffShaderTag };

        private Settings settings;
        private FilteringSettings filteringSettings;
        private RenderStateBlock renderStateBlock;
        private ProfilingSampler effProfilingSampler;

        public DrawEffPass(Settings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
            effProfilingSampler = new ProfilingSampler("DrawEffPass");
            // RenderStateMask.Nothing：不改深度/模板/混合状态，完全听材质自己的（故障片是 ZWrite Off + ZTest Always + Blend Off）
            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public void Setup(Settings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
            filteringSettings = new FilteringSettings(settings.effRenderQueueRange, settings.effLayerMask);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, effProfilingSampler))
            {
                var drawSettings = CreateDrawingSettings(k_ShaderTagIdList, ref renderingData, settings.effSortingCriteria);
                // 不写 context.Submit()，交给 URP 在 drawRenderers 前后自己 Submit，
                // 这样 Pass 能继续和后面的渲染合并，也不会打断后面的 SetRenderTarget 状态。
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings, ref renderStateBlock);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
