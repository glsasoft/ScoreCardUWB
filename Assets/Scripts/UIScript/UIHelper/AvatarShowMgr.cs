using LetsPiu.Base;
using LetsPiu.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class AvatarShowMgr : SingletonBase<AvatarShowMgr>
    {
        GameObject m_Root;
        Vector3 m_NextFreePos = new Vector3(0, 0, 0);
        Light m_Light;

        public GameObject avatarShowRoot
        {
            get
            {
                if (m_Root == null)
                {
                    InitRootGameObject();
                }

                return m_Root;
            }
        }

        public bool enableLight
        {
            set
            {
                if (m_Root == null)
                {
                    InitRootGameObject();
                }

                if (m_Light != null)
                {
                    m_Light.enabled = value;
                }
            }
        }

        public Light avatarLight
        {
            get
            {
                if (m_Root == null)
                {
                    InitRootGameObject();
                }

                return m_Light;
            }
        }

        public void SetAvatarRootParent(Transform go)
        {
            go.SetParent(avatarShowRoot.transform, true);
            UITools.SetGameObjectLayer(go.gameObject, Layer.AVATAR_SHOW);
            go.localEulerAngles = Vector3.zero;
            go.localScale = Vector3.one;
            go.localPosition = GetNextFreePos();

            if (m_Light != null)
            {
                m_Light.cullingMask = 1 << Layer.AVATAR_SHOW;
            }
        }

        void InitRootGameObject()
        {
            if (ConstantContainer.avatarRoot)
            {
                m_Root = ConstantContainer.avatarRoot.gameObject;
                GameObject lightObj = new GameObject();
                Helper.SetParent(lightObj, m_Root);
                lightObj.name = "AvatarLight";

                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = Color.white;
                light.intensity = 0.3f;
                light.bounceIntensity = 1;
                light.shadows = LightShadows.Soft;
                light.cullingMask = 1 << Layer.AVATAR_SHOW;

                light.transform.localPosition = new Vector3(0, 3.55f, 0);
                light.transform.localEulerAngles = new Vector3(30.392f, 153.946f + 180, -15.148f);

                m_Light = light;
                UITools.SetGameObjectLayer(m_Root, Layer.AVATAR_SHOW);
            }
        }

        Vector3 GetNextFreePos()
        {
            // x&y [1000,3000] z=1000
            m_NextFreePos.x += 100;
            if (m_NextFreePos.x > 1000)
            {
                m_NextFreePos.x = 1000;
                m_NextFreePos.y += 100;
                if (m_NextFreePos.y > 1000)
                {
                    m_NextFreePos = new Vector3(0, 0, 0);
                }
            }

            return m_NextFreePos;
        }
    }
}
