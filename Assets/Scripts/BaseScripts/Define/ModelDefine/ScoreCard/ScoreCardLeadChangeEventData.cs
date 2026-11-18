using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// LEAD_CHANGE 领先方变更 <c>msg.data</c>（NOTICE 语音）。
    /// <para>teamType：0=红队领先，1=蓝队领先；超级队长模式下缺省或为 null 表示双方队长击杀数持平。</para>
    /// </summary>
    public class ScoreCardLeadChangeEventData
    {
        [JsonProperty("level")]
        public int level;

        /// <summary>领先队伍：0=红队，1=蓝队，null=无领先方。</summary>
        [JsonProperty("teamType")]
        public int? teamType;

        public bool IsRedLeading => teamType == 0;
        public bool IsBlueLeading => teamType == 1;
        public bool IsNoLeader => teamType == null;
    }
}
