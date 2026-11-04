using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    public class FsmUpdatePackageManifest : IStateNode
    {
        private StateMachine m_Machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            m_Machine = machine;
        }
        void IStateNode.OnEnter()
        {
            m_Machine.SendProcess(UpdateMsgID.PatchStepsChange, 10, new object[] { "更新资源清单!" });
            UpdateManifest();
        }

        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        async void UpdateManifest()
        {
            var packageVersion = (string)m_Machine.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(HotUpdateDefine.DEFAULT_PACKAGE);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                m_Machine.SendProcess(UpdateMsgID.PackageManifestUpdateFailed, 10);
                return; ;
            }
            else
            {
                m_Machine.ChangeState<FsmCreateDownloader>();
            }
        }
    }
}