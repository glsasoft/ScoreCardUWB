using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 比分牌炸弹模式 (modeType="5") 炸弹记录条目。
    /// 历史上叫 <c>ScoreCard5SBombRecord</c>，2026-05-21 起去掉 <c>5S</c> 前缀。
    /// </summary>
    public class ScoreCardBombRecord
    {
        [JsonProperty("id")]       public string id;

        /// <summary>炸弹ID</summary>
        [JsonProperty("bombId")]   public string bombId;

        /// <summary>操作玩家ID</summary>
        [JsonProperty("playerId")] public string playerId;

        /// <summary>队伍类型 "0"=红队 "1"=蓝队 等</summary>
        [JsonProperty("teamType")] public string teamType;

        /// <summary>
        /// 炸弹状态：0=关闭 1=开启 2=就绪 3=解锁中 4=解锁成功 5=就绪爆炸 6=其它
        /// </summary>
        [JsonProperty("status")]   public string status;

        /// <summary>获取状态中文描述</summary>
        public string GetStatusText()
        {
            return status switch
            {
                "1" => "准备",
                "2" => "就绪",
                "3" => "解锁中",
                "4" => "已解锁",
                "5" => "已爆炸",
                _   => "未知"
            };
        }
    }
}
