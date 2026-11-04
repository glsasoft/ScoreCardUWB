using UnityEngine;
using UnityEditor;

namespace ArtTools
{
    public static class MissingReferenceChecker_ScriptableObject
    {
        public static void CheckScriptableObjectReferences(string assetPath, CheckResult checkResult)
        {
            // 尝试加载为ScriptableObject
            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (scriptableObject == null) return; // 不是ScriptableObject，跳过

            // 获取ScriptableObject的类型名称
            string typeName = scriptableObject.GetType().Name;

            // 检查ScriptableObject上的所有序列化字段
            SerializedObject serializedObject = new SerializedObject(scriptableObject);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue == null &&
                    property.objectReferenceInstanceIDValue != 0)
                {
                    // 获取属性的完整路径
                    string propertyPath = property.propertyPath;
                    string nodePath = $"{typeName}/{propertyPath}";

                    // 创建详细的错误描述
                    string errorDescription = $"ScriptableObject '{scriptableObject.name}' ({typeName}) 中，" +
                                              $"属性 '{property.displayName}' 的引用丢失。\n" +
                                              $"属性路径: {propertyPath}\n" +
                                              $"实例ID: {property.objectReferenceInstanceIDValue}";

                    // 引用丢失
                    MissingReference reference = new MissingReference(
                        assetPath,
                        property.displayName,
                        "对象引用",
                        nodePath,
                        errorDescription,
                        typeName,
                        property.objectReferenceInstanceIDValue
                    );

                    checkResult.AddMissingReference("ScriptableObject引用", reference);
                }
            }
        }
    }
}