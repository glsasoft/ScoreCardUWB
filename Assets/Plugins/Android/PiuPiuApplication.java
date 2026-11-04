package cn.com.glsa.app.fightgame;
import android.app.ActivityManager;
import android.os.Build;
import android.os.UserManager;
import android.util.Log;
import android.app.Application;
import cn.com.glsa.app.fightgame.wxapi.WXEntryActivity;
import com.iflytek.cloud.SpeechUtility;

public class PiuPiuApplication extends Application {
    @Override
    public void onCreate() {
        super.onCreate();
        // 科大讯飞语音合成初始化（须在使用 TTS 前完成）。
        // 开机 LOCKED_BOOT_COMPLETED 拉起进程时用户尚未解锁，讯飞读取凭据加密区 SharedPreferences 会失败；
        // 此时跳过，XFTTSBridge.init 使用 TTS 前会再调用一次 createUtility。
        if (isUserUnlocked()) {
            SpeechUtility.createUtility(this, "appid=604ef9ce");
        }
        String pkName = getPackageName();
        if(pkName.equals("cn.com.glsa.app.fightgame")){
            WXEntryActivity.WX_APP_ID = "wx4630563c45bd7776";
        }else{
            WXEntryActivity.WX_APP_ID = "wxad0d6c339e2205f0";
        }
        Log.i("Unity", "Application 包名:"+pkName+" WXEntryActivity.WX_APP_ID="+WXEntryActivity.WX_APP_ID);

        if (isMainProcess()) {
            BootJobService.ensureScheduled(this);
            BootLaunchReceiver.startPollingIfNeeded(this);
        }
    }

    private boolean isUserUnlocked() {
        if (Build.VERSION.SDK_INT < 24) return true;
        UserManager userManager = getSystemService(UserManager.class);
        return userManager == null || userManager.isUserUnlocked();
    }

    private boolean isMainProcess() {
        String processName;
        if (Build.VERSION.SDK_INT >= 28) {
            processName = Application.getProcessName();
        } else {
            processName = currentProcessNameLegacy();
        }
        return processName == null || processName.equals(getPackageName());
    }

    private String currentProcessNameLegacy() {
        int pid = android.os.Process.myPid();
        ActivityManager am = (ActivityManager) getSystemService(ACTIVITY_SERVICE);
        if (am == null) return null;
        java.util.List<ActivityManager.RunningAppProcessInfo> list = am.getRunningAppProcesses();
        if (list == null) return null;
        for (ActivityManager.RunningAppProcessInfo info : list) {
            if (info.pid == pid) return info.processName;
        }
        return null;
    }
}
