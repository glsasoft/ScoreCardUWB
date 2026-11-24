package cn.com.glsa.app.fightgame;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.util.Log;

/**
 * APK 替换完成后自动重启广播接收器。
 *
 * 普通应用 uid 在 Android 10+ 不能从 MY_PACKAGE_REPLACED 直接 startActivity
 * （会被 Abort background activity starts）。因此改为 AlarmManager 延迟 PendingIntent 拉起。
 */
public class PackageReceiver extends BroadcastReceiver {

    private static final String TAG = "PackageReceiver";
    private static final int REQUEST_CODE = 0x5712;
    private static final long LAUNCH_DELAY_MS = 800;

    @Override
    public void onReceive(Context context, Intent intent) {
        if (intent == null || intent.getAction() == null) return;

        String action = intent.getAction();
        Log.i(TAG, "onReceive: action=" + action);

        if (!Intent.ACTION_MY_PACKAGE_REPLACED.equals(action)) {
            return;
        }

        Log.i(TAG, "本应用已升级完成，准备拉起 LAUNCHER...");
        Intent launchIntent = context.getPackageManager()
                .getLaunchIntentForPackage(context.getPackageName());
        if (launchIntent == null) {
            Log.e(TAG, "未找到 LAUNCHER Activity，无法自动启动");
            return;
        }

        launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        launchIntent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
        launchIntent.addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP);

        // 先直接尝试（system uid 可成功）；失败也不影响后续闹钟兜底
        try {
            context.startActivity(launchIntent);
            Log.i(TAG, "直接 startActivity 已调用: " + launchIntent.getComponent());
        } catch (Exception e) {
            Log.w(TAG, "直接 startActivity 失败: " + e.getMessage());
        }

        scheduleLaunch(context, launchIntent);
    }

    private static void scheduleLaunch(Context context, Intent launchIntent) {
        try {
            AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
            if (alarmManager == null) {
                Log.e(TAG, "AlarmManager 不可用");
                return;
            }

            int flags = PendingIntent.FLAG_UPDATE_CURRENT;
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                flags |= PendingIntent.FLAG_IMMUTABLE;
            }

            PendingIntent pendingIntent = PendingIntent.getActivity(
                    context, REQUEST_CODE, launchIntent, flags);

            long triggerAt = System.currentTimeMillis() + LAUNCH_DELAY_MS;
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                alarmManager.setExactAndAllowWhileIdle(
                        AlarmManager.RTC_WAKEUP, triggerAt, pendingIntent);
            } else {
                alarmManager.setExact(AlarmManager.RTC_WAKEUP, triggerAt, pendingIntent);
            }

            Log.i(TAG, "已安排 " + LAUNCH_DELAY_MS + "ms 后通过 AlarmManager 拉起");
        } catch (Exception e) {
            Log.e(TAG, "安排 AlarmManager 拉起失败: " + e.getMessage(), e);
        }
    }
}
