using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team
{
    /// <summary>
    /// 解散队伍参数
    /// </summary>
    public class DisbandTeamParams
    {
        /// <summary>
        /// 房间id
        /// </summary>
        [JsonProperty("roomId")]
        public string RoomId { get; set; }

        /// <summary>
        /// 队伍id
        /// </summary>
        [JsonProperty("teamId")]
        public long TeamId { get; set; }
    }
}