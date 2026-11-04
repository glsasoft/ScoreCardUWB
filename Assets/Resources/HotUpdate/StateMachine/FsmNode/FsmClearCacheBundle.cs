using YooAsset;

namespace LetsPiu
{
    public class FsmClearCacheBundle : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            _machine.SendProcess(UpdateMsgID.PatchStepsChange, 92, new object[] { "清理未使用的缓存文件！" });
            var package = YooAssets.GetPackage(HotUpdateDefine.DEFAULT_PACKAGE);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += Operation_Completed;
        }

        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        void Operation_Completed(AsyncOperationBase obj)
        {
            // 离线全包模式：不再需要加载热更 dll，直接进入游戏启动状态
            _machine.ChangeState<FsmStartGame>();
        }
    }
}