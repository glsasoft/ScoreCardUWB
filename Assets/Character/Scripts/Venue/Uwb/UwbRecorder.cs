using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 异步 JSONL 录制。真机按日追加到 uwb_yyyyMMdd.jsonl。
    /// 每行：{"t":当日秒,"ms":unix毫秒,"raw":原始推送}
    /// </summary>
    public sealed class UwbRecorder : IDisposable
    {
        readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        readonly string _path;
        readonly Thread _thread;
        readonly AutoResetEvent _signal = new AutoResetEvent(false);
        volatile bool _running = true;
        long _dropped;
        int _queued;
        readonly int _maxQueue;

        public string FilePath => _path;
        public long Dropped => Interlocked.Read(ref _dropped);
        public int Queued => _queued;
        public bool IsRunning => _running;

        public UwbRecorder(string directory, int maxQueue = 20000)
        {
            _maxQueue = Mathf.Max(1000, maxQueue);
            if (string.IsNullOrEmpty(directory))
                directory = UwbRecordStore.DefaultDirectory;
            Directory.CreateDirectory(directory);
            _path = UwbRecordStore.TodayFilePath(directory);
            _thread = new Thread(WriteLoop)
            {
                IsBackground = true,
                Name = "UwbRecorder"
            };
            _thread.Start();
            Debug.Log("[UwbRecorder] append " + _path);
        }

        public void EnqueueRaw(string rawJson)
        {
            if (!_running || string.IsNullOrEmpty(rawJson))
                return;

            if (_queued >= _maxQueue)
            {
                Interlocked.Increment(ref _dropped);
                return;
            }

            DateTime now = DateTime.Now;
            double t = (now - now.Date).TotalSeconds;
            long ms = new DateTimeOffset(now).ToUnixTimeMilliseconds();
            string escaped = Escape(rawJson);
            string line = "{\"t\":" + t.ToString("0.###", CultureInfo.InvariantCulture) +
                          ",\"ms\":" + ms + ",\"raw\":\"" + escaped + "\"}";
            _queue.Enqueue(line);
            Interlocked.Increment(ref _queued);
            _signal.Set();
        }

        public void Dispose()
        {
            _running = false;
            _signal.Set();
            try
            {
                if (!_thread.Join(2000))
                    Debug.LogWarning("[UwbRecorder] writer join timeout");
            }
            catch
            {
                // ignore
            }

            if (_dropped > 0)
                Debug.LogWarning($"[UwbRecorder] finished path={_path} dropped={_dropped}");
            else
                Debug.Log($"[UwbRecorder] finished path={_path}");
        }

        void WriteLoop()
        {
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(
                    new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read),
                    new UTF8Encoding(false),
                    64 * 1024);
                while (_running || !_queue.IsEmpty)
                {
                    if (_queue.TryDequeue(out string line))
                    {
                        Interlocked.Decrement(ref _queued);
                        writer.WriteLine(line);
                    }
                    else
                    {
                        writer.Flush();
                        _signal.WaitOne(200);
                    }
                }

                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError("[UwbRecorder] write failed: " + e.Message);
            }
            finally
            {
                try { writer?.Dispose(); } catch { /* ignore */ }
            }
        }

        static string Escape(string s)
        {
            var sb = new StringBuilder(s.Length + 16);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
