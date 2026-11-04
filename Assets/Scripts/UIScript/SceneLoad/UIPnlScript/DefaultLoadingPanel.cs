using Cysharp.Threading.Tasks;
using LetsPiu;
using LetsPiu.Define;
using LetsPiu.UI.UIScript.GameScripts.Common.UI;
using TMPro;
using UnityEngine;

namespace LetsPiu.UI.UIScript.SceneLoad.UIPnlScript
{
    /// <summary>
    /// 登录/设备注册成功后，进入游戏大厅的加载面板。
    /// 先从桶上下载介绍视频和首页广告，全部成功后再对齐系统时间并连接 MQTT。
    /// </summary>
    public class DefaultLoadingPanel : LoadPanelBase
    {
        const string VideoFailTip = "视频下载失败，请检查网络后重新打开应用";

        [SerializeField] TMP_Text m_ProgressTip;

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            ForceAgreeServerConfirm();
        }

        public override async UniTask<LoadType> OnDataReceiveSuccess()
        {
            ShowTip("正在检查视频资源...", Color.white);
            bool ready = await RemoteVideoStore.EnsureReady((progress, text) =>
            {
                SetSliderAmount(progress * m_DataSliderMax);
                ShowTip(text, Color.white);
            });
            if (!ready)
            {
                ShowTip(VideoFailTip, new Color(1f, 0.35f, 0.35f));
                return LoadType.LoadSeverDataFail;
            }

            ShowTip("正在同步服务器时间...", Color.white);
            Debug.Log("[DefaultLoadingPanel] 视频准备完成，开始同步服务器时间");
            bool synced = await ServerTimeSync.SyncAsync();
            if (!synced)
            {
                ToastTMPPanel.Show("时间同步失败，请检查网络后重试");
                Debug.LogError("[DefaultLoadingPanel] 时间同步失败，取消连接 MQTT");
                return LoadType.LoadSeverDataFail;
            }

            ShowTip("正在连接服务器...", Color.white);
            Debug.Log("[DefaultLoadingPanel] 时间同步成功，开始连接 MQTT");
            var connected = await MqttConnector.Instance.ConnectAsync(MqttConnector.ConnectType.ScoreCard);
            if (!connected)
            {
                MqttConnector.Instance.Disconnect();
                ToastTMPPanel.Show("服务器连接失败，请检查网络连接或稍后再试");
                return LoadType.LoadSeverDataFail;
            }

            SetSliderAmount(m_DataSliderMax);
            return LoadType.OK;
        }

        void ShowTip(string message, Color color)
        {
            if (m_ProgressTip == null) return;
            if (!m_ProgressTip.gameObject.activeSelf)
            {
                m_ProgressTip.gameObject.SetActive(true);
            }

            m_ProgressTip.color = color;
            m_ProgressTip.text = message;
        }
    }
}
