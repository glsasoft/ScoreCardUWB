using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ArtTools
{
// 用于捕获和处理日志警告的辅助类
    public class LogWarningHandling : IDisposable
    {
        private string scenePath;
        private CheckResult checkResult;

        public LogWarningHandling(string scenePath, CheckResult checkResult)
        {
            this.scenePath = scenePath;
            this.checkResult = checkResult;

            // 设置日志回调来捕获警告
            Application.logMessageReceived += HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            // 只处理警告类型的日志
            if (type == LogType.Warning)
            {
                // 检查是否是关于废弃组件的警告
                if (logString.Contains("is no longer available") ||
                    logString.Contains("will be removed") ||
                    logString.Contains("Component") && logString.Contains("deprecated"))
                {
                    // 尝试从警告中提取组件和GameObject信息
                    string componentName = ExtractComponentName(logString);
                    string gameObjectName = ExtractGameObjectName(logString);

                    if (!string.IsNullOrEmpty(componentName) && !string.IsNullOrEmpty(gameObjectName))
                    {
                        // 创建一个丢失引用记录
                        MissingReference reference = new MissingReference(
                            scenePath,
                            $"{gameObjectName} - Deprecated {componentName}",
                            "废弃组件警告",
                            gameObjectName,
                            $"场景 '{scenePath}' 中的对象 '{gameObjectName}' 上存在已废弃的{componentName}组件，该组件将在编辑并保存场景后被移除。\n原始警告: {logString}",
                            componentName,
                            0
                        );

                        checkResult.AddMissingReference("废弃组件警告", reference);
                    }
                }
            }
        }

        // 从警告消息中提取组件名称
        private string ExtractComponentName(string logString)
        {
            // 尝试匹配 "Component X in" 或 "Component X is" 模式
            Match match = Regex.Match(logString, @"Component\s+([^\s]+)\s+(in|is)");
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        // 从警告消息中提取GameObject名称
        private string ExtractGameObjectName(string logString)
        {
            // 尝试匹配 "in X for" 或 "in X is" 模式
            Match match = Regex.Match(logString, @"in\s+([^\s]+)\s+(for|is)");
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        public void Dispose()
        {
            // 移除我们的日志回调
            Application.logMessageReceived -= HandleLog;
        }
    }

    public static class MissingReferenceChecker_Scene
    {
        public static void CheckSceneReferences(string scenePath, CheckResult checkResult)
        {
            // 检查是否只检查列表内场景，如果是且当前场景不在列表中，则跳过
            if (MissingReferenceSceneConfig.Instance.OnlyCheckListedScenes &&
                !MissingReferenceSceneConfig.Instance.IsSceneInNoPromptList(scenePath))
            {
                Debug.Log($"跳过检查场景: {scenePath}，因为启用了只检查列表内场景选项且该场景不在列表中。");
                return;
            }

            // 保存当前场景路径，以便检查完后恢复
            string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            bool sceneChanged = false;

            // 检查是否在不询问列表中
            bool skipPrompt = MissingReferenceSceneConfig.Instance.EnableAutoCheck &&
                              MissingReferenceSceneConfig.Instance.IsSceneInNoPromptList(scenePath);

            // 如果在不询问列表中，则直接检查；否则显示对话框
            if (skipPrompt ||
                EditorUtility.DisplayDialog("检查场景", $"检查场景 {scenePath} 需要临时打开它，这可能会影响您当前的工作。是否继续？", "继续", "跳过"))
            {
                // 如果不在不询问列表中，询问是否添加到列表
                if (!skipPrompt && MissingReferenceSceneConfig.Instance.EnableAutoCheck)
                {
                    if (EditorUtility.DisplayDialog("添加到自动检查",
                            $"是否将场景 {scenePath} 添加到自动检查列表中？\n添加后，下次检查此场景时将不再询问。", "添加", "取消"))
                    {
                        MissingReferenceSceneConfig.Instance.AddSceneToNoPromptList(scenePath);
                    }
                }

                // 保存当前场景
                if (!string.IsNullOrEmpty(currentScenePath))
                {
                    sceneChanged = true;
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }

                try
                {
                    // 打开要检查的场景
                    // 使用LogWarningHandling来捕获和处理废弃组件的警告
                    using (new LogWarningHandling(scenePath, checkResult))
                    {
                        // 如果是自动检查的场景，记录日志
                        if (MissingReferenceSceneConfig.Instance.EnableAutoCheck &&
                            MissingReferenceSceneConfig.Instance.IsSceneInNoPromptList(scenePath))
                        {
                            Debug.Log($"自动检查场景: {scenePath}");
                        }

                        UnityEngine.SceneManagement.Scene scene =
                            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                        // 获取场景中的所有GameObject
                        GameObject[] rootObjects = scene.GetRootGameObjects();
                        foreach (GameObject rootObject in rootObjects)
                        {
                            // 检查所有组件，包括可能已废弃的组件
                            CheckGameObjectHierarchy(rootObject, scenePath, checkResult);

                            // 检查是否有GUI Layer等已废弃的组件
                            CheckDeprecatedComponents(rootObject, scenePath, checkResult);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // 记录错误但不中断检查过程
                    Debug.LogWarning($"检查场景 {scenePath} 时出现异常: {ex.Message}");

                    // 将异常也添加到检查结果中
                    MissingReference reference = new MissingReference(
                        scenePath,
                        "场景加载异常",
                        "场景错误",
                        "场景根节点",
                        $"检查场景 '{scenePath}' 时出现异常: {ex.Message}",
                        "Exception",
                        0
                    );

                    checkResult.AddMissingReference("场景错误", reference);

                    // 如果场景加载异常，询问是否要从自动检查列表中移除
                    if (MissingReferenceSceneConfig.Instance.IsSceneInNoPromptList(scenePath))
                    {
                        if (EditorUtility.DisplayDialog("场景加载异常",
                                $"场景 {scenePath} 加载异常，是否要从自动检查列表中移除？", "移除", "保留"))
                        {
                            MissingReferenceSceneConfig.Instance.RemoveSceneFromNoPromptList(scenePath);
                        }
                    }
                }
                finally
                {
                    // 无论是否出现异常，都尝试恢复之前的场景
                    if (sceneChanged && !string.IsNullOrEmpty(currentScenePath))
                    {
                        try
                        {
                            EditorSceneManager.OpenScene(currentScenePath);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"恢复场景 {currentScenePath} 时出现异常: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static void CheckGameObjectHierarchy(GameObject gameObject, string scenePath, CheckResult checkResult)
        {
            // 获取游戏对象的路径
            string gameObjectPath = GetGameObjectPath(gameObject);

            // 检查当前游戏对象上的所有组件
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null)
                {
                    // 组件本身丢失
                    MissingReference reference = new MissingReference(
                        scenePath,
                        $"{gameObject.name} - Missing Script",
                        "组件",
                        gameObjectPath,
                        $"场景 '{scenePath}' 中的节点 '{gameObjectPath}' 上存在丢失的脚本组件。",
                        "Missing Script",
                        0
                    );

                    checkResult.AddMissingReference("场景组件", reference);
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
                        // 获取属性的完整路径
                        string propertyPath = property.propertyPath;
                        string nodePath = $"{gameObjectPath}/{component.GetType().Name}/{propertyPath}";

                        // 创建详细的错误描述
                        string errorDescription =
                            $"场景 '{scenePath}' 中的节点 '{gameObjectPath}' 上的组件 '{component.GetType().Name}' 中，" +
                            $"属性 '{property.displayName}' 的引用丢失。\n" +
                            $"属性路径: {propertyPath}\n" +
                            $"实例ID: {property.objectReferenceInstanceIDValue}";

                        // 引用丢失
                        MissingReference reference = new MissingReference(
                            scenePath,
                            $"{gameObject.name}.{component.GetType().Name}.{property.displayName}",
                            "对象引用",
                            nodePath,
                            errorDescription,
                            component.GetType().Name,
                            property.objectReferenceInstanceIDValue
                        );

                        checkResult.AddMissingReference("场景引用", reference);
                    }
                }
            }

            // 递归检查子对象
            foreach (Transform child in gameObject.transform)
            {
                CheckGameObjectHierarchy(child.gameObject, scenePath, checkResult);
            }
        }

        // 获取游戏对象的完整路径
        private static string GetGameObjectPath(GameObject gameObject)
        {
            string path = gameObject.name;
            Transform parent = gameObject.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        // 检查废弃的组件
        private static void CheckDeprecatedComponents(GameObject gameObject, string scenePath, CheckResult checkResult)
        {
            // 检查是否有已废弃的组件
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                // 检查空组件（可能是Missing Script）
                if (component == null)
                    continue;

                string componentTypeName = component.GetType().Name;

                // 检查已知的废弃组件类型
                if (IsDeprecatedComponentType(componentTypeName))
                {
                    // 记录废弃的组件
                    string gameObjectPath = GetGameObjectPath(gameObject);
                    MissingReference reference = new MissingReference(
                        scenePath,
                        $"{gameObject.name} - Deprecated {componentTypeName}",
                        "废弃组件",
                        gameObjectPath,
                        $"场景 '{scenePath}' 中的节点 '{gameObjectPath}' 上存在已废弃的{componentTypeName}组件，该组件将在编辑并保存场景后被移除。",
                        componentTypeName,
                        0
                    );

                    checkResult.AddMissingReference("废弃组件", reference);
                }
            }

            // 递归检查子对象
            foreach (Transform child in gameObject.transform)
            {
                CheckDeprecatedComponents(child.gameObject, scenePath, checkResult);
            }
        }

        // 判断组件类型是否已废弃
        private static bool IsDeprecatedComponentType(string componentTypeName)
        {
            // 已知的废弃组件类型列表
            string[] deprecatedTypes = new string[]
            {
                "GUILayer", // Unity 2019.3后已废弃
                "NetworkView", // Unity 2018后已废弃
                "Tree Collider", // 已废弃
                "NavMeshAgent", // 已被新版本替代
                "WebCamTexture" // 在某些平台已废弃
            };

            foreach (string type in deprecatedTypes)
            {
                if (componentTypeName == type)
                    return true;
            }

            return false;
        }
    }
}