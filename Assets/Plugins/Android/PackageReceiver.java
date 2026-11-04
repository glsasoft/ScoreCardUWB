package cn.com.glsa.app.fightgame;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;

/**
 * APK 替换完成后自动重启广播接收器。
 *
 * 监听 ACTION_MY_PACKAGE_REPLACED（仅接收本应用自身被替换的通知），
 * 收到后立即重新启动 MainActivity，实现"静默升级 → 自动重启"的完整流程。
 *
 * 典型升级流程：
 *   1. AppUpdaterBridge.installApk() 执行 pm install -r
 *   2. 系统完成 APK 替换，杀掉旧进程
 *   3. 系统向新版 APK 发送 MY_PACKAGE_REPLACED 广播
 *   4. 本 Receiver 在新版进程中被唤醒，启动 MainActivity
 */
public class PackageReceiver extends BroadcastReceiver {

    private static final String TAG = "PackageReceiver";

    @Override
    public void onReceive(Context context, Intent intent) {
        if (intent == null || intent.getAction() == null) return;

        String action = intent.getAction();
        Log.i(TAG, "onReceive: action=" + action);

        if (Intent.ACTION_MY_PACKAGE_REPLACED.equals(action)) {
            Log.i(TAG, "本应用已升级完成，正在重新启动 MainActivity...");
            try {
                Intent launchIntent = new Intent(context, MainActivity.class);
                launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                launchIntent.addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP);
                context.startActivity(launchIntent);
                Log.i(TAG, "MainActivity 已成功启动");
            } catch (Exception e) {
                Log.e(TAG, "重新启动 MainActivity 失败: " + e.getMessage(), e);
            }
        }
    }
}
