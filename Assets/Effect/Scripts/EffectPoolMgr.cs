using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using YooAsset;

namespace LetsPiu.Base
{
    public class EffectPoolMgr : SingletonBase<EffectPoolMgr>
    {
        #region 数据定义
        class EffectObj : ICacheAble
        {
            Transform[] m_ChildrenList;
            TrailRenderer[] m_TrailRendererList;
            ParticleSystem[] m_ParticleSystems;
            List<float> m_StartSizes = new List<float>();

            public GameObject go;

            public Transform[] childrenList
            {
                get
                {
                    if (this.go != null && m_ChildrenList == null)
                    {
                        m_ChildrenList = go.GetComponentsInChildren<Transform>();
                    }

                    return m_ChildrenList;
                }
            }

            public TrailRenderer[] trailRendererList
            {
                get
                {
                    if (this.go != null && m_TrailRendererList == null)
                    {
                        m_TrailRendererList = go.GetComponentsInChildren<TrailRenderer>();
                    }

                    return m_TrailRendererList;
                }
            }

            public ParticleSystem[] particleSystems
            {
                get
                {
                    if (this.go != null && m_ParticleSystems == null)
                    {
                        m_ParticleSystems = go.GetComponentsInChildren<ParticleSystem>();
                    }

                    return m_ParticleSystems;
                }
            }

            public List<float> startSizes
            {
                get { return m_StartSizes; }
            }

            bool ICacheAble.cacheFlag { get; set; }
            void ICacheAble.Reset2Cache()
            {
                m_ChildrenList = null;
                m_TrailRendererList = null;
                m_ParticleSystems = null;
                m_StartSizes.Clear();
                go = null;
            }
        }

        class EffectRes
        {
            public string fullPath;
            public ResLoader loader = new ResLoader();
            public int refCount = 0;
            public List<EffectObj> cachedEffects = new List<EffectObj>();
        }

        class EffectRequest : ICacheAble
        {
            public int id;
            public EffectRes effectRes;
            public EffectObj effectObj;
            public Action<int> onFinish;
            public bool isWaiting;

            bool ICacheAble.cacheFlag { get; set; }
            void ICacheAble.Reset2Cache()
            {
                id = 0;
                effectObj = null;
                effectRes = null;
                onFinish = null;
                isWaiting = false;
            }
        }
        #endregion

        // 等待帧数上限
        const int MAX_WAITINT_FRAME = 5;

        // 请求序列号
        int m_RequestSerialID;
        // 特效数量上限
        int m_MaxResCount = 60;
        // 实例缓存上限
        int m_MaxCacheInstanceCount = 15;
        // 实例缓存总上限
        int m_MaxTotalCacheInstanceCount = 300;
        // 缓存数量上限
        int m_MaxCacheCount = 50;

        int m_TotalCacheInstanceCount = 0;

        CancellationTokenSource m_CancelToken = new CancellationTokenSource();
        // 资源列表
        Dictionary<string, EffectRes> m_AllEffectResDict = new Dictionary<string, EffectRes>();
        List<EffectRes> m_CachedEffectResList = new List<EffectRes>();

        // 请求列表
        List<EffectRequest> m_WaitingRequestList = new List<EffectRequest>();
        Dictionary<int, EffectRequest> m_RequestDict = new Dictionary<int, EffectRequest>();

        // 对象池
        ObjectPool<EffectObj> m_EffectObjPool = new ObjectPool<EffectObj>();
        ObjectPool<EffectRequest> m_RequestPool = new ObjectPool<EffectRequest>();

        public EffectPoolMgr()
        {
            Start().Forget();
        }

        async UniTask Start()
        {
            while (m_CancelToken is not null && !m_CancelToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, m_CancelToken.Token);
                try
                {
                    UpdateWaitingRequest();
                }
                catch (Exception e)
                {
                    // 单个请求出问题不能连累驱动循环：循环一旦因异常退出，
                    // 后续所有特效请求都会永远挂在等待列表里、回调永不触发，整个特效系统静默失效。
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>诊断用：返回已加载资源槽位数 / 实例缓存总数，供 MemoryDiagnosticsLogger 采样，不影响正常逻辑。</summary>
        public (int resCount, int totalCacheInstanceCount, int requestCount) GetDebugCounts()
        {
            return (m_AllEffectResDict.Count, m_TotalCacheInstanceCount, m_RequestDict.Count);
        }

        public int Request(string resName, Action<int> onFinish)
        {
            if (string.IsNullOrEmpty(resName))
            {
                // 同样要回调：调用方普遍是 await 一个由 onFinish 兜底的 TaskCompletionSource，
                // 静默返回会让它永久挂住。
                Debug.LogError("[EffectPoolMgr] 特效寻址名为空，请求被丢弃");
                InvokeFinishSafely(onFinish, 0);
                return 0;
            }

            // 创建资源
            EffectRes effectRes;
            if (!m_AllEffectResDict.TryGetValue(resName, out effectRes))
            {
                if (CheckIsFull())
                {
                    // 槽位占满且没有可回收的缓存，多半是某处 Request 之后没有配对 Release（引用计数泄漏）。
                    // 这种状态下之后所有特效请求都会失败，把占用量打出来便于定位。
                    Debug.LogError(
                        $"[EffectPoolMgr] 特效资源槽位已满（占用 {m_AllEffectResDict.Count}/{m_MaxResCount}，"
                        + $"可回收缓存 {m_CachedEffectResList.Count}，在途请求 {m_RequestDict.Count}），请求被丢弃: {resName}");
                    InvokeFinishSafely(onFinish, 0);
                    return 0;
                }

                effectRes = CreateResItem(resName, resName);
            }
            else if (effectRes.refCount == 0)
            {
                // 从缓存中移除
                m_CachedEffectResList.Remove(effectRes);
            }

            // 创建请求
            EffectRequest request = m_RequestPool.Get();
            request.id = ++m_RequestSerialID;
            request.effectRes = effectRes;
            request.onFinish = onFinish;

            // 加入等待列表
            request.isWaiting = true;
            m_WaitingRequestList.Add(request);

            // 加入字典
            ++effectRes.refCount;
            m_RequestDict[request.id] = request;

            return request.id;
        }

        public bool Release(int requestID)
        {
            if (!m_RequestDict.ContainsKey(requestID))
            {
                return false;
            }

            EffectRequest request = m_RequestDict[requestID];

            // 从等待列表中移除
            Action<int> pendingFinish = null;
            if (request.isWaiting)
            {
                request.isWaiting = false;
                m_WaitingRequestList.Remove(request);

                // 请求在加载完成前被取消：CreateNewObject 不会再执行，onFinish 也就永不触发。
                // 调用方普遍是 await 一个由 onFinish 兜底的 TaskCompletionSource，拿不到回调会永久挂住，
                // 连它自己后续的 Release 也不再执行 —— 引用计数泄漏累积到槽位占满后，
                // 全局特效都请求不到，只能重启进程恢复。这里补一次 onFinish(0) 让调用方走失败分支。
                pendingFinish = request.onFinish;
            }

            if (request.effectObj != null)
            {
                if (request.effectObj.go != null)
                {
                    if (CanCache(request))
                    {
                        ++m_TotalCacheInstanceCount;
                        // 放入缓存池
                        request.effectObj.go.SetActive(false);
                        request.effectObj.go.transform.SetParent(ConstantContainer.effectCacheRoot);
                        request.effectRes.cachedEffects.Add(request.effectObj);
                    }
                    else
                    {
                        // 销毁特效
                        GameObject.Destroy(request.effectObj.go);
                        m_EffectObjPool.Release(request.effectObj);
                    }
                }
                else
                {
                    m_EffectObjPool.Release(request.effectObj);
                }
            }

            // 从字典中移除
            --request.effectRes.refCount;
            m_RequestDict.Remove(requestID);

            // 引用计数为0，进入缓存或者销毁
            if (request.effectRes.refCount == 0)
            {
                if (m_CachedEffectResList.Count >= m_MaxCacheCount)
                {
                    ReleaseUnused();
                }

                if (m_CachedEffectResList.Count < m_MaxCacheCount)
                {
                    m_CachedEffectResList.Add(request.effectRes);
                }
                else
                {
                    ReleaseRes(request.effectRes);
                    m_AllEffectResDict.Remove(request.effectRes.fullPath);
                }
            }

            // 放入对象池
            m_RequestPool.Release(request);

            // 总缓存数量超上限，销毁一批
            if (m_TotalCacheInstanceCount > m_MaxTotalCacheInstanceCount)
            {
                DestroyUnusedCacheObject();
            }

            // 必须等本次 Release 的状态改动全部落地后再回调：回调会同步续跑调用方的 await，
            // 而调用方的失败分支里往往又会 Release 其它特效，提前回调会造成重入。
            InvokeFinishSafely(pendingFinish, 0);

            return true;
        }

        static void InvokeFinishSafely(Action<int> onFinish, int requestID)
        {
            if (onFinish == null)
            {
                return;
            }

            try
            {
                onFinish.Invoke(requestID);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public GameObject GetGameObject(int requestID)
        {
            if (m_RequestDict.ContainsKey(requestID))
            {
                EffectRequest request = m_RequestDict[requestID];
                if (request.effectObj != null)
                {
                    return request.effectObj.go;
                }
            }

            return null;
        }

        public Transform[] GetChildrenList(int requestID)
        {
            if (m_RequestDict.ContainsKey(requestID))
            {
                EffectRequest request = m_RequestDict[requestID];
                if (request.effectObj != null)
                {
                    return request.effectObj.childrenList;
                }
            }

            return null;
        }

        public TrailRenderer[] GetTrailRendererList(int requestID)
        {
            if (m_RequestDict.ContainsKey(requestID))
            {
                EffectRequest request = m_RequestDict[requestID];
                if (request.effectObj != null)
                {
                    return request.effectObj.trailRendererList;
                }
            }

            return null;
        }

        public ParticleSystem[] GetParticleSystemList(int requestID)
        {
            if (m_RequestDict.ContainsKey(requestID))
            {
                EffectRequest request = m_RequestDict[requestID];
                if (request.effectObj != null)
                {
                    return request.effectObj.particleSystems;
                }
            }

            return null;
        }

        void UpdateWaitingRequest()
        {
            if (m_WaitingRequestList.Count <= 0)
            {
                return;
            }

            // 单帧创建数量上限
            int frameCreateLimit = 1;

            if (m_WaitingRequestList.Count > MAX_WAITINT_FRAME)
            {
                // 要保证在限制帧内创建完成
                frameCreateLimit += m_WaitingRequestList.Count / MAX_WAITINT_FRAME;
            }

            // 创建新的特效
            for (int i = 0; i < m_WaitingRequestList.Count; ++i)
            {
                EffectRequest request = m_WaitingRequestList[i];
                if (!request.effectRes.loader.isFinish)
                {
                    continue;
                }

                CreateNewObject(m_WaitingRequestList[i]);

                if (--frameCreateLimit <= 0)
                {
                    break;
                }
            }
        }

        void CreateNewObject(EffectRequest request)
        {
            // 从等待列表中移除
            request.isWaiting = false;
            m_WaitingRequestList.Remove(request);

            // 优先使用缓存对象
            if (request.effectRes.cachedEffects.Count > 0)
            {
                for (int i = request.effectRes.cachedEffects.Count - 1; i >= 0; --i)
                {
                    if (request.effectRes.cachedEffects[i].go == null)
                    {
                        --m_TotalCacheInstanceCount;

                        // 对象已经失效，清理
                        m_EffectObjPool.Release(request.effectRes.cachedEffects[i]);
                        request.effectRes.cachedEffects.RemoveAt(i);
                    }
                    else
                    {
                        --m_TotalCacheInstanceCount;

                        request.effectObj = request.effectRes.cachedEffects[i];
                        request.effectRes.cachedEffects.RemoveAt(i);
                        ResetEffect(request.effectObj);
                        break;
                    }
                }
            }

            // 未找到缓存对象，新创建一个
            if (request.effectObj == null)
            {
                var handle = request.effectRes.loader.GetAsset(request.effectRes.fullPath);
                var prefab = handle != null ? handle.AssetObject as GameObject : null;
                if (prefab == null)
                {
                    // 资源缺失多半是 prefab 改名/移动后寻址名没跟上。这里不能让它抛出去：
                    // 异常会穿过 UpdateWaitingRequest 打死 Start() 驱动循环，代价是整个特效池失效。
                    // effectObj 保持 null，调用方 GetGameObject 拿到 null 后走各自的失败分支。
                    Debug.LogError($"[EffectPoolMgr] 特效资源缺失，创建失败: {request.effectRes.fullPath}");
                }
                else
                {
                    request.effectObj = m_EffectObjPool.Get();
                    request.effectObj.go = GameObject.Instantiate(prefab);
                    handle.AttachToGameObject(request.effectObj.go);

                    var transform = request.effectObj.go.transform;
                    transform.SetParent(ConstantContainer.effectCacheRoot);
                    var childCnt = transform.childCount;
                    for (int i = 0; i < childCnt; ++i)
                    {
                        var child = transform.GetChild(i).gameObject;
                        var render = child.GetComponent<Renderer>();
                        if (render && render.sortingOrder != 0)
                        {
                            var sortingObj = child.gameObject.GetComponent<SortingObject>();

                            if (!sortingObj)
                            {
                                child.gameObject.AddComponent<SortingObject>();
                            }
                        }
                    }
                }
            }

            if (request.onFinish != null)
            {
                request.onFinish(request.id);
            }
        }

        void ResetEffect(EffectObj effectObj)
        {
            effectObj.go.SetActive(false);
            effectObj.go.SetActive(true);
            for (int i = 0; i < effectObj.trailRendererList.Length; i++)
            {
                var trail = effectObj.trailRendererList[i];
                trail.Clear();
            }
        }

        bool CheckIsFull()
        {
            if (m_AllEffectResDict.Count < m_MaxResCount)
            {
                return false;
            }

            if (m_CachedEffectResList.Count > 0)
            {
                ReleaseUnused();
                return false;
            }

            return true;
        }

        void ReleaseUnused()
        {
            if (m_CachedEffectResList.Count > 0)
            {
                EffectRes res = m_CachedEffectResList[0];
                ReleaseRes(res);

                m_AllEffectResDict.Remove(res.fullPath);
                m_CachedEffectResList.RemoveAt(0);
            }
        }

        void ReleaseRes(EffectRes res)
        {
            res.loader.ReleaseAll();

            // 清理缓存对象
            CleanCachedObjects(res);
        }

        void CleanCachedObjects(EffectRes res)
        {
            for (int i = 0; i < res.cachedEffects.Count; ++i)
            {
                EffectObj effectObj = res.cachedEffects[i];

                if (effectObj.go != null)
                {
                    GameObject.Destroy(effectObj.go);
                }

                m_EffectObjPool.Release(effectObj);
            }

            m_TotalCacheInstanceCount -= res.cachedEffects.Count;
            res.cachedEffects.Clear();
        }

        EffectRes CreateResItem(string fullPath, string resName)
        {
            EffectRes res = new EffectRes();
            res.fullPath = fullPath;
            res.loader.AddRes(res.fullPath, ResType.Prefab);
            res.loader.StartLoad().Forget();

            m_AllEffectResDict[fullPath] = res;

            return res;
        }

        bool CanCache(EffectRequest request)
        {
            if (request.effectRes.cachedEffects.Count >= m_MaxCacheInstanceCount)
            {
                // 单个缓存到达上限
                return false;
            }

            if (m_TotalCacheInstanceCount >= m_MaxTotalCacheInstanceCount)
            {
                // 总缓存到达上限，检测是否有可以清理的资源
                for (int i = 0; i < m_CachedEffectResList.Count; ++i)
                {
                    if (m_CachedEffectResList[i].cachedEffects.Count > 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        void DestroyUnusedCacheObject()
        {
            for (int i = 0; i < m_CachedEffectResList.Count; ++i)
            {
                if (m_CachedEffectResList[i].cachedEffects.Count > 0)
                {
                    CleanCachedObjects(m_CachedEffectResList[i]);
                    break;
                }
            }
        }

        public void ReleaseAllCachedEffectRes()
        {
            foreach(var res in m_CachedEffectResList)
            {
                ReleaseRes(res);
                m_AllEffectResDict.Remove(res.fullPath);
            }
            m_CachedEffectResList.Clear();
        }
    }
}
