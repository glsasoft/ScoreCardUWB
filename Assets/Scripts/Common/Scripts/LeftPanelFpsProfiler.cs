using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LetsPiu
{
    /// <summary>
    /// 非侵入式帧率/场景负载埋点：挂在 LeftPanel1920Lock 上即可。
    /// 周期性采样 FPS、渲染统计、场景对象计数，写入设备文件供 adb pull。
    /// 不修改任何业务逻辑。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    public class LeftPanelFpsProfiler : MonoBehaviour
    {
        const string HostName = "LeftPanelFpsProfiler_Host";

        [SerializeField] bool enableProfiling = true;
        [Tooltip("每隔多少秒写一条采样")]
        [SerializeField] float sampleIntervalSec = 1f;
        [Tooltip("每隔多少秒做一次场景对象扫描（较贵）")]
        [SerializeField] float sceneScanIntervalSec = 5f;
        [SerializeField] int topHeavyObjectCount = 8;
        [SerializeField] bool alsoWriteToSdcard = true;

        float _sampleTimer;
        float _scanTimer = 999f;
        float _smoothedDt;
        float _fps;

        ProfilerRecorder _drawCalls;
        ProfilerRecorder _batches;
        ProfilerRecorder _triangles;
        ProfilerRecorder _setPass;
        ProfilerRecorder _gcAlloc;
        ProfilerRecorder _mainThread;
        ProfilerRecorder _renderThread;

        SceneScan _lastScan;
        StreamWriter _writer;
        string _filePath;
        string _sdcardPath;
        readonly StringBuilder _sb = new StringBuilder(1024);
        readonly List<HeavyObject> _heavyBuf = new List<HeavyObject>(64);

        struct HeavyObject
        {
            public string name;
            public string path;
            public int skinned;
            public int mesh;
            public int particles;
            public int score;
        }

        struct SceneScan
        {
            public int skinned;
            public int meshR;
            public int particleSys;
            public int particleCount;
            public int animators;
            public int canvases;
            public int cameras;
            public int lights;
            public int audioSources;
            public string topJson;
        }

        void Awake()
        {
            if (gameObject.name != HostName)
            {
                var host = new GameObject(HostName);
                var moved = host.AddComponent<LeftPanelFpsProfiler>();
                moved.enableProfiling = enableProfiling;
                moved.sampleIntervalSec = sampleIntervalSec;
                moved.sceneScanIntervalSec = sceneScanIntervalSec;
                moved.topHeavyObjectCount = topHeavyObjectCount;
                moved.alsoWriteToSdcard = alsoWriteToSdcard;
                DontDestroyOnLoad(host);
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            if (enableProfiling)
            {
                StartRecorders();
                OpenWriter();
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnDestroy()
        {
            StopRecorders();
            CloseWriter();
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                FlushWriter();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _scanTimer = 999f;
            WriteEvent("scene", scene.name);
        }

        void Update()
        {
            if (!enableProfiling)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;
            if (dt > 0f)
            {
                _smoothedDt = _smoothedDt <= 0f ? dt : _smoothedDt + (dt - _smoothedDt) * 0.15f;
                _fps = 1f / _smoothedDt;
            }

            _sampleTimer += dt;
            _scanTimer += dt;

            if (_scanTimer >= sceneScanIntervalSec)
            {
                _scanTimer = 0f;
                _lastScan = ScanScene();
            }

            if (_sampleTimer >= sampleIntervalSec)
            {
                _sampleTimer = 0f;
                WriteSample();
            }
        }

        void StartRecorders()
        {
            _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            _batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            _triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            _setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            _gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            _mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
            _renderThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread");
        }

        void StopRecorders()
        {
            if (_drawCalls.Valid) _drawCalls.Dispose();
            if (_batches.Valid) _batches.Dispose();
            if (_triangles.Valid) _triangles.Dispose();
            if (_setPass.Valid) _setPass.Dispose();
            if (_gcAlloc.Valid) _gcAlloc.Dispose();
            if (_mainThread.Valid) _mainThread.Dispose();
            if (_renderThread.Valid) _renderThread.Dispose();
        }

        long LastValue(ProfilerRecorder recorder)
        {
            return recorder.Valid ? recorder.LastValue : -1;
        }

        void OpenWriter()
        {
            try
            {
                string dir = Path.Combine(Application.persistentDataPath, "fps_profile");
                Directory.CreateDirectory(dir);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _filePath = Path.Combine(dir, $"fps_{stamp}.jsonl");
                _writer = new StreamWriter(_filePath, true, Encoding.UTF8) { AutoFlush = true };

                if (alsoWriteToSdcard)
                {
                    try
                    {
                        string sd = "/sdcard/uwb_fps_profile";
                        Directory.CreateDirectory(sd);
                        _sdcardPath = Path.Combine(sd, $"fps_{stamp}.jsonl");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[LeftPanelFpsProfiler] sdcard path fail: {e.Message}");
                        _sdcardPath = null;
                    }
                }

                WriteEvent("start", Application.identifier);
                Debug.Log($"[LeftPanelFpsProfiler] writing {_filePath}" +
                          (_sdcardPath != null ? $" and {_sdcardPath}" : string.Empty));
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeftPanelFpsProfiler] open writer fail: {e.Message}");
            }
        }

        void CloseWriter()
        {
            WriteEvent("stop", "destroy");
            FlushWriter();
            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        void FlushWriter()
        {
            try
            {
                _writer?.Flush();
            }
            catch
            {
            }
        }

        void WriteLine(string line)
        {
            if (_writer == null)
            {
                return;
            }

            try
            {
                _writer.WriteLine(line);
                if (!string.IsNullOrEmpty(_sdcardPath))
                {
                    File.AppendAllText(_sdcardPath, line + "\n", Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeftPanelFpsProfiler] write fail: {e.Message}");
            }
        }

        void WriteEvent(string type, string detail)
        {
            _sb.Clear();
            _sb.Append("{\"type\":\"").Append(Escape(type))
                .Append("\",\"t\":").Append(Time.realtimeSinceStartup.ToString("F3"))
                .Append(",\"detail\":\"").Append(Escape(detail))
                .Append("\",\"scene\":\"").Append(Escape(SceneManager.GetActiveScene().name))
                .Append("\"}");
            WriteLine(_sb.ToString());
        }

        void WriteSample()
        {
            long draw = LastValue(_drawCalls);
            long batches = LastValue(_batches);
            long tris = LastValue(_triangles);
            long setPass = LastValue(_setPass);
            long gc = LastValue(_gcAlloc);
            // Main/Render Thread recorder 单位是纳秒
            double mainMs = LastValue(_mainThread) / 1000000.0;
            double renderMs = LastValue(_renderThread) / 1000000.0;

            string hint = BuildHint(_fps, draw, tris, setPass, gc, _lastScan);

            _sb.Clear();
            _sb.Append("{\"type\":\"sample\",\"t\":").Append(Time.realtimeSinceStartup.ToString("F3"))
                .Append(",\"scene\":\"").Append(Escape(SceneManager.GetActiveScene().name))
                .Append("\",\"fps\":").Append(_fps.ToString("F1"))
                .Append(",\"dtMs\":").Append((_smoothedDt * 1000f).ToString("F1"))
                .Append(",\"screen\":\"").Append(Screen.width).Append('x').Append(Screen.height).Append('"')
                .Append(",\"drawCalls\":").Append(draw)
                .Append(",\"batches\":").Append(batches)
                .Append(",\"tris\":").Append(tris)
                .Append(",\"setPass\":").Append(setPass)
                .Append(",\"gcBytes\":").Append(gc)
                .Append(",\"mainMs\":").Append(mainMs.ToString("F2"))
                .Append(",\"renderMs\":").Append(renderMs.ToString("F2"))
                .Append(",\"skinned\":").Append(_lastScan.skinned)
                .Append(",\"meshR\":").Append(_lastScan.meshR)
                .Append(",\"particleSys\":").Append(_lastScan.particleSys)
                .Append(",\"particleCount\":").Append(_lastScan.particleCount)
                .Append(",\"animators\":").Append(_lastScan.animators)
                .Append(",\"canvases\":").Append(_lastScan.canvases)
                .Append(",\"cameras\":").Append(_lastScan.cameras)
                .Append(",\"lights\":").Append(_lastScan.lights)
                .Append(",\"audio\":").Append(_lastScan.audioSources)
                .Append(",\"hint\":\"").Append(Escape(hint)).Append('"')
                .Append(",\"top\":").Append(string.IsNullOrEmpty(_lastScan.topJson) ? "[]" : _lastScan.topJson)
                .Append('}');
            WriteLine(_sb.ToString());
        }

        static string BuildHint(float fps, long draw, long tris, long setPass, long gc, SceneScan scan)
        {
            if (fps >= 50f)
            {
                return "ok";
            }

            var parts = new List<string>(4);
            if (scan.skinned >= 30)
            {
                parts.Add($"skinnedx{scan.skinned}");
            }

            if (tris >= 500000)
            {
                parts.Add($"tris{tris}");
            }

            if (setPass >= 80)
            {
                parts.Add($"setPass{setPass}");
            }

            if (draw >= 200)
            {
                parts.Add($"draw{draw}");
            }

            if (scan.particleCount >= 500)
            {
                parts.Add($"fx{scan.particleCount}");
            }

            if (gc >= 128 * 1024)
            {
                parts.Add($"gc{gc}");
            }

            if (scan.canvases >= 5)
            {
                parts.Add($"ui{scan.canvases}");
            }

            return parts.Count == 0 ? "cpu_or_unknown" : string.Join("|", parts);
        }

        SceneScan ScanScene()
        {
            var scan = new SceneScan();
            _heavyBuf.Clear();

            var skinned = FindObjectsOfType<SkinnedMeshRenderer>();
            scan.skinned = CountEnabledRenderers(skinned);

            var mesh = FindObjectsOfType<MeshRenderer>();
            scan.meshR = CountEnabledRenderers(mesh);

            var particles = FindObjectsOfType<ParticleSystem>();
            scan.particleSys = 0;
            scan.particleCount = 0;
            for (int i = 0; i < particles.Length; i++)
            {
                ParticleSystem ps = particles[i];
                if (ps == null || !ps.gameObject.activeInHierarchy)
                {
                    continue;
                }

                scan.particleSys++;
                scan.particleCount += ps.particleCount;
            }

            scan.animators = CountEnabled(FindObjectsOfType<Animator>());
            scan.canvases = CountEnabled(FindObjectsOfType<Canvas>());
            scan.cameras = Camera.allCamerasCount;
            scan.lights = CountEnabled(FindObjectsOfType<Light>());
            scan.audioSources = CountEnabled(FindObjectsOfType<AudioSource>());

            // 按根下 Skinned/Mesh/粒子 粗略打分，找重对象
            var roots = new Dictionary<Transform, HeavyObject>(64);
            AccumulateRenderers(skinned, roots, isSkinned: true);
            AccumulateRenderers(mesh, roots, isSkinned: false);
            for (int i = 0; i < particles.Length; i++)
            {
                ParticleSystem ps = particles[i];
                if (ps == null || !ps.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Transform root = ps.transform.root;
                roots.TryGetValue(root, out HeavyObject h);
                h.name = root.name;
                h.path = GetPath(root);
                h.particles += ps.particleCount;
                h.score = h.skinned * 10 + h.mesh + h.particles / 50;
                roots[root] = h;
            }

            _heavyBuf.AddRange(roots.Values);
            _heavyBuf.Sort((a, b) => b.score.CompareTo(a.score));

            _sb.Clear();
            _sb.Append('[');
            int n = Mathf.Min(topHeavyObjectCount, _heavyBuf.Count);
            for (int i = 0; i < n; i++)
            {
                if (i > 0) _sb.Append(',');
                HeavyObject h = _heavyBuf[i];
                _sb.Append("{\"name\":\"").Append(Escape(h.name))
                    .Append("\",\"path\":\"").Append(Escape(h.path))
                    .Append("\",\"skinned\":").Append(h.skinned)
                    .Append(",\"mesh\":").Append(h.mesh)
                    .Append(",\"particles\":").Append(h.particles)
                    .Append(",\"score\":").Append(h.score)
                    .Append('}');
            }

            _sb.Append(']');
            scan.topJson = _sb.ToString();
            return scan;
        }

        static void AccumulateRenderers<T>(T[] renderers, Dictionary<Transform, HeavyObject> roots, bool isSkinned)
            where T : Renderer
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                T r = renderers[i];
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Transform root = r.transform.root;
                roots.TryGetValue(root, out HeavyObject h);
                h.name = root.name;
                h.path = GetPath(root);
                if (isSkinned) h.skinned++;
                else h.mesh++;
                h.score = h.skinned * 10 + h.mesh + h.particles / 50;
                roots[root] = h;
            }
        }

        static int CountEnabled<T>(T[] arr) where T : Behaviour
        {
            int n = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                T b = arr[i];
                if (b != null && b.enabled && b.gameObject.activeInHierarchy)
                {
                    n++;
                }
            }

            return n;
        }

        static int CountEnabledRenderers<T>(T[] arr) where T : Renderer
        {
            int n = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                T r = arr[i];
                if (r != null && r.enabled && r.gameObject.activeInHierarchy)
                {
                    n++;
                }
            }

            return n;
        }

        static string GetPath(Transform t)
        {
            if (t == null) return string.Empty;
            _pathSb.Clear();
            var stack = new Stack<string>(8);
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }

            while (stack.Count > 0)
            {
                if (_pathSb.Length > 0) _pathSb.Append('/');
                _pathSb.Append(stack.Pop());
            }

            return _pathSb.ToString();
        }

        static readonly StringBuilder _pathSb = new StringBuilder(128);

        static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
