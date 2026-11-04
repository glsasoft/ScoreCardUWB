using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public class CameraMgr : MonoBehaviour
    {
        Camera m_MainCameara;
        Camera m_CharacterCamera;
        Transform m_EffectRoot;

        static CameraMgr s_Root;

        public Camera mainCamera => m_MainCameara;

        public Camera characterCamera => m_CharacterCamera;

        public Transform effectRoot => m_EffectRoot;

        public static CameraMgr S => s_Root;

        private void Awake()
        {
            s_Root = this;
            m_MainCameara = Camera.main;
            m_CharacterCamera = m_MainCameara.transform.Find("CharacterCamera")?.GetComponent<Camera>();
            m_EffectRoot = m_MainCameara.transform.Find("EffectRoot");
        }
    }
}
