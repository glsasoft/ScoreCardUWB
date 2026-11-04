using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LetsPiu
{
    public static class UIPrefabCreator
    {

        // ScrollView
        const string ms_FixedScrollViewPrefab = "Assets/Package_Initial/PanleUI/Component/FixedScrollView.prefab";

        [MenuItem("GameObject/UI/FixedScrollView", false, 2120)]
        public static void AddFixedScrollView(MenuCommand menuCommand)
        {
            CreatePrefabInstance(ms_FixedScrollViewPrefab, menuCommand.context as GameObject);
        }

        public static GameObject CreatePrefabInstance(string prefabPath, GameObject parentObject)
        {
            string assetRootPath = System.IO.Path.GetFullPath(Application.dataPath);
            string prefabAssetPath = "Assets" + System.IO.Path.GetFullPath(prefabPath).Substring(assetRootPath.Length).Replace("\\", "/");
            GameObject assetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabAssetPath) as GameObject;
            if (assetObject == null)
            {
                return null;
            }

            // 创建和Prefab对象没有关联的GameObject实例
            GameObject prefabClone = PrefabUtility.InstantiatePrefab(assetObject) as GameObject;
            PrefabUtility.ReconnectToLastPrefab(prefabClone);
            GameObject prefabInstance = GameObject.Instantiate(prefabClone);
            prefabInstance.name = prefabClone.name;
            GameObject.DestroyImmediate(prefabClone);

            // 绑定到了父节点
            if (parentObject != null)
            {
                SetParentAndAlign(prefabInstance, parentObject);
            }
            return prefabInstance;
        }

        static void SetParentAndAlign(GameObject child, GameObject parent, bool isWorldPositionStays = false)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, isWorldPositionStays);
            SetLayerRecursively(child, parent.layer);
        }

        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }
}
