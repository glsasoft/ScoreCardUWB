using UnityEngine;
using UnityEngine.SceneManagement;

namespace LetsPiu
{
    /// <summary>
    /// 附加组件：超宽屏时把画面锁定到左侧一块 1920x1080（1 号屏），其余区域黑屏。
    /// 原生已是单屏 1080p 时自动跳过，不影响原有逻辑。
    /// 用法：挂到 LoginScene 任意常驻物体（建议 Boot）即可。
    /// </summary>
    [DisallowMultipleComponent]
    public class LeftPanelViewportLock : MonoBehaviour
    {
        [SerializeField] int panelWidth = 1920;
        [SerializeField] int panelHeight = 1080;
        [Tooltip("从左起第几块屏，0=1号屏")]
        [SerializeField] int panelIndex = 0;

        [Header("FPS Overlay")]
        [SerializeField] bool showFps = true;
        [SerializeField] Color fpsColor = Color.green;
        [SerializeField] int fpsFontSize = 14;

        Camera _clearCamera;
        bool _active;
        int _lastScreenW;
        int _lastScreenH;
        int _lastCameraCount = -1;

        float _smoothedDelta;
        float _fps;
        GUIStyle _fpsStyle;
        bool _fpsStyleReady;

        void Awake()
        {
            if (!LeftPanelPlatform.IsAndroidPlayer)
            {
                enabled = false;
                Destroy(this);
                return;
            }

            // 若挂在 Boot 等会被 Destroy 的物体上，迁移到独立宿主，避免启动结束后组件被带走。
            if (gameObject.name != "LeftPanelViewportLock_Host")
            {
                var host = new GameObject("LeftPanelViewportLock_Host");
                var moved = host.AddComponent<LeftPanelViewportLock>();
                moved.panelWidth = panelWidth;
                moved.panelHeight = panelHeight;
                moved.panelIndex = panelIndex;
                moved.showFps = showFps;
                moved.fpsColor = fpsColor;
                moved.fpsFontSize = fpsFontSize;
                DontDestroyOnLoad(host);
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            TryActivate();
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
            if (_clearCamera != null)
            {
                Destroy(_clearCamera.gameObject);
                _clearCamera = null;
            }
        }

        void Update()
        {
            if (!showFps)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
            {
                return;
            }

            if (_smoothedDelta <= 0f)
            {
                _smoothedDelta = dt;
            }
            else
            {
                _smoothedDelta += (dt - _smoothedDelta) * 0.15f;
            }

            _fps = 1f / _smoothedDelta;
        }

        void LateUpdate()
        {
            if (!_active)
            {
                return;
            }

            if (_lastScreenW != Screen.width || _lastScreenH != Screen.height)
            {
                TryActivate();
                return;
            }

            ApplyToAllCameras();
        }

        void OnGUI()
        {
            if (!showFps)
            {
                return;
            }

            EnsureFpsStyle();
            string text = $"FPS:{_fps:F0}";

            // 画在左上，锁定左屏时也落在可见区域
            float x = 8f;
            float y = 6f;
            if (_active)
            {
                Rect panel = GetTargetRect();
                x = panel.x + 8f;
                y = Screen.height - panel.yMax + 6f;
            }

            GUI.Label(new Rect(x, y, 120f, 28f), text, _fpsStyle);
        }

        void EnsureFpsStyle()
        {
            if (_fpsStyleReady && _fpsStyle != null)
            {
                return;
            }

            int size = Mathf.Max(10, fpsFontSize);
            _fpsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Overflow
            };
            _fpsStyle.normal.textColor = fpsColor;
            _fpsStyleReady = true;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_active)
            {
                ApplyToAllCameras();
            }
        }

        void TryActivate()
        {
            _lastScreenW = Screen.width;
            _lastScreenH = Screen.height;

            bool shouldLock = Screen.width > panelWidth && Screen.height > 0;
            if (!shouldLock)
            {
                if (_active)
                {
                    RestoreCameras();
                    if (_clearCamera != null)
                    {
                        Destroy(_clearCamera.gameObject);
                        _clearCamera = null;
                    }
                }

                _active = false;
                Debug.Log(
                    $"[LeftPanelViewportLock] skip screen={Screen.width}x{Screen.height} " +
                    $"(need width>{panelWidth})");
                return;
            }

            _active = true;
            EnsureClearCamera();
            ApplyToAllCameras();
            Debug.Log(
                $"[LeftPanelViewportLock] enabled screen={Screen.width}x{Screen.height} " +
                $"rect={GetTargetRect()} panelIndex={panelIndex}");
        }

        Rect GetTargetRect()
        {
            float x = panelIndex * panelWidth;
            float h = Mathf.Min(panelHeight, Screen.height);
            float y = Mathf.Max(0f, (Screen.height - h) * 0.5f);
            return new Rect(x, y, panelWidth, h);
        }

        void EnsureClearCamera()
        {
            if (_clearCamera != null)
            {
                return;
            }

            var go = new GameObject("LeftPanelViewportLock_ClearCam");
            DontDestroyOnLoad(go);
            _clearCamera = go.AddComponent<Camera>();
            _clearCamera.clearFlags = CameraClearFlags.SolidColor;
            _clearCamera.backgroundColor = Color.black;
            _clearCamera.cullingMask = 0;
            _clearCamera.depth = -100f;
            _clearCamera.orthographic = true;
            _clearCamera.allowHDR = false;
            _clearCamera.allowMSAA = false;
            _clearCamera.rect = new Rect(0f, 0f, 1f, 1f);
            _clearCamera.pixelRect = new Rect(0f, 0f, Screen.width, Screen.height);
        }

        void ApplyToAllCameras()
        {
            Rect target = GetTargetRect();
            Camera[] cameras = Camera.allCameras;
            _lastCameraCount = cameras.Length;

            if (_clearCamera != null)
            {
                _clearCamera.pixelRect = new Rect(0f, 0f, Screen.width, Screen.height);
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera cam = cameras[i];
                if (cam == null || cam == _clearCamera)
                {
                    continue;
                }

                cam.pixelRect = target;
            }
        }

        void RestoreCameras()
        {
            Camera[] cameras = Camera.allCameras;
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera cam = cameras[i];
                if (cam == null || cam == _clearCamera)
                {
                    continue;
                }

                cam.rect = new Rect(0f, 0f, 1f, 1f);
            }

            _lastCameraCount = -1;
        }
    }
}
