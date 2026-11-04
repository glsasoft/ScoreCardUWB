using Cysharp.Threading.Tasks;
using LetsPiu.Base;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Send;
using LetsPiu.Common.Network.Model.Mqtt;
using LetsPiu.Define;
using LetsPiu.Main;
using LetsPiu.UIDefine;
using System;
using System.Threading;
using LetsPiu.UI.UIScript.Common.UI;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LetsPiu.UI
{
    public abstract class LoadPanelBase : AbstractPanel, IProgress<float>
    {
        [SerializeField] LoadingProgressBar m_Slider;

        protected GameMode m_GameMode = GameMode.DEFAULT;

        // 进度条所有操作完成要在99%处等待服务器确认其他玩家加载完成
        protected float m_DataSliderMax = 0.49f;
        protected float m_SceneShowSliderMax = 0.95f;
        protected float m_TotalShowSlider = 0.99f;

        public override int sortIndex => UISortIndex.LOADING_PANEL;

        bool bServerIsConfirm;
        CancellationTokenSource m_LoadingFinishCancelToken = new CancellationTokenSource();

        public void SetSliderAmount(float fillAmount)
        {
            m_Slider.SetProgress(fillAmount);
        }

        public void ForceAgreeServerConfirm()
        {
            bServerIsConfirm = true;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            bServerIsConfirm = false;
            SetSliderAmount(0);
        }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.AddEvent<GameState>(EventMsg.MqttGameStateChanged, MqttPlayerStateChanged);
            EventManager.Instance.AddEvent(EventMsg.LoginTokenTimeout, LoginTokenTimeout);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.Instance.RemoveEvent<GameState>(EventMsg.MqttGameStateChanged, MqttPlayerStateChanged);
            EventManager.Instance.RemoveEvent(EventMsg.LoginTokenTimeout, LoginTokenTimeout);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_LoadingFinishCancelToken is not null)
            {
                if (!m_LoadingFinishCancelToken.Token.CanBeCanceled)
                {
                    m_LoadingFinishCancelToken.Cancel();
                }

                m_LoadingFinishCancelToken.Dispose();
                m_LoadingFinishCancelToken = null;
            }
        }

        /// <summary>
        /// 报告场景加载进度
        /// </summary>
        /// <param name="value"></param>
        public void Report(float value)
        {
            SetSliderAmount(m_DataSliderMax + value * (m_SceneShowSliderMax - m_DataSliderMax));
        }

        /// <summary>
        /// 加载完成以后的逻辑
        /// </summary>
        /// <param name="panel"></param>
        public virtual void OnComplete(AbstractPanel panel)
        {
            m_Slider.SetProgress(1);
        }

        /// <summary>
        /// 开始准备模型
        /// </summary>
        public virtual async UniTask<bool> OnPrepareModel()
        {
            await UniTask.NextFrame();
            SetSliderAmount(m_TotalShowSlider);
            return true;
        }

        public virtual async UniTask<bool> OnWaitServerConfirm()
        {
            if (!bServerIsConfirm && AppStateManager.Instance.Play != null)
            {
                // 先确认一下状态，可能服务器已经判定超时了
                MqttPlayerStateChanged(AppStateManager.Instance.Play.playStatus);
            }

            if (!bServerIsConfirm)
            {
                // 通知服务器准备工作完成
                await UniTask.WaitUntil(() =>
                {
                    return bServerIsConfirm;
                }, cancellationToken: m_LoadingFinishCancelToken.Token);
            }
            return bServerIsConfirm;
        }

        public virtual async UniTask<LoadType> OnDataReceiveSuccess()
        {
            await UniTask.NextFrame();
            SetSliderAmount(m_DataSliderMax);
            return LoadType.OK;
        }

        public virtual void OnDataLoadFail(LoadType type)
        {
            Debug.LogError($"{nameof(SceneLoadMgr)}:加载失败 {type}");
        }

        void MqttPlayerStateChanged(GameState gameState)
        {
            if (!bServerIsConfirm)
            {
                bServerIsConfirm = gameState != GameState.LOADING;
            }
        }

        void LoginTokenTimeout()
        {
            CloseSelf();
        }
    }
}
