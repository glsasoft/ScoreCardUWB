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

        public void OnRawMessage(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return;
            _rawQueue.Enqueue(raw);
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
                float yaw = _mapper.EstimateYawDegrees(evt.velX, evt.velY, evt.velZ);
                if (float.IsNaN(yaw))
                {
                    if (_latest.TryGetValue(binding.playerId, out VenuePlayerSnapshot prev))
                        yaw = prev.yaw;
                    else
                        yaw = 0f;
                }

                double stamp = evt.deviceTimeMs > 0
                    ? evt.deviceTimeMs / 1000.0
                    : now;

                var snap = new VenuePlayerSnapshot
                {
                    id = binding.playerId,
                    team = binding.team,
                    name = binding.displayName,
                    x = venue.x,
                    y = venue.y,
                    z = venue.z,
                    yaw = yaw,
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
