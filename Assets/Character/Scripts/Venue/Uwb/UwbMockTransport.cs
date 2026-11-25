using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// Mock 干净版：每人一个目标；到达后在 ≤10m 的对角方向再随机下一个目标，A* 过去。
    /// </summary>
    public sealed class UwbMockTransport : IUwbTransport
    {
        const float GoalMinDistM = 2f;
        const float GoalMaxDistM = 10f;
        const float PathMaxLenM = 16f;
        const float StartStaggerMaxSec = 6f;
        const float DwellMinSec = 0.5f;
        const float DwellMaxSec = 3f;
        const int GoalCandidateCount = 6;
        struct Agent
        {
            public string uid;
            public string deviceName;
            public Vector3 pos;
            public Vector3 vel;
            public List<Vector2> path;
            public int pathIndex;
            public float speed;
            public int salt;
            public float pause;

            // 对外输出点（真值 pos 之上叠加 UWB 故障），不影响寻路
            public Vector3 emit;

            public float cornerCd;
            public bool cutActive;
            public Vector2 cutStart;
            public Vector2 cutEnd;
            public float cutArc;
            public float cutDone;

            public float jumpCd;
            public float jumpTimer;
            public Vector2 jumpOffset;
        }

        // 穿墙角：路线拐弯处输出点走弦线抄近路，削掉直角
        const float CornerLookNearM = 1.2f;
        const float CornerLookFarM = 2.6f;
        const float CornerMinAngleDeg = 55f;
        const float CornerCooldownSec = 3f;
        const float CornerCutProb = 0.45f;

        // 穿墙：贴墙行走时输出点瞬间跳到墙另一侧，停留片刻后跳回
        const float JumpProbeM = 0.5f;
        const float JumpMinM = 0.6f;
        const float JumpMaxM = 1.0f;
        const float JumpHoldMinSec = 0.6f;
        const float JumpHoldMaxSec = 2.0f;
        const float JumpCdMinSec = 8f;
        const float JumpCdMaxSec = 25f;

        readonly bool _simulateGlitch;
        int _cornerCuts;
        int _wallJumps;

        readonly List<Vector2> _candBuf = new List<Vector2>(8);

        readonly int _tagCount;
        readonly float _areaX;
        readonly float _areaY;
        readonly float _moveSpeed;
        readonly float _interval;
        readonly UwbVenueWalkGrid _grid;
        // 判定「是否真的穿墙」用的实体墙网格（小半径、细格），漫游走位用 _grid
        readonly UwbVenueWalkGrid _phys;
        readonly bool _usePath;
        Agent[] _agents;
        float _acc;
        long _cnt;
        bool _running;

        public bool IsRunning => _running;
        public event Action<string> RawMessageReceived;

        public UwbMockTransport(
            int tagCount,
            float areaX,
            float areaY,
            float moveSpeed,
            float intervalSec,
            UwbVenueWalkGrid walkGrid = null,
            bool simulateGlitch = true,
            UwbVenueWalkGrid physicalGrid = null)
        {
            _simulateGlitch = simulateGlitch;
            _phys = physicalGrid != null ? physicalGrid : walkGrid;
            _tagCount = Mathf.Max(1, tagCount);
            _areaX = Mathf.Max(1f, areaX);
            _areaY = Mathf.Max(1f, areaY);
            _moveSpeed = Mathf.Max(0.1f, moveSpeed);
            _interval = Mathf.Max(0.02f, intervalSec > 0f ? intervalSec : 0.1f);
            _grid = walkGrid;
            _usePath = walkGrid != null && walkGrid.Ready;
        }

        public void Start()
        {
            _agents = new Agent[_tagCount];
            for (int i = 0; i < _tagCount; i++)
            {
                Vector2 start = PickSpawn(i);
                _agents[i] = new Agent
                {
                    uid = "MOCK" + (i + 1).ToString("D4"),
                    deviceName = "T" + (i + 1),
                    pos = new Vector3(start.x, start.y, 1.2f),
                    vel = Vector3.zero,
                    path = new List<Vector2>(48),
                    pathIndex = 0,
                    speed = _moveSpeed * Random.Range(0.85f, 1.2f),
                    salt = 1000 + i * 97,
                    pause = Random.Range(0f, StartStaggerMaxSec),
                    emit = new Vector3(start.x, start.y, 1.2f),
                    jumpCd = Random.Range(JumpCdMinSec * 0.5f, JumpCdMaxSec)
                };
                PlanNewGoal(ref _agents[i]);
            }

            _acc = 0f;
            _cnt = 0;
            _running = true;
            Debug.Log(
                $"[UwbMock] start tags={_tagCount} hz={(1f / _interval):0.#} " +
                $"mode={(_usePath ? "A*-oneGoal" : "free-fallback")} " +
                $"walkable={(_grid != null ? _grid.WalkableCount : 0)} " +
                $"largest={(_grid != null ? _grid.LargestComponentSize : 0)}");
        }

        public void Stop()
        {
            if (_running && _simulateGlitch)
                Debug.Log($"[UwbMock] glitch summary cornerCuts={_cornerCuts} wallJumps={_wallJumps}");
            _running = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_running || _agents == null)
                return;

            if (_usePath)
                _grid.DecayGoalHeat(deltaTime);

            for (int i = 0; i < _agents.Length; i++)
            {
                Vector3 before = _agents[i].pos;
                Step(ref _agents[i], deltaTime);
                float moved = Vector2.Distance(
                    new Vector2(before.x, before.y),
                    new Vector2(_agents[i].pos.x, _agents[i].pos.y));
                UpdateEmit(ref _agents[i], moved, deltaTime);
            }

            _acc += deltaTime;
            if (_acc < _interval)
                return;
            // 扣掉一个间隔而不是清零：发包速率不被帧时间量化，和真实 UWB 的固定速率一致
            _acc -= _interval;
            if (_acc > _interval)
                _acc = 0f;

            long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int i = 0; i < _agents.Length; i++)
            {
                _cnt++;
                RawMessageReceived?.Invoke(BuildPosJson(_agents[i], _cnt, ms));
            }
        }

        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 在真值轨迹（a.pos，寻路结果）上叠加 UWB 故障得到输出点 a.emit；真值与寻路完全不受影响。
        /// </summary>
        void UpdateEmit(ref Agent a, float moved, float dt)
        {
            Vector2 truth = new Vector2(a.pos.x, a.pos.y);
            Vector2 e = truth;

            if (_simulateGlitch && _usePath)
            {
                // 穿墙角
                a.cornerCd -= dt;
                if (a.cutActive)
                {
                    a.cutDone += moved;
                    float t = a.cutDone / Mathf.Max(0.01f, a.cutArc);
                    if (t >= 1f)
                        a.cutActive = false;
                    else
                        e = Vector2.Lerp(a.cutStart, a.cutEnd, t);
                }
                else if (a.cornerCd <= 0f && moved > 0.0001f &&
                         TryDetectCorner(a, truth, out Vector2 exit, out float arc))
                {
                    a.cornerCd = CornerCooldownSec;
                    // 只有弦线真的穿过实体墙角才算穿角；拐的是开阔处的虚拟拐点，不构成错误
                    if (Random.value < CornerCutProb && !_phys.SegmentWalkable(truth, exit))
                    {
                        a.cutActive = true;
                        a.cutStart = truth;
                        a.cutEnd = exit;
                        a.cutArc = arc;
                        a.cutDone = 0f;
                        _cornerCuts++;
                    }
                }

                // 穿墙：瞬间跳到墙另一侧，停留后跳回
                if (a.jumpTimer > 0f)
                {
                    a.jumpTimer -= dt;
                    // 真值走开后偏移点不再隔着墙，就不算穿墙了，结束这次故障
                    if (_phys.SegmentWalkable(truth, truth + a.jumpOffset))
                        a.jumpTimer = 0f;
                    else
                        e += a.jumpOffset;
                    if (a.jumpTimer <= 0f)
                        a.jumpOffset = Vector2.zero;
                }
                else
                {
                    a.jumpCd -= dt;
                    if (a.jumpCd <= 0f && moved > 0.0001f)
                    {
                        Vector2 heading = new Vector2(a.vel.x, a.vel.y);
                        if (TryPickWallJump(truth, heading, out Vector2 jump))
                        {
                            a.jumpOffset = jump;
                            a.jumpTimer = Random.Range(JumpHoldMinSec, JumpHoldMaxSec);
                            a.jumpCd = Random.Range(JumpCdMinSec, JumpCdMaxSec);
                            e += a.jumpOffset;
                            _wallJumps++;
                        }
                        else
                            a.jumpCd = 0.3f;
                    }
                }
            }

            a.emit = new Vector3(e.x, e.y, a.pos.z);
        }

        /// <summary>
        /// 取一个真正越过实体墙的偏移：朝墙的一侧，幅度从随机起点逐步加大，
        /// 直到 真值→偏移点 的直线被墙挡住（偏移点落在墙另一侧或墙里）。
        /// </summary>
        bool TryPickWallJump(Vector2 truth, Vector2 heading, out Vector2 offset)
        {
            offset = default;
            if (!_phys.TryWallSideNormal(truth, heading, JumpProbeM, out Vector2 n))
                return false;

            float m = Random.Range(JumpMinM, JumpMaxM);
            for (float mag = m; mag <= JumpMaxM + 0.3f; mag += 0.1f)
            {
                Vector2 target = truth + n * mag;
                if (_phys.SegmentWalkable(truth, target))
                    continue;
                offset = n * mag;
                return true;
            }

            return false;
        }

        bool TryDetectCorner(Agent a, Vector2 truth, out Vector2 exit, out float arc)
        {
            exit = default;
            arc = 0f;
            if (a.path == null || a.pathIndex >= a.path.Count)
                return false;

            Vector2 pA = AheadPoint(a, truth, CornerLookNearM, out float arcA);
            Vector2 pC = AheadPoint(a, truth, CornerLookFarM, out float arcC);
            if (arcC - arcA < 0.6f)
                return false;

            Vector2 h1 = pA - truth;
            Vector2 h2 = pC - pA;
            if (h1.magnitude < 0.3f || h2.magnitude < 0.3f)
                return false;
            if (Vector2.Angle(h1, h2) < CornerMinAngleDeg)
                return false;

            exit = pC;
            arc = arcC;
            return true;
        }

        static Vector2 AheadPoint(Agent a, Vector2 from, float dist, out float walked)
        {
            Vector2 cur = from;
            float left = dist;
            walked = 0f;
            for (int i = a.pathIndex; i < a.path.Count; i++)
            {
                Vector2 wp = a.path[i];
                float seg = Vector2.Distance(cur, wp);
                if (seg >= left)
                {
                    walked += left;
                    return seg > 1e-5f ? cur + (wp - cur) / seg * left : cur;
                }

                left -= seg;
                walked += seg;
                cur = wp;
            }

            return cur;
        }

        void Step(ref Agent a, float dt)
        {
            if (_usePath)
                StepAlongPath(ref a, dt);
            else
                StepFree(ref a, dt);
            a.pos.z = 1.2f;
        }

        void StepAlongPath(ref Agent a, float dt)
        {
            if (a.pause > 0f)
            {
                a.pause -= dt;
                a.vel = Vector3.zero;
                return;
            }

            if (a.path == null || a.path.Count == 0 || a.pathIndex >= a.path.Count)
            {
                PlanNewGoal(ref a);
                if (a.path == null || a.path.Count == 0)
                {
                    a.vel = Vector3.zero;
                    return;
                }
            }

            Vector2 wp = a.path[a.pathIndex];
            Vector3 target = new Vector3(wp.x, wp.y, a.pos.z);
            Vector3 to = target - a.pos;
            to.z = 0f;
            float dist = to.magnitude;

            if (dist < 0.2f)
            {
                a.pathIndex++;
                if (a.pathIndex >= a.path.Count)
                {
                    // 到达当前目标 → 停留一会儿，再随机下一个
                    a.vel = Vector3.zero;
                    a.pause = Random.Range(DwellMinSec, DwellMaxSec);
                    PlanNewGoal(ref a);
                    return;
                }

                wp = a.path[a.pathIndex];
                target = new Vector3(wp.x, wp.y, a.pos.z);
                to = target - a.pos;
                to.z = 0f;
                dist = to.magnitude;
            }

            if (dist < 0.0001f)
            {
                a.vel = Vector3.zero;
                return;
            }

            Vector3 dir = to / dist;
            float step = a.speed * dt;
            if (step >= dist)
            {
                a.pos = target;
                a.vel = dir * a.speed;
                a.pathIndex++;
            }
            else
            {
                a.vel = dir * a.speed;
                a.pos += a.vel * dt;
            }
        }

        void StepFree(ref Agent a, float dt)
        {
            if (a.path == null)
                a.path = new List<Vector2>(4);
            if (a.path.Count == 0)
            {
                a.path.Add(RandomRectPoint());
                a.pathIndex = 0;
            }

            Vector2 wp = a.path[a.path.Count - 1];
            Vector3 target = new Vector3(wp.x, wp.y, a.pos.z);
            Vector3 to = target - a.pos;
            to.z = 0f;
            if (to.magnitude < 0.5f)
            {
                a.path.Clear();
                a.path.Add(RandomRectPoint());
                a.pathIndex = 0;
                return;
            }

            Vector3 dir = to.normalized;
            a.vel = dir * a.speed;
            a.pos += a.vel * dt;
            ClampInArea(ref a.pos);
        }

        /// <summary>
        /// 下一个目标：距离 2~10m，且相对当前位置为对角方向，再 A* 过去。
        /// </summary>
        void PlanNewGoal(ref Agent a)
        {
            a.path.Clear();
            a.pathIndex = 0;
            if (!_usePath)
                return;

            Vector2 from = new Vector2(a.pos.x, a.pos.y);
            if (!_grid.IsWalkableVenue(from.x, from.y))
            {
                if (_grid.TryRandomUniformInLargestComponent(out Vector2 snap))
                {
                    a.pos.x = snap.x;
                    a.pos.y = snap.y;
                    from = snap;
                }
                else
                    return;
            }

            int n = _grid.PickGoalCandidates(from, GoalMinDistM, GoalMaxDistM, GoalCandidateCount, _candBuf);
            // 每段路用新的 salt，同一对起终点也会走不同走廊，避免固定一条蚁路
            int legSalt = Random.Range(1, int.MaxValue);
            for (int i = 0; i < n; i++)
            {
                Vector2 goal = _candBuf[i];
                if (!_grid.TryFindPath(from, goal, a.path, legSalt) || a.path.Count < 2)
                {
                    a.path.Clear();
                    continue;
                }

                // 绕路过长（要绕大半圈才到）的目标直接放弃，避免全员挤在外圈走廊
                if (PathLength(a.path) > PathMaxLenM)
                {
                    a.path.Clear();
                    continue;
                }

                // 拉直：去掉 A* 格点阶梯，走出自然的直线 + 绕墙角
                _grid.SimplifyPath(a.path);

                _grid.AddGoalHeat(goal);
                return;
            }

            a.path.Clear();
            // 候选全不合格：停一小会儿，下一帧重新抽
            a.pause = Mathf.Max(a.pause, 0.5f);
        }

        static float PathLength(List<Vector2> path)
        {
            float len = 0f;
            for (int i = 1; i < path.Count; i++)
                len += Vector2.Distance(path[i - 1], path[i]);
            return len;
        }

        Vector2 PickSpawn(int index)
        {
            // 出生仍按扇区稍作分散，之后目标完全随机
            if (_usePath &&
                _grid.TrySpawnInSector(index, Mathf.Max(9, _tagCount), out Vector2 p, 0f, 0f))
                return p;
            if (_usePath && _grid.TryRandomInLargestComponent(out p, 0f, 0f))
                return p;
            return RandomRectPoint();
        }

        Vector2 RandomRectPoint()
        {
            float hx = Mathf.Max(0.5f, _areaX * 0.5f - 0.5f);
            float hy = Mathf.Max(0.5f, _areaY * 0.5f - 0.5f);
            return new Vector2(Random.Range(-hx, hx), Random.Range(-hy, hy));
        }

        void ClampInArea(ref Vector3 p)
        {
            float hx = _areaX * 0.5f;
            float hy = _areaY * 0.5f;
            p.x = Mathf.Clamp(p.x, -hx, hx);
            p.y = Mathf.Clamp(p.y, -hy, hy);
        }

        static string BuildPosJson(Agent a, long cnt, long ms)
        {
            var sb = new StringBuilder(256);
            sb.Append("{\"data\":{");
            sb.Append("\"cnt\":").Append(cnt).Append(',');
            sb.Append("\"mapId\":1,");
            sb.Append("\"noiseRadius\":0,");
            sb.Append("\"pos\":[")
                .Append(F(a.emit.x)).Append(',')
                .Append(F(a.emit.y)).Append(',')
                .Append(F(a.emit.z)).Append("],");
            sb.Append("\"vel\":[")
                .Append(F(a.vel.x)).Append(',')
                .Append(F(a.vel.y)).Append(',')
                .Append(F(a.vel.z)).Append("],");
            sb.Append("\"time\":").Append(ms).Append(',');
            sb.Append("\"z\":").Append(F(a.emit.z));
            sb.Append("},\"deviceName\":\"").Append(a.deviceName).Append("\",");
            sb.Append("\"name\":\"Pos\",");
            sb.Append("\"uid\":\"").Append(a.uid).Append("\"}");
            return sb.ToString();
        }

        static string F(float v)
        {
            return v.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
