using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LetsPiu.Character;
using LetsPiu.Character.Uwb;
using LetsPiu.Common.Network.Model.ScoreCard;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LetsPiu.UI.ScoreCardRuntime
{
    /// <summary>
    /// 积分模式战斗 Mock：5 分钟一场循环；高频互打全员掉血；约每分钟 1～2 人死→回出生点→复活。
    /// </summary>
    public class ScoreCardScoreModeMock : MonoBehaviour, IScoreCardModeMock
    {
        const float HomeRadiusM = 2.2f;
        const float HomeTimeoutSec = 25f;
        const float ReviveCdSec = 8f;
        const int MatchDurationMin = 5;
        const int MaxHp = 100;
        const string LogTag = "ScoreCardScoreModeMock";

        public static ScoreCardScoreModeMock Current { get; private set; }

        public string ModeType => ScoreCardGameMode.Score;
        public string DisplayName => "积分模式";
        public bool IsRunning => _running;

        public static bool Enabled => Current != null && Current._running;

        [SerializeField] float hitIntervalSec = 0.5f;
        [SerializeField] float firstKillDelaySec = 40f;
        [SerializeField] float killIntervalSec = 50f;
        [SerializeField] int killsPerWave = 1;
        [SerializeField] float towerIntervalSec = 100f;
        [SerializeField] float buffFirstDelaySec = 15f;
        [SerializeField] float buffIntervalSec = 25f;
        [SerializeField] int buffHealPerSec = 5;
        [SerializeField] int hitDamageMin = 6;
        [SerializeField] int hitDamageMax = 14;
        /// <summary>低血量权重倍率：残血越容易被集火，避免全员均匀掉血后一起死。</summary>
        [SerializeField] float lowHpFocusPower = 2.2f;

        bool _running;
        CancellationTokenSource _cts;
        readonly List<VenuePlayerView> _views = new List<VenuePlayerView>(32);
        readonly Dictionary<int, int> _blood = new Dictionary<int, int>(32);
        readonly Dictionary<int, int> _kills = new Dictionary<int, int>(32);
        readonly Dictionary<int, int> _deaths = new Dictionary<int, int>(32);
        readonly HashSet<int> _busy = new HashSet<int>();
        // 玩家 → 持有的 BUFF（true 红 / false 蓝），死亡与新一局清空
        readonly Dictionary<int, bool> _buffHolders = new Dictionary<int, bool>(8);
        readonly List<int> _healScratch = new List<int>(8);
        bool _firstBloodSent;
        bool _nextBuffRed = true;
        readonly List<int> _red = new List<int>(16);
        readonly List<int> _blue = new List<int>(16);
        int _redScore;
        int _blueScore;
        int _hitCursor;
        long _matchStartMs;
        CancellationTokenSource _loopCts;

        void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(this);
                return;
            }

            Current = this;
        }

        void OnDestroy()
        {
            StopMock();
            if (Current == this)
                Current = null;
        }

        public void StartMock()
        {
            if (_running)
                return;
            _running = true;
            _cts = new CancellationTokenSource();
            _loopCts = _cts;
            RunLoop(_cts.Token).Forget();
        }

        public void StopMock()
        {
            _running = false;
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            _loopCts = null;
        }

        async UniTaskVoid RunLoop(CancellationToken ct)
        {
            try
            {
                await BeginMatchAsync(ct);
                float nextHit = 0f;
                // 先让人走出出生点进场，再开始击杀波
                float nextKill = firstKillDelaySec;
                float nextTower = firstKillDelaySec + 25f;
                float nextScorePush = 1.5f;
                float nextBuff = buffFirstDelaySec;
                float nextHeal = 1f;
                float t = 0f;

                while (!ct.IsCancellationRequested)
                {
                    float dt = 0.05f;
                    await UniTask.Delay(TimeSpan.FromSeconds(dt), cancellationToken: ct);
                    t += dt;

                    CollectLiveTeams();
                    if (_red.Count == 0 || _blue.Count == 0)
                        continue;

                    long elapsedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _matchStartMs;
                    if (elapsedMs >= MatchDurationMin * 60L * 1000L)
                    {
                        await BeginMatchAsync(ct);
                        t = 0f;
                        nextHit = 0f;
                        nextKill = firstKillDelaySec;
                        nextTower = firstKillDelaySec + 25f;
                        nextScorePush = 1.5f;
                        nextBuff = buffFirstDelaySec;
                        nextHeal = 1f;
                        continue;
                    }

                    if (t >= nextHit)
                    {
                        FireScatteredHits();
                        nextHit = t + hitIntervalSec;
                    }

                    if (t >= nextKill)
                    {
                        int n = killsPerWave;
                        // 约每分钟 1 人，偶尔 2 人
                        if (Random.value < 0.25f)
                            n = 2;
                        for (int i = 0; i < n; i++)
                            TryStartKillWave(ct);
                        nextKill = t + killIntervalSec;
                    }

                    if (t >= nextTower)
                    {
                        TryStartTowerKill(ct);
                        nextTower = t + towerIntervalSec;
                    }

                    if (t >= nextBuff)
                    {
                        TryCaptureBuff();
                        nextBuff = t + buffIntervalSec;
                    }

                    if (t >= nextHeal)
                    {
                        HealBlueBuffHolders();
                        nextHeal = t + 1f;
                    }

                    // HIT 不改 match 总分，需定期 RefreshData 才能驱动顶栏/比分条
                    if (t >= nextScorePush)
                    {
                        PushScoreSnapshot();
                        nextScorePush = t + 1.5f;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{LogTag}] " + e.Message);
            }
        }

        async UniTask BeginMatchAsync(CancellationToken ct)
        {
            // 人数由设置面板决定：固定手填，或 10→20→40 循环
            int roster = UwbMockBattleHooks.TakeMatchPlayerCount != null
                ? UwbMockBattleHooks.TakeMatchPlayerCount()
                : 20;
            // 人数一确定就让 HUD 切到对应档位，不等定位 Mock 重建玩家
            UwbHudData.NotifyNewMatchRoster(roster);
            UwbMockBattleHooks.SetMockPlayerCount?.Invoke(roster);

            // 等定位 Mock 重建出双方真人（CollectLiveTeams 在空场时会灌假名单并返回 false）
            for (int i = 0; i < 50 && !ct.IsCancellationRequested; i++)
            {
                if (CollectLiveTeams() && Mathf.Abs(_red.Count + _blue.Count - roster) <= 2)
                    break;
                await UniTask.Delay(100, cancellationToken: ct);
            }

            UwbMockBattleHooks.ResetAllToTeamSpawns?.Invoke();
            await UniTask.Delay(150, cancellationToken: ct);

            _matchStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _redScore = 0;
            _blueScore = 0;
            _blood.Clear();
            _kills.Clear();
            _deaths.Clear();
            _busy.Clear();
            _buffHolders.Clear();
            _firstBloodSent = false;
            CollectLiveTeams();
            for (int i = 0; i < _red.Count; i++)
                _blood[_red[i]] = MaxHp;
            for (int i = 0; i < _blue.Count; i++)
                _blood[_blue[i]] = MaxHp;

            Fire(ScoreCardMockMqtt.BuildPage(ScoreCardSubCode.S4, BuildBaseData(aliveAll: true)));
            Debug.Log(
                $"[{LogTag}] match begin {MatchDurationMin}min roster={roster} " +
                $"players={_red.Count}+{_blue.Count} score=0/0");
        }

        void PushScoreSnapshot()
        {
            Fire(ScoreCardMockMqtt.BuildPage(ScoreCardSubCode.RefreshData, BuildBaseData(aliveAll: false)));
        }

        void AddTeamScore(bool redTeam, int delta)
        {
            if (delta == 0)
                return;
            if (redTeam)
                _redScore = Mathf.Max(0, _redScore + delta);
            else
                _blueScore = Mathf.Max(0, _blueScore + delta);
        }

        void FireScatteredHits()
        {
            // 仅敌对互打；优先集火残血，少数先死，避免全员平均掉血后一起倒。
            int pairs = Mathf.Clamp(1 + (_red.Count + _blue.Count) / 12, 1, 3);
            var ct = _loopCts != null ? _loopCts.Token : CancellationToken.None;
            for (int p = 0; p < pairs; p++)
            {
                if (!TryPickFightPair(out int shooter, out int victim, out bool shooterIsRed))
                    return;

                if (_busy.Contains(victim) || BloodOf(victim) <= 0)
                    continue;

                int dmg = Random.Range(hitDamageMin, hitDamageMax + 1);
                // 残血再挨打时略增伤，更容易被补死
                if (BloodOf(victim) <= 40)
                    dmg += Random.Range(2, 6);

                int next = BloodOf(victim) - dmg;
                if (next <= 0)
                {
                    if (_busy.Add(victim))
                        KillThenReviveAsync(shooter, victim, shooterIsRed, ct).Forget();
                    _hitCursor++;
                    continue;
                }

                SetBlood(victim, next);
                Fire(ScoreCardMockMqtt.BuildEvent(
                    ScoreCardSubCode.Shoot,
                    BuildShoot("HIT", shooter, victim, next)));

                AddTeamScore(shooterIsRed, 2);
                _hitCursor++;
            }
        }

        void TryStartKillWave(CancellationToken ct)
        {
            if (!TryPickKillVictim(out int victim, out int killer, out bool killerIsRed))
                return;
            if (!_busy.Add(victim))
                return;

            KillThenReviveAsync(killer, victim, killerIsRed, ct).Forget();
        }

        void TryStartTowerKill(CancellationToken ct)
        {
            // 优先炮杀残血一方
            bool victimIsRed = Random.value < 0.5f;
            var list = victimIsRed ? _red : _blue;
            int victim = PickWeightedAlive(list);
            if (victim <= 0 || !_busy.Add(victim))
                return;
            TowerKillThenReviveAsync(victim, ct).Forget();
        }

        // 夺取者随机取一方的存活玩家，红蓝 BUFF 交替出现
        void TryCaptureBuff()
        {
            var list = Random.value < 0.5f ? _red : _blue;
            int id = PickAlive(list);
            if (id <= 0)
                return;

            bool red = _nextBuffRed;
            _nextBuffRed = !_nextBuffRed;
            _buffHolders[id] = red;
            Fire(ScoreCardMockMqtt.BuildEvent(
                red ? ScoreCardSubCode.RedBuffCaptured : ScoreCardSubCode.BlueBuffCaptured,
                "{\"level\":1,\"killer\":{\"id\":" + id + "}}"));
        }

        // 蓝 BUFF（回复）持有者每秒回血，下一次快照里血量上涨
        void HealBlueBuffHolders()
        {
            _healScratch.Clear();
            foreach (var pair in _buffHolders)
            {
                if (!pair.Value)
                    _healScratch.Add(pair.Key);
            }

            for (int i = 0; i < _healScratch.Count; i++)
            {
                int id = _healScratch[i];
                int hp = BloodOf(id);
                if (hp > 0 && hp < MaxHp)
                    SetBlood(id, hp + buffHealPerSec);
            }
        }

        async UniTaskVoid KillThenReviveAsync(int killerId, int victimId, bool killerIsRed, CancellationToken ct)
        {
            try
            {
                SetBlood(victimId, 0);
                _buffHolders.Remove(victimId);
                int killerKills = IncStat(_kills, killerId);
                int victimDeaths = IncStat(_deaths, victimId);
                Fire(ScoreCardMockMqtt.BuildEvent(
                    ScoreCardSubCode.Shoot,
                    BuildShoot(
                        ScoreCardShootEventType.Kill,
                        killerId,
                        victimId,
                        0,
                        killerKills,
                        victimDeaths)));
                AddTeamScore(killerIsRed, 15);
                PushScoreSnapshot();
                if (!_firstBloodSent)
                {
                    _firstBloodSent = true;
                    Fire(ScoreCardMockMqtt.BuildEvent(
                        ScoreCardSubCode.FirstBlood,
                        "{\"level\":1,\"killer\":{\"id\":" + killerId + "},\"victim\":{\"id\":" + victimId + "}}"));
                }
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: ct);
                if (Random.value < 0.4f)
                {
                    Fire(ScoreCardMockMqtt.BuildEvent(
                        ScoreCardSubCode.MultiKill,
                        BuildMultiKill(killerId, Random.Range(2, 5))));
                }

                await WaitHomeThenReviveAsync(victimId, ct);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _busy.Remove(victimId);
            }
        }

        async UniTaskVoid TowerKillThenReviveAsync(int victimId, CancellationToken ct)
        {
            try
            {
                SetBlood(victimId, 0);
                _buffHolders.Remove(victimId);
                // 炮杀：受害者对侧得分
                bool victimIsRed = _red.Contains(victimId);
                AddTeamScore(!victimIsRed, 12);
                Fire(ScoreCardMockMqtt.BuildEvent(
                    ScoreCardSubCode.TowerKill,
                    "{\"victim\":{\"id\":" + victimId + ",\"blood\":0}}"));
                PushScoreSnapshot();
                await WaitHomeThenReviveAsync(victimId, ct);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _busy.Remove(victimId);
            }
        }

        async UniTask WaitHomeThenReviveAsync(int victimId, CancellationToken ct)
        {
            // 底部卡片按 reviveCd=8s 倒计时；到点且靠近出生点（或超时）再发 REVIVE。
            float waited = 0f;
            while (waited < HomeTimeoutSec && !ct.IsCancellationRequested)
            {
                bool near = UwbMockBattleHooks.IsNearHome != null &&
                            UwbMockBattleHooks.IsNearHome(victimId, HomeRadiusM);
                bool cdDone = waited >= ReviveCdSec;
                if (near && cdDone)
                    break;
                await UniTask.Delay(200, cancellationToken: ct);
                waited += 0.2f;
            }

            SetBlood(victimId, MaxHp);
            Fire(ScoreCardMockMqtt.BuildEvent(
                ScoreCardSubCode.PlayerRevive,
                "{\"player\":{\"id\":" + victimId + ",\"blood\":100}}"));
            await UniTask.Delay(TimeSpan.FromSeconds(0.6f), cancellationToken: ct);
        }

        bool TryPickFightPair(out int shooter, out int victim, out bool shooterIsRed)
        {
            shooter = victim = 0;
            shooterIsRed = false;
            if (_red.Count == 0 || _blue.Count == 0)
                return false;

            shooterIsRed = Random.value < 0.5f;
            var shooters = shooterIsRed ? _red : _blue;
            var victims = shooterIsRed ? _blue : _red;
            shooter = PickAlive(shooters);
            // 受害者按残血加权，血越少被打到的概率越高
            victim = PickWeightedAlive(victims);
            return shooter > 0 && victim > 0;
        }

        bool TryPickKillVictim(out int victim, out int killer, out bool killerIsRed)
        {
            victim = killer = 0;
            killerIsRed = Random.value < 0.5f;
            var victims = killerIsRed ? _blue : _red;
            var killers = killerIsRed ? _red : _blue;
            victim = PickWeightedAlive(victims);
            killer = PickAlive(killers);
            return victim > 0 && killer > 0 && victim != killer;
        }

        int PickAlive(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return 0;
            int start = Random.Range(0, ids.Count);
            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[(start + i) % ids.Count];
                if (id > 0 && !_busy.Contains(id) && BloodOf(id) > 0)
                    return id;
            }

            return 0;
        }

        /// <summary>血量越低权重越高（约满血 1×，空血附近数倍），形成集火少数目标。</summary>
        int PickWeightedAlive(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return 0;

            float total = 0f;
            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                if (id <= 0 || _busy.Contains(id) || BloodOf(id) <= 0)
                    continue;
                total += VictimFocusWeight(id);
            }

            if (total <= 0.0001f)
                return PickAlive(ids);

            float r = Random.value * total;
            float acc = 0f;
            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                if (id <= 0 || _busy.Contains(id) || BloodOf(id) <= 0)
                    continue;
                acc += VictimFocusWeight(id);
                if (r <= acc)
                    return id;
            }

            return PickAlive(ids);
        }

        float VictimFocusWeight(int id)
        {
            int hp = BloodOf(id);
            float missing = 1f - Mathf.Clamp01(hp / (float)MaxHp);
            // missing^power：残血权重陡增，满血仍有基础机会被点到
            return 1f + Mathf.Pow(missing, lowHpFocusPower) * 10f;
        }

        int BloodOf(int id) => _blood.TryGetValue(id, out int b) ? b : MaxHp;

        void SetBlood(int id, int hp) => _blood[id] = Mathf.Clamp(hp, 0, MaxHp);

        static int IncStat(Dictionary<int, int> map, int id)
        {
            int v = map.TryGetValue(id, out int cur) ? cur + 1 : 1;
            map[id] = v;
            return v;
        }

        bool CollectLiveTeams()
        {
            _red.Clear();
            _blue.Clear();
            _views.Clear();
            var sync = VenuePlayerSync.Current;
            if (sync != null)
                sync.CollectViews(_views);

            for (int i = 0; i < _views.Count; i++)
            {
                var v = _views[i];
                if (v == null)
                    continue;
                if (v.Team == 0)
                    _blue.Add(v.Id);
                else
                    _red.Add(v.Id);
            }

            if (_red.Count == 0 || _blue.Count == 0)
            {
                for (int i = 1; i <= 5; i++)
                    _red.Add(i * 2);
                for (int i = 0; i < 5; i++)
                    _blue.Add(i * 2 + 1);
                return false;
            }

            return true;
        }

        string BuildBaseData(bool aliveAll)
        {
            // Normalizer 只认 beginTime 字符串，不认 startTime 数字——缺 beginTime 则倒计时一直 --:--。
            // 开始时间必须固定为本场开局时刻，每次快照取 Now 会让倒计时被反复重置回 5:00
            string begin = DateTimeOffset.FromUnixTimeMilliseconds(_matchStartMs).LocalDateTime
                .ToString("yyyy-MM-dd HH:mm:ss");
            var sb = new StringBuilder(2048);
            sb.Append("{\"baseData\":{");
            sb.Append("\"playId\":\"UWB_SCORE_MOCK\",\"playName\":\"UWB积分Mock\",");
            sb.Append("\"modeType\":\"").Append(ScoreCardGameMode.Score).Append("\",");
            sb.Append("\"modeName\":\"积分模式\",");
            sb.Append("\"matchDuration\":").Append(MatchDurationMin).Append(',');
            sb.Append("\"reviveCd\":8,");
            sb.Append("\"beginTime\":\"").Append(begin).Append("\",");
            sb.Append("\"team1\":{\"name\":\"红队\",\"score\":").Append(_redScore).Append(",\"players\":[");
            AppendPlayers(sb, _red, "0", aliveAll);
            sb.Append("]},\"team2\":{\"name\":\"蓝队\",\"score\":").Append(_blueScore).Append(",\"players\":[");
            AppendPlayers(sb, _blue, "1", aliveAll);
            sb.Append("]}}}");
            return sb.ToString();
        }

        void AppendPlayers(StringBuilder sb, List<int> ids, string teamType, bool aliveAll)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) sb.Append(',');
                int id = ids[i];
                int blood = aliveAll ? MaxHp : BloodOf(id);
                sb.Append("{\"id\":").Append(id);
                sb.Append(",\"vestId\":").Append(id);
                sb.Append(",\"customerName\":\"P").Append(id).Append('"');
                sb.Append(",\"teamType\":\"").Append(teamType).Append('"');
                int kills = _kills.TryGetValue(id, out int k) ? k : 0;
                int deaths = _deaths.TryGetValue(id, out int d) ? d : 0;
                sb.Append(",\"blood\":").Append(blood).Append(",\"initBlood\":").Append(MaxHp).Append(',');
                sb.Append("\"killNum\":").Append(kills);
                sb.Append(",\"beKillNum\":").Append(deaths).Append(",\"hitNum\":0}");
            }
        }

        static string BuildShoot(
            string type,
            int shooterId,
            int victimId,
            int victimBlood,
            int shooterKillNum = -1,
            int victimBeKillNum = -1)
        {
            var sb = new StringBuilder(160);
            sb.Append("{\"type\":\"").Append(type).Append("\",");
            sb.Append("\"shooter\":{\"id\":").Append(shooterId);
            if (shooterKillNum >= 0)
                sb.Append(",\"killNum\":").Append(shooterKillNum);
            sb.Append("},\"victim\":{\"id\":").Append(victimId);
            sb.Append(",\"blood\":").Append(victimBlood);
            if (victimBeKillNum >= 0)
                sb.Append(",\"beKillNum\":").Append(victimBeKillNum);
            sb.Append("}}");
            return sb.ToString();
        }

        static string BuildMultiKill(int shooterId, int combo)
        {
            return "{\"comboCount\":" + combo + ",\"shooter\":{\"id\":" + shooterId + "}}";
        }

        static void Fire(string json) => ScoreCardMockMqtt.Fire(json, LogTag);
    }
}
