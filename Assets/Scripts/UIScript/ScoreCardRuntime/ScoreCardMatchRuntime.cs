using System;
using System.Collections.Generic;
using LetsPiu.Character;
using LetsPiu.Common.Network.Model.ScoreCard;
using UnityEngine;

namespace LetsPiu.UI.ScoreCardRuntime
{
    /// <summary>
    /// UWB 比分牌比赛数据运行时：订阅 ScoreCardCommandEvent，维护场次快照、写入 HUD，
    /// 并向 3D 层发出战斗演出提示。不负责切屏页面（单屏观战只留战斗态）。
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ScoreCardMatchRuntime : MonoBehaviour
    {
        const string HostName = "ScoreCardMatchRuntime_Host";
        const int DefaultReviveCdSec = 8;
        // 没有同步到位置回放时的展示缓冲（策划建议 300ms）
        const float DefaultDisplayBufferSec = 0.3f;

        public static ScoreCardMatchRuntime Current { get; private set; }

        public static event Action<ScoreCardCombatCue> CombatCue;

        /// <summary>死亡时刻起算的可复活时间戳（ms）。用于底部卡片 Ns可复活。</summary>
        static readonly Dictionary<int, long> s_ReviveReadyAtMs = new Dictionary<int, long>(64);

        struct Pending
        {
            public double due;
            public ScoreCardCommand cmd;
        }

        // 收到的消息先排队，到展示时刻再统一处理：血量、比分、击杀数、特效在同一帧更新，且与位置回放对齐
        readonly Queue<Pending> m_Pending = new Queue<Pending>(32);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Current != null)
                return;
            var host = new GameObject(HostName);
            DontDestroyOnLoad(host);
            host.AddComponent<ScoreCardMatchRuntime>();
        }

        void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
        }

        void OnEnable()
        {
            EventManager.Instance.AddEvent<ScoreCardCommand>(EventMsg.ScoreCardCommandEvent, OnCommand);
            CombatCue += OnCombatCue;
        }

        void OnDisable()
        {
            m_Pending.Clear();
            CombatCue -= OnCombatCue;
            EventManager.Instance.RemoveEvent<ScoreCardCommand>(EventMsg.ScoreCardCommandEvent, OnCommand);
            if (Current == this)
                Current = null;
        }

        void Update()
        {
            double now = Time.timeAsDouble;
            while (m_Pending.Count > 0 && m_Pending.Peek().due <= now)
                Dispatch(m_Pending.Dequeue().cmd);

            var match = ScoreCardMatchDataHub.Current;
            // 倒计时 / 复活秒数按时间轴每帧刷新；MQTT 事件稀疏时否则会卡住。
            TickRemainSeconds(match);
            TickPlayerStatsHud(match);
        }

        static void OnCombatCue(ScoreCardCombatCue cue)
        {
            var fx = VenueCombatFxPlayer.Current;
            if (fx == null)
                return;
            fx.HandleCue(cue.type.ToString(), cue.actorId, cue.targetId, cue.teamType);
        }

        void OnCommand(ScoreCardCommand cmd)
        {
            if (cmd == null || string.IsNullOrEmpty(cmd.subCode))
                return;

            // 与位置回放用同一个延迟（含纠偏带来的额外延迟），事件和人物位置才对得上
            var sync = VenuePlayerSync.Current;
            float delay = sync != null ? sync.interpolationDelay : DefaultDisplayBufferSec;
            m_Pending.Enqueue(new Pending { due = Time.timeAsDouble + Mathf.Max(0f, delay), cmd = cmd });
        }

        void Dispatch(ScoreCardCommand cmd)
        {
            // 服务端对首杀、连杀、BUFF 夺取会先发一条给语音播报用的 NOTICE，再发带完整数据的 EVENT；
            // 数据与特效只认 EVENT，两条都处理会把同一件事触发两次
            if (string.Equals(cmd.category, ScoreCardCommandCategory.Notice, StringComparison.OrdinalIgnoreCase))
                return;

            string sub = cmd.subCode;
            if (string.Equals(sub, ScoreCardSubCode.S4, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sub, ScoreCardSubCode.BeginPlay, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sub, ScoreCardSubCode.RefreshData, StringComparison.OrdinalIgnoreCase))
            {
                // 新一局开始：上一局的持续 BUFF 特效不带过来
                bool isRefresh = string.Equals(sub, ScoreCardSubCode.RefreshData, StringComparison.OrdinalIgnoreCase);
                if (!isRefresh)
                    VenueCombatFxPlayer.Current?.ClearAllBuffs();
                ApplyFullMatch(cmd, isRefresh);
                return;
            }

            if (string.Equals(sub, ScoreCardSubCode.Shoot, StringComparison.OrdinalIgnoreCase))
            {
                ApplyShoot(cmd);
                return;
            }

            if (string.Equals(sub, ScoreCardSubCode.PlayerRevive, StringComparison.OrdinalIgnoreCase))
            {
                ApplyRevive(cmd);
                return;
            }

            if (string.Equals(sub, ScoreCardSubCode.TowerKill, StringComparison.OrdinalIgnoreCase))
            {
                ApplyTowerKill(cmd);
                return;
            }

            if (string.Equals(sub, ScoreCardSubCode.MultiKill, StringComparison.OrdinalIgnoreCase))
            {
                ApplyMultiKill(cmd);
                return;
            }

            if (string.Equals(sub, ScoreCardSubCode.FirstBlood, StringComparison.OrdinalIgnoreCase))
            {
                ApplyFirstBlood(cmd);
                return;
            }

            if (string.Equals(sub, ScoreCardSubCode.RedBuffCaptured, StringComparison.OrdinalIgnoreCase))
            {
                ApplyBuffCaptured(cmd, isRed: true);
                return;
            }

            if (string.Equals(sub, ScoreCardSubCode.BlueBuffCaptured, StringComparison.OrdinalIgnoreCase))
            {
                ApplyBuffCaptured(cmd, isRed: false);
            }
        }

        void ApplyFullMatch(ScoreCardCommand cmd, bool isRefresh)
        {
            var match = cmd.ToMatchData();
            if (match == null)
                return;

            match.SyncTeamListAliases();
            ScoreCardMatchDataHub.Publish(match);
            // 回血只靠 REFRESH_DATA 里血量比本地上一次高来判断；S4/BEGIN_PLAY 是新一局重置，只同步不播。
            // 复活（血量从 0 回升）仍只认 PLAYER_REVIVE，NotifyHp 会排除
            SyncCombatFromMatch(match, playHpFx: isRefresh, clearBodyFx: true);
            SyncHudFromMatch(match);
        }

        void ApplyShoot(ScoreCardCommand cmd)
        {
            var match = ScoreCardMatchDataHub.Current;
            if (match == null)
                return;

            var shoot = cmd.ToShootEventData();
            if (shoot == null || !ScoreCardPlayerMergeUtil.ApplyShootEvent(match, shoot))
                return;

            ScoreCardMatchDataHub.Publish(match);
            SyncCombatFromMatch(match, playHpFx: false);
            SyncHudFromMatch(match);

            int shooterId = shoot.shooter != null ? shoot.shooter.ResolvePlayerId() : 0;
            int victimId = shoot.victim != null ? shoot.victim.ResolvePlayerId() : 0;
            bool kill = string.Equals(shoot.type, ScoreCardShootEventType.Kill, StringComparison.OrdinalIgnoreCase)
                        || (shoot.victim != null && shoot.victim.blood.HasValue && shoot.victim.blood.Value <= 0);
            Emit(kill ? ScoreCardCombatCueType.ShootKill : ScoreCardCombatCueType.ShootHit, shooterId, victimId, TeamOf(match, shooterId));
        }

        void ApplyRevive(ScoreCardCommand cmd)
        {
            var match = ScoreCardMatchDataHub.Current;
            if (match == null)
                return;

            var revive = cmd.ToReviveEventData();
            if (revive == null || !ScoreCardPlayerMergeUtil.ApplyReviveEvent(match, revive))
                return;

            ScoreCardMatchDataHub.Publish(match);
            // 血量同步不播回血；复活演出只走下面的 Revive cue
            SyncCombatFromMatch(match, playHpFx: false);
            SyncHudFromMatch(match);

            int id = revive.player != null ? revive.player.ResolvePlayerId() : 0;
            Emit(ScoreCardCombatCueType.Revive, id, 0, TeamOf(match, id));
        }

        void ApplyTowerKill(ScoreCardCommand cmd)
        {
            var match = ScoreCardMatchDataHub.Current;
            if (match == null)
                return;

            var tower = cmd.ToTowerKillEventData();
            if (tower == null || !ScoreCardPlayerMergeUtil.ApplyTowerKillEvent(match, tower))
                return;

            ScoreCardMatchDataHub.Publish(match);
            SyncCombatFromMatch(match, playHpFx: false);
            SyncHudFromMatch(match);

            int victimId = tower.victim != null ? tower.victim.ResolvePlayerId() : 0;
            Emit(ScoreCardCombatCueType.TowerKill, 0, victimId, TeamOf(match, victimId));
        }

        void ApplyMultiKill(ScoreCardCommand cmd)
        {
            var match = ScoreCardMatchDataHub.Current;
            if (match == null)
                return;

            var multi = cmd.ToMultiKillEventData();
            if (multi == null || !ScoreCardPlayerMergeUtil.ApplyMultiKillEvent(match, multi))
                return;

            ScoreCardMatchDataHub.Publish(match);
            SyncHudFromMatch(match);

            var shooter = multi.ResolveShooter();
            int id = shooter != null ? shooter.ResolvePlayerId() : 0;
            Emit(ScoreCardCombatCueType.MultiKill, id, 0, TeamOf(match, id));
        }

        void ApplyFirstBlood(ScoreCardCommand cmd)
        {
            var match = ScoreCardMatchDataHub.Current;
            if (match == null)
                return;

            var first = cmd.ToFirstBloodEventData();
            if (first == null)
                return;

            ScoreCardPlayerMergeUtil.ApplyFirstBloodEvent(match, first);
            ScoreCardMatchDataHub.Publish(match);
            SyncHudFromMatch(match);

            var killer = first.ResolveKiller();
            int id = killer != null ? killer.ResolvePlayerId() : 0;
            Emit(ScoreCardCombatCueType.MultiKill, id, 0, TeamOf(match, id));
        }

        void ApplyBuffCaptured(ScoreCardCommand cmd, bool isRed)
        {
            var match = ScoreCardMatchDataHub.Current;
            if (match == null)
                return;

            var buff = cmd.ToBuffCapturedEventData();
            if (buff == null || !ScoreCardPlayerMergeUtil.ApplyBuffCapturedEvent(match, buff))
                return;

            ScoreCardMatchDataHub.Publish(match);
            SyncHudFromMatch(match);

            var capturer = buff.ResolveCapturer();
            int id = capturer != null ? capturer.ResolvePlayerId() : 0;
            // venueTeam：1=红 0=蓝
            Emit(isRed ? ScoreCardCombatCueType.BuffRed : ScoreCardCombatCueType.BuffBlue, id, 0, isRed ? 1 : 0);
        }

        static void SyncCombatFromMatch(
            ScoreCardMatchData match,
            bool playHpFx = false,
            bool clearBodyFx = false)
        {
            if (match?.playerList == null)
                return;

            int reviveCdSec = match.reviveCd > 0 ? match.reviveCd : DefaultReviveCdSec;
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var fx = VenueCombatFxPlayer.Current;
            for (int i = 0; i < match.playerList.Count; i++)
            {
                var p = match.playerList[i];
                if (p == null || p.id <= 0)
                    continue;
                int maxHp = p.initBlood > 0 ? p.initBlood : 100;
                bool wasDead = ScoreCardCombatState.TryGet(p.id, out var prev) && prev.hp <= 0;
                if (playHpFx)
                    fx?.NotifyHp(p.id, p.blood);
                else
                    fx?.SeedHp(p.id, p.blood);
                if (clearBodyFx && p.blood > 0)
                    fx?.ClearBodyCombatFx(p.id);
                if (p.blood <= 0)
                    fx?.ClearBuff(p.id);
                ScoreCardCombatState.Set(p.id, p.blood, maxHp);

                if (p.blood <= 0)
                {
                    // 新死亡：按 reviveCd 起算底部「Ns可复活」
                    if (!wasDead || !s_ReviveReadyAtMs.ContainsKey(p.id))
                        s_ReviveReadyAtMs[p.id] = nowMs + reviveCdSec * 1000L;
                }
                else
                {
                    s_ReviveReadyAtMs.Remove(p.id);
                }
            }
        }

        static void SyncHudFromMatch(ScoreCardMatchData match)
        {
            if (match == null)
                return;

            UwbHudData.UseMock = false;
            UwbHudData.RedScore = match.TryGetTeamScore(0, out int redScore) ? redScore : 0;
            UwbHudData.BlueScore = match.TryGetTeamScore(1, out int blueScore) ? blueScore : 0;
            UwbHudData.RedEliminated = CountDead(match.team1List ?? match.redTeamList);
            UwbHudData.BlueEliminated = CountDead(match.team2List ?? match.blueTeamList);

            TickRemainSeconds(match);
            TickPlayerStatsHud(match);
        }

        static void TickRemainSeconds(ScoreCardMatchData match)
        {
            if (match == null)
                return;

            int durationMin = match.matchDuration > 0 ? match.matchDuration : match.time;
            if (durationMin <= 0 || match.startTime <= 0)
                return;

            long endMs = match.startTime + durationMin * 60L * 1000L;
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UwbHudData.RemainSeconds = Mathf.Max(0f, (endMs - nowMs) / 1000f);
        }

        static void TickPlayerStatsHud(ScoreCardMatchData match)
        {
            if (match?.playerList == null)
                return;

            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int i = 0; i < match.playerList.Count; i++)
            {
                var p = match.playerList[i];
                if (p == null || p.id <= 0)
                    continue;

                int reviveSec = -1;
                if (p.blood <= 0 && s_ReviveReadyAtMs.TryGetValue(p.id, out long readyAt))
                    reviveSec = Mathf.Max(0, (int)Math.Ceiling((readyAt - nowMs) / 1000.0));

                UwbHudData.SetStats(p.id, p.killNum, p.beKillNum, reviveSec);
            }
        }

        static int CountDead(System.Collections.Generic.List<ScoreCardPlayer> list)
        {
            if (list == null)
                return 0;
            int n = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].blood <= 0)
                    n++;
            }

            return n;
        }

        /// <summary>返回场馆队色：0=蓝，1=红（ScoreCard teamType 0=红 1=蓝，需反转）。</summary>
        static int TeamOf(ScoreCardMatchData match, int playerId)
        {
            var p = ScoreCardPlayerMergeUtil.FindPlayer(match, playerId);
            if (p == null)
                return 0;
            return string.Equals(p.teamType, "0", StringComparison.Ordinal) ? 1 : 0;
        }

        static void Emit(ScoreCardCombatCueType type, int actorId, int targetId, int teamType)
        {
            CombatCue?.Invoke(new ScoreCardCombatCue(type, actorId, targetId, teamType));
        }
    }
}
