using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 比分牌服务器推送的玩家数据（与游戏 <c>Player</c> 字段不同，由 Java 比分牌服务端定义）。
    /// 历史上叫 <c>ScoreCard5SPlayer</c>，2026-05-21 起跟随协议改造统一去掉 <c>5S</c> 前缀。
    /// SHOOT 等事件的局部更新见 <see cref="ScoreCardPlayerPartial"/>（可空字段仅合并已下发项）。
    /// </summary>
    public class ScoreCardPlayer
    {
        /// <summary>玩家ID</summary>
        [JsonProperty("id")]            public int id;

        /// <summary>微信注册用户 ID；有值表示已绑定微信，无值表示游客。</summary>
        [JsonProperty("customerId")]    public string customerId;

        /// <summary>玩家昵称</summary>
        [JsonProperty("customerName")]  public string customerName;

        /// <summary>玩家头像URL</summary>
        [JsonProperty("wxHeadImgUrl")]  public string wxHeadImgUrl;

        /// <summary>队伍类型 "0"=红队/人类 "1"=蓝队/丧尸 …</summary>
        [JsonProperty("teamType")]      public string teamType;

        /// <summary>初始队伍（生化模式：人类=0，丧尸=1；未下发时 UI 以当前 teamType 兜底）。</summary>
        [JsonProperty("initTeam")]      public string initTeam;

        /// <summary>
        /// 初始阵营（生化）：0 人类 / 1 丧尸。与 <see cref="initTeam"/> 同义，服务端现多用本字段。
        /// </summary>
        [JsonProperty("initTeamType")]
        public int? initTeamType;

        /// <summary>人类被感染成丧尸的时刻（毫秒时间戳）；未感染为空。</summary>
        [JsonProperty("infectTime")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long? infectTime;

        /// <summary>生化：人类期明细（被感染者定格在感染时刻）。</summary>
        [JsonProperty("humanStat")]
        public ScoreCardPlayerPhaseStat humanStat;

        /// <summary>生化：丧尸期明细（汇总 − 人类期）。</summary>
        [JsonProperty("zombieStat")]
        public ScoreCardPlayerPhaseStat zombieStat;

        /// <summary>玩家角色 0=普通，1、2 都是队长（见 <see cref="IsCaptain"/>）</summary>
        [JsonProperty("type")]          public string type;

        /// <summary>当前血量</summary>
        [JsonProperty("blood")]         public int blood;

        /// <summary>初始血量（未下发时 UI 以 100 兜底）</summary>
        [JsonProperty("initBlood")]     public int initBlood;

        /// <summary>护甲（0 或未下发表示无护甲）</summary>
        [JsonProperty("armor")]         public int armor;

        /// <summary>护盾（0 或未下发表示无护盾）</summary>
        [JsonProperty("shield")]        public int shield;

        /// <summary>实时持有 BUFF 列表，如 RED / BLUE。</summary>
        [JsonProperty("buffList")]
        public List<string> buffList;

        /// <summary>当前子弹</summary>
        [JsonProperty("bullet")]        public int bullet;

        /// <summary>初始子弹</summary>
        [JsonProperty("initBullet")]    public int initBullet;

        /// <summary>防弹衣（bulletProof）</summary>
        [JsonProperty("bulletProof")]   public string bulletProof;

        /// <summary>马甲ID（装备编号）</summary>
        [JsonProperty("vestId")]        public int vestId;

        /// <summary>玩家等级 LV（真实等级数字，服务端 level 字段；当前未下发、后续增加，未下发时为 0）</summary>
        [JsonProperty("level")]         public int level;

        /// <summary>玩家等级分（服务端 levelScore 字段，非展示用的 LV 数字）</summary>
        [JsonProperty("levelScore")]    public int levelScore;

        /// <summary>竞技分（服务端 rankScore 字段，与段位 tier/star 不同）</summary>
        [JsonProperty("rankScore")]     public int rankScore;

        /// <summary>当前段位（<c>{ tier, star }</c>，如青铜 I）</summary>
        [JsonProperty("rank")]
        public ScoreCardPlayerRank rank;

        /// <summary>比赛前段位（结构与 <see cref="rank"/> 一致）</summary>
        [JsonProperty("beforeRank")]
        public ScoreCardPlayerRank beforeRank;

        /// <summary>击杀数（间谍模式下为击杀敌方数量）</summary>
        [JsonProperty("killNum")]       public int killNum;

        /// <summary>误杀队友数（间谍模式有效）</summary>
        [JsonProperty("killFriendNum")] public int killFriendNum;

        /// <summary>击中数</summary>
        [JsonProperty("hitNum")]        public int hitNum;

        /// <summary>被击杀数（死亡数）</summary>
        [JsonProperty("beKillNum")]     public int beKillNum;

        /// <summary>被击次数</summary>
        [JsonProperty("beHitNum")]      public int beHitNum;

        /// <summary>助攻数</summary>
        [JsonProperty("assistNum")]     public int assistNum;

        /// <summary>复活次数</summary>
        [JsonProperty("reviveNum")]     public int reviveNum;

        /// <summary>连杀次数</summary>
        [JsonProperty("serialKillNum")] public int serialKillNum;

        /// <summary>最多连杀数</summary>
        [JsonProperty("topKillNum")]    public int topKillNum;

        /// <summary>比赛得分</summary>
        [JsonProperty("matchScore")]    public int matchScore;

        /// <summary>本局输出伤害（S8-1 伤害页）</summary>
        [JsonProperty("outputDamage")] public int outputDamage;

        /// <summary>承伤总值（玩家+防御塔，S8-1 承伤占比）</summary>
        [JsonProperty("totalHurt")]     public int totalHurt;

        /// <summary>恢复总生命值（复活+蓝 buff，S8-1 治疗占比）</summary>
        [JsonProperty("totalHeal")]     public int totalHeal;

        /// <summary>评价分（纯数字，UI 暂不使用）</summary>
        [JsonProperty("evalScore")]     public double evalScore;

        /// <summary>评级，如 SSS（S8 结算 UI 显示）</summary>
        [JsonProperty("evalGrade")]     public string evalGrade;

        /// <summary>称号名称（UI 特效未完成，暂不使用）</summary>
        [JsonProperty("titleName")]     public string titleName;

        /// <summary>称号等级（暂不使用）</summary>
        [JsonProperty("titleLevel")]    public int titleLevel;

        /// <summary>赛前竞技分</summary>
        [JsonProperty("beforeRankScore")] public int beforeRankScore;

        /// <summary>赛后竞技分</summary>
        [JsonProperty("afterRankScore")]  public int afterRankScore;

        /// <summary>赛前等级分</summary>
        [JsonProperty("beforeLevelScore")] public int beforeLevelScore;

        /// <summary>赛后等级分</summary>
        [JsonProperty("afterLevelScore")]  public int afterLevelScore;

        /// <summary>MVP/SVP 类型：0=无 1=MVP 2=SVP</summary>
        [JsonProperty("mvpType")]       public int mvpType;

        /// <summary>
        /// 小奖列表（全场至多 3 个不同 code；可多项挂在同一玩家下）。
        /// 见 <see cref="ScoreCardSmallPrize"/>、<see cref="ScoreCardSmallPrizeCode"/>。
        /// </summary>
        [JsonProperty("smallPrize")]
        public List<ScoreCardSmallPrize> smallPrize;

        /// <summary>第一次击杀的时间戳（毫秒，用于排序）</summary>
        [JsonProperty("firstKillsTime")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long? firstKillsTime;

        /// <summary>第一次死亡的时间戳（毫秒，用于排序）</summary>
        [JsonProperty("firstDeathsTime")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long? firstDeathsTime;

        /// <summary>完全阵亡的时间戳（毫秒，用于排序；值越小越先死完）</summary>
        [JsonProperty("finishDeathsTime")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long? finishDeathsTime;

        //玩家加入时间：gmtCreate 格式：2026-05-20 12:55:22
        [JsonProperty("gmtCreate")]
        public string gmtCreate;

        // ===== 竞速靶场模式专用（BEGIN_PLAY / RANGE_UPDATE 的 rangeData.players）=====

        /// <summary>出场序号（1-based）</summary>
        [JsonProperty("seqNo")]         public int seqNo;

        /// <summary>挑战状态：0 未上场 2 挑战中 3 已完成 4 已弃权</summary>
        [JsonProperty("status")]        public int rangeStatus;

        /// <summary>当前名次（完成后才有值）</summary>
        [JsonProperty("currentRank")]   public int? currentRank;

        /// <summary>胜率 0.00~100.00</summary>
        [JsonProperty("winRate")]       public double winRate;

        /// <summary>剩余生命值（回合内实时值）</summary>
        [JsonProperty("remainBlood")]   public int remainBlood;

        /// <summary>剩余总弹药</summary>
        [JsonProperty("remainBullet")]  public int remainBullet;

        /// <summary>本回合总有效靶数</summary>
        [JsonProperty("targetNum")]     public int targetNum;

        /// <summary>剩余有效靶数</summary>
        [JsonProperty("remainTarget")]  public int remainTarget;

        /// <summary>开启挑战时刻的毫秒时间戳（GO 时刻），挑战前为空</summary>
        [JsonProperty("beginTime")]     public long? beginTime;

        /// <summary>原始用时毫秒</summary>
        [JsonProperty("elapsedMs")]     public long? elapsedMs;

        /// <summary>罚时毫秒</summary>
        [JsonProperty("penaltyMs")]     public int penaltyMs;

        /// <summary>最终成绩毫秒</summary>
        [JsonProperty("finalScoreMs")]  public long? finalScoreMs;

        /// <summary>罚时命中数</summary>
        [JsonProperty("penaltyNum")]    public int penaltyNum;

        /// <summary>射击数</summary>
        [JsonProperty("shootNum")]      public int shootNum;

        /// <summary>命中率 0~1（一枪未开为空）</summary>
        [JsonProperty("accuracy")]      public double? accuracy;

        /// <summary>最高连击</summary>
        [JsonProperty("topCombo")]      public int topCombo;

        /// <summary>结束原因编码：1 清场成功 2 超时 3 阵亡 4 弹尽 5 主动退出 6 弃权（未结束为空）</summary>
        [JsonProperty("endReason")]     public int? endReason;

        /// <summary>结束原因中文描述</summary>
        [JsonProperty("endReasonDesc")] public string endReasonDesc;

        /// <summary>五维评价分（速度/判断/精准/生存/连击，约 0~10）</summary>
        [JsonProperty("evalTerms")]
        public ScoreCardEvalTerms evalTerms;

        /// <summary>获取队伍颜色名称（用于显示）</summary>
        public string GetTeamColorName()
        {
            return teamType switch
            {
                "0" => "红队",
                "1" => "蓝队",
                "2" => "绿队",
                "3" => "黄队",
                "4" => "紫队",
                _   => $"队伍{teamType}"
            };
        }

        /// <summary>是否已阵亡：<see cref="blood"/> == 0。</summary>
        public bool IsDead() => blood == 0;

        /// <summary>是否仍存活（实时剩余人数口径）：blood != 0。</summary>
        public bool IsAlive() => !IsDead();

        /// <summary>超级队长模式的队长：服务端 type=1、type=2 都算队长。</summary>
        public bool IsCaptain() => IsCaptainType(type);

        public static bool IsCaptainType(string playerType) => playerType == "1" || playerType == "2";

        /// <summary>
        /// 存活时长（秒）= <see cref="finishDeathsTime"/> - 比赛开始(<paramref name="startTimeMs"/>)。
        /// 无死亡时刻则记满全场 <paramref name="actualMatchDurationSec"/>（真实比赛时长）。
        /// 结果封顶在 [0, actualMatchDurationSec]（actualMatchDurationSec &lt;= 0 时不封顶）。
        /// </summary>
        public int GetSurviveSeconds(long startTimeMs, int actualMatchDurationSec)
        {
            if (!finishDeathsTime.HasValue)
            {
                return actualMatchDurationSec > 0 ? actualMatchDurationSec : 0;
            }

            long deltaMs = finishDeathsTime.Value - startTimeMs;
            int seconds = deltaMs > 0 ? (int)(deltaMs / 1000L) : 0;
            if (actualMatchDurationSec > 0 && seconds > actualMatchDurationSec)
            {
                seconds = actualMatchDurationSec;
            }

            return seconds;
        }

        public bool IsMvp() => mvpType == ScoreCardMvpType.Mvp;

        public bool IsSvp() => mvpType == ScoreCardMvpType.Svp;

        public bool HasSmallPrize(string prizeCode)
        {
            if (smallPrize == null || string.IsNullOrEmpty(prizeCode))
            {
                return false;
            }

            for (int i = 0; i < smallPrize.Count; i++)
            {
                var prize = smallPrize[i];
                if (prize != null
                    && string.Equals(prize.code, prizeCode, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        //玩家加入时间：gmtCreate 格式：2026-05-20 12:55:22
        public DateTime GetJoinTime()
        {
            return DateTime.Parse(gmtCreate);
        }

        /// <summary>本局竞技分变化（afterRankScore - beforeRankScore）。</summary>
        public int GetRankScoreDelta() => afterRankScore - beforeRankScore;

        /// <summary>本局等级分变化（afterLevelScore - beforeLevelScore）。</summary>
        public int GetLevelScoreDelta() => afterLevelScore - beforeLevelScore;

        /// <summary>是否微信注册用户（有 customerId）。</summary>
        public bool IsRegisteredUser() => !string.IsNullOrEmpty(customerId);
    }

    /// <summary>竞速靶场五维评价分（约 0~10）。雷达图顶点顺序：顶 SP → 右 AC → 右下 SV → 左下 CB → 左 JD。</summary>
    public class ScoreCardEvalTerms
    {
        [JsonProperty("SP")] public double SP;
        [JsonProperty("JD")] public double JD;
        [JsonProperty("AC")] public double AC;
        [JsonProperty("SV")] public double SV;
        [JsonProperty("CB")] public double CB;

        public float[] ToRadarValues()
        {
            return new[]
            {
                (float)SP,
                (float)AC,
                (float)SV,
                (float)CB,
                (float)JD,
            };
        }
    }
}
