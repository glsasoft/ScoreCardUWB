using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GLSATools
{
    /// <summary>
    /// 沿目标对象的包围盒矩形或网格真实轮廓折线，均匀复制模板对象摆一圈。
    /// </summary>
    public class WallRingTool : EditorWindow
    {
        public enum RingMode
        {
            BoundingBox,   // 沿世界包围盒的矩形四条边
            MeshOutline    // 沿网格边界边还原的真实折线
        }

        public enum FacingMode
        {
            Outward,
            Inward
        }

        [SerializeField] private GameObject m_Target;
        [SerializeField] private GameObject m_Template;
        [SerializeField] private RingMode m_Mode = RingMode.BoundingBox;
        [SerializeField] private FacingMode m_Facing = FacingMode.Outward;
        [SerializeField] private float m_Spacing = 1.5f;
        [SerializeField] private float m_HeightOffset = 0f;
        [SerializeField] private float m_ExtraYaw = 0f;
        [SerializeField] private string m_ContainerName = "WallRing";
        [SerializeField] private bool m_AutoClear = true;
        [SerializeField] private bool m_FollowTargetParent = true;

        private Vector2 m_Scroll;

        [MenuItem("Tools/特效工具/围一圈摆放 (Wall Ring)")]
        public static void Open()
        {
            var win = GetWindow<WallRingTool>(false, "Wall Ring", true);
            win.minSize = new Vector2(380f, 420f);
            win.Show();
        }

        private void OnGUI()
        {
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("来源", EditorStyles.boldLabel);
            m_Target = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("目标", "轮廓来源对象，取它子级所有 Renderer 或 MeshFilter"), m_Target, typeof(GameObject), true);
            m_Template = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("模板", "要被复制的对象，prefab 资产或场景对象都行"), m_Template, typeof(GameObject), true);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("排布", EditorStyles.boldLabel);
            m_Mode = (RingMode)EditorGUILayout.EnumPopup(
                new GUIContent("轮廓模式", "BoundingBox 用包围盒矩形，MeshOutline 用网格真实折线"), m_Mode);
            m_Spacing = Mathf.Max(0.05f, EditorGUILayout.FloatField(
                new GUIContent("间隔距离", "沿轮廓弧长均匀分配，单位米"), m_Spacing));
            m_Facing = (FacingMode)EditorGUILayout.EnumPopup(
                new GUIContent("朝向", "Outward 朝外，Inward 朝内"), m_Facing);
            m_HeightOffset = EditorGUILayout.FloatField(new GUIContent("高度偏移", "Y 轴抬升"), m_HeightOffset);
            m_ExtraYaw = EditorGUILayout.FloatField(new GUIContent("额外朝向偏移", "在法线朝向上再转的角度"), m_ExtraYaw);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("输出", EditorStyles.boldLabel);
            m_ContainerName = EditorGUILayout.TextField(new GUIContent("容器名字", "生成物会收进这个父级"), m_ContainerName);
            m_AutoClear = EditorGUILayout.Toggle(new GUIContent("生成前清空旧容器", "同名且同父级的容器会被删掉"), m_AutoClear);
            m_FollowTargetParent = EditorGUILayout.Toggle(new GUIContent("挂到目标父级下", "关闭则留在场景根目录"), m_FollowTargetParent);

            EditorGUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("生成一圈", GUILayout.Height(30)))
                {
                    Generate();
                }
                if (GUILayout.Button("清除", GUILayout.Height(30), GUILayout.Width(90)))
                {
                    ClearContainer();
                }
            }

            EditorGUILayout.Space(8);
            if (m_Target != null)
            {
                int rendererCount = 0;
                var rs = m_Target.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rs.Length; i++)
                {
                    if (rs[i] != null && rs[i].bounds.size.sqrMagnitude > 0.0001f) rendererCount++;
                }
                EditorGUILayout.HelpBox("目标子级有效 Renderer: " + rendererCount + " 个", MessageType.None);
            }

            EditorGUILayout.EndScrollView();
        }

        private void Generate()
        {
            if (m_Target == null)
            {
                EditorUtility.DisplayDialog("Wall Ring", "先选一个目标对象。", "好");
                return;
            }
            if (m_Template == null)
            {
                EditorUtility.DisplayDialog("Wall Ring", "先选一个模板对象。", "好");
                return;
            }

            List<Vector3> poly = m_Mode == RingMode.BoundingBox ? BuildBoundingBox(m_Target) : BuildOutline(m_Target);
            if (poly == null || poly.Count < 3)
            {
                EditorUtility.DisplayDialog("Wall Ring",
                    m_Mode == RingMode.BoundingBox
                        ? "目标上没有有效的 Renderer，取不到包围盒。"
                        : "取不到可用的闭合轮廓，换个模式试试，或者检查网格是否是封闭边界。", "好");
                return;
            }

            float total = PolylineLength(poly);
            if (total < 0.01f)
            {
                EditorUtility.DisplayDialog("Wall Ring", "轮廓长度接近 0，检查一下目标尺寸。", "好");
                return;
            }

            int count = Mathf.Max(3, Mathf.RoundToInt(total / Mathf.Max(0.05f, m_Spacing)));
            float step = total / count;

            Transform owner = m_FollowTargetParent ? m_Target.transform.parent : null;

            if (m_AutoClear) ClearContainer();

            var root = new GameObject(m_ContainerName);
            Undo.RegisterCreatedObjectUndo(root, "Wall Ring");
            if (owner != null)
            {
                root.transform.SetParent(owner, false);
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;
            }
            else
            {
                root.transform.position = Vector3.zero;
                root.transform.rotation = Quaternion.identity;
            }

            Vector3 center = Vector3.zero;
            for (int i = 0; i < poly.Count; i++) center += poly[i];
            center /= poly.Count;

            bool templateIsAsset = !m_Template.scene.IsValid();
            int made = 0;

            float dist = step * 0.5f;
            for (int k = 0; k < count; k++)
            {
                Vector3 pos;
                Vector3 normal;
                if (!SampleAt(poly, center, dist, out pos, out normal)) break;

                if (m_Facing == FacingMode.Inward) normal = -normal;
                float yaw = Mathf.Atan2(normal.x, normal.z) * Mathf.Rad2Deg + m_ExtraYaw;

                GameObject inst;
                if (templateIsAsset)
                {
                    inst = (GameObject)PrefabUtility.InstantiatePrefab(m_Template, root.transform);
                }
                else
                {
                    inst = Instantiate(m_Template, root.transform);
                    inst.name = m_Template.name + "_" + made.ToString("00");
                }

                Undo.RegisterCreatedObjectUndo(inst, "Wall Ring Item");
                inst.transform.position = new Vector3(pos.x, pos.y + m_HeightOffset, pos.z);
                inst.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                inst.transform.localScale = Vector3.one;
                made++;
                dist += step;
            }

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(m_Target.scene);
            Debug.Log("[Wall Ring] 模式=" + m_Mode + " 间距=" + m_Spacing.ToString("F2") +
                      " 实际间距=" + step.ToString("F2") + " 总长=" + total.ToString("F2") +
                      " 生成=" + made + " 个 -> " + root.name);
        }

        private void ClearContainer()
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            var kills = new List<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                var go = all[i];
                if (go == null || !go.scene.IsValid()) continue;
                if (go.name != m_ContainerName) continue;
                kills.Add(go);
            }
            for (int i = 0; i < kills.Count; i++)
            {
                Undo.DestroyObjectImmediate(kills[i]);
            }
            if (kills.Count > 0) Debug.Log("[Wall Ring] 清掉了 " + kills.Count + " 个旧容器: " + m_ContainerName);
        }

        // ---------- 轮廓构建 ----------

        private List<Vector3> BuildBoundingBox(GameObject target)
        {
            var rs = target.GetComponentsInChildren<Renderer>(true);
            bool has = false;
            Bounds b = new Bounds();
            for (int i = 0; i < rs.Length; i++)
            {
                if (rs[i] == null) continue;
                if (rs[i].bounds.size.sqrMagnitude < 0.0001f) continue;
                if (!has) { b = rs[i].bounds; has = true; }
                else b.Encapsulate(rs[i].bounds);
            }
            if (!has) return null;

            float minX = b.min.x, maxX = b.max.x;
            float minZ = b.min.z, maxZ = b.max.z;
            float y = b.min.y;

            var list = new List<Vector3>(4);
            list.Add(new Vector3(minX, y, minZ));
            list.Add(new Vector3(maxX, y, minZ));
            list.Add(new Vector3(maxX, y, maxZ));
            list.Add(new Vector3(minX, y, maxZ));
            return list;
        }

        private List<Vector3> BuildOutline(GameObject target)
        {
            var mf = target.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return null;

            var mesh = mf.sharedMesh;
            var tf = mf.transform;

            var verts = new List<Vector3>();
            mesh.GetVertices(verts);
            if (verts.Count == 0) return null;

            var tris = mesh.GetIndices(0);
            if (tris == null || tris.Length < 3) return null;

            var world = new Vector3[verts.Count];
            for (int i = 0; i < verts.Count; i++) world[i] = tf.TransformPoint(verts[i]);

            var edgeCount = new Dictionary<long, int>();
            for (int i = 0; i < tris.Length; i += 3)
            {
                int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                AddEdge(edgeCount, a, b);
                AddEdge(edgeCount, b, c);
                AddEdge(edgeCount, c, a);
            }

            var adj = new Dictionary<int, List<int>>();
            foreach (var kv in edgeCount)
            {
                if (kv.Value != 1) continue; // 只保留只被一个三角形用到的边
                int a = (int)(kv.Key / 100000L);
                int b = (int)(kv.Key % 100000L);
                Link(adj, a, b);
                Link(adj, b, a);
            }
            if (adj.Count < 3) return null;

            var used = new HashSet<long>();
            List<int> best = null;
            float bestLen = -1f;

            foreach (var startKv in adj)
            {
                var neighbours = startKv.Value;
                for (int ni = 0; ni < neighbours.Count; ni++)
                {
                    int nb = neighbours[ni];
                    if (used.Contains(EdgeKey(startKv.Key, nb))) continue;

                    var ring = new List<int>();
                    int cur = startKv.Key;
                    int prev = -1;
                    for (int guard = 0; guard < 10000; guard++)
                    {
                        ring.Add(cur);
                        int next = -1;
                        var list = adj[cur];
                        for (int i = 0; i < list.Count; i++)
                        {
                            int cand = list[i];
                            if (cand == prev) continue;
                            if (used.Contains(EdgeKey(cur, cand))) continue;
                            next = cand;
                            break;
                        }
                        if (next < 0) break;
                        used.Add(EdgeKey(cur, next));
                        prev = cur;
                        cur = next;
                        if (cur == startKv.Key) break;
                    }

                    if (ring.Count >= 3)
                    {
                        float len = 0f;
                        for (int i = 0; i < ring.Count; i++)
                        {
                            len += Vector3.Distance(world[ring[i]], world[ring[(i + 1) % ring.Count]]);
                        }
                        if (len > bestLen)
                        {
                            bestLen = len;
                            best = ring;
                        }
                    }
                }
            }

            if (best == null || best.Count < 3) return null;

            var poly = new List<Vector3>(best.Count);
            for (int i = 0; i < best.Count; i++) poly.Add(world[best[i]]);
            return poly;
        }

        private static void AddEdge(Dictionary<long, int> map, int a, int b)
        {
            long key = EdgeKey(a, b);
            int c;
            map.TryGetValue(key, out c);
            map[key] = c + 1;
        }

        private static long EdgeKey(int a, int b)
        {
            int lo = a < b ? a : b;
            int hi = a < b ? b : a;
            return (long)lo * 100000L + hi;
        }

        private static void Link(Dictionary<int, List<int>> map, int a, int b)
        {
            List<int> list;
            if (!map.TryGetValue(a, out list))
            {
                list = new List<int>();
                map[a] = list;
            }
            list.Add(b);
        }

        // ---------- 采样 ----------

        private static float PolylineLength(List<Vector3> poly)
        {
            float len = 0f;
            for (int i = 0; i < poly.Count; i++)
            {
                len += Vector3.Distance(poly[i], poly[(i + 1) % poly.Count]);
            }
            return len;
        }

        private static bool SampleAt(List<Vector3> poly, Vector3 center, float distance, out Vector3 pos, out Vector3 normal)
        {
            pos = Vector3.zero;
            normal = Vector3.forward;

            float acc = 0f;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector3 a = poly[i];
                Vector3 b = poly[(i + 1) % poly.Count];
                float seg = Vector3.Distance(a, b);
                if (seg < 0.0001f) continue;

                if (distance >= acc && distance <= acc + seg)
                {
                    float t = (distance - acc) / seg;
                    pos = Vector3.Lerp(a, b, t);
                    Vector3 dir = (b - a).normalized;
                    Vector3 n = new Vector3(-dir.z, 0f, dir.x);
                    Vector3 mid = (a + b) * 0.5f;
                    if (Vector3.Dot(n, mid - center) < 0f) n = -n;
                    normal = n;
                    return true;
                }
                acc += seg;
            }
            return false;
        }
    }
}
