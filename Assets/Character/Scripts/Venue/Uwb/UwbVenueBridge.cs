using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 原始推送 → 解析 →（可选录制）→ VenuePlayerSnapshot → VenuePlayerSync。
    /// 网络线程入队，主线程出队推送。
    /// </summary>
    public sealed class UwbVenueBridge
    {
        readonly ConcurrentQueue<string> _rawQueue = new ConcurrentQueue<string>();
        readonly Dictionary<int, VenuePlayerSnapshot> _latest = new Dictionary<int, VenuePlayerSnapshot>(64);
        readonly List<VenuePlayerSnapshot> _batch = new List<VenuePlayerSnapshot>(64);
        readonly UwbCoordinateMapper _mapper;
        readonly UwbTagRegistry _registry;
        readonly VenuePlayerSync _sync;
        readonly bool _usePushAll;
        readonly float _pushAllInterval;
        UwbRecorder _recorder;
        UwbPositionCorrector _corrector;
        VenueSpace _debugSpace;
        float _pushAcc;
        int _accepted;
        int _parseFail;

        public int Accepted => _accepted;
        public int ParseFail => _parseFail;
        public int PendingRaw => _rawQueue.Count;
        public int TrackedTags => _latest.Count;

        public UwbVenueBridge(
            VenuePlayerSync sync,
            UwbCoordinateMapper mapper,
            UwbTagRegistry registry,
            bool usePushAll,
            float pushAllInterval)
        {
            _sync = sync;
            _mapper = mapper ?? new UwbCoordinateMapper();
            _registry = registry ?? new UwbTagRegistry();
            _registry.Rebuild();
            _usePushAll = usePushAll;
            _pushAllInterval = Mathf.Max(0.05f, pushAllInterval);
        }

        public void SetRecorder(UwbRecorder recorder)
        {
            _recorder = recorder;
        }

        /// <summary>
        /// corrector 为空则不纠偏。debugSpace 非空时，被纠偏超过 0.25m 的点在 Scene 视图画红线（原始→纠偏）。
        /// </summary>
        public void SetCorrector(UwbPositionCorrector corrector, VenueSpace debugSpace = null)
        {
            _corrector = corrector;
            _debugSpace = debugSpace;
        }

        public void OnRawMessage(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return;
            _rawQueue.Enqueue(raw);
        }

        void DrawCorrection(Vector3 raw, Vector3 corrected)
        {
            if (_debugSpace == null)
                return;
            Vector3 d = corrected - raw;
            d.y = 0f;
            if (d.sqrMagnitude < 0.0625f)
                return;
            Vector3 lift = new Vector3(0f, 0.1f, 0f);
            Debug.DrawLine(
                _debugSpace.ToWorld(raw) + lift,
                _debugSpace.ToWorld(corrected) + lift,
                Color.red,
                0.6f);
        }

        public void Tick(float deltaTime)
        {
            if (_sync == null)
                return;

            double now = Time.timeAsDouble;
            while (_rawQueue.TryDequeue(out string raw))
            {
                _recorder?.EnqueueRaw(raw);
                if (!UwbPosParser.TryParse(raw, now, out UwbPosEvent evt))
                {
                    _parseFail++;
                    continue;
                }

                var binding = _registry.Resolve(evt.uid, evt.deviceName);
                if (binding == null)
                    continue;

                Vector3 venue = _mapper.ToVenueMeters(evt.rawX, evt.rawY, evt.rawZ);
                // 快照不再带朝向，脸朝哪由 VenuePlayerSync 按位移决定；纠偏器的 yaw 参数只是占位
                float yaw = 0f;

                double stamp = evt.deviceTimeMs > 0
                    ? evt.deviceTimeMs / 1000.0
                    : now;

                if (_corrector != null)
                {
                    if (!_corrector.Submit(binding.playerId, stamp, venue, yaw,
                            out venue, out yaw, out stamp, out Vector3 rawVenue))
                        continue;
                    DrawCorrection(rawVenue, venue);
                }

                var snap = new VenuePlayerSnapshot
                {
                    id = binding.playerId,
                    team = binding.team,
                    name = binding.displayName,
                    x = venue.x,
                    y = venue.y,
                    z = venue.z,
                    hp = _registry.defaultMaxHp,
                    maxHp = _registry.defaultMaxHp,
                    firing = false,
                    time = stamp
                };

                _latest[binding.playerId] = snap;
                if (!_usePushAll)
                    _sync.Push(snap);

                _accepted++;
            }

            if (!_usePushAll)
                return;

            _pushAcc += deltaTime;
            if (_pushAcc < _pushAllInterval)
                return;
            _pushAcc = 0f;
            if (_latest.Count == 0)
                return;

            _batch.Clear();
            foreach (var kv in _latest)
                _batch.Add(kv.Value);
            _sync.PushAll(_batch);
        }
    }
}
