using System.Collections;
using System.Collections.Generic;
using LetsPiu.Base;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace LetsPiu.Graphics
{
    /// <summary>
    /// S4 ScoreCardFightPage20v20 专用性能探针：在真机上逐个切换渲染配置，每档测 3 秒后输出对比表。
    /// 依赖本页特有的 PlayerItem 卡片与 VFX_Icon_Hp 血条结构，其它页面不可用。
    /// gpuFrameTime 需要 Player Settings 勾选 Enable Frame Timing Stats 才有值。
    /// </summary>
    public class S4Probe : MonoBehaviour
    {
        const string LogTag = "S4Probe";

        // 留出时间关掉 UIDEVPanel 和 scrcpy 投屏，两者都会明显干扰测量结果。
        const float StartDelaySeconds = 10f;
        const float WarmupSeconds = 1f;
        const float SampleSeconds = 3f;

        struct Sample
        {
            public string name;
            public float fps;
            public float frameMs;
            public double cpuMainMs;
            public double cpuRenderMs;
            public double gpuMs;
            public long drawCalls;
            public long setPassCalls;
            public long batches;
        }

        UniversalRenderPipelineAsset m_Urp;
        UniversalAdditionalCameraData m_CameraData;

        float m_OrigRenderScale;
        bool m_OrigHdr;
        bool m_OrigDepthTexture;
        float m_OrigShadowDistance;
        bool m_OrigPostFx;

        // 只记录探针启动时本来就处于启用状态的组件，避免恢复阶段把场景里
        // 故意关掉的特效（例如未获得的 buff 图标）意外打开。
        readonly List<Animator> m_Animators = new List<Animator>();
        readonly List<ParticleSystem> m_Particles = new List<ParticleSystem>();
        readonly List<ParticleSystemRenderer> m_ParticleRenderers = new List<ParticleSystemRenderer>();
        readonly List<Canvas> m_Canvases = new List<Canvas>();
        bool m_ParticlesPaused;

        // 按绘制类型拆分，用来定位 overdraw 到底是谁贡献的。
        // MeshRenderer 是卡片和 topbar 里的网格特效，既不属于 Canvas 也不属于粒子系统。
        readonly List<MeshRenderer> m_MeshRenderers = new List<MeshRenderer>();
        readonly List<Graphic> m_Images = new List<Graphic>();
        readonly List<Graphic> m_RawImages = new List<Graphic>();
        readonly List<Graphic> m_Texts = new List<Graphic>();
        readonly List<GameObject> m_PlayerCards = new List<GameObject>();

        // 每张玩家卡片的血条特效各一个 MeshRenderer，实测是本页面最大的填充率开销，
        // 单独拆出来便于随时量它当前还占多少。
        const string HpVfxRootName = "VFX_Icon_Hp";

        readonly List<MeshRenderer> m_HpRenderers = new List<MeshRenderer>();

        ProfilerRecorder m_DrawCalls;
        ProfilerRecorder m_SetPassCalls;
        ProfilerRecorder m_Batches;

        readonly List<Sample> m_Samples = new List<Sample>();
        readonly FrameTiming[] m_Timings = new FrameTiming[1];

        public static void Run()
        {
            if (FindObjectOfType<S4Probe>() != null)
            {
                Debug.LogWarning($"[{LogTag}] 已在运行，忽略本次请求");
                return;
            }

            new GameObject(LogTag).AddComponent<S4Probe>();
        }

        void Start()
        {
            m_Urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (m_Urp == null)
            {
                Debug.LogError($"[{LogTag}] 当前渲染管线不是 URP，无法探测");
                Destroy(gameObject);
                return;
            }

            var mainCamera = CameraMgr.S ? CameraMgr.S.mainCamera : Camera.main;
            m_CameraData = mainCamera ? mainCamera.GetComponent<UniversalAdditionalCameraData>() : null;

            m_OrigRenderScale = m_Urp.renderScale;
            m_OrigHdr = m_Urp.supportsHDR;
            m_OrigDepthTexture = m_Urp.supportsCameraDepthTexture;
            m_OrigShadowDistance = m_Urp.shadowDistance;
            m_OrigPostFx = m_CameraData != null && m_CameraData.renderPostProcessing;

            m_DrawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            m_SetPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            m_Batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");

            StartCoroutine(Sweep());
        }

        void OnDestroy()
        {
            if (m_DrawCalls.Valid) m_DrawCalls.Dispose();
            if (m_SetPassCalls.Valid) m_SetPassCalls.Dispose();
            if (m_Batches.Valid) m_Batches.Dispose();
        }

        void CollectSceneComponents()
        {
            foreach (var a in FindObjectsOfType<Animator>())
                if (a.enabled) m_Animators.Add(a);

            foreach (var p in FindObjectsOfType<ParticleSystem>())
            {
                m_Particles.Add(p);

                var r = p.GetComponent<ParticleSystemRenderer>();
                if (r && r.enabled) m_ParticleRenderers.Add(r);
            }

            foreach (var c in FindObjectsOfType<Canvas>())
                if (c.enabled) m_Canvases.Add(c);

            foreach (var r in FindObjectsOfType<MeshRenderer>())
            {
                if (!r.enabled) continue;
                m_MeshRenderers.Add(r);

                if (IsUnderHpVfx(r.transform)) m_HpRenderers.Add(r);
            }

            foreach (var g in FindObjectsOfType<Graphic>())
            {
                if (!g.enabled) continue;

                if (g is Image) m_Images.Add(g);
                else if (g is RawImage) m_RawImages.Add(g);
                else m_Texts.Add(g);
            }

            // 卡片实例名形如 PlayerItem20v20 / PlayerItem20v20(Clone)，前缀匹配即可覆盖各页面变体。
            foreach (var t in FindObjectsOfType<Transform>())
                if (t.gameObject.activeSelf && t.name.StartsWith("PlayerItem"))
                    m_PlayerCards.Add(t.gameObject);
        }

        static bool IsUnderHpVfx(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
                if (p.name == HpVfxRootName) return true;
            return false;
        }

        /// <summary>
        /// 逐行单独输出。多行拼成一条 Debug.Log 会被 Android logcat 的单条长度上限截断，
        /// 而且只有首行带 [S4Probe]，按关键字过滤时后续行会丢。
        /// </summary>
        static void Line(string text) => Debug.Log($"[{LogTag}] {text}");

        void LogEnvironment()
        {
            Line("========== 运行环境 (S4 20v20) ==========");
            Line($"分辨率        : {Screen.width}x{Screen.height} = {Screen.width * Screen.height / 1000000f:F2}M 像素  刷新率 {Screen.currentResolution.refreshRateRatio.value:F1}Hz");
            Line($"图形 API      : {SystemInfo.graphicsDeviceType}  {SystemInfo.graphicsDeviceVersion}");
            Line($"GPU / 显存    : {SystemInfo.graphicsDeviceName}  {SystemInfo.graphicsMemorySize}MB");
            Line($"多线程渲染    : {SystemInfo.graphicsMultiThreaded}");
            Line($"帧率控制      : targetFrameRate={Application.targetFrameRate}  vSyncCount={QualitySettings.vSyncCount}");
            Line($"URP 资产      : {m_Urp.name}");
            Line($"  renderScale={m_Urp.renderScale}  MSAA={m_Urp.msaaSampleCount}  HDR={m_Urp.supportsHDR}");
            Line($"  DepthTexture={m_Urp.supportsCameraDepthTexture}  OpaqueTexture={m_Urp.supportsCameraOpaqueTexture}");
            Line($"  shadowDistance={m_Urp.shadowDistance}  SRPBatcher={m_Urp.useSRPBatcher}");
            Line($"主相机后处理  : {(m_CameraData != null ? m_CameraData.renderPostProcessing.ToString() : "无 CameraData")}");
            Line($"启用中的组件  : Animator={m_Animators.Count} ParticleSystem={m_Particles.Count} Canvas={m_Canvases.Count}");
            Line($"绘制类型统计  : MeshRenderer={m_MeshRenderers.Count} Image={m_Images.Count} 文字={m_Texts.Count} RawImage={m_RawImages.Count} 玩家卡片={m_PlayerCards.Count}");
            Line($"血条特效      : {m_HpRenderers.Count} 个" + (m_HpRenderers.Count > 0
                ? $"  quad 缩放 {m_HpRenderers[0].transform.localScale.x:F0}x{m_HpRenderers[0].transform.localScale.y:F0}"
                : ""));
        }

        /// <summary>
        /// 网格特效被实测为本页面 overdraw 的主要来源，按屏幕覆盖面积倒序列出，
        /// 覆盖面积大的半透明层就是优先要砍的对象。
        /// </summary>
        void LogMeshRendererInventory()
        {
            var cam = CameraMgr.S ? CameraMgr.S.mainCamera : Camera.main;
            if (cam == null) return;

            float screenArea = Screen.width * (float)Screen.height;
            var rows = new List<KeyValuePair<float, string>>(m_MeshRenderers.Count);

            foreach (var r in m_MeshRenderers)
            {
                if (!r) continue;

                float coverage = EstimateScreenArea(cam, r.bounds) / screenArea;
                var mat = r.sharedMaterial;
                string shader = mat && mat.shader ? mat.shader.name : "(无材质)";
                rows.Add(new KeyValuePair<float, string>(coverage,
                    $"{coverage * 100f,6:F1}%  {shader,-46}{GetHierarchyPath(r.transform)}"));
            }

            rows.Sort((a, b) => b.Key.CompareTo(a.Key));

            Line($"---------- 网格特效清单（{rows.Count} 个，按屏幕覆盖倒序）----------");
            foreach (var row in rows) Line(row.Value);
        }

        /// <summary>
        /// 把世界包围盒的 8 个角投影成屏幕 AABB 再裁到视口内，只求量级，不追求精确。
        /// </summary>
        internal static float EstimateScreenArea(Camera cam, Bounds b)
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < 8; i++)
            {
                var sign = new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1);
                var sp = cam.WorldToScreenPoint(b.center + Vector3.Scale(b.extents, sign));
                if (sp.z < 0f) continue;

                min = Vector2.Min(min, sp);
                max = Vector2.Max(max, sp);
            }

            if (max.x <= min.x || max.y <= min.y) return 0f;

            min = Vector2.Max(min, Vector2.zero);
            max = Vector2.Min(max, new Vector2(Screen.width, Screen.height));
            return Mathf.Max(0f, max.x - min.x) * Mathf.Max(0f, max.y - min.y);
        }

        static string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            for (var p = t.parent; p != null; p = p.parent)
                path = p.name + "/" + path;
            return path;
        }

        IEnumerator Sweep()
        {
            yield return CountdownBeforeStart();

            // 组件收集必须在倒计时之后，否则会把 UIDEVPanel 自己的 Canvas / Animator 统计进来。
            CollectSceneComponents();
            LogEnvironment();
            LogMeshRendererInventory();

            // 已测明的结论，不必再重复验证：UI 各层、Animator、粒子，以及后处理 / HDR /
            // DepthTexture / 阴影，增益全部落在 ±3 fps 的噪声带内；改 URP 资产标记那几档
            // 还会在 GLES 上把管线搞坏。所以这里只保留四档：血条特效改完后量它还剩多少，
            // 以及 renderScale 这个跨轮可比的锚点。
            yield return Measure("0 基线", null);
            yield return Measure("1 关血条特效", () => SetHpRenderers(false));
            yield return Measure("2 关全部网格特效(对照)", () => SetMeshRenderers(false));
            yield return Measure("3 renderScale=0.5(跨轮锚点)", () => m_Urp.renderScale = 0.5f);

            RestoreBaseline();
            LogSummary();
            Destroy(gameObject);
        }

        IEnumerator CountdownBeforeStart()
        {
            Debug.Log($"[{LogTag}] {StartDelaySeconds:F0} 秒后开始，请关闭 UIDEVPanel 与 scrcpy 投屏");

            for (int remain = (int)StartDelaySeconds; remain > 0; remain--)
            {
                float deadline = Time.unscaledTime + 1f;
                while (Time.unscaledTime < deadline) yield return null;

                if (remain <= 5 || remain % 5 == 0)
                    Debug.Log($"[{LogTag}] 倒计时 {remain - 1} 秒");
            }

            Debug.Log($"[{LogTag}] 开始测量");
        }

        IEnumerator Measure(string label, System.Action apply)
        {
            RestoreBaseline();
            apply?.Invoke();

            float deadline = Time.unscaledTime + WarmupSeconds;
            while (Time.unscaledTime < deadline) yield return null;

            int frames = 0;
            double cpuMain = 0, cpuRender = 0, gpu = 0;
            long drawCalls = 0, setPass = 0, batches = 0;
            float start = Time.unscaledTime;

            while (Time.unscaledTime - start < SampleSeconds)
            {
                yield return null;
                frames++;

                FrameTimingManager.CaptureFrameTimings();
                if (FrameTimingManager.GetLatestTimings(1, m_Timings) > 0)
                {
                    cpuMain += m_Timings[0].cpuMainThreadFrameTime;
                    cpuRender += m_Timings[0].cpuRenderThreadFrameTime;
                    gpu += m_Timings[0].gpuFrameTime;
                }

                if (m_DrawCalls.Valid) drawCalls += m_DrawCalls.LastValue;
                if (m_SetPassCalls.Valid) setPass += m_SetPassCalls.LastValue;
                if (m_Batches.Valid) batches += m_Batches.LastValue;
            }

            float elapsed = Time.unscaledTime - start;
            if (frames == 0) frames = 1;

            m_Samples.Add(new Sample
            {
                name = label,
                fps = frames / elapsed,
                frameMs = elapsed * 1000f / frames,
                cpuMainMs = cpuMain / frames,
                cpuRenderMs = cpuRender / frames,
                gpuMs = gpu / frames,
                drawCalls = drawCalls / frames,
                setPassCalls = setPass / frames,
                batches = batches / frames,
            });

            Debug.Log($"[{LogTag}] {label} 完成: {frames / elapsed:F1} fps");
        }

        void RestoreBaseline()
        {
            m_Urp.renderScale = m_OrigRenderScale;
            m_Urp.supportsHDR = m_OrigHdr;
            m_Urp.supportsCameraDepthTexture = m_OrigDepthTexture;
            m_Urp.shadowDistance = m_OrigShadowDistance;
            SetPostFx(m_OrigPostFx);
            SetEnabled(m_Animators, true);
            SetEnabled(m_Canvases, true);
            SetEnabled(m_Images, true);
            SetEnabled(m_Texts, true);
            SetEnabled(m_RawImages, true);
            SetParticles(true);
            SetMeshRenderers(true);
            SetPlayerCards(true);
        }

        void SetMeshRenderers(bool on)
        {
            foreach (var r in m_MeshRenderers)
                if (r) r.enabled = on;
        }

        void SetHpRenderers(bool on)
        {
            foreach (var r in m_HpRenderers)
                if (r) r.enabled = on;
        }

        void SetPlayerCards(bool on)
        {
            foreach (var go in m_PlayerCards)
                if (go && go.activeSelf != on) go.SetActive(on);
        }

        void SetPostFx(bool on)
        {
            if (m_CameraData != null) m_CameraData.renderPostProcessing = on;
        }

        /// <summary>
        /// Pause 掉模拟并关掉 renderer，同时切掉粒子的 CPU 更新和 draw call。
        /// 只在真的暂停过之后才 Play，避免把原本已播完的一次性特效重新点起来。
        /// </summary>
        void SetParticles(bool on)
        {
            if (on == !m_ParticlesPaused) return;
            m_ParticlesPaused = !on;

            foreach (var p in m_Particles)
            {
                if (!p) continue;
                if (on) p.Play();
                else p.Pause();
            }

            foreach (var r in m_ParticleRenderers)
                if (r) r.enabled = on;
        }

        static void SetEnabled<T>(List<T> components, bool on) where T : Behaviour
        {
            foreach (var c in components)
                if (c) c.enabled = on;
        }

        void LogSummary()
        {
            if (m_Samples.Count == 0) return;

            Line("========== 结果汇总 ==========");
            Line($"分辨率 {Screen.width}x{Screen.height} = {Screen.width * Screen.height / 1000000f:F2}M 像素");
            Line("场景                                        FPS   帧耗时  CPU主线  CPU渲染      GPU DrawCall SetPass Batches  增益");

            // 关掉某类物体时 DrawCall 本就会降，但如果降到基线的一半以下，
            // 更可能是管线被改坏、画面根本没渲染出来，这一档的帧率提升是假的。
            var baseline = m_Samples[0];

            foreach (var s in m_Samples)
            {
                float gain = s.fps - baseline.fps;
                bool suspect = baseline.drawCalls > 0 && s.drawCalls * 2 < baseline.drawCalls;
                string flag = suspect ? "  <== DrawCall 异常偏低，画面可能没渲染，本档不可信" : "";

                Line($"{s.name,-42}{s.fps,5:F1}{s.frameMs,8:F1}{s.cpuMainMs,9:F1}{s.cpuRenderMs,9:F1}{s.gpuMs,9:F1}"
                     + $"{s.drawCalls,9}{s.setPassCalls,8}{s.batches,8}{gain,7:+0.0;-0.0}{flag}");
            }

            if (baseline.gpuMs <= 0.0)
                Line("注意：CPU/GPU 三列为 0 是因为未勾选 Enable Frame Timing Stats，仅看 FPS 与帧耗时即可");

            Line("判读：基线与档1 的帧耗时之差就是血条特效当前的净成本，优化前实测为 17.4ms");
            Line("      档1 与档2 接近 → 网格特效开销仍然基本全在血条上");
            Line("      档3 仍有大幅提升 → 逐像素 GPU 成本还是瓶颈，继续找面积大的半透明层");
        }
    }
}
