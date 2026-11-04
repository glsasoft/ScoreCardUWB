using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    [DisallowMultipleComponent]
    public class AssetNode : MonoBehaviour
    {
        private static uint startId = 0;
        private static Dictionary<uint, HashSet<HandleBase>> handleMap = new Dictionary<uint, HashSet<HandleBase>>();
        private static Dictionary<uint, HashSet<AssetNode>> nodeMap = new Dictionary<uint, HashSet<AssetNode>>();
        private static Dictionary<uint, HashSet<HandleBase>> inactiveHandleMap = new Dictionary<uint, HashSet<HandleBase>>();
        private static Dictionary<HandleBase, uint> allHandleIdMap = new Dictionary<HandleBase, uint>();

        public static void AddHandle(HandleBase handle, GameObject attachedObj)
        {
            if (handle == null)
                return;

            if (attachedObj == null)
            {
                // 资源句柄释放可以减少该资源的引用计数，当资源引用计数为零的时候，可以使用资源卸载方法卸载资源包。
                handle.Dispose();
                Debug.LogError(handle.GetType().Name + " Address: " + handle.GetAssetInfo().Address + " 附加的GameObject为Null");
                return;
            }

            AssetNode node = attachedObj.GetComponent<AssetNode>();
            if (node == null)
            {
                node = attachedObj.AddComponent<AssetNode>();
            }

            if (allHandleIdMap.TryGetValue(handle, out uint existId) && node.m_AssetsId > 0 && existId != node.m_AssetsId)
            {
                Debug.LogError(handle.GetType().Name + " Address: " + handle.GetAssetInfo().Address + " 不能附加到两个分组之中，组一: " + existId + " 组二: " + node.m_AssetsId);
                return;
            }

            if (node.m_AssetsId == 0)
            {
                if (existId > 0)
                    node.m_AssetsId = existId;
                else
                    node.m_AssetsId = ++startId;
            }

            allHandleIdMap[handle] = node.m_AssetsId;

            if (!node.m_IsAwaked)
            {
                if (existId > 0)
                    return;

                if (!inactiveHandleMap.TryGetValue(node.m_AssetsId, out var inactiveHandles) || inactiveHandles == null)
                {
                    inactiveHandles = new HashSet<HandleBase>();
                    inactiveHandleMap[node.m_AssetsId] = inactiveHandles;
                }
                inactiveHandles.Add(handle);

                Debug.LogWarning(handle.GetType().Name + " Address: " + handle.GetAssetInfo().Address + " 附加到了未激活的GameObject");
                return;
            }

            if (!handleMap.TryGetValue(node.m_AssetsId, out var handles) || handles == null)
            {
                handles = new HashSet<HandleBase>();
                handleMap[node.m_AssetsId] = handles;
            }
            handles.Add(handle);

            node.Awake();
        }

        public static void DisposeHandle(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            AssetNode node = obj.GetComponent<AssetNode>();
            if (node == null)
            {
                return;
            }
            if (!node.m_IsAwaked)
            {
                ReleaseHandles(inactiveHandleMap, node.m_AssetsId);
            }

            Object.DestroyImmediate(node);
        }

        private static void ReleaseHandles(Dictionary<uint, HashSet<HandleBase>> param_handleMap, uint assetId)
        {
            if (param_handleMap.TryGetValue(assetId, out var handles) && handles != null)
            {
                foreach (HandleBase handle in handles)
                {
                    // 资源句柄释放可以减少该资源的引用计数，当资源引用计数为零的时候，可以使用资源卸载方法卸载资源包。
                    handle.Dispose();
                    allHandleIdMap.Remove(handle);
                }
            }
            param_handleMap.Remove(assetId);
        }


        [HideInInspector]
        [SerializeField]
        private uint m_AssetsId = 0;

        private bool m_IsAwaked = false;

        private void Awake()
        {
            //this.hideFlags = HideFlags.HideInInspector;
            this.hideFlags = HideFlags.NotEditable;

            m_IsAwaked = true;

            if (m_AssetsId > 0)
            {
                if (inactiveHandleMap.TryGetValue(m_AssetsId, out var inactiveHandles) && inactiveHandles != null)
                {
                    if (!handleMap.TryGetValue(m_AssetsId, out var handles) || handles == null)
                    {
                        handles = new HashSet<HandleBase>();
                        handleMap[m_AssetsId] = handles;
                    }
                    handles.UnionWith(inactiveHandles);
                    inactiveHandleMap.Remove(m_AssetsId);
                }

                if (!nodeMap.TryGetValue(m_AssetsId, out var nodes) || nodes == null)
                {
                    nodes = new HashSet<AssetNode>();
                    nodeMap[m_AssetsId] = nodes;
                }
                nodes.Add(this);
            }
        }

        private void OnDestroy()
        {
            if (m_AssetsId > 0)
            {
                if (nodeMap.TryGetValue(m_AssetsId, out var nodes) && nodes != null)
                {
                    nodes.Remove(this);
                }

                if (nodes != null && nodes.Count <= 0)
                {
                    nodeMap.Remove(m_AssetsId);
                    ReleaseHandles(handleMap, m_AssetsId);
                }
            }
        }

        ~AssetNode()
        {
            if (!m_IsAwaked)
            {
                ReleaseHandles(inactiveHandleMap, m_AssetsId);
            }
        }
    }
}