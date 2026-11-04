using UnityEngine;
using UnityEditor;

namespace ArtTools
{
    public static class MissingReferenceChecker_Material
    {
        public static void CheckMaterialReferences(string materialPath, CheckResult checkResult)
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
                                string texturePath = "";
                                int instanceID = 0;

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
                                                instanceID = texValue.objectReferenceInstanceIDValue;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (foundMissingReference)
                                {
                                    // 创建详细的错误描述
                                    string errorDescription = $"材质球 '{material.name}' 的 '{propertyName}' 贴图引用丢失。";
                                    if (instanceID != 0)
                                    {
                                        errorDescription += $"\n实例ID: {instanceID}";
                                    }

                                    // 添加到丢失引用列表，包含详细信息
                                    MissingReference reference = new MissingReference(
                                        materialPath,
                                        propertyName,
                                        "贴图",
                                        $"Material/{material.name}/{propertyName}", // 节点路径
                                        errorDescription,
                                        "Texture",
                                        instanceID
                                    );

                                    checkResult.AddMissingReference("材质球贴图", reference);
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
    }
}