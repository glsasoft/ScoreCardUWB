package com.glsa.mqttscorecard5s.unity;

import android.content.Context;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.io.BufferedReader;
import java.io.InputStreamReader;

/**
 * APK 静默安装 Android 端桥接层。
 *
 * 依赖：APK 必须以系统签名安装（android:sharedUserId="android.uid.system"），
 * 否则 pm install -r 命令无系统权限，会返回 "Failure [INSTALL_FAILED_...]"。
 *
 * 安装完成后的应用重启由 PackageReceiver（MY_PACKAGE_REPLACED）负责，
 * 本类只负责执行安装命令并将结果回传 Unity。
 *
 * 回调方法（通过 UnitySendMessage 发送到 callbackObject）：
 *   OnApkInstallSuccess(string apkPath) — 安装成功
 *   OnApkInstallError(string errorMsg)  — 安装失败
 */
public class AppUpdaterBridge {

    private static final String TAG = "AppUpdaterBridge";

    // ────────────────────────────────────────────────────────────────────
    //  公开方法
    // ────────────────────────────────────────────────────────────────────

    /**
     * 在后台线程执行 pm install 静默安装 APK。
     *
     * 安装命令：{@code pm install -r -g -i <packageName> --user 0 <apkPath>}
     *   -r  : 替换已安装包（允许覆盖安装）
     *   -g  : 自动授予所有声明的运行时权限（新版 APK 无需用户手动授权）
     *   -i  : 指定安装者包名（与当前应用一致，避免被系统视为第三方安装）
     *   --user 0 : 安装给用户 0（主用户，嵌入式设备通常只有一个用户）
     *
     * @param context        Android Context（UnityPlayer.currentActivity）
     * @param apkPath        已下载 APK 的本地绝对路径
     * @param callbackObject Unity GameObject 名称（用于接收回调）
     */
    public static void installApk(final Context context,
                                   final String apkPath,
                                   final String callbackObject) {
        new Thread(() -> {
            try {
                String pkgName = context.getPackageName();

                // 构造安装命令
                String cmd = "pm install -r -g -i " + pkgName + " --user 0 " + apkPath;
                Log.i(TAG, "执行安装命令: " + cmd);

                CommandResult result = execShell(cmd);
                Log.i(TAG, "stdout: " + result.stdout);
                Log.i(TAG, "stderr: " + result.stderr);
                Log.i(TAG, "exitCode: " + result.exitCode);

                // pm install 成功时 stdout 包含 "Success"（不区分大小写）
                boolean success = result.exitCode == 0
                        || (result.stdout != null && result.stdout.toLowerCase().contains("success"));

                if (success) {
                    Log.i(TAG, "APK 安装成功，等待系统发送 MY_PACKAGE_REPLACED 广播...");
                    // 注意：此 sendToUnity 可能在系统杀掉进程前来不及送达，
                    // 真正的"安装后重启"由 PackageReceiver 保证。
                    sendToUnity(callbackObject, "OnApkInstallSuccess", apkPath);
                } else {
                    String err = buildErrorMessage(result);
                    Log.e(TAG, "APK 安装失败: " + err);
                    sendToUnity(callbackObject, "OnApkInstallError", err);
                }

            } catch (Exception e) {
                Log.e(TAG, "installApk 异常: " + e.getMessage(), e);
                sendToUnity(callbackObject, "OnApkInstallError",
                        "exception: " + e.getMessage());
            }
        }, "AppUpdater-Install").start();
    }

    /**
     * 获取当前已安装 APK 的版本号（versionCode）。
     * 供 C# 侧对比服务端最新版本号，决定是否需要升级。
     */
    public static int getInstalledVersionCode(Context context) {
        try {
            return context.getPackageManager()
                    .getPackageInfo(context.getPackageName(), 0).versionCode;
        } catch (Exception e) {
            Log.w(TAG, "getInstalledVersionCode 失败: " + e.getMessage());
            return 0;
        }
    }

    /**
     * 获取当前已安装 APK 的版本名称（versionName）。
     */
    public static String getInstalledVersionName(Context context) {
        try {
            return context.getPackageManager()
                    .getPackageInfo(context.getPackageName(), 0).versionName;
        } catch (Exception e) {
            Log.w(TAG, "getInstalledVersionName 失败: " + e.getMessage());
            return "";
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  私有：Shell 执行
    // ────────────────────────────────────────────────────────────────────

    private static CommandResult execShell(String command) throws Exception {
        Process process = Runtime.getRuntime().exec(new String[]{"sh", "-c", command});

        // 并发读取 stdout 和 stderr，防止缓冲区满死锁
        final StringBuilder stdoutSb = new StringBuilder();
        final StringBuilder stderrSb = new StringBuilder();

        Thread stdoutReader = new Thread(() -> {
            try (BufferedReader br = new BufferedReader(
                    new InputStreamReader(process.getInputStream()))) {
                String line;
                while ((line = br.readLine()) != null) {
                    stdoutSb.append(line).append("\n");
                }
            } catch (Exception ignored) {}
        });

        Thread stderrReader = new Thread(() -> {
            try (BufferedReader br = new BufferedReader(
                    new InputStreamReader(process.getErrorStream()))) {
                String line;
                while ((line = br.readLine()) != null) {
                    stderrSb.append(line).append("\n");
                }
            } catch (Exception ignored) {}
        });

        stdoutReader.start();
        stderrReader.start();

        int exitCode = process.waitFor();
        stdoutReader.join(3000);
        stderrReader.join(3000);

        CommandResult result = new CommandResult();
        result.stdout   = stdoutSb.toString().trim();
        result.stderr   = stderrSb.toString().trim();
        result.exitCode = exitCode;
        return result;
    }

    private static String buildErrorMessage(CommandResult result) {
        StringBuilder sb = new StringBuilder();
        if (result.stderr != null && !result.stderr.isEmpty()) sb.append(result.stderr);
        if (result.stdout != null && !result.stdout.isEmpty()) {
            if (sb.length() > 0) sb.append(" | ");
            sb.append(result.stdout);
        }
        if (sb.length() == 0) sb.append("exitCode=").append(result.exitCode);
        return sb.toString();
    }

    // ────────────────────────────────────────────────────────────────────
    //  私有：Unity 消息
    // ────────────────────────────────────────────────────────────────────

    private static void sendToUnity(String gameObject, String method, String msg) {
        if (gameObject == null || gameObject.isEmpty()) return;
        try {
            UnityPlayer.UnitySendMessage(gameObject, method, msg != null ? msg : "");
        } catch (Throwable t) {
            Log.e(TAG, "sendToUnity failed method=" + method, t);
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  内部数据类
    // ────────────────────────────────────────────────────────────────────

    private static class CommandResult {
        String stdout;
        String stderr;
        int    exitCode;
    }
}
