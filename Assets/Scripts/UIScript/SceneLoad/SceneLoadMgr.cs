using Cysharp.Threading.Tasks;
using LetsPiu.UIDefine;
using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using LetsPiu.Define;
using LetsPiu.Main;
using UnityEngine.SceneManagement;
using LetsPiu.Base;
using LetsPiu.Base.BaseScripts.Define;

namespace LetsPiu.UI
{
    public class SceneLoadMgr : SingletonBase<SceneLoadMgr>
    {
        ResourcePackage m_DefaultPackage;

        Dictionary<int, int> m_dicScene2StartUID;

        Dictionary<int, int> m_dicScene2Loading;

        LoadPanelBase m_LoadingPanel;
        UIID m_CompletedUIID;

        bool m_IsSwitching = false;
        SceneType m_CurrentSceneType;

        public SceneType currentSceneType => m_CurrentSceneType;

        public void SwitchScene(SceneType sceneType, Action completeCB = null)
        {
            Init();
            GameMode gameMode = GameMode.DEFAULT;
            SwitchScene(sceneType, gameMode, completeCB).Forget();
        }

        public async UniTask SwitchScene(SceneType sceneType, GameMode gameMode, Action completeCB = null)
        {
            var key = (int)sceneType * 100 + (int)gameMode;
            if (!m_dicScene2StartUID.ContainsKey(key))
            {
                Debug.LogError($"[{nameof(SceneLoadMgr)}]: sceneType：{sceneType}  gameMode：{gameMode} 该模式可打开面板未注册，注册后再来。");
                return;
            }

            if (!m_dicScene2Loading.ContainsKey(key))
            {
                Debug.LogError($"[{nameof(SceneLoadMgr)}]: sceneType：{sceneType}  gameMode：{gameMode} 该模式进度面板未注册，注册后再来。");
                return;
            }

            string nextSceneName = GetSceneName(sceneType);
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError($"{nameof(SceneLoadMgr)}: <color=red>{sceneType}</color> <color=red>{gameMode}</color> 不存在有效场景。");
                return;
            }

            if (RemoteVideoStore.EntryBlocked)
            {
                Debug.LogError("[SceneLoadMgr] 视频下载失败，请重新打开应用");
                return;
            }

            if (m_IsSwitching)
            {
                return;
            }

            var loadingUIID = (UIID)m_dicScene2Loading[key];
            m_CompletedUIID = (UIID)m_dicScene2StartUID[key];

            m_IsSwitching = true;

            await UIModule.S.ReleaseSpriteAtlas();

            m_CurrentSceneType = sceneType;
            m_LoadingPanel = null;
            UIModule.S.OpenPanel(loadingUIID, pnl =>
            {
                if (pnl is LoadPanelBase loadingPanel)
                {
                    m_LoadingPanel = loadingPanel;
                }
            });

            // 等待界面加载完成
            await UniTask.WaitUntil(() => { return m_LoadingPanel is not null; });
            await UniTask.NextFrame();
            await UniTask.NextFrame();

            UIModule.S.ResetAllPanel(uid =>
            {
                return uid == m_LoadingPanel.uiID;
            });

            // 执行服务器数据加载
            var loadType = await m_LoadingPanel.OnDataReceiveSuccess();
            if (loadType != LoadType.OK)
            {
                m_LoadingPanel.OnDataLoadFail(loadType);
                if (!RemoteVideoStore.EntryBlocked)
                {
                    m_IsSwitching = false;
                }
                return;
            }

            var scene = SceneManager.GetSceneByName(nextSceneName);
            if (!scene.IsValid())
            {
                var loadHandle = m_DefaultPackage.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
                var ratio = GetSceneRatio(sceneType);

                while (!loadHandle.IsDone)
                {
                    m_LoadingPanel.Report(loadHandle.Progress * ratio);
                    await UniTask.NextFrame();
                }
                m_LoadingPanel.Report(1 * ratio);

                SceneHandle loadHandle_1 = null;
                var addSceneName = GetAdditiveSceneName(sceneType, gameMode);
                if (!string.IsNullOrEmpty(addSceneName))
                {
                    loadHandle_1 = m_DefaultPackage.LoadSceneAsync(addSceneName, LoadSceneMode.Additive);
                    while (!loadHandle_1.IsDone)
                    {
                        m_LoadingPanel.Report(ratio + loadHandle_1.Progress * (1 - ratio));
                        await UniTask.NextFrame();
                    }
                    m_LoadingPanel.Report(1);
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(addSceneName));
                }
                else
                    m_LoadingPanel.Report(1);

                await ClearRes();

                loadHandle.Dispose();
                loadHandle_1?.Dispose();
            }
            else
            {
                m_LoadingPanel.Report(1);
                await UniTask.NextFrame();
            }

            var isLoadPlayerMode = await m_LoadingPanel.OnPrepareModel();
            if (!isLoadPlayerMode)
            {
                Debug.LogError($"{nameof(SceneLoadMgr)}: sceneType：<color=red>{sceneType}</color>  gameMode：<color=red>{gameMode}></color> 角色模型加载异常！");
                return;
            }

            AbstractPanel panel = null;
            UIModule.S.OpenPanel(m_CompletedUIID, pnl =>
            {
                panel = pnl;
            });

            await UniTask.WaitUntil(() => { return panel != null; });
            await UniTask.NextFrame();
            await UniTask.NextFrame();

            // 等待服务器确认加载完成
            var confirmDataIsLoadSuccess = await m_LoadingPanel.OnWaitServerConfirm();
            if (!confirmDataIsLoadSuccess)
            {
                Debug.LogError($"{nameof(SceneLoadMgr)}: sceneType：<color=red>{sceneType}</color>  gameMode：<color=red>{gameMode}></color> 服务器确认状态异常！");
                return;
            }

            m_LoadingPanel.OnComplete(panel);

            completeCB?.Invoke();
            //延迟1秒关闭Loading面板
            await UniTask.Delay(1000);
            UIModule.S.ClosePanel(m_LoadingPanel.uiID);
            EventManager.Instance.Broadcast(EventMsg.SceneSwitchSuccess, sceneType);
            m_IsSwitching = false;
        }

        public async UniTask AddScene(SceneType sceneType, Action completeCB = null)
        {
            var sceneName = GetSceneName(sceneType);
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
            {
                return;
            }

            var loadHandle = m_DefaultPackage.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!loadHandle.IsDone)
            {
                await UniTask.NextFrame();
            }
            loadHandle.Release();
            loadHandle.Dispose();

            completeCB?.Invoke();
        }

        public async UniTask RemoveScene(SceneType sceneType, Action completeCB = null)
        {
            var sceneName = GetSceneName(sceneType);
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
            {
                return;
            }

            var handle = SceneManager.UnloadSceneAsync(sceneName);
            await handle.ToUniTask();
            handle = null;
            completeCB?.Invoke();
        }

        void Init()
        {
            if (m_DefaultPackage is null)
            {
                m_DefaultPackage = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            }

            if (m_dicScene2StartUID is null)
            {
                m_dicScene2StartUID = new Dictionary<int, int>();

                AddStartPanelInfo(SceneType.Hall, GameMode.DEFAULT, UIID.HomePagePanel);
            }

            if (m_dicScene2Loading is null)
            {
                m_dicScene2Loading = new Dictionary<int, int>();
                AddLoadingPanelInfo(SceneType.Hall, GameMode.DEFAULT, UIID.DefaultLoadingPanel);
            }
        }

        void AddStartPanelInfo(SceneType scene, GameMode mode, UIID uiid)
        {
            m_dicScene2StartUID.Add((int)scene * 100 + (int)mode, (int)uiid);
        }

        void AddLoadingPanelInfo(SceneType scene, GameMode mode, UIID uiid)
        {
            m_dicScene2Loading.Add((int)scene * 100 + (int)mode, (int)uiid);
        }

        string GetSceneName(SceneType sceneType) => sceneType switch
        {
            SceneType.Hall => "Scene_Homepage",
            _ => "",
        };

        float GetSceneRatio(SceneType sceneType) => Mathf.Clamp01(sceneType switch
        {
            _ => 1.0f,
        });

        string GetAdditiveSceneName(SceneType sceneType, GameMode gameMode)
        {
            return "";
        }

        async UniTask ClearRes()
        {
            AudioPlayModule.S.ReleaseAll();
            EffectMgr.S.ClearCache();
            AvatarMgr.S.DisposeSafe();

            await UniTask.NextFrame();
            await UniTask.NextFrame();

            await m_DefaultPackage.UnloadUnusedAssetsAsync().ToUniTask(); //卸载引用计数为0的AB包，AssetBundle.Unload(true)

            await Resources.UnloadUnusedAssets().ToUniTask();
        }
    }
}
