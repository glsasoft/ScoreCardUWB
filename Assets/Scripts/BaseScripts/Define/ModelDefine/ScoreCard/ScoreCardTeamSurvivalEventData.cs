using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// TEAM_SURVIVAL 队伍残局提示 <c>msg.data</c>（NOTICE 语音，歼灭模式仅剩1/3人存活）。
    /// <para>teamType：0=红队，1=蓝队；aliveCount：该队当前存活人数。</para>
    /// </summary>
    public class ScoreCardTeamSurvivalEventData
    {
        [JsonProperty("teamType")]
        public int teamType;

        [JsonProperty("aliveCount")]
        public int aliveCount;

        public bool IsRedTeam => teamType == 0;
        public bool IsBlueTeam => teamType == 1;
    }
}
