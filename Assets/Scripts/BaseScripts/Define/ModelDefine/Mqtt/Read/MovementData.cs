using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    // 值类型传递会发生拷贝，事件采用类
    public class MovementData
    {
        public float Velocity = 0;
        public bool IsGrounded = false;
        public bool IsRunning = false;
    }
}
