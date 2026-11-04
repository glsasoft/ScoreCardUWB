using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Players
{
    /// <summary>
    /// 房间内加入队伍参数
    /// </summary>
    public class JoinTeamParams
    {
        [JsonProperty("teamId")]
        public int teamId { get; set; }

        [JsonProperty("roomId")]
        public string roomId { get; set; }

        [JsonProperty("teamIndex")]
        public int teamIndex { get; set; }
    }
}