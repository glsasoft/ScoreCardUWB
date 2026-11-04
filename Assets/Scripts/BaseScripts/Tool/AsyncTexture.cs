using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LetsPiu
{
    public class AsyncTexture : MonoBehaviour
    {
        [SerializeField]
        RawImage m_RawImage;
        [SerializeField]
        string m_LocalTextureName;
        [SerializeField]
        bool m_AutoHide = false;
        [SerializeField]
        bool m_IsSetNativeSize = false;

        ResLoader m_ResLoader;
        Action m_ResLoadedCallBack;

        bool m_ResLoadFinish = false;

        public RawImage rawImage
        {
            get { return m_RawImage; }
        }

        public bool autoHide
        {
            get { return m_AutoHide; }
            set { m_AutoHide = value; }
        }

        public bool isSetNativeSize
        {
            get { return m_IsSetNativeSize; }
            set
            {
                m_IsSetNativeSize = value;

                if (m_IsSetNativeSize && m_ResLoadFinish)
                {
                    m_RawImage.SetNativeSize();
                }
            }
        }

        public Action onResLoadFinish
        {
            get { return m_ResLoadedCallBack; }
            set
            {
                m_ResLoadedCallBack = value;

                if (m_ResLoadedCallBack != null && m_ResLoadFinish)
                {
                    ERunner.Run(m_ResLoadedCallBack);
                }
            }
        }

        public string localTextureName
        {
            get { return m_LocalTextureName; }
            set
            {
                if (!Application.isPlaying)
                {
                    m_LocalTextureName = value;
                    return;
                }

                if (m_LocalTextureName == value)
                {
                    if (m_ResLoadedCallBack != null && m_ResLoadFinish)
                    {
                        ERunner.Run(m_ResLoadedCallBack);
                    }

                    return;
                }

                m_LocalTextureName = value;
                LoadTexture(m_LocalTextureName);
            }
        }

        public Sprite sprite
        {
            set
            {
                if (m_RawImage != null)
                {
                    if (value != null)
                    {
                        m_RawImage.texture = value.texture;
                        m_RawImage.enabled = true;
                    }
                    else
                    {
                        m_RawImage.texture = null;
                        m_RawImage.enabled = false;
                    }
                }
            }
        }

        public void LoadTexture(string path)
        {
            if (m_RawImage == null)
            {
                return;
            }

            //m_RawImage.texture = null;

            if (m_AutoHide)
            {
                m_RawImage.enabled = false;
            }

            if (m_ResLoader != null)
            {
                m_ResLoader.ReleaseAll();
            }

            m_LocalTextureName = path;

            if (string.IsNullOrEmpty(m_LocalTextureName))
            {
                return;
            }

            if (m_ResLoader == null)
            {
                m_ResLoader = new ResLoader();
            }

            m_ResLoadFinish = false;
            m_ResLoader.AddRes(m_LocalTextureName, ResType.Texture, OnResLoadFinish);
            m_ResLoader.StartLoad().Forget(); ;
        }

        void Awake()
        {
            if (m_RawImage == null)
            {
                m_RawImage = GetComponent<RawImage>();
            }

            if (!string.IsNullOrEmpty(m_LocalTextureName))
                LoadTexture(m_LocalTextureName);
        }

        void OnDestroy()
        {
            if (m_ResLoader != null)
            {
                m_ResLoader.ReleaseAll();
                m_ResLoader = null;
            }
        }

        void OnResLoadFinish(string res)
        {
            if (res == null)
            {
                return;
            }
            var handle = m_ResLoader.GetAsset(res);
            m_RawImage.texture = handle.AssetObject as Texture;
            m_RawImage.enabled = true;
            handle.Dispose();

            if (m_IsSetNativeSize)
            {
                m_RawImage.SetNativeSize();
            }

            if (onResLoadFinish != null)
            {
                ERunner.Run(onResLoadFinish);
            }

            m_ResLoadFinish = true;
        }
    }
}
