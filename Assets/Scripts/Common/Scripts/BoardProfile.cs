using UnityEngine;

/// <summary>
/// 开发板硬件适配：根据系统属性 ro.product.product.model 自动识别板子型号，
/// 给出对应的 LORA 串口路径与耳机声卡（外置声卡）过滤关键词。
///
/// 新增/修改开发板只需改下面的 <see cref="Table"/> 一行即可：
///   型号关键词(小写) → (串口路径, 耳机声卡过滤关键词)
///
/// 已适配：
///   双赞 SZ358D : 串口 /dev/ttyS4，耳机声卡 card 0 [rockchipes8316c]
///   触想 CX3588 : 串口 /dev/ttyS0，耳机声卡 card 1 [rockchipes8388]
/// </summary>
public static class BoardProfile
{
    public readonly struct Profile
    {
        public readonly string SerialPort;
        public readonly string AudioCardSelector;

        public Profile(string serialPort, string audioCardSelector)
        {
            SerialPort = serialPort;
            AudioCardSelector = audioCardSelector;
        }
    }

    // 型号关键词用 model.Contains(keyword) 做不区分大小写匹配，按顺序取首个命中。
    private static readonly (string ModelKeyword, Profile Profile)[] Table =
    {
        ("sz358d", new Profile("/dev/ttyS4", "rockchipes8316")),
        ("cx3588", new Profile("/dev/ttyS0", "rockchipes8388")),
    };

    // 未识别型号时的兜底：串口沿用历史默认，声卡留空（=不强制绑定，走系统默认 ALSA 设备）。
    private static readonly Profile Fallback = new Profile("/dev/ttyS0", "");

    private const string ModelProp = "ro.product.product.model";
    private const string SerialPortOverrideKey = "BoardProfile.SerialPortOverride";

    private static bool _resolved;
    private static string _model = "";
    private static Profile _current = Fallback;

    /// <summary>板型自动识别的默认 LORA 串口（不受手动覆盖影响）。</summary>
    public static string DefaultSerialPort => Resolve().SerialPort;

    /// <summary>实际使用的 LORA 串口：PlayerPrefs 手动覆盖优先，否则板型默认。</summary>
    public static string SerialPort
    {
        get
        {
            if (HasSerialPortOverride)
            {
                string ov = PlayerPrefs.GetString(SerialPortOverrideKey, "");
                if (!string.IsNullOrEmpty(ov)) return ov;
            }
            return Resolve().SerialPort;
        }
    }

    /// <summary>是否已通过 UIDev 等方式手动指定串口（持久化于 PlayerPrefs）。</summary>
    public static bool HasSerialPortOverride => PlayerPrefs.HasKey(SerialPortOverrideKey);

    /// <summary>手动覆盖的串口路径；未设置时为空。</summary>
    public static string SerialPortOverride => PlayerPrefs.GetString(SerialPortOverrideKey, "");

    /// <summary>保存手动串口并持久化，下次启动 <see cref="SerialPort"/> 优先使用该路径。</summary>
    public static void SetSerialPortOverride(string port)
    {
        if (string.IsNullOrEmpty(port))
        {
            ClearSerialPortOverride();
            return;
        }

        PlayerPrefs.SetString(SerialPortOverrideKey, port);
        PlayerPrefs.Save();
        Debug.Log($"[BoardProfile] 手动串口已保存: {port}（板型默认={DefaultSerialPort}，实际生效={SerialPort}）");
    }

    /// <summary>清除手动覆盖，恢复板型自动识别串口。</summary>
    public static void ClearSerialPortOverride()
    {
        if (!HasSerialPortOverride) return;

        PlayerPrefs.DeleteKey(SerialPortOverrideKey);
        PlayerPrefs.Save();
        Debug.Log($"[BoardProfile] 已清除手动串口，恢复板型默认: {DefaultSerialPort}");
    }

    /// <summary>当前板子的耳机声卡过滤关键词；未识别型号时为空。</summary>
    public static string AudioCardSelector => Resolve().AudioCardSelector;

    /// <summary>已识别到的型号字符串（原始值）。</summary>
    public static string DetectedModel { get { Resolve(); return _model; } }

    private static Profile Resolve()
    {
        if (_resolved) return _current;
        _resolved = true;

        _model = ReadModel();
        string key = (_model ?? "").Trim().ToLowerInvariant();

        _current = Fallback;
        foreach (var (kw, profile) in Table)
        {
            if (!string.IsNullOrEmpty(key) && key.Contains(kw))
            {
                _current = profile;
                break;
            }
        }

        Debug.Log($"[BoardProfile] model='{_model}' → 板型默认串口={_current.SerialPort} " +
                  (HasSerialPortOverride
                      ? $"手动覆盖={SerialPortOverride} → 实际串口={SerialPort} "
                      : $"实际串口={SerialPort} ") +
                  $"耳机声卡='{(string.IsNullOrEmpty(_current.AudioCardSelector) ? "(默认)" : _current.AudioCardSelector)}'");
        return _current;
    }

    private static string ReadModel()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var sp = new AndroidJavaClass("android.os.SystemProperties");
            string v = sp.CallStatic<string>("get", ModelProp, "");
            if (!string.IsNullOrEmpty(v)) return v;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[BoardProfile] 读取 {ModelProp} 失败，回退 Build.MODEL：{e.Message}");
        }
        try
        {
            using var build = new AndroidJavaClass("android.os.Build");
            return build.GetStatic<string>("MODEL") ?? "";
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[BoardProfile] 读取 Build.MODEL 失败：{e.Message}");
            return "";
        }
#else
        return "";
#endif
    }

    /// <summary>启动时主动识别并打印（可选，便于现场排查）。</summary>
    public static void LogProfile() => Resolve();
}
