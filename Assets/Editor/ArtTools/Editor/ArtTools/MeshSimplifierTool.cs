using System.IO;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;

namespace ArtTools
{
    /// <summary>
    /// 基于 UnityMeshSimplifier 插件的 Mesh 简化编辑器工具
    /// 菜单路径: Tools/Mesh Simplifier
    /// </summary>
    public class MeshSimplifierTool : EditorWindow
    {
        #region 字段

        // 源 Mesh
        private Mesh _sourceMesh;

        // 简化质量 (0~1)，1=不简化，0=最大简化
        private float _quality = 0.5f;

        // 保存路径
        private string _saveFolder = "Assets";
        private string _saveName = "";

        // 简化选项
        private bool _preserveBorderEdges = true;
        private bool _preserveUVSeamEdges = false;
        private bool _preserveUVFoldoverEdges = false;
        private bool _preserveSurfaceCurvature = false;
        private bool _enableSmartLink = false;
        private double _agressiveness = 7.0;
        private int _maxIterationCount = 100;

        // 结果信息
        private int _srcTriCount = 0;
        private int _srcVertCount = 0;
        private int _dstTriCount = 0;
        private int _dstVertCount = 0;
        private bool _hasResult = false;

        // 滚动位置
        private Vector2 _scrollPos;

        #endregion

        #region 菜单入口

        [MenuItem("Tools/Mesh Simplifier")]
        public static void OpenWindow()
        {
            var window = GetWindow<MeshSimplifierTool>("Mesh 简化工具");
            window.minSize = new Vector2(680, 380);
            window.Show();
        }

        #endregion

        #region OnGUI

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(8);

            // ---- 源 Mesh 选择 ----
            GUILayout.Label("1. 选择目标网格", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _sourceMesh = (Mesh)EditorGUILayout.ObjectField("源 Mesh", _sourceMesh, typeof(Mesh), false);
            if (EditorGUI.EndChangeCheck())
            {
                _hasResult = false;
                if (_sourceMesh != null)
                {
                    _srcTriCount = _sourceMesh.triangles.Length / 3;
                    _srcVertCount = _sourceMesh.vertexCount;
                    // 默认保存目录
                    if (_saveFolder == "Assets")
                        _saveFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_sourceMesh)).Replace('\\', '/');
                    // 默认保存名
                    if (string.IsNullOrEmpty(_saveName))
                        _saveName = _sourceMesh.name + "_Simplified";
                }
            }

            if (_sourceMesh != null)
            {
                EditorGUILayout.LabelField("顶点数", _srcVertCount.ToString());
                EditorGUILayout.LabelField("三角面数", _srcTriCount.ToString());
            }

            EditorGUILayout.Space(8);

            // ---- 简化参数 ----
            GUILayout.Label("2. 调整简化参数", EditorStyles.boldLabel);

            _quality = EditorGUILayout.Slider(
                new GUIContent("简化质量 (Quality)", "1=不简化, 0=最大简化\n值越小, 三角面越少"),
                _quality, 0f, 1f);

            // 预估简化后面数
            if (_sourceMesh != null)
            {
                int estimatedTris = Mathf.RoundToInt(_srcTriCount * _quality);
                EditorGUILayout.LabelField("预估简化后三角面数", estimatedTris.ToString());
            }

            EditorGUILayout.Space(4);

            _preserveBorderEdges = EditorGUILayout.Toggle(
                new GUIContent("保留边界边", "PreserveBorderEdges\n保持网格开口边界不变"),
                _preserveBorderEdges);

            _preserveUVSeamEdges = EditorGUILayout.Toggle(
                new GUIContent("保留 UV 接缝", "PreserveUVSeamEdges\n保持 UV 接缝边不被合并"),
                _preserveUVSeamEdges);

            _preserveUVFoldoverEdges = EditorGUILayout.Toggle(
                new GUIContent("保留 UV 折叠", "PreserveUVFoldoverEdges\n保持 UV 折叠边不被合并"),
                _preserveUVFoldoverEdges);

            _preserveSurfaceCurvature = EditorGUILayout.Toggle(
                new GUIContent("保留表面曲率", "PreserveSurfaceCurvature\n考虑曲率, 质量更高但更慢"),
                _preserveSurfaceCurvature);

            _enableSmartLink = EditorGUILayout.Toggle(
                new GUIContent("启用智能链接", "EnableSmartLink\n智能合并相同位置顶点, 减少伪影"),
                _enableSmartLink);

            _agressiveness = EditorGUILayout.DoubleField(
                new GUIContent("激进程度", "Agressiveness\n值越大质量越高但更慢, 推荐 7"),
                _agressiveness);

            _maxIterationCount = EditorGUILayout.IntField(
                new GUIContent("最大迭代次数", "MaxIterationCount\n值越大越接近目标质量但更慢"),
                _maxIterationCount);

            EditorGUILayout.Space(8);

            // ---- 保存位置 ----
            GUILayout.Label("3. 选择保存位置", EditorStyles.boldLabel);

            // 文件夹选择
            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField("保存目录", _saveFolder);
            if (GUILayout.Button("浏览...", GUILayout.Width(70)))
            {
                string selected = EditorUtility.OpenFolderPanel("选择保存目录", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    // 转为相对路径
                    string projectPath = Application.dataPath.Replace('/', Path.DirectorySeparatorChar);
                    projectPath = projectPath.Substring(0, projectPath.Length - "Assets".Length);
                    if (selected.StartsWith(projectPath))
                    {
                        _saveFolder = selected.Substring(projectPath.Length).Replace('\\', '/');
                    }
                    else
                    {
                        // 如果选了项目外的目录, 提示一下
                        Debug.LogWarning("保存目录应在项目 Assets 目录内, 已自动回退到 Assets");
                        _saveFolder = "Assets";
                    }
                }
            }
            GUILayout.EndHorizontal();

            _saveName = EditorGUILayout.TextField("文件名 (不含扩展名)", _saveName);

            // 预览完整路径
            if (!string.IsNullOrEmpty(_saveName))
            {
                string fullPath = Path.Combine(_saveFolder, _saveName + ".mesh").Replace('\\', '/');
                EditorGUILayout.LabelField("完整路径", fullPath);
            }

            EditorGUILayout.Space(8);

            // ---- 简化按钮 ----
            EditorGUI.BeginDisabledGroup(_sourceMesh == null || string.IsNullOrEmpty(_saveName));
            {
                if (GUILayout.Button("执行简化并保存", GUILayout.Height(36)))
                {
                    DoSimplify();
                }
            }
            EditorGUI.EndDisabledGroup();

            // ---- 结果展示 ----
            if (_hasResult)
            {
                EditorGUILayout.Space(8);
                GUILayout.Label("简化结果", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("原始顶点数", _srcVertCount.ToString());
                EditorGUILayout.LabelField("简化后顶点数", _dstVertCount.ToString());
                EditorGUILayout.LabelField("原始三角面数", _srcTriCount.ToString());
                EditorGUILayout.LabelField("简化后三角面数", _dstTriCount.ToString());
                float reduction = _srcTriCount > 0 ? (1f - (float)_dstTriCount / _srcTriCount) * 100f : 0f;
                EditorGUILayout.LabelField("面数减少", string.Format("{0:F1}%", reduction));
            }

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region 核心逻辑

        /// <summary>
        /// 执行 Mesh 简化并保存为 Asset
        /// </summary>
        private void DoSimplify()
        {
            if (_sourceMesh == null)
            {
                EditorUtility.DisplayDialog("提示", "请先选择源 Mesh", "确定");
                return;
            }

            if (string.IsNullOrEmpty(_saveName))
            {
                EditorUtility.DisplayDialog("提示", "请输入保存文件名", "确定");
                return;
            }

            // 确保目录存在
            if (!AssetDatabase.IsValidFolder(_saveFolder))
            {
                EditorUtility.DisplayDialog("提示", "保存目录不存在: " + _saveFolder, "确定");
                return;
            }

            EditorUtility.DisplayProgressBar("Mesh 简化", "正在简化网格...", 0.3f);

            try
            {
                // 创建简化器并设置参数
                var simplifier = new MeshSimplifier(_sourceMesh);
                simplifier.SimplificationOptions = new SimplificationOptions
                {
                    PreserveBorderEdges = _preserveBorderEdges,
                    PreserveUVSeamEdges = _preserveUVSeamEdges,
                    PreserveUVFoldoverEdges = _preserveUVFoldoverEdges,
                    PreserveSurfaceCurvature = _preserveSurfaceCurvature,
                    EnableSmartLink = _enableSmartLink,
                    VertexLinkDistance = double.Epsilon,
                    MaxIterationCount = _maxIterationCount,
                    Agressiveness = _agressiveness,
                    ManualUVComponentCount = false,
                    UVComponentCount = 2
                };

                // 执行简化
                simplifier.SimplifyMesh(_quality);

                // 获取结果 Mesh
                Mesh resultMesh = simplifier.ToMesh();
                resultMesh.name = _saveName;

                EditorUtility.DisplayProgressBar("Mesh 简化", "正在保存资源...", 0.7f);

                // 保存为 Asset
                string assetPath = Path.Combine(_saveFolder, _saveName + ".mesh").Replace('\\', '/');

                AssetDatabase.CreateAsset(resultMesh, assetPath);

                AssetDatabase.Refresh();

                // 记录结果
                _dstTriCount = resultMesh.triangles.Length / 3;
                _dstVertCount = resultMesh.vertexCount;
                _hasResult = true;

                EditorUtility.ClearProgressBar();

                // 选中生成的资源
                var savedAsset = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                EditorGUIUtility.PingObject(savedAsset);

                Debug.Log(string.Format(
                    "[MeshSimplifierTool] 简化完成: {0} -> 顶点 {1}=>{2}, 三角面 {3}=>{4}, 保存至 {5}",
                    _sourceMesh.name, _srcVertCount, _dstVertCount, _srcTriCount, _dstTriCount, assetPath));
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", "简化过程中发生错误:\n" + e.Message, "确定");
                Debug.LogError("[MeshSimplifierTool] 简化失败: " + e);
            }
        }

        #endregion
    }
}
