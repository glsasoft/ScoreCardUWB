using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Presets;

namespace ExpandParticleSystem
{
    public class EditorParticlePreset : EditorWindow
    {
        private static EditorParticlePreset window;
        public static ParticlePresetClass presetClass;
        private Vector2 scrollPosition = Vector2.zero;//预设列表滚动位置
        [MenuItem("ArtTools/VFX/粒子预设")]
        static void Window()
        {
            window = (EditorParticlePreset)GetWindow(typeof(EditorParticlePreset), false, "粒子拓展预设设置", true);
            window.Show();
        }
        private void OnEnable()
        {
            string path = Application.dataPath + "/Resources";
            if(Directory.Exists(path))
            {
                CreatePreset();
            }
            else
            {
                //文件不存在
                Directory.CreateDirectory(path);
                CreatePreset();
            }
          
        }
        ///创建预设集合
        private void CreatePreset()
        {
            string path_Create = @"Assets/Resources/" + "ParticlePresetClass" + ".asset";
            if (File.Exists(path_Create))
            {
                presetClass = Resources.Load(path_Create.GetGameObjectResourcePath()) as ParticlePresetClass;
            }
            else
            {
                //创建需要加载的预设集合
                ParticlePresetClass level = ScriptableObject.CreateInstance<ParticlePresetClass>();
                AssetDatabase.CreateAsset(level, path_Create);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                presetClass = Resources.Load(path_Create.GetGameObjectResourcePath()) as ParticlePresetClass;

            }
        }
        private void OnGUI()
        {
             EditorGUILayout.ObjectField("粒子预设集合", presetClass, typeof(ParticlePresetClass), false);
            //总开关：控制粒子系统 Inspector 上显不显示扩展面板
            using (new GUILayout.HorizontalScope())
            {
                bool panelEnabled = ExpandParticleSettings.PanelEnabled;
                bool newPanelEnabled = EditorGUILayout.ToggleLeft(new GUIContent("在粒子系统上显示扩展面板", "关掉后粒子系统组件下只显示 Unity 原生面板"), panelEnabled);
                if (newPanelEnabled != panelEnabled)
                {
                    ExpandParticleSettings.PanelEnabled = newPanelEnabled;
                }
                GUILayout.FlexibleSpace();
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("刷新预览", "清掉缩略图缓存，按当前材质重新渲染"), GUILayout.Width(80)))
                {
                    PlanePreview.Clear();
                    Repaint();
                }
                GUILayout.FlexibleSpace();
            }
            //预设多了整页滚动
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            ShowObject();
            EditorGUILayout.EndScrollView();
        }
        private void ShowObject()
        {
            GUILayout.Label("自定义预设");
            presetClass.EnsureMaterialList();
            ParticleSystem selectedParticle = GetSelectedParticleSystem();
            for (int i = 0; i < presetClass.presets.Count; i++)
            {
                using (new GUILayout.VerticalScope("box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        //左边材质缩略图，右边预设字段占满剩余宽度
                        Texture preview = GetPresetPreview(i);
                        if (preview != null)
                        {
                            GUILayout.Label(preview, GUILayout.Width(40), GUILayout.Height(40));
                        }
                        else
                        {
                            GUILayout.Space(40);
                        }
                        EditorGUI.BeginChangeCheck();
                        presetClass.presets[i] = (Preset)EditorGUILayout.ObjectField(presetClass.presets[i], typeof(Preset), false);
                        if (EditorGUI.EndChangeCheck() && presetClass.presets[i] != null)
                        {
                            if(!presetClass.presets[i].GetTargetTypeName().Equals("ParticleSystem"))
                            {
                                presetClass.presets[i] = null;
                                EditorUtility.DisplayDialog("警告", "这个不是粒子预设", "关闭");
                            }
                        }
                    }
                    //材质每个占一行，按窗口宽度拉伸
                    ShowMaterials(i);
                    PresetMaterialSet set = i < presetClass.presetMaterials.Count ? presetClass.presetMaterials[i] : null;
                    int materialCount = (set == null || set.materials == null) ? 0 : set.materials.Length;
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(selectedParticle == null))
                        {
                            if (GUILayout.Button(new GUIContent("记材质(" + materialCount + ")", "把当前选中粒子的渲染器材质记录到这个预设")))
                            {
                                presetClass.CaptureMaterials(i, selectedParticle);
                                EditorUtility.SetDirty(presetClass);
                            }
                        }
                        if (GUILayout.Button(new GUIContent("清空材质", "清掉这个预设记录的材质"), GUILayout.Width(72)))
                        {
                            presetClass.presetMaterials[i] = null;
                            EditorUtility.SetDirty(presetClass);
                        }
                        if (GUILayout.Button("", new GUIStyle("OL Plus"), GUILayout.Width(22)))
                        {
                            presetClass.presets.Add(null);
                            presetClass.presetMaterials.Add(null);
                        }
                        if (GUILayout.Button("", new GUIStyle("OL Minus"), GUILayout.Width(22)))
                        {
                            presetClass.presets.RemoveAt(i);
                            if (i < presetClass.presetMaterials.Count)
                            {
                                presetClass.presetMaterials.RemoveAt(i);
                            }
                            if (presetClass.presets.Count == 0)
                            {
                                presetClass.presets.Add(null);
                                presetClass.EnsureMaterialList();
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 显示某个预设记录的材质，每个材质占一行
        /// </summary>
        /// <param name="index">预设下标</param>
        private void ShowMaterials(int index)
        {
            if (index >= presetClass.presetMaterials.Count)
            {
                return;
            }
            PresetMaterialSet set = presetClass.presetMaterials[index];
            if (set == null || set.materials == null)
            {
                return;
            }
            for (int m = 0; m < set.materials.Length; m++)
            {
                set.materials[m] = (Material)EditorGUILayout.ObjectField(set.materials[m], typeof(Material), false);
            }
        }
        /// <summary>
        /// 预设的缩略图，取它记录的第一个材质的预览图
        /// </summary>
        /// <param name="index">预设下标</param>
        /// <returns>材质预览图，没有材质时返回 null</returns>
        private Texture GetPresetPreview(int index)
        {
            if (presetClass == null || presetClass.presetMaterials == null || index >= presetClass.presetMaterials.Count)
            {
                return null;
            }
            PresetMaterialSet set = presetClass.presetMaterials[index];
            if (set == null || set.materials == null || set.materials.Length == 0 || set.materials[0] == null)
            {
                return null;
            }
            Material material = set.materials[0];
            return PlanePreview.Get(material);
        }
        /// <summary>
        /// 当前选中的粒子系统，用来记录材质
        /// </summary>
        /// <returns>选中对象上的粒子系统，没有就返回 null</returns>
        private ParticleSystem GetSelectedParticleSystem()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                return null;
            }
            return selected.GetComponent<ParticleSystem>();
        }
    }

}
