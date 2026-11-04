using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    public class FsmCreateDownloader : IStateNode
    {
        private StateMachine m_Machine;
        ResourceDownloaderOperation downloader = null;

        void IStateNode.OnCreate(StateMachine machine)
        {
            m_Machine = machine;
        }
        void IStateNode.OnEnter()
        {
            m_Machine.SendProcess(UpdateMsgID.PatchStepsChange, 15, new object[] { "创建资源下载器!" });
            CreateDownloader();
        }
        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        void CreateDownloader()
        {
            var tagList = (string[])m_Machine.GetBlackboardValue("NeedTagList");
            var package = YooAssets.GetPackage(HotUpdateDefine.DEFAULT_PACKAGE);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;

            if (downloader == null)
            {
                if (tagList == null)
                {
                    Debug.Log("全部下载");
                    downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
                }
                else
                {
                    Debug.Log("下载部分标签:" + tagList.Length);
                    downloader = package.CreateResourceDownloader(tagList, downloadingMaxNum, failedTryAgain);
                }
            }

            m_Machine.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("Not found any download files !");
                // 离线全包模式：本地已包含全部资源，直接进入游戏启动
                m_Machine.ChangeState<FsmStartGame>();
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                long totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
                // 检测完成以后 需要询问用户是否更新资源包。
                m_Machine.SendProcess(UpdateMsgID.FoundUpdateFiles, 20, new object[] { totalDownloadCount, totalDownloadBytes });
#else
                //下载前检测磁盘空间不足
                if (Utils.Utils.CheckStorageSpace(totalDownloadBytes))
                {
                    // 检测完成以后 需要询问用户是否更新资源包。
                    m_Machine.SendProcess(UpdateMsgID.FoundUpdateFiles, 20, new object[] { totalDownloadCount, totalDownloadBytes });
                }
                else
                {
                    // 内存不足  通知用户清理
                    m_Machine.SendProcess(UpdateMsgID.DeviceNoEnoughSpace, 15, new object[] { totalDownloadBytes });
                }
#endif
            }
        }
    }
}