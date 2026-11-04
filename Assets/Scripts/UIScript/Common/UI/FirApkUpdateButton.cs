using System;
using System.Collections;
using System.Collections.Generic;
using LetsPiu.UI.UIScript.LoginScripts.Logic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LetsPiu.Common.UI
{
    /// <summary>
    /// 屏幕底部中央的「检查新版本」按钮：鼠标移动时显示，静止 <see cref="AutoHideSeconds"/> 秒后隐藏；
    /// 检查/下载/安装期间常驻显示进度。启动时自动创建，不依赖场景或 UIDEV 宏。
    /// 对话框与按钮同在自建的 Overlay 画布上，不用游戏的 DialogManager：游戏 UI 由 UI 摄像机渲染，
    /// 关门等顶层特效会盖住 DialogManager 的弹窗却不拦截点击，导致看不见的按钮被误点。
    /// 版本判断：fir 返回的 build 与本机 versionCode 比较，因此每次上传前必须递增 Bundle Version Code。
    /// fir 查询接口为分发网页内部接口（非官方开放 API），页面改版后可能需要调整解析。
    /// </summary>
    public class FirApkUpdateButton : MonoBehaviour
    {
        private const string FirServer = "https://download.appmeta.cn";
        private const string FirShort = "k8lexgap";
        private const float AutoHideSeconds = 5f;
        // Overlay 画布在所有摄像机之后绘制，必然压住游戏 UI 与特效；数值只用于和其他 Overlay 画布排序
        private const int SortingOrder = 30000050;
        // 避开 UIDev 的 Toggle 对勾（高 60）和 SubCode 测试器底栏（y 12~84）
        private const float ButtonBottomOffset = 100f;
        private const string IdleLabel = "检查新版本";
        private const string DeleteRegisterLabel = "删除注册数据";

        private static readonly Color NeutralColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        private static readonly Color PrimaryColor = new Color(0.16f, 0.42f, 0.85f, 1f);
        private static readonly Color DangerColor = new Color(0.78f, 0.2f, 0.2f, 1f);

        private GameObject _root;
        private Button _button;
        private TextMeshProUGUI _label;
        private Vector3 _lastMousePos;
        private float _lastMouseMoveTime = float.NegativeInfinity;
        private bool _busy;

        private GameObject _dialog;
        private TextMeshProUGUI _dialogTitle;
        private TextMeshProUGUI _dialogMessage;
        private Transform _dialogButtons;

        private class ReleaseInfo
        {
            public string Version;
            public int Build;
            public string Changelog;
            public long FileSize;
            public string DownloadUrl;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("[FirApkUpdateButton]");
            DontDestroyOnLoad(go);
            go.AddComponent<FirApkUpdateButton>();
        }

        private void Awake()
        {
            BuildUI();
            _lastMousePos = Input.mousePosition;
            _root.SetActive(false);
        }

        private void Update()
        {
            Vector3 mousePos = Input.mousePosition;
            if ((mousePos - _lastMousePos).sqrMagnitude > 1f)
            {
                _lastMousePos = mousePos;
                _lastMouseMoveTime = Time.unscaledTime;
            }

            bool visible = !_dialog.activeSelf && (_busy || Time.unscaledTime - _lastMouseMoveTime < AutoHideSeconds);
            if (_root.activeSelf != visible) _root.SetActive(visible);
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("FirApkUpdate_Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _root = new GameObject("Btn_CheckUpdate", typeof(RectTransform), typeof(Image), typeof(Button));
            _root.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, ButtonBottomOffset);
            rt.sizeDelta = new Vector2(560, 72);
            _root.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.07f, 0.85f);
            _button = _root.GetComponent<Button>();
            _button.onClick.AddListener(OnCheckClicked);

            _label = CreateText(rt, 28, TextAlignmentOptions.Center);
            _label.enableWordWrapping = false;
            _label.text = IdleLabel;

            BuildDialog(canvasGo.transform);
        }

        private void BuildDialog(Transform canvas)
        {
            // 全屏遮罩拦截对话框外的点击，避免点穿到下层游戏界面
            _dialog = new GameObject("Dialog", typeof(RectTransform), typeof(Image));
            _dialog.transform.SetParent(canvas, false);
            var blocker = (RectTransform)_dialog.transform;
            blocker.anchorMin = Vector2.zero;
            blocker.anchorMax = Vector2.one;
            blocker.offsetMin = blocker.offsetMax = Vector2.zero;
            _dialog.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var panel = new GameObject("Panel",
                typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(blocker, false);
            var prt = (RectTransform)panel.transform;
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(1000, 0);
            panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 0.98f);
            var vl = panel.GetComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(48, 48, 40, 40);
            vl.spacing = 32;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childControlWidth = vl.childControlHeight = true;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _dialogTitle = CreateText(prt, 40, TextAlignmentOptions.Center);
            _dialogMessage = CreateText(prt, 28, TextAlignmentOptions.TopLeft);

            var row = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(prt, false);
            var hl = row.GetComponent<HorizontalLayoutGroup>();
            hl.spacing = 30;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = hl.childControlHeight = true;
            hl.childForceExpandWidth = hl.childForceExpandHeight = false;
            _dialogButtons = row.transform;

            _dialog.SetActive(false);
        }

        private static TextMeshProUGUI CreateText(Transform parent, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableWordWrapping = true;
            return text;
        }

        /// <summary>按钮点击后先关闭对话框再执行回调，回调里可直接打开下一个对话框。</summary>
        private void ShowDialog(string title, string message, params (string label, Action onClick, Color color)[] buttons)
        {
            foreach (Transform child in _dialogButtons) Destroy(child.gameObject);

            _dialogTitle.text = title;
            _dialogMessage.text = message;
            foreach (var (label, onClick, color) in buttons)
            {
                var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                go.transform.SetParent(_dialogButtons, false);
                go.GetComponent<Image>().color = color;
                var le = go.GetComponent<LayoutElement>();
                le.preferredWidth = 260;
                le.preferredHeight = 72;
                CreateText(go.transform, 28, TextAlignmentOptions.Center).text = label;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _dialog.SetActive(false);
                    onClick?.Invoke();
                });
            }
            _dialog.SetActive(true);
        }

        private void OnCheckClicked()
        {
            if (_busy) return;
            StartCoroutine(CheckFlow());
        }

        private IEnumerator CheckFlow()
        {
            SetBusy(true, "正在检查更新...");

            using var req = UnityWebRequest.Get($"{FirServer}/{FirShort}");
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Fail($"检查更新失败：{req.error}");
                yield break;
            }

            ReleaseInfo info = null;
            string parseError = null;
            try { info = ParseRelease(req.downloadHandler.text); }
            catch (Exception ex) { parseError = ex.Message; }

            if (info == null)
            {
                Fail($"版本信息解析失败：{parseError}");
                yield break;
            }

            SetBusy(false);

            int localBuild = AppUpdater.Instance.GetInstalledVersionCode();
            string versions =
                $"服务器版本：{info.Version}（Build {info.Build}）\n" +
                $"当前版本：{AppUpdater.Instance.GetInstalledVersionName()}（Build {localBuild}）\n";
            string packageInfo =
                $"安装包大小：{FormatMB(info.FileSize)}\n" +
                $"更新说明：{info.Changelog}\n\n" +
                "安装时应用会自动关闭，完成后自动重启。";

            if (info.Build > localBuild)
            {
                ShowResultDialog("发现新版本", versions + packageInfo,
                    "下载并安装", () => StartDownload(info, forceRedownload: false));
            }
            else if (info.Build == localBuild)
            {
                ShowResultDialog("已是最新版本",
                    versions + "\n服务器版本与当前版本一致，无需下载。\n如需修复安装，可强制重新下载并安装。\n\n" + packageInfo,
                    "强制重新安装", () => StartDownload(info, forceRedownload: true));
            }
            else
            {
                ShowResultDialog("当前版本较新", versions + "\n服务器上的版本比当前安装的版本旧，系统不允许降级安装。");
            }
        }

        /// <summary>
        /// 版本检查结果对话框；已注册时底部额外提供「删除注册数据」按钮。
        /// confirmText 为空时只有关闭按钮。
        /// </summary>
        private void ShowResultDialog(string title, string message, string confirmText = null, Action onConfirm = null)
        {
            var buttons = new List<(string, Action, Color)> { (confirmText != null ? "取消" : "确定", null, NeutralColor) };

            var deviceInfo = AppStateManager.Instance.DeviceInfo5S;
            if (deviceInfo != null && deviceInfo.IsValid())
                buttons.Add((DeleteRegisterLabel, ConfirmDeleteRegisterInfo, DangerColor));

            if (confirmText != null)
                buttons.Add((confirmText, onConfirm, PrimaryColor));

            ShowDialog(title, message, buttons.ToArray());
        }

        private void ConfirmDeleteRegisterInfo()
        {
            ShowDialog("确认删除",
                "确定要删除本机的设备注册数据吗？\n删除后将返回登录页，需要重新点击「开始注册」。",
                ("取消", null, NeutralColor),
                ("删除", LoginPanel.ClearRegisterInfoAndReturnToLogin, DangerColor));
        }

        private void StartDownload(ReleaseInfo info, bool forceRedownload)
        {
            // 下载文件 + 安装时系统解包各占一份空间
            long required = info.FileSize * 2;
            if (GetAvailableBytes() < required)
            {
                Fail($"存储空间不足，至少需要 {FormatMB(required)} 可用空间");
                return;
            }

            SetBusy(true, "准备下载...");
            AppUpdater.Instance.StartUpdate(info.DownloadUrl,
                onProgress: (progress, state) =>
                {
                    _label.text = state == AppUpdater.UpdateState.Installing
                        ? "正在安装，完成后自动重启..."
                        : $"下载中 {progress:P0}（{FormatMB((long)(progress * info.FileSize))} / {FormatMB(info.FileSize)}）";
                },
                onComplete: () => _label.text = "安装完成，应用即将重启...",
                onError: err => Fail($"更新失败：{err}"),
                forceRedownload: forceRedownload,
                apkFileName: $"update_{info.Build}.apk");
        }

        private ReleaseInfo ParseRelease(string json)
        {
            JToken app = JObject.Parse(json)["app"];
            JToken master = app?["releases"]?["master"];
            if (master == null) throw new Exception("没有找到已发布的版本");
            if (app.Value<bool>("has_passwd")) throw new Exception("下载需要密码，请在 fir 后台关闭下载密码");

            string appId = app.Value<string>("id");
            string token = app.Value<string>("token");
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(token))
                throw new Exception("缺少 app id 或下载 token");

            return new ReleaseInfo
            {
                Version = master.Value<string>("version"),
                Build = int.Parse(master.Value<string>("build")),
                Changelog = master.Value<string>("changelog"),
                FileSize = master.Value<long>("fsize"),
                DownloadUrl = $"{FirServer}/apps/{appId}/install?short={FirShort}&download_token={token}",
            };
        }

        private void Fail(string message)
        {
            Debug.LogError($"[FirApkUpdateButton] {message}");
            SetBusy(false);
            ShowDialog("温馨提示", message, ("确定", null, PrimaryColor));
        }

        private void SetBusy(bool busy, string label = IdleLabel)
        {
            _busy = busy;
            _button.interactable = !busy;
            _label.text = label;
        }

        private static string FormatMB(long bytes) => $"{bytes / 1048576f:F0}MB";

        private static long GetAvailableBytes()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var statFs = new AndroidJavaObject("android.os.StatFs", Application.temporaryCachePath);
            return statFs.Call<long>("getAvailableBytes");
#else
            return long.MaxValue;
#endif
        }
    }
}
