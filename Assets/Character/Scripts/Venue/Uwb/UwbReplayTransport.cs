using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 回放按日 jsonl。支持循环时段 [rangeStart, rangeEnd]（秒，相对文件内 t）。
    /// 无文件或无帧时不回放。
    /// </summary>
    public sealed class UwbReplayTransport : IUwbTransport
    {
        struct Frame
        {
            public double t;
            public string raw;
        }

        readonly string _path;
        readonly bool _loop;
        readonly float _speed;
        readonly double _rangeStart;
        readonly double _rangeEnd;
        readonly List<Frame> _frames = new List<Frame>(4096);
        int _index;
        double _playTime;
        bool _running;
        double _clipStart;
        double _clipEnd;

        public bool IsRunning => _running;
        public event Action<string> RawMessageReceived;
        public int FrameCount => _frames.Count;
        public double ClipStart => _clipStart;
        public double ClipEnd => _clipEnd;

        /// <param name="rangeStart">时段起点秒；&lt;0 表示文件最小 t</param>
        /// <param name="rangeEnd">时段终点秒；&lt;0 或 &lt;=start 表示文件最大 t</param>
        public UwbReplayTransport(string path, bool loop = true, float speed = 1f,
            double rangeStart = -1, double rangeEnd = -1)
        {
            _path = path;
            _loop = loop;
            _speed = Mathf.Max(0.01f, speed);
            _rangeStart = rangeStart;
            _rangeEnd = rangeEnd;
        }

        public void Start()
        {
            _frames.Clear();
            _index = 0;
            _running = false;
            if (string.IsNullOrEmpty(_path) || !File.Exists(_path))
            {
                Debug.LogWarning("[UwbReplay] 无日志文件，跳过回放: " + _path);
                return;
            }

            double fileMin = double.MaxValue;
            double fileMax = double.MinValue;
            var all = new List<Frame>(4096);
            foreach (string line in File.ReadLines(_path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                try
                {
                    JObject obj = JObject.Parse(line);
                    double t = obj.Value<double?>("t") ?? 0;
                    string raw = obj.Value<string>("raw");
                    if (string.IsNullOrEmpty(raw))
                        continue;
                    all.Add(new Frame { t = t, raw = raw });
                    if (t < fileMin) fileMin = t;
                    if (t > fileMax) fileMax = t;
                }
                catch
                {
                    // skip
                }
            }

            if (all.Count == 0)
            {
                Debug.LogWarning("[UwbReplay] 文件无有效帧: " + _path);
                return;
            }

            _clipStart = _rangeStart >= 0 ? _rangeStart : fileMin;
            _clipEnd = _rangeEnd > _clipStart ? _rangeEnd : fileMax;
            for (int i = 0; i < all.Count; i++)
            {
                Frame f = all[i];
                if (f.t < _clipStart || f.t > _clipEnd)
                    continue;
                _frames.Add(f);
            }

            if (_frames.Count == 0)
            {
                Debug.LogWarning(
                    $"[UwbReplay] 时段内无帧 range=[{_clipStart:0.##},{_clipEnd:0.##}] file={_path}");
                return;
            }

            _playTime = _frames[0].t;
            _index = 0;
            _running = true;
            Debug.Log(
                $"[UwbReplay] frames={_frames.Count}/{all.Count} " +
                $"range=[{_clipStart:0.##},{_clipEnd:0.##}] loop={_loop} file={_path}");
        }

        public void Stop()
        {
            _running = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_running || _frames.Count == 0)
                return;

            _playTime += deltaTime * _speed;
            while (_index < _frames.Count && _frames[_index].t <= _playTime)
            {
                RawMessageReceived?.Invoke(_frames[_index].raw);
                _index++;
            }

            if (_index < _frames.Count)
                return;

            if (_loop)
            {
                _index = 0;
                _playTime = _frames[0].t;
            }
            else
            {
                _running = false;
                Debug.Log("[UwbReplay] finished");
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
