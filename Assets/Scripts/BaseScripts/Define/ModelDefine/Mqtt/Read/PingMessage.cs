using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    public class PingMessage
    {
        /// <summary>
        /// 场次状态
        /// </summary>
        public GameState? playStatus;
        /// <summary>
        /// 毒圈经度
        /// </summary>
        public string longitude;
        /// <summary>
        /// 毒圈维度
        /// </summary>
        public string latitude;
        /// <summary>
        /// 毒圈半径
        /// </summary>
        public string radius;
        public int? index { get; set; }

        public PlayerState? playerStatus;
        public GameMode? roomType;
        public GameMode? groupType;
        public GameState? roomStatus;
        public MatchStatus? groupStatus;

        public int groupPlayerCount;

        /// <summary>
        /// matchId
        /// </summary>
        public string matchId;

        /// <summary>
        /// 接受对局的人数
        /// </summary>
        public int confirmedCount;

        /// <summary>
        /// 有值说明要更新玩家初始子弹数
        /// </summary>
        public int? initBullet = null;
        /// <summary>
        /// 有值说明要更新玩家当前剩余子弹
        /// </summary>
        public int? bullet = null;

        /// <summary>
        /// 房间类型
        /// </summary>
        public int? type = null;
    }
}
