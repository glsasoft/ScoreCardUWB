using LetsPiu.UIDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class UIData
    {
        protected string m_Name;
        protected UIID m_UIID;
        protected Type m_PanelClassType;
        protected int m_CacheCount = 0;
        protected bool m_IsSingleton = true;//默认都是单例

        public UIData(UIID uiID, Type pType, string pName)
        {
            m_UIID = uiID;
            m_PanelClassType = pType;
            m_Name = pName;
        } 
        public UIData(UIID uiID, Type pType, string pName, int cacheCount, bool singleton)
        {
            m_UIID = uiID;
            m_PanelClassType = pType;
            m_Name = pName;
            m_CacheCount = cacheCount;
            m_IsSingleton = singleton;
        }

        public string name
        {
            get { return m_Name; }
        }

        public UIID uiID
        {
            get { return m_UIID; }
        }

        public Type panelClassType
        {
            get { return m_PanelClassType; }
        }

        public int cacheCount
        {
            get { return m_CacheCount; }
        }

        public bool isSingleton
        {
            get { return m_IsSingleton; }
        }

    }

    public class PanelUIData : UIData
    {
        public PanelUIData(UIID uiID, Type pType, string pName)
            : base(uiID, pType, pName)
        {
        }
        public PanelUIData(UIID uiID, Type pType, string pName, int cacheCount, bool singleton)
            : base(uiID, pType, pName, cacheCount, singleton)
        {
        }
    }

    public class InnerPageData : PanelUIData
    {
        public InnerPageData(UIID uiID, Type pType, string pName)
            : base(uiID, pType, pName)
        {
        }
    }
}

