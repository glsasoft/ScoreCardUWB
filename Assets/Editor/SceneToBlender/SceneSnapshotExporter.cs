#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BaChang.Tools
{
    /// <summary>
    /// 把当前 Unity 场景整体导出成 glTF 2.0（.gltf + .bin + textures），供 Blender 直接导入。
    ///
    /// 背景：场景里的零件模型本体都在原点，摆放信息只存在于场景 Transform 上。
    /// 因此这里把每个实例的 localToWorldMatrix 一起写进 glTF 的 node，
    /// 导入 Blender 后每个零件的位置/旋转/缩放与 Unity 完全一致。
    ///
    /// 坐标：Unity 左手 Y-up(+Z 朝前) -> glTF 右手 Y-up(-Z 朝前)
    ///   position/normal: (x, y, -z)
    ///   uv:              (u, 1-v)
    ///   node 矩阵:       C * M * C^-1, C = diag(1,1,-1,1)
    ///
    /// 输出：<项目根>/SceneExport_Blender/
    /// </summary>
    public static class SceneSnapshotExporter
    {
        const string OutFolderName = "SceneExport_Blender";

        // ------------------------------------------------------------ 内部结构

        class Item
        {
            public string group;
            public string name;
            public Mesh mesh;
            public Matrix4x4 world;
            public Material[] mats;
        }

        // ------------------------------------------------------------ 状态

        static MemoryStream _bin;
        static BinaryWriter _bw;
        static readonly List<string> _bufferViews = new List<string>();
        static readonly List<string> _accessors = new List<string>();
        static readonly List<string> _meshes = new List<string>();
        static readonly List<string> _materials = new List<string>();
        static readonly List<string> _textures = new List<string>();
        static readonly List<string> _images = new List<string>();
        static readonly List<string> _nodes = new List<string>();

        static readonly Dictionary<string, int> _meshCache = new Dictionary<string, int>();
        static readonly Dictionary<Material, int> _matCache = new Dictionary<Material, int>();
        static readonly Dictionary<string, int> _texCache = new Dictionary<string, int>();

        static string _outDir;
        static string _texDir;
        static bool _flipWinding;
        static int _windingPos, _windingNeg, _skipped, _failed;
        static readonly List<string> _warnings = new List<string>();

        // ------------------------------------------------------------ 入口

        // 菜单项由 BlenderSceneImporter 的窗口统一承载（Tools/搬迁到 Blender），
        // 这里不再单独挂 MenuItem，避免和窗口入口抢同一个父节点。
        public static void ExportCurrentScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[SceneSnapshot] 没有打开的当前场景。");
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            _outDir = Path.Combine(projectRoot, OutFolderName);
            _texDir = Path.Combine(_outDir, "textures");
            try
            {
                if (Directory.Exists(_texDir)) Directory.Delete(_texDir, true);
                Directory.CreateDirectory(_texDir);
            }
            catch (Exception e)
            {
                Debug.LogError("[SceneSnapshot] 无法创建输出目录: " + _texDir + " / " + e.Message);
                return;
            }

            _bin = new MemoryStream();
            _bw = new BinaryWriter(_bin);
            _bufferViews.Clear(); _accessors.Clear(); _meshes.Clear(); _materials.Clear();
            _textures.Clear(); _images.Clear(); _nodes.Clear();
            _meshCache.Clear(); _matCache.Clear(); _texCache.Clear();
            _windingPos = _windingNeg = _skipped = _failed = 0;
            _warnings.Clear();

            var items = Collect(scene);
            if (items.Count == 0)
            {
                Debug.LogError("[SceneSnapshot] 场景里没有找到可导出的 MeshRenderer。");
                return;
            }

            DetectWinding(items);

            // ---- 分组，保留 Meshes 下的分区名
            var groupOrder = new List<string>();
            var groupItems = new Dictionary<string, List<Item>>();
            foreach (var it in items)
            {
                string g = string.IsNullOrEmpty(it.group) ? "Scene" : it.group;
                if (!groupItems.ContainsKey(g)) { groupItems[g] = new List<Item>(); groupOrder.Add(g); }
                groupItems[g].Add(it);
            }

            var rootNodes = new List<int>();
            foreach (var g in groupOrder)
            {
                var childNodes = new List<int>();
                foreach (var it in groupItems[g])
                {
                    int meshIdx = BuildMesh(it.mesh, it.mats);
                    if (meshIdx < 0) { _failed++; continue; }
                    int nodeIdx = _nodes.Count;
                    _nodes.Add("{\"name\":" + JsonStr(it.name) +
                               ",\"mesh\":" + meshIdx +
                               ",\"matrix\":" + MatrixJson(ToRightHanded(it.world)) + "}");
                    childNodes.Add(nodeIdx);
                }
                int gIdx = _nodes.Count;
                _nodes.Add("{\"name\":" + JsonStr(g) + ",\"children\":[" +
                           JoinInts(childNodes) + "]}");
                rootNodes.Add(gIdx);
            }

            // ---- 写 .bin / .gltf
            _bw.Flush();
            string binName = scene.name + ".bin";
            File.WriteAllBytes(Path.Combine(_outDir, binName), _bin.ToArray());

            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"asset\": {\"version\": \"2.0\", \"generator\": \"BaChang SceneSnapshotExporter\"},\n");
            sb.Append("  \"scene\": 0,\n");
            sb.Append("  \"scenes\": [{\"name\": ").Append(JsonStr(scene.name))
              .Append(", \"nodes\": [").Append(JoinInts(rootNodes)).Append("]}],\n");
            sb.Append("  \"nodes\": [").Append(string.Join(",", _nodes.ToArray())).Append("],\n");
            sb.Append("  \"meshes\": [").Append(string.Join(",", _meshes.ToArray())).Append("],\n");
            sb.Append("  \"materials\": [").Append(string.Join(",", _materials.ToArray())).Append("],\n");
            if (_textures.Count > 0)
                sb.Append("  \"textures\": [").Append(string.Join(",", _textures.ToArray())).Append("],\n");
            if (_images.Count > 0)
                sb.Append("  \"images\": [").Append(string.Join(",", _images.ToArray())).Append("],\n");
            sb.Append("  \"samplers\": [{\"magFilter\": 9729, \"minFilter\": 9987, \"wrapS\": 10497, \"wrapT\": 10497}],\n");
            sb.Append("  \"accessors\": [").Append(string.Join(",", _accessors.ToArray())).Append("],\n");
            sb.Append("  \"bufferViews\": [").Append(string.Join(",", _bufferViews.ToArray())).Append("],\n");
            sb.Append("  \"buffers\": [{\"uri\": ").Append(JsonStr(binName))
              .Append(", \"byteLength\": ").Append(_bin.Length).Append("}],\n");
            sb.Append("  \"extensionsUsed\": [\"KHR_materials_emissive_strength\"]\n");
            sb.Append("}\n");

            string gltfPath = Path.Combine(_outDir, scene.name + ".gltf");
            File.WriteAllText(gltfPath, sb.ToString(), new UTF8Encoding(false));

            // ---- 清单，方便排查
            var manifest = new StringBuilder();
            manifest.Append("{\n");
            manifest.Append("  \"scene\": ").Append(JsonStr(scene.name)).Append(",\n");
            manifest.Append("  \"exported\": ").Append(JsonStr(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))).Append(",\n");
            manifest.Append("  \"instances\": ").Append(items.Count).Append(",\n");
            manifest.Append("  \"nodes\": ").Append(_nodes.Count).Append(",\n");
            manifest.Append("  \"meshes\": ").Append(_meshes.Count).Append(",\n");
            manifest.Append("  \"materials\": ").Append(_materials.Count).Append(",\n");
            manifest.Append("  \"textures\": ").Append(_textures.Count).Append(",\n");
            manifest.Append("  \"failed\": ").Append(_failed).Append(",\n");
            manifest.Append("  \"skippedNonMeshRenderer\": ").Append(_skipped).Append(",\n");
            manifest.Append("  \"flipWinding\": ").Append(_flipWinding ? "true" : "false").Append(",\n");
            manifest.Append("  \"windingPositive\": ").Append(_windingPos).Append(",\n");
            manifest.Append("  \"windingNegative\": ").Append(_windingNeg).Append(",\n");
            manifest.Append("  \"groups\": [");
            for (int i = 0; i < groupOrder.Count; i++)
            {
                if (i > 0) manifest.Append(", ");
                manifest.Append("{\"name\": ").Append(JsonStr(groupOrder[i]))
                        .Append(", \"count\": ").Append(groupItems[groupOrder[i]].Count).Append("}");
            }
            manifest.Append("],\n");
            manifest.Append("  \"warnings\": [");
            for (int i = 0; i < _warnings.Count; i++)
            {
                if (i > 0) manifest.Append(", ");
                manifest.Append(JsonStr(_warnings[i]));
            }
            manifest.Append("]\n}\n");
            File.WriteAllText(Path.Combine(_outDir, "manifest.json"), manifest.ToString(), new UTF8Encoding(false));

            AssetDatabase.Refresh();

            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[SceneSnapshot] {0} 实例 / {1} node / {2} mesh / {3} mat / {4} tex；绕序flip={5} (+{6}/-{7})；失败 {8}；-> {9}",
                items.Count, _nodes.Count, _meshes.Count, _materials.Count, _textures.Count,
                _flipWinding, _windingPos, _windingNeg, _failed, gltfPath));

            Debug.Log("[SceneSnapshot] 输出目录: " + _outDir);
        }

        // ------------------------------------------------------------ 收集

        static List<Item> Collect(Scene scene)
        {
            var items = new List<Item>();
            foreach (var root in scene.GetRootGameObjects())
            {
                bool meshesRoot = root.name == "Meshes";
                foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mf = mr.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) { _skipped++; continue; }
                    string grp;
                    if (meshesRoot)
                    {
                        // 往上爬到 Meshes 的直接子节点，这样 ZhuZi/DiMian 里再嵌套的 LuoShi 之类也能归对组
                        Transform t = mf.transform;
                        while (t.parent != null && t.parent != root.transform) t = t.parent;
                        grp = t.name;
                    }
                    else grp = root.name;

                    items.Add(new Item
                    {
                        group = grp,
                        name = mr.name,
                        mesh = mf.sharedMesh,
                        world = mr.transform.localToWorldMatrix,
                        mats = mr.sharedMaterials
                    });
                }
            }
            return items;
        }

        /// 判断 Unity 三角形绕序：顶点法线与右手叉积同向 => 已是 CCW 正面，无需翻转。
        static void DetectWinding(List<Item> items)
        {
            int checkedTris = 0;
            foreach (var it in items)
            {
                var mesh = it.mesh;
                if (mesh == null || !mesh.isReadable) continue;
                Vector3[] v, n;
                try { v = mesh.vertices; n = mesh.normals; } catch { continue; }
                if (v.Length == 0 || n.Length == 0) continue;

                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    int[] tri;
                    try { tri = mesh.GetTriangles(s); } catch { continue; }
                    if (tri == null || tri.Length < 3) continue;
                    for (int i = 0; i + 2 < tri.Length && i < 30; i += 3)
                    {
                        Vector3 a = v[tri[i]], b = v[tri[i + 1]], c = v[tri[i + 2]];
                        Vector3 fn = Vector3.Cross(b - a, c - a);
                        if (fn.sqrMagnitude < 1e-12f) continue;
                        Vector3 vn = (n[tri[i]] + n[tri[i + 1]] + n[tri[i + 2]]).normalized;
                        if (Vector3.Dot(fn.normalized, vn) >= 0f) _windingPos++;
                        else _windingNeg++;
                        checkedTris++;
                    }
                }
                if (checkedTris > 200) break;
            }
            // 顶点法线与右手叉积同向 => Unity 里是 CCW 正面。
            // 但导出时顶点做了 z 镜像，镜像会让「叉积法线 vs 顶点法线」的符号翻转，
            // 原本 CCW 的三角形进入 glTF 后变成 CW，必须翻转索引才符合 glTF 的 CCW 正面约定。
            _flipWinding = _windingPos > _windingNeg;
        }

        // ------------------------------------------------------------ 几何

        static string MatSignature(Material[] mats)
        {
            if (mats == null) return "-";
            var sb = new StringBuilder();
            foreach (var m in mats) sb.Append(m == null ? "null" : m.GetInstanceID().ToString()).Append('|');
            return sb.ToString();
        }

        static int BuildMesh(Mesh mesh, Material[] mats)
        {
            string key = mesh.GetInstanceID().ToString(CultureInfo.InvariantCulture) + "#" + MatSignature(mats);
            int cached;
            if (_meshCache.TryGetValue(key, out cached)) return cached;

            Vector3[] verts;
            try { verts = mesh.vertices; }
            catch (Exception e)
            {
                _warnings.Add("mesh not readable: " + mesh.name + " (" + e.Message + ")");
                _meshCache[key] = -1;
                return -1;
            }
            if (verts == null || verts.Length == 0)
            {
                _warnings.Add("mesh empty: " + mesh.name);
                _meshCache[key] = -1;
                return -1;
            }

            Vector3[] norms = null;
            try { var t = mesh.normals; if (t != null && t.Length == verts.Length) norms = t; } catch { }
            Vector2[] uvs = null;
            try { var t = mesh.uv; if (t != null && t.Length == verts.Length) uvs = t; } catch { }

            int n = verts.Length;

            var pos = new float[n * 3];
            for (int i = 0; i < n; i++)
            {
                pos[i * 3 + 0] = verts[i].x;
                pos[i * 3 + 1] = verts[i].y;
                pos[i * 3 + 2] = -verts[i].z;
            }
            int posAcc = AddFloatAccessor(pos, 3, true);

            int nrmAcc = -1;
            if (norms != null)
            {
                var na = new float[n * 3];
                for (int i = 0; i < n; i++)
                {
                    na[i * 3 + 0] = norms[i].x;
                    na[i * 3 + 1] = norms[i].y;
                    na[i * 3 + 2] = -norms[i].z;
                }
                nrmAcc = AddFloatAccessor(na, 3, false);
            }

            int uvAcc = -1;
            if (uvs != null)
            {
                var ua = new float[n * 2];
                for (int i = 0; i < n; i++)
                {
                    ua[i * 2 + 0] = uvs[i].x;
                    ua[i * 2 + 1] = 1f - uvs[i].y;
                }
                uvAcc = AddFloatAccessor(ua, 2, false);
            }

            var prims = new List<string>();
            int subCount = mesh.subMeshCount;
            for (int s = 0; s < subCount; s++)
            {
                int[] tri;
                try { tri = mesh.GetTriangles(s); } catch { continue; }
                if (tri == null || tri.Length < 3) continue;

                var idx = new uint[tri.Length];
                if (_flipWinding)
                {
                    for (int i = 0; i + 2 < tri.Length; i += 3)
                    {
                        idx[i + 0] = (uint)tri[i + 0];
                        idx[i + 1] = (uint)tri[i + 2];
                        idx[i + 2] = (uint)tri[i + 1];
                    }
                }
                else
                {
                    for (int i = 0; i < tri.Length; i++) idx[i] = (uint)tri[i];
                }

                int idxAcc = AddUIntAccessor(idx);
                int matIdx = AddMaterial(mats != null && s < mats.Length ? mats[s] : null);

                var attrs = new StringBuilder("{");
                attrs.Append("\"POSITION\":").Append(posAcc);
                if (nrmAcc >= 0) attrs.Append(",\"NORMAL\":").Append(nrmAcc);
                if (uvAcc >= 0) attrs.Append(",\"TEXCOORD_0\":").Append(uvAcc);
                attrs.Append("}");

                var prim = new StringBuilder("{");
                prim.Append("\"attributes\":").Append(attrs);
                prim.Append(",\"indices\":").Append(idxAcc);
                if (matIdx >= 0) prim.Append(",\"material\":").Append(matIdx);
                prim.Append(",\"mode\":4}");
                prims.Add(prim.ToString());
            }

            if (prims.Count == 0) { _meshCache[key] = -1; return -1; }

            int meshIdx = _meshes.Count;
            _meshes.Add("{\"name\":" + JsonStr(mesh.name) + ",\"primitives\":[" +
                        string.Join(",", prims.ToArray()) + "]}");
            _meshCache[key] = meshIdx;
            return meshIdx;
        }

        static int AddFloatAccessor(float[] data, int comps, bool isPosition)
        {
            int offset = (int)_bin.Position;
            for (int i = 0; i < data.Length; i++) _bw.Write(data[i]);
            int len = (int)_bin.Position - offset;

            _bufferViews.Add("{\"buffer\":0,\"byteOffset\":" + offset + ",\"byteLength\":" + len + ",\"target\":34962}");
            int bv = _bufferViews.Count - 1;

            int count = data.Length / comps;
            var sb = new StringBuilder();
            sb.Append("{\"bufferView\":").Append(bv);
            sb.Append(",\"componentType\":5126,\"count\":").Append(count);
            sb.Append(",\"type\":").Append(comps == 3 ? "\"VEC3\"" : comps == 2 ? "\"VEC2\"" : "\"VEC4\"");

            if (isPosition)
            {
                var min = new float[comps];
                var max = new float[comps];
                for (int c = 0; c < comps; c++) { min[c] = float.MaxValue; max[c] = float.MinValue; }
                for (int i = 0; i < count; i++)
                    for (int c = 0; c < comps; c++)
                    {
                        float v = data[i * comps + c];
                        if (v < min[c]) min[c] = v;
                        if (v > max[c]) max[c] = v;
                    }
                sb.Append(",\"min\":[");
                for (int c = 0; c < comps; c++) { if (c > 0) sb.Append(','); sb.Append(F(min[c])); }
                sb.Append("],\"max\":[");
                for (int c = 0; c < comps; c++) { if (c > 0) sb.Append(','); sb.Append(F(max[c])); }
                sb.Append(']');
            }
            sb.Append('}');
            _accessors.Add(sb.ToString());
            return _accessors.Count - 1;
        }

        static int AddUIntAccessor(uint[] data)
        {
            int offset = (int)_bin.Position;
            for (int i = 0; i < data.Length; i++) _bw.Write(data[i]);
            int len = (int)_bin.Position - offset;

            _bufferViews.Add("{\"buffer\":0,\"byteOffset\":" + offset + ",\"byteLength\":" + len + ",\"target\":34963}");
            int bv = _bufferViews.Count - 1;

            _accessors.Add("{\"bufferView\":" + bv +
                           ",\"componentType\":5125,\"count\":" + data.Length +
                           ",\"type\":\"SCALAR\"}");
            return _accessors.Count - 1;
        }

        // ------------------------------------------------------------ 材质

        static int AddMaterial(Material m)
        {
            if (m == null) return -1;
            int cached;
            if (_matCache.TryGetValue(m, out cached)) return cached;

            bool blend = (m.HasProperty("_BlendMode2") && Mathf.Approximately(m.GetFloat("_BlendMode2"), 1f)) ||
                         (m.HasProperty("_Surface") && Mathf.Approximately(m.GetFloat("_Surface"), 1f)) ||
                         (m.HasProperty("_Blend") && Mathf.Approximately(m.GetFloat("_Blend"), 1f));

            var baseColor = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white;
            if (m.HasProperty("_Color")) baseColor = m.GetColor("_Color");

            var baseTex = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : null;
            if (baseTex == null && m.HasProperty("_MainTex")) baseTex = m.GetTexture("_MainTex");

            var normalTex = m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap") : null;
            var emissionTex = m.HasProperty("_EmissionMap") ? m.GetTexture("_EmissionMap") : null;
            var mrTex = m.HasProperty("_MetallicGlossMap") ? m.GetTexture("_MetallicGlossMap") : null;

            float metallic = m.HasProperty("_Metallic") ? m.GetFloat("_Metallic") : 0f;
            float smoothness = m.HasProperty("_Smoothness") ? m.GetFloat("_Smoothness") : 0.5f;
            float bumpScale = m.HasProperty("_BumpScale") ? m.GetFloat("_BumpScale") : 1f;

            var emissionColor = m.HasProperty("_EmissionColor") ? m.GetColor("_EmissionColor") : Color.black;
            bool emissionOn = m.HasProperty("_EmissionToggle") ? m.GetFloat("_EmissionToggle") > 0.5f : true;

            var sb = new StringBuilder();
            sb.Append("{\"name\":").Append(JsonStr(m.name)).Append(",\"pbrMetallicRoughness\":{");

            sb.Append("\"baseColorFactor\":[")
              .Append(F(baseColor.r)).Append(',').Append(F(baseColor.g)).Append(',')
              .Append(F(baseColor.b)).Append(',').Append(F(baseColor.a)).Append(']');

            if (baseTex != null)
            {
                int t = AddTexture(baseTex, TexKind.Color, m.name + "_base");
                if (t >= 0) sb.Append(",\"baseColorTexture\":{\"index\":").Append(t).Append('}');
            }

            // 有 _MetallicGlossMap 时，_Metallic / _Smoothness 只是「用贴图」的开关值（Piu 里恒为 1），
            // glTF 的 factor 是乘在贴图采样上的，所以必须填 1，否则 roughness 会被乘成 0 → 全反射镜面。
            bool useMrTex = mrTex != null;
            sb.Append(",\"metallicFactor\":").Append(F(useMrTex ? 1f : metallic));
            sb.Append(",\"roughnessFactor\":").Append(F(useMrTex ? 1f : 1f - smoothness));

            if (useMrTex)
            {
                int t = AddMetallicRoughnessTexture(mrTex as Texture2D, m.name + "_mr");
                if (t >= 0) sb.Append(",\"metallicRoughnessTexture\":{\"index\":").Append(t).Append('}');
            }

            sb.Append('}');

            if (normalTex != null)
            {
                int t = AddTexture(normalTex, TexKind.Normal, m.name + "_n");
                if (t >= 0)
                    sb.Append(",\"normalTexture\":{\"index\":").Append(t)
                      .Append(",\"scale\":").Append(F(bumpScale)).Append('}');
            }

            var emissive = emissionOn ? emissionTex : null;
            float strength = 1f;
            if (emissionColor.maxColorComponent > 0.0001f)
            {
                if (emissive != null)
                {
                    int t = AddTexture(emissive, TexKind.Color, m.name + "_e");
                    if (t >= 0) sb.Append(",\"emissiveTexture\":{\"index\":").Append(t).Append('}');
                    strength = Mathf.Max(emissionColor.r, Mathf.Max(emissionColor.g, emissionColor.b));
                    float inv = strength > 1f ? 1f / strength : 1f;
                    sb.Append(",\"emissiveFactor\":[")
                      .Append(F(emissionColor.r * inv)).Append(',')
                      .Append(F(emissionColor.g * inv)).Append(',')
                      .Append(F(emissionColor.b * inv)).Append(']');
                }
                else
                {
                    sb.Append(",\"emissiveFactor\":[")
                      .Append(F(Mathf.Min(1f, emissionColor.r))).Append(',')
                      .Append(F(Mathf.Min(1f, emissionColor.g))).Append(',')
                      .Append(F(Mathf.Min(1f, emissionColor.b))).Append(']');
                }
            }

            bool doubleSided = m.HasProperty("_Cull") && Mathf.Approximately(m.GetFloat("_Cull"), 0f);
            if (doubleSided) sb.Append(",\"doubleSided\":true");
            sb.Append(",\"alphaMode\":").Append(blend ? "\"BLEND\"" : "\"OPAQUE\"");

            if (strength > 1.0001f)
            {
                sb.Append(",\"extensions\":{\"KHR_materials_emissive_strength\":{\"emissiveStrength\":")
                  .Append(F(strength)).Append("}}");
            }

            sb.Append('}');

            int idx = _materials.Count;
            _materials.Add(sb.ToString());
            _matCache[m] = idx;
            return idx;
        }

        enum TexKind { Color, Normal, Linear }

        /// 直接把源文件拷进 textures/，颜色最准（不经过任何色彩空间转换）。
        static int AddTexture(Texture tex, TexKind kind, string hint)
        {
            if (tex == null) return -1;
            string srcPath = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(srcPath) || !File.Exists(srcPath))
            {
                _warnings.Add("texture has no source file: " + tex.name);
                return -1;
            }

            string key = srcPath + "|" + kind;
            int cached;
            if (_texCache.TryGetValue(key, out cached)) return cached;

            string fileName = Path.GetFileName(srcPath);
            string dest = Path.Combine(_texDir, fileName);
            int dup = 1;
            while (File.Exists(dest) && !PathsEqual(dest, srcPath))
            {
                dest = Path.Combine(_texDir, Path.GetFileNameWithoutExtension(fileName) +
                                           "_" + dup + Path.GetExtension(fileName));
                dup++;
            }
            try
            {
                if (!File.Exists(dest)) File.Copy(srcPath, dest, true);
            }
            catch (Exception e)
            {
                _warnings.Add("copy texture failed: " + srcPath + " (" + e.Message + ")");
                return -1;
            }

            string uri = "textures/" + Path.GetFileName(dest);
            int imgIdx = _images.Count;
            _images.Add("{\"uri\":" + JsonStr(uri) + ",\"name\":" + JsonStr(tex.name) + "}");

            int texIdx = _textures.Count;
            _textures.Add("{\"sampler\":0,\"source\":" + imgIdx + ",\"name\":" + JsonStr(tex.name) + "}");

            _texCache[key] = texIdx;
            return texIdx;
        }

        /// Unity 的 _MetallicGlossMap: R=metallic, A=smoothness
        /// glTF 的 metallicRoughness:   G=roughness,  B=metallic
        /// 通道不一样，必须重打包。
        static int AddMetallicRoughnessTexture(Texture2D src, string hint)
        {
            if (src == null) return -1;
            string key = "mr|" + AssetDatabase.GetAssetPath(src);
            int cached;
            if (_texCache.TryGetValue(key, out cached)) return cached;

            try
            {
                int w = src.width, h = src.height;
                var pixels = ReadPixelsLinear(src);
                if (pixels == null) return -1;

                var outPx = new Color32[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    var p = pixels[i];
                    byte metallic = p.r;
                    byte rough = (byte)(255 - p.a);
                    outPx[i] = new Color32(255, rough, metallic, 255);
                }

                string fileName = SanitizeFileName(hint) + "_mr.png";
                string dest = Path.Combine(_texDir, fileName);
                WritePng(outPx, w, h, dest);

                int imgIdx = _images.Count;
                _images.Add("{\"uri\":\"textures/" + fileName + "\",\"name\":" + JsonStr(fileName) + "}");
                int texIdx = _textures.Count;
                _textures.Add("{\"sampler\":0,\"source\":" + imgIdx + ",\"name\":" + JsonStr(fileName) + "}");
                _texCache[key] = texIdx;
                return texIdx;
            }
            catch (Exception e)
            {
                _warnings.Add("metallicRoughness repack failed: " + src.name + " (" + e.Message + ")");
                return -1;
            }
        }

        static Color32[] ReadPixelsLinear(Texture src)
        {
            int w = src.width, h = src.height;
            if (w <= 0 || h <= 0) return null;
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var prev = RenderTexture.active;
            try
            {
                Graphics.Blit(src, rt);
                RenderTexture.active = rt;
                var t2 = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
                t2.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                t2.Apply();
                var px = t2.GetPixels32();
                UnityEngine.Object.DestroyImmediate(t2);
                return px;
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        static void WritePng(Color32[] px, int w, int h, string path)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            t.SetPixels32(px);
            t.Apply();
            File.WriteAllBytes(path, t.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(t);
        }

        // ------------------------------------------------------------ 工具

        static Matrix4x4 ToRightHanded(Matrix4x4 m)
        {
            // C * M * C^-1, C = diag(1,1,-1,1)  =>  第 2 行与第 2 列取反（[2,2] 取反两次复原）
            var r = m;
            for (int i = 0; i < 4; i++)
            {
                r[i, 2] = -r[i, 2];
                r[2, i] = -r[2, i];
            }
            return r;
        }

        static string MatrixJson(Matrix4x4 m)
        {
            var sb = new StringBuilder("[");
            int k = 0;
            for (int c = 0; c < 4; c++)
                for (int row = 0; row < 4; row++)
                {
                    if (k++ > 0) sb.Append(',');
                    sb.Append(F(m[row, c]));
                }
            sb.Append(']');
            return sb.ToString();
        }

        static string F(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) v = 0f;
            return v.ToString("0.######", CultureInfo.InvariantCulture);
        }

        static string JoinInts(List<int> list)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++) { if (i > 0) sb.Append(','); sb.Append(list[i]); }
            return sb.ToString();
        }

        static string SanitizeFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "tex";
            var sb = new StringBuilder();
            foreach (char c in s)
                sb.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 ? '_' : c);
            return sb.ToString();
        }

        static bool PathsEqual(string a, string b)
        {
            return string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
        }

        static string JsonStr(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
#endif
