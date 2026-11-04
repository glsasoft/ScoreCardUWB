using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;

namespace LetsPiu
{
    public class FsmInitializePackage : IStateNode
    {
        private StateMachine m_Machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            m_Machine = machine;
        }

        void IStateNode.OnEnter()
        {
            m_Machine.SendProcess(UpdateMsgID.PatchStepsChange, 0, new object[] { "初始化资源包中" });
            InitPackage();
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        async void InitPackage()
        {
            var packageName = HotUpdateDefine.DEFAULT_PACKAGE;
            var playMode = (EPlayMode)m_Machine.GetBlackboardValue("PlayMode");
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            InitializationOperation initializationOperation = null;
            // 编辑器下的模拟模式
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                Debug.Log("GetHostServerURL 资源服务器地址: " + defaultHostServer+" 备用地址: "+ fallbackHostServer);
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            await initializationOperation.ToUniTask();

            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"{initializationOperation.Error}");
                m_Machine.SendProcess(UpdateMsgID.InitializeFailed, 5);
            }
            else
            {
                m_Machine.ChangeState<FsmRequestPackageVersion>();
            }
        }

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
#if UNITY_EDITOR
            string hostServerIP = "https://scordcard-1305354732.cos.ap-shanghai.myqcloud.com";
            string appVersion = "Test";
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return $"{hostServerIP}/UnityAsset/Android/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return $"{hostServerIP}/UnityAsset/IPhone/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                return $"{hostServerIP}/UnityAsset/WebGL/{appVersion}";
            else
                return $"{hostServerIP}/PiuAsset/Android/{appVersion}";
#else
        string hostServerIP = "https://scordcard-1305354732.cos.ap-shanghai.myqcloud.com";
        string appVersion = "Test";
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/UnityAsset/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/UnityAsset/IOS/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/UnityAsset/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/UnityAsset/Android/{appVersion}";
#endif
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }
            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }
            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }
    }
}