using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.SceneManagement;



namespace ArtTools
{
    public class MissingReferenceChecker : EditorWindow
    {
        // 要检查的目录列表
        private List<string> directoriesToCheck = new List<string>();
        private string newDirectory = "Assets";
        private Vector2 scrollPosition;
        private bool checkMaterials = true;
        private bool checkPrefabs = true;
        private bool checkScenes = true;
        private bool checkScriptableObjects = true;

        // 检查结果
        private CheckResult checkResult = new CheckResult();

        // 用于折叠显示的状态
        private Dictionary<string, bool> categoryFoldouts = new Dictionary<string, bool>();
        private bool showAllFoldouts = true;

        // 用于跟踪每个资源的折叠状态
        private Dictionary<string, Dictionary<string, bool>> assetFoldouts =
            new Dictionary<string, Dictionary<string, bool>>();

        [MenuItem("工具/资源引用丢失查询/资源引用丢失查询")]
        public static void ShowWindow()
        {
            GetWindow<MissingReferenceChecker>("引用丢失检查器");
        }

        private void OnGUI()
        {
            GUILayout.Label("资源引用丢失检查器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 检查选项
            EditorGUILayout.LabelField("检查选项", EditorStyles.boldLabel);
            checkMaterials = EditorGUILayout.Toggle("检查材质球贴图引用", checkMaterials);
            checkPrefabs = EditorGUILayout.Toggle("检查预制体引用", checkPrefabs);
            checkScenes = EditorGUILayout.Toggle("检查场景引用", checkScenes);
            checkScriptableObjects = EditorGUILayout.Toggle("检查ScriptableObject引用", checkScriptableObjects);

            // 场景配置选项
            if (checkScenes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("场景自动检查:", GUILayout.Width(100));
                MissingReferenceSceneConfig.Instance.EnableAutoCheck =
                    EditorGUILayout.Toggle(MissingReferenceSceneConfig.Instance.EnableAutoCheck);

                if (GUILayout.Button("场景配置", GUILayout.Width(80)))
                {
                    MissingReferenceSceneConfigEditor.ShowWindow();
                }

                EditorGUILayout.EndHorizontal();

                // 只检查列表内场景的开关
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("只检查列表内场景:", GUILayout.Width(100));
                MissingReferenceSceneConfig.Instance.OnlyCheckListedScenes =
                    EditorGUILayout.Toggle(MissingReferenceSceneConfig.Instance
                        .OnlyCheckListedScenes);
                EditorGUILayout.EndHorizontal();

                if (MissingReferenceSceneConfig.Instance.EnableAutoCheck)
                {
                    EditorGUILayout.HelpBox("已启用场景自动检查，配置列表中的场景将不会显示询问对话框。", MessageType.Info);
                }

                if (MissingReferenceSceneConfig.Instance.OnlyCheckListedScenes)
                {
                    EditorGUILayout.HelpBox("已启用只检查列表内场景，不在列表中的场景将被跳过。", MessageType.Info);

                    // 检查场景列表是否为空
                    if (MissingReferenceSceneConfig.Instance.ScenesToCheckWithoutPrompt.Count ==
                        0)
                    {
                        EditorGUILayout.HelpBox("警告：您已启用只检查列表内场景，但场景列表为空！请添加场景到列表中，否则将无法检查任何场景。", MessageType.Warning);
                    }
                }

                // 检查是否启用了自动检查但场景列表为空
                if (MissingReferenceSceneConfig.Instance.EnableAutoCheck &&
                    MissingReferenceSceneConfig.Instance.ScenesToCheckWithoutPrompt.Count == 0)
                {
                    EditorGUILayout.HelpBox("警告：您已启用自动检查功能，但场景列表为空！请添加场景到列表中，以便自动检查。", MessageType.Warning);
                }
            }

            EditorGUILayout.Space();

            // 目录选择
            EditorGUILayout.LabelField("要检查的目录", EditorStyles.boldLabel);

            // 显示已添加的目录
            for (int i = 0; i < directoriesToCheck.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(directoriesToCheck[i]);
                if (GUILayout.Button("移除", GUILayout.Width(60)))
                {
                    directoriesToCheck.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            // 添加新目录
            EditorGUILayout.BeginHorizontal();
            newDirectory = EditorGUILayout.TextField(newDirectory);
            if (GUILayout.Button("添加目录", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(newDirectory) && !directoriesToCheck.Contains(newDirectory))
                {
                    directoriesToCheck.Add(newDirectory);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("选择文件夹"))
            {
                string path = EditorUtility.OpenFolderPanel("选择要检查的文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // 转换为相对于Assets的路径
                    string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                    if (!directoriesToCheck.Contains(relativePath))
                    {
                        directoriesToCheck.Add(relativePath);
                    }
                }
            }

            EditorGUILayout.Space();

            // 开始检查按钮
            if (GUILayout.Button("开始检查"))
            {
                if (directoriesToCheck.Count > 0)
                {
                    checkResult.Clear();
                    checkResult.IsCheckComplete = false;
                    CheckMissingReferences();
                    checkResult.IsCheckComplete = true;
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请至少添加一个要检查的目录", "确定");
                }
            }

            EditorGUILayout.Space();

            // 显示检查结果
            if (checkResult.IsCheckComplete)
            {
                DisplayResults();

                // 导出结果按钮
                if (GUILayout.Button("导出结果"))
                {
                    ExportResults();
                }
            }
        }

        private void CheckMissingReferences()
        {
            // 获取所有要检查的资源
            List<string> assetsToCheck = new List<string>();

            foreach (string directory in directoriesToCheck)
            {
                if (AssetDatabase.IsValidFolder(directory))
                {
                    string[] assets = AssetDatabase.FindAssets("", new[] { directory });
                    foreach (string guid in assets)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        assetsToCheck.Add(assetPath);
                    }
                }
            }

            // 检查每个资源
            int totalAssets = assetsToCheck.Count;
            for (int i = 0; i < totalAssets; i++)
            {
                string assetPath = assetsToCheck[i];
                EditorUtility.DisplayProgressBar("检查引用丢失", $"正在检查: {assetPath}", (float)i / totalAssets);

                // 根据资源类型进行不同的检查
                string extension = Path.GetExtension(assetPath).ToLower();

                // 检查材质球
                if (checkMaterials && extension == ".mat")
                {
                    MissingReferenceChecker_Material.CheckMaterialReferences(assetPath, checkResult);
                }

                // 检查预制体
                if (checkPrefabs && extension == ".prefab")
                {
                    MissingReferenceChecker_Prefab.CheckPrefabReferences(assetPath, checkResult);
                }

                // 检查场景
                if (checkScenes && extension == ".unity")
                {
                    MissingReferenceChecker_Scene.CheckSceneReferences(assetPath, checkResult);
                }

                // 检查ScriptableObject
                if (checkScriptableObjects && extension == ".asset")
                {
                    MissingReferenceChecker_ScriptableObject.CheckScriptableObjectReferences(assetPath, checkResult);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private void CheckMaterialReferences(string materialPath)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) return;

            // 获取材质球的所有属性
            Shader shader = material.shader;
            if (shader == null) return;

            try
            {
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(shader, i);

                        // 先检查材质球是否有该属性
                        if (material.HasProperty(propertyName))
                        {
                            Texture texture = material.GetTexture(propertyName);

                            // 检查贴图引用是否丢失
                            if (texture == null)
                            {
                                // 检查是否真的设置了贴图但引用丢失
                                SerializedObject serializedMaterial = new SerializedObject(material);

                                // 正确访问材质球的贴图属性
                                SerializedProperty texturesArray =
                                    serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");
                                bool foundMissingReference = false;

                                if (texturesArray != null)
                                {
                                    for (int j = 0; j < texturesArray.arraySize; j++)
                                    {
                                        SerializedProperty texEntry = texturesArray.GetArrayElementAtIndex(j);
                                        SerializedProperty texName = texEntry.FindPropertyRelative("first");

                                        // 检查是否是我们要找的属性
                                        if (texName.stringValue == propertyName)
                                        {
                                            SerializedProperty texValue =
                                                texEntry.FindPropertyRelative("second.m_Texture");
                                            if (texValue != null && texValue.objectReferenceInstanceIDValue != 0 &&
                                                texValue.objectReferenceValue == null)
                                            {
                                                // 找到真正丢失的引用
                                                foundMissingReference = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (foundMissingReference)
                                {
                                    // 添加到丢失引用列表
                                    AddMissingReference("材质球贴图", materialPath, propertyName, "贴图");
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"检查材质球 {materialPath} 时出错: {ex.Message}");
            }
        }

        private void CheckPrefabReferences(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            // 检查预制体上的所有组件
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null)
                {
                    // 组件本身丢失
                    AddMissingReference("预制体组件", prefabPath, "Missing Script", "组件");
                    continue;
                }

                // 检查组件上的所有序列化字段
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == null &&
                        property.objectReferenceInstanceIDValue != 0)
                    {
                        // 引用丢失
                        AddMissingReference("预制体引用", prefabPath, $"{component.GetType().Name}.{property.displayName}",
                            "对象引用");
                    }
                }
            }
        }

        private void CheckSceneReferences(string scenePath)
        {
            // 注意：检查场景需要打开场景，这可能会影响当前工作
            // 这里只是简单实现，实际使用时可能需要更复杂的处理

            // 保存当前场景路径，以便检查完后恢复
            string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            bool sceneChanged = false;

            if (EditorUtility.DisplayDialog("检查场景", $"检查场景 {scenePath} 需要临时打开它，这可能会影响您当前的工作。是否继续？", "继续", "跳过"))
            {
                // 保存当前场景
                if (!string.IsNullOrEmpty(currentScenePath))
                {
                    sceneChanged = true;
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }

                // 打开要检查的场景
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // 获取场景中的所有GameObject
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObject in rootObjects)
                {
                    // 检查所有组件
                    Component[] components = rootObject.GetComponentsInChildren<Component>(true);
                    foreach (Component component in components)
                    {
                        if (component == null)
                        {
                            // 组件本身丢失
                            AddMissingReference("场景组件", scenePath, $"{rootObject.name} - Missing Script", "组件");
                            continue;
                        }

                        // 检查组件上的所有序列化字段
                        SerializedObject serializedObject = new SerializedObject(component);
                        SerializedProperty property = serializedObject.GetIterator();

                        while (property.NextVisible(true))
                        {
                            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                                property.objectReferenceValue == null &&
                                property.objectReferenceInstanceIDValue != 0)
                            {
                                // 引用丢失
                                AddMissingReference("场景引用", scenePath,
                                    $"{rootObject.name}.{component.GetType().Name}.{property.displayName}", "对象引用");
                            }
                        }
                    }
                }

                // 恢复之前的场景
                if (sceneChanged && !string.IsNullOrEmpty(currentScenePath))
                {
                    EditorSceneManager.OpenScene(currentScenePath);
                }
            }
        }

        private void CheckScriptableObjectReferences(string assetPath)
        {
            // 尝试加载为ScriptableObject
            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (scriptableObject == null) return; // 不是ScriptableObject，跳过

            // 检查ScriptableObject上的所有序列化字段
            SerializedObject serializedObject = new SerializedObject(scriptableObject);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue == null &&
                    property.objectReferenceInstanceIDValue != 0)
                {
                    // 引用丢失
                    AddMissingReference("ScriptableObject引用", assetPath, property.displayName, "对象引用");
                }
            }
        }

        private void AddMissingReference(string category, string assetPath, string propertyName, string missingType)
        {
            checkResult.AddMissingReference(category, new MissingReference(assetPath, propertyName, missingType));
        }

        private void DisplayResults()
        {
            EditorGUILayout.LabelField("检查结果", EditorStyles.boldLabel);

            if (checkResult.MissingReferences.Count == 0)
            {
                EditorGUILayout.HelpBox("未发现丢失的引用！", MessageType.Info);
                return;
            }

            // 添加全部展开/折叠按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(showAllFoldouts ? "全部折叠" : "全部展开", GUILayout.Width(100)))
            {
                showAllFoldouts = !showAllFoldouts;
                foreach (var category in checkResult.MissingReferences.Keys)
                {
                    categoryFoldouts[category] = showAllFoldouts;
                }
            }

            // 显示问题总数
            EditorGUILayout.LabelField($"共发现 {checkResult.TotalIssueCount} 个问题", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var category in checkResult.MissingReferences.Keys.OrderBy(k => k))
            {
                // 确保每个类别都有折叠状态
                if (!categoryFoldouts.ContainsKey(category))
                {
                    categoryFoldouts[category] = showAllFoldouts;
                }

                EditorGUILayout.BeginVertical("box");

                // 类别标题和折叠控制
                EditorGUILayout.BeginHorizontal();
                categoryFoldouts[category] = EditorGUILayout.Foldout(categoryFoldouts[category],
                    $"{category} ({checkResult.MissingReferences[category].Count}个问题)", true,
                    EditorStyles.foldoutHeader);
                EditorGUILayout.EndHorizontal();

                // 如果展开，显示该类别的所有问题
                if (categoryFoldouts[category])
                {
                    // 按资源路径对引用进行分组
                    var referencesByAsset = checkResult.MissingReferences[category]
                        .GroupBy(r => r.AssetPath)
                        .OrderBy(g => g.Key)
                        .ToList();

                    // 表头
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("资源路径", EditorStyles.boldLabel, GUILayout.Width(300));
                    EditorGUILayout.LabelField("问题数量", EditorStyles.boldLabel, GUILayout.Width(80));
                    EditorGUILayout.LabelField("操作", EditorStyles.boldLabel, GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(2);

                    // 确保每个类别都有资源折叠状态字典
                    if (!assetFoldouts.ContainsKey(category))
                    {
                        assetFoldouts[category] = new Dictionary<string, bool>();
                    }

                    // 显示每个资源及其问题
                    foreach (var assetGroup in referencesByAsset)
                    {
                        string assetPath = assetGroup.Key;
                        var assetReferences = assetGroup.ToList();
                        int problemCount = assetReferences.Count;

                        // 确保每个资源都有折叠状态
                        if (!assetFoldouts[category].ContainsKey(assetPath))
                        {
                            assetFoldouts[category][assetPath] = true; // 默认展开
                        }

                        EditorGUILayout.BeginVertical("box");

                        // 资源标题行
                        EditorGUILayout.BeginHorizontal();

                        // 资源路径（可点击，带折叠控制）
                        GUI.color = new Color(0.8f, 0.8f, 1.0f);
                        assetFoldouts[category][assetPath] = EditorGUILayout.Foldout(assetFoldouts[category][assetPath],
                            assetPath, true);
                        GUI.color = Color.white;

                        // 问题数量
                        EditorGUILayout.LabelField($"{problemCount}个", GUILayout.Width(80));

                        // 操作按钮
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(120));

                        // 选择按钮
                        if (GUILayout.Button("选择", GUILayout.Width(50)))
                        {
                            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                            Selection.activeObject = asset;
                        }

                        // 定位按钮
                        if (GUILayout.Button("定位", GUILayout.Width(50)))
                        {
                            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                            EditorGUIUtility.PingObject(asset);
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();

                        // 如果展开，显示该资源的所有问题
                        if (assetFoldouts[category][assetPath])
                        {
                            EditorGUI.indentLevel++;

                            // 显示属性表头
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20); // 缩进
                            EditorGUILayout.LabelField("属性名称", EditorStyles.miniLabel, GUILayout.Width(200));
                            EditorGUILayout.LabelField("丢失类型", EditorStyles.miniLabel, GUILayout.Width(100));
                            EditorGUILayout.LabelField("节点路径", EditorStyles.miniLabel, GUILayout.Width(200));
                            EditorGUILayout.EndHorizontal();

                            // 显示每个属性的问题
                            foreach (var reference in assetReferences)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(20); // 缩进
                                EditorGUILayout.LabelField(reference.PropertyName, GUILayout.Width(200));
                                EditorGUILayout.LabelField(reference.MissingType, GUILayout.Width(100));
                                EditorGUILayout.LabelField(reference.NodePath, GUILayout.Width(200));
                                EditorGUILayout.EndHorizontal();

                                // 显示详细错误描述（如果有）
                                if (!string.IsNullOrEmpty(reference.ErrorDescription))
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(40); // 更多缩进
                                    EditorGUILayout.HelpBox(reference.ErrorDescription, MessageType.Warning);
                                    EditorGUILayout.EndHorizontal();
                                }
                            }

                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ExportResults()
        {
            if (checkResult.MissingReferences.Count == 0)
            {
                EditorUtility.DisplayDialog("导出结果", "没有丢失的引用可以导出", "确定");
                return;
            }

            // 创建导出格式选择菜单
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("文本格式 (TXT)"), false, () => ExportAsTxt());
            menu.AddItem(new GUIContent("CSV格式 (Excel)"), false, () => ExportAsCsv());
            menu.ShowAsContext();
        }

        private void ExportAsTxt()
        {
            string path = EditorUtility.SaveFilePanel("导出检查结果 (TXT)", "", "MissingReferences", "txt");
            if (string.IsNullOrEmpty(path)) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("资源引用丢失检查结果");
            sb.AppendLine("=======================");
            sb.AppendLine();

            foreach (var category in checkResult.MissingReferences.Keys.OrderBy(k => k))
            {
                sb.AppendLine($"【{category}】");

                // 按资源路径对引用进行分组
                var referencesByAsset = checkResult.MissingReferences[category]
                    .GroupBy(r => r.AssetPath)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var assetGroup in referencesByAsset)
                {
                    string assetPath = assetGroup.Key;
                    var assetReferences = assetGroup.ToList();
                    int problemCount = assetReferences.Count;

                    sb.AppendLine($"  资源路径: {assetPath} ({problemCount}个问题)");

                    foreach (var reference in assetReferences)
                    {
                        sb.AppendLine($"    - 属性: {reference.PropertyName}");
                        sb.AppendLine($"      类型: {reference.MissingType}");

                        // 添加节点路径信息
                        if (!string.IsNullOrEmpty(reference.NodePath))
                        {
                            sb.AppendLine($"      节点路径: {reference.NodePath}");
                        }

                        // 添加组件类型信息
                        if (!string.IsNullOrEmpty(reference.ComponentTypeName))
                        {
                            sb.AppendLine($"      组件类型: {reference.ComponentTypeName}");
                        }

                        // 添加详细错误描述
                        if (!string.IsNullOrEmpty(reference.ErrorDescription))
                        {
                            sb.AppendLine($"      错误描述: {reference.ErrorDescription}");
                        }
                    }

                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            File.WriteAllText(path, sb.ToString());
            EditorUtility.DisplayDialog("导出成功", $"检查结果已导出到: {path}", "确定");

            // 在资源管理器中显示文件
            EditorUtility.RevealInFinder(path);
        }

        private void ExportAsCsv()
        {
            string path = EditorUtility.SaveFilePanel("导出检查结果 (CSV)", "", "MissingReferences", "csv");
            if (string.IsNullOrEmpty(path)) return;

            StringBuilder sb = new StringBuilder();

            // CSV 表头 - 添加了新的列
            sb.AppendLine("类别,资源路径,问题数量,属性名称,丢失类型,节点路径,组件类型,错误描述");

            // CSV 内容
            foreach (var category in checkResult.MissingReferences.Keys.OrderBy(k => k))
            {
                // 按资源路径对引用进行分组
                var referencesByAsset = checkResult.MissingReferences[category]
                    .GroupBy(r => r.AssetPath)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var assetGroup in referencesByAsset)
                {
                    string assetPath = assetGroup.Key;
                    var assetReferences = assetGroup.ToList();
                    int problemCount = assetReferences.Count;

                    // 处理CSV中的特殊字符
                    string escapedPath = assetPath.Replace(",", "，");
                    string escapedCategory = category.Replace(",", "，");

                    foreach (var reference in assetReferences)
                    {
                        string escapedProperty = reference.PropertyName.Replace(",", "，");
                        string escapedType = reference.MissingType.Replace(",", "，");
                        string escapedNodePath = reference.NodePath?.Replace(",", "，") ?? "";
                        string escapedComponentType = reference.ComponentTypeName?.Replace(",", "，") ?? "";
                        string escapedErrorDesc =
                            reference.ErrorDescription?.Replace(",", "，").Replace("\n", " ") ?? "";

                        sb.AppendLine(
                            $"{escapedCategory},{escapedPath},{problemCount},{escapedProperty},{escapedType},{escapedNodePath},{escapedComponentType},{escapedErrorDesc}");
                    }
                }
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            EditorUtility.DisplayDialog("导出成功", $"CSV检查结果已导出到: {path}", "确定");

            // 在资源管理器中显示文件
            EditorUtility.RevealInFinder(path);
        }
    }
}