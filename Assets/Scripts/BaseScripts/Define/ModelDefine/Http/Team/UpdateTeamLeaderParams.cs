using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team
{
    /// <summary>
    /// 更新队伍队长参数
    /// </summary>
    public class UpdateTeamLeaderParams
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
    }
}