package cn.com.glsa.app.fightgame;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Build;
import android.provider.Settings;
import android.util.Log;

/**
 * 开机拉起轮询：进程一旦被唤醒，用 RTC 精确闹钟每 3 秒拉一次，直到主界面在前台。
 *
 * AlarmManager 闹钟本身不能跨重启：开机后系统会清空所有 app 闹钟。
 * 所以本类只负责「进程已经起来之后」的细粒度兜底；进程怎么被拉起来见 BootJobService。
 *
 * 停条件：某次轮询时 MainActivity 已经在前台。之后用户按 Home 不再拉回。
 * 成功状态按 Settings.Global.BOOT_COUNT 记到 SharedPreferences，进程被杀后本轮开机也不会重入。
 */
public class BootLaunchReceiver extends BroadcastReceiver {

    private static final String TAG = "BootLaunchReceiver";
    static final String ACTION_POLL = "cn.com.glsa.app.fightgame.action.BOOT_LAUNCH_POLL";

    private static final String PREFS = "boot_launch";
    private static final String KEY_DONE_BOOT = "done_boot_count";

    private static final long FIRST_DELAY_MS = 3000;
    private static final long POLL_INTERVAL_MS = 3000;
    private static final int MAX_ATTEMPTS = 60;
    private static final int REQUEST_CODE = 0x5711;

    private static volatile boolean sReachedMainUi = false;
    private static volatile boolean sPolling = false;
    private static volatile int sAttempts = 0;

    @Override
    public void onReceive(Context context, Intent intent) {
        if (intent == null || intent.getAction() == null) return;

        String action = intent.getAction();
        Log.i(TAG, "onReceive: action=" + action);

        if (Intent.ACTION_BOOT_COMPLETED.equals(action)
                || Intent.ACTION_LOCKED_BOOT_COMPLETED.equals(action)
                || Intent.ACTION_USER_UNLOCKED.equals(action)
                || "android.intent.action.QUICKBOOT_POWERON".equals(action)) {
            startPolling(context);
            return;
        }

        if (!ACTION_POLL.equals(action)) return;
        onPollTick(context);
    }

    public static void startPollingIfNeeded(Context context) {
        if (hasReachedMainUiThisBoot(context)) return;
        startPolling(context);
    }

    public static void startPolling(Context context) {
        Context app = context.getApplicationContext();
        if (hasReachedMainUiThisBoot(app)) {
            Log.i(TAG, "本轮开机已进入主界面，跳过轮询");
            return;
        }
        if (sPolling) return;
        sPolling = true;
        sAttempts = 0;
        Log.i(TAG, "开始开机轮询");
        schedule(app, FIRST_DELAY_MS);
    }

    private static void onPollTick(Context context) {
        Context app = context.getApplicationContext();
        if (hasReachedMainUiThisBoot(app)) {
            stopPolling(app);
            return;
        }

        if (MainActivity.isForeground) {
            Log.i(TAG, "MainActivity 已在前台，停止开机轮询");
            markReachedMainUi(app);
            stopPolling(app);
            return;
        }

        sAttempts++;
        if (sAttempts > MAX_ATTEMPTS) {
            Log.w(TAG, "开机轮询超过次数上限，停止；等待 JobScheduler 下一轮再试");
            sPolling = false;
            cancel(app);
            return;
        }

        Log.i(TAG, "开机轮询第 " + sAttempts + " 次，拉起 MainActivity");
        bringMainActivityToFront(app);
        schedule(app, POLL_INTERVAL_MS);
    }

    static void bringMainActivityToFront(Context context) {
        try {
            Intent launchIntent = new Intent(context, MainActivity.class);
            launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            launchIntent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
            launchIntent.addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP);
            context.startActivity(launchIntent);
        } catch (Exception e) {
            Log.e(TAG, "拉起 MainActivity 失败: " + e.getMessage(), e);
        }
    }

    static boolean hasReachedMainUiThisBoot(Context context) {
        if (sReachedMainUi) return true;
        int boot = currentBootCount(context);
        if (boot <= 0) return false;
        int done = prefs(context).getInt(KEY_DONE_BOOT, -1);
        if (done == boot) {
            sReachedMainUi = true;
            return true;
        }
        return false;
    }

    private static void markReachedMainUi(Context context) {
        sReachedMainUi = true;
        int boot = currentBootCount(context);
        if (boot > 0) {
            prefs(context).edit().putInt(KEY_DONE_BOOT, boot).apply();
        }
    }

    private static int currentBootCount(Context context) {
        try {
            return Settings.Global.getInt(context.getContentResolver(), Settings.Global.BOOT_COUNT);
        } catch (Exception e) {
            return 0;
        }
    }

    private static SharedPreferences prefs(Context context) {
        Context storageContext = context;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            storageContext = context.createDeviceProtectedStorageContext();
        }
        return storageContext.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
    }

    private static PendingIntent pollPendingIntent(Context context) {
        Intent intent = new Intent(context, BootLaunchReceiver.class);
        intent.setAction(ACTION_POLL);
        // 后台广播队列开机时会被 BOOT_COMPLETED 卡住（实测 com.android.phone 接收器超时 60 秒），
        // 轮询必须走前台队列，否则拉起要等到 BOOT_COMPLETED 全部派发完。
        intent.addFlags(Intent.FLAG_RECEIVER_FOREGROUND);
        return PendingIntent.getBroadcast(
                context, REQUEST_CODE, intent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
    }

    private static void schedule(Context context, long delayMs) {
        AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
        if (alarmManager == null) return;

        // RTC 墙上时钟 + setExactAndAllowWhileIdle：Doze 下仍按秒级触发。
        // 系统 uid 不受普通应用待机桶限制，但闹钟仍不能跨重启，必须先有进程被拉起。
        long triggerAt = System.currentTimeMillis() + delayMs;
        PendingIntent pendingIntent = pollPendingIntent(context);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            alarmManager.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAt, pendingIntent);
        } else {
            alarmManager.setExact(AlarmManager.RTC_WAKEUP, triggerAt, pendingIntent);
        }
    }

    private static void stopPolling(Context context) {
        sPolling = false;
        cancel(context);
    }

    private static void cancel(Context context) {
        AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
        if (alarmManager == null) return;
        alarmManager.cancel(pollPendingIntent(context));
    }
}
