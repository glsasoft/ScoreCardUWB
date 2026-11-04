using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LetsPiu.Graphics
{
    public class QualityLevelSetter : EditorWindowWithAssetSelector
    {
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Graphics/资产选择器_画质分级设置器")]
        private static void OpenWindow()
        {
            GetWindow<QualityLevelSetter>("画质分级设置器").Show();
        }
        
        // 加入后缀
        private string nameSuffix;
        
        // 更换Shader
        private string shaderPath;
        
        // 更换材质
        private GameObject MaterialSwitchPrefab;
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height));
            
            DrawAssetsGUI();
            if (GUILayout.Button("打开资产选择器")) AssetSelector.OpenWindow();
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.LabelField("1. 重命名", EditorStyles.boldLabel);
            // 为选中对象加入自定义后缀
            nameSuffix = EditorGUILayout.TextField("后缀名", nameSuffix);
            if (GUILayout.Button("加入后缀"))
            {
                if (EditorUtility.DisplayDialog("加入后缀",$"是否确定加入后缀:{nameSuffix}? \n" +
                            "该操作不可撤销", "确认", "取消"))
                AddSuffix();
            }
            
            if (GUILayout.Button("删除后缀"))
            {
                if (EditorUtility.DisplayDialog("加入后缀",$"是否确定加入后缀:{nameSuffix}? \n" +
                                                       "该操作不可撤销", "确认", "取消")) 
                    DelSuffix();
            }
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            shaderPath = EditorGUILayout.TextField("Shader路径", shaderPath);
            EditorGUILayout.LabelField("2. 更换Shader", EditorStyles.boldLabel);
            if (GUILayout.Button("更换Shader"))
            {
                if (EditorUtility.DisplayDialog("更换Shader",
                        $"是否确定为选中材质球更换shader：{shaderPath}? \n" +
                        "该操作不可撤销", "确认", "取消"))
                    SwitchShader();
            }
            

            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.LabelField("3. 更换材质", EditorStyles.boldLabel);
            MaterialSwitchPrefab = (GameObject)EditorGUILayout.ObjectField("MaterialSwitch Prefab", MaterialSwitchPrefab, typeof(GameObject), true);
            if (GUILayout.Button("更换为Asset材质球"))
            {
                if (EditorUtility.DisplayDialog("应用新材质球",
                        $"是否确定为{MaterialSwitchPrefab.name}应用新材质球? \n" +
                        "该操作可撤销（确定应用记得在inspector中override）", "确认", "取消"))
                {
                    SwitchMat();
                }
            }
            
            EditorGUILayout.EndScrollView();
            
        }

        private void AddSuffix()
        {
            foreach (Object asset in assets)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string newName = asset.name + nameSuffix;
                string error = AssetDatabase.RenameAsset(path, newName);
                
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"重命名失败：{asset.name} -> {newName}，错误信息：{error}");
                }
                else
                {
                    Debug.Log($"成功重命名：{asset.name} -> {newName}");
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DelSuffix()
        {
            foreach (Object asset in assets)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string name = asset.name;

                // 如果当前名称不以 nameSuffix 结尾，则跳过
                if (!name.EndsWith(nameSuffix))
                {
                    Debug.Log($"跳过：{name}（不包含后缀“{nameSuffix}”）");
                    continue;
                }

                // 去掉后缀
                string newName = name.Substring(0, name.Length - nameSuffix.Length);
                string error = AssetDatabase.RenameAsset(path, newName);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"重命名失败：{name} -> {newName}，错误信息：{error}");
                }
                else
                {
                    Debug.Log($"成功重命名：{name} -> {newName}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SwitchShader()
        {
            if (shaderPath == null)
            {
                Debug.LogError("未选择shaderPath");
                return;
            }
        }

        private void SwitchMat()
        {
            if (MaterialSwitchPrefab == null)
            {
                Debug.LogError("未选择MaterialSwitchPrefab");
                return;
            }

            if (nameSuffix == null)
            {
                Debug.LogError("未填入nameSuffix");
                return;
            }
            
            Renderer[] renderers = MaterialSwitchPrefab.GetComponentsInChildren<Renderer>(true);
            Dictionary<string, Material> suffixMaterialMap = new();

            foreach (Object asset in assets)
            {
                if (asset is Material mat)
                {
                    string baseName = mat.name.Replace(nameSuffix, "");
                    suffixMaterialMap[baseName] = mat;
                }
            }

            foreach (Renderer renderer in renderers)
            {
                Undo.RecordObject(renderer, "更换材质");
                
                Material[] originalMaterials = renderer.sharedMaterials;
                Material[] newMaterials = new Material[originalMaterials.Length];

                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    Material oldMat = originalMaterials[i];
                    if (oldMat == null) continue;

                    string baseName = oldMat.name;
                    if (suffixMaterialMap.TryGetValue(baseName, out Material newMat))
                    {
                        newMaterials[i] = newMat;
                        Debug.Log($"替换材质：{baseName} -> {newMat.name}");
                    }
                    else
                    {
                        newMaterials[i] = oldMat;
                        Debug.LogWarning($"未找到后缀材质：{baseName + nameSuffix}");
                    }
                }
                
                renderer.sharedMaterials = newMaterials;
                EditorUtility.SetDirty(renderer);
            }
            
            Debug.Log("材质替换完成，请在 Inspector 中记得 Apply prefab override");
        }
        
    }
}
