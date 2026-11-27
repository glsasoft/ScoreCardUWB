using System.Collections.Generic;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 将 SHOOT 等事件中的 <see cref="ScoreCardPlayerPartial"/> 合并到场次玩家快照。
    /// </summary>
    public static class ScoreCardPlayerMergeUtil
    {
        public static bool ApplyShootEvent(ScoreCardMatchData match, ScoreCardShootEventData shootEvent)
        {
            if (match == null || shootEvent == null)
            {
                return false;
            }

            bool updated = false;
            if (shootEvent.shooter != null)
            {
                updated |= TryMergePartial(match, shootEvent.shooter);
            }

            if (shootEvent.victim != null)
            {
                updated |= TryMergePartial(match, shootEvent.victim);
            }

            if (shootEvent.assistList != null)
            {
                for (int i = 0; i < shootEvent.assistList.Count; i++)
                {
                    updated |= TryMergePartial(match, shootEvent.assistList[i]);
                }
            }

            // 击杀数 / 被击杀数完全以服务器数据为准：
            // 若 SHOOT 携带 killNum/beKillNum 则经 TryMergePartial 写入，否则等 REFRESH_DATA 全量刷新，客户端不再本地累加。

            return updated;
        }

        public static bool ApplyFirstBloodEvent(ScoreCardMatchData match, ScoreCardFirstBloodEventData firstBloodEvent)
        {
            if (match == null || firstBloodEvent == null)
            {
                return false;
            }

            bool updated = false;
            var killer = firstBloodEvent.ResolveKiller();
            if (killer != null)
            {
                updated |= TryMergePartial(match, killer);
            }

            var victim = firstBloodEvent.ResolveVictim();
            if (victim != null)
            {
                updated |= TryMergePartial(match, victim);
            }

            return updated;
        }

        public static bool ApplyTowerKillEvent(ScoreCardMatchData match, ScoreCardTowerKillEventData towerKillEvent)
        {
            return match != null && towerKillEvent?.victim != null && TryMergePartial(match, towerKillEvent.victim);
        }

        public static bool ApplyReviveEvent(ScoreCardMatchData match, ScoreCardReviveEventData reviveEvent)
        {
            return match != null && reviveEvent?.player != null && TryMergePartial(match, reviveEvent.player);
        }

        public static bool ApplyBuffCapturedEvent(ScoreCardMatchData match, ScoreCardBuffCapturedEventData buffCapturedEvent)
        {
            var capturer = buffCapturedEvent?.ResolveCapturer();
            if (match == null || capturer == null)
            {
                return false;
            }

            return TryMergePartial(match, capturer);
        }

        public static bool ApplyMultiKillEvent(ScoreCardMatchData match, ScoreCardMultiKillEventData multiKillEvent)
        {
            var shooterPartial = multiKillEvent?.ResolveShooter();
            if (match == null || shooterPartial == null)
            {
                return false;
            }

            // killNum 是累计击杀数（服务端连杀事件里的 shooter 已带最新值），连杀数只在 comboCount 里，不能写进 killNum。
            return TryMergePartial(match, shooterPartial);
        }

        public static bool TryMergePartial(ScoreCardMatchData match, ScoreCardPlayerPartial partial)
        {
            if (match == null || partial == null)
            {
                return false;
            }

            int playerId = partial.ResolvePlayerId();
            if (playerId <= 0)
            {
                return false;
            }

            var player = FindPlayer(match, playerId);
            if (player == null)
            {
                return false;
            }

            partial.MergeInto(player);
            return true;
        }

        public static ScoreCardPlayer FindPlayer(ScoreCardMatchData match, int playerId)
        {
            if (match == null || playerId <= 0)
            {
                return null;
            }

            match.SyncTeamListAliases();

            var player = FindInList(match.playerList, playerId);
            if (player != null)
            {
                return player;
            }

            foreach (var (_, players) in match.GetNonEmptyTeams())
            {
                player = FindInList(players, playerId);
                if (player != null)
                {
                    return player;
                }
            }

            return null;
        }

        static ScoreCardPlayer FindInList(List<ScoreCardPlayer> players, int playerId)
        {
            if (players == null)
            {
                return null;
            }

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player != null && player.id == playerId)
                {
                    return player;
                }
            }

            return null;
        }
    }
}
