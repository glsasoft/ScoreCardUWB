using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 俯视角空气透视雾。在 AfterRenderingPostProcessing 入队，排在 SwapAB 的 Final Blit 之前。
/// 参数来源：场景里有 AerialFogController 就用它插值出来的结果，没有就用本 Feature 上的值。
/// </summary>
public class AerialFogRenderFeature : ScriptableRendererFeature
{
    [Tooltip("指向 AerialFog.mat。为空时不入队。")]
    public Material material;

    [Tooltip("没有 AerialFogController 时使用这组参数。")]
    public AerialFogParams parameters = AerialFogParams.Default;

    [Tooltip("插入时机。必须在透明物体之前（450），透明物不写深度，放后面会被雾整片涂掉。见设计文档 7.5。")]
    public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

    [Tooltip("调试预览。0 正常出图；1 原始深度；2 线性深度；3 视距；4 距离裁切；5 光深；6 天空遮罩。")]
    [Range(0, 6)] public int debugMode;

    AerialFogPass _pass;

    public override void Create()
    {
        _pass = new AerialFogPass();
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var p = AerialFogController.Instance != null
            ? AerialFogController.Instance.CurrentParams
            : parameters;

        _pass.Setup(material, p, renderPassEvent, debugMode);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        ref CameraData cameraData = ref renderingData.cameraData;

        // 预览和反射相机不画。Scene View 留着，方便在编辑器里直接调参。
        if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection)
            return;

        // Overlay 叠加在 base 相机的结果上，再雾一遍就是双重雾。
        if (cameraData.renderType == CameraRenderType.Overlay)
            return;

        if (material == null)
        {
            Debug.LogWarning($"[AerialFog] Renderer Feature \"{name}\" 没有指定材质，跳过。", this);
            return;
        }

        // 声明需要深度和颜色。必须在这里（AddRenderPasses）声明，URP 是在入队之后才收集需求的，
        // 放到 pass 的 OnCameraSetup 里就晚了——多相机切换时这个相机会拿到上一个相机留下的
        // _CameraDepthTexture，画面被按错误的距离雾化。
        _pass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);

        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _pass?.Dispose();
    }

    // 把 FogEnd 夹在 FogStart 之上，避免 Inspector 里填反导致整个画面糊死。
    void OnValidate()
    {
        if (parameters.fogEnd <= parameters.fogStart)
            parameters.fogEnd = parameters.fogStart + 0.01f;
    }

    class AerialFogPass : ScriptableRenderPass
    {
        static readonly int ID_FogStart = Shader.PropertyToID("_FogStart");
        static readonly int ID_FogEnd = Shader.PropertyToID("_FogEnd");
        static readonly int ID_FogCurve = Shader.PropertyToID("_FogCurve");
        static readonly int ID_Density = Shader.PropertyToID("_Density");
        static readonly int ID_HeightFalloff = Shader.PropertyToID("_HeightFalloff");
        static readonly int ID_BaseHeight = Shader.PropertyToID("_BaseHeight");
        static readonly int ID_MaxOpacity = Shader.PropertyToID("_MaxOpacity");
        static readonly int ID_ColorNear = Shader.PropertyToID("_ColorNear");
        static readonly int ID_ColorFar = Shader.PropertyToID("_ColorFar");
        static readonly int ID_SkyFogAmount = Shader.PropertyToID("_SkyFogAmount");
        // URP 14 的 ShaderPropertyId 是 internal，这里自己算。
        static readonly int ID_BlitTexture = Shader.PropertyToID("_BlitTexture");
        static readonly int ID_BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        static readonly int ID_DebugMode = Shader.PropertyToID("_DebugMode");
        static readonly int ID_SunColor = Shader.PropertyToID("_SunColor");
        static readonly int ID_SunInfluence = Shader.PropertyToID("_SunInfluence");

        // 全局版（AerialFogCommon.hlsl 用），给需要自己吃雾的透明物体 shader 读。
        static readonly int ID_FogStartGlobal = Shader.PropertyToID("_AerialFogStart");
        static readonly int ID_FogEndGlobal = Shader.PropertyToID("_AerialFogEnd");
        static readonly int ID_FogCurveGlobal = Shader.PropertyToID("_AerialFogCurve");
        static readonly int ID_DensityGlobal = Shader.PropertyToID("_AerialFogDensity");
        static readonly int ID_HeightFalloffGlobal = Shader.PropertyToID("_AerialFogHeightFalloff");
        static readonly int ID_BaseHeightGlobal = Shader.PropertyToID("_AerialFogBaseHeight");
        static readonly int ID_MaxOpacityGlobal = Shader.PropertyToID("_AerialFogMaxOpacity");
        static readonly int ID_ColorNearGlobal = Shader.PropertyToID("_AerialFogColorNear");
        static readonly int ID_ColorFarGlobal = Shader.PropertyToID("_AerialFogColorFar");

        Material _material;
        AerialFogParams _params;
        int _debugMode;
        RTHandle _copiedColor;

        readonly ProfilingSampler _sampler = new ProfilingSampler("AerialFog");
        // DrawProcedural 时把源贴图和 scale-bias 塞进去，避免直接读正在写的相机颜色。
        readonly MaterialPropertyBlock _blitProps = new MaterialPropertyBlock();

        public void Setup(Material mat, in AerialFogParams p, RenderPassEvent evt, int debug)
        {
            _material = mat;
            _params = p;
            renderPassEvent = evt;
            _debugMode = debug;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // 深度需求在 AddRenderPasses 里声明（见 AerialFogRenderFeature.AddRenderPasses），这里不再重复。

            // 自己管理渲染目标，必须把 ScriptableRenderer 的活动目标失效掉。
            ResetTarget();

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _copiedColor, desc, FilterMode.Bilinear, TextureWrapMode.Clamp,
                name: "_AerialFogColorCopy");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null || _copiedColor == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, _sampler))
            {
                var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

                // 先把当前颜色拷一份，后面片元要从这里采样。
                Blitter.BlitCameraTexture(cmd, source, _copiedColor);

                PushParams();

                CoreUtils.SetRenderTarget(cmd, source);

                _blitProps.Clear();
                _blitProps.SetTexture(ID_BlitTexture, _copiedColor);
                _blitProps.SetVector(ID_BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));

                cmd.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, 3, 1, _blitProps);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void PushParams()
        {
            _material.SetFloat(ID_FogStart, _params.fogStart);
            _material.SetFloat(ID_FogEnd, _params.fogEnd);
            _material.SetFloat(ID_FogCurve, _params.fogCurve);
            _material.SetFloat(ID_Density, _params.density);
            _material.SetFloat(ID_HeightFalloff, _params.heightFalloff);
            _material.SetFloat(ID_BaseHeight, _params.baseHeight);
            _material.SetFloat(ID_MaxOpacity, _params.maxOpacity);
            _material.SetColor(ID_ColorNear, _params.colorNear);
            _material.SetColor(ID_ColorFar, _params.colorFar);
            _material.SetFloat(ID_SkyFogAmount, _params.skyFogAmount);
            _material.SetFloat(ID_DebugMode, _debugMode);

            _material.SetFloat(ID_SunInfluence, _params.sunInfluence);
            var sun = RenderSettings.sun;
            _material.SetColor(ID_SunColor, sun != null ? sun.color : Color.white);

            PushGlobalParams();
        }

        /// <summary>
        /// 同一组参数再往全局推一份，给"自己吃雾"的物体 shader 用
        /// （AerialFogCommon.hlsl 里的 AerialFogApply，主要是 Transparent 队列的东西）。
        /// 名字加 _AerialFog 前缀，避免和材质属性撞。
        /// </summary>
        void PushGlobalParams()
        {
            Shader.SetGlobalFloat(ID_FogStartGlobal, _params.fogStart);
            Shader.SetGlobalFloat(ID_FogEndGlobal, _params.fogEnd);
            Shader.SetGlobalFloat(ID_FogCurveGlobal, _params.fogCurve);
            Shader.SetGlobalFloat(ID_DensityGlobal, _params.density);
            Shader.SetGlobalFloat(ID_HeightFalloffGlobal, _params.heightFalloff);
            Shader.SetGlobalFloat(ID_BaseHeightGlobal, _params.baseHeight);
            Shader.SetGlobalFloat(ID_MaxOpacityGlobal, _params.maxOpacity);
            Shader.SetGlobalColor(ID_ColorNearGlobal, _params.colorNear);
            Shader.SetGlobalColor(ID_ColorFarGlobal, _params.colorFar);
        }

        /// <summary>
        /// 主光只取 Lighting 面板里指定的 Sun，不做全场景搜索。
        /// 没指定就是 null，雾不染色，零额外开销。
        /// </summary>
        Light ResolveSun()
        {
            return RenderSettings.sun;
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose()
        {
            _copiedColor?.Release();
            _copiedColor = null;
        }
    }
}
