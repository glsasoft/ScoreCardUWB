using System;
using System.Collections.Generic;
using System.Text;
using LetsPiu.Base;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace LetsPiu.Graphics
{
    /// <summary>
    /// 真机帧率排查开关：按类别隐藏特效、UI、后处理，配合屏幕上的性能 HUD 二分定位掉帧来源。
    /// 只记录自己关掉的组件，恢复时只还原这些，不会误开原本就关着的东西。
    /// 开启期间每秒重新扫描一次，页面切换后新出现的对象也会被关掉。
    /// </summary>
    public class PerfSwitch : MonoBehaviour
    {
        public enum Item
        {
            MeshRenderer,
            Particle,
            VfxAll,
            VfxSpeedMod,
            VfxStartLoop,
            VfxPlayerScreen,
            VfxSingle,
            UiImage,
            UiText,
            UiRawImage,
            UiMask,
            UiEffect,
            UiCanvas,
            Animator,
            PostFx,
            HalfRenderScale,
        }

        const string LogTag = "[PerfSwitch] ";
        const string VfxPrefix = "VFX_";
        const float RescanIntervalSec = 1f;
        const float HudRefreshSec = 0.5f;

        static PerfSwitch s_Instance;
        static Transform s_ExcludeRoot;

        static PerfSwitch Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = new GameObject("PerfSwitch");
                    DontDestroyOnLoad(go);
                    s_Instance = go.AddComponent<PerfSwitch>();
                }
                return s_Instance;
            }
        }

        readonly HashSet<Item> m_On = new();
        readonly Dictionary<Item, List<Renderer>> m_Renderers = new();
        readonly Dictionary<Item, List<Behaviour>> m_Behaviours = new();
        readonly List<ParticleSystem> m_PausedParticles = new();

        float m_RescanTimer;
        string m_CycleName;
        int m_CycleIndex = -1;

        sealed class PieceState
        {
            public Transform Root;
            public bool Subtree;
            public bool On;
            public readonly List<Renderer> Hidden = new();
        }

        readonly Dictionary<string, PieceState> m_Pieces = new();

        bool m_PostFxSaved;
        bool m_OrigPostFx;
        bool m_ScaleSaved;
        float m_OrigRenderScale;
        bool m_FeaturesPushed;

        bool m_Hud;
        ProfilerRecorder m_DrawCalls;
        ProfilerRecorder m_SetPassCalls;
        ProfilerRecorder m_Batches;
        readonly FrameTiming[] m_Timings = new FrameTiming[1];
        readonly StringBuilder m_Sb = new();
        GUIStyle m_HudStyle;
        string m_HudText = string.Empty;
        int m_HudFrames;
        float m_HudElapsed;

        /// <summary>
        /// 指定不参与 UI 类开关的根节点，一般传调试面板自己，避免把开关按钮也关掉。
        /// </summary>
        public static void SetExcludeRoot(Transform root) => s_ExcludeRoot = root;

        public static void Set(Item item, bool on) => Instance.SetInternal(item, on);

        public static void SetHud(bool on) => Instance.SetHudInternal(on);

        public static void LogReport() => Instance.Report();

        /// <summary>
        /// 依次单独隐藏场景里每个 VFX_ 根节点，返回按钮要显示的文字。走完一圈后全部恢复。
        /// </summary>
        public static string CycleVfx() => Instance.CycleVfxInternal();

        public struct SpeedPieceInfo
        {
            public string Key;
            public string Label;
            public bool Hidden;
        }

        /// <summary>
        /// 把「竞速入场特效SpeedMod」实际关掉的每棵特效拆开，每个带渲染器的子节点单独一条。
        /// </summary>
        public static List<SpeedPieceInfo> RefreshSpeedShootingPieces() => Instance.RefreshSpeedPieces();

        public static void SetSpeedPiece(string key, bool on) => Instance.SetPiece(key, on);

        /// <summary>
        /// 调试期会改动 RendererFeature 的 isActive，这里先存一份，退出时还原，避免编辑器里改脏资源。
        /// </summary>
        public static void ProtectRendererFeatures() => Instance.ProtectFeaturesInternal();

        /// <summary>
        /// 返回所有 RendererFeature 的名称与当前开关状态，调用方不需要引用 URP 程序集。
        /// 初始化失败时返回 null。
        /// </summary>
        public static List<KeyValuePair<string, bool>> GetRendererFeatureStates()
        {
            var manager = RendererFeatureManager.Instance;
            if (!manager.initialized)
            {
                Debug.LogWarning($"{LogTag}RendererFeature 不可用: {manager.errorMessage}");
                return null;
            }

            var list = new List<KeyValuePair<string, bool>>();
            foreach (var pair in manager.rendererFeatures)
                list.Add(new KeyValuePair<string, bool>(pair.Key, pair.Value.isActive));
            return list;
        }

        public static void SetRendererFeatureActive(string featureName, bool active)
        {
            if (RendererFeatureManager.Instance.rendererFeatures.TryGetValue(featureName, out var feature))
                feature.SetActive(active);
        }

        void Update()
        {
            if (m_Hud) UpdateHud();

            bool pieceOn = AnyPieceOn();
            if (m_On.Count == 0 && !pieceOn) return;
            m_RescanTimer += Time.unscaledDeltaTime;
            if (m_RescanTimer < RescanIntervalSec) return;

            m_RescanTimer = 0f;
            foreach (var item in m_On) Apply(item);
            if (!pieceOn) return;
            foreach (var state in m_Pieces.Values)
                if (state.On) ApplyPiece(state);
        }

        void OnDestroy() => RestoreEverything();

        void OnApplicationQuit() => RestoreEverything();

        void SetInternal(Item item, bool on)
        {
            if (on)
            {
                m_On.Add(item);
                Apply(item);
            }
            else
            {
                m_On.Remove(item);
                Restore(item);
            }
            Debug.Log($"{LogTag}{item} = {(on ? "关闭(隐藏)" : "恢复")}");
        }

        void Apply(Item item)
        {
            switch (item)
            {
                case Item.MeshRenderer:
                    foreach (var r in FindObjectsOfType<MeshRenderer>()) Hide(item, r);
                    break;
                case Item.Particle:
                    HideParticles(item);
                    break;
                case Item.VfxAll:
                    HideVfx(item, _ => true);
                    break;
                case Item.VfxSpeedMod:
                    HideVfx(item, n => n.StartsWith("VFX_StartScreen_Start_SpeedMod", StringComparison.Ordinal));
                    break;
                case Item.VfxStartLoop:
                    HideVfx(item, n => n.StartsWith("VFX_StartScreen_Loop_", StringComparison.Ordinal));
                    break;
                case Item.VfxPlayerScreen:
                    HideVfx(item, n => n.StartsWith("VFX_PlayerScreen_", StringComparison.Ordinal));
                    break;
                case Item.VfxSingle:
                    HideVfx(item, n => n == m_CycleName);
                    break;
                case Item.UiImage:
                    foreach (var g in FindObjectsOfType<Image>()) Hide(item, g);
                    break;
                case Item.UiText:
                    foreach (var g in FindObjectsOfType<Graphic>())
                        if (g is not Image && g is not RawImage) Hide(item, g);
                    break;
                case Item.UiRawImage:
                    foreach (var g in FindObjectsOfType<RawImage>()) Hide(item, g);
                    break;
                case Item.UiMask:
                    foreach (var m in FindObjectsOfType<Mask>()) Hide(item, m);
                    foreach (var m in FindObjectsOfType<RectMask2D>()) Hide(item, m);
                    break;
                case Item.UiEffect:
                    foreach (var e in FindObjectsOfType<BaseMeshEffect>()) Hide(item, e);
                    break;
                case Item.UiCanvas:
                    foreach (var c in FindObjectsOfType<Canvas>()) Hide(item, c);
                    break;
                case Item.Animator:
                    foreach (var a in FindObjectsOfType<Animator>()) Hide(item, a);
                    break;
                case Item.PostFx:
                    ApplyPostFx();
                    break;
                case Item.HalfRenderScale:
                    ApplyHalfRenderScale();
                    break;
            }
        }

        void Restore(Item item)
        {
            if (m_Renderers.TryGetValue(item, out var renderers))
            {
                foreach (var r in renderers)
                    if (r != null) r.enabled = true;
                renderers.Clear();
            }

            if (m_Behaviours.TryGetValue(item, out var behaviours))
            {
                foreach (var b in behaviours)
                    if (b != null) b.enabled = true;
                behaviours.Clear();
            }

            switch (item)
            {
                case Item.Particle:
                    foreach (var p in m_PausedParticles)
                        if (p != null) p.Play();
                    m_PausedParticles.Clear();
                    break;
                case Item.PostFx:
                    RestorePostFx();
                    break;
                case Item.HalfRenderScale:
                    RestoreRenderScale();
                    break;
            }
        }

        void RestoreEverything()
        {
            foreach (var item in new List<Item>(m_On)) Restore(item);
            m_On.Clear();
            foreach (var state in m_Pieces.Values)
            {
                RestorePiece(state);
                state.On = false;
            }

            if (m_FeaturesPushed)
            {
                m_FeaturesPushed = false;
                RendererFeatureManager.Instance.Pop();
            }

            DisposeRecorders();
        }

        static bool Excluded(Component c)
        {
            if (s_ExcludeRoot == null) return false;
            var t = c.transform;
            return t.IsChildOf(s_ExcludeRoot) || s_ExcludeRoot.IsChildOf(t);
        }

        static List<T> GetList<T>(Dictionary<Item, List<T>> map, Item item)
        {
            if (!map.TryGetValue(item, out var list)) map[item] = list = new List<T>();
            return list;
        }

        void Hide(Item item, Renderer r)
        {
            if (r == null || !r.enabled || Excluded(r)) return;
            r.enabled = false;
            GetList(m_Renderers, item).Add(r);
        }

        void Hide(Item item, Behaviour b)
        {
            if (b == null || !b.enabled || Excluded(b)) return;
            b.enabled = false;
            GetList(m_Behaviours, item).Add(b);
        }

        void HideParticles(Item item)
        {
            foreach (var p in FindObjectsOfType<ParticleSystem>())
            {
                if (Excluded(p)) continue;
                Hide(item, p.GetComponent<ParticleSystemRenderer>());
                if (!p.isPlaying) continue;
                p.Pause();
                m_PausedParticles.Add(p);
            }
        }

        void HideVfx(Item item, Func<string, bool> match)
        {
            foreach (var pair in CollectVfxRoots())
            {
                if (!match(pair.Key)) continue;
                foreach (var r in pair.Value) Hide(item, r);
            }
        }

        static Dictionary<string, List<Renderer>> CollectVfxRoots()
        {
            var map = new Dictionary<string, List<Renderer>>();
            foreach (var r in FindObjectsOfType<Renderer>())
            {
                if (Excluded(r)) continue;
                var root = FindVfxRoot(r.transform);
                if (root == null) continue;
                if (!map.TryGetValue(root.name, out var list)) map[root.name] = list = new List<Renderer>();
                list.Add(r);
            }
            return map;
        }

        static Transform FindVfxRoot(Transform t)
        {
            Transform root = null;
            for (var p = t; p != null; p = p.parent)
                if (p.name.StartsWith(VfxPrefix, StringComparison.Ordinal)) root = p;
            return root;
        }

        const string SpeedModPrefix = "VFX_StartScreen_Start_SpeedMod";

        bool AnyPieceOn()
        {
            foreach (var state in m_Pieces.Values)
                if (state.On) return true;
            return false;
        }

        void SetPiece(string key, bool on)
        {
            if (!m_Pieces.TryGetValue(key, out var state)) return;
            if (state.On == on) return;
            state.On = on;
            if (on) ApplyPiece(state);
            else RestorePiece(state);
            Debug.Log($"{LogTag}S2-2 {key} = {(on ? "关闭(隐藏)" : "恢复")}");
        }

        void ApplyPiece(PieceState state)
        {
            if (state.Root == null) return;
            var renderers = state.Subtree
                ? state.Root.GetComponentsInChildren<Renderer>(true)
                : state.Root.GetComponents<Renderer>();
            foreach (var r in renderers)
            {
                if (r == null || !r.enabled || Excluded(r) || state.Hidden.Contains(r)) continue;
                r.enabled = false;
                state.Hidden.Add(r);
            }
        }

        static void RestorePiece(PieceState state)
        {
            foreach (var r in state.Hidden)
                if (r != null) r.enabled = true;
            state.Hidden.Clear();
        }

        List<SpeedPieceInfo> RefreshSpeedPieces()
        {
            var list = new List<SpeedPieceInfo>();
            var roots = FindSpeedModRoots();
            if (roots.Count == 0)
            {
                Debug.Log($"{LogTag}场景里没有 {SpeedModPrefix}，进入竞速靶场后再拆分");
                return list;
            }

            var seen = new HashSet<string>();
            foreach (var effectRoot in roots)
            {
                string title = effectRoot.name.EndsWith("_Card", StringComparison.Ordinal) ? "SpeedMod卡片" : "SpeedMod";
                var folder = effectRoot;
                for (int i = 0; i < effectRoot.childCount; i++)
                {
                    var child = effectRoot.GetChild(i);
                    if (child.name != "VFX") continue;
                    folder = child;
                    break;
                }

                for (int i = 0; i < folder.childCount; i++)
                {
                    var child = folder.GetChild(i);
                    int childCount = CountRenderers(child, true);
                    if (childCount == 0) continue;
                    AddSpeedPiece(list, seen, title, effectRoot, child, true);
                    if (childCount <= 1) continue;
                    foreach (var node in child.GetComponentsInChildren<Transform>(true))
                    {
                        if (node == child) continue;
                        int own = CountRenderers(node, false);
                        int sub = CountRenderers(node, true);
                        if (own > 0 && sub == own)
                            AddSpeedPiece(list, seen, title, effectRoot, node, false);
                        else if (sub > 1)
                            AddSpeedPiece(list, seen, title, effectRoot, node, true);
                    }
                }
            }

            var stale = new List<string>();
            foreach (var key in m_Pieces.Keys)
                if (!seen.Contains(key)) stale.Add(key);
            foreach (var key in stale)
            {
                RestorePiece(m_Pieces[key]);
                m_Pieces.Remove(key);
            }

            Debug.Log($"{LogTag}SpeedMod 子特效 {list.Count} 个，已拆成单独开关");
            return list;
        }

        List<Transform> FindSpeedModRoots()
        {
            var result = new List<Transform>();
            var seen = new HashSet<int>();
            foreach (var r in FindObjectsOfType<Renderer>(true))
            {
                if (r == null || Excluded(r)) continue;
                var root = FindVfxRoot(r.transform);
                if (root == null || !root.name.StartsWith(SpeedModPrefix, StringComparison.Ordinal)) continue;
                if (!seen.Add(root.GetInstanceID())) continue;
                if (!KeepSpeedModRoot(root)) continue;
                result.Add(root);
            }
            result.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return result;
        }

        static bool KeepSpeedModRoot(Transform root)
        {
            if (root.gameObject.activeInHierarchy) return true;
            for (var p = root; p != null; p = p.parent)
                if (p.name == "SpeedShootingTeamPage")
                    return p.gameObject.activeInHierarchy;
            return false;
        }

        void AddSpeedPiece(List<SpeedPieceInfo> list, HashSet<string> seen, string title, Transform effectRoot, Transform node, bool subtree)
        {
            string rel = RelativePath(effectRoot, node);
            string baseKey = string.IsNullOrEmpty(rel) ? title : title + "/" + rel;
            string key = baseKey;
            int dup = 2;
            while (!seen.Add(key)) key = baseKey + "#" + dup++;

            int mesh = 0, particle = 0, other = 0;
            var renderers = subtree ? node.GetComponentsInChildren<Renderer>(true) : node.GetComponents<Renderer>();
            foreach (var r in renderers)
            {
                if (r is MeshRenderer) mesh++;
                else if (r is ParticleSystemRenderer) particle++;
                else other++;
            }
            if (mesh + particle + other == 0) return;

            if (!m_Pieces.TryGetValue(key, out var state))
                m_Pieces[key] = state = new PieceState();
            state.Subtree = subtree;
            if (state.Root != node)
            {
                RestorePiece(state);
                state.Root = node;
                if (state.On) ApplyPiece(state);
            }

            string label = $"{title}/{rel} {DescribeRenderers(mesh, particle, other)}";
            list.Add(new SpeedPieceInfo { Key = key, Label = label, Hidden = state.On });
        }

        static int CountRenderers(Transform node, bool subtree)
        {
            return subtree ? node.GetComponentsInChildren<Renderer>(true).Length : node.GetComponents<Renderer>().Length;
        }

        static string DescribeRenderers(int mesh, int particle, int other)
        {
            var sb = new StringBuilder();
            if (mesh > 0) sb.Append("网格").Append(mesh);
            if (particle > 0)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append("粒子").Append(particle);
            }
            if (other > 0)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append("其他").Append(other);
            }
            return sb.ToString();
        }

        static string RelativePath(Transform root, Transform node)
        {
            var names = new Stack<string>();
            for (var p = node; p != null && p != root; p = p.parent)
            {
                if (p.name == "VFX" && p.parent == root) continue;
                names.Push(p.name);
            }
            return string.Join("/", names);
        }

        string CycleVfxInternal()
        {
            SetInternal(Item.VfxSingle, false);

            var names = new List<string>(CollectVfxRoots().Keys);
            names.Sort(StringComparer.Ordinal);

            int next = m_CycleIndex + 1;
            if (next >= names.Count)
            {
                m_CycleIndex = -1;
                return $"⏭ 逐个隐藏VFX(共{names.Count}个,已全部显示,点击从头开始)";
            }

            m_CycleIndex = next;
            m_CycleName = names[next];
            SetInternal(Item.VfxSingle, true);
            return $"⏭ 正在隐藏 [{next + 1}/{names.Count}] {m_CycleName}";
        }

        static UniversalAdditionalCameraData GetCameraData()
        {
            var cam = CameraMgr.S ? CameraMgr.S.mainCamera : Camera.main;
            return cam ? cam.GetComponent<UniversalAdditionalCameraData>() : null;
        }

        void ApplyPostFx()
        {
            var data = GetCameraData();
            if (data == null) return;
            if (!m_PostFxSaved)
            {
                m_OrigPostFx = data.renderPostProcessing;
                m_PostFxSaved = true;
            }
            data.renderPostProcessing = false;
        }

        void RestorePostFx()
        {
            if (!m_PostFxSaved) return;
            m_PostFxSaved = false;
            var data = GetCameraData();
            if (data != null) data.renderPostProcessing = m_OrigPostFx;
        }

        void ApplyHalfRenderScale()
        {
            if (GraphicsSettings.currentRenderPipeline is not UniversalRenderPipelineAsset urp) return;
            if (!m_ScaleSaved)
            {
                m_OrigRenderScale = urp.renderScale;
                m_ScaleSaved = true;
            }
            urp.renderScale = 0.5f;
        }

        void RestoreRenderScale()
        {
            if (!m_ScaleSaved) return;
            m_ScaleSaved = false;
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
                urp.renderScale = m_OrigRenderScale;
        }

        void ProtectFeaturesInternal()
        {
            if (m_FeaturesPushed) return;
            m_FeaturesPushed = true;
            RendererFeatureManager.Instance.Push();
        }

        void SetHudInternal(bool on)
        {
            m_Hud = on;
            if (on)
            {
                m_DrawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
                m_SetPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
                m_Batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
                m_HudFrames = 0;
                m_HudElapsed = 0f;
                m_HudText = "measuring...";
            }
            else
            {
                DisposeRecorders();
            }
        }

        void DisposeRecorders()
        {
            if (m_DrawCalls.Valid) m_DrawCalls.Dispose();
            if (m_SetPassCalls.Valid) m_SetPassCalls.Dispose();
            if (m_Batches.Valid) m_Batches.Dispose();
        }

        void UpdateHud()
        {
            m_HudFrames++;
            m_HudElapsed += Time.unscaledDeltaTime;
            if (m_HudElapsed < HudRefreshSec) return;

            float fps = m_HudFrames / m_HudElapsed;
            m_HudFrames = 0;
            m_HudElapsed = 0f;

            m_Sb.Clear();
            m_Sb.Append("FPS ").Append(fps.ToString("F1")).Append("  ").Append((1000f / Mathf.Max(fps, 0.01f)).ToString("F1")).Append("ms");

            FrameTimingManager.CaptureFrameTimings();
            if (FrameTimingManager.GetLatestTimings(1, m_Timings) > 0 && m_Timings[0].gpuFrameTime > 0)
            {
                m_Sb.Append("  GPU ").Append(m_Timings[0].gpuFrameTime.ToString("F1"))
                    .Append("  CPU ").Append(m_Timings[0].cpuFrameTime.ToString("F1"));
            }

            m_Sb.Append("\nDC ").Append(ReadLast(m_DrawCalls))
                .Append("  SetPass ").Append(ReadLast(m_SetPassCalls))
                .Append("  Batch ").Append(ReadLast(m_Batches));

            m_Sb.Append("\nOff: ");
            if (m_On.Count == 0 && !AnyPieceOn()) m_Sb.Append("-");
            foreach (var item in m_On) m_Sb.Append(item).Append(' ');
            int pieceCount = 0;
            foreach (var state in m_Pieces.Values)
                if (state.On) pieceCount++;
            if (pieceCount > 0) m_Sb.Append("S2-2x").Append(pieceCount).Append(' ');

            m_HudText = m_Sb.ToString();
        }

        static string ReadLast(ProfilerRecorder recorder) => recorder.Valid ? recorder.LastValue.ToString() : "-";

        void OnGUI()
        {
            if (!m_Hud) return;

            if (m_HudStyle == null)
            {
                m_HudStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerLeft, wordWrap = false };
                m_HudStyle.normal.textColor = Color.yellow;
            }
            m_HudStyle.fontSize = Mathf.Max(20, Screen.height / 30);

            var rect = new Rect(10f, 0f, Screen.width / 3f, Screen.height - 10f);
            GUI.Label(rect, m_HudText, m_HudStyle);
        }

        void Report()
        {
            var cam = CameraMgr.S ? CameraMgr.S.mainCamera : Camera.main;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            var data = GetCameraData();

            Debug.Log($"{LogTag}====== 渲染统计 ======");
            Debug.Log($"{LogTag}分辨率 {Screen.width}x{Screen.height} = {Screen.width * (long)Screen.height / 1000000f:F2}M像素  targetFPS={Application.targetFrameRate}  vSync={QualitySettings.vSyncCount}");
            if (urp != null)
                Debug.Log($"{LogTag}URP renderScale={urp.renderScale} MSAA={urp.msaaSampleCount} HDR={urp.supportsHDR} DepthTex={urp.supportsCameraDepthTexture} OpaqueTex={urp.supportsCameraOpaqueTexture}");
            Debug.Log($"{LogTag}主相机后处理={(data != null ? data.renderPostProcessing.ToString() : "无")}");

            int mesh = 0, particle = 0, otherRenderer = 0;
            foreach (var r in FindObjectsOfType<Renderer>())
            {
                if (!r.enabled) continue;
                if (r is MeshRenderer) mesh++;
                else if (r is ParticleSystemRenderer) particle++;
                else otherRenderer++;
            }

            int image = 0, rawImage = 0, text = 0;
            foreach (var g in FindObjectsOfType<Graphic>())
            {
                if (!g.enabled) continue;
                if (g is Image) image++;
                else if (g is RawImage) rawImage++;
                else text++;
            }

            Debug.Log($"{LogTag}Renderer: Mesh={mesh} Particle={particle} 其他={otherRenderer}");
            Debug.Log($"{LogTag}UI: Image={image} RawImage={rawImage} 文字={text} Canvas={FindObjectsOfType<Canvas>().Length} Mask={FindObjectsOfType<Mask>().Length} RectMask2D={FindObjectsOfType<RectMask2D>().Length} Animator={FindObjectsOfType<Animator>().Length}");

            float screenArea = Screen.width * (float)Screen.height;
            var rows = new List<KeyValuePair<float, string>>();
            foreach (var pair in CollectVfxRoots())
            {
                int m = 0, p = 0, o = 0;
                float area = 0f;
                foreach (var r in pair.Value)
                {
                    if (!r.enabled) continue;
                    if (r is MeshRenderer) m++;
                    else if (r is ParticleSystemRenderer) p++;
                    else o++;
                    if (cam != null) area += S4Probe.EstimateScreenArea(cam, r.bounds);
                }
                float layers = area / screenArea;
                rows.Add(new KeyValuePair<float, string>(layers, $"{layers,6:F1}层  {pair.Key}  mesh={m} particle={p} 其他={o}"));
            }
            rows.Sort((a, b) => b.Key.CompareTo(a.Key));

            Debug.Log($"{LogTag}VFX_根节点 {rows.Count} 个(粗估屏幕叠加层数,按从高到低):");
            foreach (var row in rows) Debug.Log($"{LogTag}  {row.Value}");

            var manager = RendererFeatureManager.Instance;
            if (!manager.initialized) return;
            var sb = new StringBuilder();
            foreach (var pair in manager.rendererFeatures)
                sb.Append(pair.Key).Append('=').Append(pair.Value.isActive ? "开" : "关").Append("  ");
            Debug.Log($"{LogTag}RendererFeature: {sb}");
        }
    }
}
