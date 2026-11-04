using System;
using UnityEngine;

namespace LetsPiu.Base
{
    public class EffectResourceItem : ICacheAble
    {
        bool ICacheAble.cacheFlag { get; set; }

        void ICacheAble.Reset2Cache()
        {
            m_PesistID = 0;
            m_TDData = null;
            m_CacheParent = null;
            m_Pos = Vector3.zero;
            m_Orient = Vector3.zero;
            m_OnLoadedCB = null;
            m_IsFinish = false;
            m_IsStart = false;
            m_IsInWorld = false;
            m_WorldPos = Vector3.zero;
            m_WorldRotation = Quaternion.identity;
            m_PlayEffectName = null;
            m_EffectObject = null;
            m_IsInRequest = false;
            m_LifeTime = 0;
            m_EffectRequestID = 0;
            m_Duration = 0;
            m_ExistPlayTime = 0;
            m_HasStartDelay = false;
            m_ForceEndByDuration = false;
        }

        static int s_SerialID;

        int m_PesistID;

        TDEffectResource m_TDData;
        Transform m_CacheParent; //异步加载缓存父节点，特效加载完了，节点挂在这个节点上
        Vector3 m_Pos;
        Vector3 m_Orient;
        Action<int> m_OnLoadedCB;
        Action<int> m_OnRequestFinishCB;

        bool m_IsStart;
        bool m_IsFinish;
        bool m_IsInWorld;
        Vector3 m_WorldPos;
        Quaternion m_WorldRotation;

        string m_PlayEffectName;
        GameObject m_EffectObject;
        bool m_IsInRequest;
        int m_EffectRequestID;
        float m_LifeTime;
        float m_ExistPlayTime;
        float m_Duration;
        bool m_HasStartDelay = false;

        bool m_ForceEndByDuration = false;

        public int pesistID => m_PesistID;
        public bool isFinish => m_IsFinish;
        public GameObject effectObject => m_EffectObject;
        public TDEffectResource data => m_TDData;

        public Vector3 pos
        {
            set
            {
                m_Pos = value;
            }
        }

        public Vector3 orient
        {
            set
            {
                m_Orient = value;
            }
        }

        public EffectResourceItem()
        {
            m_OnRequestFinishCB = OnPoolRequestFinished;
        }

        /// <summary>
        /// 设置播放时长, 用于提前结束特效
        /// </summary>
        /// <param name="duration">播放时长, 单位 秒</param>
        public void SetDuration(float duration, bool forceEndLoop = false)
        {
            m_Duration = duration;
            m_LifeTime = m_Duration;
            m_ForceEndByDuration = forceEndLoop;
        }

        public void SetWorldPosition(Vector3 position, Quaternion rotation)
        {
            m_IsInWorld = true;
            m_WorldPos = position;
            m_WorldRotation = rotation;
        }

        public void Dispose()           
        {
            m_IsFinish = true;
            ReleaseEffect();
        }

        public void End()   
        {
            if (m_TDData.DelayDestroyTime != 0 && !m_HasStartDelay)
            {
                m_LifeTime = m_TDData.DelayDestroyTime / 1000f;
                m_HasStartDelay = true;
                ProcessWaitTrailEnd();
            }
            else
            {
                ReleaseEffect();
                m_IsFinish = true;
            }
        }

        public void Init(int effectID, Transform parent, Vector3 pos, Vector3 orient,
            Action<int> loadedCallBack)
        {
            m_PesistID = ++s_SerialID;
            m_CacheParent = parent;
            m_Pos = pos;
            m_Orient = orient;
            m_OnLoadedCB = loadedCallBack;
            m_TDData = effectID > 0 ? TDEffectResourceTable.GetData(effectID) : null;

            //Debug.Log("EffectResourceItem Init effectID: " + effectID + " parent: " + parent + " pos: " + pos + " orient: " + orient+ "m_TDData.ResName"+ m_TDData.ResName);

            if (m_TDData == null)
            {
                m_IsFinish = true;
                return;
            }
            //Debug.Log("EffectResourceItem Init effectID1: " + effectID + " parent: " + m_TDData.ResName.Length);
            if (m_TDData.ResName.Length <= 0)
            {
                m_IsFinish = true;
                return;
            }
            
            m_LifeTime = (m_TDData.DelayDestroyTime + m_TDData.Duration) / 1000f;
            m_ExistPlayTime = 0;
            m_PlayEffectName = m_TDData.ResName;
        }

        public void SetParent(Transform parent, bool resetPos)
        {
            m_CacheParent = parent;
            if (m_EffectObject != null)
            {
                Helper.SetParent(m_EffectObject.transform, parent, resetPos);
            }
        }

        // 等待父物体恢复最大时间（秒）
        private float m_WaitForParentTime = 0f;
        private const float m_MaxWaitForParentTime = 1.0f; // 最多等待1秒

        public void Update(float dt)
        {
            if (m_IsFinish)
            {
                return;
            }

            // 检测父类对象是否存在
            if (m_CacheParent == null)
            {
                m_WaitForParentTime += dt;
                if (m_WaitForParentTime >= m_MaxWaitForParentTime)
                {
                    m_IsFinish = true;
                    ReleaseEffect();
                }
                return;
            }
            else
            {
                m_WaitForParentTime = 0f; // 父物体恢复，重置等待计时
            }

            if (!m_IsStart)
            {
                StartEffect();
            }

            if (m_IsInRequest)
            {
                return;
            }

            m_LifeTime -= dt;
            if (m_LifeTime <= 0 && (m_TDData.Duration != -1 || m_ForceEndByDuration))
            {
                m_IsFinish = true;
                ReleaseEffect();
                return;
            }

            m_ExistPlayTime += dt;
            if (!m_HasStartDelay)
            {
                CheckDelayDesotry();
            }
        }

        public void ClearTrail()
        {
            TrailRenderer[] rendererList = EffectPoolMgr.S.GetTrailRendererList(m_EffectRequestID);
            if (rendererList != null && rendererList.Length > 0)
            {
                for (int i = 0; i < rendererList.Length; ++i)
                {
                    if (rendererList[i] == null)
                    {
                        continue;
                    }

                    rendererList[i].Clear();
                }
            }
        }

        public Transform[] GetChildrenList()
        {
            return EffectPoolMgr.S.GetChildrenList(m_EffectRequestID);
        }

        void StartEffect()
        {
            m_IsStart = true;
            m_IsInRequest = true;
            m_EffectRequestID = EffectPoolMgr.S.Request(m_PlayEffectName, m_OnRequestFinishCB);
        }

        void OnPoolRequestFinished(int requestID)
        {
            m_IsInRequest = false;

            // requestID<=0：请求失败，或在加载完成前被 Release 取消。池里不会有实例，
            // 不能再拿旧的 m_EffectRequestID 去取对象。与 PlayExistEffect 取不到对象时一致，
            // 通知一次加载回调后结束。
            if (requestID <= 0)
            {
                m_OnLoadedCB?.Invoke(m_PesistID);
                return;
            }

            if (m_EffectRequestID <= 0)
            {
                m_EffectRequestID = requestID;
            }

            PlayExistEffect();
            Update(0);
        }

        void PlayExistEffect()
        {
            if (m_EffectObject != null)
            {
                return;
            }
            //Debug.Log("EffectResourceItem Init effectID" + m_EffectRequestID);
            if (m_EffectRequestID > 0)
            {
                m_EffectObject = EffectPoolMgr.S.GetGameObject(m_EffectRequestID);
                if (m_EffectObject == null)
                {
                    m_OnLoadedCB?.Invoke(m_PesistID);
                    return;
                }

                // 特效复用前需要先禁用自己, 设置到正确位置后再激活
                m_EffectObject.SetActive(false);
                Helper.SetParent(m_EffectObject.transform, m_CacheParent);

                UpdateLayer();
                UpdateTransform();

                m_ExistPlayTime = 0;

                // 清理拖尾效果
                ClearTrail();
                m_EffectObject.SetActive(true);
                m_OnLoadedCB?.Invoke(m_PesistID);
            }
            else
            {
                m_OnLoadedCB?.Invoke(m_PesistID);
            }
        }

        void UpdateLayer()
        {
            return;

            if (m_CacheParent == null)
            {
                return;
            }

            Transform[] childrenList = EffectPoolMgr.S.GetChildrenList(m_EffectRequestID);
            if (childrenList == null)
            {
                return;
            }

            int layer = m_CacheParent.gameObject.layer;
            int ignoreLayerMask = 1 << Layer.INVISIBLE;

            for (int i = 0; i < childrenList.Length; ++i)
            {
                if (childrenList[i] == null)
                {
                    continue;
                }

                if (((1 << childrenList[i].gameObject.layer) & ignoreLayerMask) == 0)
                {
                    Helper.SetLayerRecursively(childrenList[i].gameObject.transform, layer);
                }
            }
        }

        void UpdateTransform()
        {
            var objTransform = m_EffectObject.transform;
            objTransform.SetParent(m_CacheParent);
            if (m_IsInWorld)
            {
                objTransform.SetPositionAndRotation(m_WorldPos, m_WorldRotation);
            }
            else
            {
                objTransform.localPosition = m_Pos;
                objTransform.localEulerAngles = m_Orient;
            }
        }

        void CheckDelayDesotry()
        {
            if (m_TDData.DelayDestroyTime != 0 && !m_HasStartDelay && m_TDData.Duration <= m_ExistPlayTime)
            {
                m_HasStartDelay = true;
                ProcessWaitTrailEnd();
            }
        }

        void ReleaseEffect()
        {
            m_EffectObject = null;

            if (m_EffectRequestID > 0)
            {
                EffectPoolMgr.S.Release(m_EffectRequestID);
                m_EffectRequestID = 0;
            }
        }

        bool ProcessWaitTrailEnd()
        {
            return true;

            // 隐藏非拖尾节点
            if (m_EffectObject != null)
            {
                Transform[] childList = m_EffectObject.GetComponentsInChildren<Transform>();

                // 隐藏非拖尾节点下的其他对象
                for (int i = 0; i < childList.Length; ++i)
                {
                    Transform childParent = childList[i].parent;

                    if (childList[i] != m_EffectObject.transform && childParent != null && !childParent.name.StartsWith("tuowei_"))
                    {
                        //childList[i].gameObject.layer = Layer.INVISIBLE;
                    }
                }
            }

            return true;
        }
    }
}
