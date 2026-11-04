using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LetsPiu
{
    /// <summary>
    /// 通用异步加载 UI 图片（支持 RawImage 和 Image，带 Sprite 缓存）
    /// </summary>
    public class AsyncTexture_New : MonoBehaviour
    {
        [SerializeField] private string m_LocalTextureName;
        [SerializeField] private bool m_AutoHide = false;
        [SerializeField] private bool m_IsSetNativeSize = false;

        private ResLoader m_ResLoader;
        private Action m_ResLoadedCallBack;
        private bool m_ResLoadFinish = false;

        private Image m_Image;
        private RawImage m_RawImage;

        // 🔑 缓存池：避免重复 Sprite.Create
        private static Dictionary<Texture2D, Sprite> s_SpriteCache = new Dictionary<Texture2D, Sprite>();

        public bool autoHide
        {
            get => m_AutoHide;
            set => m_AutoHide = value;
        }

        public bool isSetNativeSize
        {
            get => m_IsSetNativeSize;
            set
            {
                m_IsSetNativeSize = value;
                if (m_IsSetNativeSize && m_ResLoadFinish)
                {
                    if (m_Image != null) m_Image.SetNativeSize();
                    if (m_RawImage != null) m_RawImage.SetNativeSize();
                }
            }
        }

        public Action onResLoadFinish
        {
            get => m_ResLoadedCallBack;
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
            get => m_LocalTextureName;
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
                        ERunner.Run(m_ResLoadedCallBack);
                    return;
                }

                m_LocalTextureName = value;
                LoadRes(m_LocalTextureName);
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

                if (m_Image != null)
                {
                    m_Image.sprite = value;
                    m_Image.enabled = value != null;
                }
            }
        }

        public void LoadRes(string path)
        {
            if (m_Image == null && m_RawImage == null)
                return;

            //if (m_Image != null) m_Image.sprite = null;
            //if (m_RawImage != null) m_RawImage.texture = null;

            if (m_AutoHide)
            {
                if (m_Image != null) m_Image.enabled = false;
                if (m_RawImage != null) m_RawImage.enabled = false;
            }

            if (m_ResLoader != null)
                m_ResLoader.ReleaseAll();

            m_LocalTextureName = path;

            if (string.IsNullOrEmpty(m_LocalTextureName))
                return;

            if (m_ResLoader == null)
                m_ResLoader = new ResLoader();

            m_ResLoadFinish = false;

            // 统一用 Texture2D 加载
            m_ResLoader.AddRes(m_LocalTextureName, ResType.Texture2D, OnResLoadFinish);
            m_ResLoader.StartLoad().Forget();
        }

        private void Awake()
        {
            // 自动获取当前物体上的 Image 和 RawImage 组件
            m_Image = GetComponent<Image>();
            m_RawImage = GetComponent<RawImage>();

            if (m_Image == null && m_RawImage == null)
            {
                Debug.LogError("AsyncTexture_New: 该物体上没有挂载 Image or RawImage 组件.", gameObject);
            }

            if (!string.IsNullOrEmpty(m_LocalTextureName))
                LoadRes(m_LocalTextureName);
        }

        private void OnDestroy()
        {
            if (m_ResLoader != null)
            {
                m_ResLoader.ReleaseAll();
                m_ResLoader = null;
            }
        }

        private void OnResLoadFinish(string res)
        {
            if (string.IsNullOrEmpty(res))
                return;

            var handle = m_ResLoader.GetAsset(res);
            var assetObj = handle.AssetObject;
            handle.Dispose();

            if (m_Image != null)
            {
                var tex = assetObj as Texture2D;
                if (tex != null)
                {
                    // 优先从缓存取 Sprite
                    if (!s_SpriteCache.TryGetValue(tex, out var sprite) || sprite == null)
                    {
                        sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                               new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
                        s_SpriteCache[tex] = sprite;
                    }

                    m_Image.sprite = sprite;
                    m_Image.enabled = true;
                    if (m_IsSetNativeSize) m_Image.SetNativeSize();
                }
            }
            else if (m_RawImage != null)
            {
                m_RawImage.texture = assetObj as Texture;
                m_RawImage.enabled = true;
                if (m_IsSetNativeSize) m_RawImage.SetNativeSize();
            }

            if (onResLoadFinish != null)
                ERunner.Run(onResLoadFinish);

            m_ResLoadFinish = true;
        }
    }
}