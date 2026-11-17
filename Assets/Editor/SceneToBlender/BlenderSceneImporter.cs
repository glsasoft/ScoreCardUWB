#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BaChang.Tools
{
    /// <summary>
    /// 把 Blender 导出的「每物体一个 FBX + 只记录 transform 的清单」在 Unity 里重建场景。
    ///
    /// 输入（Blender 插件的产物）：
    ///   &lt;项目根&gt;/SceneExport_Unity/scene_transform.json
    ///   &lt;项目根&gt;/SceneExport_Unity/fbx/*.fbx
    ///
    /// 输出（本工具）：
    ///   &lt;输出根&gt;/&lt;场景名&gt;/&lt;场景名&gt;.unity
    ///   &lt;输出根&gt;/&lt;场景名&gt;/FBX/*.fbx
    ///
    /// 坐标管线：
    ///   F    = 交换 y/z，把 Blender 的 (x,y,z) 映射到 Unity 的 (x,z,y)，det = -1。
    ///   S    = 只翻 X，det = -1。FBX 从右手系导到左手系时顶点 X 已被取反。
    ///   R90X = 抵消 FBX 模型根自带的 270°X 校正。
    ///       P = F · B · S · R90X        （B = 物体的 Blender 世界矩阵）  det(P) = +1
    ///      世界顶点 = P · C · g         （C = FBX 根自带的 270°X + 100 倍单位）
    ///   旧的 F·B·F 漏了 X 翻转和 R90X，表现是整体绕 Z 差 180°、位置对不上；
    ///   更早的 F·B·R90X 则 det = -1，物体被镜像。
    /// </summary>
    public class BlenderSceneImporter : EditorWindow
    {
        /// 批处理/脚本调用时关掉模态对话框
        public static bool quiet = false;

        // ============================================================ 配置存储

        const string PrefOutRoot = "BaChang.S2U.OutRoot";
        const string PrefSceneName = "BaChang.S2U.SceneName";
        const string PrefNormalDx5 = "BaChang.S2U.NormalDx5";
        const string PrefOnlyUpdateSelected = "BaChang.S2U.OnlyUpdateSelected";
        const string PrefBeginnerMode = "BaChang.S2U.BeginnerMode";

        const string DefaultSrcFolder = "SceneExport_Unity";
        const string PointerFile = "SceneToUnity.path";
        const string DefaultOutRoot = "Assets/SceneFromBlender";

        /// 场景 PBR 着色器的真名。Blender 导出的材质全靠它的 _NRMTex / _SMO / _NormalMode 才绑得准。
        /// 文件在 Assets/Art/Shaders/Scene/GLSA_ScenePBR_DXT5nm.shader。
        const string PbrShaderName = "GLSA/Scene/PBR_EnvStandard";

        static string OutRoot
        {
            get
            {
                string v = EditorPrefs.GetString(PrefOutRoot, DefaultOutRoot);
                return string.IsNullOrEmpty(v) ? DefaultOutRoot : v;
            }
            set
            {
                EditorPrefs.SetString(PrefOutRoot, value ?? "");
            }
        }

        static string SceneName
        {
            get { return EditorPrefs.GetString(PrefSceneName, ""); }
            set { EditorPrefs.SetString(PrefSceneName, value ?? ""); }
        }

        /// 法线贴图的 meta 走哪一套：
        ///   false（默认）= 普通法线图，TextureImporterType.NormalMap，Unity 自己打包 + 解包；
        ///   true          = DXT5nm（dx5），Type 保持 Default，PC 平台压 DXT5，shader 自己解 G/A。
        /// 这个开关同时决定材质上的 _NormalMode，两边必须成对，否则解出来的法线是歪的。
        static bool NormalDx5
        {
            get { return EditorPrefs.GetBool(PrefNormalDx5, false); }
            set { EditorPrefs.SetBool(PrefNormalDx5, value); }
        }

        /// 增量更新时是否只处理场景里选中的物体
        static bool OnlyUpdateSelected
        {
            get { return EditorPrefs.GetBool(PrefOnlyUpdateSelected, false); }
            set { EditorPrefs.SetBool(PrefOnlyUpdateSelected, value); }
        }

        /// 菜鸡模式：勾上显示各段说明文字，不勾只剩控件和按钮
        static bool BeginnerMode
        {
            get { return EditorPrefs.GetBool(PrefBeginnerMode, true); }
            set { EditorPrefs.SetBool(PrefBeginnerMode, value); }
        }

        static string ProjectRoot
        {
            get { return Directory.GetParent(Application.dataPath).FullName; }
        }

        // ============================================================ 路径

        /// Blender 导出目录：优先读项目根的 SceneToUnity.path，没有就默认
        static string ResolveSrcRoot()
        {
            string root = ProjectRoot;
            string ptr = Path.Combine(root, PointerFile);
            if (File.Exists(ptr))
            {
                try
                {
                    string p = File.ReadAllText(ptr).Trim().Trim('"');
                    if (!string.IsNullOrEmpty(p) && Directory.Exists(p))
                        return p;
                }
                catch { }
            }
            return Path.Combine(root, DefaultSrcFolder);
        }

        /// 把 "Assets/A/B/C" 逐级建出来
        static void EnsureFolder(string assetFolder)
        {
            if (string.IsNullOrEmpty(assetFolder)) return;
            string[] parts = assetFolder.Replace('\\', '/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets") return;

            string cur = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder();
            char[] bad = Path.GetInvalidFileNameChars();
            foreach (char c in s.Trim())
                sb.Append(Array.IndexOf(bad, c) >= 0 || c == '/' || c == '\\' ? '_' : c);
            return sb.ToString().Trim();
        }

        static string ManifestToString(Manifest m)
        {
            if (m == null) return "清单读不出来";
            return string.Format(CultureInfo.InvariantCulture,
                "来源 {0} / 场景 {1} / 范围 {2} / 实例 {3} / FBX {4}",
                m.source, m.sourceScene, m.scope, m.instanceCount, m.fbxCount);
        }

        // ============================================================ 数据

        [Serializable]
        public class Manifest
        {
            public string source;
            public string sourceScene;
            public string scope;
            public string outDir;
            public string note;
            public int instanceCount;
            public int fbxCount;
            public FbxEntry[] fbx;
            public InstanceEntry[] instances;
            public MaterialEntry[] materials;
        }

        [Serializable]
        public class MaterialEntry
        {
            public string name;
            public string alpha;
            public string baseTex;
            public string normalTex;
            public string rmoTex;
            public int normalMode;   // 0 = RGB, 1 = 原始 DXT5nm
            public bool hasNormal;
            /// 纯色材质的基础色（线性空间，Blender 端给的 Principled Base Color）。
            /// 只有基础色**没接贴图**时才有值；接了贴图的材质这里是 null，
            /// 免得把 Blender 里那个无意义的默认 0.8 灰色乘到贴图上。
            public float[] baseColor;
        }

        [Serializable]
        public class FbxEntry
        {
            public string data;
            public string fbx;
            public int verts;
        }

        [Serializable]
        public class InstanceEntry
        {
            public string name;
            public string fbx;
            public string data;
            public string group;
            public string parent;
            public float[] bPos;
            public float[] bRot;
            public float[] bScale;
            public string[] materials;
        }

        // ============================================================ 导入

        /// 真正干活：把 srcRoot 的内容重建到 outAssetDir/<sceneName>。
        /// normalDx5 = false 走普通法线图（NormalMap 类型），true 按 DXT5nm 原始通道处理。
        public static void ImportInto(string srcRoot, string outAssetRoot, string sceneName,
                                      bool normalDx5 = false)
        {
            string manifestPath = Path.Combine(srcRoot, "scene_transform.json");
            string srcFbx = Path.Combine(srcRoot, "fbx");

            if (!File.Exists(manifestPath))
            {
                EditorUtility.DisplayDialog("找不到清单",
                    "没有找到:\n" + manifestPath +
                    "\n\n先在 Blender 里导出一次。", "好");
                return;
            }
            if (!Directory.Exists(srcFbx))
            {
                EditorUtility.DisplayDialog("找不到 FBX", "没有找到:\n" + srcFbx, "好");
                return;
            }

            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));
            if (manifest == null || manifest.instances == null || manifest.instances.Length == 0)
            {
                EditorUtility.DisplayDialog("清单为空", "scene_transform.json 里没有 instances。", "好");
                return;
            }

            string name = Sanitize(sceneName);
            if (string.IsNullOrEmpty(name))
                name = Sanitize(manifest.sourceScene);
            if (string.IsNullOrEmpty(name))
                name = "BlenderScene";

            string outAssetDir = (outAssetRoot.TrimEnd('/', '\\') + "/" + name).Replace('\\', '/');
            string fbxAssetDir = outAssetDir + "/FBX";
            string sceneAssetPath = outAssetDir + "/" + name + ".unity";

            // 已存在就只提示，不拦
            if (AssetDatabase.IsValidFolder(outAssetDir))
                Debug.Log("[BlenderSceneImporter] 目标目录已存在，将覆盖同名文件: " + outAssetDir);

            // ---- 1. 建目录 + 拷 FBX
            EnsureFolder(fbxAssetDir);

            string fbxAbsDir = Path.Combine(ProjectRoot, fbxAssetDir.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(fbxAbsDir);

            int copied = 0;
            foreach (var f in Directory.GetFiles(srcFbx, "*.fbx"))
            {
                string dst = Path.Combine(fbxAbsDir, Path.GetFileName(f));
                bool need = !File.Exists(dst) ||
                            new FileInfo(f).LastWriteTimeUtc > new FileInfo(dst).LastWriteTimeUtc;
                if (need)
                {
                    File.Copy(f, dst, true);
                    copied++;
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);

            // ---- 1.5 材质 + 贴图
            int matBuilt = 0;
            var matMap = BuildMaterials(manifest, outAssetDir, srcRoot, normalDx5, ref matBuilt);

            // ---- 1.6 FBX 导入设置 + 材质外部映射
            // 必须排在 BuildMaterials 之后：要 remap 到刚建出来的 .mat，素材得先存在。
            // 设完这一步，以后把 FBX 直接拖进场景也自带材质，不用手动挂。
            ApplyModelImportSettings(fbxAssetDir, outAssetDir + "/Materials", matMap);

            // ---- 2. 新建场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- 3. 摆
            // 摆放矩阵统一走 BuildPlacementMatrix：P = F·B·S·R90X，det(P) = +1。
            // F = 交换 y/z（Blender Z-up <-> Unity Y-up），det(F) = -1。
            // S = 只翻 X，det(S) = -1。FBX 从右手系导到左手系，顶点 X 已被取反。
            // R90X = 抵消 FBX 模型根自带的 270°X（C 的逆）。

            var groups = new Dictionary<string, Transform>();
            var prefabCache = new Dictionary<string, GameObject>();
            int placed = 0, failed = 0;
            var missing = new List<string>();

            // 统一父级：所有实例收进一个根，场景面板不会炸成一堆并列节点
            var rootGO = new GameObject(sceneName + "_Imported");
            var rootT = rootGO.transform;
            int groupCount = 0;

            // 收集整体包围盒，用来摆相机
            Bounds sceneBounds = new Bounds();
            bool hasBounds = false;

            try
            {
                for (int i = 0; i < manifest.instances.Length; i++)
                {
                    var e = manifest.instances[i];
                    if (i % 25 == 0)
                        EditorUtility.DisplayProgressBar("从 Blender 导入",
                            string.Format(CultureInfo.InvariantCulture, "{0}/{1}  {2}", i, manifest.instances.Length, e.name),
                            0.1f + 0.85f * i / manifest.instances.Length);

                    GameObject inst;
                    if (!TrySpawnInstance(e, rootT, groups, ref groupCount,
                                          matMap, fbxAssetDir, prefabCache, out inst))
                    {
                        failed++;
                        if (missing.Count < 12) missing.Add(e.fbx);
                        continue;
                    }

                    // 累计整体包围盒，给相机取景用
                    var rs = inst.GetComponentsInChildren<MeshRenderer>(true);
                    for (int ri = 0; ri < rs.Length; ri++)
                    {
                        if (!hasBounds) { sceneBounds = rs[ri].bounds; hasBounds = true; }
                        else sceneBounds.Encapsulate(rs[ri].bounds);
                    }

                    placed++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // ---- 3.5 默认相机 + 平行光
            Vector3 lookAt = hasBounds ? sceneBounds.center : Vector3.zero;
            float radius = hasBounds ? Mathf.Max(sceneBounds.extents.magnitude, 1f) : 10f;

            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = Mathf.Max(radius * 20f, 1000f);
            Vector3 camDir = new Vector3(0.55f, 0.8f, -1f).normalized;
            camGO.transform.position = lookAt + camDir * (radius * 1.9f);
            camGO.transform.LookAt(lookAt);
            if (UnityEngine.Object.FindObjectOfType<AudioListener>() == null)
                camGO.AddComponent<AudioListener>();

            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            lightGO.transform.position = lookAt + Vector3.up * radius;

            // ---- 4. 保存
            EditorSceneManager.SaveScene(scene, sceneAssetPath);
            AssetDatabase.Refresh();

            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[BlenderSceneImporter] {0} 个实例 / 摆放 {1} / 失败 {2} / 子分组 {3} / 新拷 FBX {4}\n  根: {5}\n  scene: {6}\n  fbx  : {7}",
                manifest.instances.Length, placed, failed, groupCount, copied, rootT.name, sceneAssetPath, fbxAssetDir));

            if (missing.Count > 0)
                Debug.LogWarning("[BlenderSceneImporter] 缺 FBX: " + string.Join(", ", missing.ToArray()));

            if (!quiet) EditorUtility.DisplayDialog("导入完成",
                "实例 " + placed + " / " + manifest.instances.Length +
                "\n失败 " + failed + "   子分组 " + groupCount + "   新拷 FBX " + copied +
                "\n\n根节点:\n" + rootT.name +
                "\n\n场景:\n" + sceneAssetPath +
                "\n\nFBX:\n" + fbxAssetDir,
                "好");

            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sceneAssetPath);
            if (obj != null) EditorGUIUtility.PingObject(obj);
        }

        // ============================================================ 工具

        static Vector3 ToVec3(float[] a, Vector3 fallback = default(Vector3))
        {
            if (a == null || a.Length < 3) return fallback;
            return new Vector3(a[0], a[1], a[2]);
        }

        /// Blender 四元数是 (w,x,y,z)，Unity 是 (x,y,z,w)。
        /// Blender 写出来的浮点数长度常有 1e-4 级的偏差，Unity 会判定为无效四元数并回退成单位旋转，
        /// 所以这里必须显式归一化。
        static Quaternion ToQuat(float[] a)
        {
            if (a == null || a.Length < 4) return Quaternion.identity;
            float x = a[1], y = a[2], z = a[3], w = a[0];
            float len = Mathf.Sqrt(x * x + y * y + z * z + w * w);
            if (len < 1e-6f) return Quaternion.identity;
            return new Quaternion(x / len, y / len, z / len, w / len);
        }

        /// 任意矩阵分解成 TRS 写进 Transform（处理旋转与负缩放）
        static void ApplyMatrix(Transform t, Matrix4x4 m)
        {
            Vector3 p = new Vector3(m.m03, m.m13, m.m23);
            Vector3 c0 = new Vector3(m.m00, m.m10, m.m20);
            Vector3 c1 = new Vector3(m.m01, m.m11, m.m21);
            Vector3 c2 = new Vector3(m.m02, m.m12, m.m22);

            float sx = c0.magnitude, sy = c1.magnitude, sz = c2.magnitude;
            bool mirrored = m.determinant < 0f;
            if (mirrored) sx = -sx;
            float sign = mirrored ? -1f : 1f;

            Matrix4x4 rot = Matrix4x4.identity;
            Vector3 n0 = c0.normalized * sign;
            Vector3 n1 = c1.normalized;
            Vector3 n2 = c2.normalized;
            rot.SetColumn(0, new Vector4(n0.x, n0.y, n0.z, 0f));
            rot.SetColumn(1, new Vector4(n1.x, n1.y, n1.z, 0f));
            rot.SetColumn(2, new Vector4(n2.x, n2.y, n2.z, 0f));

            t.localPosition = p;
            t.localRotation = rot.rotation;
            t.localScale = new Vector3(sx, sy, sz);
        }

        /// <summary>
        /// 实例化一个 FBX prefab 并按清单摆好：挂分组 → 套 P·C → 回填材质。
        /// ImportInto 和 ImportAddedObjects 共用，保证两条路的摆放逐位一致。
        /// 返回 false = FBX 资产没加载到（没拷进来，或者 Unity 还没导入完）。
        /// </summary>
        static bool TrySpawnInstance(InstanceEntry e, Transform rootT,
                                     Dictionary<string, Transform> groups, ref int groupCount,
                                     Dictionary<string, Material> matMap, string fbxAssetDir,
                                     Dictionary<string, GameObject> prefabCache,
                                     out GameObject inst)
        {
            inst = null;
            if (e == null || string.IsNullOrEmpty(e.fbx)) return false;

            GameObject prefab;
            if (!prefabCache.TryGetValue(e.fbx, out prefab))
            {
                string assetPath = fbxAssetDir + "/" +
                    Path.GetFileNameWithoutExtension(e.fbx) + ".fbx";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                prefabCache[e.fbx] = prefab;
            }
            if (prefab == null) return false;

            inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (inst == null) return false;
            var child = inst.transform;

            // 只有真正带分组（且分组名不等于实例名）时才多插一层
            Transform parentT = rootT;
            string gname = string.IsNullOrEmpty(e.group) ? null : e.group;
            if (gname != null && gname != e.name)
            {
                Transform groupT;
                if (!groups.TryGetValue(gname, out groupT) || groupT == null)
                {
                    // 往已经摆过一次的场景里加东西时，分组节点得复用现成的，
                    // 否则每点一次按钮就多一层同名空节点。
                    groupT = FindGroupNode(rootT, gname);
                    if (groupT == null)
                    {
                        var g = new GameObject(gname);
                        groupT = g.transform;
                        groupT.SetParent(rootT, false);
                        groupCount++;
                    }
                    groups[gname] = groupT;
                }
                parentT = groupT;
            }

            Matrix4x4 B = Matrix4x4.TRS(ToVec3(e.bPos), ToQuat(e.bRot), ToVec3(e.bScale, Vector3.one));
            Matrix4x4 P = BuildPlacementMatrix(B);

            // 不建 holder 空节点，把摆放矩阵直接并进 FBX 实例根（层级少一层）。
            // FBX 根自带的 270°X + 100 倍轴校仍是它的 local 的一部分，靠左乘 P 覆盖。
            child.SetParent(parentT, false);
            Matrix4x4 C = Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale);
            ApplyMatrix(child, P * C);

            ApplyInstanceMaterials(inst, e, matMap);
            return true;
        }

        /// <summary>
        /// 材质回填：按 slot 顺序对齐 manifest 里的材质名。
        /// FBX 已经 remap 到外部 .mat 时这里基本什么都不做，是给 remap 漏掉的情况兜底。
        /// </summary>
        static void ApplyInstanceMaterials(GameObject inst, InstanceEntry e,
                                           Dictionary<string, Material> matMap)
        {
            if (inst == null || e == null || e.materials == null) return;
            if (matMap == null || matMap.Count == 0) return;

            var rends = inst.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var rr in rends)
            {
                var slots = rr.sharedMaterials;
                bool changed = false;
                for (int si = 0; si < slots.Length && si < e.materials.Length; si++)
                {
                    string mname = e.materials[si];
                    if (string.IsNullOrEmpty(mname)) continue;
                    Material got;
                    if (matMap.TryGetValue(mname, out got) && got != null && slots[si] != got)
                    {
                        slots[si] = got;
                        changed = true;
                    }
                }
                if (changed) rr.sharedMaterials = slots;
            }
        }

        /// <summary>
        /// 在已有根节点下按名字找分组节点。
        /// 必须排除 mesh 实例本身：manifest 的 group 就是顶级物体名，分组名和物体名撞车时
        /// 认错了会把新物体挂到某个 mesh 底下，跟着人家的变换一起动。
        /// </summary>
        static Transform FindGroupNode(Transform rootT, string gname)
        {
            if (rootT == null || string.IsNullOrEmpty(gname)) return null;
            for (int i = 0; i < rootT.childCount; i++)
            {
                var c = rootT.GetChild(i);
                if (c.name != gname) continue;
                if (c.GetComponent<MeshFilter>() == null && c.GetComponent<MeshRenderer>() == null)
                    return c;
            }
            return null;
        }

        /// 一张贴图想要什么 meta。同一张图可能既被当基础色又被当法线，得合并需求。
        struct TexWant
        {
            public bool srgb;
            public bool isNormal;
        }

        /// 基础色贴图开 sRGB；法线 / SMO 是数据图，必须关掉 sRGB（线性采样）。
        ///
        /// 法线图有两条路，由 normalDx5 切：
        ///   普通法线图（不勾）：textureType = NormalMap。Unity 按标准流程把源色重排成
        ///       DXT5nm / BC5，采样时再解包；shader 侧对应 _NormalMode = 2。
        ///   原始 DXT5nm（勾上）：textureType 保持 Default，只把 PC 平台压成 DXT5 保住 alpha
        ///       里的 X 通道，shader 自己解 GA；对应 _NormalMode = 1。
        ///       这种图本来就是 DXT5nm 的通道布局，再走 NormalMap 会被 Unity 二次打包，
        ///       跟原始通道打架，还会弹「未标记为法线图」的提示。
        ///
        /// meta 类型和 shader 的 _NormalMode 必须成对，只对一边解出来的法线都是歪的。
        static int ApplyTextureImportSettings(Manifest manifest, string texAssetDir, bool normalDx5,
                                              out int srgbFixed, out int normalFixed)
        {
            var wanted = new Dictionary<string, TexWant>(StringComparer.OrdinalIgnoreCase);

            foreach (var me in manifest.materials)
            {
                MarkTex(wanted, texAssetDir, me.baseTex, true, false);
                MarkTex(wanted, texAssetDir, me.normalTex, false, true);
                MarkTex(wanted, texAssetDir, me.rmoTex, false, false);
            }

            srgbFixed = 0;
            normalFixed = 0;
            int changed = 0;

            // 批量编辑里不能调 SaveAndReimport —— 它在 StartAssetEditing 期间是延迟的，
            // 一批几百张图连着调很容易只生效一部分，法线图还按默认 sRGB 进来。
            // 正确顺序：批内只改 importer 设置 → 出批 → 再逐个 ImportAsset。
            var pending = new List<string>();

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var kv in wanted)
                {
                    var importer = AssetImporter.GetAtPath(kv.Key) as TextureImporter;
                    if (importer == null) continue;

                    bool dirty = false;

                    // ---- 颜色空间
                    if (importer.sRGBTexture != kv.Value.srgb)
                    {
                        importer.sRGBTexture = kv.Value.srgb;
                        dirty = true;
                        if (!kv.Value.srgb) srgbFixed++;
                    }

                    // ---- 法线图的类型 + 压缩格式（被当基础色用的图不改类型）
                    if (kv.Value.isNormal && !kv.Value.srgb)
                    {
                        var wantType = normalDx5 ? TextureImporterType.Default : TextureImporterType.NormalMap;
                        if (importer.textureType != wantType)
                        {
                            importer.textureType = wantType;
                            dirty = true;
                        }

                        if (SetDxt5Override(importer, normalDx5)) dirty = true;
                        normalFixed++;
                    }

                    if (!dirty) continue;
                    EditorUtility.SetDirty(importer);
                    pending.Add(kv.Key);
                    changed++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            for (int i = 0; i < pending.Count; i++)
            {
                if (i % 20 == 0 && pending.Count > 20)
                    EditorUtility.DisplayProgressBar("贴图导入设置", pending[i], (float)i / pending.Count);
                AssetDatabase.ImportAsset(pending[i], ImportAssetOptions.ForceUpdate);
            }
            EditorUtility.ClearProgressBar();

            return changed;
        }

        /// dx5 = true：PC 平台挂 DXT5（BC3，带 alpha，才存得住 X 通道）
        /// dx5 = false：清掉覆盖，法线图交给 Unity 的 NormalMap 流程自己选 BC5
        static bool SetDxt5Override(TextureImporter importer, bool dxt5)
        {
            var ps = importer.GetPlatformTextureSettings("DefaultTexturePlatform");

            if (!dxt5)
            {
                if (!ps.overridden) return false;
                ps.overridden = false;
                importer.SetPlatformTextureSettings(ps);
                return true;
            }

            if (ps.overridden && ps.format == TextureImporterFormat.DXT5) return false;

            ps.name = "DefaultTexturePlatform";
            ps.overridden = true;
            ps.format = TextureImporterFormat.DXT5;
            ps.compressionQuality = 100;
            importer.SetPlatformTextureSettings(ps);
            return true;
        }

        static void MarkTex(Dictionary<string, TexWant> wanted, string texAssetDir, string rel,
                            bool srgb, bool isNormal)
        {
            if (string.IsNullOrEmpty(rel)) return;
            string assetPath = texAssetDir + "/" + Path.GetFileName(rel);
            TexWant prev;
            if (wanted.TryGetValue(assetPath, out prev))
            {
                // 同一张图既当基础色又当数据图时不降级，宁可留 sRGB；
                // 已经标成法线的，后面再来数据图需求也得保留法线标记。
                if (prev.srgb) return;
                if (prev.isNormal) isNormal = true;
            }
            wanted[assetPath] = new TexWant { srgb = srgb, isNormal = isNormal };
        }

        /// 数一下清单里 Blender 检测到的法线通道布局，用来跟开关对表。
        /// manifest 的 normalMode：0 = RGB（B 即 Z），1 = 原始 DXT5nm（R 恒为 1）。
        static void CountNormalMode(Manifest manifest, out int rgb, out int dxt5nm)
        {
            rgb = 0;
            dxt5nm = 0;
            if (manifest == null || manifest.materials == null) return;
            foreach (var me in manifest.materials)
            {
                if (!me.hasNormal || string.IsNullOrEmpty(me.normalTex)) continue;
                if (me.normalMode > 0) dxt5nm++;
                else rgb++;
            }
        }

        /// 建 .mat 资产，返回 {材质名: Material}。
        /// 贴图从 Blender 导出的 textures/ 拷进 <场景目录>/Textures/，再按 base/normal/rmo 绑上。
        /// normalDx5 决定法线贴图的 meta 类型，同时决定材质上的 _NormalMode，两边必须一致。
        static Dictionary<string, Material> BuildMaterials(Manifest manifest, string outAssetDir,
                                                           string srcRoot, bool normalDx5, ref int built)
        {
            var map = new Dictionary<string, Material>();
            built = 0;
            if (manifest.materials == null || manifest.materials.Length == 0)
                return map;

            string matAssetDir = outAssetDir + "/Materials";
            string texAssetDir = outAssetDir + "/Textures";
            EnsureFolder(matAssetDir);
            EnsureFolder(texAssetDir);

            string srcTexAbs = Path.Combine(srcRoot, "textures");
            string dstTexAbs = Path.Combine(ProjectRoot, texAssetDir.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(dstTexAbs);

            // 收集所有引用的贴图文件名，一次性拷
            var texNames = new HashSet<string>();
            foreach (var me in manifest.materials)
            {
                foreach (var t in new[] { me.baseTex, me.normalTex, me.rmoTex })
                    if (!string.IsNullOrEmpty(t)) texNames.Add(Path.GetFileName(t));
            }

            // 贴图源目录不在 + 清单里又确实引用了贴图 = 必然是白模。
            // 以前这里一声不响地跳过，最后只在场景里看到一片白，根本不知道哪一步断的。
            bool srcTexMissing = !Directory.Exists(srcTexAbs);
            if (srcTexMissing && texNames.Count > 0)
                Debug.LogError("[BlenderSceneImporter] Blender 导出目录里根本没有 textures/:\n  " + srcTexAbs +
                               "\n清单引用了 " + texNames.Count + " 张贴图，一张都拷不到 —— 材质会全白。\n" +
                               "先在 Blender 里重新导出一次（新版插件会把打包进 .blend 的贴图也落盘）。");

            int copiedTex = 0, noSrcTex = 0;
            var noSrcTexNames = new List<string>();
            foreach (var n in texNames)
            {
                string s = Path.Combine(srcTexAbs, n);
                string d = Path.Combine(dstTexAbs, n);
                if (!File.Exists(s))
                {
                    // 目标里已经有同名的也能用（上次导过的），那就不算缺
                    if (!File.Exists(d))
                    {
                        noSrcTex++;
                        if (noSrcTexNames.Count < 10) noSrcTexNames.Add(n);
                    }
                    continue;
                }
                if (!File.Exists(d) || new FileInfo(s).LastWriteTimeUtc > new FileInfo(d).LastWriteTimeUtc)
                {
                    File.Copy(s, d, true);
                    copiedTex++;
                }
            }

            if (noSrcTex > 0)
                Debug.LogError(string.Format(CultureInfo.InvariantCulture,
                    "[BlenderSceneImporter] {0} 张贴图在源目录和目标目录里都找不到，这些材质会缺图（白模）: {1}{2}",
                    noSrcTex, string.Join(", ", noSrcTexNames.ToArray()),
                    noSrcTex > noSrcTexNames.Count ? " ..." : ""));

            if (texNames.Count > 0)
                AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);

            // Unity 的资产路径大小写敏感，Blender 写出来的名字和实际落盘差一个字母就 Load 到 null，
            // 表现同样是白模。建一张「文件名 → 资产路径」的表兜底。
            var texLookup = BuildTexLookup(texAssetDir);

            // ---- 贴图 meta：基础色吃 sRGB，法线 / SMO 是数据图必须线性。
            //      法线的类型和压缩格式按 normalDx5 切（NormalMap / Default+DXT5）。
            int srgbFixed, normalFixed;
            int texFixed = ApplyTextureImportSettings(manifest, texAssetDir, normalDx5,
                                                      out srgbFixed, out normalFixed);
            if (texFixed > 0)
                Debug.Log(string.Format(CultureInfo.InvariantCulture,
                    "[BlenderSceneImporter] 改了 {0} 张贴图的 meta（关 sRGB {1} 张 / 法线图按 {2} 处理 {3} 张）",
                    texFixed, srgbFixed, normalDx5 ? "DXT5nm：Default + PC 压 DXT5" : "普通法线图：NormalMap", normalFixed));

            // 清单里 Blender 检测到的通道布局跟开关对不上就说一声，光靠猜会解出歪法线
            int nmRgb, nmDxt5;
            CountNormalMode(manifest, out nmRgb, out nmDxt5);
            if (normalDx5 && nmRgb > 0)
                Debug.LogWarning(string.Format(CultureInfo.InvariantCulture,
                    "[BlenderSceneImporter] 勾了 DXT5nm，但清单里有 {0} 个材质标的是 RGB 法线，这批结构可能不对。", nmRgb));
            else if (!normalDx5 && nmDxt5 > 0)
                Debug.LogWarning(string.Format(CultureInfo.InvariantCulture,
                    "[BlenderSceneImporter] 没勾 DXT5nm，但清单里有 {0} 个材质标的是原始 DXT5nm，按普通法线图导入可能解错。", nmDxt5));

            var shader = Shader.Find(PbrShaderName);
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            bool glsa = shader != null && shader.name == PbrShaderName;
            if (!glsa)
                Debug.LogWarning("[BlenderSceneImporter] 没找到 " + PbrShaderName +
                                 "，退回 URP/Lit，RMO / 法线模式都会绑不准。");

            var usedNames = new HashSet<string>();
            bool warnedDx5Fallback = false;
            int wantBase = 0, wantNormal = 0, wantRmo = 0;
            int boundBase = 0, boundNormal = 0, boundRmo = 0;
            foreach (var me in manifest.materials)
            {
                if (string.IsNullOrEmpty(me.name)) continue;

                string fname = Sanitize(me.name);
                if (string.IsNullOrEmpty(fname)) fname = "mat";

                string unique = fname;
                int idx = 1;
                while (usedNames.Contains(unique))
                {
                    unique = fname + "_" + idx;
                    idx++;
                }
                usedNames.Add(unique);

                string matPath = matAssetDir + "/" + unique + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                bool isNew = mat == null;
                if (isNew) mat = new Material(shader);
                else mat.shader = shader;
                mat.name = unique;

                Texture2D Load(string rel)
                {
                    if (string.IsNullOrEmpty(rel)) return null;
                    string fn = Path.GetFileName(rel);
                    string p;
                    if (texLookup.TryGetValue(fn, out p))
                        return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                    // 表里没有就退回直拼（老版本没有表也能跑）
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(texAssetDir + "/" + fn);
                }

                if (me.baseTex != null) wantBase++;
                if (me.normalTex != null) wantNormal++;
                if (me.rmoTex != null) wantRmo++;

                var bt = Load(me.baseTex);
                if (bt != null)
                {
                    mat.SetTexture("_BaseMap", bt);
                    mat.SetTexture("_MainTex", bt);
                    boundBase++;
                }
                else if (me.baseColor != null && me.baseColor.Length >= 3)
                {
                    // 纯色材质：Blender 里基础色没接贴图，把那个颜色带过来，
                    // 否则 Unity 这边只有白色，整场景会显得比 Blender 亮一截。
                    mat.SetColor("_BaseColor", new Color(
                        me.baseColor[0], me.baseColor[1], me.baseColor[2],
                        me.baseColor.Length > 3 ? me.baseColor[3] : 1f));
                }

                if (glsa)
                {
                    // GLSA/Scene/PBR_EnvStandard
                    //   _BaseMap   Albedo
                    //   _NRMTex    法线贴图（_NormalMode 决定怎么解）
                    //   _SMO       R=Smoothness G=Metallic B=Occlusion A=EmissionMask
                    // _NormalMode: 0 = RGB(BC7)，1 = 原始 DXT5nm 通道，2 = Unity 已编码的法线贴图。
                    // 必须跟 meta 成对：NormalMap 导入的图走 2，Default + DXT5 的走 1。
                    var nt = Load(me.normalTex);
                    if (nt != null)
                    {
                        mat.SetTexture("_NRMTex", nt);
                        mat.SetFloat("_NormalMode", normalDx5 ? 1f : 2f);
                        mat.SetFloat("_UseNormalMap", 1f);
                        boundNormal++;
                    }
                    else
                    {
                        mat.SetFloat("_NormalMode", 0f);
                        // 没图就显式关掉，别让 shader 拿着 gray 默认图 + NormalScale 乱来
                        mat.SetFloat("_UseNormalMap", 0f);
                    }

                    var rt = Load(me.rmoTex);
                    if (rt != null)
                    {
                        mat.SetTexture("_SMO", rt);
                        boundRmo++;
                    }

                    // 贴图已经带了这些，标量拉满，免得二次衰减
                    mat.SetFloat("_Metallic", 1f);
                    mat.SetFloat("_Smoothness", 1f);
                    mat.SetFloat("_OcclusionStrength", 1f);
                    mat.SetFloat("_BumpScale", 1f);
                    mat.SetFloat("_NormalFlipY", 0f);
                    mat.SetFloat("_DXT5nmSwap", 0f);

                    if (me.alpha == "mask")
                    {
                        mat.SetFloat("_AlphaClip", 1f);
                        mat.EnableKeyword("_ALPHATEST_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    }
                    else
                    {
                        mat.SetFloat("_AlphaClip", 0f);
                        mat.DisableKeyword("_ALPHATEST_ON");
                    }
                }
                else
                {
                    // 退路：URP/Lit
                    var nt = Load(me.normalTex);
                    if (nt != null)
                    {
                        mat.SetTexture("_BumpMap", nt);
                        mat.EnableKeyword("_NORMALMAP");
                        // _BumpMap 走的是 Unity 已编码法线那套；原始 DXT5nm 的图会被解错
                        if (normalDx5 && !warnedDx5Fallback)
                        {
                            warnedDx5Fallback = true;
                            Debug.LogWarning("[BlenderSceneImporter] DXT5nm 的图挂到 URP/Lit 的 _BumpMap 上解不对，" +
                                             "想要正确效果得用 " + PbrShaderName + "。");
                        }
                    }

                    var rt = Load(me.rmoTex);
                    if (rt != null)
                    {
                        mat.SetTexture("_OcclusionMap", rt);
                        boundRmo++;
                    }

                    if (me.alpha == "mask")
                    {
                        mat.SetFloat("_AlphaClip", 1f);
                        mat.EnableKeyword("_ALPHATEST_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    }
                }

                if (isNew) AssetDatabase.CreateAsset(mat, matPath);
                else EditorUtility.SetDirty(mat);

                built++;
                map[me.name] = mat;
            }

            AssetDatabase.SaveAssets();

            // 一条汇总：绑上几张、清单里要几张、拷了几张。
            // 白模的时候看这一行就能分清是「清单没给路径」还是「给了路径但图上没到」。
            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[BlenderSceneImporter] 材质 {0} 个 / 贴图 引用 {1}·拷入 {2} / 绑上 base {3}/{4} normal {5}/{6} rmo {7}/{8}",
                built, texNames.Count, copiedTex,
                boundBase, wantBase, boundNormal, wantNormal, boundRmo, wantRmo));

            if (built > 0 && wantBase > 0 && boundBase == 0)
                Debug.LogError("[BlenderSceneImporter] " + built + " 个材质一条 base 贴图都没绑上 —— 场景必然是白模。\n" +
                               "先确认 Blender 导出目录的 textures/ 里有文件，再看清单里的 baseTex 是不是全是 null。\n" +
                               "（老版 Blender 插件只认磁盘上的外部贴图，打包进 .blend 的图会全部漏掉。）");

            return map;
        }

        /// 在 Textures 目录里建「文件名 → 资产路径」表，键不分大小写。
        /// Unity 的资产路径大小写敏感：Blender 写出来是 Wall_A.png、落盘是 wall_a.png，
        /// 直接拼路径 Load 就是 null，材质看着像没贴图。
        static Dictionary<string, string> BuildTexLookup(string texAssetDir)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!AssetDatabase.IsValidFolder(texAssetDir)) return d;

            foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { texAssetDir }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                string f = Path.GetFileName(p);
                if (!string.IsNullOrEmpty(f) && !d.ContainsKey(f)) d[f] = p;
            }
            return d;
        }

        // ============================================================ 模型导入设置

        /// 场景 FBX 的 ModelImporter 统一配置 + 材质外部映射。
        ///
        /// 为什么要自己设：Unity 默认值对「从 Blender 搬过来的静态场景件」全是浪费或坑。
        ///   animationType / importAnimation : Blender 导出时会给每个物体打一帧关键帧，
        ///       导入后生成 Take 001（多半是 0 条曲线）。留着的话万一被播放，
        ///       会盖掉本工具按 manifest 摆好的 transform。
        ///   importCameras / importLights    : Blender 那边导出范围选「全部/集合」时会把相机和灯
        ///       一起打进 FBX，导入后场景里凭空多出 Camera / Light，还会抢 MainCamera。
        ///   importBlendShapes / DeformPercent / Visibility : 静态件一个都用不上。
        ///   weldVertices                    : Blender 已经分好顶点，Unity 再焊会吃掉硬边。
        ///   materialLocation = External + AddRemap : 不设的话材质生成在 FBX 内部，
        ///       拖进场景是灰的，得手动挂；设完直接引用 Materials/ 下的 .mat。
        ///
        /// 注意 AddRemap 的 key 是 **FBX 内部的材质名**，必须在切成 External 之前读，
        /// 切完内部 Material 子资产就没了。
        static void ApplyModelImportSettings(string fbxAssetDir, string matAssetDir,
                                             Dictionary<string, Material> matMap)
        {
            if (!AssetDatabase.IsValidFolder(fbxAssetDir))
            {
                Debug.LogWarning("[BlenderSceneImporter] 找不到 FBX 目录，跳过导入设置: " + fbxAssetDir);
                return;
            }

            // FindAssets 放在批量编辑外面，避免批量编辑期间查资源库拿不到新数据
            var paths = new List<string>();
            foreach (var g in AssetDatabase.FindAssets("t:Model", new[] { fbxAssetDir }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) paths.Add(p);
            }

            ApplyModelImportSettings(paths, matAssetDir, matMap);
        }

        /// 同上，但只处理指定的 FBX 列表（增量更新用，不动其他文件）
        static void ApplyModelImportSettings(List<string> fbxPaths, string matAssetDir,
                                             Dictionary<string, Material> matMap)
        {
            if (fbxPaths == null || fbxPaths.Count == 0)
            {
                Debug.Log("[BlenderSceneImporter] 没有需要更新导入设置的 FBX。");
                return;
            }

            int fbxCount = 0, remapped = 0, noMat = 0;
            var noMatNames = new List<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int gi = 0; gi < fbxPaths.Count; gi++)
                {
                    string fbxPath = fbxPaths[gi];

                    var mi = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                    if (mi == null) continue;
                    fbxCount++;

                    if (fbxCount % 50 == 0)
                        EditorUtility.DisplayProgressBar("设置 FBX 导入项", fbxPath, (float)gi / fbxPaths.Count);

                    // 先记下内部材质名（多材质模型会有多个槽）。
                    // 注意：materialLocation 一旦是 External，FBX 里就没有 Material 子资产了，
                    // 这个循环会拿到空表，重导时材质就一条都对不上。所以再补一条退路：
                    // 从外部对象映射（存在 .meta 里，重导不会丢）里把名字捞回来。
                    var srcMatNames = new List<string>();
                    foreach (var a in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                    {
                        var m = a as Material;
                        if (m != null && !srcMatNames.Contains(m.name)) srcMatNames.Add(m.name);
                    }
                    if (srcMatNames.Count == 0)
                    {
                        foreach (var kv in mi.GetExternalObjectMap())
                        {
                            if (kv.Key.type == typeof(Material) && !string.IsNullOrEmpty(kv.Key.name)
                                && !srcMatNames.Contains(kv.Key.name))
                                srcMatNames.Add(kv.Key.name);
                        }
                    }

                    mi.animationType = ModelImporterAnimationType.None;
                    mi.importAnimation = false;
                    mi.importBlendShapes = false;
                    mi.importBlendShapeDeformPercent = false;
                    mi.importVisibility = false;
                    mi.importCameras = false;
                    mi.importLights = false;
                    mi.weldVertices = false;
                    mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                    mi.materialLocation = ModelImporterMaterialLocation.External;

                    for (int si = 0; si < srcMatNames.Count; si++)
                    {
                        string srcName = srcMatNames[si];
                        var mat = FindMaterial(matMap, srcName);
                        if (mat == null)
                            // .mat 的文件名是 Sanitize 过的，这里也得过一遍，否则带冒号/斜杠的名字找不到
                            mat = AssetDatabase.LoadAssetAtPath<Material>(
                                matAssetDir + "/" + Sanitize(srcName) + ".mat");
                        if (mat == null)
                            mat = FindMaterialByFileName(matAssetDir, srcName);

                        if (mat == null)
                        {
                            noMat++;
                            if (noMatNames.Count < 8) noMatNames.Add(Path.GetFileName(fbxPath) + " -> " + srcName);
                            continue;
                        }

                        mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), srcName), mat);
                        remapped++;
                    }

                    mi.SaveAndReimport();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }

            sw.Stop();
            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[BlenderSceneImporter] FBX 导入设置: {0} 个文件 / 材质映射 {1} 条 / 无对应材质 {2} 条（{3} ms）",
                fbxCount, remapped, noMat, sw.ElapsedMilliseconds));

            if (noMatNames.Count > 0)
                Debug.LogWarning("[BlenderSceneImporter] 这些 FBX 材质没找到对应 .mat，拖进场景会是灰的: "
                                 + string.Join(", ", noMatNames.ToArray()));
        }

        /// 按 manifest 材质名或资产名找材质
        static Material FindMaterial(Dictionary<string, Material> map, string name)
        {
            if (map == null || string.IsNullOrEmpty(name)) return null;

            Material hit;
            if (map.TryGetValue(name, out hit) && hit != null) return hit;

            foreach (var kv in map)
            {
                if (kv.Value != null && kv.Value.name == name) return kv.Value;
            }
            return null;
        }

        /// 兜底：按 Sanitize 后的文件名在 Materials 目录里找 .mat（不分大小写）。
        /// Blender 的材质名可能带空格/全角符号，.mat 文件名是 Sanitize 过的，
        /// 对不上就会「无对应材质」，FBX 拖进场景是灰的。
        static Material FindMaterialByFileName(string matAssetDir, string name)
        {
            if (string.IsNullOrEmpty(name) || !AssetDatabase.IsValidFolder(matAssetDir)) return null;
            string want = Sanitize(name);
            if (string.IsNullOrEmpty(want)) return null;

            foreach (var g in AssetDatabase.FindAssets("t:Material", new[] { matAssetDir }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (string.Equals(Path.GetFileNameWithoutExtension(p), want, StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<Material>(p);
            }
            return null;
        }

        // ============================================================ 只重建材质

        /// 只重建「材质 + 贴图」，**不动场景、不动 FBX**。
        ///
        /// 白模基本都出在这一步：贴图没落盘，或者落了但没绑上。
        /// 重跑这个方法比整场重导快得多，而且场景里的摆放、层级、Prefab 覆盖全都不动，
        /// 已经修好的位置不会被再摆一遍。
        ///
        /// remapFbx = true 时连 FBX 的外部材质映射一起刷（慢，会给每个 FBX 存一次导入设置）；
        /// 场景里的实例本来就是直接引用 Materials/*.mat 的 GUID，光是换 .mat 的内容就夠了，
        /// 所以默认不开。
        public static void RebuildMaterials(string srcRoot, string outAssetRoot, string sceneName,
                                            bool normalDx5 = false, bool remapFbx = false)
        {
            string manifestPath = Path.Combine(srcRoot, "scene_transform.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("[BlenderSceneImporter] 找不到清单: " + manifestPath);
                if (!quiet) EditorUtility.DisplayDialog("找不到清单", "没有找到:\n" + manifestPath, "好");
                return;
            }

            Manifest manifest;
            try { manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath)); }
            catch { manifest = null; }
            if (manifest == null || manifest.materials == null || manifest.materials.Length == 0)
            {
                Debug.LogError("[BlenderSceneImporter] 清单里没有 materials。");
                if (!quiet) EditorUtility.DisplayDialog("清单没有材质", "scene_transform.json 里没有 materials。", "好");
                return;
            }

            string name = Sanitize(sceneName);
            if (string.IsNullOrEmpty(name)) name = Sanitize(manifest.sourceScene);
            if (string.IsNullOrEmpty(name)) name = "BlenderScene";

            string outAssetDir = (outAssetRoot.TrimEnd('/', '\\') + "/" + name).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(outAssetDir))
            {
                Debug.LogError("[BlenderSceneImporter] 目标目录不存在，先完整导入一次: " + outAssetDir);
                if (!quiet) EditorUtility.DisplayDialog("找不到目标目录",
                    "没有:\n" + outAssetDir + "\n\n先完整导入一次。", "好");
                return;
            }

            int matBuilt = 0;
            var matMap = BuildMaterials(manifest, outAssetDir, srcRoot, normalDx5, ref matBuilt);

            if (remapFbx)
                ApplyModelImportSettings(outAssetDir + "/FBX", outAssetDir + "/Materials", matMap);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 回头数一下场景里还有多少材质球是空的，直接把结果说出来，不用自己翻
            int empty = 0, total = 0;
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        foreach (var m in r.sharedMaterials)
                        {
                            total++;
                            if (m == null) { empty++; continue; }
                            if (m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") == null) empty++;
                        }
                    }
                }
            }

            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[BlenderSceneImporter] 重建材质完成: {0} 个 .mat / 场景材质槽 {1} 个，其中没基础色贴图的 {2} 个\n  目录: {3}",
                matBuilt, total, empty, outAssetDir));

            if (!quiet) EditorUtility.DisplayDialog("材质重建完成",
                "材质 " + matBuilt + " 个\n场景材质槽 " + total + " 个（没基础贴图 " + empty + " 个）" +
                "\n\n目录:\n" + outAssetDir, "好");
        }

        // ============================================================ 增量更新

        /// 只把变了的 FBX 拷回来重导入，**不碰场景**。
        ///
        /// 场景里的物体是 Prefab 实例，FBX 资产一更新，MeshFilter 上的 Mesh 引用自动换新的，
        /// 位置/材质/层级都不动，比整个 ImportInto 快得多。
        ///
        /// onlySelected = true：按场景里选中的物体名去清单里找 FBX，只处理这些；
        ///                  false：比源目录和目标目录的修改时间，只处理源比目标新的。
        public static int UpdateChangedMeshes(string srcRoot, string outAssetRoot, string sceneName,
                                              bool onlySelected = false)
        {
            string manifestPath = Path.Combine(srcRoot, "scene_transform.json");
            string srcFbx = Path.Combine(srcRoot, "fbx");

            if (!File.Exists(manifestPath) || !Directory.Exists(srcFbx))
            {
                Debug.LogWarning("[BlenderSceneImporter] 找不到清单或 fbx 目录: " + srcRoot);
                if (!quiet) EditorUtility.DisplayDialog("没有可用的导出", "找不到:\n" + manifestPath, "好");
                return 0;
            }

            Manifest manifest;
            try { manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath)); }
            catch { manifest = null; }
            if (manifest == null || manifest.instances == null || manifest.instances.Length == 0)
            {
                Debug.LogWarning("[BlenderSceneImporter] 清单读不出来或没有 instances。");
                return 0;
            }

            string name = Sanitize(sceneName);
            if (string.IsNullOrEmpty(name)) name = Sanitize(manifest.sourceScene);
            if (string.IsNullOrEmpty(name)) name = "BlenderScene";

            string outAssetDir = (outAssetRoot.TrimEnd('/', '\\') + "/" + name).Replace('\\', '/');
            string fbxAssetDir = outAssetDir + "/FBX";
            string matAssetDir = outAssetDir + "/Materials";

            if (!AssetDatabase.IsValidFolder(fbxAssetDir))
            {
                Debug.LogWarning("[BlenderSceneImporter] 目标场景目录里没有 FBX 文件夹: " + fbxAssetDir);
                if (!quiet) EditorUtility.DisplayDialog("找不到目标目录",
                    "没有:\n" + fbxAssetDir + "\n\n先用「导入 Blender 场景」完整导入一次。", "好");
                return 0;
            }

            string fbxAbsDir = Path.Combine(ProjectRoot, fbxAssetDir.Replace('/', Path.DirectorySeparatorChar));

            // 实例名 -> FBX 文件名
            var nameToFbx = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var e in manifest.instances)
            {
                if (!string.IsNullOrEmpty(e.name) && !string.IsNullOrEmpty(e.fbx))
                    nameToFbx[e.name] = Path.GetFileName(e.fbx);
            }

            var wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (onlySelected)
            {
                var picked = Selection.gameObjects;
                for (int i = 0; i < picked.Length; i++)
                {
                    string instName = FindInstanceName(picked[i] != null ? picked[i].transform : null, nameToFbx);
                    if (!string.IsNullOrEmpty(instName)) wanted.Add(nameToFbx[instName]);
                }

                if (wanted.Count == 0)
                {
                    Debug.LogWarning("[BlenderSceneImporter] 选中的物体在清单里找不到对应的 FBX。");
                    if (!quiet) EditorUtility.DisplayDialog("没有可更新的",
                        "选中的物体在清单里对不上 FBX。\n\n先在场景里选中要更新的物体。", "好");
                    return 0;
                }
            }

            // ---- 1. 拷变了的 FBX
            var affected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var kv in nameToFbx)
            {
                if (onlySelected && !wanted.Contains(kv.Value)) continue;
                affected.Add(kv.Key);
            }

            var copied = new List<string>();
            var files = Directory.GetFiles(srcFbx, "*.fbx");
            for (int i = 0; i < files.Length; i++)
            {
                string fname = Path.GetFileName(files[i]);
                if (onlySelected && !wanted.Contains(fname)) continue;

                string dst = Path.Combine(fbxAbsDir, fname);
                bool need = !File.Exists(dst) ||
                            new FileInfo(files[i]).LastWriteTimeUtc > new FileInfo(dst).LastWriteTimeUtc;
                if (!need) continue;

                File.Copy(files[i], dst, true);
                copied.Add(fbxAssetDir + "/" + fname);
            }

            if (copied.Count > 0)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
                ApplyModelImportSettings(copied, matAssetDir, null);
            }

            // ---- 2. 再按清单同步场景实例的 transform
            // 必须排在 Refresh 之后：FBX 重导入会重建 prefab 并重新套实例 override，
            // 早于它设的位置会被冲掉。
            int instMoved = 0, instMissing = 0;
            UpdateInstanceTransforms(manifest, affected, ref instMoved, ref instMissing);

            if (copied.Count == 0 && instMoved == 0)
            {
                Debug.Log(string.Format(CultureInfo.InvariantCulture,
                    "[BlenderSceneImporter] 没有 FBX 需要更新；场景实例 transform 同步了 {0} 个（缺 {1} 个）。",
                    instMoved, instMissing));
                if (!quiet) EditorUtility.DisplayDialog("没有变化",
                    "FBX 都不比目标新，场景实例 transform 同步了 " + instMoved + " 个。", "好");
                return 0;
            }

            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[BlenderSceneImporter] 增量更新: {0} 个 FBX + {1} 个实例 transform（缺 {2}）: {3}",
                copied.Count, instMoved, instMissing,
                copied.Count <= 8 ? string.Join(", ", copied.ToArray()) : copied[0] + " ... " + copied[copied.Count - 1]));

            if (!quiet) EditorUtility.DisplayDialog("更新完成",
                "更新了 " + copied.Count + " 个 FBX" + (onlySelected ? "（选中范围）" : "（按修改时间）") +
                "\n场景实例 transform 同步了 " + instMoved + " 个" +
                "\n\nMesh 和位置都换成清单里的最新状态。", "好");

            return copied.Count;
        }

        /// 按 manifest 的 bPos / bRot / bScale 重摆场景里的实例。
        ///
        /// 摆法和 ImportInto 一致：P = F·B·S·R90X，再并进 FBX 根自带的 C（270°X + 100 缩放）。
        /// 关键点：实例的 local 早就被上一次 ApplyMatrix 覆盖过了，不能拿它当 C ——
        /// 必须先从 prefab 源把原始 local 还原回来，否则每跑一次就再叠一次 P。
        static void UpdateInstanceTransforms(Manifest manifest, HashSet<string> instanceNames,
                                             ref int moved, ref int missing)
        {
            if (manifest == null || manifest.instances == null || instanceNames == null || instanceNames.Count == 0)
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            // 场景里按名字找实例
            var found = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (instanceNames.Contains(t.name) && !found.ContainsKey(t.name))
                        found[t.name] = t;
                }
            }

            if (found.Count == 0) return;

            foreach (var kv in found)
            {
                if (kv.Value != null)
                    Undo.RegisterFullObjectHierarchyUndo(kv.Value.gameObject, "S2U 增量更新 transform");
            }

            foreach (var e in manifest.instances)
            {
                if (e == null || string.IsNullOrEmpty(e.name)) continue;
                if (!instanceNames.Contains(e.name)) continue;

                Transform t;
                if (!found.TryGetValue(e.name, out t) || t == null)
                {
                    missing++;
                    continue;
                }

                // 还原 FBX 根自带的原始局部变换（C）
                var src = PrefabUtility.GetCorrespondingObjectFromSource(t);
                if (src != null)
                {
                    t.localPosition = src.localPosition;
                    t.localRotation = src.localRotation;
                    t.localScale = src.localScale;
                }

                Matrix4x4 C = Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
                Matrix4x4 B = Matrix4x4.TRS(ToVec3(e.bPos), ToQuat(e.bRot), ToVec3(e.bScale, Vector3.one));
                Matrix4x4 P = BuildPlacementMatrix(B);

                ApplyMatrix(t, P * C);
                moved++;
            }

            if (moved > 0) EditorSceneManager.MarkSceneDirty(scene);
        }

        /// P = F · B · S · R90X（和 ImportInto 里那套完全一样）
        static Matrix4x4 BuildPlacementMatrix(Matrix4x4 B)
        {
            Matrix4x4 flipYZ = Matrix4x4.identity;
            flipYZ.m11 = 0; flipYZ.m12 = 1;
            flipYZ.m21 = 1; flipYZ.m22 = 0;

            Matrix4x4 mirrorX = Matrix4x4.identity;
            mirrorX.m00 = -1f;

            Matrix4x4 rot90X = Matrix4x4.Rotate(Quaternion.Euler(90f, 0f, 0f));

            return flipYZ * B * mirrorX * rot90X;
        }

        /// 从选中的物体往上找，直到名字能在清单里查到实例
        static string FindInstanceName(Transform t, Dictionary<string, string> nameToFbx)
        {
            var cur = t;
            while (cur != null)
            {
                if (nameToFbx.ContainsKey(cur.name)) return cur.name;
                cur = cur.parent;
            }
            return null;
        }

        // ============================================================ 导入新增物体

        /// <summary>
        /// 只把清单里「当前场景上还没有的」物体加进**当前打开的场景**：
        /// 不新建场景、不动已有实例的位置和材质、不建相机和灯。
        ///
        /// 用法：Blender 里新建物体 → N 面板「导出选中」→ Unity 点这个按钮。
        ///
        /// 「已经有了」按实例名判定（Blender 物体名）。本方法建出来的实例会把 GameObject
        /// 名字显式设成清单里的 name，所以同一个清单重复点按钮不会重复加。
        ///
        /// 返回真正加进来的实例数。
        /// </summary>
        public static int ImportAddedObjects(string srcRoot, string outAssetRoot, string sceneName,
                                             bool normalDx5 = false)
        {
            string manifestPath = Path.Combine(srcRoot, "scene_transform.json");
            string srcFbx = Path.Combine(srcRoot, "fbx");

            if (!File.Exists(manifestPath) || !Directory.Exists(srcFbx))
            {
                Debug.LogWarning("[BlenderSceneImporter] 找不到清单或 fbx 目录: " + srcRoot);
                if (!quiet) EditorUtility.DisplayDialog("没有可用的导出", "找不到:\n" + manifestPath, "好");
                return 0;
            }

            Manifest manifest;
            try { manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath)); }
            catch { manifest = null; }
            if (manifest == null || manifest.instances == null || manifest.instances.Length == 0)
            {
                Debug.LogWarning("[BlenderSceneImporter] 清单读不出来或没有 instances。");
                if (!quiet) EditorUtility.DisplayDialog("清单为空", "scene_transform.json 里没有 instances。", "好");
                return 0;
            }

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning("[BlenderSceneImporter] 当前没有打开的场景。");
                return 0;
            }
            bool sceneWasDirty = scene.isDirty;

            string name = Sanitize(sceneName);
            if (string.IsNullOrEmpty(name)) name = Sanitize(manifest.sourceScene);
            if (string.IsNullOrEmpty(name)) name = "BlenderScene";

            string outAssetDir = (outAssetRoot.TrimEnd('/', '\\') + "/" + name).Replace('\\', '/');
            string fbxAssetDir = outAssetDir + "/FBX";
            string matAssetDir = outAssetDir + "/Materials";

            if (!AssetDatabase.IsValidFolder(fbxAssetDir))
            {
                Debug.LogWarning("[BlenderSceneImporter] 目标场景目录里没有 FBX 文件夹: " + fbxAssetDir);
                if (!quiet) EditorUtility.DisplayDialog("找不到目标目录",
                    "没有:\n" + fbxAssetDir +
                    "\n\n先用「导入 Blender 场景」完整导入一次，新物体才有地方放。", "好");
                return 0;
            }

            // ---- 1. 当前场景里已经有什么（全场景按名字收，不只 Imported 根下面）
            var existing = new HashSet<string>(StringComparer.Ordinal);
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!string.IsNullOrEmpty(t.name)) existing.Add(t.name);
                }
            }

            var added = new List<InstanceEntry>();
            int skipped = 0;
            foreach (var e in manifest.instances)
            {
                if (e == null || string.IsNullOrEmpty(e.name)) continue;

                // 老的全量导入是并行导出的，FBX 根名 = 物体名，所以 name 和 data 名都得比一次
                string dataName = string.IsNullOrEmpty(e.fbx)
                    ? null : Path.GetFileNameWithoutExtension(e.fbx);
                if (existing.Contains(e.name) ||
                    (!string.IsNullOrEmpty(dataName) && existing.Contains(dataName)))
                {
                    skipped++;
                    continue;
                }
                added.Add(e);
            }

            if (added.Count == 0)
            {
                Debug.Log(string.Format(CultureInfo.InvariantCulture,
                    "[BlenderSceneImporter] 清单里 {0} 个实例当前场景都已经有了（跳过 {1} 个），没有新物体可加。",
                    manifest.instances.Length, skipped));
                if (!quiet) EditorUtility.DisplayDialog("没有新物体",
                    "清单里的 " + manifest.instances.Length + " 个实例，当前场景里都已经有了。\n\n" +
                    "要重新加，先把场景里对应的物体删掉；或者回 Blender 新建物体再「导出选中」一次。", "好");
                return 0;
            }

            // ---- 2. 只拷新物体用到的 FBX
            var needFbx = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in added)
                if (!string.IsNullOrEmpty(e.fbx)) needFbx.Add(Path.GetFileName(e.fbx));

            string fbxAbsDir = Path.Combine(ProjectRoot, fbxAssetDir.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(fbxAbsDir);

            var copied = new List<string>();
            var missingSrcFbx = new List<string>();
            foreach (var f in needFbx)
            {
                string s = Path.Combine(srcFbx, f);
                if (!File.Exists(s)) { missingSrcFbx.Add(f); continue; }

                string d = Path.Combine(fbxAbsDir, f);
                bool need = !File.Exists(d) ||
                            new FileInfo(s).LastWriteTimeUtc > new FileInfo(d).LastWriteTimeUtc;
                if (!need) continue;

                File.Copy(s, d, true);
                copied.Add(fbxAssetDir + "/" + f);
            }

            if (copied.Count > 0)
                AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);

            // ---- 3. 材质 + 导入设置
            // BuildMaterials 只认清单里出现的材质，老材质原样不动（重名的会按清单重写一遍设置）。
            int matBuilt = 0;
            var matMap = BuildMaterials(manifest, outAssetDir, srcRoot, normalDx5, ref matBuilt);
            // 只给新拷进来的 FBX 设导入项：整个 FBX 目录扫一遍会把几百个老文件也 SaveAndReimport。
            if (copied.Count > 0) ApplyModelImportSettings(copied, matAssetDir, matMap);

            // ---- 4. 找（或建）统一父级
            Transform rootT = null;
            string expectRootName = name + "_Imported";
            foreach (var r in scene.GetRootGameObjects())
            {
                if (r.name == expectRootName) { rootT = r.transform; break; }
            }
            if (rootT == null)
            {
                // 场景名和当初导入时填的不一样时兜一下：场景里只有一个 *_Imported 根就认它
                Transform sole = null;
                int roots = 0;
                foreach (var r in scene.GetRootGameObjects())
                {
                    if (r.name.EndsWith("_Imported", StringComparison.Ordinal))
                    {
                        sole = r.transform;
                        roots++;
                    }
                }
                if (roots == 1) rootT = sole;
            }
            if (rootT == null)
            {
                var go = new GameObject(expectRootName);
                Undo.RegisterCreatedObjectUndo(go, "S2U 新建导入根节点");
                rootT = go.transform;
                Debug.LogWarning("[BlenderSceneImporter] 当前场景里没有 " + expectRootName +
                                 "，新建了一个，新物体都收在里面。");
            }

            // ---- 5. 摆
            var groups = new Dictionary<string, Transform>(StringComparer.Ordinal);
            var prefabCache = new Dictionary<string, GameObject>();
            var placedObjects = new List<GameObject>();
            int groupCount = 0, placed = 0, failed = 0;
            var missing = new List<string>();

            try
            {
                for (int i = 0; i < added.Count; i++)
                {
                    var e = added[i];
                    if (i % 20 == 0)
                        EditorUtility.DisplayProgressBar("导入新物体",
                            string.Format(CultureInfo.InvariantCulture, "{0}/{1}  {2}", i, added.Count, e.name),
                            (float)i / Mathf.Max(1, added.Count));

                    GameObject inst;
                    if (!TrySpawnInstance(e, rootT, groups, ref groupCount,
                                          matMap, fbxAssetDir, prefabCache, out inst))
                    {
                        failed++;
                        if (missing.Count < 12) missing.Add(e.fbx);
                        continue;
                    }

                    // 少于 150 个物体时 Blender 走 probe 路径导出，FBX 根节点名全是 __s2u_export_probe。
                    // 照搬就会在场景里堆一堆同名物体，下次再点这个按钮也认不出「已经有了」。
                    inst.name = e.name;
                    Undo.RegisterCreatedObjectUndo(inst, "S2U 导入新物体");

                    placedObjects.Add(inst);
                    placed++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (placedObjects.Count > 0) Selection.objects = placedObjects.ToArray();
            if (placed > 0) EditorSceneManager.MarkSceneDirty(scene);

            // 导入前场景本来就是干净的才自动保存，免得替用户保存一堆没想保存的临时改动
            bool saved = false;
            if (placed > 0 && !sceneWasDirty && !string.IsNullOrEmpty(scene.path))
            {
                EditorSceneManager.SaveScene(scene);
                saved = true;
            }

            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[BlenderSceneImporter] 导入新物体: 新增 {0} 个 / 跳过已存在 {1} 个 / 新拷 FBX {2} 个 / 材质 {3} 个 / " +
                "失败 {4} 个 / 分组 {5} 个\n  根: {6}\n  场景: {7} {8}",
                placed, skipped, copied.Count, matBuilt, failed, groupCount,
                rootT.name, scene.path, saved ? "(已保存)" : "(未保存)"));
            if (missing.Count > 0)
                Debug.LogWarning("[BlenderSceneImporter] 这些 FBX 没加载到: " + string.Join(", ", missing.ToArray()));
            if (missingSrcFbx.Count > 0)
                Debug.LogWarning("[BlenderSceneImporter] 源目录里缺这些 FBX，对应物体加不进来: "
                                 + string.Join(", ", missingSrcFbx.ToArray()));

            if (!quiet) EditorUtility.DisplayDialog("导入新物体完成",
                "新增 " + placed + " 个物体" + (skipped > 0 ? "（跳过已有的 " + skipped + " 个）" : "") +
                "\n新拷 FBX " + copied.Count + " 个   失败 " + failed + " 个" +
                "\n\n收进:\n" + rootT.name +
                "\n\n" + (saved ? "场景已保存" : "场景已标脏，记得 Ctrl+S"),
                "好");

            return placed;
        }

        // ============================================================ 窗口

        Vector2 _scroll;
        Manifest _manifest;
        string _manifestPath;

        [MenuItem("Tools/搬迁到 Blender", false, 1)]
        public static void OpenWindow()
        {
            var w = GetWindow<BlenderSceneImporter>("Blender 场景导入");
            w.minSize = new Vector2(460, 460);
            w.Show();
        }

        void OnEnable()
        {
            ReloadManifest();
        }

        void ReloadManifest()
        {
            _manifest = null;
            _manifestPath = Path.Combine(ResolveSrcRoot(), "scene_transform.json");
            if (File.Exists(_manifestPath))
            {
                try { _manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(_manifestPath)); }
                catch { _manifest = null; }
            }
            Repaint();
        }

        string PreviewOutDir()
        {
            string name = Sanitize(SceneName);
            if (string.IsNullOrEmpty(name) && _manifest != null)
                name = Sanitize(_manifest.sourceScene);
            if (string.IsNullOrEmpty(name)) name = "BlenderScene";
            return (OutRoot.TrimEnd('/', '\\') + "/" + name).Replace('\\', '/');
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            // ---------------------------------------------------- 菜鸡模式
            bool bm = EditorGUILayout.ToggleLeft("菜鸡模式（显示说明）", BeginnerMode);
            if (bm != BeginnerMode) BeginnerMode = bm;

            EditorGUILayout.Space(6);

            // ---------------------------------------------------- 反向导出
            EditorGUILayout.LabelField("⓪ 反向：Unity → Blender（glTF）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (BeginnerMode)
                    EditorGUILayout.LabelField(
                        "把当前 Unity 场景整体导出成 glTF（带世界变换 + PBR 材质 + 贴图），Blender 直接导入。",
                        EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("导出当前场景到 glTF (Blender)", GUILayout.Height(26)))
                    SceneSnapshotExporter.ExportCurrentScene();
            }

            EditorGUILayout.Space(10);

            // ---------------------------------------------------- 来源
            EditorGUILayout.LabelField("① Blender 导出目录", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string src = ResolveSrcRoot();
                EditorGUILayout.LabelField(src, EditorStyles.wordWrappedMiniLabel);

                if (_manifest != null)
                    EditorGUILayout.LabelField(ManifestToString(_manifest), EditorStyles.wordWrappedMiniLabel);
                else
                    EditorGUILayout.HelpBox("这里没有 scene_transform.json，先在 Blender 里导出一次。", MessageType.Warning);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("刷新", GUILayout.Width(70))) ReloadManifest();
                if (GUILayout.Button("打开目录", GUILayout.Width(80)))
                {
                    if (Directory.Exists(src)) EditorUtility.RevealInFinder(src);
                }
                if (GUILayout.Button("选择其他导出目录...", GUILayout.Width(160)))
                {
                    string picked = EditorUtility.OpenFolderPanel("选择 Blender 导出目录", src, "");
                    if (!string.IsNullOrEmpty(picked))
                    {
                        File.WriteAllText(Path.Combine(ProjectRoot, PointerFile), picked);
                        ReloadManifest();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(6);

            // ---------------------------------------------------- 输出位置
            EditorGUILayout.LabelField("② 输出到哪个文件夹", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                string newRoot = EditorGUILayout.TextField("输出根目录", OutRoot);
                if (EditorGUI.EndChangeCheck()) OutRoot = newRoot;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("设置为当前选中", GUILayout.Height(22)))
                    SetFromSelection();
                if (GUILayout.Button("浏览...", GUILayout.Width(80), GUILayout.Height(22)))
                    BrowseOutRoot();
                if (GUILayout.Button("重置", GUILayout.Width(70), GUILayout.Height(22)))
                {
                    OutRoot = DefaultOutRoot;
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndHorizontal();

                if (!AssetDatabase.IsValidFolder(OutRoot) && BeginnerMode)
                    EditorGUILayout.HelpBox("这个文件夹还不存在，导入时会自动创建。", MessageType.Info);
            }

            EditorGUILayout.Space(6);

            // ---------------------------------------------------- 场景名
            EditorGUILayout.LabelField("③ 新场景名称", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.SetNextControlName("SceneNameField");
                string newName = EditorGUILayout.TextField("场景名", SceneName);
                if (newName != SceneName) SceneName = newName;

                if (BeginnerMode)
                {
                    string hint = SceneName;
                    if (string.IsNullOrEmpty(hint) && _manifest != null) hint = _manifest.sourceScene;
                    if (string.IsNullOrEmpty(hint)) hint = "BlenderScene";
                    EditorGUILayout.LabelField("留空则用 Blender 场景名: " + hint, EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space(6);

            // ---------------------------------------------------- 法线格式
            EditorGUILayout.LabelField("④ 法线", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                bool dx5 = EditorGUILayout.ToggleLeft("DXT5 法线", NormalDx5);
                if (dx5 != NormalDx5) NormalDx5 = dx5;
            }

            EditorGUILayout.Space(6);

            // ---------------------------------------------------- 预览
            EditorGUILayout.LabelField("⑤ 将生成", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string dir = PreviewOutDir();
                EditorGUILayout.LabelField("场景  " + dir + "/" + Path.GetFileName(dir) + ".unity",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("FBX    " + dir + "/FBX/",
                    EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.Space(6);

            bool canImport = _manifest != null;

            // ---------------------------------------------------- 只重建材质（白模救急）
            EditorGUILayout.LabelField("⑤.5 只重建材质与贴图（不动场景）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (BeginnerMode)
                    EditorGUILayout.LabelField(
                        "场景里材质球发白的就用这个：重拷 textures/、重写 .mat，场景摆放和 FBX 全都不动。",
                        EditorStyles.wordWrappedMiniLabel);

                using (new EditorGUI.DisabledScope(!canImport))
                {
                    if (GUILayout.Button("只重建材质与贴图", GUILayout.Height(26)))
                        DoRebuildMaterials();
                }
            }

            EditorGUILayout.Space(10);

            // ---------------------------------------------------- 增量更新
            EditorGUILayout.LabelField("⑥ 增量更新（不动场景）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                bool onlySel = EditorGUILayout.ToggleLeft("只处理场景里选中的物体", OnlyUpdateSelected);
                if (onlySel != OnlyUpdateSelected) OnlyUpdateSelected = onlySel;

                if (BeginnerMode)
                {
                    EditorGUILayout.LabelField(
                        OnlyUpdateSelected
                            ? "按选中物体名去清单里查 FBX，只处理这些。"
                            : "比对源文件和目标 FBX 的修改时间，只处理源比目标新的那些。",
                        EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.LabelField(
                        "Mesh 换成新的，实例位置也按清单里的 bPos / bRot / bScale 重摆。",
                        EditorStyles.wordWrappedMiniLabel);
                }

                using (new EditorGUI.DisabledScope(!canImport))
                {
                    if (GUILayout.Button("只更新网格", GUILayout.Height(26)))
                        DoUpdateMeshes();
                }
            }

            EditorGUILayout.Space(10);

            // ---------------------------------------------------- 导入新物体
            EditorGUILayout.LabelField("⑦ 导入新物体（加到当前场景）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (BeginnerMode)
                {
                    EditorGUILayout.LabelField(
                        "Blender 里新建物体 → N 面板「导出选中」→ 回来点这个。只把当前场景还没有的物体加进来，" +
                        "已有物体、相机、灯、摆放全都不动。",
                        EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.LabelField(
                        "按实例名认重，所以同一个清单重复点也不会加两遍。",
                        EditorStyles.wordWrappedMiniLabel);
                }

                using (new EditorGUI.DisabledScope(!canImport))
                {
                    if (GUILayout.Button("导入新物体", GUILayout.Height(26)))
                        DoImportAdded();
                }
            }

            EditorGUILayout.Space(10);

            // ---------------------------------------------------- 主按钮
            using (new EditorGUI.DisabledScope(!canImport))
            {
                var style = new GUIStyle(GUI.skin.button);
                style.fontSize = 14;
                style.fixedHeight = 38;
                if (GUILayout.Button("导入 Blender 场景", style))
                    DoImport();
            }

            if (!canImport)
                EditorGUILayout.HelpBox("没有可导入的数据，先去 Blender 导出。", MessageType.Error);

            EditorGUILayout.EndScrollView();
        }

        void SetFromSelection()
        {
            var obj = Selection.activeObject;
            if (obj == null)
            {
                EditorUtility.DisplayDialog("没有选中", "先在 Project 窗口里选一个文件夹。", "好");
                return;
            }
            string p = AssetDatabase.GetAssetPath(obj);
            if (!AssetDatabase.IsValidFolder(p))
            {
                EditorUtility.DisplayDialog("不是文件夹",
                    "选中的是:\n" + p + "\n\n请在 Project 窗口里选文件夹。", "好");
                return;
            }
            OutRoot = p;
            GUI.FocusControl(null);
            Repaint();
        }

        void BrowseOutRoot()
        {
            string abs = Path.Combine(ProjectRoot,
                OutRoot.Replace('/', Path.DirectorySeparatorChar));
            string picked = EditorUtility.OpenFolderPanel("选择输出根目录（必须在 Assets 下）", abs, "");
            if (string.IsNullOrEmpty(picked)) return;

            string normalized = picked.Replace('\\', '/');
            string root = ProjectRoot.Replace('\\', '/');
            if (!normalized.StartsWith(root + "/Assets", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("只能在项目内",
                    "选的目录不在 Assets 下:\n" + picked, "好");
                return;
            }
            OutRoot = normalized.Substring(root.Length + 1);
            GUI.FocusControl(null);
            Repaint();
        }

        void DoImport()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.isDirty)
            {
                bool ok = EditorUtility.DisplayDialog("当前场景有未保存改动",
                    "导入会新建场景并关闭当前的:\n" + active.name +
                    "\n\n先保存吗？", "先保存再导入", "取消");
                if (!ok) return;
                EditorSceneManager.SaveOpenScenes();
            }

            ImportInto(ResolveSrcRoot(), OutRoot, SceneName, NormalDx5);
            ReloadManifest();
        }

        void DoUpdateMeshes()
        {
            UpdateChangedMeshes(ResolveSrcRoot(), OutRoot, SceneName, OnlyUpdateSelected);
            ReloadManifest();
        }

        /// 只把清单里当前场景还没有的物体加进来，不重建场景
        void DoImportAdded()
        {
            ImportAddedObjects(ResolveSrcRoot(), OutRoot, SceneName, NormalDx5);
            ReloadManifest();
        }

        /// 白模救急：只重做材质和贴图，场景不动
        void DoRebuildMaterials()
        {
            RebuildMaterials(ResolveSrcRoot(), OutRoot, SceneName, NormalDx5);
            ReloadManifest();
        }
    }
}
#endif
