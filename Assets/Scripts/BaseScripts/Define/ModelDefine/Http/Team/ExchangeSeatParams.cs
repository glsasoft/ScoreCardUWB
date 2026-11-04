using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team
{
    /// <summary>
    /// 更新队伍队长参数
    /// </summary>
    public class ExchangeSeatParams
    {
        /// <summary>
        /// 房间id
        /// </summary>
        [JsonProperty("roomId")]
        public string RoomId { get; set; }

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
        
        /// <summary>
        /// 拒绝还是同意交换队伍 2拒绝 1同意
        /// </summary>
        public int result { get; set; }
    }
}