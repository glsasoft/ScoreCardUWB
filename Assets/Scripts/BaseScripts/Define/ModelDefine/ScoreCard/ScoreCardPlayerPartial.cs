using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// SHOOT 等事件中的局部玩家快照：字段均为可空，仅非 null 字段参与合并到 <see cref="ScoreCardPlayer"/>。
    /// </summary>
    public class ScoreCardPlayerPartial
    {
        [JsonProperty("id")]
        public int? id;

        [JsonProperty("customerName")]
        public string customerName;

        /// <summary>SHOOT 消息常用 <c>name</c>，合并时写入 <see cref="ScoreCardPlayer.customerName"/>。</summary>
        [JsonProperty("name")]
        public string name;

        [JsonProperty("wxHeadImgUrl")]
        public string wxHeadImgUrl;

        [JsonProperty("teamType")]
        public string teamType;

        [JsonProperty("initTeam")]
        public string initTeam;

        [JsonProperty("type")]
        public string type;

        [JsonProperty("blood")]
        public int? blood;

        [JsonProperty("initBlood")]
        public int? initBlood;

        [JsonProperty("armor")]
        public int? armor;

        [JsonProperty("shield")]
        public int? shield;

        [JsonProperty("buffList")]
        public List<string> buffList;

        [JsonProperty("bullet")]
        public int? bullet;

        [JsonProperty("initBullet")]
        public int? initBullet;

        [JsonProperty("bulletProof")]
        public string bulletProof;

        [JsonProperty("vestId")]
        public int? vestId;

        [JsonProperty("level")]
        public int? level;

        [JsonProperty("levelScore")]
        public int? levelScore;

        [JsonProperty("rankScore")]
        public int? rankScore;

        [JsonProperty("rank")]
        public ScoreCardPlayerRank rank;

        [JsonProperty("beforeRank")]
        public ScoreCardPlayerRank beforeRank;

        [JsonProperty("killNum")]
        public int? killNum;

        [JsonProperty("killFriendNum")]
        public int? killFriendNum;

        [JsonProperty("hitNum")]
        public int? hitNum;

        [JsonProperty("beKillNum")]
        public int? beKillNum;

        [JsonProperty("beHitNum")]
        public int? beHitNum;

        [JsonProperty("assistNum")]
        public int? assistNum;

        [JsonProperty("reviveNum")]
        public int? reviveNum;

        [JsonProperty("serialKillNum")]
        public int? serialKillNum;

        [JsonProperty("topKillNum")]
        public int? topKillNum;

        [JsonProperty("matchScore")]
        public int? matchScore;

        [JsonProperty("evalScore")]
        public double? evalScore;

        [JsonProperty("evalGrade")]
        public string evalGrade;

        [JsonProperty("titleName")]
        public string titleName;

        [JsonProperty("titleLevel")]
        public int? titleLevel;

        [JsonProperty("beforeRankScore")]
        public int? beforeRankScore;

        [JsonProperty("afterRankScore")]
        public int? afterRankScore;

        [JsonProperty("beforeLevelScore")]
        public int? beforeLevelScore;

        [JsonProperty("afterLevelScore")]
        public int? afterLevelScore;

        [JsonProperty("customerId")]
        public string customerId;

        [JsonProperty("mvpType")]
        public int? mvpType;

        [JsonProperty("smallPrize")]
        public List<ScoreCardSmallPrize> smallPrize;

        [JsonProperty("firstKillsTime")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long? firstKillsTime;

        [JsonProperty("firstDeathsTime")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long? firstDeathsTime;

        [JsonProperty("finishDeathsTime")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long? finishDeathsTime;

        [JsonProperty("gmtCreate")]
        public string gmtCreate;

        /// <summary>竞速靶场回合结束原因。1 清场成功，2 超时，3 阵亡，4 弹尽，5 主动退出，6 弃权。未结束为空。</summary>
        [JsonProperty("endReason")]
        public int? endReason;

        public int ResolvePlayerId()
        {
            return id ?? 0;
        }

        /// <summary>语音播报用显示名：优先 customerName，其次 name。</summary>
        public string ResolveDisplayName()
        {
            if (!string.IsNullOrEmpty(customerName)) return customerName;
            return name;
        }

        /// <summary>将本快照中已下发的字段合并到目标玩家；未下发（null）的字段保持不变。</summary>
        public void MergeInto(ScoreCardPlayer target)
        {
            if (target == null)
            {
                return;
            }

            if (id.HasValue) target.id = id.Value;

            if (customerName != null)
            {
                target.customerName = customerName;
            }
            else if (name != null)
            {
                target.customerName = name;
            }

            if (wxHeadImgUrl != null) target.wxHeadImgUrl = wxHeadImgUrl;
            if (teamType != null) target.teamType = teamType;
            if (initTeam != null) target.initTeam = initTeam;
            if (type != null) target.type = type;
            if (blood.HasValue) target.blood = blood.Value;
            if (initBlood.HasValue) target.initBlood = initBlood.Value;
            if (armor.HasValue) target.armor = armor.Value;
            if (shield.HasValue) target.shield = shield.Value;
            if (buffList != null) target.buffList = buffList;
            if (bullet.HasValue) target.bullet = bullet.Value;
            if (initBullet.HasValue) target.initBullet = initBullet.Value;
            if (bulletProof != null) target.bulletProof = bulletProof;
            if (vestId.HasValue) target.vestId = vestId.Value;
            if (level.HasValue) target.level = level.Value;
            if (levelScore.HasValue) target.levelScore = levelScore.Value;
            if (rankScore.HasValue) target.rankScore = rankScore.Value;
            if (rank != null) target.rank = rank;
            if (beforeRank != null) target.beforeRank = beforeRank;
            if (killNum.HasValue) target.killNum = killNum.Value;
            if (killFriendNum.HasValue) target.killFriendNum = killFriendNum.Value;
            if (hitNum.HasValue) target.hitNum = hitNum.Value;
            if (beKillNum.HasValue) target.beKillNum = beKillNum.Value;
            if (beHitNum.HasValue) target.beHitNum = beHitNum.Value;
            if (assistNum.HasValue) target.assistNum = assistNum.Value;
            if (reviveNum.HasValue) target.reviveNum = reviveNum.Value;
            if (serialKillNum.HasValue) target.serialKillNum = serialKillNum.Value;
            if (topKillNum.HasValue) target.topKillNum = topKillNum.Value;
            if (matchScore.HasValue) target.matchScore = matchScore.Value;
            if (evalScore.HasValue) target.evalScore = evalScore.Value;
            if (evalGrade != null) target.evalGrade = evalGrade;
            if (titleName != null) target.titleName = titleName;
            if (titleLevel.HasValue) target.titleLevel = titleLevel.Value;
            if (beforeRankScore.HasValue) target.beforeRankScore = beforeRankScore.Value;
            if (afterRankScore.HasValue) target.afterRankScore = afterRankScore.Value;
            if (beforeLevelScore.HasValue) target.beforeLevelScore = beforeLevelScore.Value;
            if (afterLevelScore.HasValue) target.afterLevelScore = afterLevelScore.Value;
            if (customerId != null) target.customerId = customerId;
            if (mvpType.HasValue) target.mvpType = mvpType.Value;
            if (smallPrize != null) target.smallPrize = smallPrize;
            if (firstKillsTime.HasValue) target.firstKillsTime = firstKillsTime;
            if (firstDeathsTime.HasValue) target.firstDeathsTime = firstDeathsTime;
            if (finishDeathsTime.HasValue) target.finishDeathsTime = finishDeathsTime;
            if (gmtCreate != null) target.gmtCreate = gmtCreate;
        }
    }
}
