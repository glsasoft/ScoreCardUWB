using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LetsPiu
{
    public abstract class EditorWindowWithAssetSelector : EditorWindow, ISerializationCallbackReceiver
    {
        // 外部访问用，只读接口
        public IReadOnlyList<Object> assets
        {
            get => _assets;
            set
            {
                // 支持撤销
                Undo.RecordObject(this, "修改选择的资产列表");
                _assets = serializedAssets = new List<Object>(value);
            }
        }
        
        // 资产数据存储
        private static List<Object> _assets = new();
        [SerializeField] private List<Object> serializedAssets; // Unity不支持序列化静态字段，所以需要一个非静态字段作为中介
        
        // ？：专门用于Inspector处理的工具（绘制、撤销、拖转等）
        protected SerializedObject serializedObject { get; private set; }
        private SerializedProperty assetProperty;   // Unity GUI 绘制系统能读/写的对象
        
        private ReorderableList assetList;  
        private bool showAssetList;
        private Vector2 assetListScrollPosition;    // 滚动视图的位置

        protected virtual void Update()
        {
            Repaint();
        }
        
        protected virtual void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            assetProperty = serializedObject.FindProperty(nameof(serializedAssets));
            assetList = new ReorderableList(serializedObject, assetProperty)
            {
                headerHeight = 0,
                elementHeight = EditorGUIUtility.singleLineHeight,
                multiSelect = true,         // 可多选
                drawHeaderCallback = rect => { },
                // 元素背景绘制
                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    if (assetProperty.arraySize == 0) return;
                    // 如果选中，深灰色背景
                    if (active) EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
                    // 如果非法，棕色背景
                    if (!ValidateAsset(assetProperty.GetArrayElementAtIndex(index).objectReferenceValue, out _))
                        EditorGUI.DrawRect(rect, new Color(0.53f, 0.36f, 0.17f));
                    // 键盘焦点，蓝色背景
                    if (focused) EditorGUI.DrawRect(rect, new Color(0.17f, 0.36f, 0.53f));
                },
                // 元素内容绘制
                drawElementCallback = (rect, index, active, focused) =>
                {
                    SerializedProperty property = assetProperty.GetArrayElementAtIndex(index);
                    ValidateAsset(property.objectReferenceValue, out string error);
                    EditorGUI.PropertyField(rect, property, new GUIContent($"Element {index}", error));
                }
            };
        }

        protected void DrawAssetsGUI()
        {
            serializedObject.Update();
            Rect assetListTitleRect = EditorGUILayout.GetControlRect(); // 折叠标题区域
            GUI.SetNextControlName(nameof(assetList));
            // 折叠面板
            showAssetList = EditorGUI.BeginFoldoutHeaderGroup(assetListTitleRect, showAssetList, "资产列表");
            if (showAssetList)
            {
                float height = Mathf.Min(assetList.GetHeight(), 320);
                assetListScrollPosition =
                    EditorGUILayout.BeginScrollView(assetListScrollPosition,
                        GUILayout.Height(height + 10));
                Rect rect = EditorGUILayout.GetControlRect(false, assetList.GetHeight());
                Rect visibleRect = new(rect.x, rect.y + assetListScrollPosition.y, rect.width, height);
                assetList.DoList(rect, visibleRect);    // 绘制命令
                EditorGUILayout.EndScrollView();
            }
            EditorGUI.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties(); // 写回数据

            PerformDragEvent(assetListTitleRect);   // 拖拽检测
            CheckAssetsValidation();                // 合法性剔除

            if (GUILayout.Button("清空列表")) assets = new List<Object>();
        }
        
        // 拖入响应
        protected void PerformDragEvent(Rect assetListRect)
        {
            Event e = Event.current;
            
            
            if (mouseOverWindow != this) return;                    // 拖拽是否作用于当前窗口
            if (!assetListRect.Contains(e.mousePosition)) return;   // 鼠标是否在目标区域内

            switch (e.type)
            {
                case EventType.DragUpdated: //拖入，鼠标没松
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    GUI.FocusControl(nameof(assetList));        // 聚焦列表，防止键盘相应其他控件
                    break;
                }
                case EventType.DragPerform: //鼠标松开
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    DragAndDrop.AcceptDrag();
                    AddAssets(DragAndDrop.objectReferences);    // 加入资源列表
                    GUI.FocusControl(null);               // 清除焦点
                    break;
                }
            }
        }
        
        // 拖拽的数据接收
        protected void AddAssets(Object[] addedAssetsAndFolders)
        {
            List<string> addedFolders = new();
            List<Object> addedAssets = new();
            
            foreach (Object asset in addedAssetsAndFolders)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(path)) continue;
                // 文件夹和资源 区别处理
                if (AssetDatabase.IsValidFolder(path)) addedFolders.Add(path);
                else addedAssets.Add(asset);
            }
            
            // 遍历文件夹资源
            if (addedFolders.Count != 0)
            {
                string[] addedAssetGUIDs = AssetDatabase.FindAssets("", addedFolders.ToArray());
                // 遍历guid；获取路径；加载
                foreach (string guid in addedAssetGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(path)) continue;
                    Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (asset is not null) addedAssets.Add(asset);
                }
            }
            
            // 去重并更新assets
            List<Object> newAssets = new(assets);
            foreach (Object obj in addedAssets)
                if (!newAssets.Contains(obj)) newAssets.Add(obj);
            assets = newAssets;
        }

        private void CheckAssetsValidation()
        {
            int invalidCount = 0;
            foreach (Object asset in assets)
                if (!ValidateAsset(asset, out _)) invalidCount++;

            if (invalidCount != 0)
            {
                EditorGUILayout.HelpBox($"{invalidCount} 个资产存在问题", MessageType.Warning);
                
                if (GUILayout.Button("剔除存在问题的资产"))
                {
                    List<Object> a = new(assets.Count);
                    foreach (Object asset in assets)
                        if (ValidateAsset(asset, out _)) a.Add(asset);
                    assets = a;
                }
            }
        }

        // 虚函数：子类根据具体情况重写
        protected virtual bool ValidateAsset(Object asset, out string error)
        {
            if (!asset)
            {
                error = "资产为空或缺失";
                return false;
            }
            
            error = null;
            return true;
        }
        
        
        // 编辑前同步进来：实际编辑的是可序列化数据
        public void OnBeforeSerialize()
        {
            serializedAssets = _assets;
        }
        // 编辑后同步回去
        public void OnAfterDeserialize()
        {
            _assets = serializedAssets;
        }
    }
}