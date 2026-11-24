using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LetsPiu
{
    /// <summary>
    /// 附加组件：超宽屏时把 Unity 实际渲染分辨率降到 1920x1080，
    /// 并把 Android 窗口钉在最左侧对应面板，保证 3D/UI 落在左屏。
    /// 原生已是目标分辨率时自动跳过。不影响原有业务代码。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    public class FixedNativeResolution : MonoBehaviour
    {
        [SerializeField] int targetWidth = 1920;
        [SerializeField] int targetHeight = 1080;
        [Tooltip("从左起第几块屏，0=最左侧 1 号屏")]
        [SerializeField] int panelIndex = 0;
        [SerializeField] FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;
        [Tooltip("分辨率/窗口被外部改回时，是否自动重新锁定")]
        [SerializeField] bool keepLocked = true;

        const string HostName = "FixedNativeResolution_Host";
        // Gravity.LEFT | Gravity.TOP
        const int GravityLeftTop = 3 | 48;

        int _physicalW;
        int _physicalH;
        bool _physicalCaptured;
        bool _windowPinned;
        float _nextPinRetryAt;

        void Awake()
        {
            if (gameObject.name != HostName)
            {
                var host = new GameObject(HostName);
                var moved = host.AddComponent<FixedNativeResolution>();
                moved.targetWidth = targetWidth;
                moved.targetHeight = targetHeight;
                moved.panelIndex = panelIndex;
                moved.fullScreenMode = fullScreenMode;
                moved.keepLocked = keepLocked;
                DontDestroyOnLoad(host);
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            CapturePhysicalSize();
            TryApply("Awake");
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _windowPinned = false;
            TryApply("sceneLoaded:" + scene.name);
        }

        void LateUpdate()
        {
            if (!keepLocked)
            {
                return;
            }

            if (NeedsResolutionApply() || NeedsWindowPin())
            {
                TryApply("LateUpdate");
            }
        }

        void CapturePhysicalSize()
        {
            if (_physicalCaptured)
            {
                return;
            }

            int systemW = Display.main != null ? Display.main.systemWidth : 0;
            int systemH = Display.main != null ? Display.main.systemHeight : 0;
            _physicalW = Mathf.Max(systemW, Screen.width);
            _physicalH = Mathf.Max(systemH, Screen.height);
            _physicalCaptured = _physicalW > 0 && _physicalH > 0;
        }

        bool IsUltraWidePhysical()
        {
            CapturePhysicalSize();
            return _physicalCaptured && _physicalW > targetWidth;
        }

        bool NeedsResolutionApply()
        {
            if (!IsUltraWidePhysical())
            {
                return false;
            }

            return Screen.width != targetWidth || Screen.height != targetHeight;
        }

        bool NeedsWindowPin()
        {
            if (!IsUltraWidePhysical())
            {
                return false;
            }

            if (_windowPinned && Time.unscaledTime < _nextPinRetryAt)
            {
                return false;
            }

            return true;
        }

        void TryApply(string reason)
        {
            CapturePhysicalSize();

            if (!IsUltraWidePhysical())
            {
                Debug.Log(
                    $"[FixedNativeResolution] skip physical={_physicalW}x{_physicalH} " +
                    $"screen={Screen.width}x{Screen.height} target={targetWidth}x{targetHeight} reason={reason}");
                return;
            }

            if (NeedsResolutionApply())
            {
                int beforeW = Screen.width;
                int beforeH = Screen.height;
                Screen.SetResolution(targetWidth, targetHeight, fullScreenMode);
                Debug.Log(
                    $"[FixedNativeResolution] SetResolution {beforeW}x{beforeH} -> " +
                    $"{targetWidth}x{targetHeight} now={Screen.width}x{Screen.height} " +
                    $"physical={_physicalW}x{_physicalH} reason={reason}");
            }

            PinWindowToLeftPanel(reason);
        }

        void PinWindowToLeftPanel(string reason)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            int offsetX = panelIndex * targetWidth;
            int width = targetWidth;
            int height = targetHeight;
            string logReason = reason;

            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (activity == null)
                    {
                        return;
                    }

                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        AndroidJavaObject act = null;
                        try
                        {
                            using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                            {
                                act = up.GetStatic<AndroidJavaObject>("currentActivity");
                            }

                            if (act == null)
                            {
                                return;
                            }

                            using (var window = act.Call<AndroidJavaObject>("getWindow"))
                            using (var lp = window.Call<AndroidJavaObject>("getAttributes"))
                            {
                                lp.Set("gravity", GravityLeftTop);
                                lp.Set("x", offsetX);
                                lp.Set("y", 0);
                                lp.Set("width", width);
                                lp.Set("height", height);
                                window.Call("setAttributes", lp);
                                window.Call("setLayout", width, height);
                            }

                            // 同步 Unity 表面视图尺寸，避免仍按 MATCH_PARENT 铺满超宽屏
                            // android.R.id.content = 0x01020002
                            using (var content = act.Call<AndroidJavaObject>("findViewById", 0x01020002))
                            {
                                if (content != null)
                                {
                                    int childCount = content.Call<int>("getChildCount");
                                    for (int i = 0; i < childCount; i++)
                                    {
                                        using (var child = content.Call<AndroidJavaObject>("getChildAt", i))
                                        {
                                            if (child == null)
                                            {
                                                continue;
                                            }

                                            using (var viewLp = child.Call<AndroidJavaObject>("getLayoutParams"))
                                            {
                                                if (viewLp == null)
                                                {
                                                    continue;
                                                }

                                                viewLp.Set("width", width);
                                                viewLp.Set("height", height);
                                                child.Call("setLayoutParams", viewLp);
                                                child.Call("setX", 0f);
                                                child.Call("setY", 0f);
                                            }
                                        }
                                    }
                                }
                            }

                            _windowPinned = true;
                            _nextPinRetryAt = Time.unscaledTime + 2f;
                            Debug.Log(
                                $"[FixedNativeResolution] pin left panel x={offsetX} " +
                                $"{width}x{height} reason={logReason}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[FixedNativeResolution] pin window failed: {e.Message}");
                            _windowPinned = false;
                            _nextPinRetryAt = Time.unscaledTime + 1f;
                        }
                        finally
                        {
                            act?.Dispose();
                        }
                    }));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FixedNativeResolution] android bridge failed: {e.Message}");
                _windowPinned = false;
                _nextPinRetryAt = Time.unscaledTime + 1f;
            }
#else
            _windowPinned = true;
            _nextPinRetryAt = Time.unscaledTime + 5f;
            Debug.Log(
                $"[FixedNativeResolution] editor/non-android: resolution only, " +
                $"left panel index={panelIndex} reason={reason}");
#endif
        }
    }
}
