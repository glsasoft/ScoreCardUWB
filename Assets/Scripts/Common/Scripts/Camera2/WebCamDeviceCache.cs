using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// <see cref="WebCamTexture.devices"/> 枚举结果缓存（非 Android 平台：Windows/Editor 等）。
///
/// 该属性每次访问都会触发系统摄像头驱动查询（Windows 上走 DirectShow/Media Foundation），
/// 装有 OBS Virtual Camera / NVIDIA Broadcast 等虚拟摄像头驱动时尤其慢，单次耗时可达数百毫秒。
/// 若同一时间窗口内多处重复调用（例如 <c>ScoreCardWebCamPage.BuildCameraSlotList</c> 与
/// <c>WebCamController.InitWebCam</c> 各查询一次），耗时会成倍叠加，表现为 UI 切换瞬间卡顿。
/// 缓存后同一 TTL 窗口内只真正查询一次，其余调用直接复用结果。
/// </summary>
public static class WebCamDeviceCache
{
    const float CacheTtlSeconds = 5f;

    static WebCamDevice[] s_devices;
    static float s_cachedAtRealtime = -1f;

    public static WebCamDevice[] GetDevices(bool forceRefresh = false)
    {
        float now = Time.realtimeSinceStartup;
        if (!forceRefresh && s_devices != null && now - s_cachedAtRealtime < CacheTtlSeconds)
            return s_devices;

        var sw = Stopwatch.StartNew();
        s_devices = WebCamTexture.devices;
        Debug.Log($"[WebCamDeviceCache][耗时] 真实查询 WebCamTexture.devices 耗时={sw.ElapsedMilliseconds}ms count={s_devices.Length}");
        s_cachedAtRealtime = now;
        return s_devices;
    }
}
