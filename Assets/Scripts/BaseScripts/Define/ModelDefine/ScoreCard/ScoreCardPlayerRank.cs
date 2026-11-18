using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 玩家段位（MQTT <c>rank</c> / <c>beforeRank</c>）：<c>{ "tier": "青铜", "star": "一" }</c> 表示青铜 I。
    /// </summary>
    public class ScoreCardPlayerRank
    {
        [JsonProperty("tier")]
        public string tier;

        [JsonProperty("star")]
        public string star;

        [JsonIgnore]
        public ScoreCardRankTier TierEnum => ScoreCardRankUtil.ParseTier(tier);

        [JsonIgnore]
        public ScoreCardRankStar StarEnum => ScoreCardRankUtil.ParseStar(star);

        [JsonIgnore]
        public bool IsValid => TierEnum != ScoreCardRankTier.Unknown;

        /// <summary>段位特效 / 图标在配置表中的槽位（0~30），供 UI 数组或字典索引。</summary>
        [JsonIgnore]
        public int VisualSlotIndex => ScoreCardRankUtil.GetVisualSlotIndex(TierEnum, StarEnum);

        public string ToDisplayText() => ScoreCardRankUtil.ToDisplayText(TierEnum, StarEnum);

        public string ToAssetKey() => ScoreCardRankUtil.ToAssetKey(TierEnum, StarEnum);
    }

    /// <summary>大段位（与 MQTT <c>tier</c> 中文一一对应）。</summary>
    public enum ScoreCardRankTier
    {
        Unknown = -1,
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Platinum = 3,
        Diamond = 4,
        Grandmaster = 5,
        King = 6,
    }

    /// <summary>小段位星级（与 MQTT <c>star</c> 中文对应；最强王者为 <see cref="None"/>）。</summary>
    public enum ScoreCardRankStar
    {
        None = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
    }

    /// <summary>
    /// 段位解析与展示工具。
    /// UI 关联推荐：用 <see cref="GetVisualSlotIndex"/> 索引 ScriptableObject / 预制体数组（共 31 项），
    /// 或用 <see cref="ToAssetKey"/> 作 Addressables / Resources 资源名。
    /// </summary>
    public static class ScoreCardRankUtil
    {
        public const int VisualSlotCount = 31;

        static readonly Dictionary<string, ScoreCardRankTier> s_TierMap =
            new(StringComparer.Ordinal)
            {
                { "青铜", ScoreCardRankTier.Bronze },
                { "白银", ScoreCardRankTier.Silver },
                { "黄金", ScoreCardRankTier.Gold },
                { "铂金", ScoreCardRankTier.Platinum },
                { "钻石", ScoreCardRankTier.Diamond },
                { "宗师", ScoreCardRankTier.Grandmaster },
                { "最强王者", ScoreCardRankTier.King },
            };

        static readonly Dictionary<string, ScoreCardRankStar> s_StarMap =
            new(StringComparer.Ordinal)
            {
                { "一", ScoreCardRankStar.One },
                { "二", ScoreCardRankStar.Two },
                { "三", ScoreCardRankStar.Three },
                { "四", ScoreCardRankStar.Four },
                { "五", ScoreCardRankStar.Five },
            };

        static readonly string[] s_TierNames =
        {
            "青铜", "白银", "黄金", "铂金", "钻石", "宗师", "最强王者",
        };

        static readonly string[] s_RomanStars = { "", "I", "II", "III", "IV", "V" };

        public static ScoreCardRankTier ParseTier(string tier)
        {
            if (string.IsNullOrWhiteSpace(tier)) return ScoreCardRankTier.Unknown;
            return s_TierMap.TryGetValue(tier.Trim(), out var value)
                ? value
                : ScoreCardRankTier.Unknown;
        }

        public static ScoreCardRankStar ParseStar(string star)
        {
            if (string.IsNullOrWhiteSpace(star)) return ScoreCardRankStar.None;
            return s_StarMap.TryGetValue(star.Trim(), out var value)
                ? value
                : ScoreCardRankStar.None;
        }

        /// <summary>
        /// 31 槽位：青铜 I~V(0~4)、白银 I~V(5~9) … 宗师 I~V(25~29)、最强王者(30)。
        /// </summary>
        public static int GetVisualSlotIndex(ScoreCardRankTier tier, ScoreCardRankStar star)
        {
            if (tier == ScoreCardRankTier.Unknown) return -1;
            if (tier == ScoreCardRankTier.King) return 30;
            if (star < ScoreCardRankStar.One || star > ScoreCardRankStar.Five) return -1;
            return (int)tier * 5 + ((int)star - 1);
        }

        public static bool TryGetTierStar(int slotIndex, out ScoreCardRankTier tier, out ScoreCardRankStar star)
        {
            tier = ScoreCardRankTier.Unknown;
            star = ScoreCardRankStar.None;
            if (slotIndex < 0 || slotIndex >= VisualSlotCount) return false;

            if (slotIndex == 30)
            {
                tier = ScoreCardRankTier.King;
                return true;
            }

            tier = (ScoreCardRankTier)(slotIndex / 5);
            star = (ScoreCardRankStar)(slotIndex % 5 + 1);
            return tier <= ScoreCardRankTier.Grandmaster;
        }

        public static string ToDisplayText(ScoreCardRankTier tier, ScoreCardRankStar star)
        {
            if (tier == ScoreCardRankTier.Unknown) return string.Empty;
            if (tier == ScoreCardRankTier.King) return s_TierNames[(int)tier];

            int tierIndex = (int)tier;
            if (tierIndex < 0 || tierIndex >= s_TierNames.Length - 1) return string.Empty;
            if (star < ScoreCardRankStar.One || star > ScoreCardRankStar.Five) return s_TierNames[tierIndex];

            return s_TierNames[tierIndex] + s_RomanStars[(int)star];
        }

        /// <summary>资源键，如 <c>Bronze_I</c>、<c>King</c>，便于按名加载特效预制体。</summary>
        public static string ToAssetKey(ScoreCardRankTier tier, ScoreCardRankStar star)
        {
            if (tier == ScoreCardRankTier.Unknown) return string.Empty;
            if (tier == ScoreCardRankTier.King) return "King";
            if (star < ScoreCardRankStar.One || star > ScoreCardRankStar.Five) return tier.ToString();
            return $"{tier}_{s_RomanStars[(int)star]}";
        }
    }
}
