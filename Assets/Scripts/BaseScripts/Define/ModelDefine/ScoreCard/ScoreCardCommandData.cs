using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 新协议 <see cref="ScoreCardCommand.data"/> 外壳：场次主体在 <see cref="baseData"/>，
    /// 结算曲线在 <see cref="statLine"/>；PLAYER_JOIN/LEAVE 轻量载荷仅有 <see cref="playId"/> + <see cref="playerList"/>。
    /// </summary>
    public class ScoreCardCommandData
    {
        [JsonProperty("baseData")]
        public ScoreCardBaseData baseData;

        /// <summary>竞速靶场模式回合内数据（与 <see cref="baseData"/> 平级；BEGIN_PLAY 两者都带，RANGE_UPDATE 仅此项）。</summary>
        [JsonProperty("rangeData")]
        public ScoreCardRangeData rangeData;

        [JsonProperty("statLine")]
        public List<ScoreCardStatLineSample> statLine;

        /// <summary>红队队长击杀明细，与 <see cref="statLine"/> 平级；每项一条击杀，<c>relativeSecond</c> 为开赛后秒数。</summary>
        [JsonProperty("redCaptainKills")]
        public List<ScoreCardCaptainKillEvent> redCaptainKills;

        /// <summary>蓝队队长击杀明细，结构与 <see cref="redCaptainKills"/> 相同。</summary>
        [JsonProperty("blueCaptainKills")]
        public List<ScoreCardCaptainKillEvent> blueCaptainKills;

        /// <summary>
        /// 超级队长逐帧统计曲线，与 <see cref="statLine"/> 平级，每约 8 秒一条，
        /// 用于 S6-1 熊猫讲解顶部两位队长的实时血条/击杀/伤害/积分展示。
        /// </summary>
        [JsonProperty("captainStatLine")]
        public List<ScoreCardCaptainStatSample> captainStatLine;

        [JsonProperty("playId")]
        public string playId;

        [JsonProperty("playerList")]
        public List<ScoreCardPlayer> playerList;

        [JsonProperty("winRateLine")]
        public List<ScoreCardWinRateLineSample> winRateLine;

        [JsonProperty("matchDuration")]
        public int matchDuration;
    }

    /// <summary>
    /// 新协议场次基础数据（嵌套于 <see cref="ScoreCardCommandData.baseData"/>）。
    /// 队伍数量与各队玩家由同级 <c>team1</c>~<c>team5</c> 表达，玩家在 <c>teamN.players</c> 中。
    /// </summary>
    public class ScoreCardBaseData
    {
        [JsonProperty("storeName")]     public string storeName;
        [JsonProperty("modeType")]      public string modeType;
        [JsonProperty("modeName")]      public string modeName;
        [JsonProperty("playId")]        public string playId;
        [JsonProperty("placeCode")]     public string placeCode;
        [JsonProperty("playName")]      public string playName;
        [JsonProperty("playStatus")]    public int playStatus;
        [JsonProperty("beginTime")]     public string beginTime;
        [JsonProperty("endTime")]       public string endTime;
        [JsonProperty("matchDuration")] public int matchDuration;

        /// <summary>复活模式（荣耀积分/攻防积分）玩家死亡后的复活倒计时（秒）。</summary>
        [JsonProperty("reviveCd")]      public int reviveCd;

        [JsonProperty("team1")] public ScoreCardTeamInfo team1;
        [JsonProperty("team2")] public ScoreCardTeamInfo team2;
        [JsonProperty("team3")] public ScoreCardTeamInfo team3;
        [JsonProperty("team4")] public ScoreCardTeamInfo team4;
        [JsonProperty("team5")] public ScoreCardTeamInfo team5;
    }

    /// <summary>
    /// 竞速靶场模式回合内数据（<see cref="ScoreCardCommandData.rangeData"/>）。
    /// 玩家出场顺序仍看 <c>players</c> 数组顺序，与 <see cref="ScoreCardBaseData.team1"/> 的顺序一致。
    /// </summary>
    public class ScoreCardRangeData
    {
        [JsonProperty("playId")]     public string playId;
        [JsonProperty("playStatus")] public int playStatus;
        [JsonProperty("going")]      public bool going;
        [JsonProperty("totalNum")]   public int totalNum;
        [JsonProperty("finishedNum")] public int finishedNum;

        /// <summary>当前出场玩家 id（0 表示尚无人出场）。</summary>
        [JsonProperty("currentPlayerId")] public int currentPlayerId;

        [JsonProperty("turnLimitSeconds")] public int turnLimitSeconds;
        [JsonProperty("penaltySeconds")]   public int penaltySeconds;

        [JsonProperty("players")] public List<ScoreCardPlayer> players;
    }

    /// <summary>
    /// 新协议队伍对象（嵌套于 <see cref="ScoreCardBaseData"/>）。
    /// </summary>
    public class ScoreCardTeamInfo
    {
        [JsonProperty("teamType")]
        public int teamType;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("score")]
        public int score;

        [JsonProperty("resultStatus")]
        public int resultStatus;

        /// <summary>队伍累计击中数（服务端 team.hit）。</summary>
        [JsonProperty("hit")]
        public int hit;

        /// <summary>队伍累计被击中数（服务端 team.beHit）。</summary>
        [JsonProperty("beHit")]
        public int beHit;

        /// <summary>队伍累计击杀数（服务端 team.killNum）。</summary>
        [JsonProperty("killNum")]
        public int killNum;

        /// <summary>队伍累计被击杀数（服务端 team.beKill）。</summary>
        [JsonProperty("beKill")]
        public int beKill;

        /// <summary>队伍累计助攻数（服务端 team.assistNum）。</summary>
        [JsonProperty("assistNum")]
        public int assistNum;

        [JsonProperty("players")]
        public List<ScoreCardPlayer> players;
    }

    /// <summary>
    /// 新协议结算采样点（<see cref="ScoreCardCommandData.statLine"/> 元素）。
    /// </summary>
    public class ScoreCardStatLineSample
    {
        [JsonProperty("relativeSecond")]
        public int relativeSecond;

        [JsonProperty("team1")]
        public ScoreCardSettlementTeam team1;

        [JsonProperty("team2")]
        public ScoreCardSettlementTeam team2;
    }

    /// <summary>
    /// 新协议胜率采样点（<see cref="ScoreCardCommandData.winRateLine"/> 元素）。
    /// </summary>
    public class ScoreCardWinRateLineSample
    {
        [JsonProperty("relativeSecond")]
        public int relativeSecond;

        [JsonProperty("team1WinRate")]
        public float team1WinRate;

        [JsonProperty("team2WinRate")]
        public float team2WinRate;
    }

    /// <summary>
    /// 超级队长击杀明细（与 <c>statLine</c> 平级）。
    /// <c>relativeSecond</c> 为开赛后第几秒击杀了该玩家。
    /// </summary>
    public class ScoreCardCaptainKillEvent
    {
        [JsonProperty("relativeSecond")]
        public int relativeSecond;

        [JsonProperty("vestId")]
        public int vestId;

        [JsonProperty("customerName")]
        public string customerName;

        [JsonProperty("wxHeadImgUrl")]
        public string wxHeadImgUrl;
    }

    /// <summary>
    /// 超级队长逐帧统计采样点（<see cref="ScoreCardCommandData.captainStatLine"/> 元素）。
    /// </summary>
    public class ScoreCardCaptainStatSample
    {
        [JsonProperty("relativeSecond")]
        public int relativeSecond;

        [JsonProperty("redCaptain")]
        public ScoreCardCaptainStatEntry redCaptain;

        [JsonProperty("blueCaptain")]
        public ScoreCardCaptainStatEntry blueCaptain;
    }

    /// <summary>
    /// 单个队长在某采样时刻的累计数据（均为截至该秒的累计值，非增量）。
    /// </summary>
    public class ScoreCardCaptainStatEntry
    {
        [JsonProperty("kills")]
        public int kills;

        [JsonProperty("assists")]
        public int assists;

        [JsonProperty("score")]
        public int score;

        [JsonProperty("damage")]
        public int damage;

        [JsonProperty("deaths")]
        public int deaths;

        [JsonProperty("damageTaken")]
        public int damageTaken;

        [JsonProperty("shield")]
        public int shield;

        [JsonProperty("armor")]
        public int armor;

        [JsonProperty("buffList")]
        public List<string> buffList;
    }
}
