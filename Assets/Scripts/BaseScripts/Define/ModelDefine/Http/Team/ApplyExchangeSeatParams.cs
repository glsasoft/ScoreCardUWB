using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team
{
    /// <summary>
    /// 申请更换队伍参数
    /// </summary>
    public class ApplyExchangeSeatParams
    {
        /// <summary>
        /// 房间id
        /// </summary>
        [JsonProperty("roomId")]
        public long RoomId { get; set; }

        /// <summary>
        /// 玩家短编号
        /// </summary>
        [JsonProperty("shortId")]
        public long ShortId { get; set; }

        /// <summary>
        /// 目标玩家短编号
        /// </summary>
        [JsonProperty("targetShortId")]
        public long TargetShortId { get; set; }
    }
}