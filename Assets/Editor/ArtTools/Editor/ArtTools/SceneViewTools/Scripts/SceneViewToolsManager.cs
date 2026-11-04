/**
 * 文件名: SceneViewToolsManager.cs
 * 作用: 场景视图工具管理器，负责工具的初始化、窗口管理和核心功能
 * 作者: ArtTools Team
 * 日期: 2024
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 场景视图工具管理器
    /// 负责管理场景视图工具的初始化、窗口显示和核心功能
    /// </summary>
    public static class SceneViewToolsManager
    {
        #region 私有字段
        
        /// <summary>
        /// 每个Scene视图的窗口数据字典
        /// 键为SceneView的实例ID，值为对应的窗口数据
        /// </summary>
        private static Dictionary<int, SceneViewWindowData> sceneViewWindows = new Dictionary<int, SceneViewWindowData>();
        
        /// <summary>
        /// 工具数据引用
        /// </summary>
        private static SceneViewToolsData toolsData;
        
        /// <summary>
        /// GUI样式缓存
        /// </summary>
        private static GUIStyle buttonStyle;
        private static GUIStyle boxStyle;
        private static GUISkin customSkin;
        
        /// <summary>
        /// 是否需要重新调整窗口大小（全局标志）
        /// </summary>
        private static bool needWindowResize = false;
        
        /// <summary>
        /// 当前活动的窗口数据（用于查找方法访问）
        /// </summary>
        private static SceneViewWindowData currentWindowData;
        
        /// <summary>
        /// 性能统计数据
        /// </summary>
        private static PerformanceStats performanceStats = new PerformanceStats();
        
        #endregion
        
        #region 性能统计类
        
        /// <summary>
        /// 性能统计数据类
        /// 用于记录和分析查找操作的性能指标
        /// </summary>
        private class PerformanceStats
        {
            /// <summary>
            /// 查找操作统计字典
            /// 键为操作名称，值为统计信息
            /// </summary>
            public Dictionary<string, OperationStats> operationStats = new Dictionary<string, OperationStats>();
            
            /// <summary>
            /// 是否显示性能统计信息
            /// </summary>
            public bool showStats = false;
            
            /// <summary>
            /// 统计信息滚动位置
            /// </summary>
            public Vector2 statsScrollPosition = Vector2.zero;
        }
        
        /// <summary>
        /// 单个操作的统计信息
        /// 记录操作的执行次数、总耗时、平均耗时等
        /// </summary>
        private class OperationStats
        {
            /// <summary>
            /// 操作执行次数
            /// </summary>
            public int executionCount = 0;
            
            /// <summary>
            /// 总耗时（毫秒）
            /// </summary>
            public double totalTimeMs = 0.0;
            
            /// <summary>
            /// 最后一次执行耗时（毫秒）
            /// </summary>
            public double lastExecutionTimeMs = 0.0;
            
            /// <summary>
            /// 最大耗时（毫秒）
            /// </summary>
            public double maxTimeMs = 0.0;
            
            /// <summary>
            /// 最小耗时（毫秒）
            /// </summary>
            public double minTimeMs = double.MaxValue;
            
            /// <summary>
            /// 平均耗时（毫秒）
            /// </summary>
            public double AverageTimeMs => executionCount > 0 ? totalTimeMs / executionCount : 0.0;
            
            /// <summary>
            /// 记录一次操作执行
            /// </summary>
            /// <param name="timeMs">执行耗时（毫秒）</param>
            public void RecordExecution(double timeMs)
            {
                executionCount++;
                totalTimeMs += timeMs;
                lastExecutionTimeMs = timeMs;
                maxTimeMs = System.Math.Max(maxTimeMs, timeMs);
                minTimeMs = System.Math.Min(minTimeMs, timeMs);
            }
        }
        
        #endregion
        
        #region 窗口数据类
        
        /// <summary>
        /// Scene视图窗口数据
        /// 存储每个Scene视图独立的窗口状态和位置信息
        /// </summary>
        private class SceneViewWindowData
        {
            /// <summary>
            /// 工具窗口矩形区域
            /// </summary>
            public Rect toolsWindowRect = new Rect(1, 1, 1, 1);
            
            /// <summary>
            /// 对象列表窗口矩形区域
            /// </summary>
            public Rect objectListWindowRect = new Rect(1, 1, 1, 1);
            
            /// <summary>
            /// 当前找到的对象列表
            /// </summary>
            public List<GameObject> foundObjects = new List<GameObject>();
            
            /// <summary>
            /// 对象列表窗口标题
            /// </summary>
            public string objectListWindowTitle = "对象列表";
            
            /// <summary>
            /// 是否需要重置窗口大小
            /// </summary>
            public bool needResize = false;
            
            /// <summary>
            /// 对象列表滚动位置
            /// </summary>
            public Vector2 scrollPosition = Vector2.zero;
            
            /// <summary>
            /// 是否隐藏未激活的物件
            /// </summary>
            public bool hideInactiveObjects = false;
        }
        
        /// <summary>
        /// 当前选中的PlayableDirector
        /// </summary>
        private static PlayableDirector selectedPlayableDirector;
        
        /// <summary>
        /// 是否显示时间轴编辑信息
        /// </summary>
        private static bool showTimelineEditInfo = false;
        
        /// <summary>
        /// 是否显示灯光编辑信息
        /// </summary>
        private static bool showLightEditInfo = false;
        
        /// <summary>
        /// 时间轴日志信息
        /// </summary>
        private static string timelineLogMessage = string.Empty;
        private static MessageType timelineLogType = MessageType.Info;
        
        /// <summary>
        /// 灯光日志信息
        /// </summary>
        private static string lightLogMessage = string.Empty;
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化场景视图工具
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // 初始化配置
            SceneViewToolsConfig.InitializeSettings();
            
            // 注册场景视图GUI回调
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            
            Debug.Log("[SceneViewToolsManager] 场景视图工具已初始化");
        }
        
        #endregion
        
        #region 性能监控方法
        
        /// <summary>
        /// 执行带性能监控的操作
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="operation">要执行的操作</param>
        /// <returns>操作执行结果</returns>
        private static T ExecuteWithPerformanceMonitoring<T>(string operationName, System.Func<T> operation)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                T result = operation();
                stopwatch.Stop();
                
                RecordOperationPerformance(operationName, stopwatch.Elapsed.TotalMilliseconds);
                
                return result;
            }
            catch (System.Exception ex)
            {
                stopwatch.Stop();
                RecordOperationPerformance(operationName, stopwatch.Elapsed.TotalMilliseconds);
                Debug.LogError($"[SceneViewTools] 操作 '{operationName}' 执行失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 执行带性能监控的无返回值操作
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="operation">要执行的操作</param>
        private static void ExecuteWithPerformanceMonitoring(string operationName, System.Action operation)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                operation();
                stopwatch.Stop();
                
                RecordOperationPerformance(operationName, stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (System.Exception ex)
            {
                stopwatch.Stop();
                RecordOperationPerformance(operationName, stopwatch.Elapsed.TotalMilliseconds);
                Debug.LogError($"[SceneViewTools] 操作 '{operationName}' 执行失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 记录操作性能数据
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="timeMs">执行耗时（毫秒）</param>
        private static void RecordOperationPerformance(string operationName, double timeMs)
        {
            if (!performanceStats.operationStats.ContainsKey(operationName))
            {
                performanceStats.operationStats[operationName] = new OperationStats();
            }
            
            performanceStats.operationStats[operationName].RecordExecution(timeMs);
            
            // 如果耗时超过100ms，输出警告日志
            if (timeMs > 100.0)
            {
                Debug.LogWarning($"[SceneViewTools] 操作 '{operationName}' 耗时较长: {timeMs:F2}ms");
            }
        }
        
        /// <summary>
        /// 清除性能统计数据
        /// </summary>
        public static void ClearPerformanceStats()
        {
            performanceStats.operationStats.Clear();
            Debug.Log("[SceneViewTools] 性能统计数据已清除");
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 请求窗口重新调整大小
        /// </summary>
        public static void RequestWindowResize()
        {
            needWindowResize = true;
            
            // 同时标记所有Scene视图窗口需要重置
            foreach (var windowData in sceneViewWindows.Values)
            {
                windowData.needResize = true;
            }
        }
        
        /// <summary>
        /// 获取或创建指定Scene视图的窗口数据
        /// </summary>
        /// <param name="sceneViewId">Scene视图的实例ID</param>
        /// <returns>对应的窗口数据</returns>
        private static SceneViewWindowData GetOrCreateWindowData(int sceneViewId)
        {
            if (!sceneViewWindows.TryGetValue(sceneViewId, out SceneViewWindowData windowData))
            {
                windowData = new SceneViewWindowData();
                sceneViewWindows[sceneViewId] = windowData;
            }
            return windowData;
        }
        
        /// <summary>
        /// 清理已关闭的Scene视图数据
        /// </summary>
        private static void CleanupClosedSceneViews()
        {
            var activeSceneViewIds = new HashSet<int>();
            
            // 收集所有活动的Scene视图ID
            foreach (SceneView sceneView in SceneView.sceneViews)
            {
                if (sceneView != null)
                {
                    activeSceneViewIds.Add(sceneView.GetInstanceID());
                }
            }
            
            // 移除不再活动的Scene视图数据
            var keysToRemove = new List<int>();
            foreach (var kvp in sceneViewWindows)
            {
                if (!activeSceneViewIds.Contains(kvp.Key))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                sceneViewWindows.Remove(key);
            }
        }
        
        /// <summary>
        /// 设置时间轴编辑信息
        /// </summary>
        /// <param name="message">信息内容</param>
        /// <param name="messageType">信息类型</param>
        public static void SetTimelineEditInfo(string message, MessageType messageType = MessageType.Info)
        {
            timelineLogMessage = message;
            timelineLogType = messageType;
            showTimelineEditInfo = !string.IsNullOrEmpty(message);
        }
        
        /// <summary>
        /// 设置灯光编辑信息
        /// </summary>
        /// <param name="message">信息内容</param>
        public static void SetLightEditInfo(string message)
        {
            lightLogMessage = message;
            showLightEditInfo = !string.IsNullOrEmpty(message);
        }
        
        /// <summary>
        /// 清除编辑信息
        /// </summary>
        public static void ClearEditInfo()
        {
            showTimelineEditInfo = false;
            showLightEditInfo = false;
            timelineLogMessage = string.Empty;
            lightLogMessage = string.Empty;
        }
        
        #endregion
        
        #region 场景GUI处理
        
        /// <summary>
        /// 场景视图GUI回调
        /// </summary>
        /// <param name="sceneView">场景视图</param>
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!SceneViewToolsConfig.IsToolsEnabled)
                return;
            
            // 获取当前Scene视图的窗口数据
            int sceneViewId = sceneView.GetInstanceID();
            SceneViewWindowData windowData = GetOrCreateWindowData(sceneViewId);
            
            // 设置当前活动的窗口数据，供查找方法使用
            currentWindowData = windowData;
            
            // 初始化样式和数据
            InitializeStylesAndData();
            
            // 定期清理已关闭的Scene视图数据（每100帧清理一次）
            if (Time.frameCount % 100 == 0)
            {
                CleanupClosedSceneViews();
            }
            
            // 处理窗口大小重置
            if (needWindowResize || windowData.needResize)
            {
                // 强制重置窗口大小，让GUILayout重新计算
                windowData.toolsWindowRect = new Rect(windowData.toolsWindowRect.x, windowData.toolsWindowRect.y, 1, 1);
                windowData.objectListWindowRect = new Rect(windowData.objectListWindowRect.x, windowData.objectListWindowRect.y, 1, 1);
                windowData.needResize = false;
                needWindowResize = false;
                
                // 强制重绘Scene视图
                SceneView.RepaintAll();
            }
            
            Handles.BeginGUI();
            
            // 显示编辑信息
            DrawEditInfo();
            
            // 绘制主工具窗口
            DrawMainToolsWindow(windowData);
            
            // 绘制对象列表窗口
            DrawObjectListWindow(windowData);
            
            Handles.EndGUI();
        }
        
        /// <summary>
        /// 初始化样式和数据
        /// </summary>
        private static void InitializeStylesAndData()
        {
            // 初始化按钮样式
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { textColor = new Color(0.65f, 0.65f, 0.65f, 1f) }
                };
            }
            
            // 加载自定义皮肤
            if (customSkin == null)
            {
                customSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(SceneViewToolsConfig.GUI_SKIN_PATH);
            }
            
            // 设置盒子样式
            boxStyle = customSkin?.box ?? EditorStyles.helpBox;
            
            // 加载工具数据
            if (toolsData == null)
            {
                LoadOrCreateToolsData();
            }
        }
        
        /// <summary>
        /// 加载或创建工具数据
        /// </summary>
        private static void LoadOrCreateToolsData()
        {
            toolsData = AssetDatabase.LoadAssetAtPath<SceneViewToolsData>(SceneViewToolsConfig.DATA_ASSET_PATH);
            
            if (toolsData == null)
            {
                // 创建新的工具数据
                toolsData = ScriptableObject.CreateInstance<SceneViewToolsData>();
                toolsData.name = "SceneViewToolsData";
                
                // 确保目录存在
                string directory = System.IO.Path.GetDirectoryName(SceneViewToolsConfig.DATA_ASSET_PATH);
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    AssetDatabase.CreateFolder("Assets/Editor/ArtTools/Editor/ArtTools/SceneViewTools", "Data");
                }
                
                AssetDatabase.CreateAsset(toolsData, SceneViewToolsConfig.DATA_ASSET_PATH);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"[SceneViewToolsManager] 已创建工具数据文件: {SceneViewToolsConfig.DATA_ASSET_PATH}");
            }
        }
        
        /// <summary>
        /// 绘制编辑信息
        /// </summary>
        private static void DrawEditInfo()
        {
            if (showTimelineEditInfo && !string.IsNullOrEmpty(timelineLogMessage))
            {
                bool originalRichText = EditorStyles.helpBox.richText;
                EditorStyles.helpBox.richText = true;
                EditorGUILayout.HelpBox(timelineLogMessage, timelineLogType);
                EditorStyles.helpBox.richText = originalRichText;
            }
            
            if (showLightEditInfo && !string.IsNullOrEmpty(lightLogMessage))
            {
                bool originalRichText = EditorStyles.helpBox.richText;
                EditorStyles.helpBox.richText = true;
                EditorGUILayout.HelpBox(lightLogMessage, MessageType.None);
                EditorStyles.helpBox.richText = originalRichText;
            }
        }
        
        /// <summary>
        /// 绘制主工具窗口
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        private static void DrawMainToolsWindow(SceneViewWindowData windowData)
        {
            windowData.toolsWindowRect = GUILayout.Window(0, windowData.toolsWindowRect, DrawMainWindowContent, "场景工具");
            
            // 调整窗口位置
            AdjustMainWindowPosition(windowData);
        }
        
        /// <summary>
        /// 调整主窗口位置
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        private static void AdjustMainWindowPosition(SceneViewWindowData windowData)
        {
            if (SceneViewToolsConfig.IsWindowOnLeft)
            {
                windowData.toolsWindowRect.x = 0;
            }
            else
            {
                windowData.toolsWindowRect.x = Screen.width - windowData.toolsWindowRect.width;
            }
            
            windowData.toolsWindowRect.y = Screen.height / 2 - windowData.toolsWindowRect.height / 2;
        }
        
        /// <summary>
        /// 绘制对象列表窗口
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        private static void DrawObjectListWindow(SceneViewWindowData windowData)
        {
            if (windowData.foundObjects.Count > 0)
            {
                windowData.objectListWindowRect = GUILayout.Window(1, windowData.objectListWindowRect, 
                    (int id) => DrawObjectListContent(id, windowData), windowData.objectListWindowTitle);
                
                // 调整对象列表窗口位置
                AdjustObjectListWindowPosition(windowData);
            }
        }
        
        /// <summary>
        /// 调整对象列表窗口位置
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        private static void AdjustObjectListWindowPosition(SceneViewWindowData windowData)
        {
            if (SceneViewToolsConfig.IsWindowOnLeft)
            {
                windowData.objectListWindowRect.x = windowData.toolsWindowRect.width + 5f;
            }
            else
            {
                windowData.objectListWindowRect.x = Screen.width - windowData.toolsWindowRect.width - windowData.objectListWindowRect.width - 5f;
            }
            
            windowData.objectListWindowRect.y = Screen.height / 2 - windowData.toolsWindowRect.height / 2;
            
            // 动态调整窗口大小
            if (windowData.foundObjects.Count > 10)
            {
                // 根据对象数量动态调整窗口大小
                float baseHeight = 100f; // 基础高度（标题、按钮等）
                float scrollHeight = Mathf.Min(300f, windowData.foundObjects.Count * 20f); // 滚动区域高度
                float totalHeight = baseHeight + scrollHeight;
                
                windowData.objectListWindowRect.width = Mathf.Max(250f, SceneViewToolsConfig.CAMERA_LIST_MAX_SIZE.x);
                windowData.objectListWindowRect.height = Mathf.Min(totalHeight, Screen.height * 0.8f); // 不超过屏幕高度的80%
            }
            else
            {
                // 对象较少时使用紧凑布局
                windowData.objectListWindowRect.width = 200f;
                float compactHeight = 80f + windowData.foundObjects.Count * 22f; // 每个按钮约22像素高度
                windowData.objectListWindowRect.height = Mathf.Min(compactHeight, 300f);
            }
        }
        
        #endregion
        
        #region 窗口内容绘制
        
        /// <summary>
        /// 绘制主窗口内容
        /// </summary>
        /// <param name="windowId">窗口ID</param>
        private static void DrawMainWindowContent(int windowId)
        {
            DrawWindowControls();
            
            if (SceneViewToolsConfig.IsWindowExpanded)
            {
                DrawToolsContent();
            }
        }
        
        /// <summary>
        /// 绘制窗口控制按钮
        /// </summary>
        private static void DrawWindowControls()
        {
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(SceneViewToolsConfig.IsWindowExpanded ? "缩" : "放"))
            {
                SceneViewToolsConfig.IsWindowExpanded = !SceneViewToolsConfig.IsWindowExpanded;
                RequestWindowResize();
            }
            
            if (GUILayout.Button(SceneViewToolsConfig.IsWindowOnLeft ? "→" : "←"))
            {
                SceneViewToolsConfig.IsWindowOnLeft = !SceneViewToolsConfig.IsWindowOnLeft;
                RequestWindowResize();
            }
            
            if (GUILayout.Button("关"))
            {
                SceneViewToolsConfig.IsToolsEnabled = false;
            }
            
            GUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制工具内容
        /// </summary>
        private static void DrawToolsContent()
        {
            GUILayout.BeginVertical(boxStyle);
            
            DrawSceneObjectFinders();
            DrawPlayableDirectorField();
            DrawCustomSearchItems();
            DrawPerformanceStats();
            
            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制场景对象查找器
        /// </summary>
        private static void DrawSceneObjectFinders()
        {
            GUILayout.Label("场景物件查找");
            
            // 相机查找按钮
            GUILayout.BeginHorizontal();
            DrawCameraFindButtons();
            GUILayout.EndHorizontal();
            
            // 灯光查找按钮
            GUILayout.BeginHorizontal();
            DrawLightFindButtons();
            GUILayout.EndHorizontal();
            
            // 其他组件查找按钮
            DrawOtherComponentFindButtons();
        }
        
        /// <summary>
        /// 绘制相机查找按钮
        /// </summary>
        private static void DrawCameraFindButtons()
        {
            if (GUILayout.Button("主相机"))
            {
                FindMainCamera();
            }
            
            if (GUILayout.Button("相机"))
            {
                FindAllCameras();
            }
        }
        
        /// <summary>
        /// 绘制灯光查找按钮
        /// </summary>
        private static void DrawLightFindButtons()
        {
            if (GUILayout.Button("主灯光"))
            {
                FindMainLight();
            }
            
            if (GUILayout.Button("灯光"))
            {
                FindAllLights();
            }
        }
        
        /// <summary>
        /// 绘制其他组件查找按钮
        /// </summary>
        private static void DrawOtherComponentFindButtons()
        {
            if (GUILayout.Button("反射球"))
            {
                FindReflectionProbes();
            }
            
            if (GUILayout.Button("特效"))
            {
                FindParticleSystems();
            }
            
            if (GUILayout.Button("查找隐藏物体"))
            {
                FindHiddenObjects();
            }
        }
        
        /// <summary>
        /// 绘制PlayableDirector字段
        /// </summary>
        private static void DrawPlayableDirectorField()
        {
            selectedPlayableDirector = (PlayableDirector)EditorGUILayout.ObjectField(
                selectedPlayableDirector, typeof(PlayableDirector), true);
        }
        
        /// <summary>
        /// 绘制自定义搜索项
        /// </summary>
        private static void DrawCustomSearchItems()
        {
            if (toolsData == null) return;
            
            // 配置文件按钮
            if (GUILayout.Button("配置文件"))
            {
                Selection.activeObject = toolsData;
            }
            
            GUILayout.Label("按名称查找");
            
            // 游戏对象搜索项
            foreach (var item in toolsData.gameObjectItems)
            {
                if (!string.IsNullOrEmpty(item.objectName))
                {
                    if (GUILayout.Button(item.GetDisplayLabel()))
                    {
                        FindGameObjectByName(item.objectName);
                    }
                }
            }
            
            GUILayout.Label("资源查找");
            
            // 资源搜索项
            foreach (var item in toolsData.assetItems)
            {
                if (item.asset != null)
                {
                    if (GUILayout.Button(item.GetDisplayLabel()))
                    {
                        Selection.activeObject = item.asset;
                    }
                }
            }
        }
        
        /// <summary>
        /// 绘制性能统计信息
        /// </summary>
        private static void DrawPerformanceStats()
        {
            GUILayout.Space(5f);
            
            // 性能统计折叠面板
            bool newShowStats = EditorGUILayout.Foldout(performanceStats.showStats, "性能统计", true);
            if (newShowStats != performanceStats.showStats)
            {
                performanceStats.showStats = newShowStats;
                RequestWindowResize();
            }
            
            if (performanceStats.showStats)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                if (performanceStats.operationStats.Count == 0)
                {
                    GUILayout.Label("暂无性能数据", EditorStyles.miniLabel);
                }
                else
                {
                    // 统计信息标题
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("操作", EditorStyles.boldLabel, GUILayout.Width(80));
                    GUILayout.Label("次数", EditorStyles.boldLabel, GUILayout.Width(40));
                    GUILayout.Label("平均耗时", EditorStyles.boldLabel, GUILayout.Width(60));
                    GUILayout.Label("最后耗时", EditorStyles.boldLabel, GUILayout.Width(60));
                    GUILayout.EndHorizontal();
                    
                    // 如果统计项较多，使用滚动视图
                    if (performanceStats.operationStats.Count > 5)
                    {
                        performanceStats.statsScrollPosition = GUILayout.BeginScrollView(
                            performanceStats.statsScrollPosition,
                            GUILayout.Height(120f)
                        );
                    }
                    
                    // 显示每个操作的统计信息
                    foreach (var kvp in performanceStats.operationStats.OrderByDescending(x => x.Value.lastExecutionTimeMs))
                    {
                        var operationName = kvp.Key;
                        var stats = kvp.Value;
                        
                        GUILayout.BeginHorizontal();
                        
                        // 操作名称
                        GUILayout.Label(operationName, GUILayout.Width(80));
                        
                        // 执行次数
                        GUILayout.Label(stats.executionCount.ToString(), GUILayout.Width(40));
                        
                        // 平均耗时（根据耗时长短使用不同颜色）
                        var avgColor = stats.AverageTimeMs > 100 ? Color.red : 
                                      stats.AverageTimeMs > 50 ? Color.yellow : Color.green;
                        var originalColor = GUI.contentColor;
                        GUI.contentColor = avgColor;
                        GUILayout.Label($"{stats.AverageTimeMs:F1}ms", GUILayout.Width(60));
                        
                        // 最后一次耗时
                        var lastColor = stats.lastExecutionTimeMs > 100 ? Color.red : 
                                       stats.lastExecutionTimeMs > 50 ? Color.yellow : Color.green;
                        GUI.contentColor = lastColor;
                        GUILayout.Label($"{stats.lastExecutionTimeMs:F1}ms", GUILayout.Width(60));
                        
                        GUI.contentColor = originalColor;
                        
                        GUILayout.EndHorizontal();
                    }
                    
                    if (performanceStats.operationStats.Count > 5)
                    {
                        GUILayout.EndScrollView();
                    }
                }
                
                // 控制按钮
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("清除统计", GUILayout.Width(80)))
                {
                    ClearPerformanceStats();
                }
                
                // 显示总体统计信息
                if (performanceStats.operationStats.Count > 0)
                {
                    var totalOperations = performanceStats.operationStats.Values.Sum(s => s.executionCount);
                    var totalTime = performanceStats.operationStats.Values.Sum(s => s.totalTimeMs);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"总计: {totalOperations} 次操作, {totalTime:F1}ms", EditorStyles.miniLabel);
                }
                
                GUILayout.EndHorizontal();
                
                GUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// 绘制对象列表窗口内容
        /// </summary>
        /// <param name="windowId">窗口ID</param>
        /// <param name="windowData">窗口数据</param>
        private static void DrawObjectListContent(int windowId, SceneViewWindowData windowData)
        {
            GUILayout.BeginVertical();
            
            // 显示对象数量信息
            var filteredObjects = GetFilteredObjects(windowData);
            var totalCount = windowData.foundObjects.Count;
            var filteredCount = filteredObjects.Count;
            
            if (windowData.hideInactiveObjects && filteredCount < totalCount)
            {
                GUILayout.Label($"显示 {filteredCount} 个对象（共 {totalCount} 个，已隐藏 {totalCount - filteredCount} 个未激活）", EditorStyles.miniLabel);
            }
            else
            {
                GUILayout.Label($"找到 {totalCount} 个对象", EditorStyles.miniLabel);
            }
            
            // 如果对象数量较多，使用滚动视图
            if (filteredCount > 10)
            {
                // 限制滚动区域的高度
                float maxHeight = Mathf.Min(300f, filteredCount * 20f);
                windowData.scrollPosition = GUILayout.BeginScrollView(
                    windowData.scrollPosition, 
                    GUILayout.Height(maxHeight)
                );
            }
            
            // 绘制对象按钮列表（使用过滤后的对象）
            foreach (var obj in filteredObjects)
            {
                if (obj != null)
                {
                    // 根据对象的激活状态使用不同的样式
                    if (!obj.activeInHierarchy)
                    {
                        // 为非激活对象使用灰色文本
                        var originalColor = GUI.contentColor;
                        GUI.contentColor = Color.gray;
                        
                        if (GUILayout.Button($"[隐藏] {obj.name}"))
                        {
                            Selection.activeGameObject = obj;
                            EditorGUIUtility.PingObject(obj);
                        }
                        
                        GUI.contentColor = originalColor;
                    }
                    else
                    {
                        if (GUILayout.Button(obj.name))
                        {
                            Selection.activeGameObject = obj;
                            EditorGUIUtility.PingObject(obj);
                        }
                    }
                }
                else
                {
                    // 处理已删除的对象
                    GUI.enabled = false;
                    GUILayout.Button("<已删除的对象>", EditorStyles.miniButton);
                    GUI.enabled = true;
                }
            }
            
            // 结束滚动视图
            if (filteredCount > 10)
            {
                GUILayout.EndScrollView();
            }
            
            GUILayout.Space(5f);
            
            // 过滤选项区域
            GUILayout.BeginHorizontal();
            
            bool newHideInactive = GUILayout.Toggle(windowData.hideInactiveObjects, "隐藏未激活物件");
            if (newHideInactive != windowData.hideInactiveObjects)
            {
                windowData.hideInactiveObjects = newHideInactive;
                // 触发列表刷新
                RefreshObjectList(windowData);
            }
            
            GUILayout.EndHorizontal();
            
            // 底部按钮区域
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("全选"))
            {
                // 选择所有有效且可见的对象
                var validObjects = GetFilteredObjects(windowData).Where(obj => obj != null).ToArray();
                if (validObjects.Length > 0)
                {
                    Selection.objects = validObjects;
                }
            }
            
            if (GUILayout.Button("清理空引用"))
            {
                // 移除已删除的对象引用
                int originalCount = windowData.foundObjects.Count;
                windowData.foundObjects.RemoveAll(obj => obj == null);
                int removedCount = originalCount - windowData.foundObjects.Count;
                
                if (removedCount > 0)
                {
                    Debug.Log($"[SceneViewTools] 已清理 {removedCount} 个空引用");
                }
            }
            
            if (GUILayout.Button("关闭"))
            {
                windowData.foundObjects.Clear();
                windowData.scrollPosition = Vector2.zero;
                windowData.hideInactiveObjects = false;
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        
        #endregion
        
        #region 对象过滤方法
        
        /// <summary>
        /// 获取过滤后的对象列表
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        /// <returns>过滤后的对象列表</returns>
        private static List<GameObject> GetFilteredObjects(SceneViewWindowData windowData)
        {
            if (!windowData.hideInactiveObjects)
            {
                return windowData.foundObjects;
            }
            
            // 过滤掉未激活的对象
            return windowData.foundObjects.Where(obj => obj != null && obj.activeInHierarchy).ToList();
        }
        
        /// <summary>
        /// 刷新对象列表（重新应用过滤）
        /// </summary>
        /// <param name="windowData">窗口数据</param>
        private static void RefreshObjectList(SceneViewWindowData windowData)
        {
            // 重置滚动位置
            windowData.scrollPosition = Vector2.zero;
            
            // 触发窗口大小重新计算
            windowData.needResize = true;
            
            // 更新对象数量显示
            var filteredCount = GetFilteredObjects(windowData).Count;
            var totalCount = windowData.foundObjects.Count;
            
            if (windowData.hideInactiveObjects && filteredCount < totalCount)
            {
                Debug.Log($"[SceneViewTools] 已隐藏 {totalCount - filteredCount} 个未激活物件，显示 {filteredCount} 个激活物件");
            }
        }
        
        #endregion
        
        #region 查找方法
        
        /// <summary>
        /// 查找主相机
        /// </summary>
        private static void FindMainCamera()
        {
            ExecuteWithPerformanceMonitoring("查找主相机", () =>
            {
                GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
                if (mainCameras.Length > 0)
                {
                    Selection.objects = mainCameras;
                }
                else
                {
                    FindAllCameras();
                }
            });
        }
        
        /// <summary>
        /// 查找所有相机
        /// </summary>
        private static void FindAllCameras()
        {
            ExecuteWithPerformanceMonitoring("查找所有相机", () =>
            {
                if (currentWindowData == null) return;
                
                currentWindowData.foundObjects.Clear();
                Camera[] cameras = Resources.FindObjectsOfTypeAll<Camera>();
                
                foreach (var camera in cameras)
                {
                    if (IsValidSceneObject(camera.gameObject))
                    {
                        currentWindowData.foundObjects.Add(camera.gameObject);
                    }
                }
                
                currentWindowData.objectListWindowTitle = "相机";
                
                // 触发窗口大小重新计算
                currentWindowData.needResize = true;
                
                if (currentWindowData.foundObjects.Count == 0)
                {
                    Debug.LogError("未在场景中查找到相机");
                }
            });
        }
        
        /// <summary>
        /// 查找主灯光
        /// </summary>
        private static void FindMainLight()
        {
            ExecuteWithPerformanceMonitoring("查找主灯光", () =>
            {
                GameObject[] mainLights = GameObject.FindGameObjectsWithTag("PMLight_Sun");
                if (mainLights.Length > 0)
                {
                    Selection.objects = mainLights;
                }
                else
                {
                    FindAllLights();
                    if (currentWindowData != null && currentWindowData.foundObjects.Count == 1)
                    {
                        Selection.activeObject = currentWindowData.foundObjects[0];
                        currentWindowData.foundObjects.Clear();
                    }
                }
            });
        }
        
        /// <summary>
        /// 查找所有灯光
        /// </summary>
        private static void FindAllLights()
        {
            ExecuteWithPerformanceMonitoring("查找所有灯光", () =>
            {
                if (currentWindowData == null) return;
                
                currentWindowData.foundObjects.Clear();
                Light[] lights = Resources.FindObjectsOfTypeAll<Light>();
                
                foreach (var light in lights)
                {
                    if (IsValidSceneObject(light.gameObject))
                    {
                        currentWindowData.foundObjects.Add(light.gameObject);
                    }
                }
                
                currentWindowData.objectListWindowTitle = "灯光";
                
                // 触发窗口大小重新计算
                currentWindowData.needResize = true;
                
                if (currentWindowData.foundObjects.Count == 0)
                {
                    Debug.LogError("未在场景中查找到灯光");
                }
            });
        }
        
        /// <summary>
        /// 查找反射球
        /// </summary>
        private static void FindReflectionProbes()
        {
            ExecuteWithPerformanceMonitoring("查找反射球", () =>
            {
                if (currentWindowData == null) return;
                
                currentWindowData.foundObjects.Clear();
                ReflectionProbe[] probes = Resources.FindObjectsOfTypeAll<ReflectionProbe>();
                
                foreach (var probe in probes)
                {
                    if (IsValidSceneObject(probe.gameObject))
                    {
                        currentWindowData.foundObjects.Add(probe.gameObject);
                    }
                }
                
                currentWindowData.objectListWindowTitle = "反射球";
                
                // 触发窗口大小重新计算
                currentWindowData.needResize = true;
                
                if (currentWindowData.foundObjects.Count == 0)
                {
                    Debug.LogError("未在场景中查找到反射球");
                }
                else if (currentWindowData.foundObjects.Count == 1)
                {
                    Selection.activeObject = currentWindowData.foundObjects[0];
                    currentWindowData.foundObjects.Clear();
                }
            });
        }
        
        /// <summary>
        /// 查找粒子系统
        /// </summary>
        private static void FindParticleSystems()
        {
            ExecuteWithPerformanceMonitoring("查找粒子系统", () =>
            {
                if (currentWindowData == null) return;
                
                currentWindowData.foundObjects.Clear();
                ParticleSystem[] particleSystems = Resources.FindObjectsOfTypeAll<ParticleSystem>();
                
                foreach (var ps in particleSystems)
                {
                    if (IsValidSceneObject(ps.gameObject))
                    {
                        currentWindowData.foundObjects.Add(ps.gameObject);
                    }
                }
                
                currentWindowData.objectListWindowTitle = "特效";
                
                // 触发窗口大小重新计算
                currentWindowData.needResize = true;
                
                if (currentWindowData.foundObjects.Count == 0)
                {
                    Debug.LogError("未在场景中查找到特效");
                }
            });
        }
        
        /// <summary>
        /// 查找隐藏对象
        /// </summary>
        private static void FindHiddenObjects()
        {
            ExecuteWithPerformanceMonitoring("查找隐藏对象", () =>
            {
                if (currentWindowData == null) return;
                
                currentWindowData.foundObjects.Clear();
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                
                foreach (var obj in allObjects)
                {
                    if (IsValidSceneObject(obj) && !obj.activeInHierarchy)
                    {
                        currentWindowData.foundObjects.Add(obj);
                    }
                }
                
                currentWindowData.objectListWindowTitle = "隐藏物体";
                
                // 触发窗口大小重新计算
                currentWindowData.needResize = true;
                
                if (currentWindowData.foundObjects.Count == 0)
                {
                    Debug.LogError("未在场景中查找到隐藏物体");
                }
            });
        }
        
        /// <summary>
        /// 根据名称查找游戏对象
        /// </summary>
        /// <param name="objectName">对象名称</param>
        private static void FindGameObjectByName(string objectName)
        {
            GameObject foundObject = GameObject.Find(objectName);
            if (foundObject != null)
            {
                Selection.activeGameObject = foundObject;
                EditorGUIUtility.PingObject(foundObject);
            }
            else
            {
                // 在所有对象中查找
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name == objectName && IsValidSceneObject(obj))
                    {
                        Selection.activeGameObject = obj;
                        EditorGUIUtility.PingObject(obj);
                        return;
                    }
                }
                
                Debug.LogWarning($"未找到名为 '{objectName}' 的游戏对象");
            }
        }
        
        /// <summary>
        /// 检查是否为有效的场景对象
        /// </summary>
        /// <param name="obj">要检查的游戏对象</param>
        /// <returns>是否为有效的场景对象</returns>
        private static bool IsValidSceneObject(GameObject obj)
        {
            return obj != null && 
                   obj.hideFlags == HideFlags.None && 
                   !EditorUtility.IsPersistent(obj.transform.root.gameObject);
        }
        
        #endregion
    }
}