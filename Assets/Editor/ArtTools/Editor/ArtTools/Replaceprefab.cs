using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace ArtTools
{
    public class ObjectReplaceEditorWindow : EditorWindow
    {
        [MenuItem("Tools/替换场景中的物体")]
        public static void Open()
        {
            EditorWindow.GetWindow(typeof(ObjectReplaceEditorWindow));
        }

        public GameObject newPrefab;
        static GameObject tonewPrefab;
        private string replaceName = "";
        private bool isChangeName = false;

        private string FolderPath = "";

        public class ReplaceData
        {
            public GameObject old;
            public GameObject replace;
            public int index = 0;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("选择一个新的物体");
            newPrefab = (GameObject)EditorGUILayout.ObjectField(newPrefab, typeof(GameObject), true,
                GUILayout.MinWidth(100f));
            tonewPrefab = newPrefab;
            //isChangeName = EditorGUILayout.ToggleLeft("不改变场景中物体的名字", isChangeName);
            if (GUILayout.Button("替换选中的物体"))
            {
                ReplaceObjects();
            }
            EditorGUILayout.LabelField("----------------------");
            FolderPath = EditorGUILayout.TextField("Folder Path:", FolderPath);
            //Drag and Drop area
            Event e = Event.current;
            Rect dropArea = GUILayoutUtility.GetLastRect();
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(e.mousePosition)) return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        string[] files = DragAndDrop.paths;
                        if (files.Length > 0 && Directory.Exists(files[0]))
                        {
                            FolderPath = files[0];
                        }
                    }
                    break;
            }
            
            if (GUILayout.Button("替换所选文件夹对应预制体"))
            {
                OnClickFolderReplace();
            }
            
            
            

            EditorGUILayout.LabelField("----------------------");
            replaceName = EditorGUILayout.TextField("需要替换的物体名字", replaceName);
            if (GUILayout.Button("替换相同名字的"))
            {
                ReplaceObjectsByName(replaceName, false);
            }

            if (GUILayout.Button("替换包含名字的 慎用"))
            {
                ReplaceObjectsByName(replaceName, true);
            }

            EditorGUILayout.LabelField("----------------------");
            if (GUILayout.Button("保存修改"))
            {
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }

        private void OnClickFolderReplace()
        {
            Object select = Selection.activeObject;
            if (select == null) return;
            
            if (string.IsNullOrEmpty(FolderPath) || !Directory.Exists(FolderPath)) return;
            //加载所有预制体
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] {FolderPath});
            // Object[] all = AssetDatabase.LoadAllAssetsAtPath(FolderPath);
            Dictionary<string, GameObject> prefabDic = new();
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                
                prefabDic.Add(prefab.name, prefab);
            }
            
            //替换所有选中目标下的物体
            if (select is GameObject s)
            {
                List<ReplaceData> replaceDatas = new List<ReplaceData>();
                List<GameObject> childList = new();
                for (int i = 0; i < s.transform.childCount; i++)
                {
                    Transform child = s.transform.GetChild(i);
                    GameObject temp = child.gameObject;
                    
                    if(!prefabDic.ContainsKey(temp.name)) continue;
                    
                    childList.Add(temp);
                }

                foreach (var child in childList)
                {
                    ReplaceData replaceData = new ReplaceData();
                    replaceData.old = child;
                    ReplaceOne(replaceData, prefabDic[child.name]);
                    replaceDatas.Add(replaceData);
                }
                
                HandleReplaceData(replaceDatas);
            }
            
        }

        void ReplaceObjects()
        {
            if (tonewPrefab == null) return;
            Object[] objects = Selection.objects;
            List<ReplaceData> replaceDatas = new List<ReplaceData>();
            foreach (Object item in objects)
            {
                GameObject temp = (GameObject)item;
                ReplaceData replaceData = new ReplaceData();
                replaceData.old = temp;
                ReplaceOne(replaceData);
                replaceDatas.Add(replaceData);
            }

            HandleReplaceData(replaceDatas);
        }

        void HandleReplaceData(List<ReplaceData> replaceDatas)
        {
            foreach (var replaceData in replaceDatas)
            {
                replaceData.replace.transform.SetSiblingIndex(replaceData.index);
                if (null != replaceData.old && null != replaceData.old.gameObject)
                    DestroyImmediate(replaceData.old.gameObject);
            }
        }

        void ReplaceObjectsByName(string name, bool isContain)
        {
            if (string.IsNullOrEmpty(name)) return;
            List<ReplaceData> replaceDatas = new List<ReplaceData>();
            Transform[] all = Object.FindObjectsOfType<Transform>();
            foreach (var item in all)
            {
                //Debug.LogError(item.name);
                ReplaceData replaceData = new ReplaceData();
                replaceData.old = item.gameObject;
                if (!isContain && item.gameObject.name == name)
                {
                    ReplaceOne(replaceData);
                    replaceDatas.Add(replaceData);
                }
                else if (isContain && item.gameObject.name.Contains(name))
                {
                    ReplaceOne(replaceData);
                    replaceDatas.Add(replaceData);
                }
            }

            HandleReplaceData(replaceDatas);
        }

        public void ReplaceOne(ReplaceData replaceData)
        {
            GameObject replace = (GameObject)PrefabUtility.InstantiatePrefab(tonewPrefab);
            replace.transform.SetParent(replaceData.old.transform.parent);
            replace.transform.localPosition = replaceData.old.transform.localPosition;
            replace.transform.localRotation = replaceData.old.transform.localRotation;
            replace.transform.localScale = replaceData.old.transform.localScale;
            replaceData.replace = replace;
            replaceData.index = replaceData.old.transform.GetSiblingIndex();
        }
        
        public void ReplaceOne(ReplaceData replaceData, GameObject prefab)
        {
            GameObject replace = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            replace.transform.SetParent(replaceData.old.transform.parent);
            replace.transform.localPosition = replaceData.old.transform.localPosition;
            replace.transform.localRotation = replaceData.old.transform.localRotation;
            replace.transform.localScale = replaceData.old.transform.localScale;
            replaceData.replace = replace;
            replaceData.index = replaceData.old.transform.GetSiblingIndex();
        }
    }
}