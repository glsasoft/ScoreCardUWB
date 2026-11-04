using Cysharp.Threading.Tasks;
using LetsPiu.Base;
using LetsPiu.Common.Network.Model.Http.Play;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LetsPiu.Main
{
    public class AvatarMgr : SingletonBase<AvatarMgr>
    {
        Dictionary<string, GameObject> m_dicAvatarOriginRes = new();
        Dictionary<string, List<GameObject>> m_dicAvatarCacheRes = new();

        int m_CacheMaxCount = 0;

        ResLoader m_ResLoader;

        public AvatarMgr()
        {
            m_ResLoader ??= new ResLoader();
        }

        public async UniTask<GameObject> GetAvatar(string resId)
        {
            Debug.Log("[AvatarMgr] resId: " + resId);
            if (string.IsNullOrEmpty(resId))
            {
                return null;
            }
            if (!m_dicAvatarOriginRes.ContainsKey(resId))
            {
                m_ResLoader.AddRes(resId, ResType.Prefab, path =>
                {
                    // 加载资源
                    var assetHandle = m_ResLoader.GetAsset(path);
                    m_dicAvatarOriginRes[resId] = assetHandle.AssetObject as GameObject;
                    assetHandle.Dispose();
                });

                await m_ResLoader.StartLoad();
            }

            return GetCacheData(resId);
        }
        
        public void RecycleAvater(string resId, GameObject obj)
        {
            if (string.IsNullOrEmpty(resId))
            {
                GameObject.Destroy(obj);
                return;
            }
            if (!m_dicAvatarCacheRes.ContainsKey(resId))
            {
                m_dicAvatarCacheRes[resId] = new List<GameObject>();
            }

            var list = m_dicAvatarCacheRes[resId];

            // 缓存数量多大，直接清理掉 减轻缓存压力。
            if (list.Count >= m_CacheMaxCount)
            {
                GameObject.Destroy(obj);
                return;
            }

            Helper.ResetTransform(obj.transform);
            obj.SetActive(false);
            obj.transform.SetParent(ConstantContainer.avatarCacheRoot);
            obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            Helper.SetLayerRecursively(obj.transform, Layer.Default);

            m_dicAvatarCacheRes[resId].Add(obj);
        }

        // 释放掉对象池缓存
        public void DisposeSafe()
        {
            foreach (var item in m_dicAvatarCacheRes)
            {
                if (item.Value.Count > 0)
                {
                    for (int i = item.Value.Count - 1; i >= 0; i--)
                    {
                        GameObject.Destroy(item.Value[i]);
                    }
                }
            }
            m_dicAvatarOriginRes.Clear();
            m_dicAvatarCacheRes.Clear();

            if (m_ResLoader != null)
            {
                m_ResLoader.ReleaseAll();
            }

            Debug.Log("[AvatarMgr] 清理资源");
        }

        GameObject GetCacheData(string resId)
        {
            if (!m_dicAvatarCacheRes.TryGetValue(resId, out var cacheLst))
            {
                // 装一个到cache池子里
                m_dicAvatarCacheRes[resId] = new List<GameObject>();
                cacheLst = m_dicAvatarCacheRes[resId];
            }

            if (cacheLst.Count <= 0)
            {
                if (!m_dicAvatarOriginRes.TryGetValue(resId, out var originObj) || originObj == null)
                {
                    Debug.LogError("没有资源: " + resId);
                    return null;
                }
                var cacheObj = GameObject.Instantiate(m_dicAvatarOriginRes[resId]);
                Helper.ResetTransform(cacheObj.transform);
                cacheObj.SetActive(false);
                cacheObj.transform.SetParent(ConstantContainer.avatarCacheRoot);

                cacheLst.Add(cacheObj);
            }
            var obj = cacheLst[0];
            cacheLst.RemoveAt(0);
            obj.SetActive(true);
            return obj;
        }
    }
}
