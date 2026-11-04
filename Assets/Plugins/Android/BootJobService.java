package cn.com.glsa.app.fightgame;

import android.app.job.JobInfo;
import android.app.job.JobParameters;
import android.app.job.JobScheduler;
import android.app.job.JobService;
import android.content.ComponentName;
import android.content.Context;
import android.os.Build;
import android.util.Log;

/**
 * 跨重启兜底：把任务持久化到 JobScheduler（system_server 磁盘）。
 *
 * BOOT_COMPLETED 可能丢失，AlarmManager 闹钟开机必被清空，两者都不够。
 * JobScheduler 由系统服务在开机后自行恢复已 persist 的任务并拉起本进程，
 * 不依赖本 app 是否收到开机广播。周期下限是平台的 15 分钟；关机超过一个周期后，
 * 开机时任务会视为逾期，通常在 JobScheduler 就绪后很快执行一次。
 *
 * 真正秒级拉起仍交给 BootLaunchReceiver 的 RTC 轮询。
 */
public class BootJobService extends JobService {

    private static final String TAG = "BootJobService";
    private static final int JOB_ID = 0x5712;
    private static final long PERIOD_MS = 15 * 60 * 1000L;

    public static void ensureScheduled(Context context) {
        JobScheduler scheduler = (JobScheduler) context.getSystemService(Context.JOB_SCHEDULER_SERVICE);
        if (scheduler == null) return;

        if (hasPendingJob(scheduler)) return;

        long period = PERIOD_MS;
        if (Build.VERSION.SDK_INT >= 24) {
            period = JobInfo.getMinPeriodMillis();
        }

        JobInfo job = new JobInfo.Builder(JOB_ID, new ComponentName(context, BootJobService.class))
                .setPersisted(true)
                .setPeriodic(period)
                .setBackoffCriteria(5000, JobInfo.BACKOFF_POLICY_LINEAR)
                .build();
        int result = scheduler.schedule(job);
        Log.i(TAG, "schedule persisted boot job result=" + result);
    }

    private static boolean hasPendingJob(JobScheduler scheduler) {
        if (Build.VERSION.SDK_INT >= 24) {
            return scheduler.getPendingJob(JOB_ID) != null;
        }
        java.util.List<JobInfo> jobs = scheduler.getAllPendingJobs();
        if (jobs == null) return false;
        for (JobInfo info : jobs) {
            if (info.getId() == JOB_ID) return true;
        }
        return false;
    }

    @Override
    public boolean onStartJob(JobParameters params) {
        Log.i(TAG, "onStartJob：唤醒开机轮询");
        BootLaunchReceiver.startPollingIfNeeded(this);
        jobFinished(params, false);
        return false;
    }

    @Override
    public boolean onStopJob(JobParameters params) {
        return false;
    }
}
