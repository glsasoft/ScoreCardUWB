using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    /// <summary>
    /// 按名字异步加载特效实例。同名特效共用一个 ResLoader，释放后进入缓存，超时或超出上限再销毁。
    /// </summary>
    public class EffectMgr : SingletonBase<EffectMgr>
    {
        // 每个名字最多保留的缓存实例数。达到上限后再释放，直接销毁
        public int CacheMaxCount { get; set; } = 8;

        // 某个名字没有在播实例后，超过该秒数就清空缓存并释放 ResLoader。小于等于 0 表示不按时间清理
        public float CacheExpireSeconds { get; set; } = 60f * 5;

        const int ExpireCheckIntervalMs = 1000;

        int m_NextId = 1;
        bool m_ExpireLoopStarted;

        // 按资源名分组，同名共用一个 ResLoader 和一份预制体
        readonly Dictionary<string, EffectPool> m_Pools = new();
        // 正在使用的特效，key 为 GetEffect 返回的 id。加载中也会占一个位置
        readonly Dictionary<int, PlayingEffect> m_Playing = new();
        // ClearCache 和超时清理共用，避免遍历字典时直接删除
        readonly List<string> m_IdleNames = new();

        class EffectPool
        {
            public string Name;
            public ResLoader Loader;
            public GameObject Prefab;
            public bool IsLoading;
            // 最近一次释放的时间，用来判断这个名字是否闲置超时
            public float LastReleaseTime;
            // 已发出且未释放的数量。加载中也计入，避免请求还在飞时把池子清掉
            public int ActiveCount;
            public readonly Queue<GameObject> Cache = new();
        }

        class PlayingEffect
        {
            public int ID;
            public string Name;
            public GameObject Go;
        }

        public EffectMgr()
        {
            EnsureExpireLoop();
        }

        /// <summary>
        /// 按资源名取得一个特效。立刻返回唯一 id，实例就绪后调用 onComplete。
        /// 名字为空时返回 0，且不会回调。加载失败时回调参数为 0，此前返回的 id 已失效。
        /// 命中缓存或预制体已在内存中时，onComplete 会在返回前同步调用。
        /// </summary>
        public int GetEffect(string effectName, Action<int> onComplete)
        {
            if (string.IsNullOrEmpty(effectName))
            {
                Debug.LogWarning("[EffectMgr] 特效名为空");
                return 0;
            }

            EnsureExpireLoop();

            if (!m_Pools.TryGetValue(effectName, out var pool))
            {
                pool = new EffectPool()
                {
                    Name = effectName,
                    Loader = new ResLoader(),
                };
                m_Pools.Add(effectName, pool);
            }

            var playing = new PlayingEffect()
            {
                ID = m_NextId++,
                Name = pool.Name,
            };
            pool.ActiveCount++;
            m_Playing[playing.ID] = playing;

            var cached = TakeCached(pool);
            if (cached != null)
            {
                playing.Go = cached;
            }
            else if (pool.Prefab != null)
            {
                playing.Go = GameObject.Instantiate(pool.Prefab);
            }

            if (playing.Go != null)
            {
                onComplete?.Invoke(playing.ID);
                return playing.ID;
            }

            pool.IsLoading = true;
            pool.Loader.AddRes(pool.Name, ResType.Prefab);
            pool.Loader.StartLoad(() =>
            {
                pool.IsLoading = false;
                if (pool.Loader == null)
                    return;

                var handle = pool.Loader.GetAsset(pool.Name);
                pool.Prefab = handle.AssetObject as GameObject;
                handle.Dispose();

                // 加载完成前已经释放的请求，不再创建实例
                if (!m_Playing.ContainsKey(playing.ID))
                    return;

                if (pool.Prefab != null)
                {
                    playing.Go = GameObject.Instantiate(pool.Prefab);
                    onComplete?.Invoke(playing.ID);
                }
                else
                {
                    Debug.LogError("[EffectMgr] 加载特效失败: " + pool.Name);
                    ReleaseEffect(playing.ID);
                    onComplete?.Invoke(0);
                }
            }).Forget();

            return playing.ID;
        }

        /// <summary>
        /// 取得 id 对应的实例。还在加载时返回 null，不会取消这次加载。
        /// 实例已被外部销毁时，释放该 id 并返回 null。
        /// </summary>
        public GameObject GetEffectGameObject(int id)
        {
            if (id == 0 || !m_Playing.TryGetValue(id, out var playing))
                return null;

            if (playing.Go == null)
            {
                // 被 Unity 销毁的对象比较结果为 null，但引用本身不是 null。尚未创建的 Go 才是真正的 null
                if (!ReferenceEquals(playing.Go, null))
                    ReleaseEffect(id);
                return null;
            }

            if (playing.Go.activeSelf == false)
                playing.Go.SetActive(true);

            return playing.Go;
        }

        /// <summary>
        /// 释放实例。未超过缓存上限时关闭并挂到 effectCacheRoot，否则直接销毁。
        /// 加载尚未完成时调用，只取消这次请求。
        /// </summary>
        public void ReleaseEffect(int id)
        {
            if (id == 0 || !m_Playing.TryGetValue(id, out var playing))
                return;

            m_Playing.Remove(id);

            if (!m_Pools.TryGetValue(playing.Name, out var pool))
            {
                GameObject.Destroy(playing.Go);
                return;
            }

            pool.ActiveCount--;
            if (pool.ActiveCount < 0)
                pool.ActiveCount = 0;

            pool.LastReleaseTime = Time.realtimeSinceStartup;

            // 加载中释放：还没有实例，只减少占用计数
            if (playing.Go == null)
                return;

            int maxCount = CacheMaxCount < 0 ? 0 : CacheMaxCount;
            if (pool.Cache.Count >= maxCount)
            {
                GameObject.Destroy(playing.Go);
                return;
            }

            Park(playing.Go);
            pool.Cache.Enqueue(playing.Go);
        }

        /// <summary>
        /// 清掉当前没有在播实例的缓存，并释放对应 ResLoader。仍在使用的名字保留。
        /// </summary>
        public void ClearCache()
        {
            m_IdleNames.Clear();
            foreach (var kv in m_Pools)
            {
                var pool = kv.Value;
                if (pool.ActiveCount <= 0)
                {
                    m_IdleNames.Add(kv.Key);
                }
            }

            for (int i = 0; i < m_IdleNames.Count; i++)
            {
                string name = m_IdleNames[i];
                if (m_Pools.TryGetValue(name, out var pool))
                {
                    ReleasePool(pool);
                    m_Pools.Remove(name);
                }
            }
        }

        GameObject TakeCached(EffectPool pool)
        {
            while (pool.Cache.Count > 0)
            {
                var cached = pool.Cache.Dequeue();
                // 跳过已经失效的缓存。取出后脱离缓存根节点并重新激活
                if (cached == null)
                    continue;

                cached.transform.SetParent(null);
                cached.SetActive(true);
                return cached;
            }
            return null;
        }

        // 挂到缓存根节点，避免跟着调用方的父节点一起被销毁
        static void Park(GameObject go)
        {
            if (go.activeSelf)
                go.SetActive(false);

            var root = ConstantContainer.effectCacheRoot;
            if (root != null)
                Helper.SetParent(go.transform, root, true);
            else
            {
                go.transform.SetParent(null);
                Helper.ResetTransform(go.transform);
            }
        }

        void EnsureExpireLoop()
        {
            if (m_ExpireLoopStarted || !Application.isPlaying)
                return;

            m_ExpireLoopStarted = true;
            ExpireLoop().Forget();
        }

        async UniTaskVoid ExpireLoop()
        {
            while (Application.isPlaying)
            {
                await UniTask.Delay(ExpireCheckIntervalMs);
                if (!Application.isPlaying)
                    break;
                ClearExpiredCache();
            }

            m_ExpireLoopStarted = false;
        }

        // 没有在播实例且距离上次释放已超过 CacheExpireSeconds 时，卸载该名字
        void ClearExpiredCache()
        {
            if (CacheExpireSeconds <= 0f)
                return;

            float now = Time.realtimeSinceStartup;
            m_IdleNames.Clear();
            foreach (var kv in m_Pools)
            {
                var pool = kv.Value;
                if (pool.ActiveCount <= 0 && now - pool.LastReleaseTime >= CacheExpireSeconds)
                {
                    m_IdleNames.Add(kv.Key);
                }
                if (pool.Cache.Count > CacheMaxCount)
                {
                    int removeCount = pool.Cache.Count - CacheMaxCount;
                    for (int i = 0; i < removeCount; i++)
                    {
                        var item = pool.Cache.Dequeue();
                        if (item != null)
                            GameObject.Destroy(item);
                    }
                }
            }

            for (int i = 0; i < m_IdleNames.Count; i++)
            {
                string name = m_IdleNames[i];
                if (m_Pools.TryGetValue(name, out var pool))
                {
                    ReleasePool(pool);
                    m_Pools.Remove(name);
                }
            }
        }

        void ReleasePool(EffectPool pool)
        {
            pool.Loader?.ReleaseAll();
            pool.Loader = null;
            pool.Prefab = null;

            while (pool.Cache.Count > 0)
            {
                var item = pool.Cache.Dequeue();
                if (item != null)
                    GameObject.Destroy(item);
            }
        }
    }
}
