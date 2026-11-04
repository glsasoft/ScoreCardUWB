namespace LetsPiu
{
    public class FsmStartGame : IStateNode
    {
        UpdateOperation _owner;
        StateMachine m_Machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            m_Machine = machine;
            _owner = machine.Owner as UpdateOperation;
        }
        void IStateNode.OnEnter()
        {
            m_Machine.SendProcess(UpdateMsgID.PatchStepsChange, 100, new string[] { "游戏开始！" });
            _owner.SetFinish();
        }

        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }
    }
}