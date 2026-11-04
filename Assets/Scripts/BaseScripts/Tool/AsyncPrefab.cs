using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// 通用异步加载Prefab
    /// </summary>
    public class AsyncPrefab : MonoBehaviour
    {
        [SerializeField] private string m_LocalObjectName;

        private ResLoader m_ResLoader;
        private Action m_ResLoadedCallBack;
        private bool m_ResLoadFinish = false;

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

        public string localObjectName
        {
            get => m_LocalObjectName;
            set
            {
                if (!Application.isPlaying)
                {
                    m_LocalObjectName = value;
                    return;
                }

                if (m_LocalObjectName == value)
                {
                    if (m_ResLoadedCallBack != null && m_ResLoadFinish)
                        ERunner.Run(m_ResLoadedCallBack);
                    return;
                }

                m_LocalObjectName = value;
                LoadRes(m_LocalObjectName);
            }
        }

        public void LoadRes(string path)
        {
            if (m_ResLoader != null)
                m_ResLoader.ReleaseAll();

            m_LocalObjectName = path;

            if (string.IsNullOrEmpty(m_LocalObjectName))
                return;

            if (m_ResLoader == null)
                m_ResLoader = new ResLoader();

            m_ResLoadFinish = false;

            m_ResLoader.AddRes(m_LocalObjectName, ResType.Prefab, OnResLoadFinish);
            m_ResLoader.StartLoad().Forget();
        }

        private void Awake()
        {
            if (!string.IsNullOrEmpty(m_LocalObjectName))
                LoadRes(m_LocalObjectName);
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

            var go = assetObj as GameObject;
            if (go != null)
            {
                var instance = Instantiate(go, transform);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.SetActive(true);
            }

            if (onResLoadFinish != null)
                ERunner.Run(onResLoadFinish);

            m_ResLoadFinish = true;
        }
    }
}