using System;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Character
{
    /// <summary>
    /// 客户端位置同步。服务器接入后，在主线程把玩家快照交给 Push 或 PushAll。
    /// Push 更新单个玩家；PushAll 用整份名单替换，名单里没有的玩家会立刻移除。
    /// 超过 removeAfterSeconds 没有任何更新的玩家也会移除。
    /// 位置按 interpolationDelay 回放，血量和射击用最新一包，避免血条跟着位置一起滞后。
    /// </summary>
    [DisallowMultipleComponent]
    public class VenuePlayerSync : MonoBehaviour
    {
        struct Sample
        {
            public double time;
            public Vector3 pos;
            public int hp;
            public int maxHp;
            public int team;
            public string name;
            public bool firing;
        }

        sealed class Track
        {
            public VenuePlayerView view;
            public readonly List<Sample> samples = new List<Sample>(8);
            public double lastPush;
            public double presentedTime;
            public bool started;
            public Vector3 resolved;
            public bool hasResolved;
            public double blockedSince;
            public Quaternion facing;
            public bool hasFacing;
        }

        public static VenuePlayerSync Current { get; private set; }

        [Tooltip("场馆坐标锚点。留空则用同一个物体上的 VenueSpace，再没有就按场景坐标直接摆。")]
        public VenueSpace space;

        [Tooltip("玩家模型。留空时用胶囊代替，方便美术场景还没好时先看同步。")]
        public GameObject modelPrefab;

        [Tooltip("回放比最新数据包晚多少秒。要大于发送间隔，两包之间才有缓冲可插值。")]
        public float interpolationDelay = 0.2f;

        [Tooltip("多久没收到这个玩家就从场景里拿走。")]
        public float removeAfterSeconds = 2f;

        [Tooltip("多少米每秒对应角色动画里的走步。跑步大约是这个值的两倍。")]
        public float gaitReferenceSpeed = 1.8f;

        [Tooltip("模型脸朝向和 +Z 不一致时补这个偏航角。")]
        public float modelYawOffset;

        [Tooltip("血条离脚底的高度。按模型身高调。")]
        public float statusHeight = 2.15f;

        public bool snapToGround = true;

        [Tooltip("挡墙用的胶囊半径。墙要在 Obstacles 层。要比门宽的一半小，否则会卡在门框上。")]
        public float bodyRadius = 0.25f;

        [Tooltip("挡墙用的胶囊高度。")]
        public float bodyHeight = 1.8f;

        [Tooltip("定位点陷进墙里时，沿墙面最多推出这么远。再远就认为人在墙的另一侧，这一帧留在墙外。")]
        public float maxWallPush = 0.5f;

        [Tooltip("服务器位置一直在墙的另一侧、并且落在空地上，超过这么久就跟过去。用来放行美术多做的墙。")]
        public float wallReleaseDelay = 1f;

        readonly Dictionary<int, Track> _tracks = new Dictionary<int, Track>();
        readonly HashSet<int> _seen = new HashSet<int>();
        readonly List<int> _remove = new List<int>(8);
        readonly RaycastHit[] _hits = new RaycastHit[16];
        readonly Collider[] _overlaps = new Collider[8];
        CapsuleCollider _probe;

        Transform _root;
        bool _hasClock;
        double _serverToLocal;
        bool _clockJump;

        float worldUiScale = 1;

        void Awake()
        {
            if (space == null)
                space = GetComponent<VenueSpace>();

            if (UIRoot.S && UIRoot.S.worldCanvas)
            {
                this.transform.SetParent(UIRoot.S.worldCanvas.transform);
                var virtualCamera = GameObject.Find("/SceneBase/Virtual Camera");
                Canvas rootCanvas = UIRoot.S.rootCanvas;
                if (virtualCamera != null && rootCanvas != null)
                {
                    float dis = (transform.position - virtualCamera.transform.position).magnitude;
                    worldUiScale = dis / rootCanvas.planeDistance * rootCanvas.transform.localScale.x;
                }
            }
        }

        void OnEnable()
        {
            Current = this;
            if (_root == null)
            {
                var players = new GameObject("Players");
                players.transform.SetParent(transform, false);
                _root = players.transform;
            }
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            DespawnAll();
            if (_probe != null)
            {
                Destroy(_probe.gameObject);
                _probe = null;
            }
        }

        public void Push(VenuePlayerSnapshot snapshot)
        {
            if (snapshot == null)
                return;
            _clockJump = false;
            Accept(snapshot);
            if (!_clockJump)
                return;
            _clockJump = false;
            ClearBuffers();
            Accept(snapshot);
        }

        public void PushAll(IReadOnlyList<VenuePlayerSnapshot> snapshots)
        {
            if (snapshots == null)
                return;

            _seen.Clear();
            _clockJump = false;
            for (int i = 0; i < snapshots.Count; i++)
            {
                VenuePlayerSnapshot snapshot = snapshots[i];
                if (snapshot == null)
                    continue;
                _seen.Add(snapshot.id);
                Accept(snapshot);
            }

            if (_clockJump)
            {
                _clockJump = false;
                ClearBuffers();
                for (int i = 0; i < snapshots.Count; i++)
                {
                    if (snapshots[i] != null)
                        Accept(snapshots[i]);
                }
            }

            _remove.Clear();
            foreach (KeyValuePair<int, Track> pair in _tracks)
            {
                if (!_seen.Contains(pair.Key))
                    _remove.Add(pair.Key);
            }
            for (int i = 0; i < _remove.Count; i++)
                Despawn(_remove[i]);
        }

        void LateUpdate()
        {
            double now = Time.timeAsDouble;
            _remove.Clear();
            foreach (KeyValuePair<int, Track> pair in _tracks)
            {
                if (now - pair.Value.lastPush > removeAfterSeconds)
                    _remove.Add(pair.Key);
                else
                    Present(pair.Value);
            }
            for (int i = 0; i < _remove.Count; i++)
                Despawn(_remove[i]);
        }

        void Accept(VenuePlayerSnapshot snapshot)
        {
            double localTime = ToLocal(snapshot.time);
            if (_clockJump)
                return;

            Track track = GetOrCreate(snapshot.id);
            track.lastPush = Time.timeAsDouble;
            track.view.SetIdentity(snapshot.name, snapshot.team);
            AddSample(track, new Sample
            {
                time = localTime,
                pos = new Vector3(snapshot.x, snapshot.y, snapshot.z),
                hp = snapshot.hp,
                maxHp = snapshot.maxHp,
                team = snapshot.team,
                name = snapshot.name,
                firing = snapshot.firing
            });
        }

        double ToLocal(double serverTime)
        {
            double now = Time.timeAsDouble;
            if (serverTime <= 0d)
                return now;

            double observed = now - serverTime;
            if (!_hasClock)
            {
                _hasClock = true;
                _serverToLocal = observed;
            }
            else if (Math.Abs(observed - _serverToLocal) > 2d)
            {
                _serverToLocal = observed;
                _clockJump = true;
            }
            return serverTime + _serverToLocal;
        }

        Track GetOrCreate(int id)
        {
            if (_tracks.TryGetValue(id, out Track track))
                return track;

            var go = modelPrefab != null ? Instantiate(modelPrefab, _root) : CreatePlaceholder();
            go.transform.SetParent(_root, false);
            VenuePlayerView view = go.GetComponent<VenuePlayerView>();
            if (view == null)
                view = go.AddComponent<VenuePlayerView>();
            Renderer tint = modelPrefab == null ? go.GetComponentInChildren<Renderer>() : null;
            view.Bind(id, tint, gaitReferenceSpeed, statusHeight, worldUiScale);

            track = new Track { view = view };
            _tracks.Add(id, track);
            return track;
        }

        GameObject CreatePlaceholder()
        {
            var root = new GameObject("Player");
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            Collider collider = body.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            return root;
        }

        void AddSample(Track track, Sample sample)
        {
            List<Sample> list = track.samples;
            int count = list.Count;
            if (count > 0 && sample.time < list[count - 1].time - 0.0001d)
                return;

            // 写入前就把点推到墙外，并存成世界坐标。之后插值直接用这些点。
            bool replaceLast = count > 0 && sample.time - list[count - 1].time < 0.0001d;
            if (replaceLast && count >= 2)
            {
                track.resolved = list[count - 2].pos;
                track.hasResolved = true;
            }
            else if (!replaceLast && count >= 1)
            {
                track.resolved = list[count - 1].pos;
                track.hasResolved = true;
            }

            Vector3 world = ToWorld(sample.pos);
            if (snapToGround)
                world = SnapGround(world, track.view.transform);
            sample.pos = ResolveAgainstWalls(world, track);

            if (replaceLast)
                list[count - 1] = sample;
            else
                list.Add(sample);

            double keep = sample.time - 1d;
            int drop = 0;
            while (drop < list.Count - 2 && list[drop + 1].time < keep)
                drop++;
            if (drop > 0)
                list.RemoveRange(0, drop);
            if (list.Count > 32)
                list.RemoveRange(0, list.Count - 32);
        }

        void Present(Track track)
        {
            List<Sample> samples = track.samples;
            if (samples.Count == 0)
                return;

            int last = samples.Count - 1;
            // 回放时间跟着本地时钟走。若钉在「最新一包的时间」上，两包之间位置不变，下一包到了才跳一截。
            double renderTime = samples.Count == 1
                ? samples[0].time
                : Time.timeAsDouble - Math.Max(0d, interpolationDelay);
            if (renderTime > samples[last].time)
                renderTime = samples[last].time;
            if (renderTime < samples[0].time)
                renderTime = samples[0].time;

            Vector3 world = Evaluate(samples, renderTime);
            if (snapToGround)
                world = SnapGround(world, track.view.transform);
            Quaternion rot = Facing(track, samples, renderTime, world);

            Sample newest = samples[last];
            track.view.Show(world, rot, newest.hp, newest.maxHp);

            if (!track.started)
                track.presentedTime = renderTime - 0.0001d;
            if (newest.hp > 0)
            {
                for (int i = 0; i < samples.Count; i++)
                {
                    Sample sample = samples[i];
                    if (sample.firing && sample.time > track.presentedTime && sample.time <= renderTime)
                    {
                        track.view.Fire();
                        break;
                    }
                }
            }

            track.presentedTime = renderTime;
            track.started = true;
        }

        static Vector3 Evaluate(List<Sample> samples, double time)
        {
            if (time <= samples[0].time)
                return samples[0].pos;

            int last = samples.Count - 1;
            if (time >= samples[last].time)
                return samples[last].pos;

            int index = 0;
            while (index < last - 1 && samples[index + 1].time < time)
                index++;
            Sample a = samples[index];
            Sample b = samples[index + 1];
            double span = b.time - a.time;
            float t = span > 0.00001d ? (float)((time - a.time) / span) : 1f;
            return Vector3.Lerp(a.pos, b.pos, t);
        }

        const double FaceWindow = 0.25d;
        const float FaceMinDistance = 0.08f;

        Quaternion Facing(Track track, List<Sample> samples, double renderTime, Vector3 worldPos)
        {
            // 没有服务器朝向。看这一小段水平位移；太短就保持上次，免得定位抖动把人转圈。
            Vector3 from = Evaluate(samples, renderTime - FaceWindow);
            Vector3 flat = worldPos - from;
            flat.y = 0f;
            if (flat.sqrMagnitude >= FaceMinDistance * FaceMinDistance)
            {
                Quaternion want = Quaternion.LookRotation(flat, Vector3.up);
                track.facing = track.hasFacing
                    ? Quaternion.Slerp(track.facing, want, 1f - Mathf.Exp(-10f * Time.deltaTime))
                    : want;
                track.hasFacing = true;
            }

            Quaternion rot = track.hasFacing ? track.facing : DefaultFacing();
            if (modelYawOffset != 0f)
                rot *= Quaternion.Euler(0f, modelYawOffset, 0f);
            return rot;
        }

        Quaternion DefaultFacing()
        {
            Vector3 forward = ToWorld(Vector3.forward) - ToWorld(Vector3.zero);
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                return Quaternion.identity;
            return Quaternion.LookRotation(forward, Vector3.up);
        }

        Vector3 SnapGround(Vector3 world, Transform ignore)
        {
            Vector3 origin = world + Vector3.up * 4f;
            int count = Physics.RaycastNonAlloc(origin, Vector3.down, _hits, 8f, 1 << Layer.MAP, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            bool found = false;
            Vector3 point = world;
            for (int i = 0; i < count; i++)
            {
                Transform hit = _hits[i].collider.transform;
                if (hit == ignore || hit.IsChildOf(ignore))
                    continue;
                if (_hits[i].distance >= best)
                    continue;
                best = _hits[i].distance;
                point = _hits[i].point;
                found = true;
            }
            if (found)
                world.y = point.y;
            return world;
        }

        Vector3 ToWorld(Vector3 venue)
        {
            return space != null ? space.ToWorld(venue) : venue;
        }

        const float WallSkin = 0.05f;
        const int WallMask = 1 << Layer.Obstacles;

        Vector3 ResolveAgainstWalls(Vector3 desired, Track track)
        {
            Vector3 fallback = track.hasResolved ? track.resolved : desired;
            Vector3 candidate = desired;
            if (track.hasResolved)
            {
                Vector3 flat = desired - track.resolved;
                flat.y = 0f;
                float distance = flat.magnitude;
                if (distance > 0.0001f && CapsuleHitsWall(track.resolved, flat / distance, distance, out float travel))
                {
                    if (IsClearOfWalls(desired) && TryReleaseBlockedWall(track))
                    {
                        track.blockedSince = 0d;
                        track.resolved = desired;
                        track.hasResolved = true;
                        return desired;
                    }

                    candidate = track.resolved + (flat / distance) * travel;
                    candidate.y = desired.y;
                }
                else
                    track.blockedSince = 0d;
            }

            candidate = PushOutOfWalls(candidate, fallback, out bool rejected);
            if (rejected && track.hasResolved)
                candidate = track.resolved;
            candidate.y = desired.y;
            if (rejected && !track.hasResolved)
                return candidate;

            track.resolved = candidate;
            track.hasResolved = true;
            return candidate;
        }

        bool CapsuleHitsWall(Vector3 feet, Vector3 direction, float distance, out float travel)
        {
            CapsuleEnds(feet, out Vector3 bottom, out Vector3 top);
            // 起点已经在墙里时 CapsuleCast 会返回 false，路径会被当成没撞墙，人就穿过去。
            if (CapsuleOverlapsWall(bottom, top))
            {
                travel = 0f;
                return true;
            }

            if (Physics.CapsuleCast(bottom, top, bodyRadius, direction, out RaycastHit hit, distance, WallMask, QueryTriggerInteraction.Ignore))
            {
                travel = Mathf.Max(0f, hit.distance - WallSkin);
                return true;
            }

            travel = distance;
            return false;
        }

        bool CapsuleOverlapsWall(Vector3 bottom, Vector3 top)
        {
            int count = Physics.OverlapCapsuleNonAlloc(bottom, top, bodyRadius, _overlaps, WallMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (_overlaps[i] != null)
                    return true;
            }

            return false;
        }

        bool TryReleaseBlockedWall(Track track)
        {
            double now = Time.timeAsDouble;
            if (track.blockedSince <= 0d)
            {
                track.blockedSince = now;
                return false;
            }

            return now - track.blockedSince >= wallReleaseDelay;
        }

        bool IsClearOfWalls(Vector3 feet)
        {
            CapsuleEnds(feet, out Vector3 bottom, out Vector3 top);
            return !CapsuleOverlapsWall(bottom, top);
        }

        Vector3 PushOutOfWalls(Vector3 feet, Vector3 fallback, out bool rejected)
        {
            rejected = false;
            PrepareProbe();

            Vector3 pushed = feet;
            for (int pass = 0; pass < 2; pass++)
            {
                CapsuleEnds(pushed, out Vector3 bottom, out Vector3 top);
                int count = Physics.OverlapCapsuleNonAlloc(bottom, top, bodyRadius, _overlaps, WallMask, QueryTriggerInteraction.Ignore);
                bool moved = false;
                for (int i = 0; i < count; i++)
                {
                    Collider other = _overlaps[i];
                    if (other == null)
                        continue;
                    if (!Physics.ComputePenetration(_probe, pushed, Quaternion.identity, other, other.transform.position, other.transform.rotation, out Vector3 dir, out float dist))
                        continue;
                    dir.y = 0f;
                    if (dir.sqrMagnitude < 0.0001f)
                        continue;
                    Vector3 extra = pushed - feet;
                    extra.y = 0f;
                    if (extra.magnitude + dist > maxWallPush)
                    {
                        rejected = true;
                        return fallback;
                    }
                    pushed += dir.normalized * (dist + 0.01f);
                    moved = true;
                }
                if (!moved)
                    break;
            }

            return pushed;
        }

        void CapsuleEnds(Vector3 feet, out Vector3 bottom, out Vector3 top)
        {
            float height = Mathf.Max(bodyHeight, bodyRadius * 2f);
            bottom = feet + Vector3.up * bodyRadius;
            top = feet + Vector3.up * (height - bodyRadius);
        }

        void PrepareProbe()
        {
            EnsureProbe();
            CapsuleEnds(Vector3.zero, out Vector3 bottom, out Vector3 top);
            float height = Mathf.Max((top.y - bottom.y) + bodyRadius * 2f, bodyRadius * 2f);
            _probe.radius = bodyRadius;
            _probe.height = height;
            _probe.center = new Vector3(0f, (bottom.y + top.y) * 0.5f, 0f);
        }

        void EnsureProbe()
        {
            if (_probe != null)
                return;
            var go = new GameObject("WallProbe");
            go.layer = Layer.IgnoreRaycast;
            _probe = go.AddComponent<CapsuleCollider>();
            _probe.direction = 1;
            // 只拿来做 ComputePenetration。不勾选的话，这个胶囊会留在原点挡到场景里的物理。
            _probe.isTrigger = true;
        }

        void ClearBuffers()
        {
            foreach (Track track in _tracks.Values)
            {
                track.samples.Clear();
                track.started = false;
                track.blockedSince = 0d;
                track.hasResolved = false;
            }
        }

        void Despawn(int id)
        {
            if (!_tracks.TryGetValue(id, out Track track))
                return;
            _tracks.Remove(id);
            if (track.view != null)
                Destroy(track.view.gameObject);
        }

        /// <summary>
        /// 清空场上全部玩家（切换数据源/Mock 人数时调用，避免残留）。
        /// </summary>
        public void ClearAllPlayers()
        {
            DespawnAll();
            _hasClock = false;
            _serverToLocal = 0;
            _clockJump = false;
        }

        void DespawnAll()
        {
            foreach (Track track in _tracks.Values)
            {
                if (track.view != null)
                    Destroy(track.view.gameObject);
            }
            _tracks.Clear();
        }
    }
}
