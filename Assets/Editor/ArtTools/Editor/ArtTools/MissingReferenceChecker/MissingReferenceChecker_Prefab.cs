using UnityEngine;
using UnityEditor;

namespace ArtTools
{
    public static class MissingReferenceChecker_Prefab
    {
        public static void CheckPrefabReferences(string prefabPath, CheckResult checkResult)
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
                    Transform parentTransform = GetParentTransform(components, prefab);
                    string nodePath = GetGameObjectPath(parentTransform.gameObject, prefab);

                    MissingReference reference = new MissingReference(
                        prefabPath,
                        "Missing Script",
                        "组件",
                        nodePath,
                        $"预制体 '{prefab.name}' 的节点 '{nodePath}' 上存在丢失的脚本组件。",
                        "Missing Script",
                        0
                    );

                    checkResult.AddMissingReference("预制体组件", reference);
                    continue;
                }

                // 获取组件所在的游戏对象路径
                string gameObjectPath = GetGameObjectPath(component.gameObject, prefab);

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
                            $"预制体 '{prefab.name}' 的节点 '{gameObjectPath}' 上的组件 '{component.GetType().Name}' 中，" +
                            $"属性 '{property.displayName}' 的引用丢失。\n" +
                            $"属性路径: {propertyPath}\n" +
                            $"实例ID: {property.objectReferenceInstanceIDValue}";

                        // 引用丢失
                        MissingReference reference = new MissingReference(
                            prefabPath,
                            $"{component.GetType().Name}.{property.displayName}",
                            "对象引用",
                            nodePath,
                            errorDescription,
                            component.GetType().Name,
                            property.objectReferenceInstanceIDValue
                        );

                        checkResult.AddMissingReference("预制体引用", reference);
                    }
                }
            }
        }

        // 获取丢失组件的父级Transform
        private static Transform GetParentTransform(Component[] components, GameObject prefab)
        {
            // 这个方法尝试找到丢失组件的父级Transform
            // 由于组件已经丢失，我们只能通过其他方式推断
            // 这里简单返回预制体的根Transform，实际使用时可能需要更复杂的逻辑
            return prefab.transform;
        }

        // 获取游戏对象的完整路径
        private static string GetGameObjectPath(GameObject gameObject, GameObject root)
        {
            if (gameObject == root)
                return gameObject.name;

            string path = gameObject.name;
            Transform parent = gameObject.transform.parent;

            while (parent != null && parent.gameObject != root)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            if (parent != null) // 确保我们找到了根节点
                path = parent.name + "/" + path;

            return path;
        }
    }
}