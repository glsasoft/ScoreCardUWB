using System;
using UnityEngine;

/// <summary>
/// 设备唯一标识及设备信息工具类（静态）。
///
/// Android 端通过 <c>DeviceIdentifierBridge.java</c> 实现三重降级策略：
///   1. Android ID（<c>Settings.Secure.ANDROID_ID</c>）—— 出厂写入，重置后变化
///   2. 持久化 GUID（首次生成后写入 SharedPreferences）—— 卸载重装才变化
///   3. 临时 UUID（进程内稳定，重启后变化，仅兜底）
///
/// Editor / PC 端使用 Unity <c>SystemInfo.deviceUniqueIdentifier</c>。
///
/// 所有属性均带缓存，进程内只查询一次。
///
/// 使用示例：
/// <code>
///   string id = DeviceIdentifier.DeviceId;
///   Debug.Log($"设备ID: {id}  版本: {DeviceIdentifier.VersionName}({DeviceIdentifier.VersionCode})");
/// </code>
/// </summary>
public static class DeviceIdentifier
{
    // ════════════════════════════════════════════════════════════════════
    //  常量
    // ════════════════════════════════════════════════════════════════════

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string BridgeClass = "com.glsa.mqttscorecard5s.unity.DeviceIdentifierBridge";
#endif

    // ════════════════════════════════════════════════════════════════════
    //  缓存字段（首次访问时填充，进程内复用）
    // ════════════════════════════════════════════════════════════════════

    private static string _deviceId;
    private static string _androidId;
    private static string _packageName;
    private static string _versionName;
    private static int    _versionCode = -1;
    private static int    _sdkInt      = -1;
    private static string _deviceModel;
    private static string _deviceBrand;
    private static string _hardware;

    // ════════════════════════════════════════════════════════════════════
    //  设备唯一标识
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 设备唯一标识（三重降级，带来源前缀）。
    /// <list type="bullet">
    ///   <item><c>aid_xxxxx</c>  — 来自 Android ID</item>
    ///   <item><c>guid_xxxxx</c> — 来自持久化 GUID（卸载重装后变化）</item>
    ///   <item><c>tmp_xxxxx</c>  — 来自临时 UUID（重启后变化，仅兜底）</item>
    ///   <item><c>editor_xxxxx</c> — Editor / PC 下使用 SystemInfo</item>
    /// </list>
    /// </summary>
    public static string DeviceId
    {
        get
        {
            if (_deviceId != null) return _deviceId;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var ctx = GetContext();
                using var jc  = new AndroidJavaClass(BridgeClass);
                _deviceId = jc.CallStatic<string>("getDeviceId", ctx);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeviceIdentifier] GetDeviceId failed: {ex.Message}");
                _deviceId = "fallback_" + SystemInfo.deviceUniqueIdentifier;
            }
#else
            _deviceId = "editor_" + SystemInfo.deviceUniqueIdentifier;
#endif
            Debug.Log($"[DeviceIdentifier] DeviceId = {_deviceId}");
            return _deviceId;
        }
    }

    /// <summary>
    /// 原始 Android ID（不含前缀，可能为空字符串）。
    /// 用于需要与服务端旧版逻辑兼容的场景。
    /// </summary>
    public static string AndroidId
    {
        get
        {
            if (_androidId != null) return _androidId;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var ctx = GetContext();
                using var jc  = new AndroidJavaClass(BridgeClass);
                _androidId = jc.CallStatic<string>("getAndroidId", ctx) ?? "";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeviceIdentifier] GetAndroidId failed: {ex.Message}");
                _androidId = "";
            }
#else
            _androidId = "";
#endif
            return _androidId;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  应用信息
    // ════════════════════════════════════════════════════════════════════

    /// <summary>应用包名（如 <c>cn.com.glsa.app.fightgame</c>）。</summary>
    public static string PackageName
    {
        get
        {
            if (_packageName != null) return _packageName;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var ctx = GetContext();
                using var jc  = new AndroidJavaClass(BridgeClass);
                _packageName = jc.CallStatic<string>("getPackageName", ctx) ?? "";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeviceIdentifier] GetPackageName failed: {ex.Message}");
                _packageName = Application.identifier;
            }
#else
            _packageName = Application.identifier;
#endif
            return _packageName;
        }
    }

    /// <summary>应用版本名称（如 <c>"1.2.3"</c>）。</summary>
    public static string VersionName
    {
        get
        {
            if (_versionName != null) return _versionName;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var ctx = GetContext();
                using var jc  = new AndroidJavaClass(BridgeClass);
                _versionName = jc.CallStatic<string>("getVersionName", ctx) ?? Application.version;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeviceIdentifier] GetVersionName failed: {ex.Message}");
                _versionName = Application.version;
            }
#else
            _versionName = Application.version;
#endif
            return _versionName;
        }
    }

    /// <summary>应用版本号（整型，如 <c>123</c>）。</summary>
    public static int VersionCode
    {
        get
        {
            if (_versionCode >= 0) return _versionCode;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var ctx = GetContext();
                using var jc  = new AndroidJavaClass(BridgeClass);
                _versionCode = jc.CallStatic<int>("getVersionCode", ctx);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeviceIdentifier] GetVersionCode failed: {ex.Message}");
                _versionCode = 0;
            }
#else
            if (!int.TryParse(Application.version.Replace(".", ""), out _versionCode))
                _versionCode = 0;
#endif
            return _versionCode;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  系统信息
    // ════════════════════════════════════════════════════════════════════

    /// <summary>Android SDK 版本号（如 <c>31</c> = Android 12）。Editor 返回 0。</summary>
    public static int SdkInt
    {
        get
        {
            if (_sdkInt >= 0) return _sdkInt;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var jc = new AndroidJavaClass(BridgeClass);
                _sdkInt = jc.CallStatic<int>("getSdkInt");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeviceIdentifier] GetSdkInt failed: {ex.Message}");
                _sdkInt = 0;
            }
#else
            _sdkInt = 0;
#endif
            return _sdkInt;
        }
    }

    /// <summary>设备型号（如 <c>"RK3588"</c>）。</summary>
    public static string DeviceModel
    {
        get
        {
            if (_deviceModel != null) return _deviceModel;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var jc = new AndroidJavaClass(BridgeClass);
                _deviceModel = jc.CallStatic<string>("getDeviceModel") ?? SystemInfo.deviceModel;
            }
            catch { _deviceModel = SystemInfo.deviceModel; }
#else
            _deviceModel = SystemInfo.deviceModel;
#endif
            return _deviceModel;
        }
    }

    /// <summary>设备品牌（如 <c>"Rockchip"</c>）。</summary>
    public static string DeviceBrand
    {
        get
        {
            if (_deviceBrand != null) return _deviceBrand;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var jc = new AndroidJavaClass(BridgeClass);
                _deviceBrand = jc.CallStatic<string>("getDeviceBrand") ?? "";
            }
            catch { _deviceBrand = ""; }
#else
            _deviceBrand = "";
#endif
            return _deviceBrand;
        }
    }

    /// <summary>设备硬件名称（如 <c>"rk30board"</c>）。</summary>
    public static string Hardware
    {
        get
        {
            if (_hardware != null) return _hardware;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var jc = new AndroidJavaClass(BridgeClass);
                _hardware = jc.CallStatic<string>("getHardware") ?? "";
            }
            catch { _hardware = ""; }
#else
            _hardware = "";
#endif
            return _hardware;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  工具方法
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 清除所有缓存，下次访问属性时重新从 Android 侧查询。
    /// 通常仅用于测试目的。
    /// </summary>
    public static void ClearCache()
    {
        _deviceId    = null;
        _androidId   = null;
        _packageName = null;
        _versionName = null;
        _versionCode = -1;
        _sdkInt      = -1;
        _deviceModel = null;
        _deviceBrand = null;
        _hardware    = null;
        Debug.Log("[DeviceIdentifier] 缓存已清除");
    }

    /// <summary>
    /// 清除 Android 端保存的持久化 GUID。
    /// 下次调用 <see cref="DeviceId"/> 时将重新生成新 GUID。
    /// 通常仅用于测试目的。
    /// </summary>
    public static void ClearSavedGuid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var ctx = GetContext();
            using var jc  = new AndroidJavaClass(BridgeClass);
            jc.CallStatic("clearSavedGuid", ctx);
            _deviceId = null; // 同时清除缓存，下次重新生成
            Debug.Log("[DeviceIdentifier] 持久化 GUID 已清除");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DeviceIdentifier] ClearSavedGuid failed: {ex.Message}");
        }
#else
        Debug.Log("[DeviceIdentifier] ClearSavedGuid (Editor mock)");
#endif
    }

    /// <summary>
    /// 输出所有设备信息到 Debug.Log（调试用）。
    /// </summary>
    public static void LogAll()
    {
        Debug.Log(
            $"[DeviceIdentifier]\n" +
            $"  DeviceId    = {DeviceId}\n" +
            $"  AndroidId   = {AndroidId}\n" +
            $"  PackageName = {PackageName}\n" +
            $"  VersionName = {VersionName}  VersionCode = {VersionCode}\n" +
            $"  SdkInt      = {SdkInt}\n" +
            $"  DeviceBrand = {DeviceBrand}  DeviceModel = {DeviceModel}\n" +
            $"  Hardware    = {Hardware}");
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有工具
    // ════════════════════════════════════════════════════════════════════

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject GetContext()
    {
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }
#endif
}
