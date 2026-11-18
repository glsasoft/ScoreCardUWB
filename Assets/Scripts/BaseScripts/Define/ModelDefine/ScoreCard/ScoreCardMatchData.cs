using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 比分牌场次数据，由 <see cref="ScoreCardCommand.data"/> 反序列化得到。
    /// 与旧 <c>ScoreCard5SMessage</c> 相比去掉了 <c>wgc</c>、<c>screenType</c> 等仅在
    /// 旧 wgc 路由中使用的字段；新协议下消息类型由 <c>category + subCode</c> 表达。
    /// 队伍列表同时支持文档中的 <c>redTeamList</c>… 与服务端实际下发的 <c>team1List</c>… 两套命名。
    /// </summary>
    public class ScoreCardMatchData
    {
        // ===== 场次基础信息 =====

        [JsonProperty("playName")]  public string playName;
        [JsonProperty("playId")]    public string playId;
        [JsonProperty("modeName")]  public string modeName;

        /// <summary>场次状态（服务端随 data 下发；客户端路由不依赖此字段）</summary>
        [JsonProperty("playStatus")] public int playStatus;

        /// <summary>
        /// 游戏模式（字符串）："0"=积分 "1"=歼灭 "2"=斩首 "3"=反恐 "4"=大逃杀 "5"=炸弹
        /// "6"=特战行动 "7"=超级队长 "8"=联盟对战 "9"=生化模式 "10"=谍影重重
        /// </summary>
        [JsonProperty("modeType")]  public string modeType;

        [JsonProperty("storeName")] public string storeName;
        [JsonProperty("placeCode")] public string placeCode;

        /// <summary>服务器时间戳（毫秒）</summary>
        [JsonProperty("serverTimestamp")] public long serverTimestamp;
        /// <summary>比赛开始时间戳（毫秒）</summary>
        [JsonProperty("startTime")]       public long startTime;
        /// <summary>比赛开始时间字符串</summary>
        [JsonProperty("beginTime")]       public string beginTime;
        /// <summary>比赛结束时间字符串（S6~S8 结算时由服务端下发）</summary>
        [JsonProperty("endTime")]         public string endTime;
        /// <summary>比赛时长（分钟）</summary>
        [JsonProperty("time")]            public int time;
        /// <summary>比赛时长（分钟，服务端新字段，与 <see cref="time"/> 同义）</summary>
        [JsonProperty("matchDuration")]   public int matchDuration;
        /// <summary>推送间隔（秒，字符串形式）</summary>
        [JsonProperty("interval")]        public string interval;
        /// <summary>进攻方标识（团队 teamType 字符串）</summary>
        [JsonProperty("attacker")]        public string attacker;
        /// <summary>复活模式玩家死亡后的复活倒计时（秒），来自 baseData.reviveCd。</summary>
        [JsonProperty("reviveCd")]        public int reviveCd;

        /// <summary>竞速靶场：当前出场玩家 id，由 rangeData.currentPlayerId 归并而来（非服务端顶层字段）。</summary>
        [JsonIgnore] public int rangeCurrentPlayerId;

        /// <summary>竞速靶场：本快照是否已合并过 rangeData，用于跨"仅 baseData"消息延续 range 字段。</summary>
        [JsonIgnore] public bool hasRangeData;

        /// <summary>
        /// 竞速靶场顶部条要显示的玩家：有人出场时取 <see cref="rangeCurrentPlayerId"/>；
        /// 两人之间的空档（TURN_END 不带 currentPlayerId，归并后为 0）取开始时间最晚的已结束玩家，即刚结束的那位；
        /// 还没有人开始过时取列表第一位。
        /// </summary>
        public ScoreCardPlayer ResolveRangeDisplayPlayer()
        {
            if (playerList == null) return null;

            if (rangeCurrentPlayerId > 0)
            {
                var current = playerList.Find(p => p != null && p.id == rangeCurrentPlayerId);
                if (current != null) return current;
            }

            ScoreCardPlayer lastFinished = null;
            foreach (var p in playerList)
            {
                if (p == null || p.beginTime == null) continue;
                if (p.rangeStatus != RangeStatusFinished && p.rangeStatus != RangeStatusForfeited) continue;
                if (lastFinished == null || p.beginTime > lastFinished.beginTime) lastFinished = p;
            }

            return lastFinished ?? playerList.Find(p => p != null);
        }

        const int RangeStatusFinished = 3;
        const int RangeStatusForfeited = 4;

        // ===== 玩家 / 队伍 =====

        [JsonProperty("playerList")]     public List<ScoreCardPlayer> playerList;

        /// <summary>服务端 team1~5 命名（team1=红 team2=蓝 …）</summary>
        [JsonProperty("team1List")]      public List<ScoreCardPlayer> team1List;
        [JsonProperty("team2List")]      public List<ScoreCardPlayer> team2List;
        [JsonProperty("team3List")]      public List<ScoreCardPlayer> team3List;
        [JsonProperty("team4List")]      public List<ScoreCardPlayer> team4List;
        [JsonProperty("team5List")]      public List<ScoreCardPlayer> team5List;

        /// <summary>文档/旧协议颜色命名</summary>
        [JsonProperty("redTeamList")]    public List<ScoreCardPlayer> redTeamList;
        [JsonProperty("blueTeamList")]   public List<ScoreCardPlayer> blueTeamList;
        [JsonProperty("greenTeamList")]  public List<ScoreCardPlayer> greenTeamList;
        [JsonProperty("yellowTeamList")] public List<ScoreCardPlayer> yellowTeamList;
        [JsonProperty("purpleTeamList")] public List<ScoreCardPlayer> purpleTeamList;

        /// <summary>炸弹模式（modeType="5"）专用</summary>
        [JsonProperty("bombRecordList")] public List<ScoreCardBombRecord> bombRecordList;

        /// <summary>新协议队伍显示名（team1~team5），由 Normalizer 从 baseData.teamN.name 填充。</summary>
        [JsonIgnore] readonly string[] m_TeamDisplayNames = new string[5];

        /// <summary>队伍总分（team1~team5），由 Normalizer 从 baseData.teamN.score 填充。</summary>
        [JsonIgnore] readonly int[] m_TeamScores = new int[5];

        [JsonIgnore] readonly bool[] m_TeamScoreSet = new bool[5];

        /// <summary>队伍胜负（0 平局 1 胜利 2 失败），由 Normalizer 从 baseData.teamN.resultStatus 填充。</summary>
        [JsonIgnore] readonly int[] m_TeamResultStatuses = new int[5];

        [JsonIgnore] readonly bool[] m_TeamResultStatusSet = new bool[5];

        /// <summary>队伍战斗汇总（击中/被击中/击杀/被击杀/助攻），由 Normalizer 从 baseData.teamN 填充。</summary>
        [JsonIgnore] readonly int[] m_TeamHit = new int[5];
        [JsonIgnore] readonly int[] m_TeamBeHit = new int[5];
        [JsonIgnore] readonly int[] m_TeamKillNum = new int[5];
        [JsonIgnore] readonly int[] m_TeamBeKill = new int[5];
        [JsonIgnore] readonly int[] m_TeamAssistNum = new int[5];
        [JsonIgnore] readonly bool[] m_TeamCombatStatsSet = new bool[5];

        public void SetTeamDisplayName(int teamIndex, string name)
        {
            if (teamIndex >= 0 && teamIndex < m_TeamDisplayNames.Length)
            {
                m_TeamDisplayNames[teamIndex] = name;
            }
        }

        public string GetTeamDisplayName(int teamIndex)
        {
            if (teamIndex >= 0 && teamIndex < m_TeamDisplayNames.Length
                && !string.IsNullOrEmpty(m_TeamDisplayNames[teamIndex]))
            {
                return m_TeamDisplayNames[teamIndex];
            }

            return teamIndex switch
            {
                0 => "红队",
                1 => "蓝队",
                2 => "绿队",
                3 => "黄队",
                4 => "紫队",
                _ => $"队伍{teamIndex}",
            };
        }

        public void SetTeamScore(int teamIndex, int score)
        {
            if (teamIndex >= 0 && teamIndex < m_TeamScores.Length)
            {
                m_TeamScores[teamIndex] = score;
                m_TeamScoreSet[teamIndex] = true;
            }
        }

        public bool TryGetTeamScore(int teamIndex, out int score)
        {
            if (teamIndex >= 0 && teamIndex < m_TeamScoreSet.Length && m_TeamScoreSet[teamIndex])
            {
                score = m_TeamScores[teamIndex];
                return true;
            }

            score = 0;
            return false;
        }

        public void SetTeamResultStatus(int teamIndex, int resultStatus)
        {
            if (teamIndex >= 0 && teamIndex < m_TeamResultStatuses.Length)
            {
                m_TeamResultStatuses[teamIndex] = resultStatus;
                m_TeamResultStatusSet[teamIndex] = true;
            }
        }

        public bool TryGetTeamResultStatus(int teamIndex, out int resultStatus)
        {
            if (teamIndex >= 0 && teamIndex < m_TeamResultStatusSet.Length && m_TeamResultStatusSet[teamIndex])
            {
                resultStatus = m_TeamResultStatuses[teamIndex];
                return true;
            }

            resultStatus = 0;
            return false;
        }

        public void SetTeamCombatStats(int teamIndex, int hit, int beHit, int killNum, int beKill, int assistNum)
        {
            if (teamIndex < 0 || teamIndex >= m_TeamCombatStatsSet.Length)
            {
                return;
            }

            m_TeamHit[teamIndex] = hit;
            m_TeamBeHit[teamIndex] = beHit;
            m_TeamKillNum[teamIndex] = killNum;
            m_TeamBeKill[teamIndex] = beKill;
            m_TeamAssistNum[teamIndex] = assistNum;
            m_TeamCombatStatsSet[teamIndex] = true;
        }

        public bool TryGetTeamCombatStats(
            int teamIndex,
            out int hit,
            out int beHit,
            out int killNum,
            out int beKill,
            out int assistNum)
        {
            if (teamIndex >= 0 && teamIndex < m_TeamCombatStatsSet.Length && m_TeamCombatStatsSet[teamIndex])
            {
                hit = m_TeamHit[teamIndex];
                beHit = m_TeamBeHit[teamIndex];
                killNum = m_TeamKillNum[teamIndex];
                beKill = m_TeamBeKill[teamIndex];
                assistNum = m_TeamAssistNum[teamIndex];
                return true;
            }

            hit = 0;
            beHit = 0;
            killNum = 0;
            beKill = 0;
            assistNum = 0;
            return false;
        }

        // ===== 辅助方法 =====

        /// <summary>
        /// 将 <c>team1List</c>… 与 <c>redTeamList</c>… 互为别名，便于旧代码与新服务端 JSON 共存。
        /// </summary>
        public void SyncTeamListAliases()
        {
            SyncPair(ref team1List, ref redTeamList);
            SyncPair(ref team2List, ref blueTeamList);
            SyncPair(ref team3List, ref greenTeamList);
            SyncPair(ref team4List, ref yellowTeamList);
            SyncPair(ref team5List, ref purpleTeamList);
        }

        static void SyncPair(ref List<ScoreCardPlayer> primary, ref List<ScoreCardPlayer> alias)
        {
            bool primaryHas = primary != null && primary.Count > 0;
            bool aliasHas = alias != null && alias.Count > 0;
            if (primaryHas && !aliasHas)
            {
                alias = primary;
            }
            else if (aliasHas && !primaryHas)
            {
                primary = alias;
            }
        }

        /// <summary>获取所有非空队伍列表</summary>
        public List<(string teamName, List<ScoreCardPlayer> players)> GetNonEmptyTeams()
        {
            SyncTeamListAliases();

            var result = new List<(string, List<ScoreCardPlayer>)>();
            if (team1List != null && team1List.Count > 0) result.Add((GetTeamDisplayName(0), team1List));
            else if (redTeamList != null && redTeamList.Count > 0) result.Add((GetTeamDisplayName(0), redTeamList));

            if (team2List != null && team2List.Count > 0) result.Add((GetTeamDisplayName(1), team2List));
            else if (blueTeamList != null && blueTeamList.Count > 0) result.Add((GetTeamDisplayName(1), blueTeamList));

            if (team3List != null && team3List.Count > 0) result.Add((GetTeamDisplayName(2), team3List));
            else if (greenTeamList != null && greenTeamList.Count > 0) result.Add((GetTeamDisplayName(2), greenTeamList));

            if (team4List != null && team4List.Count > 0) result.Add((GetTeamDisplayName(3), team4List));
            else if (yellowTeamList != null && yellowTeamList.Count > 0) result.Add((GetTeamDisplayName(3), yellowTeamList));

            if (team5List != null && team5List.Count > 0) result.Add((GetTeamDisplayName(4), team5List));
            else if (purpleTeamList != null && purpleTeamList.Count > 0) result.Add((GetTeamDisplayName(4), purpleTeamList));

            return result;
        }

        /// <summary>
        /// 根据 <see cref="modeType"/> 和队伍数量返回对战模式 ID（与 Java MqttInfo.getFightType() 对应）。
        /// 1=积分 2=歼灭 3=多队 4=斩首 5=反恐 6=大逃杀 7=炸弹 8=特战行动
        /// 9=超级队长 10=联盟对战(3~5队) 11=生化模式 12=谍影重重
        /// </summary>
        public int GetFightType()
        {
            int teamCount = GetNonEmptyTeams().Count;
            if (teamCount > 2) return 10;

            return modeType switch
            {
                "0"  => 1,
                "1"  => 2,
                "2"  => 4,
                "3"  => 5,
                "4"  => 6,
                "5"  => 7,
                "6"  => 8,
                "7"  => 9,
                "8"  => 10,
                "9"  => 11,
                "10" => 12,
                _    => 1,
            };
        }

        /// <summary>
        /// 按 NvN 选择 S4 中央 Page 资源名（与 GLSAQuadView 实际 prefab 对应）。
        /// 队伍数 &gt; 2 时按 5v5 走（联盟对战暂用 5v5 视图）。
        /// </summary>
        public string ResolveFightPagePrefabName()
        {
            int teamCount = GetNonEmptyTeams().Count;
            if (teamCount == 0)
            {
                return "ScoreCardFightPage5v5"; // 兜底
            }

            // 取最大单队人数估算
            int maxPerTeam = 0;
            foreach (var (_, players) in GetNonEmptyTeams())
            {
                if (players != null && players.Count > maxPerTeam)
                {
                    maxPerTeam = players.Count;
                }
            }

            return maxPerTeam switch
            {
                <= 1  => "ScoreCardFightPage1v1",
                <= 5  => "ScoreCardFightPage5v5",
                <= 10 => "ScoreCardFightPage10v10",
                _     => "ScoreCardFightPage20v20",
            };
        }
    }
}
