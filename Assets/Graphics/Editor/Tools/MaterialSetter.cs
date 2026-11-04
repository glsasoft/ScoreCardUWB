using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LetsPiu.Graphics
{
    
    public class MaterialSetter : EditorWindowWithAssetSelector
    {
        private Vector2 scrollPosition;

        private GameObject SourceFbx;
        private GameObject MaterialApplyPrefab;

        private string shaderPath;
        public Object textureFolder; // 存储用户拖入的文件夹
        private List<Texture2D> folderTextures = new List<Texture2D>();

        public Object textureReplaceFolder;

        private string[] shaderOptions = new string[]
        {
            "ShaderTemplate/PBR_Scene_V4",
            "ShaderTemplate/PBR_Scene_V4_Medium",
            "ShaderTemplate/PBR_Character_V4",
            "ShaderTemplate/Toon_Face_V1"
        };
        private int selectedShaderIndex = 0;
        
        
        [MenuItem("Tools/Graphics/资产选择器_材质设置器")]
        private static void OpenWindow()
        {
            GetWindow<MaterialSetter>("材质设置器").Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height));
            DrawAssetsGUI();
            if(GUILayout.Button("打开资产选择器")) AssetSelector.OpenWindow();
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            
            // 01 材质生成
            EditorGUILayout.LabelField("1. 材质生成",EditorStyles.boldLabel);
            SourceFbx = (GameObject)EditorGUILayout.ObjectField("Source Fbx", SourceFbx, typeof(GameObject), false);
            if (GUILayout.Button("根据 Fbx 生成材质球"))
            {
                if (EditorUtility.DisplayDialog("生成材质球", 
                                                    $"为{SourceFbx.name}生成对应材质球\n" +
                                                              $"路径为：{AssetDatabase.GetAssetPath(SourceFbx)}" +
                                                              "该操作无法被撤销,是否确定？", "确认", "取消"))
                {
                    MaterialGenerator();
                    EditorUtility.ClearProgressBar();
                }
            }
            GUILayout.Button("根据 Textures 生成材质球（代办）");
            
            // 02 材质球应用
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.LabelField("2. 材质应用",EditorStyles.boldLabel);
            MaterialApplyPrefab = (GameObject)EditorGUILayout.ObjectField("MaterialApply Prefab", MaterialApplyPrefab, typeof(GameObject), true);
            if (GUILayout.Button("为 Prefab 应用新材质球"))
            {
                if (EditorUtility.DisplayDialog("应用新材质球", 
                        $"为{MaterialApplyPrefab.name}应用新材质球\n" +
                        "该操作可撤销（确定应用记得在inspector中override）", "确认", "取消"))
                {
                    MaterialApply();
                }
            }
            
            // 03 材质贴图设置
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.LabelField("3. 材质属性设置",EditorStyles.boldLabel);
            selectedShaderIndex = EditorGUILayout.Popup("Shader Path", selectedShaderIndex, shaderOptions);
            shaderPath = shaderOptions[selectedShaderIndex];
            
            textureFolder = EditorGUILayout.ObjectField("贴图文件夹", textureFolder, typeof(DefaultAsset), false);
            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button("mat 属性设置 (应用shader和同名tex)"))
            {
                if (EditorUtility.DisplayDialog("mat 属性设置", 
                        $"Shader：{shaderPath} \n" +
                                $"材质：上述材质资产\n", "确认", "取消"))
                {
                    ApplyShaderAndMatchedTextures();
                }
            }
            
            // 04 材质贴图替换
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.LabelField("4. 材质贴图替换",EditorStyles.boldLabel);
            selectedShaderIndex = EditorGUILayout.Popup("Shader Path", selectedShaderIndex, shaderOptions);
            shaderPath = shaderOptions[selectedShaderIndex];
            textureReplaceFolder = EditorGUILayout.ObjectField("贴图文件夹", textureReplaceFolder, typeof(DefaultAsset), false);
            if (GUILayout.Button("mat贴图替换 （替换为文件夹中的同名tex)"))
            {
                if (EditorUtility.DisplayDialog("mat 贴图替换", 
                        $"不可撤销", "确认", "取消"))
                {
                    ReplaceShaderAndMatchedTextures();
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void ReplaceShaderAndMatchedTextures()
        {
            if (textureReplaceFolder == null)
            {
                Debug.LogWarning("请先指定贴图文件夹！");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(textureReplaceFolder);
            string[] texGuids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
            Dictionary<string, Texture> textureDict = new Dictionary<string, Texture>();

            // 1. 加载文件夹中的贴图，并构建字典（去掉后缀部分作为Key）
            foreach (string guid in texGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (tex == null) continue;

                string filename = Path.GetFileNameWithoutExtension(path);
                string baseName = RemoveLastUnderscoreSuffix(filename);
                if (!textureDict.ContainsKey(baseName))
                {
                    textureDict.Add(baseName, tex);
                }
            }

            // 2. 遍历材质并替换贴图
            foreach (var obj in assets)
            {
                if (!(obj is Material mat)) continue;

                Undo.RecordObject(mat, "批量替换贴图");

                Shader shader = Shader.Find(shaderPath);
                if (shader != null) mat.shader = shader;

                int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(mat.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                        continue;

                    string propertyName = ShaderUtil.GetPropertyName(mat.shader, i);
                    Texture oldTex = mat.GetTexture(propertyName);

                    if (oldTex == null) continue;

                    string oldName = oldTex.name;
                    string oldBaseName = RemoveLastUnderscoreSuffix(oldName);

                    if (textureDict.TryGetValue(oldBaseName, out Texture newTex))
                    {
                        mat.SetTexture(propertyName, newTex);
                        Debug.Log($"替换材质 {mat.name} 的贴图 {propertyName}：{oldTex.name} → {newTex.name}");
                    }
                }

                EditorUtility.SetDirty(mat);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // 工具函数：去除最后一个"_"后面的后缀
        private string RemoveLastUnderscoreSuffix(string name)
        {
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore > 0)
            {
                return name.Substring(0, lastUnderscore);
            }
            return name;
        }

        
        private void MaterialGenerator() 
        {
            if (SourceFbx == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择一个有效的 Prefab。", "确定");
                return;
            }

            // 获取 Prefab 的路径
            string prefabPath = AssetDatabase.GetAssetPath(SourceFbx);
            if (string.IsNullOrEmpty(prefabPath))
            {
                EditorUtility.DisplayDialog("错误", "无法获取 Prefab 路径。", "确定");
                return;
            }

            // 获取 Prefab 所在目录
            string folderPath = System.IO.Path.GetDirectoryName(prefabPath);
            string materialsFolderPath = System.IO.Path.Combine(folderPath, "Materials");

            // 创建 Materials 文件夹（如果不存在）
            if (!System.IO.Directory.Exists(materialsFolderPath))
            {
                System.IO.Directory.CreateDirectory(materialsFolderPath);
                AssetDatabase.Refresh(); // 刷新以确保 Unity 认识到新创建的文件夹
            }

            // 收集新创建的材质球
            List<Object> generatedMaterials = new();
            // 获取 Prefab 上的所有渲染器及其材质
            Renderer[] renderers = SourceFbx.GetComponentsInChildren<Renderer>(true);
            int rendererIndex = 0;
            
            foreach (var renderer in renderers)
            {
                int materialIndex = 0;
                foreach (Material sourceMaterial in renderer.sharedMaterials)
                {
                    if (sourceMaterial == null)
                    {
                        Debug.LogWarning($"Renderer on {renderer.name} has a null material at index {materialIndex}. Skipping.");
                        continue;
                    }

                    string materialName = sourceMaterial.name;
                 
                    string materialFileName = $"M_{materialName}.mat";
                    string materialAssetPath = System.IO.Path.Combine(materialsFolderPath, materialFileName);
                    // 转换为 Unity 资产路径（"Assets/.../Materials/M_name.mat"）
                    
                    // 检查是否已经存在同名材质球
                    Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
                    Material newMaterial;

                    if (existingMaterial != null)
                    {
                        // 重复就替换
                        newMaterial = new Material(sourceMaterial); // 创建副本
                        AssetDatabase.CreateAsset(newMaterial, materialAssetPath);
                        Debug.Log($"材质 {materialName} 已替换。");
                    }
                    else
                    {
                        // 创建新材质球
                        newMaterial = new Material(sourceMaterial); // 复制原始材质属性
                        AssetDatabase.CreateAsset(newMaterial, materialAssetPath);
                        Debug.Log($"材质 {materialName} 已创建于 {materialAssetPath}");
                    }
                    generatedMaterials.Add(newMaterial);
                    materialIndex++;
                    
                }
                EditorUtility.DisplayProgressBar("生成材质球中...", $"处理材质: {renderer.name}", (float)rendererIndex / renderers.Length);
                rendererIndex++;
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            assets = generatedMaterials;
            EditorUtility.DisplayDialog("完成", "材质球已生成完毕。", "确认");
        }
        
        private void MaterialApply()
        {
            // 收集材质资产
            Dictionary<string, Material> materialMap = new();
            foreach (Object obj in assets)
            {
                if (obj is Material mat)
                {
                    string key = Regex.Replace(mat.name, @"^M_", "", RegexOptions.IgnoreCase).ToLower();
                    materialMap[key] = mat;
                }
            }
            
            Renderer[] renderers = MaterialApplyPrefab.GetComponentsInChildren<Renderer>(true);
            bool modified = false;

            // 遍历 Renderer，按 Renderer 的 GameObject 名称匹配材质名
            foreach (Renderer renderer in renderers)
            {
                string rendererName = renderer.gameObject.name.ToLower();
                string matchedKey = null;

                foreach (string key in materialMap.Keys)
                {
                    if (rendererName.Contains(key))
                    {
                        matchedKey = key;
                        break;
                    }
                }
                
                if (matchedKey == null)
                    continue;

                Material targetMaterial = materialMap[matchedKey];          // 目标材质
                Material[] originalMaterials = renderer.sharedMaterials;
                Material[] newMaterials = new Material[originalMaterials.Length];
                bool replaced = false;

                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    Material original = originalMaterials[i];
                    // 已是目标材质则跳过
                    if (original == targetMaterial)
                    {
                        newMaterials[i] = original;
                        continue;
                    }

                    newMaterials[i] = targetMaterial;
                    replaced = true;
                }

                if (replaced)
                {
                    Undo.RecordObject(renderer, "替换材质");
                    renderer.sharedMaterials = newMaterials;
                    EditorUtility.SetDirty(renderer);   // 提醒unity保存
                    modified = true;
                }
            }
            if (modified) Debug.Log($"Prefab 实例 {MaterialApplyPrefab.name} 材质替换完成（可撤销）");
            else Debug.LogWarning("没有找到匹配材质，实例未修改。");
        }

        private void ApplyShaderAndMatchedTextures()
        {
            Shader shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("错误", $"找不到 Shader：{shaderPath}", "确定");
                return;
            }
            
            if (textureFolder == null)
            {
                EditorUtility.DisplayDialog("错误", "请先指定贴图文件夹", "确定");
                return;
            }
            
            string folderPath = AssetDatabase.GetAssetPath(textureFolder);
            folderTextures = GetTexturesInFolder(folderPath);
            
            if (folderTextures.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "指定的文件夹中没有找到任何贴图", "确定");
                return;
            }
            
            foreach (Material mat in assets)
            {
                if (mat == null) continue;

                string matName = mat.name.ToLower().Replace("m_", "");
                List<Texture2D> matchedTextures = folderTextures.Where(tex => tex != null && tex.name.ToLower().Contains(matName)).ToList();
                
                if (matchedTextures.Count == 0)
                {
                    Debug.LogWarning($"未找到与材质 [{mat.name}] 匹配的贴图");
                    continue;
                }
                
                Undo.RecordObject(mat, "设置Shader和贴图");
                mat.shader = shader;
                foreach (Texture2D tex in matchedTextures)
                {
                    string texName = tex.name.ToLower();

                    if (texName.Contains("albedo"))
                        mat.SetTexture("_BaseMap", tex);
                    if (texName.Contains("emission"))
                        mat.SetTexture("_EmissionMap", tex);
                    if (texName.Contains("normal"))
                        mat.SetTexture("_NormalMap", tex);
                    if (texName.Contains("mro"))
                        mat.SetTexture("_MaskMap", tex);
                    if(texName.Contains("thickness"))
                        mat.SetTexture("_ThicknessMap", tex);
                }
                EditorUtility.SetDirty(mat);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private List<Texture2D> GetTexturesInFolder(string folderPath)
        {
            List<Texture2D> textures = new List<Texture2D>();

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    textures.Add(tex);
                }
            }
            return textures;
        }
        
        protected override bool ValidateAsset(Object asset, out string error)
        {
            if(!base.ValidateAsset(asset, out error)) return false;

            if (asset is not Material)
            {
                error = "资产类型需要为Material";
                return false;
            }

            error = null;
            return true;
        }
    }
}
