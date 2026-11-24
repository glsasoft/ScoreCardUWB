using System;

namespace LetsPiu.Character
{
    /// <summary>
    /// 服务器一次上报的单个玩家状态。坐标是场馆本地坐标，单位米，Y 朝上。
    /// 没有朝向，客户端按水平位移决定脸朝哪。
    /// time 是秒。同一条流里保持单调即可，Unix 秒或开局后的秒数都可以。0 表示用收到的时刻。
    /// 主线程调用 VenuePlayerSync.Push / PushAll 喂进来。
    /// </summary>
    [Serializable]
    public class VenuePlayerSnapshot
    {
        public int id;
        public int team;
        public string name;
        public float x;
        public float y;
        public float z;
        public int hp;
        public int maxHp;
        public bool firing;
        public double time;
    }
}
