using System;
using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    public enum UpdateMsgID
    {
        /// <summary>
        /// 补丁包初始化失败
        /// </summary>
        InitializeFailed,
        /// <summary>
        /// 补丁流程步骤改变
        /// <para>回调函数参数: 
        /// <see cref="string"/>
        /// </para>
        /// </summary>
        PatchStepsChange,
        /// <summary>
        /// 发现更新文件
        /// <para>回调函数参数: 
        /// 下载总数量<see cref="int"/>
        /// 下载总字节数<see cref="long"/>
        /// </para>
        /// </summary>
        FoundUpdateFiles,
        /// <summary>
        /// 下载进度更新
        /// <para>回调函数参数: 
        /// <see cref="YooAsset.DownloadUpdateData"/>
        /// </para>
        /// </summary>
        DownloadUpdate,
        /// <summary>
        /// 资源版本请求失败
        /// </summary>
        PackageVersionRequestFailed,
        /// <summary>
        /// 资源清单更新失败
        /// </summary>
        PackageManifestUpdateFailed,
        /// <summary>
        /// 网络文件下载失败
        /// </summary>
        WebFileDownloadFailed,
        /// <summary>
        /// 设备足够内存
        /// </summary>
        DeviceNoEnoughSpace,
    }

    public class UpdateOperation : GameAsyncOperation
    {
        enum ESteps
        {
            None,
            Update,
            Done,
        }

        private readonly StateMachine m_Machine;
        private ESteps m_Steps = ESteps.None;
        public StateMachine machine => m_Machine;

        public UpdateOperation(Action<UpdateMsgID, float, object[]> callBack, EPlayMode playMode, string[] tag = null, bool isFirstUpdate = true)
        {
            // 创建状态机
            m_Machine = new StateMachine(this);
            m_Machine.SetProcessCallBack(callBack);

            if (isFirstUpdate)
            {
                m_Machine.AddNode<FsmInitializePackage>();
                m_Machine.AddNode<FsmRequestPackageVersion>();
                m_Machine.AddNode<FsmUpdatePackageManifest>();
                m_Machine.AddNode<FsmStartGame>();
            }

            m_Machine.AddNode<FsmCreateDownloader>();
            m_Machine.AddNode<FsmDownloadPackageFiles>();
            m_Machine.AddNode<FsmDownloadPackageOver>();
            m_Machine.AddNode<FsmClearCacheBundle>();

            m_Machine.SetBlackboardValue("PlayMode", playMode);
            m_Machine.SetBlackboardValue("NeedTagList", tag);
            m_Machine.SetBlackboardValue("IsFirstUpdate", isFirstUpdate);
        }

        protected override void OnStart()
        {
            m_Steps = ESteps.Update;
            var isFirstUpdate = (bool)m_Machine.GetBlackboardValue("IsFirstUpdate");
            if (isFirstUpdate)
            {
                m_Machine.Run<FsmInitializePackage>();
            }
            else
            {
                m_Machine.Run<FsmCreateDownloader>();
            }
        }

        protected override void OnUpdate()
        {
            if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
                return;

            if (m_Steps == ESteps.Update)
            {
                m_Machine.Update();
            }
        }

        protected override void OnAbort()
        {

        }

        public void SetFinish()
        {
            m_Steps = ESteps.Done;
            m_Machine.Stop();
            Status = EOperationStatus.Succeed;
            Debug.Log($"Package update finish!");
        }
    }
}
