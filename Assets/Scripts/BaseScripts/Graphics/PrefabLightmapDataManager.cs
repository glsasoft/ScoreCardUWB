using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Graphics
{
    public class PrefabLightmapDataManager : MonoBehaviour
    {
        
        public List<PrefabLightmapData> data = new();
        // 当前激活的贴图配置
        private PrefabLightmapData currentData;

        private void Awake()
        {
            if (!SceneLightmapDataManager.Instance)
                DontDestroyOnLoad(new GameObject("Scene Lightmap Data Manager", typeof(SceneLightmapDataManager)));
            
            if(data.Count > 0)
                Load(data[0]);
        }
        
        private void Start()
        {
            // if(data.Count > 0)
            //     Load(data[0]);
        }

        // 加载
        public void Load(PrefabLightmapData d)
        {
            // 如果有光照配置，先释放再加载
            if (currentData is not null) currentData.Unload();
            currentData = d;
            if (currentData is not null) currentData.Load();
        }

        // 销毁
        public void OnDestroy()
        {
            if (currentData is not null) currentData.Unload();
        }

        // Unity编辑器扩展部分
#if UNITY_EDITOR
        [CustomEditor(typeof(PrefabLightmapDataManager))]
        private class PrefabLightmapDataManagerEditor : Editor
        {
            // 缓存编辑器状态
            private class EditorBuffer
            {
                public bool showTools = true;
                public string newLightmapName = "New Lightmap";
            }

            private static readonly Dictionary<PrefabLightmapDataManager, EditorBuffer> buffers = new();
            
            private PrefabLightmapDataManager manager;
            private EditorBuffer buffer;

            private void OnEnable()
            {
                manager = (PrefabLightmapDataManager)target;
                
                if (!buffers.TryGetValue(manager, out buffer))
                {
                    buffer = new EditorBuffer();
                    buffers.Add(manager, buffer);
                }
            }

            // Inspector 面板
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                // 创建一个带标题的可展开折叠区域 - Foldout
                buffer.showTools = EditorGUILayout.BeginFoldoutHeaderGroup(buffer.showTools, "Tools");
                
                if (buffer.showTools)
                {
                    // 遍历data列表，每个date对象表示一组光照贴图信息
                    for (int i = 0; i < manager.data.Count; i++)
                    {
                        PrefabLightmapData data = manager.data[i];
                        
                        EditorGUILayout.BeginHorizontal();
                        // 显示配置名称
                        EditorGUILayout.PrefixLabel(string.IsNullOrWhiteSpace(data.name) ? $"Element {i}" : data.name);
                        
                        // 按钮 - 加载
                        if (GUILayout.Button("加载"))
                        {   
                            // 记录整个GameObject层级的Undo
                            Undo.RegisterFullObjectHierarchyUndo(manager.gameObject, "加载光照贴图");
                            // 加载 lightmap
                            manager.Load(data);
                            // 标记对象 - 需要保存
                            EditorUtility.SetDirty(manager.gameObject);
                        }
                        
                        // 按钮 - 覆盖
                        if (GUILayout.Button("覆盖")) 
                        {
                            // 记录 manager 改动
                            Undo.RecordObject(manager, "覆盖光照贴图");
                            // 保存
                            data.Save(manager.gameObject);
                            EditorUtility.SetDirty(manager);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    // 输入框 TextField
                    buffer.newLightmapName = EditorGUILayout.TextField(GUIContent.none, buffer.newLightmapName,
                        GUILayout.Width(EditorGUIUtility.labelWidth - 3));
                    if (GUILayout.Button("创建并保存"))
                    {
                        Undo.RecordObject(manager, "创建并保存光照贴图");
                        PrefabLightmapData data = new() { name = buffer.newLightmapName };
                        data.Save(manager.gameObject);
                        manager.data.Add(data);
                        EditorUtility.SetDirty(manager);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
#endif
    }
}