using Cysharp.Threading.Tasks;
using LetsPiu.Define;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LetsPiu.Base
{
    public class EffectResourceModule : SingletonBase<EffectResourceModule>
    {
        Dictionary<int, EffectResourceItem> m_EffectItemDict = new Dictionary<int, EffectResourceItem>();
        // 效果列表
        List<EffectResourceItem> m_EffectItemList = new List<EffectResourceItem>();
        // 移除列表
        List<int> m_RemoveItems = new List<int>();

        ObjectPool<EffectResourceItem> m_ObjectPool = new ObjectPool<EffectResourceItem>();

        int m_MaxCount = 100;

        CancellationTokenSource m_CancelToken;

        Dictionary<int, Action> m_RemoveCallbackDic = new Dictionary<int, Action>();

        public EffectResourceModule()
        {
            m_CancelToken = new CancellationTokenSource();
            m_RemoveItems.Clear();
            OnLateUpdate().Forget();
        }

        public bool ExistEffect(int pesistID)
        {
            return m_EffectItemDict.ContainsKey(pesistID);
        }

        /// <summary>
        ///  添加特效
        /// </summary>
        /// <param name="effectID">特效配置ID</param>
        /// <param name="parent">特效父节点</param>
        /// <param name="loadedCallBack">特效加载完成回调</param>
        /// <param name="prior">特效优先级，-1表示使用配置的优先级</param>
        /// <returns></returns>
        public int AddEffect(int effectID, Transform parent, Action<int> loadedCallBack = null)
        {
            return AddEffect(effectID, parent, Vector3.zero, Vector3.zero, loadedCallBack);
        }

        /// <summary>
        ///  添加特效
        /// </summary>
        /// <param name="effectID">特效配置ID</param>
        /// <param name="parent">特效父节点</param>
        /// <param name="pos">特效创建的位置</param>
        /// <param name="orient">特效创建的朝向</param>
        /// <param name="loadedCallBack">特效加载完成回调</param>
        /// <param name="prior">特效优先级，-1表示使用配置的优先级</param>
        public int AddEffect(int effectID, Transform parent,
            Vector3 pos, Vector3 orient, Action<int> loadedCallBack = null)
        {
            
            if (parent == null)
            {
                return 0;
            }
            //Debug.Log("bdl: 小技能已就绪" + parent.name+ "CheckFullAndTryRemove(effectID)"+ CheckFullAndTryRemove(effectID));
            // 检测特效是否已满，并且尝试移除低优先级的特效
            bool isFull = CheckFullAndTryRemove(effectID);
            if (isFull)
            {
                // 特效已满，无法创建
                return 0;
            }

            EffectResourceItem item = m_ObjectPool.Get();
            item.Init(effectID, parent, pos, orient, loadedCallBack);

            if (item.isFinish)
            {
                //Debug.Log("EffectResourceItem Init effectID2" + parent.name);
                m_ObjectPool.Release(item);
                return 0;
            }
            //Debug.Log("EffectResourceItem Init effectID2" + parent.name);
            AddEffect2List(item);

            return item.pesistID;
        }

        // 移除特效，如果需要调用结束阶段，请调用EndEffect接口
        public bool RemoveEffect(int pesistID)
        {
            EffectResourceItem item = FindEffect(pesistID);
            if (item == null)
            {
                return false;
            }

            //Helper.SetLayerRecursively(item.effectObject?.transform, Layer.Default);
            item.Dispose();
            m_EffectItemDict.Remove(pesistID);
            m_EffectItemList.Remove(item);

            m_ObjectPool.Release(item);

            if (m_RemoveCallbackDic.TryGetValue(pesistID, out Action removeCallback))
            {
                removeCallback?.Invoke();
                m_RemoveCallbackDic.Remove(pesistID);
            }

            return true;
        }

        public void SetRemoveCallback(int pesistID, Action callback)
        {
            m_RemoveCallbackDic[pesistID] = callback;
        }

        // 结束特效，特效如果存在结束阶段，会播放结束阶段特效
        public bool EndEffect(int pesistID)
        {
            EffectResourceItem item = FindEffect(pesistID);
            if (item == null)
            {
                return false;
            }

            item.End();

            return true;
        }

        public GameObject GetEffectGameObject(int pesistID)
        {
            EffectResourceItem effResItem = FindEffect(pesistID);
            if (effResItem != null)
            {
                return effResItem.effectObject;
            }

            return null;
        }

        public void SetOrient(int pesistID, Vector3 orient)
        {
            EffectResourceItem effResItem = FindEffect(pesistID);
            if (effResItem != null)
            {
                effResItem.orient = orient;
            }
        }

        public void SetPos(int pesistID, Vector3 pos, bool clearTrail = false)
        {
            EffectResourceItem effResItem = FindEffect(pesistID);
            if (effResItem != null)
            {
                effResItem.pos = pos;

                if (clearTrail)
                {
                    effResItem.ClearTrail();
                }
            }
        }

        public void ClearTrail(int pesistID)
        {
            EffectResourceItem effResItem = FindEffect(pesistID);
            if (effResItem != null)
            {
                effResItem.ClearTrail();
            }
        }

        /// <summary>
        /// 设置世界坐标和旋转
        /// </summary>
        public void SetWorldPosition(int sid, Vector3 position, Quaternion rotation)
        {
            var item = FindEffect(sid);
            if (item == null)
            {
                return;
            }

            item.SetWorldPosition(position, rotation);
        }

        /// <summary>
        /// 设置播放时长, 用于提前结束特效, 单位 秒
        /// </summary>
        /// <param name="pesistId"></param>
        /// <param name="duration"></param>
        public void SetDuration(int pesistId, float duration,bool forceEndLoop = false)
        {
            var effItem = FindEffect(pesistId);
            if (effItem != null)
            {
                effItem.SetDuration(duration, forceEndLoop);
            }
        }

        public void ClearAllEffects()
        {
            try
            {
                foreach (var item in m_EffectItemList)
                {
                    item.Dispose();
                    m_ObjectPool.Release(item);
                }
                m_EffectItemDict.Clear();
                m_EffectItemList.Clear();

                for (int i = 0; i < m_RemoveItems.Count; ++i)
                {
                    RemoveEffect(m_RemoveItems[i]);
                }
                m_RemoveItems.Clear();

                EffectPoolMgr.S.ReleaseAllCachedEffectRes();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        async UniTask OnLateUpdate()
        {
            Debug.Log(m_CancelToken + "m_CancelToken.Cancel()");
            while (m_CancelToken is not null && !m_CancelToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, m_CancelToken.Token);
                for (int i = 0; i < m_EffectItemList.Count; ++i)
                {
                    EffectResourceItem item = m_EffectItemList[i];

                    if (item.isFinish)
                    {
                        m_RemoveItems.Add(item.pesistID);
                    }
                    else
                    {
                        item.Update(Time.deltaTime);
                    }
                }

                for (int i = 0; i < m_RemoveItems.Count; ++i)
                {
                    RemoveEffect(m_RemoveItems[i]);
                }
                m_RemoveItems.Clear();
            }
        }

        // 检测特效是否已满，并且尝试移除
        bool CheckFullAndTryRemove(int effectID)
        {
            TDEffectResource tdEffectResource = TDEffectResourceTable.GetData(effectID);
            if (tdEffectResource == null)
            {
                // 无效的配置，直接返回已满，不再创建
                return true;
            }

            if (m_EffectItemList.Count <= m_MaxCount)
            {
                return false;
            }

            if (tdEffectResource.EffectType == (int)EffecResourceType.OnTheUI
                || tdEffectResource.EffectType == (int)EffecResourceType.UnderTheUI)
            {
                return false;
            }

            // 列表已排序，第一个就是优先级最低的
            EffectResourceItem item = m_EffectItemList[0];
            if (item.data != null)
            {
                RemoveEffect(item.pesistID);
                return false;
            }

            return true;
        }

        void AddEffect2List(EffectResourceItem item)
        {
            m_EffectItemDict.Add(item.pesistID, item);

            int i = m_EffectItemList.Count - 1;

            for (; i >= 0; --i)
            {
                EffectResourceItem temp = m_EffectItemList[i];
                if (temp.data != null)
                {
                    break;
                }
            }

            if (i < m_EffectItemList.Count - 1)
            {
                m_EffectItemList.Insert(i + 1, item);
            }
            else
            {
                m_EffectItemList.Add(item);
            }
        }

        EffectResourceItem FindEffect(int pesistID)
        {
            if (pesistID < 0)
            {
                return null;
            }

            EffectResourceItem item;
            if (m_EffectItemDict.TryGetValue(pesistID, out item))
            {
                return item;
            }

            return null;
        }
    }
}
