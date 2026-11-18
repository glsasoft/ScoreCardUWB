using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// subCode = <see cref="ScoreCardSubCode.WinRate"/> 时内部模型；
    /// 新协议 MQTT data 使用 <c>winRateLine</c>，由 <see cref="ScoreCardDataNormalizer.ToWinRateData"/> 映射为本结构。
    /// 每条消息包含从比赛开始至当前采样时刻的完整历史列表（累计追加，每 5 秒一条）。
    /// </summary>
    public class ScoreCardWinRateData
    {
        /// <summary>
        /// 本场比赛总时长，单位：分钟（5 ~ 20）。用于客户端动态设置 X 轴范围和标签。
        /// 值 ≤ 0 表示服务端未携带，客户端应使用 <see cref="ScoreCardMatchData.matchDuration"/> 兜底。
        /// </summary>
        [JsonProperty("matchDuration")]
        public int matchDuration;

        /// <summary>
        /// 胜率采样历史列表，按 <see cref="ScoreCardWinRateSample.time"/> 升序排列。
        /// </summary>
        [JsonProperty("winRateList")]
        public List<ScoreCardWinRateSample> winRateList;
    }

    /// <summary>
    /// 单次胜率采样快照，对应服务器每 5 秒推送一条的数据。
    /// </summary>
    public class ScoreCardWinRateSample
    {
        /// <summary>采样时刻，单位：秒（5, 10, 15 … 480）。</summary>
        [JsonProperty("time")]
        public int time;

        /// <summary>红队（team1）胜率信息。</summary>
        [JsonProperty("team1")]
        public ScoreCardWinRateTeam team1;

        /// <summary>蓝队（team2）胜率信息。</summary>
        [JsonProperty("team2")]
        public ScoreCardWinRateTeam team2;
    }

    /// <summary>
    /// 单队胜率数据。
    /// </summary>
    public class ScoreCardWinRateTeam
    {
        /// <summary>该次采样的胜率，取值 0.00 ~ 100.00。红蓝互补（两者之和 = 100）。</summary>
        [JsonProperty("winRate")]
        public float winRate;
    }
}
