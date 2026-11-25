using System;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 1~4 号预注册比分牌（测试门店 store22）。
/// Editor / Release 均可使用：写入 DeviceInfo5S 缓存，跳过注册接口。
/// </summary>
public static class ScoreCardTestDevicePresets
{
    public const int Count = 4;

    static readonly string[] DeviceCodes =
    {
        "20260509174531",
        "20260509174649",
        "20260509174744",
        "20260509174815",
    };

    static readonly string[] DeviceJson =
    {
        "{\"publish_topic\":\"/store22/20260509174538845/android/update\",\"device_name\":\"20260509174538845\",\"umeng_flag\":\"store22-20260509174531\",\"product_key\":\"store22\",\"store_type\":0,\"id\":1,\"subscribe_topic\":\"/store22/20260509174538845/android/get\",\"uri\":\"tcp://lz0000000000000022.qmlz.store:1883\",\"device_pass\":\"cf82eb\"}",
        "{\"publish_topic\":\"/store22/20260509174656819/android/update\",\"device_name\":\"20260509174656819\",\"umeng_flag\":\"store22-20260509174649\",\"product_key\":\"store22\",\"store_type\":0,\"id\":2,\"subscribe_topic\":\"/store22/20260509174656819/android/get\",\"uri\":\"tcp://lz0000000000000022.qmlz.store:1883\",\"device_pass\":\"ea48f5\"}",
        "{\"publish_topic\":\"/store22/20260509174752474/android/update\",\"device_name\":\"20260509174752474\",\"umeng_flag\":\"store22-20260509174744\",\"product_key\":\"store22\",\"store_type\":0,\"id\":3,\"subscribe_topic\":\"/store22/20260509174752474/android/get\",\"uri\":\"tcp://lz0000000000000022.qmlz.store:1883\",\"device_pass\":\"773e75\"}",
        "{\"publish_topic\":\"/store22/20260509174823380/android/update\",\"device_name\":\"20260509174823380\",\"umeng_flag\":\"store22-20260509174815\",\"product_key\":\"store22\",\"store_type\":0,\"id\":4,\"subscribe_topic\":\"/store22/20260509174823380/android/get\",\"uri\":\"tcp://lz0000000000000022.qmlz.store:1883\",\"device_pass\":\"7beede\"}",
    };

    /// <summary>预设写入成功后请求进入大厅。由 RegisterDevicePanel 订阅。</summary>
    public static event Action EnterHallRequested;

    /// <summary>清除本地注册并回登录/注册页。由 LoginPanel 订阅。</summary>
    public static event Action ClearRegisterRequested;

    public static string GetDeviceCode(int index)
    {
        if (index < 0 || index >= DeviceCodes.Length)
            return "";
        return DeviceCodes[index];
    }

    public static string GetButtonLabel(int index)
    {
        if (index < 0 || index >= Count)
            return "";
        return $"{index + 1}号预注册  {GetDeviceCode(index)}";
    }

    /// <summary>
    /// 写入本地设备缓存。成功返回 true；不自动进大厅——调用方自行 Notify / RequestEnterHall。
    /// </summary>
    public static bool TryApply(int index, out string errorMessage)
    {
        errorMessage = null;
        if (index < 0 || index >= DeviceJson.Length)
        {
            errorMessage = "预注册设备索引越界";
            return false;
        }

        DeviceInfo5S deviceInfo;
        try
        {
            deviceInfo = JsonConvert.DeserializeObject<DeviceInfo5S>(DeviceJson[index]);
        }
        catch (Exception e)
        {
            errorMessage = "预注册数据无效: " + e.Message;
            Debug.LogError($"[ScoreCardTestDevicePresets] JSON 反序列化失败 index={index}: {e.Message}");
            return false;
        }

        if (deviceInfo == null || !deviceInfo.IsValid())
        {
            errorMessage = deviceInfo == null ? "预注册数据为空" : deviceInfo.GetInvalidDataInfo();
            Debug.LogError($"[ScoreCardTestDevicePresets] 数据无效 index={index}: {errorMessage}");
            return false;
        }

        deviceInfo.PrintDeviceInfo($"[ScoreCardTestDevicePresets] 应用 {index + 1}号");
        Debug.Log($"[ScoreCardTestDevicePresets] 应用 {index + 1}号，跳过注册接口，写入缓存");
        AppStateManager.Instance.DeviceInfo5S = deviceInfo;
        return true;
    }

    public static void RequestEnterHall()
    {
        EnterHallRequested?.Invoke();
    }

    public static void RequestClearRegisterAndReturnToLogin()
    {
        ClearRegisterRequested?.Invoke();
    }
}
