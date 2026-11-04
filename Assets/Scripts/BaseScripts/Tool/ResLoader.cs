using UnityEngine;
using System;
using System.Collections.Generic;
using YooAsset;
using Cysharp.Threading.Tasks;
using UnityEngine.U2D;
using LetsPiu.Base;
using System.Threading;
using LetsPiu.Base.BaseScripts.Define;

namespace LetsPiu
{
    public class ResLoader
    {
        List<string> m_LoadList = new();
        List<string> m_WaitingList = new();
        List<string> m_LoadingList = new();

        Dictionary<string, Action<string>> m_ResCallBacks;

        ResourcePackage m_DefaultPackage;

        CancellationTokenSource m_CancelToken;

        List<HandleBase> m_HandelList = new List<HandleBase>();

        public AssetHandle GetAsset(string path)
        {
            return m_DefaultPackage.LoadAssetSync(path);
        }

        public ResLoader()
        {
            try
            {
                m_DefaultPackage = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        ~ResLoader()
        {
            ReleaseAll();
        }

        // 加载进度
        public float progress
        {
            get
            {
                if (m_LoadList.Count <= 0)
                {
                    return 1;
                }
                else
                {
                    return (float)m_LoadList.Count / (m_WaitingList.Count + m_LoadingList.Count + m_LoadList.Count);
                }
            }
        }

        // 是否加载完成
        public bool isFinish { get { return (m_WaitingList.Count + m_LoadingList.Count) <= 0; } }

        public void AddRes(string resName, ResType resType)
        {
            if (string.IsNullOrEmpty(resName))
            {
                Debug.LogWarning("[ResLoader:AddRes]can't add res with name empty.");
                return;
            }

            var key = GetResKey(resName, resType);

            // 已经在加载或者已经加载
            if (FindInList(m_LoadList, key) >= 0 || FindInList(m_LoadingList, key) >= 0)
            {
                return;
            } 

            if (FindInList(m_WaitingList, key) < 0)
            {
                m_WaitingList.Add(key);
            }
        }

        public void AddRes(string resName, ResType resType, Action<string> callBack)
        {
            var key = GetResKey(resName, resType);

            if (callBack != null)
            {
                // 资源已经加载完成，直接回调
                if (FindInList(m_LoadList, key) >= 0)
                {
                    try
                    {
                        callBack(resName);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
                else
                {
                    if (m_ResCallBacks == null)
                    {
                        m_ResCallBacks = new Dictionary<string, Action<string>>();
                    }

                    m_ResCallBacks[key] = callBack;
                }
            }

            AddRes(resName, resType);
        }

        public async UniTask StartLoad(Action callBack = null)
        {
            if (this.isFinish)
            {
                try
                {
                    callBack?.Invoke();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            else
            {
                for (int i = m_WaitingList.Count - 1; i >= 0; i--)
                {
                    var ret = m_WaitingList.Get(i);
                    if (string.IsNullOrEmpty(ret))
                    {
                        continue;
                    }
                    LoadAsync(ret).Forget();
                }
                m_WaitingList.Clear();

                await UniTask.NextFrame();
                await UniTask.WaitUntil(() => m_LoadingList.Count <= 0);

                try
                {
                    callBack?.Invoke();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void ReleaseAll()
        {
            m_WaitingList.Clear();
            m_LoadingList.Clear();
            m_LoadList.Clear();

            if (m_ResCallBacks != null)
            {
                m_ResCallBacks.Clear();
            }

            if (m_CancelToken?.Token.CanBeCanceled == true)
            {
                m_CancelToken?.Cancel();
            }
            m_CancelToken?.Dispose();
            m_CancelToken = null;

            for (int i = 0; i < m_HandelList.Count; i++)
            {
                m_HandelList[i].Release();
            }
            m_HandelList.Clear();
        }

        int FindInList(List<string> fullNameList, string resKey)
        {
            for (int i = 0; i < fullNameList.Count; ++i)
            {
                if (fullNameList[i] == resKey)
                {
                    return i;
                }
            }
            return -1;
        }

        async UniTask LoadAsync(string addressKey)
        {
            if (m_LoadingList.Contains(addressKey))
            {
                return;
            }
            m_LoadingList.Add(addressKey);
            //m_WaitingList.Remove(addressKey);
            (string address, ResType type) = GetAddressAndType(addressKey);

            if (!m_DefaultPackage.CheckLocationValid(address))
            {
                if (m_ResCallBacks != null) m_ResCallBacks.Remove(addressKey);
                m_LoadingList.Remove(addressKey);
                Debug.LogError($"[ResLoader] has no valid Res. ResName: {address}");
                return;
            }

            if (m_CancelToken == null || m_CancelToken.IsCancellationRequested)
            {
                m_CancelToken?.Dispose();
                m_CancelToken = new CancellationTokenSource();
            }

            switch (type)
            {
                case ResType.UIAltas:
                    {
                        var spriteHandele = m_DefaultPackage.LoadAssetAsync<SpriteAtlas>(address);
                        m_HandelList.Add(spriteHandele);
                        await spriteHandele.ToUniTask().AttachExternalCancellation(m_CancelToken.Token);
                        break;
                    }
                case ResType.Texture:
                    {
                        var textureHandele = m_DefaultPackage.LoadAssetAsync<Texture>(address);
                        m_HandelList.Add(textureHandele);
                        await textureHandele.ToUniTask().AttachExternalCancellation(m_CancelToken.Token);
                        break;
                    }
                case ResType.Scene:
                    {
                        var sceneHandele = m_DefaultPackage.LoadSceneAsync(address);
                        m_HandelList.Add(sceneHandele);
                        await sceneHandele.ToUniTask().AttachExternalCancellation(m_CancelToken.Token);
                        break;
                    }
                case ResType.Prefab:
                    {
                        var prefabHandele = m_DefaultPackage.LoadAssetAsync<GameObject>(address);
                        m_HandelList.Add(prefabHandele);
                        await prefabHandele.ToUniTask().AttachExternalCancellation(m_CancelToken.Token);
                        break;
                    }
                case ResType.AudioClip:
                    {
                        var audioHandele = m_DefaultPackage.LoadAssetAsync<AudioClip>(address);
                        m_HandelList.Add(audioHandele);
                        await audioHandele.ToUniTask().AttachExternalCancellation(m_CancelToken.Token);
                        break;
                    }
                default:
                    break;
            }

            if (m_ResCallBacks != null && m_ResCallBacks.TryGetValue(addressKey, out var action))
            {
                try
                {
                    action?.Invoke(address);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }

                m_ResCallBacks.Remove(addressKey);
            }

            m_LoadList.Add(addressKey);
            m_LoadingList.Remove(addressKey);
        }

        string GetResKey(string address, ResType type)
        {
            return string.Format("{0}_{1}", (int)type, address);
        }

        (string, ResType) GetAddressAndType(string addressKey)
        {
            int index = addressKey.IndexOf('_');
            return (addressKey.Substring(index + 1), (ResType)int.Parse(addressKey.Substring(0, index)));
        }
    }
}