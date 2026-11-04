using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LetsPiu.Base
{
    public class UIParticleEffect : UIBehaviour
    {
        Action<UIParticleEffect> m_OnLoaded;

        int m_EffectPesistID;
        GameObject m_EffGo;

        Canvas m_ParentCanvas = null;

        public enum SortingType
        {
            Absolute,               // 绝对
            RelativeParent,         // 相对父控件
            RelativeSpcify,         // 相对指定对象控件
        }

        // 特效ID
        public int effectID;

        // 持续时间，为0时表示无限
        public float duration;

        // 渲染队列类型，默认为绝对
        public SortingType sortingType = SortingType.Absolute;

        // 渲染队列取值
        public int absoluteOrder = 4000;

        // 相对深度
        public int relativeOrderDepth = 1;

        // 渲染队列相对的UI控件
        public Canvas relativeCanvas;

        // 相对指定对象控件
        public Canvas spcifyCanvas;

        // 设置完成回调
        public Action onFinish;

        // 特效实体ID
        public int effectPesistID { get { return m_EffectPesistID; } }

        public GameObject effectRoot
        {
            get { return m_EffGo; }
        }

        public static UIParticleEffect Begin(int effectID, Transform parent, float duration = 0f,
            System.Action<UIParticleEffect> onLoaded = null)
        {
            return Begin2(effectID, parent, duration, 1.0f, onLoaded);
        }

        public static UIParticleEffect Begin2(int effectID, Transform parent, float duration = 0.0f, float scale = 1.0f,
            System.Action<UIParticleEffect> onLoaded = null)
        {
            if (parent == null)
            {
                Debug.LogErrorFormat("[UIParticleEffect:Begin2]create {0} failed, parent can't be null.", effectID);
            }

            GameObject go = new GameObject("UIEffect");
            Helper.SetParent(go, parent);

            UIParticleEffect uiEffect = go.AddComponent<UIParticleEffect>();

            uiEffect.effectID = effectID;
            if (duration <= 0f)
            {
                uiEffect.duration = TDEffectResourceTable.GetExistTime(effectID);
            }
            else
            {
                uiEffect.duration = duration;
            }

            if (parent == null)
            {
                uiEffect.duration = 0.1f;
            }

            uiEffect.m_OnLoaded = onLoaded;
            return uiEffect;
        }

        protected override void Start()
        {
            base.Start();

            m_EffGo = null;
            m_EffectPesistID = EffectResourceModule.S.AddEffect((int)effectID, transform, (pesistID) =>
            {
                m_EffGo = EffectResourceModule.S.GetEffectGameObject(pesistID);
                // 获取相对的控件
                if (SortingType.RelativeParent == sortingType)
                {
                    relativeCanvas = GetComponentInParent<Canvas>();
                }
                else if (SortingType.RelativeSpcify == sortingType)
                {
                    relativeCanvas = spcifyCanvas;
                }

                absoluteOrder = CalcSortingOrder();
                SetSortingOrder(transform, absoluteOrder);

                // 设置层级
                if (m_ParentCanvas != null)
                {
                    Helper.PassLayer(m_ParentCanvas.gameObject, gameObject);
                }

                if (m_OnLoaded != null)
                {
                    m_OnLoaded(this);
                }
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
        }

        void LateUpdate()
        {
            if (duration > 0f)
            {
                duration -= Time.deltaTime;

                // 持续时间到
                if (duration <= 0f)
                {
                    if (onFinish != null)
                    {
                        try
                        {
                            onFinish();
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }

                    Destroy(gameObject);
                    return;
                }
            }

            if (this.sortingType != SortingType.Absolute)
            {
                int newOrder = CalcSortingOrder();

                if (newOrder != absoluteOrder)
                {
                    absoluteOrder = newOrder;
                    SetSortingOrder(transform, absoluteOrder);
                }
            }
        }

        void Dispose()
        {
            if (m_EffectPesistID != 0)
            {
                EffectResourceModule.S.RemoveEffect(m_EffectPesistID);
                m_EffGo = null;
                m_EffectPesistID = 0;
            }
        }

        int CalcSortingOrder()
        {
            if (sortingType != SortingType.Absolute)
            {
                if (relativeCanvas != null)
                {
                    return relativeCanvas.sortingOrder + relativeOrderDepth;
                }

                CheckParentCanvas();

                if (m_ParentCanvas != null)
                {
                    return m_ParentCanvas.sortingOrder + relativeOrderDepth;
                }
            }

            return absoluteOrder;
        }

        void CheckParentCanvas()
        {
            if (null == m_ParentCanvas)
            {
                m_ParentCanvas = transform.GetComponentInParent<Canvas>();
            }
        }

        void SetSortingOrder(Transform transform, int sortingOrder)
        {
            Renderer[] childs = transform.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < childs.Length; ++i)
            {
                int order = sortingOrder;
                var sortingObj = childs[i].GetComponent<SortingObject>();
                if (sortingObj)
                {
                    order += sortingObj.orderIdx;
                    sortingObj.enabled = false;
                }
                childs[i].sortingOrder = order;
            }
        }
    }
}
 