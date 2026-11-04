package com.glsa.mqttscorecard5s.unity;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.SharedPreferences;
import android.provider.Settings;
import android.util.Log;

import java.util.UUID;

/**
 * 设备唯一标识 Android 端桥接层。
 *
 * 三重降级策略（优先级从高到低）：
 *   1. Android ID（Settings.Secure.ANDROID_ID）
 *      - 出厂写入，恢复出厂设置后会重置。
 *      - 存在已知无效值 "9774d56d682e549c"（部分模拟器/早期机型），自动过滤。
 *   2. 持久化 GUID
 *      - 首次调用时随机生成 UUID，保存到应用私有 SharedPreferences。
 *      - 仅在 APP 被卸载重装后重置，适合设备注册场景。
 *   3. 临时 UUID（兜底）
 *      - 进程内保持不变，但重启后变化，仅在前两种均失败时使用。
 *
 * 同时提供版本号、包名等常用设备信息查询，供 Unity 侧统一调用。
 */
public class DeviceIdentifierBridge {

    private static final String TAG        = "DeviceIdentifierBridge";
    private static final String PREFS_NAME = "glsa_device_prefs";
    private static final String KEY_GUID   = "device_guid";

    // 已知无效 Android ID（Android 2.2 仿真器 / 部分机型出厂值）
    private static final String INVALID_ANDROID_ID = "9774d56d682e549c";

    // ────────────────────────────────────────────────────────────────────
    //  设备唯一标识
    // ────────────────────────────────────────────────────────────────────

    /**
     * 获取设备唯一标识（三重降级，带 "aid_" / "guid_" / "tmp_" 前缀便于区分来源）。
     *
     * @param context Android Context（UnityPlayer.currentActivity）
     * @return 非空唯一标识字符串
     */
    @SuppressLint("HardwareIds")
    public static String getDeviceId(Context context) {
        // ── 1. Android ID ────────────────────────────────────────────────
        try {
            String androidId = Settings.Secure.getString(
                    context.getContentResolver(), Settings.Secure.ANDROID_ID);
            if (androidId != null
                    && !androidId.isEmpty()
                    && !INVALID_ANDROID_ID.equals(androidId)) {
                String id = "aid_" + androidId;
                Log.i(TAG, "getDeviceId (Android ID): " + id);
                return id;
            }
            Log.w(TAG, "Android ID 无效或为空: " + androidId);
        } catch (Exception e) {
            Log.w(TAG, "Android ID 获取异常: " + e.getMessage());
        }

        // ── 2. 持久化 GUID ────────────────────────────────────────────────
        try {
            SharedPreferences prefs = context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
            String saved = prefs.getString(KEY_GUID, null);
            if (saved != null && !saved.isEmpty()) {
                Log.i(TAG, "getDeviceId (Saved GUID): " + saved);
                return saved;
            }
            // 首次生成
            String newGuid = "guid_" + UUID.randomUUID().toString().replace("-", "");
            prefs.edit().putString(KEY_GUID, newGuid).apply();
            Log.i(TAG, "getDeviceId (New GUID): " + newGuid);
            return newGuid;
        } catch (Exception e) {
            Log.w(TAG, "持久化 GUID 失败: " + e.getMessage());
        }

        // ── 3. 临时 UUID（兜底） ──────────────────────────────────────────
        String temp = "tmp_" + UUID.randomUUID().toString().replace("-", "");
        Log.w(TAG, "getDeviceId (Temp UUID, NOT persistent): " + temp);
        return temp;
    }

    /**
     * 获取原始 Android ID（不加前缀，可能为空）。
     * 常用于与服务端旧版逻辑保持兼容。
     */
    @SuppressLint("HardwareIds")
    public static String getAndroidId(Context context) {
        try {
            String id = Settings.Secure.getString(
                    context.getContentResolver(), Settings.Secure.ANDROID_ID);
            return id != null ? id : "";
        } catch (Exception e) {
            Log.w(TAG, "getAndroidId 异常: " + e.getMessage());
            return "";
        }
    }

    /**
     * 清除已保存的 GUID，下次调用 {@link #getDeviceId} 时重新生成。
     * 通常仅用于测试目的。
     */
    public static void clearSavedGuid(Context context) {
        try {
            context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
                    .edit().remove(KEY_GUID).apply();
            Log.i(TAG, "已清除保存的 GUID");
        } catch (Exception e) {
            Log.w(TAG, "clearSavedGuid 异常: " + e.getMessage());
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  应用信息
    // ────────────────────────────────────────────────────────────────────

    /** 获取应用包名。 */
    public static String getPackageName(Context context) {
        return context != null ? context.getPackageName() : "";
    }

    /** 获取应用版本名称（如 "1.2.3"）。 */
    public static String getVersionName(Context context) {
        try {
            return context.getPackageManager()
                    .getPackageInfo(context.getPackageName(), 0).versionName;
        } catch (Exception e) {
            Log.w(TAG, "getVersionName 异常: " + e.getMessage());
            return "";
        }
    }

    /** 获取应用版本号（整型，如 123）。 */
    public static int getVersionCode(Context context) {
        try {
            return context.getPackageManager()
                    .getPackageInfo(context.getPackageName(), 0).versionCode;
        } catch (Exception e) {
            Log.w(TAG, "getVersionCode 异常: " + e.getMessage());
            return 0;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  系统信息
    // ────────────────────────────────────────────────────────────────────

    /** 获取 Android SDK 版本号（如 31 = Android 12）。 */
    public static int getSdkInt() {
        return android.os.Build.VERSION.SDK_INT;
    }

    /** 获取设备型号（如 "RK3588"）。 */
    public static String getDeviceModel() {
        return android.os.Build.MODEL != null ? android.os.Build.MODEL : "";
    }

    /** 获取设备品牌（如 "Rockchip"）。 */
    public static String getDeviceBrand() {
        return android.os.Build.BRAND != null ? android.os.Build.BRAND : "";
    }

    /** 获取设备硬件名称（如 "rk30board"）。 */
    public static String getHardware() {
        return android.os.Build.HARDWARE != null ? android.os.Build.HARDWARE : "";
    }
}
