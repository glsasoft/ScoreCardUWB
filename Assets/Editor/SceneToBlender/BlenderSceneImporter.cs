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

        const string DefaultSrcFolder = "SceneExport_Unity";
        const string PointerFile = "SceneToUnity.path";
        const string DefaultOutRoot = "Assets/SceneFromBlender";

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

        /// 真正干活：把 srcRoot 的内容重建到 outAssetDir/<sceneName>
        public static void ImportInto(string srcRoot, string outAssetRoot, string sceneName)
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
            var matMap = BuildMaterials(manifest, outAssetDir, srcRoot, ref matBuilt);

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

                    Transform groupT;
                    string gname = string.IsNullOrEmpty(e.group) ? "Imported" : e.group;
                    if (!groups.TryGetValue(gname, out groupT))
                    {
                        var g = new GameObject(gname);
                        groupT = g.transform;
                        groups[gname] = groupT;
                    }

                    var holder = new GameObject(e.name);
                    holder.transform.SetParent(groupT, false);
                    child.SetParent(holder.transform, false);

                    Vector3 bp = ToVec3(e.bPos);
                    Quaternion bq = ToQuat(e.bRot);
                    Vector3 bs = ToVec3(e.bScale, Vector3.one);
                    Matrix4x4 B = Matrix4x4.TRS(bp, bq, bs);
                    ApplyMatrix(holder.transform, flipYZ * B * mirrorX * rot90X);

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

                    placed++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // ---- 4. 保存
            EditorSceneManager.SaveScene(scene, sceneAssetPath);
            AssetDatabase.Refresh();

            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[BlenderSceneImporter] {0} 个实例 / 摆放 {1} / 失败 {2} / 分组 {3} / 新拷 FBX {4}\n  scene: {5}\n  fbx  : {6}",
                manifest.instances.Length, placed, failed, groups.Count, copied, sceneAssetPath, fbxAssetDir));

            if (missing.Count > 0)
                Debug.LogWarning("[BlenderSceneImporter] 缺 FBX: " + string.Join(", ", missing.ToArray()));

            if (!quiet) EditorUtility.DisplayDialog("导入完成",
                "实例 " + placed + " / " + manifest.instances.Length +
                "\n失败 " + failed + "   分组 " + groups.Count + "   新拷 FBX " + copied +
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

        /// Blender 四元数是 (w,x,y,z)，Unity 是 (x,y,z,w)
        static Quaternion ToQuat(float[] a)
        {
            if (a == null || a.Length < 4) return Quaternion.identity;
            return new Quaternion(a[1], a[2], a[3], a[0]);
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

        /// 建 .mat 资产，返回 {材质名: Material}。
        /// 贴图从 Blender 导出的 textures/ 拷进 <场景目录>/Textures/，再按 base/normal/rmo 绑上。
        static Dictionary<string, Material> BuildMaterials(Manifest manifest, string outAssetDir,
                                                           string srcRoot, ref int built)
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

            var shader = Shader.Find("GF2/Scene/PBR_EnvStandard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            bool gf2 = shader != null && shader.name == "GF2/Scene/PBR_EnvStandard";
            if (!gf2)
                Debug.LogWarning("[BlenderSceneImporter] 没找到 GF2/Scene/PBR_EnvStandard，退回 URP/Lit，RMO 会绑不准。");

            var usedNames = new HashSet<string>();
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

                if (gf2)
                {
                    // GF2/Scene/PBR_EnvStandard
                    //   _BaseMap   Albedo
                    //   _NRMTex    法线原始通道（_NormalMode 决定怎么解）
                    //   _SMO       R=Smoothness G=Metallic B=Occlusion A=EmissionMask
                    var nt = Load(me.normalTex);
                    if (nt != null)
                    {
                        mat.SetTexture("_NRMTex", nt);
                        mat.SetFloat("_NormalMode", me.normalMode > 0 ? 1f : 0f);
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

            // ---------------------------------------------------- 反向导出
            EditorGUILayout.LabelField("⓪ 反向：Unity → Blender（glTF）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
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

                if (!AssetDatabase.IsValidFolder(OutRoot))
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

                string hint = SceneName;
                if (string.IsNullOrEmpty(hint) && _manifest != null) hint = _manifest.sourceScene;
                if (string.IsNullOrEmpty(hint)) hint = "BlenderScene";
                EditorGUILayout.LabelField("留空则用 Blender 场景名: " + hint, EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(6);

            // ---------------------------------------------------- 预览
            EditorGUILayout.LabelField("④ 将生成", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string dir = PreviewOutDir();
                EditorGUILayout.LabelField("场景  " + dir + "/" + Path.GetFileName(dir) + ".unity",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("FBX    " + dir + "/FBX/",
                    EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.Space(10);

            // ---------------------------------------------------- 主按钮
            bool canImport = _manifest != null;
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

            ImportInto(ResolveSrcRoot(), OutRoot, SceneName);
            ReloadManifest();
        }
    }
}
#endif
