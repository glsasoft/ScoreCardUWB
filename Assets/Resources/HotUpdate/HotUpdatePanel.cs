using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LetsPiu
{
    public class HotUpdatePanel : MonoBehaviour
    {
        [SerializeField] Image m_Slider;
        [SerializeField] GameObject m_MessageBoxObj;
        [SerializeField] TMP_Text m_ContentTxt;
        [SerializeField] Button m_BtnOK;

        class MessageInfo
        {
            public string warningInfo;
            public Action callBack;

            public MessageInfo(string warnInfo, Action callBack)
            {
                warningInfo = warnInfo;
                this.callBack = callBack;
            }
        }

        Queue<MessageInfo> m_MsgInfoQueue = new();
        bool m_IsShowMsg;
        StateMachine m_Machine;

        void Awake()
        {
            SetMsgBoxVisiable(false);
            m_Slider.fillAmount = 0;
            m_ContentTxt.text = string.Empty;
        }

        public void SetMachine(StateMachine stateMachine)
        {
            m_Machine = stateMachine;
        }

        public void UpdateCallBack(UpdateMsgID msgId, float process, params object[] paramList)
        {
            Debug.Log($"进度打印：：{process}");
            string info = null;
            switch (msgId)
            {
                case UpdateMsgID.InitializeFailed:
                    InitializeFailed();
                    break;
                case UpdateMsgID.PatchStepsChange:
                    info = paramList[0] as string;
                    PatchStepsChange(info);
                    break;
                case UpdateMsgID.FoundUpdateFiles:
                    var totalCount = (long)paramList[0];
                    var totalBytes = (long)paramList[1];
                    FoundUpdateFiles(totalCount, totalBytes);
                    break;
                case UpdateMsgID.DownloadUpdate:
                    var downloadData = (DownloadUpdateData)paramList[0];
                    DownloadUpdate(downloadData);
                    break;
                case UpdateMsgID.PackageVersionRequestFailed:
                    PackageVersionRequestFailed();
                    break;
                case UpdateMsgID.PackageManifestUpdateFailed:
                    PackageManifestUpdateFailed();
                    break;
                case UpdateMsgID.WebFileDownloadFailed:
                    var errodData = (DownloadErrorData)paramList[0];
                    WebFileDownloadFailed(errodData);
                    break;
                case UpdateMsgID.DeviceNoEnoughSpace:
                    var storageCount = (long)paramList[0];
                    DeviceNoEnoughSpace(storageCount);
                    break;
                default:
                    break;
            }

            m_Slider.fillAmount = process / 100f;
        }

        void DeviceNoEnoughSpace(long count)
        {
            SetMessageBox($"当前设备没有足够的内存，还需要 : {count / 1024 / 1024}MB,请清理后再尝试!", () =>
            {
                m_Machine.ChangeState<FsmCreateDownloader>();
            });
        }

        void WebFileDownloadFailed(DownloadErrorData error)
        {
            SetMessageBox($"文件 : {error.FileName}下载失败，请重新下载", () =>
            {
                m_Machine.ChangeState<FsmDownloadPackageFiles>();
            });
        }

        void PackageManifestUpdateFailed()
        {
            SetMessageBox($"网络异常，无法检测最新版本，请检查网络状态！", () =>
            {
                m_Machine.ChangeState<FsmUpdatePackageManifest>();
            });
        }

        void PackageVersionRequestFailed()
        {
            // 离线全包模式下不应该走到这里（OfflinePlayMode 不请求远端版本号）。
            // 万一走到这里，直接进入游戏启动流程，避免阻塞。
            Debug.LogWarning("[HotUpdatePanel] PackageVersionRequestFailed in offline-only mode, fallback to start game directly.");
            m_Machine.ChangeState<FsmStartGame>();
        }

        void DownloadUpdate(DownloadUpdateData msg)
        {
        }

        void FoundUpdateFiles(long TotalCount, long TotalSizeBytes)
        {
            float sizeMB = TotalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            string totalSizeMB = sizeMB.ToString("f1");
            // SetMessageBox($"发现可更新文件, 文件数量 {TotalCount}， 文件体积 {totalSizeMB}MB", () =>
            // {
            //     if (!ShowNetworkMessage())
            //         m_Machine.ChangeState<FsmDownloadPackageFiles>();
            // });
            //直接更新不提示
            m_Machine.ChangeState<FsmDownloadPackageFiles>();
        }

        bool ShowNetworkMessage()
        {
            if (Application.internetReachability != NetworkReachability.ReachableViaCarrierDataNetwork)
                return false;

            //9:仅WiFi，10:移动网络及WiFi
            if (PlayerPrefs.GetInt("SettingID_AutoDownloadRes", -1) == 9)
            {
                SetMessageBox("当前是移动网络，打开WiFi更省流量！", () =>
                {
                    m_Machine.ChangeState<FsmDownloadPackageFiles>();
                });
                return true;
            }

            return false;
        }

        void PatchStepsChange(string info)
        {
            Debug.Log(info);
        }

        void InitializeFailed()
        {
            SetMessageBox($"初始化更新失败 !", () =>
            {
                m_Machine.ChangeState<FsmInitializePackage>();
            });
        }

        // 设置对话框消息
        void SetMessageBox(string content, Action ok)
        {
            Debug.Log($"收到消息：{content}");
            // 封装一个消息体
            MessageInfo msgInfo = new MessageInfo(content, ok);
            m_MsgInfoQueue.Enqueue(msgInfo);

            // 执行播放事件
            OpenMsgBox();
        }

        // 展示对话框
        void OpenMsgBox()
        {
            if (m_IsShowMsg)
            {
                return;
            }

            if (m_MsgInfoQueue.Count <= 0)
            {
                return;
            }

            SetMsgBoxVisiable(true);

            var msg = m_MsgInfoQueue.Dequeue();
            Debug.Log($"播放消息：{msg.warningInfo}");

            m_ContentTxt.text = msg.warningInfo;
            m_BtnOK.onClick.RemoveAllListeners();
            m_BtnOK.onClick.AddListener(async () =>
            {
                msg.callBack.Invoke();
                m_MessageBoxObj.SetActive(false);
                // 每个消息之间的显示 要等待两帧
                await UniTask.DelayFrame(2);
                m_IsShowMsg = false;
                // 执行播放事件
                OpenMsgBox();
            });
        }

        // 设置对话框状态
        void SetMsgBoxVisiable(bool bIsVisiable)
        {
            m_MessageBoxObj.SetActive(bIsVisiable);
            m_IsShowMsg = bIsVisiable;
        }
    }
}
