using System;
using System.Globalization;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 本地 Mock：按官方 Pos JSON 格式吐包，便于无真机时联调场景与录制格式。
    /// </summary>
    public sealed class UwbMockTransport : IUwbTransport
    {
        struct Agent
        {
            public string uid;
            public string deviceName;
            public Vector3 pos;
            public Vector3 target;
            public Vector3 vel;
        }

        readonly int _tagCount;
        readonly float _areaX;
        readonly float _areaY;
        readonly float _moveSpeed;
        readonly float _interval;
        Agent[] _agents;
        float _acc;
        long _cnt;
        bool _running;

        public bool IsRunning => _running;
        public event Action<string> RawMessageReceived;

        public UwbMockTransport(int tagCount, float areaX, float areaY, float moveSpeed, float intervalSec)
        {
            _tagCount = Mathf.Max(1, tagCount);
            _areaX = Mathf.Max(1f, areaX);
            _areaY = Mathf.Max(1f, areaY);
            _moveSpeed = Mathf.Max(0.1f, moveSpeed);
            _interval = Mathf.Max(0.02f, intervalSec);
        }

        public void Start()
        {
            _agents = new Agent[_tagCount];
            for (int i = 0; i < _tagCount; i++)
            {
                _agents[i] = new Agent
                {
                    uid = "MOCK" + (i + 1).ToString("D4"),
                    deviceName = "T" + (i + 1),
                    pos = RandomPoint(),
                    target = RandomPoint()
                };
            }

            _acc = 0f;
            _cnt = 0;
            _running = true;
            Debug.Log($"[UwbMock] start tags={_tagCount} area=±{_areaX * 0.5f}x±{_areaY * 0.5f} (centered)");
        }

        public void Stop()
        {
            _running = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_running || _agents == null)
                return;

            for (int i = 0; i < _agents.Length; i++)
                Step(ref _agents[i], deltaTime);

            _acc += deltaTime;
            if (_acc < _interval)
                return;
            _acc = 0f;

            double nowSec = Time.realtimeSinceStartupAsDouble;
            long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int i = 0; i < _agents.Length; i++)
            {
                _cnt++;
                RawMessageReceived?.Invoke(BuildPosJson(_agents[i], _cnt, ms, nowSec));
            }
        }

        public void Dispose()
        {
            Stop();
        }

        void Step(ref Agent a, float dt)
        {
            Vector3 to = a.target - a.pos;
            to.z = 0f;
            if (to.magnitude < 0.5f)
            {
                a.target = RandomPoint();
                to = a.target - a.pos;
            }

            Vector3 dir = to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.right;
            a.vel = dir * _moveSpeed;
            a.pos += a.vel * dt;
            // 与旧 VenuePlayerMockFeed 一致：以场馆原点为中心（±area/2），避免整块偏到地图外
            float hx = _areaX * 0.5f;
            float hy = _areaY * 0.5f;
            a.pos.x = Mathf.Clamp(a.pos.x, -hx, hx);
            a.pos.y = Mathf.Clamp(a.pos.y, -hy, hy);
            a.pos.z = 1.2f;
        }

        Vector3 RandomPoint()
        {
            float hx = Mathf.Max(0.5f, _areaX * 0.5f - 0.5f);
            float hy = Mathf.Max(0.5f, _areaY * 0.5f - 0.5f);
            return new Vector3(Random.Range(-hx, hx), Random.Range(-hy, hy), 1.2f);
        }

        static string BuildPosJson(Agent a, long cnt, long ms, double unused)
        {
            // 官方字段：pos=[x,y,z] 水平 XY + 高度 Z
            var sb = new StringBuilder(256);
            sb.Append("{\"data\":{");
            sb.Append("\"cnt\":").Append(cnt).Append(',');
            sb.Append("\"mapId\":1,");
            sb.Append("\"noiseRadius\":0.05,");
            sb.Append("\"pos\":[")
                .Append(F(a.pos.x)).Append(',')
                .Append(F(a.pos.y)).Append(',')
                .Append(F(a.pos.z)).Append("],");
            sb.Append("\"vel\":[")
                .Append(F(a.vel.x)).Append(',')
                .Append(F(a.vel.y)).Append(',')
                .Append(F(a.vel.z)).Append("],");
            sb.Append("\"time\":").Append(ms).Append(',');
            sb.Append("\"z\":").Append(F(a.pos.z));
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
