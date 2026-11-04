using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    public class FsmDownloadPackageFiles : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            _machine.SendProcess(UpdateMsgID.PatchStepsChange, 20, new object[] { "开始下载资源文件!" });
            BeginDownload();
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        async void BeginDownload()
        {
            var downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("Downloader");
            if (downloader == null)
            {
                Debug.Log("下载器无效，请重新下载");
                return;
            }
            downloader.DownloadErrorCallback = (errorData) =>
            {
                // 下载失败
                _machine.SendProcess(UpdateMsgID.WebFileDownloadFailed, 20, new object[] { errorData });
            };

            downloader.DownloadUpdateCallback = (info) =>
            {
                var totalProgress = 70;

                // 通知下载进度 下载完成 进度到了90
                _machine.SendProcess(UpdateMsgID.DownloadUpdate, 20 + totalProgress * info.Progress, new object[] { info });
            };

            downloader.BeginDownload();
            await downloader.ToUniTask();

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
            {
                return;
            }

            _machine.ChangeState<FsmDownloadPackageOver>();
        }
    }
}