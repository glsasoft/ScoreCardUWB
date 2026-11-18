using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// CAPTAIN_DOWN 超级队长阵亡 <c>msg.data</c>（NOTICE 语音）。
    /// <para>teamType：0=红队队长阵亡，1=蓝队队长阵亡。</para>
    /// </summary>
    public class ScoreCardCaptainDownEventData
    {
        [JsonProperty("teamType")]
        public int teamType;

        public bool IsRedTeam => teamType == 0;
        public bool IsBlueTeam => teamType == 1;
    }
}
