using LetsPiu.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEffect : MonoBehaviour
{
    [SerializeField] int m_UIEffectID;
    [SerializeField] UIParticleEffect.SortingType m_SortingType = UIParticleEffect.SortingType.RelativeParent;
    [SerializeField] int m_RelativeOrderDepth;
    [SerializeField] Canvas m_EffectCanvas;
    [SerializeField] bool m_UseSelfRoot = false;
    // 对应 SortingType 为 RelativeSpcify 时 指定对象控件
    [SerializeField] Canvas m_SpcifyCanvas;
    [SerializeField] float m_Size = 1f;

    UIParticleEffect m_Effect;
    public System.Action<UIParticleEffect> onLoaded;
    public Action OnFinish;

    public int effectID
    {
        get { return m_UIEffectID; }
        set
        {
            m_UIEffectID = value;

            if (m_Effect != null)
            {
                m_Effect.effectID = m_UIEffectID;
            }
        }
    }

    void Awake()
    {
        if (null == m_EffectCanvas)
        {
            m_EffectCanvas = GetComponent<Canvas>();
        }
    }

    [ContextMenu("TestPlay")]
    void OnEnable()
    {
        TryDestroyEffect();
        m_Effect = UIParticleEffect.Begin2(m_UIEffectID, m_UseSelfRoot ? transform : m_EffectCanvas.transform, 0f, m_Size, onLoaded);
        m_Effect.sortingType = m_SortingType;
        m_Effect.relativeOrderDepth = m_RelativeOrderDepth;
        m_Effect.spcifyCanvas = m_SpcifyCanvas;
        if (OnFinish != null)
        {
            m_Effect.onFinish = OnFinish;
        }
    }

    void OnDisable()
    {
        TryDestroyEffect();
    }

    void OnDestroy()
    {
        TryDestroyEffect();
    }

    void TryDestroyEffect()
    {
        if (null != m_Effect)
        {
            Destroy(m_Effect.gameObject);
        }
    }
}
