using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ArtTools
{
    /// <summary>
    /// 场景检查配置类，用于存储不需要询问直接检查的场景路径列表
    /// </summary>
    public class MissingReferenceSceneConfig : ScriptableObject
    {
        // 不需要询问直接检查的场景路径列表
        public List<string> ScenesToCheckWithoutPrompt = new List<string>();
        
        // 是否启用自动检查功能
        public bool EnableAutoCheck = true;
        
        // 是否只检查列表内的场景
        public bool OnlyCheckListedScenes = false;
        
        // 配置文件的保存路径
        private const string ConfigPath = "Assets/Editor/ArtTools/Editor/ArtTools/MissingReferenceChecker/Resources";
        private const string ConfigFileName = "MissingReferenceSceneConfig.asset";
        
        // 获取配置实例（单例模式）
        private static MissingReferenceSceneConfig _instance;
        public static MissingReferenceSceneConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 尝试从Resources加载
                    _instance = AssetDatabase.LoadAssetAtPath<MissingReferenceSceneConfig>(
                        Path.Combine(ConfigPath, ConfigFileName));
                    
                    // 如果不存在，则创建新的配置文件
                    if (_instance == null)
                    {
                        _instance = CreateInstance<MissingReferenceSceneConfig>();
                        
                        // 确保目录存在
                        if (!Directory.Exists(ConfigPath))
                        {
                            Directory.CreateDirectory(ConfigPath);
                        }
                        
                        // 保存配置文件
                        AssetDatabase.CreateAsset(_instance, Path.Combine(ConfigPath, ConfigFileName));
                        AssetDatabase.SaveAssets();
                    }
                }
                
                return _instance;
            }
        }
        
        /// <summary>
        /// 添加场景到不询问列表
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        public void AddSceneToNoPromptList(string scenePath)
        {
            if (!ScenesToCheckWithoutPrompt.Contains(scenePath))
            {
                ScenesToCheckWithoutPrompt.Add(scenePath);
                // 添加后自动排序
                SortSceneList();
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 对场景列表按路径进行排序
        /// </summary>
        public void SortSceneList()
        {
            ScenesToCheckWithoutPrompt.Sort();
            SaveConfig();
        }
        
        /// <summary>
        /// 从不询问列表中移除场景
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        public void RemoveSceneFromNoPromptList(string scenePath)
        {
            if (ScenesToCheckWithoutPrompt.Contains(scenePath))
            {
                ScenesToCheckWithoutPrompt.Remove(scenePath);
                SaveConfig();
            }
        }
        
        /// <summary>
        /// 检查场景是否在不询问列表中
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        /// <returns>是否在不询问列表中</returns>
        public bool IsSceneInNoPromptList(string scenePath)
        {
            return ScenesToCheckWithoutPrompt.Contains(scenePath);
        }
        
        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveConfig()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
    
    /// <summary>
    /// 场景配置编辑器窗口
    /// </summary>
    public class MissingReferenceSceneConfigEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private string newScenePath = "";
        
        [MenuItem("工具/资源引用丢失查询//场景配置")]
        public static void ShowWindow()
        {
            GetWindow<MissingReferenceSceneConfigEditor>("场景检查配置");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("场景检查配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 启用自动检查的开关
            MissingReferenceSceneConfig.Instance.EnableAutoCheck = EditorGUILayout.Toggle(
                "启用自动检查", MissingReferenceSceneConfig.Instance.EnableAutoCheck);
                
            // 只检查列表内场景的开关
            MissingReferenceSceneConfig.Instance.OnlyCheckListedScenes = EditorGUILayout.Toggle(
                "只检查列表内场景", MissingReferenceSceneConfig.Instance.OnlyCheckListedScenes);
            
            EditorGUILayout.HelpBox("启用自动检查后，列表中的场景将不会显示询问对话框，直接进行检查。\n启用只检查列表内场景后，不在列表中的场景将被跳过。", MessageType.Info);
            
            EditorGUILayout.Space();
            GUILayout.Label("不询问直接检查的场景列表", EditorStyles.boldLabel);
            
            // 显示场景数量
            EditorGUILayout.LabelField($"当前列表中共有 {MissingReferenceSceneConfig.Instance.ScenesToCheckWithoutPrompt.Count} 个场景");
            
            // 显示当前列表
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            List<string> scenesToRemove = new List<string>();
            foreach (string scenePath in MissingReferenceSceneConfig.Instance.ScenesToCheckWithoutPrompt)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(scenePath);
                
                if (GUILayout.Button("移除", GUILayout.Width(60)))
                {
                    scenesToRemove.Add(scenePath);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // 移除选中的场景
            foreach (string scenePath in scenesToRemove)
            {
                MissingReferenceSceneConfig.Instance.RemoveSceneFromNoPromptList(scenePath);
            }
            
            EditorGUILayout.EndScrollView();
            
            // 添加新场景
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            newScenePath = EditorGUILayout.TextField("场景路径", newScenePath);
            
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(newScenePath))
                {
                    MissingReferenceSceneConfig.Instance.AddSceneToNoPromptList(newScenePath);
                    newScenePath = "";
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            // 选择单个场景按钮
            if (GUILayout.Button("选择单个场景"))
            {
                string path = EditorUtility.OpenFilePanel("选择场景文件", "Assets", "unity");
                if (!string.IsNullOrEmpty(path))
                {
                    // 转换为相对于Assets的路径
                    string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                    MissingReferenceSceneConfig.Instance.AddSceneToNoPromptList(relativePath);
                }
            }
            
            /*// 批量选择场景按钮
            if (GUILayout.Button("批量选择场景"))
            {
                string[] paths = EditorUtility.OpenFilePanelWithFilters("批量选择场景文件", "Assets", new string[] {"Unity场景", "unity"});
                if (paths != null && paths.Length > 0)
                {
                    int addedCount = 0;
                    foreach (string path in paths)
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            // 转换为相对于Assets的路径
                            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                            if (!MissingReferenceSceneConfig.Instance.IsSceneInNoPromptList(relativePath))
                            {
                                MissingReferenceSceneConfig.Instance.AddSceneToNoPromptList(relativePath);
                                addedCount++;
                            }
                        }
                    }
                    if (addedCount > 0)
                    {
                        EditorUtility.DisplayDialog("批量添加成功", $"已成功添加{addedCount}个场景到不询问列表", "确定");
                    }
                }
            }*/
            
            EditorGUILayout.EndHorizontal();
            
            // 选择文件夹按钮
            if (GUILayout.Button("从文件夹批量导入场景"))
            {
                string folderPath = EditorUtility.OpenFolderPanel("选择场景文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(folderPath))
                {
                    // 确保路径是项目内的路径
                    if (folderPath.StartsWith(Application.dataPath))
                    {
                        // 查找文件夹下所有.unity文件
                        string[] sceneFiles = Directory.GetFiles(folderPath, "*.unity", SearchOption.AllDirectories);
                        int addedCount = 0;
                        
                        foreach (string scenePath in sceneFiles)
                        {
                            // 转换为相对于Assets的路径
                            string relativePath = "Assets" + scenePath.Substring(Application.dataPath.Length);
                            if (!MissingReferenceSceneConfig.Instance.IsSceneInNoPromptList(relativePath))
                            {
                                MissingReferenceSceneConfig.Instance.AddSceneToNoPromptList(relativePath);
                                addedCount++;
                            }
                        }
                        
                        if (addedCount > 0)
                        {
                            EditorUtility.DisplayDialog("批量导入成功", $"已从文件夹导入{addedCount}个场景到不询问列表", "确定");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("导入结果", "没有找到新的场景文件或所有场景已在列表中", "确定");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("路径错误", "请选择项目内的文件夹", "确定");
                    }
                }
            }
            
            // 保存按钮
            EditorGUILayout.Space();
            
            // 拖放区域
            EditorGUILayout.HelpBox("将场景文件拖放到此处进行批量添加", MessageType.Info);
            
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "拖放场景文件到这里");
            
            // 处理拖放事件
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;
                        
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        int addedCount = 0;
                        foreach (string path in DragAndDrop.paths)
                        {
                            // 检查是否是场景文件
                            if (path.EndsWith(".unity"))
                            {
                                // 确保路径是相对于Assets的路径
                                string relativePath = path;
                                if (path.StartsWith(Application.dataPath))
                                {
                                    relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                                }
                                
                                if (!MissingReferenceSceneConfig.Instance.IsSceneInNoPromptList(relativePath))
                                {
                                    MissingReferenceSceneConfig.Instance.AddSceneToNoPromptList(relativePath);
                                    addedCount++;
                                }
                            }
                        }
                        
                        if (addedCount > 0)
                        {
                            EditorUtility.DisplayDialog("拖放添加成功", $"已成功添加{addedCount}个场景到不询问列表", "确定");
                        }
                    }
                    
                    evt.Use();
                    break;
            }
            
            // 清空列表按钮
            EditorGUILayout.Space();
            if (GUILayout.Button("清空列表") && 
                EditorUtility.DisplayDialog("确认清空", "确定要清空所有场景列表吗？", "确定", "取消"))
            {
                MissingReferenceSceneConfig.Instance.ScenesToCheckWithoutPrompt.Clear();
                MissingReferenceSceneConfig.Instance.SaveConfig();
                EditorUtility.DisplayDialog("清空成功", "场景列表已清空", "确定");
            }
            
            if (GUILayout.Button("排序场景列表"))
            {
                MissingReferenceSceneConfig.Instance.SortSceneList();
                EditorUtility.DisplayDialog("排序成功", "场景列表已按路径排序", "确定");
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("保存配置"))
            {
                MissingReferenceSceneConfig.Instance.SaveConfig();
                EditorUtility.DisplayDialog("保存成功", "场景检查配置已保存", "确定");
            }
        }
    }
}