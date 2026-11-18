using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// S6 战绩回顾系列（subCode = S6-1 / S6-2 / S6-3）<c>msg.data</c> 的反序列化目标。
    /// <para>每条消息包含从比赛开始至当前采样时刻的完整历史列表（累计追加，每 5 秒一条）。</para>
    /// <para>三个 subCode 共用同一份数据结构，区别仅在 <see cref="ScoreCardReviewPanel"/>
    /// 对其展示流程的不同：S6-1 动态回放，S6-2 满填充→合拢→积分曲线，S6-3 保留积分曲线 + 结果页。</para>
    /// </summary>
    public class ScoreCardSettlementData
    {
        /// <summary>
        /// 本场比赛总时长，单位：分钟（5 ~ 20）。用于客户端动态设置进度条范围。
        /// 值 ≤ 0 表示服务端未携带，客户端应使用 <see cref="ScoreCardMatchData.matchDuration"/> 兜底。
        /// </summary>
        [JsonProperty("matchDuration")]
        public int matchDuration;

        /// <summary>
        /// 真实比赛时长（秒）= endTime - beginTime，由 <see cref="ScoreCardBaseData"/> 计算得出。
        /// 用于战斗回顾的进度条/时间/动画时长；≤0 表示未知，回顾时回退到最后一帧采样时间。
        /// </summary>
        public int actualMatchSec;

        /// <summary>
        /// 结算采样历史列表，按 <see cref="ScoreCardSettlementSample.time"/> 升序排列。
        /// </summary>
        [JsonProperty("settlementList")]
        public List<ScoreCardSettlementSample> settlementList;

        /// <summary>红队队长击杀明细（与协议 <c>redCaptainKills</c> 相同，<c>relativeSecond</c> 为开赛后秒数）。</summary>
        [JsonProperty("redCaptainKills")]
        public List<ScoreCardCaptainKillEvent> redCaptainKills;

        /// <summary>蓝队队长击杀明细。</summary>
        [JsonProperty("blueCaptainKills")]
        public List<ScoreCardCaptainKillEvent> blueCaptainKills;

        /// <summary>
        /// 超级队长逐帧统计曲线（与协议 <c>captainStatLine</c> 相同），
        /// 用于回放时驱动两位队长的实时血条/击杀/伤害/积分。
        /// </summary>
        [JsonProperty("captainStatLine")]
        public List<ScoreCardCaptainStatSample> captainStatLine;
    }

    /// <summary>
    /// 单次结算采样快照，对应服务器每 5 秒推送一条的数据。
    /// </summary>
    public class ScoreCardSettlementSample
    {
        /// <summary>采样时刻，单位：秒（5, 10, 15 …）。</summary>
        [JsonProperty("time")]
        public int time;

        /// <summary>红队（team1）本次采样的累计数据。</summary>
        [JsonProperty("team1")]
        public ScoreCardSettlementTeam team1;

        /// <summary>蓝队（team2）本次采样的累计数据。</summary>
        [JsonProperty("team2")]
        public ScoreCardSettlementTeam team2;
    }

    /// <summary>
    /// 单队结算数据快照（累计值）。
    /// </summary>
    public class ScoreCardSettlementTeam
    {
        /// <summary>该次采样的累计积分。</summary>
        [JsonProperty("score")]
        public int score;

        /// <summary>该次采样的累计击杀数（statLine 键名 kills，与玩家 killNum 不同）。</summary>
        [JsonProperty("kills")]
        public int kills;

        /// <summary>该次采样的累计死亡数（statLine 键名 deaths，与玩家 beKillNum 不同）。</summary>
        [JsonProperty("deaths")]
        public int deaths;

        /// <summary>该次采样的累计助攻数（statLine 键名 assists，与 baseData 队伍 assistNum 不同）。</summary>
        [JsonProperty("assists")]
        public int assists;

        /// <summary>该次采样的累计伤害值。</summary>
        [JsonProperty("damage")]
        public int damage;

        /// <summary>
        /// 该次采样时刻的实时存活人数（statLine 键名 aliveCount）。
        /// 生化模式人类/丧尸阵营会随感染变化，回顾存活须读此字段，不可用终局名单反推。
        /// </summary>
        [JsonProperty("aliveCount")]
        public int aliveCount;
    }
}
