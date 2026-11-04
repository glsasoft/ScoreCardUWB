namespace LetsPiu
{
    public class FsmDownloadPackageOver : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            _machine.SendProcess(UpdateMsgID.PatchStepsChange, 91, new object[] { "资源文件下载完毕！" });
            _machine.ChangeState<FsmClearCacheBundle>();
        }

        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }
    }
}
