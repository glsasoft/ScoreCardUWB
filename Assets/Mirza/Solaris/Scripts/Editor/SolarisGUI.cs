using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using System.Linq;

namespace Mirza.Solaris
{
    public class SolarisGUI : ShaderGUI
    {
        Material material; Shader shader;

        readonly Dictionary<string, bool> foldoutStates = new();

        ShaderGUIData shaderGUIData;
        const string shaderGUIDataAssetPath = "Assets/Mirza/Solaris/Scripts/Editor/Data/Solaris ShaderGUI Data.asset";

        MaterialProperty[] emptyProperties = new MaterialProperty[0];

        // Draw a title box with a title and description text.

        void DrawTitle()
        {
            GUIStyle titleStyle = new(EditorStyles.helpBox)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,

                alignment = TextAnchor.MiddleCenter,

                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(8, 8, 8, 8)
            };

            GUIStyle descriptionStyle = new(EditorStyles.label)
            {
                fontSize = 12,

                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,

                padding = new RectOffset(8, 8, 8, 8)
            };

            // Render.

            EditorGUILayout.BeginVertical(titleStyle);
            {
                GUILayout.Label(shaderGUIData.title, titleStyle);
                GUILayout.Label(shaderGUIData.description, descriptionStyle);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
        }

        // Get (search) header data by name.

        string GetDataForHeader(string name)
        {
            if (shaderGUIData == null || shaderGUIData.tooltips == null)
            {
                return string.Empty;
            }

            foreach (var data in shaderGUIData.tooltips)
            {
                if (data.name == name)
                {
                    return data.text;
                }
            }

            return string.Empty;
        }

        // Custom foldout style.

        GUIStyle m_DarkFoldoutHeaderStyle;
        GUIStyle darkFoldoutHeaderStyle
        {
            get
            {
                if (m_DarkFoldoutHeaderStyle == null)
                {
                    m_DarkFoldoutHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader)
                    {
                        //normal = { background = EditorGUIUtility.Load("builtin skins/darkskin/images/box.png") as Texture2D },

                        fontStyle = FontStyle.Bold,

                        border = new RectOffset(16, 8, 20, 20),
                        margin = new RectOffset(0, 0, 4, 4),
                        padding = new RectOffset(24, 8, 0, 0),

                        // Retain foldout arrow behavior w/ customized background.

                        normal = EditorStyles.foldout.normal,
                        focused = EditorStyles.foldout.focused,
                        hover = EditorStyles.foldout.hover,
                        active = EditorStyles.foldout.active,

                        onNormal = EditorStyles.foldout.onNormal,
                        onFocused = EditorStyles.foldout.onFocused,
                        onHover = EditorStyles.foldout.onHover,
                        onActive = EditorStyles.foldout.onActive,
                    };
                }
                return m_DarkFoldoutHeaderStyle;
            }
        }

        // Render help box for a header if it exists, return the text.

        string TryRenderHeaderHelpBox(string header)
        {
            string text = GetDataForHeader(header);

            if (!string.IsNullOrEmpty(text))
            {
                EditorGUILayout.HelpBox(text, MessageType.None);
            }

            return text;
        }

        // Check if property has different values across materials.

        bool HasDifferentValues(MaterialProperty property, Material[] materials)
        {
            object firstValue = GetPropertyValue(property, materials[0]);

            for (int i = 1; i < materials.Length; i++)
            {
                if (!Equals(firstValue, GetPropertyValue(property, materials[i])))
                {
                    // Found a difference.

                    return true;
                }
            }

            // All values are the same.

            return false;
        }

        // Check if property has same values across materials.

        bool HasSameValues(MaterialProperty property, Material[] materials)
        {
            object firstValue = GetPropertyValue(property, materials[0]);

            for (int i = 1; i < materials.Length; i++)
            {
                if (!Equals(firstValue, GetPropertyValue(property, materials[i])))
                {
                    // Found a difference.

                    return false;
                }
            }

            // All values are the same.

            return true;
        }

        // Get the value of property for a specific material.

        object GetPropertyValue(MaterialProperty property, Material material)
        {
            switch (property.type)
            {
                case MaterialProperty.PropType.Color:
                    return material.GetColor(property.name);
                case MaterialProperty.PropType.Vector:
                    return material.GetVector(property.name);
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    return material.GetFloat(property.name);
                case MaterialProperty.PropType.Texture:
                    return material.GetTexture(property.name);
                case MaterialProperty.PropType.Int:
                    return material.GetInt(property.name);
                default:
                    return null;
            }
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            material = materialEditor.target as Material;
            shader = material.shader;

            // Load GUI data.

            shaderGUIData = AssetDatabase.LoadAssetAtPath<ShaderGUIData>(shaderGUIDataAssetPath);

            // Draw title box.

            DrawTitle();

            // Get selected materials.

            Material[] selectedMaterials = materialEditor.targets.OfType<Material>().ToArray();
            bool multipleMaterialsSelected = selectedMaterials.Length > 1;

            // Dropdown for filtering options.

            string[] filterOptions = { "全部", "仅相同", "仅不同" };
            int selectedFilter = EditorPrefs.GetInt("ProceduralFireMaterialFilterOption", 0);

            if (multipleMaterialsSelected) // Only show filter dropdown if multiple materials selected.
            {
                EditorGUI.BeginChangeCheck();
                {
                    selectedFilter = EditorGUILayout.Popup("筛选属性", selectedFilter, filterOptions);

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetInt("ProceduralFireMaterialFilterOption", selectedFilter);
                    }
                }
            }

            // Expand/Collapse all foldouts button.

            else
            {

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("全部展开"))
                    {
                        foreach (var foldout in foldoutStates.Keys.ToList())
                        {
                            foldoutStates[foldout] = true;
                            EditorPrefs.SetBool(foldout, true);
                        }
                    }

                    if (GUILayout.Button("全部折叠"))
                    {
                        foreach (var foldout in foldoutStates.Keys.ToList())
                        {
                            foldoutStates[foldout] = false;
                            EditorPrefs.SetBool(foldout, false);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            // Iterate over all properties.

            bool insideFoldout = false;
            string currentFoldoutName = string.Empty;

            Color originalColor = GUI.backgroundColor;

            foreach (MaterialProperty property in properties)
            //for (int i = 0; i < properties.Length; i++)
            {
                //MaterialProperty property = properties[i];

                // Apply filtering when multiple materials selected, based on the selected option.

                if (multipleMaterialsSelected)
                {
                    // -- Show only same.

                    if (selectedFilter == 1 && !HasSameValues(property, selectedMaterials))
                    {
                        continue;
                    }

                    // -- Show only different.

                    else if (selectedFilter == 2 && !HasDifferentValues(property, selectedMaterials))
                    {
                        continue;
                    }
                }

                // If multiple materials selected and filtering, don't show foldouts.

                bool multipleMaterialsSelectedAndFiltering = multipleMaterialsSelected && selectedFilter > 0;

                if (multipleMaterialsSelectedAndFiltering)
                {
                    insideFoldout = false;
                }
                else
                {
                    // Read property attributes.
                    // Headers can also have info boxes, without being a foldout.

                    // This is optional, since it doesn't look that good...

                    int propertyIndex = shader.FindPropertyIndex(property.name);
                    string[] attributes = shader.GetPropertyAttributes(propertyIndex);

                    foreach (string attribute in attributes)
                    {
                        //if (attribute.StartsWith("Tooltip"))
                        //{
                        //    string tooltip = attribute.Replace("Tooltip(", string.Empty).Replace(")", string.Empty);
                        //}

                        if (attribute.StartsWith("Header"))
                        {
                            string header = attribute.Replace("Header(", string.Empty).Replace(")", string.Empty);

                            // Only render help box if header is not a foldout, since foldouts have their own help boxes.

                            if (!foldoutStates.ContainsKey(header))
                            {
                                TryRenderHeaderHelpBox(header);
                            }
                        }
                    }

                    // Check for start of a foldout.

                    if (property.name.StartsWith("_StartFoldout"))
                    {
                        insideFoldout = true;

                        // Extract foldout name from property name.

                        EditorGUILayout.Space(10);

                        currentFoldoutName = property.displayName.Replace("开始折叠-", string.Empty);

                        if (!foldoutStates.ContainsKey(currentFoldoutName))
                        {
                            // Default to expanded if bool not found.

                            foldoutStates[currentFoldoutName] = EditorPrefs.GetBool(currentFoldoutName, true);
                        }

                        string tooltip = GetDataForHeader(currentFoldoutName);
                        //string tooltip = TryRenderHeaderHelpBox(currentFoldoutName);

                        GUIContent foldoutLabel = new(currentFoldoutName, tooltip);

                        //GUIContent foldoutLabel = new(currentFoldoutName, string.Empty);

                        // Render foldout.

                        EditorGUI.BeginChangeCheck();
                        {
                            GUI.backgroundColor = originalColor * 0.8f;
                            {
                                foldoutStates[currentFoldoutName] = EditorGUILayout.Foldout(foldoutStates[currentFoldoutName], foldoutLabel, true, darkFoldoutHeaderStyle);
                            }
                            GUI.backgroundColor = originalColor;

                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetBool(currentFoldoutName, foldoutStates[currentFoldoutName]);
                            }
                        }

                        continue;
                    }

                    // Check for end of foldout.

                    if (property.name.StartsWith("_EndFoldout"))
                    {
                        insideFoldout = false;

                        continue;
                    }
                }

                // Skip hidden properties.

                if (property.flags == MaterialProperty.PropFlags.HideInInspector || property.name.StartsWith("unity_"))
                {
                    continue;
                }

                // Render if outside a foldout or if foldout is expanded.

                if (!insideFoldout || (foldoutStates.ContainsKey(currentFoldoutName) && foldoutStates[currentFoldoutName]))
                {
                    materialEditor.ShaderProperty(property, property.displayName);
                }

            }

            base.OnGUI(materialEditor, emptyProperties);
        }
    }
}