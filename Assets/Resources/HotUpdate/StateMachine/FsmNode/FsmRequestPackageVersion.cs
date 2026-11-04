using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    public class FsmRequestPackageVersion : IStateNode
    {
        private StateMachine m_Machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            m_Machine = machine;
        }
        void IStateNode.OnEnter()
        {
            m_Machine.SendProcess(UpdateMsgID.PatchStepsChange, 5, new object[] { "请求资源版本!" });
            UpdatePackageVersion();
        }

        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        async void UpdatePackageVersion()
        {
            var package = YooAssets.GetPackage(HotUpdateDefine.DEFAULT_PACKAGE);
            var operation = package.RequestPackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning("FsmRequestPackageVersion 热更版本号请求失败:"+operation.Error);
                
                //如果存在本地缓存版本号，则使用本地缓存版本号继续后续流程
                string localPackageVersion = PlayerPrefs.GetString(HotUpdateDefine.LOCAL_PACKAGE_VERSION_KEY, string.Empty);
                if (!string.IsNullOrEmpty(localPackageVersion))
                {
                    Debug.LogWarning($"FsmRequestPackageVersion 使用本地缓存的热更版本号 : {localPackageVersion}");
                    m_Machine.SetBlackboardValue("PackageVersion", localPackageVersion);
                    m_Machine.ChangeState<FsmUpdatePackageManifest>();
                    return;
                }
                
                m_Machine.SendProcess(UpdateMsgID.PackageVersionRequestFailed, 5);
            }
            else
            {
                Debug.Log($"FsmRequestPackageVersion 请求到的热更版本号 : {operation.PackageVersion}");
                m_Machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                //将版本好缓存到本地，以便下次加载失败后直接使用
                PlayerPrefs.SetString(HotUpdateDefine.LOCAL_PACKAGE_VERSION_KEY, operation.PackageVersion);
                Debug.Log("FsmRequestPackageVersion 已缓存热更版本号到本地: " + operation.PackageVersion);
                m_Machine.ChangeState<FsmUpdatePackageManifest>();
            }
        }
    }
}