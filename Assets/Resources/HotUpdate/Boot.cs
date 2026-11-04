using System;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    /// <summary>
    /// 永久离线包模式启动入口。
    /// 已移除 HybridCLR 热更，所有 LetsPiu.* 程序集直接 IL2CPP 编进主工程；
    /// 资源走 YooAsset 的 OfflinePlayMode，全部从 StreamingAssets 读取。
    /// </summary>
    public class Boot : MonoBehaviour
    {
        public EPlayMode m_InitModel = EPlayMode.OfflinePlayMode;

        Transform m_Root;
        HotUpdatePanel m_LoadPanel = null;

        void Awake()
        {
            Application.runInBackground = true;
#if UNITY_EDITOR
            m_InitModel = EPlayMode.EditorSimulateMode;
#else
            m_InitModel = EPlayMode.OfflinePlayMode;
#endif
            Debug.Log($"资源系统运行模式：{m_InitModel}");
            m_Root = GameObject.Find("UIRoot/RootCanvas/PanelRoot").GetComponent<Transform>();
        }

        async void Start()
        {
            // x86 设备/模拟器拦截：当前 APK 仅支持 armeabi-v7a / arm64-v8a
#if UNITY_ANDROID && !UNITY_EDITOR
            if (await IsX86AndroidAndShouldQuit())
            {
                return;
            }
#endif

            EnsureVersionOverlay();
            YooAssets.Initialize();

            var loadingPanelAsset = Resources.Load<GameObject>("HotUpdate/HotUpdatePanel");
            m_LoadPanel = Instantiate(loadingPanelAsset, m_Root).GetComponent<HotUpdatePanel>();

            var operate = new UpdateOperation(m_LoadPanel.UpdateCallBack, m_InitModel, null, true);
            m_LoadPanel.SetMachine(operate.machine);
            YooAssets.StartOperation(operate);
            await operate.ToUniTask();

            await InvokeStartGame();

            Resources.UnloadUnusedAssets().ToUniTask().Forget();
            Destroy(gameObject);
        }

        /// <summary>
        /// 通过反射调用 LetsPiu.UI 程序集中的 StartGame.OnBeforeCreation()。
        /// 之所以用反射而不是直接引用：LetsPiu.UI 程序集内部已反向引用了本 LetsPiu.HotUpdate
        /// 程序集（用于 HotUpdatePanel/UpdateOperation 等），直接引用会形成循环依赖。
        /// IL2CPP 完全支持这种反射调用，运行时稳定。
        /// </summary>
        static void EnsureVersionOverlay()
        {
            var uiAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "LetsPiu.UI");
            var overlayType = uiAssembly?.GetType("LetsPiu.UI.ScoreCardCommon.ScoreCardMatchIdOverlay");
            overlayType?.GetMethod("EnsureInstalled", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
        }

        async UniTask InvokeStartGame()
        {
            var uiAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "LetsPiu.UI");
            if (uiAssembly == null)
            {
                Debug.LogError("[Boot] 找不到 LetsPiu.UI 程序集，无法启动游戏");
                return;
            }

            var startGameType = uiAssembly.GetType("StartGame");
            if (startGameType == null)
            {
                Debug.LogError("[Boot] LetsPiu.UI 程序集中找不到 StartGame 类型");
                return;
            }

            var method = startGameType.GetMethod("OnBeforeCreation", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError("[Boot] StartGame.OnBeforeCreation 方法不存在");
                return;
            }

            var result = method.Invoke(null, null);
            if (result is UniTask uniTask)
            {
                await uniTask;
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        async UniTask<bool> IsX86AndroidAndShouldQuit()
        {
            try
            {
                using var buildClass = new AndroidJavaClass("android.os.Build");
                string[] abis = null;
                try
                {
                    var supportedAbis = buildClass.GetStatic<AndroidJavaObject>("SUPPORTED_ABIS");
                    if (supportedAbis != null)
                    {
                        int len = supportedAbis.Call<int>("length");
                        abis = new string[len];
                        for (int i = 0; i < len; i++)
                        {
                            abis[i] = supportedAbis.Get<string>(i.ToString());
                        }
                    }
                }
                catch
                {
                    try { abis = new[] { buildClass.GetStatic<string>("CPU_ABI") }; }
                    catch { abis = null; }
                }

                if (abis == null) return false;

                for (int i = 0; i < abis.Length; i++)
                {
                    var a = (abis[i] ?? string.Empty).ToLowerInvariant();
                    if (a.Contains("x86") || a.Contains("x86_64"))
                    {
                        Debug.LogWarning("Detected x86 CPU ABI on Android device/emulator - this Unity build does not support x86. Showing warning and exiting to prevent crash.");
                        try
                        {
                            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                            if (activity != null)
                            {
                                using var builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity);
                                builder.Call<AndroidJavaObject>("setTitle", "不支持的设备");
                                builder.Call<AndroidJavaObject>("setMessage", "检测到 x86 模拟器或设备，当前 Unity 构建不支持 x86，请使用 armeabi-v7a 或 arm64-v8a 设备。应用将退出以防止崩溃。");
                                builder.Call<AndroidJavaObject>("setCancelable", false);
                                AndroidJavaRunnable onOk = () =>
                                {
                                    try { activity.Call("finish"); } catch { }
                                    try { System.Diagnostics.Process.GetCurrentProcess().Kill(); } catch { }
                                };
                                builder.Call<AndroidJavaObject>("setPositiveButton", "确定", onOk);
                                var dialog = builder.Call<AndroidJavaObject>("create");
                                dialog.Call("show");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning("Failed to show native dialog: " + ex.Message);
                        }

                        await UniTask.Delay(1500);
                        Application.Quit();
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("x86 detection failed: " + e.Message);
            }

            return false;
        }
#endif
    }
}
