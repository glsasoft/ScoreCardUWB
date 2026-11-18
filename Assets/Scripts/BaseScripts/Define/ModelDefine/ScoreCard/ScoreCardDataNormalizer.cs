using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 将新协议 <c>{ baseData, statLine?, winRateLine? }</c> 归一化为 Panel 层使用的内部模型。
    /// </summary>
    public static class ScoreCardDataNormalizer
    {
        public static ScoreCardMatchData ToMatchData(JObject data)
        {
            if (data == null) return null;

            var wrapped = data.ToObject<ScoreCardCommandData>();
            if (wrapped?.baseData != null)
            {
                var match = FromBaseData(wrapped.baseData);
                if (wrapped.rangeData != null)
                {
                    MergeRangeDataInto(match, wrapped.rangeData);
                }

                return match;
            }

            return null;
        }

        /// <summary>RANGE_UPDATE（无 baseData）：把 rangeData 合并进已有快照。</summary>
        public static bool MergeRangeData(ScoreCardMatchData snapshot, ScoreCardRangeData rangeData)
        {
            if (snapshot == null || rangeData == null) return false;

            MergeRangeDataInto(snapshot, rangeData);
            return true;
        }

        static void MergeRangeDataInto(ScoreCardMatchData match, ScoreCardRangeData rangeData)
        {
            match.hasRangeData = true;
            match.rangeCurrentPlayerId = rangeData.currentPlayerId;

            if (rangeData.players == null) return;

            match.playerList ??= new List<ScoreCardPlayer>();
            foreach (var rp in rangeData.players)
            {
                if (rp == null) continue;

                var target = match.playerList.Find(p => p != null && p.id == rp.id);
                if (target != null)
                {
                    CopyRangeFields(rp, target);
                }
                else
                {
                    match.playerList.Add(rp);
                }
            }

            match.SyncTeamListAliases();
        }

        static void CopyRangeFields(ScoreCardPlayer src, ScoreCardPlayer dst)
        {
            dst.seqNo = src.seqNo;
            dst.rangeStatus = src.rangeStatus;
            dst.currentRank = src.currentRank;
            dst.winRate = src.winRate;
            dst.remainBlood = src.remainBlood;
            dst.initBullet = src.initBullet;
            dst.remainBullet = src.remainBullet;
            dst.targetNum = src.targetNum;
            dst.remainTarget = src.remainTarget;
            dst.beginTime = src.beginTime;
            dst.elapsedMs = src.elapsedMs;
            dst.penaltyMs = src.penaltyMs;
            dst.finalScoreMs = src.finalScoreMs;
            dst.hitNum = src.hitNum;
            dst.penaltyNum = src.penaltyNum;
            dst.shootNum = src.shootNum;
            dst.accuracy = src.accuracy;
            dst.topCombo = src.topCombo;
            dst.endReason = src.endReason;
            dst.endReasonDesc = src.endReasonDesc;
            dst.evalTerms = src.evalTerms;
        }

        /// <summary>
        /// 竞速靶场 S4/REFRESH_DATA 等仅带 baseData 的消息不含 rangeData 时，从上一份快照延续
        /// rangeData 相关字段，避免被 baseData 反序列化出的默认值清零（其它模式的快照无 hasRangeData，天然跳过）。
        /// </summary>
        public static void CarryForwardRangeData(ScoreCardMatchData previous, ScoreCardMatchData current)
        {
            if (current == null || current.hasRangeData
                || previous == null || !previous.hasRangeData || previous.playerList == null)
            {
                return;
            }

            current.rangeCurrentPlayerId = previous.rangeCurrentPlayerId;
            current.hasRangeData = true;

            if (current.playerList == null) return;

            foreach (var p in current.playerList)
            {
                if (p == null) continue;

                var old = previous.playerList.Find(x => x != null && x.id == p.id);
                if (old != null)
                {
                    CopyRangeFields(old, p);
                }
            }
        }

        public static ScoreCardSettlementData ToSettlementData(JObject data)
        {
            if (data == null) return null;

            var wrapped = data.ToObject<ScoreCardCommandData>();
            if (wrapped == null) return null;

            if (wrapped.statLine != null && wrapped.statLine.Count > 0)
            {
                return FromStatLine(
                    wrapped.statLine,
                    wrapped.baseData,
                    wrapped.redCaptainKills,
                    wrapped.blueCaptainKills,
                    wrapped.captainStatLine);
            }

            if (HasCaptainKillEvents(wrapped.redCaptainKills)
                || HasCaptainKillEvents(wrapped.blueCaptainKills)
                || HasCaptainStatLine(wrapped.captainStatLine))
            {
                return new ScoreCardSettlementData
                {
                    matchDuration    = wrapped.baseData?.matchDuration ?? 0,
                    actualMatchSec   = ResolveActualMatchSec(wrapped.baseData),
                    settlementList   = new List<ScoreCardSettlementSample>(),
                    redCaptainKills  = wrapped.redCaptainKills,
                    blueCaptainKills = wrapped.blueCaptainKills,
                    captainStatLine  = wrapped.captainStatLine,
                };
            }

            return null;
        }

        /// <summary>data 是否含非空 statLine（空数组视为无效）。</summary>
        public static bool HasNonEmptyStatLine(JObject data)
        {
            if (data == null) return false;

            var wrapped = data.ToObject<ScoreCardCommandData>();
            return wrapped?.statLine != null && wrapped.statLine.Count > 0;
        }

        /// <summary>将缓存的 settlement 曲线合并进 cmd.data（保留原有 baseData）。</summary>
        public static JObject MergeStatLineIntoData(JObject data, ScoreCardSettlementData cached)
        {
            if (cached?.settlementList == null || cached.settlementList.Count == 0)
            {
                return data;
            }

            var wrapped = data?.ToObject<ScoreCardCommandData>() ?? new ScoreCardCommandData();
            if (wrapped.statLine != null && wrapped.statLine.Count > 0)
            {
                return data;
            }

            wrapped.statLine = ToStatLineSamples(cached.settlementList);
            if (!HasCaptainKillEvents(wrapped.redCaptainKills) && HasCaptainKillEvents(cached.redCaptainKills))
            {
                wrapped.redCaptainKills = cached.redCaptainKills;
            }

            if (!HasCaptainKillEvents(wrapped.blueCaptainKills) && HasCaptainKillEvents(cached.blueCaptainKills))
            {
                wrapped.blueCaptainKills = cached.blueCaptainKills;
            }

            if (!HasCaptainStatLine(wrapped.captainStatLine) && HasCaptainStatLine(cached.captainStatLine))
            {
                wrapped.captainStatLine = cached.captainStatLine;
            }

            if (wrapped.baseData != null && cached.matchDuration > 0)
            {
                wrapped.baseData.matchDuration = cached.matchDuration;
            }

            return JObject.FromObject(wrapped);
        }

        static List<ScoreCardStatLineSample> ToStatLineSamples(List<ScoreCardSettlementSample> settlementList)
        {
            var list = new List<ScoreCardStatLineSample>(settlementList.Count);
            foreach (var s in settlementList)
            {
                if (s == null) continue;
                list.Add(new ScoreCardStatLineSample
                {
                    relativeSecond = s.time,
                    team1          = s.team1,
                    team2          = s.team2,
                });
            }

            return list;
        }

        /// <summary>
        /// PLAYER_JOIN：将 incoming 玩家 upsert 到现有快照。
        /// </summary>
        public static bool MergeJoinPlayers(ScoreCardMatchData snapshot, ScoreCardCommandData payload)
        {
            if (snapshot == null || payload?.playerList == null || payload.playerList.Count == 0)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(payload.playId))
            {
                snapshot.playId = payload.playId;
            }

            snapshot.playerList ??= new List<ScoreCardPlayer>();
            foreach (var p in payload.playerList)
            {
                if (p == null) continue;
                int idx = snapshot.playerList.FindIndex(x => x != null && x.id == p.id);
                if (idx >= 0)
                {
                    snapshot.playerList[idx] = p;
                }
                else
                {
                    snapshot.playerList.Add(p);
                }
            }

            snapshot.SyncTeamListAliases();
            RebuildTeamListsFromPlayers(snapshot);
            return true;
        }

        /// <summary>
        /// PLAYER_LEAVE：从快照移除指定 id 的玩家。
        /// </summary>
        public static bool RemovePlayers(ScoreCardMatchData snapshot, ScoreCardCommandData payload)
        {
            if (snapshot?.playerList == null || payload?.playerList == null || payload.playerList.Count == 0)
            {
                return false;
            }

            var removeIds = new HashSet<int>();
            foreach (var p in payload.playerList)
            {
                if (p != null) removeIds.Add(p.id);
            }

            if (removeIds.Count == 0) return false;

            snapshot.playerList.RemoveAll(p => p != null && removeIds.Contains(p.id));
            snapshot.SyncTeamListAliases();
            RebuildTeamListsFromPlayers(snapshot);
            return true;
        }

        public static ScoreCardCommandData ParseCommandData(JObject data)
        {
            return data?.ToObject<ScoreCardCommandData>();
        }

        /// <summary>WIN_RATE EVENT：读 <c>winRateLine</c>，映射为内部 <see cref="ScoreCardWinRateData"/>。</summary>
        public static ScoreCardWinRateData ToWinRateData(JObject data, int fallbackMatchDurationMin = 0)
        {
            if (data == null) return null;

            var wrapped = data.ToObject<ScoreCardCommandData>();
            if (wrapped?.winRateLine != null && wrapped.winRateLine.Count > 0)
            {
                return FromWinRateLine(wrapped.winRateLine, wrapped.matchDuration, fallbackMatchDurationMin);
            }

            return null;
        }

        static ScoreCardWinRateData FromWinRateLine(
            List<ScoreCardWinRateLineSample> winRateLine,
            int matchDuration,
            int fallbackMatchDurationMin)
        {
            var list = new List<ScoreCardWinRateSample>(winRateLine.Count);
            foreach (var s in winRateLine)
            {
                if (s == null) continue;
                list.Add(new ScoreCardWinRateSample
                {
                    time = s.relativeSecond,
                    team1 = new ScoreCardWinRateTeam { winRate = s.team1WinRate },
                    team2 = new ScoreCardWinRateTeam { winRate = s.team2WinRate },
                });
            }

            int duration = matchDuration > 0 ? matchDuration : fallbackMatchDurationMin;
            return new ScoreCardWinRateData
            {
                matchDuration = duration,
                winRateList   = list,
            };
        }

        static ScoreCardMatchData FromBaseData(ScoreCardBaseData baseData)
        {
            var match = new ScoreCardMatchData
            {
                storeName     = baseData.storeName,
                modeType      = baseData.modeType,
                modeName      = baseData.modeName,
                playId        = baseData.playId,
                placeCode     = baseData.placeCode,
                playName      = baseData.playName,
                playStatus    = baseData.playStatus,
                beginTime     = baseData.beginTime,
                endTime       = baseData.endTime,
                matchDuration = baseData.matchDuration,
                time          = baseData.matchDuration,
                reviveCd      = baseData.reviveCd,
            };

            if (!string.IsNullOrWhiteSpace(baseData.beginTime)
                && DateTime.TryParse(
                    baseData.beginTime,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var beginDt))
            {
                match.startTime = new DateTimeOffset(beginDt).ToUnixTimeMilliseconds();
            }

            ApplyTeamInfo(baseData.team1, match, 0);
            ApplyTeamInfo(baseData.team2, match, 1);
            ApplyTeamInfo(baseData.team3, match, 2);
            ApplyTeamInfo(baseData.team4, match, 3);
            ApplyTeamInfo(baseData.team5, match, 4);

            match.playerList = CollectPlayerListFromTeams(baseData);
            match.SyncTeamListAliases();
            return match;
        }

        static List<ScoreCardPlayer> CollectPlayerListFromTeams(ScoreCardBaseData baseData)
        {
            var list = new List<ScoreCardPlayer>();
            AppendTeamPlayers(list, baseData?.team1);
            AppendTeamPlayers(list, baseData?.team2);
            AppendTeamPlayers(list, baseData?.team3);
            AppendTeamPlayers(list, baseData?.team4);
            AppendTeamPlayers(list, baseData?.team5);
            return list;
        }

        static void AppendTeamPlayers(List<ScoreCardPlayer> target, ScoreCardTeamInfo team)
        {
            if (team?.players == null) return;

            foreach (var p in team.players)
            {
                if (p != null)
                {
                    target.Add(p);
                }
            }
        }

        static void ApplyTeamInfo(ScoreCardTeamInfo team, ScoreCardMatchData match, int teamIndex)
        {
            if (team == null) return;

            var players = team.players != null && team.players.Count > 0
                ? new List<ScoreCardPlayer>(team.players)
                : new List<ScoreCardPlayer>();
            string displayName = string.IsNullOrEmpty(team.name) ? DefaultTeamName(teamIndex) : team.name;
            match.SetTeamScore(teamIndex, team.score);
            match.SetTeamResultStatus(teamIndex, team.resultStatus);
            match.SetTeamCombatStats(teamIndex, team.hit, team.beHit, team.killNum, team.beKill, team.assistNum);

            switch (teamIndex)
            {
                case 0:
                    match.team1List = players;
                    match.redTeamList = players;
                    match.SetTeamDisplayName(0, displayName);
                    break;
                case 1:
                    match.team2List = players;
                    match.blueTeamList = players;
                    match.SetTeamDisplayName(1, displayName);
                    break;
                case 2:
                    match.team3List = players;
                    match.greenTeamList = players;
                    match.SetTeamDisplayName(2, displayName);
                    break;
                case 3:
                    match.team4List = players;
                    match.yellowTeamList = players;
                    match.SetTeamDisplayName(3, displayName);
                    break;
                case 4:
                    match.team5List = players;
                    match.purpleTeamList = players;
                    match.SetTeamDisplayName(4, displayName);
                    break;
            }
        }

        static string DefaultTeamName(int teamIndex)
        {
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

        static void RebuildTeamListsFromPlayers(ScoreCardMatchData match)
        {
            if (match?.playerList == null) return;

            match.team1List = new List<ScoreCardPlayer>();
            match.team2List = new List<ScoreCardPlayer>();
            match.team3List = new List<ScoreCardPlayer>();
            match.team4List = new List<ScoreCardPlayer>();
            match.team5List = new List<ScoreCardPlayer>();

            foreach (var p in match.playerList)
            {
                if (p == null) continue;
                switch (p.teamType)
                {
                    case "0": match.team1List.Add(p); break;
                    case "1": match.team2List.Add(p); break;
                    case "2": match.team3List.Add(p); break;
                    case "3": match.team4List.Add(p); break;
                    case "4": match.team5List.Add(p); break;
                }
            }

            match.redTeamList = match.team1List;
            match.blueTeamList = match.team2List;
            match.greenTeamList = match.team3List;
            match.yellowTeamList = match.team4List;
            match.purpleTeamList = match.team5List;
        }

        static ScoreCardSettlementData FromStatLine(
            List<ScoreCardStatLineSample> statLine,
            ScoreCardBaseData baseData,
            List<ScoreCardCaptainKillEvent> redCaptainKills,
            List<ScoreCardCaptainKillEvent> blueCaptainKills,
            List<ScoreCardCaptainStatSample> captainStatLine)
        {
            var list = new List<ScoreCardSettlementSample>(statLine.Count);
            foreach (var s in statLine)
            {
                if (s == null) continue;
                list.Add(new ScoreCardSettlementSample
                {
                    time  = s.relativeSecond,
                    team1 = s.team1,
                    team2 = s.team2,
                });
            }

            int matchDuration = baseData?.matchDuration ?? 0;
            if (matchDuration <= 0 && list.Count > 0)
            {
                int lastSec = list[list.Count - 1].time;
                matchDuration = Math.Max(1, (int)Math.Ceiling(lastSec / 60.0));
            }

            return new ScoreCardSettlementData
            {
                matchDuration    = matchDuration,
                actualMatchSec   = ResolveActualMatchSec(baseData),
                settlementList   = list,
                redCaptainKills  = redCaptainKills,
                blueCaptainKills = blueCaptainKills,
                captainStatLine  = captainStatLine,
            };
        }

        static bool HasCaptainKillEvents(List<ScoreCardCaptainKillEvent> kills)
        {
            return kills != null && kills.Count > 0;
        }

        static bool HasCaptainStatLine(List<ScoreCardCaptainStatSample> stat)
        {
            return stat != null && stat.Count > 0;
        }

        /// <summary>
        /// 由 baseData 的 beginTime/endTime 计算真实对战时长（秒）。
        /// 与 S4 <c>ScoreCardMatchCountdown.BeginTimeOffsetSec</c> 一致：扣掉 S3 倒计时视频。
        /// </summary>
        static int ResolveActualMatchSec(ScoreCardBaseData baseData)
        {
            if (baseData == null) return 0;
            if (!TryParseTime(baseData.beginTime, out var begin)) return 0;
            if (!TryParseTime(baseData.endTime, out var end)) return 0;

            // 与 LetsPiu.UI.ScoreCardMatchCountdown.BeginTimeOffsetSec 保持同步。
            const int beginTimeOffsetSec = 7;
            double sec = (end - begin).TotalSeconds - beginTimeOffsetSec;
            return sec > 0 ? (int)Math.Round(sec) : 0;
        }

        static bool TryParseTime(string text, out DateTime dt)
        {
            dt = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            return DateTime.TryParse(
                text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
        }
    }
}
