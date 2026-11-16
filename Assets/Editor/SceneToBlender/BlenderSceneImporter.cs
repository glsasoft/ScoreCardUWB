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
            // F = 交换 y/z（Blender Z-up <-> Unity Y-up），det(F) = -1。
            // S = 只翻 X，det(S) = -1。FBX 从右手系导到左手系，顶点 X 已被取反。
            // R90X = 抵消 FBX 模型根自带的 270°X（C 的逆）。
            // 最终 P = F·B·S·R90X，det(P) = +1，位置/尺寸与 Blender 逐位对齐。
            Matrix4x4 flipYZ = Matrix4x4.identity;
            flipYZ.m11 = 0; flipYZ.m12 = 1;
            flipYZ.m21 = 1; flipYZ.m22 = 0;

            Matrix4x4 mirrorX = Matrix4x4.identity;
            mirrorX.m00 = -1f;

            Matrix4x4 rot90X = Matrix4x4.Rotate(Quaternion.Euler(90f, 0f, 0f));

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

                    GameObject prefab;
                    if (!prefabCache.TryGetValue(e.fbx, out prefab))
                    {
                        string assetPath = fbxAssetDir + "/" +
                            Path.GetFileNameWithoutExtension(e.fbx) + ".fbx";
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        prefabCache[e.fbx] = prefab;
                    }
                    if (prefab == null)
                    {
                        failed++;
                        if (missing.Count < 12) missing.Add(e.fbx);
                        continue;
                    }

                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    var child = inst.transform;

                    // 只有真正带分组（且分组名不等于实例名）时才多插一层
                    Transform parentT = rootT;
                    string gname = string.IsNullOrEmpty(e.group) ? null : e.group;
                    if (gname != null && gname != e.name)
                    {
                        Transform groupT;
                        if (!groups.TryGetValue(gname, out groupT))
                        {
                            var g = new GameObject(gname);
                            groupT = g.transform;
                            groupT.SetParent(rootT, false);
                            groups[gname] = groupT;
                            groupCount++;
                        }
                        parentT = groupT;
                    }

                    Vector3 bp = ToVec3(e.bPos);
                    Quaternion bq = ToQuat(e.bRot);
                    Vector3 bs = ToVec3(e.bScale, Vector3.one);
                    Matrix4x4 B = Matrix4x4.TRS(bp, bq, bs);
                    Matrix4x4 P = flipYZ * B * mirrorX * rot90X;

                    // 不建 holder 空节点，把摆放矩阵直接并进 FBX 实例根（层级少一层）。
                    // FBX 根自带的 270°X + 100 倍轴校仍是它的 local 的一部分，靠左乘 P 覆盖。
                    child.SetParent(parentT, false);
                    Matrix4x4 C = Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale);
                    ApplyMatrix(child, P * C);

                    // 材质回填：按 slot 顺序对齐 manifest 里的材质名
                    if (matMap.Count > 0 && e.materials != null)
                    {
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
                    importer.SaveAndReimport();
                    changed++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

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

            foreach (var n in texNames)
            {
                string s = Path.Combine(srcTexAbs, n);
                string d = Path.Combine(dstTexAbs, n);
                if (!File.Exists(s)) continue;
                if (!File.Exists(d) || new FileInfo(s).LastWriteTimeUtc > new FileInfo(d).LastWriteTimeUtc)
                    File.Copy(s, d, true);
            }

            if (texNames.Count > 0)
                AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);

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
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(texAssetDir + "/" + Path.GetFileName(rel));
                }

                var bt = Load(me.baseTex);
                if (bt != null)
                {
                    mat.SetTexture("_BaseMap", bt);
                    mat.SetTexture("_MainTex", bt);
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
                    }
                    else
                    {
                        mat.SetFloat("_NormalMode", 0f);
                    }

                    var rt = Load(me.rmoTex);
                    if (rt != null) mat.SetTexture("_SMO", rt);

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
                        mat.SetTexture("_OcclusionMap", rt);

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
            return map;
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

                    // 先记下内部材质名（多材质模型会有多个槽）
                    var srcMatNames = new List<string>();
                    foreach (var a in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                    {
                        var m = a as Material;
                        if (m != null && !srcMatNames.Contains(m.name)) srcMatNames.Add(m.name);
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
                            mat = AssetDatabase.LoadAssetAtPath<Material>(matAssetDir + "/" + srcName + ".mat");

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
    }
}
#endif
