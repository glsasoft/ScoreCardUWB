using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 比分牌 MQTT <c>msgType=scoreCard</c> 下行协议外壳。
    /// 示例：<c>{"msgType":"scoreCard","msg":{"category":"PAGE","subCode":"S6-2","autoPlay":true,"data":{...}}}</c>
    /// </summary>
    public class ScoreCardCommandEnvelope
    {
        [JsonProperty("msgType")]
        public string msgType;

        [JsonProperty("msg")]
        public ScoreCardCommand msg;
    }

    /// <summary>
    /// 比分牌切屏/事件统一指令。
    /// <para><see cref="category"/>：<see cref="ScoreCardCommandCategory.Page"/> 或 <see cref="ScoreCardCommandCategory.Event"/>。</para>
    /// <para><see cref="subCode"/>：见 <see cref="ScoreCardSubCode"/> 常量集合。</para>
    /// <para><see cref="data"/>：场次数据或 kill 等事件载荷；按 <see cref="subCode"/> 不同，反序列化目标也不同
    /// （场次类用 <see cref="ToMatchData"/> 取 <see cref="ScoreCardMatchData"/>）。</para>
    /// <para><see cref="autoPlay"/>：仅 PAGE 指令 envelope 上的标志，开页时透传给 Panel，影响其内部动画自播。</para>
    /// </summary>
    public class ScoreCardCommand
    {
        [JsonProperty("category")]
        public string category;

        [JsonProperty("subCode")]
        public string subCode;

        [JsonProperty("data")]
        public JObject data;

        [JsonProperty("autoPlay")]
        public bool? autoPlay;

        /// <summary>服务端消息时间戳（毫秒），位于 <c>msg</c> 层而非 <c>data</c> 内。</summary>
        [JsonProperty("serverTimestamp")]
        public long serverTimestamp;

        /// <summary>是否切屏类指令（PAGE，或 BEGIN_PLAY / S6 系列 EVENT）。</summary>
        public bool IsScreenSwitch()
        {
            if (string.Equals(category, ScoreCardCommandCategory.Page, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(subCode, ScoreCardSubCode.BeginPlay, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return IsS6SubCode(subCode);
        }

        /// <summary>是否为 S6 回顾子页 subCode（服务端可能以 EVENT 下发）。</summary>
        public static bool IsS6SubCode(string subCode)
        {
            return string.Equals(subCode, ScoreCardSubCode.S6_1, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(subCode, ScoreCardSubCode.S6_2, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(subCode, ScoreCardSubCode.S6_3, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>是否为 S3 倒计时入口（BEGIN_PLAY EVENT 或 S3 PAGE）。</summary>
        public static bool IsS3EntrySubCode(string subCode)
        {
            return string.Equals(subCode, ScoreCardSubCode.BeginPlay, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(subCode, ScoreCardSubCode.S3, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>是否为 WIN_RATE 胜率推送。</summary>
        public bool IsWinRateEvent()
        {
            return string.Equals(subCode, ScoreCardSubCode.WinRate, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 将 <see cref="data"/> 解析为场次模型；若 <see cref="data"/> 为空返回 <c>null</c>。
        /// </summary>
        public ScoreCardMatchData ToMatchData()
        {
            var matchData = ScoreCardDataNormalizer.ToMatchData(data);
            matchData?.SyncTeamListAliases();
            return matchData;
        }

        /// <summary>
        /// 将 <see cref="data"/> 解析为 S6 结算模型；读 <c>statLine</c>。
        /// </summary>
        public ScoreCardSettlementData ToSettlementData()
        {
            return ScoreCardDataNormalizer.ToSettlementData(data);
        }

        /// <summary>
        /// 将 <see cref="data"/> 解析为 S4 胜率模型；读 <c>winRateLine</c>。
        /// </summary>
        public ScoreCardWinRateData ToWinRateData(int fallbackMatchDurationMin = 0)
        {
            return ScoreCardDataNormalizer.ToWinRateData(data, fallbackMatchDurationMin);
        }

        /// <summary>将 <see cref="data"/> 解析为 SHOOT 击中事件。</summary>
        public ScoreCardShootEventData ToShootEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardShootEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 MULTI_KILL 连杀事件。</summary>
        public ScoreCardMultiKillEventData ToMultiKillEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardMultiKillEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 FIRST_BLOOD 首杀事件。</summary>
        public ScoreCardFirstBloodEventData ToFirstBloodEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardFirstBloodEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 TOWER_KILL 防御靶灯击杀事件。</summary>
        public ScoreCardTowerKillEventData ToTowerKillEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardTowerKillEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 PLAYER_REVIVE 复活事件。</summary>
        public ScoreCardReviveEventData ToReviveEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardReviveEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 LAST_SECONDS 倒计时提醒（含 seconds）。</summary>
        public ScoreCardLastSecondsEventData ToLastSecondsEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardLastSecondsEventData>();
        }

        /// <summary>兼容旧 LAST_30S；等价于 <see cref="ToLastSecondsEventData"/>。</summary>
        public ScoreCardLast30sEventData ToLast30sEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardLast30sEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 RANGE_HIT 竞速靶场命中靶灯。</summary>
        public ScoreCardRangeHitEventData ToRangeHitEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardRangeHitEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 COMBO_HIT 竞速连击高光。</summary>
        public ScoreCardComboHitEventData ToComboHitEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardComboHitEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 TARGET_REMAIN 剩余靶数。</summary>
        public ScoreCardTargetRemainEventData ToTargetRemainEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardTargetRemainEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 RED/BLUE_BUFF_CAPTURED 夺取 BUFF 事件。</summary>
        public ScoreCardBuffCapturedEventData ToBuffCapturedEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardBuffCapturedEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 LEAD_CHANGE 领先变化事件。</summary>
        public ScoreCardLeadChangeEventData ToLeadChangeEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardLeadChangeEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 TEAM_SURVIVAL 队伍残局提示事件。</summary>
        public ScoreCardTeamSurvivalEventData ToTeamSurvivalEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardTeamSurvivalEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为竞速靶场 TURN_BEGIN / TURN_END。</summary>
        public ScoreCardTurnEventData ToTurnEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardTurnEventData>();
        }

        /// <summary>将 <see cref="data"/> 解析为 CAPTAIN_DOWN 超级队长阵亡事件。</summary>
        public ScoreCardCaptainDownEventData ToCaptainDownEventData()
        {
            if (data == null)
            {
                return null;
            }

            return data.ToObject<ScoreCardCaptainDownEventData>();
        }
    }
}

