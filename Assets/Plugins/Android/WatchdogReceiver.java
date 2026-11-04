package cn.com.glsa.app.fightgame;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.util.Log;

/**
 * 存活看门狗：通过 AlarmManager 周期性检查 MainActivity 是否还在（进程是否被 OOM 杀掉），
 * 不在则重新拉起。
 *
 * 为什么不用"HOME Activity 死亡后系统自动拉起"：实测（adb kill -9 主进程）验证过，
 * 这台设备上进程死亡后系统并不会自动重新拉起 HOME，必须自己兜底。
 *
 * 为什么用 AlarmManager 而不是常驻 Service：AlarmManager 的定时任务由 system_server
 * 管理，不依赖本 app 进程存活；主进程被杀后，之前已排好的定时器依然会按时触发，
 * 触发时系统会为本包重新拉起一个进程来处理广播，从而带出重新拉起 MainActivity 的机会。
 *
 * 5 秒链靠 onReceive 里自我续期，内存极度紧张时某次广播可能没送达（主线程卡死或进程先被杀），
 * 链条就此断掉（实测：lowmemorykiller 杀进程后再无任何拉起）。因此另设一个由 AlarmManager
 * 自己重复触发的兜底闹钟（系统限制最短 60 秒），它触发时同样进 onReceive，顺带把 5 秒链接回来。
 */
public class WatchdogReceiver extends BroadcastReceiver {

    private static final String TAG = "WatchdogReceiver";
    private static final long CHECK_INTERVAL_MS = 5000;
    private static final int REQUEST_CODE = 0x5710;
    private static final long BACKUP_INTERVAL_MS = 60000;
    private static final int BACKUP_REQUEST_CODE = 0x5713;

    @Override
    public void onReceive(Context context, Intent intent) {
        scheduleNext(context);

        if (!MainActivity.isRunning) {
            Log.w(TAG, "MainActivity 未存活，重新拉起");
            Intent launchIntent = new Intent(context, MainActivity.class);
            launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            context.startActivity(launchIntent);
        }
    }

    public static void scheduleNext(Context context) {
        AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
        if (alarmManager == null) return;

        PendingIntent pendingIntent = PendingIntent.getBroadcast(
                context, REQUEST_CODE, watchdogIntent(context),
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        long triggerAt = System.currentTimeMillis() + CHECK_INTERVAL_MS;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            alarmManager.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAt, pendingIntent);
        } else {
            alarmManager.setExact(AlarmManager.RTC_WAKEUP, triggerAt, pendingIntent);
        }
    }

    /** 兜底重复闹钟；同一 PendingIntent 重复调用只会替换，不会叠加。 */
    public static void scheduleBackup(Context context) {
        AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
        if (alarmManager == null) return;

        PendingIntent pendingIntent = PendingIntent.getBroadcast(
                context, BACKUP_REQUEST_CODE, watchdogIntent(context),
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        alarmManager.setRepeating(AlarmManager.RTC_WAKEUP,
                System.currentTimeMillis() + BACKUP_INTERVAL_MS, BACKUP_INTERVAL_MS, pendingIntent);
    }

    // 走前台广播队列：后台队列会被其他应用的慢接收器堵住（开机时实测被卡 60 秒）
    private static Intent watchdogIntent(Context context) {
        Intent intent = new Intent(context, WatchdogReceiver.class);
        intent.addFlags(Intent.FLAG_RECEIVER_FOREGROUND);
        return intent;
    }
}
