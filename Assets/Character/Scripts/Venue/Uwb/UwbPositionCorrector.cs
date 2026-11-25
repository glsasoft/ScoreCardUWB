using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 渲染前的位置纠偏：定长滞后窗口里做地图匹配。
    /// 窗口内原始点干净（可走、相邻点直线可走、步长合理）时原样输出，不引入任何抖动；
    /// 出现穿墙跳变/穿墙角时，才在可走候选点上做 Viterbi：相邻帧候选必须直线可走，
    /// 代价 = 截断的离原始点距离 + 步长 + 加速度（二阶差分），所以输出沿原前进方向平滑，
    /// 不会跟着墙对面的离群点跑，也不会削墙角。
    /// 只依赖 UwbVenueWalkGrid 的可走性，不含地图专用参数。
    /// </summary>
    public sealed class UwbPositionCorrector
    {
        const int LatticeHalf = 4;
        const int PredictHalf = 2;
        const int MaxCandidates =
            (LatticeHalf * 2 + 1) * (LatticeHalf * 2 + 1) + (PredictHalf * 2 + 1) * (PredictHalf * 2 + 1);
        const float CandidateRadiusM = 1.2f;
        const float PredictRadiusM = 0.6f;
        const float FallbackRadiusM = 2.4f;
        const float EmissionSigmaM = 0.35f;
        const float EmissionWeight = 1f / (2f * EmissionSigmaM * EmissionSigmaM);
        const float EmissionCapM = 0.4f;
        const float StepWeight = 0.3f;
        const float SpeedPenaltyWeight = 40f;
        const float AccelWeight = 25f;
        const float MaxSpeedMps = 3.5f;
        const float StepSlackM = 0.1f;
        const float ResetGapSec = 3f;
        const float OffsetTauSec = 1.2f;
        const float MaxOutputSpeedMps = 2f;
        const float ChaseSpeedMps = 1.9f;
        const float ChaseDoneM = 0.15f;
        const int CleanHorizonFrames = 3;
        const float ReacquireDistM = 1.2f;
        const int ReacquireFrames = 4;
        const float MaxVelMps = 2.8f;
        // 每个玩家每包都会更新一次，增益要小：单次掉帧不能让全场的回放延迟跟着跳
        const float AvgDtGain = 0.003f;

        sealed class Frame
        {
            public double time;
            public Vector2 raw;
            public float height;
            public float yaw;
            public bool clean;
            public int count;
            public readonly Vector2[] pos = new Vector2[MaxCandidates];
            public readonly float[] emit = new float[MaxCandidates];
            public readonly float[] cost = new float[MaxCandidates];
            public readonly int[] back = new int[MaxCandidates];
        }

        sealed class Track
        {
            public readonly List<Frame> window = new List<Frame>(8);
            public readonly Stack<Frame> pool = new Stack<Frame>(8);
            public bool hasAnchor;
            public Vector2 anchor;
            public bool hasAnchor2;
            public Vector2 anchor2;
            public double anchorTime;
            public bool hasVel;
            public Vector2 vel;
            public int farFrames;
            public Vector2 offset;
            public bool chasing;
            public Vector2 chasePos;
            public readonly List<Vector2> chasePath = new List<Vector2>(64);
            public bool hasPrevRaw;
            public Vector2 prevRaw;
            public double prevRawTime;
            public double lastTime = double.NegativeInfinity;
        }

        readonly UwbVenueWalkGrid _grid;
        readonly int _lag;
        readonly Dictionary<int, Track> _tracks = new Dictionary<int, Track>(64);
        float _avgDt = 0.1f;

        public int Outputs { get; private set; }
        public int ViterbiRuns { get; private set; }
        public int Moved { get; private set; }
        public int Breaks { get; private set; }
        public int Reacquires { get; private set; }
        public float MaxDisplacement { get; private set; }
        public double SumDisplacement { get; private set; }

        public float LagSeconds => _lag * _avgDt;

        public UwbPositionCorrector(UwbVenueWalkGrid grid, int lagFrames)
        {
            _grid = grid;
            _lag = Mathf.Max(1, lagFrames);
        }

        public void ResetStats()
        {
            Outputs = 0;
            ViterbiRuns = 0;
            Moved = 0;
            Breaks = 0;
            Reacquires = 0;
            MaxDisplacement = 0f;
            SumDisplacement = 0d;
        }

        /// <summary>
        /// 送入一个原始点（场馆坐标，地面为 x、z）。窗口攒够 lagFrames 帧后，
        /// 每送入一帧就输出滞后 lagFrames 帧的纠偏结果（含该帧原始点 outRaw）；输出前返回 false。
        /// </summary>
        public bool Submit(
            int playerId,
            double time,
            Vector3 venuePos,
            float yaw,
            out Vector3 outPos,
            out float outYaw,
            out double outTime,
            out Vector3 outRaw)
        {
            outPos = default;
            outYaw = 0f;
            outTime = 0d;
            outRaw = default;

            if (!_tracks.TryGetValue(playerId, out Track t))
            {
                t = new Track();
                _tracks.Add(playerId, t);
            }

            double dt = time - t.lastTime;
            if (dt <= 0d || dt > ResetGapSec)
                ResetTrack(t);
            else
                _avgDt = Mathf.Lerp(_avgDt, Mathf.Clamp((float)dt, 0.02f, 0.5f), AvgDtGain);
            t.lastTime = time;

            Frame f = t.pool.Count > 0 ? t.pool.Pop() : new Frame();
            f.time = time;
            f.raw = new Vector2(venuePos.x, venuePos.z);
            f.height = venuePos.y;
            f.yaw = yaw;
            f.clean = IsRawClean(t, f);
            t.prevRaw = f.raw;
            t.prevRawTime = time;
            t.hasPrevRaw = true;
            t.window.Add(f);

            if (t.window.Count <= _lag)
                return false;

            Frame head = t.window[0];
            bool wasChasing = t.chasing;
            Vector2 p;
            if (wasChasing)
            {
                p = ChaseStep(t, head);
            }
            else if (!TryFastPath(t, head, out p))
            {
                ViterbiRuns++;
                int pick = Decode(t, out int pickNext);
                p = head.pos[pick];

                // 候选是 0.3m 的离散格点，直接输出会在直线上左右锯齿（S 形）。
                // 和前后路径点做 1-2-1 平均，抹掉格点量化；平滑后的点必须仍然可走、不穿墙。
                if (pickNext >= 0 && t.hasAnchor)
                {
                    Vector2 next = t.window[1].pos[pickNext];
                    Vector2 smooth = 0.5f * p + 0.25f * t.anchor + 0.25f * next;
                    if (_grid.IsWalkableVenue(smooth.x, smooth.y) && _grid.SegmentWalkable(t.anchor, smooth))
                        p = smooth;
                }
            }

            // 输出限速：任何来源（原始点跳变、glitch 结束回弹、追赶）造成的位移，每帧不超过 MaxOutputSpeedMps。
            // 只是把位移截短在 锚点→p 这条已验证可走的线段上，不会引入穿墙。
            if (t.hasAnchor)
            {
                float maxMove = MaxOutputSpeedMps * Mathf.Clamp((float)(head.time - t.anchorTime), 0.02f, 1f);
                Vector2 dp = p - t.anchor;
                float dm = dp.magnitude;
                if (dm > maxMove)
                    p = t.anchor + dp / dm * maxMove;
            }

            float disp = Vector2.Distance(p, head.raw);
            Outputs++;
            SumDisplacement += disp;
            if (disp > 0.3f)
                Moved++;
            if (disp > MaxDisplacement)
                MaxDisplacement = disp;

            outPos = new Vector3(p.x, head.height, p.y);
            outRaw = new Vector3(head.raw.x, head.height, head.raw.y);
            outYaw = head.yaw;
            outTime = head.time;

            if (t.hasAnchor)
            {
                double adt = head.time - t.anchorTime;
                if (adt > 0.001d)
                {
                    Vector2 v = (p - t.anchor) / (float)adt;
                    t.vel = t.hasVel ? Vector2.Lerp(t.vel, v, 0.5f) : v;
                    t.hasVel = true;
                }

                t.hasAnchor2 = true;
                t.anchor2 = t.anchor;
            }

            // 输出长时间偏离一直干净的原始点（超过 ReacquireDistM 连续 ReacquireFrames 帧）：
            // 说明匹配走进了错误的一侧，放弃锚点，下一帧从原始点重新起步
            if (!wasChasing && head.clean && disp > ReacquireDistM)
                t.farFrames++;
            else
                t.farFrames = 0;

            if (t.vel.sqrMagnitude > MaxVelMps * MaxVelMps)
                t.vel = t.vel.normalized * MaxVelMps;

            t.hasAnchor = true;
            t.anchor = p;
            t.anchorTime = head.time;
            t.offset = p - head.raw;
            if (wasChasing)
            {
                t.hasVel = false;
                t.hasAnchor2 = false;
            }

            // 匹配走进了错误的一侧：不瞬移到原始点，而是沿可走路径以略快于步行的速度绕回去
            if (t.farFrames >= ReacquireFrames)
            {
                t.farFrames = 0;
                t.chasing = true;
                t.chasePos = p;
                t.hasVel = false;
                t.hasAnchor2 = false;
                t.offset = Vector2.zero;
                Reacquires++;
            }

            t.window.RemoveAt(0);
            t.pool.Push(head);
            return true;
        }

        /// <summary>
        /// 追赶模式：从当前位置到最新的干净原始点求一条可走路径，按 ChaseSpeedMps 沿路径前进。
        /// 追上（距离很近）或求不出路径时结束；求不出路径才退化为直接对齐。
        /// </summary>
        Vector2 ChaseStep(Track t, Frame head)
        {
            float dtc = t.hasAnchor
                ? Mathf.Clamp((float)(head.time - t.anchorTime), 0.02f, 1f)
                : 0.1f;
            Vector2 cur = t.chasePos;
            Vector2 target = head.raw;

            if (!head.clean)
                return cur;

            if (!_grid.TryFindPath(cur, target, t.chasePath))
            {
                t.chasing = false;
                return target;
            }

            t.chasePath.Insert(0, cur);
            _grid.SimplifyPath(t.chasePath);

            float budget = ChaseSpeedMps * dtc;
            for (int i = 1; i < t.chasePath.Count && budget > 0f; i++)
            {
                Vector2 seg = t.chasePath[i] - cur;
                float len = seg.magnitude;
                if (len <= budget)
                {
                    budget -= len;
                    cur = t.chasePath[i];
                }
                else
                {
                    cur += seg / len * budget;
                    budget = 0f;
                }
            }

            if (Vector2.Distance(cur, target) < ChaseDoneM)
            {
                t.chasing = false;
                cur = target;
            }

            t.chasePos = cur;
            return cur;
        }

        void ResetTrack(Track t)
        {
            t.chasing = false;
            for (int i = 0; i < t.window.Count; i++)
                t.pool.Push(t.window[i]);
            t.window.Clear();
            t.hasAnchor = false;
            t.hasAnchor2 = false;
            t.hasVel = false;
            t.hasPrevRaw = false;
            t.offset = Vector2.zero;
        }

        bool IsRawClean(Track t, Frame f)
        {
            if (!_grid.IsWalkableVenue(f.raw.x, f.raw.y))
                return false;
            if (!t.hasPrevRaw)
                return true;
            float step = Vector2.Distance(t.prevRaw, f.raw);
            if (step > MaxStep(f.time - t.prevRawTime))
                return false;
            return _grid.SegmentWalkable(t.prevRaw, f.raw);
        }

        /// <summary>
        /// 窗口开头几帧都干净：直接输出原始点，叠加上一次纠偏留下的偏移（指数衰减回 0，避免回弹跳变）。
        /// </summary>
        bool TryFastPath(Track t, Frame head, out Vector2 p)
        {
            p = head.raw;
            int n = Mathf.Min(CleanHorizonFrames, t.window.Count);
            for (int i = 0; i < n; i++)
            {
                if (!t.window[i].clean)
                    return false;
            }

            double dt0 = t.hasAnchor ? head.time - t.anchorTime : 0d;
            Vector2 off = t.offset * Mathf.Exp(-Mathf.Max(0f, (float)dt0) / OffsetTauSec);
            if (off.sqrMagnitude < 0.0004f)
                off = Vector2.zero;

            p = head.raw + off;
            if (off != Vector2.zero && !_grid.IsWalkableVenue(p.x, p.y))
                return false;

            if (t.hasAnchor)
            {
                if (Vector2.Distance(t.anchor, p) > MaxStep(dt0))
                    return false;
                if (!_grid.SegmentWalkable(t.anchor, p))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 候选 = 原始点周围一圈 + 按上一段速度外推位置周围一小圈。
        /// 后者保证原始点被墙隔开/偏得很远时，仍有沿原前进方向的候选可选。
        /// </summary>
        void BuildCandidates(Track t, Frame f)
        {
            f.count = 0;
            AppendLattice(f, f.raw, CandidateRadiusM, LatticeHalf);
            if (t.hasAnchor && t.hasVel)
            {
                Vector2 pred = t.anchor + t.vel * (float)(f.time - t.anchorTime);
                AppendLattice(f, pred, PredictRadiusM, PredictHalf);
            }

            if (f.count == 0)
                AppendLattice(f, f.raw, FallbackRadiusM, LatticeHalf);
            if (f.count == 0)
            {
                f.pos[0] = f.raw;
                f.emit[0] = 0f;
                f.count = 1;
            }
        }

        void AppendLattice(Frame f, Vector2 center, float radius, int half)
        {
            float step = radius / half;
            float r2 = radius * radius * 1.0001f;
            for (int iz = -half; iz <= half; iz++)
            {
                for (int ix = -half; ix <= half; ix++)
                {
                    float dx = ix * step;
                    float dz = iz * step;
                    if (dx * dx + dz * dz > r2)
                        continue;
                    Vector2 p = new Vector2(center.x + dx, center.y + dz);
                    if (!_grid.IsWalkableVenue(p.x, p.y))
                        continue;
                    if (f.count >= MaxCandidates)
                        return;
                    f.pos[f.count] = p;
                    f.emit[f.count] = EmissionCost((p - f.raw).magnitude);
                    f.count++;
                }
            }
        }

        /// <summary>
        /// 在窗口内前向 DP，回溯得到窗口首帧应选的候选下标。
        /// 首帧以上一次输出点为锚；锚点连不上（真瞬移/丢失重连）就自由起步。
        /// 窗口中途出现无法连通的帧，就在它之前截断。
        /// </summary>
        int Decode(Track t, out int idxNext)
        {
            List<Frame> w = t.window;
            int n = w.Count;
            for (int i = 0; i < n; i++)
                BuildCandidates(t, w[i]);

            Frame f0 = w[0];
            bool anchored = false;
            if (t.hasAnchor)
            {
                float maxStep = MaxStep(f0.time - t.anchorTime);
                for (int i = 0; i < f0.count; i++)
                {
                    f0.cost[i] = float.PositiveInfinity;
                    f0.back[i] = -1;
                    Vector2 pi = f0.pos[i];
                    float d = Vector2.Distance(t.anchor, pi);
                    if (d > maxStep || !_grid.SegmentWalkable(t.anchor, pi))
                        continue;
                    float c = f0.emit[i] + StepWeight * d * d + SpeedPenalty(t, d, f0.time - t.anchorTime);
                    if (t.hasAnchor2)
                        c += AccelWeight * (pi - 2f * t.anchor + t.anchor2).sqrMagnitude;
                    f0.cost[i] = c;
                    anchored = true;
                }

                if (!anchored)
                    Breaks++;
            }

            if (!anchored)
            {
                for (int i = 0; i < f0.count; i++)
                {
                    f0.cost[i] = f0.emit[i];
                    f0.back[i] = -1;
                }
            }

            int end = n - 1;
            for (int fi = 1; fi < n; fi++)
            {
                Frame prev = w[fi - 1];
                Frame cur = w[fi];
                float maxStep = MaxStep(cur.time - prev.time);
                bool any = false;
                for (int j = 0; j < cur.count; j++)
                {
                    float best = float.PositiveInfinity;
                    int bestI = -1;
                    Vector2 pj = cur.pos[j];
                    for (int i = 0; i < prev.count; i++)
                    {
                        float pc = prev.cost[i];
                        if (pc >= best)
                            continue;
                        Vector2 pi = prev.pos[i];
                        float d = Vector2.Distance(pi, pj);
                        if (d > maxStep)
                            continue;
                        float c = pc + StepWeight * d * d + SpeedPenalty(t, d, cur.time - prev.time);

                        if (fi == 1)
                        {
                            if (anchored)
                                c += AccelWeight * (pj - 2f * pi + t.anchor).sqrMagnitude;
                        }
                        else
                        {
                            Vector2 pp = w[fi - 2].pos[prev.back[i]];
                            c += AccelWeight * (pj - 2f * pi + pp).sqrMagnitude;
                        }

                        if (c >= best)
                            continue;
                        if (!_grid.SegmentWalkable(pi, pj))
                            continue;
                        best = c;
                        bestI = i;
                    }

                    if (bestI >= 0)
                    {
                        cur.cost[j] = best + cur.emit[j];
                        cur.back[j] = bestI;
                        any = true;
                    }
                    else
                    {
                        cur.cost[j] = float.PositiveInfinity;
                        cur.back[j] = -1;
                    }
                }

                if (!any)
                {
                    end = fi - 1;
                    break;
                }
            }

            Frame last = w[end];
            int idx = 0;
            float bestCost = float.PositiveInfinity;
            for (int i = 0; i < last.count; i++)
            {
                if (last.cost[i] < bestCost)
                {
                    bestCost = last.cost[i];
                    idx = i;
                }
            }

            idxNext = -1;
            for (int fi = end; fi > 0; fi--)
            {
                if (fi == 1)
                    idxNext = idx;
                idx = w[fi].back[idx];
            }

            return idx;
        }

        /// <summary>
        /// 超过正常步速的那部分步长重罚：glitch 结束后输出平滑追回原始点，而不是一帧跳回去。
        /// 阈值随这个人自己的速度走（跑动不会被误罚），最低 0.2m/帧。
        /// </summary>
        static float SpeedPenalty(Track t, float d, double dt)
        {
            float v = t.hasVel ? t.vel.magnitude : 0f;
            float thr = Mathf.Max(0.2f, 1.4f * v * Mathf.Clamp((float)dt, 0.02f, 1f));
            float over = d - thr;
            return over > 0f ? SpeedPenaltyWeight * over * over : 0f;
        }

        /// <summary>
        /// Huber 型：小偏差按二次（普通噪声），超过 EmissionCapM 后线性增长，
        /// 离群点不会拽着路径跑，但路径也不能漂离原始点太远。
        /// </summary>
        static float EmissionCost(float d)
        {
            if (d <= EmissionCapM)
                return EmissionWeight * d * d;
            return EmissionWeight * (EmissionCapM * EmissionCapM + 2f * EmissionCapM * (d - EmissionCapM));
        }

        static float MaxStep(double dt)
        {
            return MaxSpeedMps * Mathf.Clamp((float)dt, 0.02f, 1f) + StepSlackM;
        }
    }
}
