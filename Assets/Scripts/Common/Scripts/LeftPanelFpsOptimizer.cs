using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LetsPiu
{
    /// <summary>
    /// 非侵入式帧率优化插件：挂在 LeftPanel1920Lock 上即可，不改业务脚本。
    /// 支持延迟重优化：地图静态合批、角色蒙皮合并、Canvas 拆出。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1100)]
    public class LeftPanelFpsOptimizer : MonoBehaviour
    {
        const string HostName = "LeftPanelFpsOptimizer_Host";
        const string WorldRootName = "LeftPanelOptimizedWorldRoot";
        const string StatusRootName = "LeftPanelOptimizedStatusRoot";
        const string CombinedName = "CombinedSkinned";

        [Header("开关")]
        [SerializeField] bool enableOptimizer = true;
        [SerializeField] float applyIntervalSec = 2f;
        [Tooltip("场景加载后延迟多久做地图合批/蒙皮合并（可接受卡顿）")]
        [SerializeField] float heavyOptimizeDelaySec = 2.5f;

        [Header("Animator")]
        [SerializeField] bool forceAnimatorCulling = true;

        [Header("阴影 / 灯光（特效友好默认偏保守）")]
        [SerializeField] bool disableCharacterShadows = true;
        [SerializeField] bool disableAllLightShadows = false;
        [SerializeField] bool limitRealtimeLights = false;
        [SerializeField] int maxRealtimeLights = 2;

        [Header("画质兜底")]
        [SerializeField] bool applyQualityHints = false;
        [SerializeField] bool disableRealtimeReflections = false;

        [Header("Canvas 下 3D 角色拆出")]
        [SerializeField] bool detachSkinnedFromCanvas = true;

        [Header("地图合批")]
        [SerializeField] bool combineStaticMaps = true;
        [SerializeField]
        string[] mapRootNameHints =
        {
            "FootBall2_Imported",
            "SongjiangWandaMap",
            "SceneBase"
        };

        [Header("角色蒙皮合并")]
        [Tooltip("运行时 CombineMeshes 易 weight mismatch，默认关；需稳定合批方案后再开")]
        [SerializeField] bool mergePlayerSkinnedMeshes = false;
        [SerializeField] bool stripCharacterProbes = true;

        [Header("粒子")]
        [SerializeField] bool quietIdleParticles = false;

        float _timer = 999f;
        float _sceneLoadedAt = -999f;
        string _mapBatchedScene;
        Transform _worldRoot;
        Transform _statusRoot;
        Canvas _statusCanvas;
        readonly HashSet<int> _detachedPlayers = new HashSet<int>();
        readonly HashSet<int> _mergedPlayers = new HashSet<int>();
        readonly List<StatusFollow> _follows = new List<StatusFollow>(64);
        bool _qualityApplied;
        int _lastLogFrame = -999;

        sealed class StatusFollow : MonoBehaviour
        {
            public Transform source;
            public Vector3 worldOffset;
            Camera _cam;

            void LateUpdate()
            {
                if (source == null)
                {
                    return;
                }

                transform.position = source.TransformPoint(worldOffset);
                if (_cam == null)
                {
                    _cam = Camera.main;
                }

                if (_cam != null)
                {
                    transform.forward = _cam.transform.forward;
                }
            }
        }

        void Awake()
        {
            if (gameObject.name != HostName)
            {
                var host = new GameObject(HostName);
                var moved = host.AddComponent<LeftPanelFpsOptimizer>();
                moved.enableOptimizer = enableOptimizer;
                moved.applyIntervalSec = applyIntervalSec;
                moved.heavyOptimizeDelaySec = heavyOptimizeDelaySec;
                moved.forceAnimatorCulling = forceAnimatorCulling;
                moved.disableCharacterShadows = disableCharacterShadows;
                moved.disableAllLightShadows = disableAllLightShadows;
                moved.limitRealtimeLights = limitRealtimeLights;
                moved.maxRealtimeLights = maxRealtimeLights;
                moved.applyQualityHints = applyQualityHints;
                moved.disableRealtimeReflections = disableRealtimeReflections;
                moved.detachSkinnedFromCanvas = detachSkinnedFromCanvas;
                moved.combineStaticMaps = combineStaticMaps;
                moved.mapRootNameHints = mapRootNameHints;
                moved.mergePlayerSkinnedMeshes = mergePlayerSkinnedMeshes;
                moved.stripCharacterProbes = stripCharacterProbes;
                moved.quietIdleParticles = quietIdleParticles;
                DontDestroyOnLoad(host);
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            _sceneLoadedAt = Time.realtimeSinceStartup;
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _timer = 999f;
            _sceneLoadedAt = Time.realtimeSinceStartup;
            _mapBatchedScene = null;
            _detachedPlayers.Clear();
            _mergedPlayers.Clear();
            _follows.Clear();
            _worldRoot = null;
            _statusRoot = null;
            _statusCanvas = null;
        }

        void LateUpdate()
        {
            if (!enableOptimizer)
            {
                return;
            }

            _timer += Time.unscaledDeltaTime;
            if (_timer < applyIntervalSec)
            {
                return;
            }

            _timer = 0f;
            ApplyOnce();
        }

        void ApplyOnce()
        {
            int animN = 0, shadowN = 0, lightN = 0, detachN = 0, particleN = 0;
            int mapN = 0, mergeN = 0;

            if (applyQualityHints && !_qualityApplied)
            {
                ApplyQualityHints();
                _qualityApplied = true;
            }

            if (forceAnimatorCulling)
            {
                animN = OptimizeAnimators();
            }

            if (disableCharacterShadows)
            {
                shadowN = OptimizeRendererShadows();
            }

            if (disableAllLightShadows || limitRealtimeLights)
            {
                lightN = OptimizeLights();
            }

            if (detachSkinnedFromCanvas)
            {
                detachN = DetachPlayersFromCanvas();
            }

            bool heavyReady = Time.realtimeSinceStartup - _sceneLoadedAt >= heavyOptimizeDelaySec;
            if (heavyReady)
            {
                string sceneName = SceneManager.GetActiveScene().name;
                if (combineStaticMaps && _mapBatchedScene != sceneName)
                {
                    mapN = CombineStaticMaps();
                    _mapBatchedScene = sceneName;
                }

                if (mergePlayerSkinnedMeshes)
                {
                    mergeN = MergePlayerSkinnedMeshes();
                }
            }

            if (quietIdleParticles)
            {
                particleN = QuietParticles();
            }

            if (Time.frameCount - _lastLogFrame > 120)
            {
                _lastLogFrame = Time.frameCount;
                Debug.Log(
                    $"[LeftPanelFpsOptimizer] anim={animN} shadowOff={shadowN} lights={lightN} " +
                    $"detach={detachN} mapBatch={mapN} skinnedMerge={mergeN} " +
                    $"particleQuiet={particleN} detached={_detachedPlayers.Count} merged={_mergedPlayers.Count}");
            }
        }

        void ApplyQualityHints()
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.shadowCascades = 0;
            QualitySettings.pixelLightCount = Mathf.Max(1, maxRealtimeLights);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.softParticles = false;
            QualitySettings.softVegetation = false;
            if (disableRealtimeReflections)
            {
                QualitySettings.realtimeReflectionProbes = false;
            }

            Debug.Log("[LeftPanelFpsOptimizer] quality hints applied");
        }

        int OptimizeAnimators()
        {
            int n = 0;
            Animator[] animators = Object.FindObjectsOfType<Animator>();
            for (int i = 0; i < animators.Length; i++)
            {
                Animator a = animators[i];
                if (a == null)
                {
                    continue;
                }

                if (a.cullingMode != AnimatorCullingMode.CullUpdateTransforms &&
                    a.cullingMode != AnimatorCullingMode.CullCompletely)
                {
                    a.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    n++;
                }

                a.applyRootMotion = false;
            }

            return n;
        }

        int OptimizeRendererShadows()
        {
            int n = 0;
            SkinnedMeshRenderer[] skinned = Object.FindObjectsOfType<SkinnedMeshRenderer>();
            for (int i = 0; i < skinned.Length; i++)
            {
                n += DisableShadows(skinned[i]);
            }

            MeshRenderer[] meshes = Object.FindObjectsOfType<MeshRenderer>();
            for (int i = 0; i < meshes.Length; i++)
            {
                MeshRenderer r = meshes[i];
                if (r == null)
                {
                    continue;
                }

                string rootName = r.transform.root != null ? r.transform.root.name : r.name;
                if (rootName.StartsWith("Player") || r.transform.name.Contains("Player"))
                {
                    n += DisableShadows(r);
                }
            }

            return n;
        }

        static int DisableShadows(Renderer r)
        {
            if (r == null)
            {
                return 0;
            }

            bool changed = false;
            if (r.shadowCastingMode != ShadowCastingMode.Off)
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                changed = true;
            }

            if (r.receiveShadows)
            {
                r.receiveShadows = false;
                changed = true;
            }

            return changed ? 1 : 0;
        }

        int OptimizeLights()
        {
            Light[] lights = Object.FindObjectsOfType<Light>();
            var points = new List<Light>(lights.Length);
            int n = 0;
            Light bestDirectional = null;

            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || !light.enabled)
                {
                    continue;
                }

                if (disableAllLightShadows && light.shadows != LightShadows.None)
                {
                    light.shadows = LightShadows.None;
                    n++;
                }

                if (light.type == LightType.Directional)
                {
                    if (bestDirectional == null || light.intensity > bestDirectional.intensity)
                    {
                        bestDirectional = light;
                    }
                }
                else if (light.type == LightType.Point || light.type == LightType.Spot)
                {
                    points.Add(light);
                }
            }

            if (!limitRealtimeLights)
            {
                return n;
            }

            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || light.type != LightType.Directional || !light.enabled)
                {
                    continue;
                }

                if (bestDirectional != null && light != bestDirectional)
                {
                    light.enabled = false;
                    n++;
                }
            }

            points.Sort((a, b) => b.intensity.CompareTo(a.intensity));
            int keepPoints = Mathf.Max(0, maxRealtimeLights - (bestDirectional != null ? 1 : 0));
            for (int i = keepPoints; i < points.Count; i++)
            {
                if (points[i].enabled)
                {
                    points[i].enabled = false;
                    n++;
                }
            }

            return n;
        }

        int CombineStaticMaps()
        {
            int n = 0;
            for (int i = 0; i < mapRootNameHints.Length; i++)
            {
                string hint = mapRootNameHints[i];
                if (string.IsNullOrEmpty(hint))
                {
                    continue;
                }

                GameObject root = GameObject.Find(hint);
                if (root == null)
                {
                    // 深搜一次
                    Transform[] all = Object.FindObjectsOfType<Transform>();
                    for (int t = 0; t < all.Length; t++)
                    {
                        if (all[t] != null && all[t].name == hint)
                        {
                            root = all[t].gameObject;
                            break;
                        }
                    }
                }

                if (root == null)
                {
                    continue;
                }

                MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(false);
                if (renderers.Length < 2)
                {
                    continue;
                }

                // 合批前打开 GPU Instancing（对未合进同一 batch 的重复材质仍有帮助）
                for (int r = 0; r < renderers.Length; r++)
                {
                    MeshRenderer mr = renderers[r];
                    if (mr == null)
                    {
                        continue;
                    }

                    Material[] mats = mr.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m] != null && !mats[m].enableInstancing)
                        {
                            mats[m].enableInstancing = true;
                        }
                    }

                    if (stripCharacterProbes)
                    {
                        mr.lightProbeUsage = LightProbeUsage.Off;
                        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
                        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    }
                }

                try
                {
                    StaticBatchingUtility.Combine(root);
                    n += renderers.Length;
                    Debug.Log($"[LeftPanelFpsOptimizer] StaticBatchingUtility.Combine({hint}) renderers={renderers.Length}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[LeftPanelFpsOptimizer] map batch fail {hint}: {e.Message}");
                }
            }

            return n;
        }

        int MergePlayerSkinnedMeshes()
        {
            int n = 0;
            // 先拆 Canvas，再合并，避免合完又在 Canvas 下
            if (detachSkinnedFromCanvas)
            {
                DetachPlayersFromCanvas();
            }

            SkinnedMeshRenderer[] skinned = Object.FindObjectsOfType<SkinnedMeshRenderer>();
            var players = new HashSet<Transform>();
            for (int i = 0; i < skinned.Length; i++)
            {
                if (skinned[i] == null)
                {
                    continue;
                }

                Transform player = FindPlayerRoot(skinned[i].transform);
                if (player != null)
                {
                    players.Add(player);
                }
            }

            foreach (Transform player in players)
            {
                if (player == null)
                {
                    continue;
                }

                int id = player.GetInstanceID();
                if (_mergedPlayers.Contains(id))
                {
                    continue;
                }

                if (player.Find(CombinedName) != null)
                {
                    _mergedPlayers.Add(id);
                    continue;
                }

                // 无论成功/失败都标记，避免 weight mismatch 时每 2s 全员重试拖垮帧率
                bool ok = TryMergeSkinnedUnder(player);
                _mergedPlayers.Add(id);
                if (ok)
                {
                    n++;
                }
            }

            return n;
        }

        bool TryMergeSkinnedUnder(Transform player)
        {
            SkinnedMeshRenderer[] smrs = player.GetComponentsInChildren<SkinnedMeshRenderer>(false);
            var sources = new List<SkinnedMeshRenderer>(smrs.Length);
            for (int i = 0; i < smrs.Length; i++)
            {
                SkinnedMeshRenderer smr = smrs[i];
                if (smr == null || !smr.enabled || smr.sharedMesh == null)
                {
                    continue;
                }

                if (smr.gameObject.name == CombinedName)
                {
                    return true;
                }

                sources.Add(smr);
            }

            if (sources.Count <= 1)
            {
                // 单件也做探针关闭
                if (sources.Count == 1 && stripCharacterProbes)
                {
                    StripProbes(sources[0]);
                }

                return sources.Count == 1;
            }

            try
            {
                var boneSet = new List<Transform>(64);
                var boneIndex = new Dictionary<Transform, int>(64);
                var bindposes = new List<Matrix4x4>(64);
                var combine = new List<CombineInstance>(sources.Count);
                var boneWeights = new List<BoneWeight>(4096);
                var materials = new List<Material>(8);
                int vertexBase = 0;

                // 只合并「单 submesh + 单材质」部件，避免 CombineMeshes 顶点数与 boneWeight 错位
                var usedSources = new List<SkinnedMeshRenderer>(sources.Count);
                for (int s = 0; s < sources.Count; s++)
                {
                    SkinnedMeshRenderer smr = sources[s];
                    Mesh mesh = smr.sharedMesh;
                    Transform[] bones = smr.bones;
                    Matrix4x4[] meshBind = mesh.bindposes;
                    BoneWeight[] weights = mesh.boneWeights;
                    Material[] mats = smr.sharedMaterials;

                    if (mesh.subMeshCount != 1 || mats == null || mats.Length < 1)
                    {
                        continue;
                    }

                    if (bones == null || bones.Length == 0 || weights == null || weights.Length != mesh.vertexCount)
                    {
                        continue;
                    }

                    int[] remap = new int[bones.Length];
                    for (int b = 0; b < bones.Length; b++)
                    {
                        Transform bone = bones[b];
                        if (bone == null)
                        {
                            remap[b] = 0;
                            continue;
                        }

                        if (!boneIndex.TryGetValue(bone, out int idx))
                        {
                            idx = boneSet.Count;
                            boneIndex[bone] = idx;
                            boneSet.Add(bone);
                            Matrix4x4 bp = (meshBind != null && b < meshBind.Length)
                                ? meshBind[b]
                                : bone.worldToLocalMatrix * smr.transform.localToWorldMatrix;
                            bindposes.Add(bp);
                        }

                        remap[b] = idx;
                    }

                    for (int w = 0; w < weights.Length; w++)
                    {
                        BoneWeight bw = weights[w];
                        bw.boneIndex0 = remap[Mathf.Clamp(bw.boneIndex0, 0, remap.Length - 1)];
                        bw.boneIndex1 = remap[Mathf.Clamp(bw.boneIndex1, 0, remap.Length - 1)];
                        bw.boneIndex2 = remap[Mathf.Clamp(bw.boneIndex2, 0, remap.Length - 1)];
                        bw.boneIndex3 = remap[Mathf.Clamp(bw.boneIndex3, 0, remap.Length - 1)];
                        boneWeights.Add(bw);
                    }

                    combine.Add(new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = 0,
                        transform = Matrix4x4.identity
                    });
                    materials.Add(mats[0]);
                    usedSources.Add(smr);
                    vertexBase += mesh.vertexCount;
                }

                if (usedSources.Count <= 1 || combine.Count == 0 || boneSet.Count == 0)
                {
                    // 可合部件不足：关掉探针后标记完成，避免每帧重试
                    if (stripCharacterProbes)
                    {
                        for (int i = 0; i < sources.Count; i++)
                        {
                            StripProbes(sources[i]);
                        }
                    }

                    return true;
                }

                var combinedMesh = new Mesh { name = player.name + "_CombinedSkinned" };
                combinedMesh.indexFormat = vertexBase > 65535
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16;
                combinedMesh.CombineMeshes(combine.ToArray(), false, false);
                if (combinedMesh.vertexCount != boneWeights.Count)
                {
                    // CombineMeshes 常会改顶点拓扑，运行时蒙皮合并不稳；放弃并只做轻量优化
                    Object.Destroy(combinedMesh);
                    if (stripCharacterProbes)
                    {
                        for (int i = 0; i < usedSources.Count; i++)
                        {
                            StripProbes(usedSources[i]);
                            usedSources[i].updateWhenOffscreen = false;
                            usedSources[i].skinnedMotionVectors = false;
                        }
                    }

                    Debug.LogWarning(
                        $"[LeftPanelFpsOptimizer] skinned merge abort {player.name}: " +
                        $"weight mismatch ({boneWeights.Count}->{combinedMesh.vertexCount}), light-opt only");
                    return false;
                }

                combinedMesh.boneWeights = boneWeights.ToArray();
                combinedMesh.bindposes = bindposes.ToArray();
                combinedMesh.RecalculateBounds();

                var go = new GameObject(CombinedName);
                go.transform.SetParent(player, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                var combined = go.AddComponent<SkinnedMeshRenderer>();
                combined.sharedMesh = combinedMesh;
                combined.bones = boneSet.ToArray();
                combined.rootBone = usedSources[0].rootBone != null ? usedSources[0].rootBone : boneSet[0];
                combined.sharedMaterials = materials.ToArray();
                combined.updateWhenOffscreen = false;
                combined.skinnedMotionVectors = false;
                if (disableCharacterShadows)
                {
                    combined.shadowCastingMode = ShadowCastingMode.Off;
                    combined.receiveShadows = false;
                }

                if (stripCharacterProbes)
                {
                    StripProbes(combined);
                }

                for (int i = 0; i < usedSources.Count; i++)
                {
                    usedSources[i].enabled = false;
                }

                Debug.Log(
                    $"[LeftPanelFpsOptimizer] merged skinned {player.name}: {usedSources.Count}/{sources.Count} -> 1, " +
                    $"bones={boneSet.Count}, mats={materials.Count}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LeftPanelFpsOptimizer] skinned merge fail {player.name}: {e.Message}");
                return false;
            }
        }

        static void StripProbes(Renderer r)
        {
            if (r == null)
            {
                return;
            }

            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        int DetachPlayersFromCanvas()
        {
            EnsureWorldRoots();
            int n = 0;
            SkinnedMeshRenderer[] skinned = Object.FindObjectsOfType<SkinnedMeshRenderer>();
            for (int i = 0; i < skinned.Length; i++)
            {
                SkinnedMeshRenderer smr = skinned[i];
                if (smr == null)
                {
                    continue;
                }

                Transform player = FindPlayerRoot(smr.transform);
                if (player == null || !IsUnderCanvas(player))
                {
                    continue;
                }

                int id = player.GetInstanceID();
                if (_detachedPlayers.Contains(id))
                {
                    continue;
                }

                Transform status = player.Find("Status");
                if (status != null && _statusRoot != null)
                {
                    Vector3 worldPos = status.position;
                    Vector3 localOff = player.InverseTransformPoint(worldPos);
                    status.SetParent(_statusRoot, true);
                    var follow = status.gameObject.GetComponent<StatusFollow>();
                    if (follow == null)
                    {
                        follow = status.gameObject.AddComponent<StatusFollow>();
                    }

                    follow.source = player;
                    follow.worldOffset = localOff;
                    _follows.Add(follow);
                }

                player.SetParent(_worldRoot, true);
                _detachedPlayers.Add(id);
                n++;
            }

            return n;
        }

        void EnsureWorldRoots()
        {
            if (_worldRoot == null)
            {
                var go = GameObject.Find(WorldRootName);
                if (go == null)
                {
                    go = new GameObject(WorldRootName);
                }

                _worldRoot = go.transform;
                DontDestroyOnLoad(go);
            }

            if (_statusRoot != null && _statusCanvas != null)
            {
                return;
            }

            Canvas worldCanvas = null;
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas c = canvases[i];
                if (c != null && c.gameObject.name.ToLower().Contains("world"))
                {
                    worldCanvas = c;
                    break;
                }
            }

            if (worldCanvas == null && canvases.Length > 0)
            {
                worldCanvas = canvases[0];
            }

            _statusCanvas = worldCanvas;
            if (_statusCanvas == null)
            {
                return;
            }

            Transform existing = _statusCanvas.transform.Find(StatusRootName);
            if (existing != null)
            {
                _statusRoot = existing;
                return;
            }

            var statusGo = new GameObject(StatusRootName, typeof(RectTransform));
            _statusRoot = statusGo.transform;
            _statusRoot.SetParent(_statusCanvas.transform, false);
            var rt = statusGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Transform FindPlayerRoot(Transform t)
        {
            Transform cur = t;
            Transform best = null;
            while (cur != null)
            {
                string n = cur.name;
                if (n.StartsWith("Player_") || n == "Player")
                {
                    return cur;
                }

                if (n.StartsWith("Players"))
                {
                    best = t;
                    while (best.parent != null && best.parent != cur)
                    {
                        best = best.parent;
                    }

                    return best;
                }

                if (cur.parent != null && cur.parent.GetComponent<Canvas>() != null)
                {
                    best = cur;
                }

                cur = cur.parent;
            }

            return best;
        }

        static bool IsUnderCanvas(Transform t)
        {
            while (t != null)
            {
                if (t.GetComponent<Canvas>() != null)
                {
                    return true;
                }

                t = t.parent;
            }

            return false;
        }

        int QuietParticles()
        {
            int n = 0;
            ParticleSystem[] systems = Object.FindObjectsOfType<ParticleSystem>();
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                if (ps == null || !ps.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!ps.isPlaying && ps.particleCount == 0)
                {
                    var em = ps.emission;
                    if (em.enabled)
                    {
                        em.enabled = false;
                        n++;
                    }
                }
            }

            return n;
        }
    }
}
