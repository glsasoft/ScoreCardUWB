package cn.com.glsa.app.fightgame;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattServer;
import android.bluetooth.BluetoothGattServerCallback;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothManager;
import android.bluetooth.BluetoothProfile;
import android.bluetooth.le.AdvertiseCallback;
import android.bluetooth.le.AdvertiseData;
import android.bluetooth.le.AdvertiseSettings;
import android.bluetooth.le.BluetoothLeAdvertiser;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.os.ParcelUuid;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.io.ByteArrayOutputStream;

import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;
import java.util.UUID;

/**
 * 比分牌 BLE 从机（Peripheral）：广播 + GATT Server。
 * 供微信小程序等主机扫描连接；协议细节后续扩展，当前只跑通广播/连接/状态可读可通知。
 *
 * Service UUID: {@link #SERVICE_UUID}
 * Status Char:  {@link #STATUS_CHAR_UUID} (READ + NOTIFY)
 * Service Data（扫描期可过滤）: 同一 Service UUID，UTF-8 载荷为 {@code GLSA-SC-{编号}}
 */
public final class ScoreCardBlePeripheral {

    private static final String TAG = "ScoreCardBlePeripheral";

    public static final String SERVICE_UUID = "A1B2C3D4-0001-4000-8000-00805F9B34FB";
    public static final String STATUS_CHAR_UUID = "A1B2C3D4-0002-4000-8000-00805F9B34FB";
    public static final String COMMAND_CHAR_UUID = "A1B2C3D4-0003-4000-8000-00805F9B34FB";
    /** Client Characteristic Configuration Descriptor */
    private static final UUID CCCD_UUID = UUID.fromString("00002902-0000-1000-8000-00805f9b34fb");

    private static final String UNITY_GO = "ScoreCardBleBridge_Singleton";
    private static final String DEFAULT_DEVICE_NAME = "GLSA-SC-0";
    /** Local Name 约 11 可见字符；Service Data 可携带完整逻辑名 */
    private static final int MAX_LOCAL_NAME_LEN = 11;

    private static final Handler MAIN = new Handler(Looper.getMainLooper());

    private static Activity sActivity;
    private static BluetoothManager sBtManager;
    private static BluetoothAdapter sAdapter;
    private static BluetoothLeAdvertiser sAdvertiser;
    private static BluetoothGattServer sGattServer;
    private static BluetoothGattCharacteristic sStatusChar;
    private static BluetoothGattCharacteristic sCommandChar;
    private static volatile boolean sAdvertising;
    private static volatile String sLastError = "";
    private static volatile String sStatusPayload = "{\"ok\":1}";
    private static final Set<BluetoothDevice> sNotifyDevices = new HashSet<>();

    /** 完整逻辑名，写入 Service Data（连接前可过滤） */
    private static volatile String sLogicalName = DEFAULT_DEVICE_NAME;
    /** 截断后的 Local Name */
    private static volatile String sLocalName = DEFAULT_DEVICE_NAME;
    /** 业务希望保持广播（stop 前为 true）；蓝牙被关后可据此自动重开并恢复广播 */
    private static volatile boolean sWantRunning;
    private static volatile String sPendingDeviceName = DEFAULT_DEVICE_NAME;
    private static boolean sBtReceiverRegistered;
    private static int sEnableWaitAttempt;
    private static final int ENABLE_WAIT_MAX = 40; // 40 * 500ms ≈ 20s
    private static final long ENABLE_WAIT_MS = 500L;
    /** 广播包过大时的降级尝试：0/1/2 */
    private static int sAdvertiseAttempt;
    private static final int ADVERTISE_ATTEMPT_MAX = 3;
    /** addService 异步完成后再广播 */
    private static volatile boolean sPendingAdvertiseAfterService;

    /**
     * 默认 ATT MTU=23 → Notify/Write 有效载荷约 20 字节。
     * Status Notify 只发短 tip；完整 JSON 放在特征 value，由 Central READ（Long Read）取回。
     */
    private static final int ATT_DEFAULT_PAYLOAD = 20;
    private static final ByteArrayOutputStream sPrepCommandBuf = new ByteArrayOutputStream();
    private static volatile boolean sPrepCommandActive;

    private ScoreCardBlePeripheral() {}

    public static boolean isAdvertising() {
        return sAdvertising;
    }

    public static String getLastError() {
        return sLastError != null ? sLastError : "";
    }

    public static String getServiceUuid() {
        return SERVICE_UUID;
    }

    public static String getStatusCharUuid() {
        return STATUS_CHAR_UUID;
    }

    public static String getCommandCharUuid() {
        return COMMAND_CHAR_UUID;
    }

    /**
     * 启动 BLE 广播与 GATT Server。可重复调用（已启动则先 stop 再启）。
     *
     * @param activity   Unity Activity；传 null 则尝试取 currentActivity
     * @param deviceName 广播名，格式 GLSA-SC-{比分牌编号}；空则用 {@link #DEFAULT_DEVICE_NAME}
     */
    @SuppressLint("MissingPermission")
    public static void start(Activity activity, String deviceName) {
        MAIN.post(() -> startOnMain(activity, deviceName));
    }

    @SuppressLint("MissingPermission")
    public static void stop() {
        MAIN.post(ScoreCardBlePeripheral::stopOnMain);
    }

    /**
     * 更新状态载荷。完整 JSON 写入特征（供 READ）；Notify 仅发 ≤20 字节 tip，
     * 避免微信小程序在 MTU 协商失败时出现截断/错位。
     */
    @SuppressLint("MissingPermission")
    public static void setStatus(String utf8Status) {
        if (utf8Status == null) utf8Status = "";
        final String payload = utf8Status;
        MAIN.post(() -> {
            sStatusPayload = payload;
            if (sStatusChar != null) {
                byte[] full = payload.getBytes(StandardCharsets.UTF_8);
                sStatusChar.setValue(full);
                notifySubscribers(buildNotifyTip(payload));
                // notifyCharacteristicChanged 可能改写特征 value，恢复完整载荷供 READ
                sStatusChar.setValue(full);
            }
        });
    }

    /** 从 Status JSON 抽 seq，生成 ≤20 字节 tip，例如 {"seq":12} */
    private static byte[] buildNotifyTip(String statusJson) {
        long seq = 0;
        if (statusJson != null) {
            int i = statusJson.indexOf("\"seq\"");
            if (i >= 0) {
                int colon = statusJson.indexOf(':', i);
                if (colon >= 0) {
                    int end = colon + 1;
                    while (end < statusJson.length()
                            && (statusJson.charAt(end) == ' ' || statusJson.charAt(end) == '\t')) {
                        end++;
                    }
                    int start = end;
                    while (end < statusJson.length()) {
                        char c = statusJson.charAt(end);
                        if (c < '0' || c > '9') break;
                        end++;
                    }
                    if (end > start) {
                        try {
                            seq = Long.parseLong(statusJson.substring(start, end));
                        } catch (NumberFormatException ignored) {
                        }
                    }
                }
            }
        }
        String tip = "{\"seq\":" + seq + "}";
        byte[] bytes = tip.getBytes(StandardCharsets.UTF_8);
        if (bytes.length <= ATT_DEFAULT_PAYLOAD) return bytes;
        return Arrays.copyOf(bytes, ATT_DEFAULT_PAYLOAD);
    }

    @SuppressLint("MissingPermission")
    private static void startOnMain(Activity activity, String deviceName) {
        sWantRunning = true;
        sPendingDeviceName = (deviceName == null || deviceName.trim().isEmpty())
                ? DEFAULT_DEVICE_NAME : deviceName.trim();
        sEnableWaitAttempt = 0;

        stopAdvertisingAndGattOnly();

        try {
            if (activity == null) {
                activity = UnityPlayer.currentActivity;
            }
            if (activity == null) {
                fail("Activity 为空");
                return;
            }
            sActivity = activity;
            Context app = activity.getApplicationContext();

            if (!app.getPackageManager().hasSystemFeature(PackageManager.FEATURE_BLUETOOTH_LE)) {
                fail("设备不支持 BLE");
                return;
            }

            sBtManager = (BluetoothManager) app.getSystemService(Context.BLUETOOTH_SERVICE);
            if (sBtManager == null) {
                fail("BluetoothManager 为空");
                return;
            }
            sAdapter = sBtManager.getAdapter();
            if (sAdapter == null) {
                fail("BluetoothAdapter 为空（无蓝牙硬件或驱动未就绪）");
                return;
            }

            ensureBtReceiver(app);

            if (!sAdapter.isEnabled()) {
                // enable() 仅表示「请求已受理」，真正开启是异步的；须等待 STATE_ON
                boolean accepted;
                try {
                    accepted = sAdapter.enable();
                } catch (SecurityException se) {
                    fail("无权限开启蓝牙: " + se.getMessage());
                    return;
                }
                Log.w(TAG, "蓝牙未开启，已请求 enable() => " + accepted + "，等待 STATE_ON…");
                sendUnity("OnBleLog", "waiting bluetooth ON after enable()=" + accepted);
                scheduleWaitEnabledAndStart();
                return;
            }

            continueStartWhenBtOn();
        } catch (SecurityException se) {
            fail("权限不足: " + se.getMessage());
        } catch (Exception e) {
            fail("start 异常: " + e.getMessage());
            Log.e(TAG, "start failed", e);
        }
    }

    @SuppressLint("MissingPermission")
    private static void scheduleWaitEnabledAndStart() {
        if (!sWantRunning) return;
        MAIN.postDelayed(() -> {
            if (!sWantRunning) return;
            if (sAdapter != null && sAdapter.isEnabled()) {
                Log.i(TAG, "蓝牙已开启，继续启动广播");
                continueStartWhenBtOn();
                return;
            }
            sEnableWaitAttempt++;
            if (sEnableWaitAttempt >= ENABLE_WAIT_MAX) {
                fail("等待蓝牙开启超时（请确认系统蓝牙可手动打开，或检查 BLUETOOTH_CONNECT 权限）");
                return;
            }
            // 仍关着则再请求一次 enable（部分机型首次会被忽略）
            if (sAdapter != null && !sAdapter.isEnabled() && (sEnableWaitAttempt % 4 == 0)) {
                try {
                    boolean ok = sAdapter.enable();
                    Log.w(TAG, "再次请求 enable() => " + ok + " attempt=" + sEnableWaitAttempt);
                } catch (Exception e) {
                    Log.w(TAG, "enable retry: " + e.getMessage());
                }
            }
            scheduleWaitEnabledAndStart();
        }, ENABLE_WAIT_MS);
    }

    @SuppressLint("MissingPermission")
    private static void continueStartWhenBtOn() {
        if (!sWantRunning) return;
        try {
            if (sAdapter == null || !sAdapter.isEnabled()) {
                scheduleWaitEnabledAndStart();
                return;
            }
            // isMultipleAdvertisementSupported=false 只表示「不支持多路并发广播」。
            // 不少 RK 镜像会误报 false，但仍可用 getBluetoothLeAdvertiser() 做单路 Peripheral 广播。
            if (!sAdapter.isMultipleAdvertisementSupported()) {
                Log.w(TAG, "isMultipleAdvertisementSupported=false，仍尝试单路 BluetoothLeAdvertiser");
                sendUnity("OnBleLog", "multi-advertisement unsupported, try single advertiser");
            }

            sLogicalName = sPendingDeviceName;
            sLocalName = truncateLocalName(sLogicalName);
            try {
                sAdapter.setName(sLocalName);
            } catch (Exception e) {
                Log.w(TAG, "setName 失败: " + e.getMessage());
            }

            sAdvertiser = sAdapter.getBluetoothLeAdvertiser();
            if (sAdvertiser == null) {
                // 刚开蓝牙时 advertiser 可能短暂为 null
                if (sEnableWaitAttempt < ENABLE_WAIT_MAX) {
                    sEnableWaitAttempt++;
                    Log.w(TAG, "BluetoothLeAdvertiser 暂为空，重试 attempt=" + sEnableWaitAttempt);
                    MAIN.postDelayed(ScoreCardBlePeripheral::continueStartWhenBtOn, ENABLE_WAIT_MS);
                    return;
                }
                fail("不支持 BLE 广播：BluetoothLeAdvertiser 为空（芯片/驱动可能仅支持 Central，无 Peripheral）");
                return;
            }

            Context app = sActivity != null ? sActivity.getApplicationContext() : null;
            if (app == null) {
                fail("Activity 为空");
                return;
            }
            if (!openGattServer(app)) {
                return;
            }
            // 广播在 onServiceAdded 成功后启动（见 sGattCallback）
        } catch (SecurityException se) {
            fail("权限不足: " + se.getMessage());
        } catch (Exception e) {
            fail("continueStart 异常: " + e.getMessage());
            Log.e(TAG, "continueStart failed", e);
        }
    }

    private static void ensureBtReceiver(Context app) {
        if (sBtReceiverRegistered || app == null) return;
        IntentFilter filter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
        try {
            if (Build.VERSION.SDK_INT >= 33) {
                app.registerReceiver(sBtStateReceiver, filter, Context.RECEIVER_NOT_EXPORTED);
            } else {
                app.registerReceiver(sBtStateReceiver, filter);
            }
            sBtReceiverRegistered = true;
            Log.i(TAG, "已注册蓝牙开关广播");
        } catch (Exception e) {
            Log.w(TAG, "registerReceiver 失败: " + e.getMessage());
        }
    }

    private static void unregisterBtReceiver() {
        if (!sBtReceiverRegistered) return;
        try {
            Context app = sActivity != null ? sActivity.getApplicationContext() : null;
            if (app != null) {
                app.unregisterReceiver(sBtStateReceiver);
            }
        } catch (Exception e) {
            Log.w(TAG, "unregisterReceiver: " + e.getMessage());
        }
        sBtReceiverRegistered = false;
    }

    private static final BroadcastReceiver sBtStateReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (intent == null || !BluetoothAdapter.ACTION_STATE_CHANGED.equals(intent.getAction())) {
                return;
            }
            int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, BluetoothAdapter.ERROR);
            Log.i(TAG, "蓝牙状态变化 state=" + state + " wantRunning=" + sWantRunning);
            if (state == BluetoothAdapter.STATE_ON) {
                if (sWantRunning && !sAdvertising) {
                    sendUnity("OnBleLog", "bluetooth ON, resume advertising");
                    MAIN.post(ScoreCardBlePeripheral::continueStartWhenBtOn);
                }
            } else if (state == BluetoothAdapter.STATE_OFF || state == BluetoothAdapter.STATE_TURNING_OFF) {
                sAdvertising = false;
                if (sWantRunning) {
                    sendUnity("OnBleLog", "bluetooth OFF, will re-enable");
                    MAIN.post(() -> {
                        stopAdvertisingAndGattOnly();
                        if (sAdapter != null && sWantRunning) {
                            try {
                                sAdapter.enable();
                            } catch (Exception e) {
                                Log.w(TAG, "auto enable after OFF: " + e.getMessage());
                            }
                            sEnableWaitAttempt = 0;
                            scheduleWaitEnabledAndStart();
                        }
                    });
                }
            }
        }
    };

    /**
     * 启动/降级广播（ADV 31 字节限制：128-bit UUID 与 LocalName 放不下同一包）。
     * 0 = ADV: UUID；ScanResponse: LocalName（小程序/调试工具可见名字）
     * 1 = ADV: UUID；ScanResponse: ManufacturerData(名字)
     * 2 = ADV: UUID only
     * 3 = ADV: LocalName only（兜底，仅按名字扫时能见）
     */
    @SuppressLint("MissingPermission")
    private static void startAdvertisingAttempt() {
        if (sAdvertiser == null) {
            fail("BluetoothLeAdvertiser 为空");
            return;
        }

        ParcelUuid serviceUuid = new ParcelUuid(UUID.fromString(SERVICE_UUID));
        byte[] nameBytes = sLogicalName.getBytes(StandardCharsets.UTF_8);

        AdvertiseSettings settings = new AdvertiseSettings.Builder()
                .setAdvertiseMode(AdvertiseSettings.ADVERTISE_MODE_LOW_LATENCY)
                .setTxPowerLevel(AdvertiseSettings.ADVERTISE_TX_POWER_HIGH)
                .setConnectable(true)
                .setTimeout(0)
                .build();

        AdvertiseData data;
        AdvertiseData scanResponse = null;
        switch (sAdvertiseAttempt) {
            case 0:
                data = new AdvertiseData.Builder()
                        .addServiceUuid(serviceUuid)
                        .setIncludeDeviceName(false)
                        .build();
                scanResponse = new AdvertiseData.Builder()
                        .setIncludeDeviceName(true)
                        .build();
                break;
            case 1:
                data = new AdvertiseData.Builder()
                        .addServiceUuid(serviceUuid)
                        .setIncludeDeviceName(false)
                        .build();
                // 0xFFFF = 开发用 Company ID；载荷为 GLSA-SC-{n}，替代过大的 128-bit ServiceData
                scanResponse = new AdvertiseData.Builder()
                        .addManufacturerData(0xFFFF, nameBytes)
                        .build();
                break;
            case 2:
                data = new AdvertiseData.Builder()
                        .addServiceUuid(serviceUuid)
                        .setIncludeDeviceName(false)
                        .build();
                break;
            default:
                data = new AdvertiseData.Builder()
                        .setIncludeDeviceName(true)
                        .build();
                break;
        }

        try {
            if (scanResponse != null) {
                sAdvertiser.startAdvertising(settings, data, scanResponse, sAdvertiseCallback);
            } else {
                sAdvertiser.startAdvertising(settings, data, sAdvertiseCallback);
            }
        } catch (Exception e) {
            fail("startAdvertising 异常: " + e.getMessage());
            return;
        }

        Log.i(TAG, "startAdvertising attempt=" + sAdvertiseAttempt
                + " localName=" + sLocalName
                + " logical=" + sLogicalName
                + " service=" + SERVICE_UUID);
        sendUnity("OnBleLog", "startAdvertising attempt=" + sAdvertiseAttempt
                + " name=" + sLogicalName);
    }

    private static String truncateLocalName(String name) {
        if (name == null || name.isEmpty()) return DEFAULT_DEVICE_NAME;
        if (name.length() <= MAX_LOCAL_NAME_LEN) return name;
        int dash = name.lastIndexOf('-');
        if (dash > 0 && dash < name.length() - 1) {
            String suffix = name.substring(dash);
            int prefixLen = Math.max(0, MAX_LOCAL_NAME_LEN - suffix.length());
            return name.substring(0, prefixLen) + suffix;
        }
        return name.substring(0, MAX_LOCAL_NAME_LEN);
    }

    @SuppressLint("MissingPermission")
    private static boolean openGattServer(Context app) {
        sGattServer = sBtManager.openGattServer(app, sGattCallback);
        if (sGattServer == null) {
            fail("openGattServer 失败");
            return false;
        }

        BluetoothGattService service = new BluetoothGattService(
                UUID.fromString(SERVICE_UUID),
                BluetoothGattService.SERVICE_TYPE_PRIMARY);

        sStatusChar = new BluetoothGattCharacteristic(
                UUID.fromString(STATUS_CHAR_UUID),
                BluetoothGattCharacteristic.PROPERTY_READ | BluetoothGattCharacteristic.PROPERTY_NOTIFY,
                BluetoothGattCharacteristic.PERMISSION_READ);

        BluetoothGattDescriptor cccd = new BluetoothGattDescriptor(
                CCCD_UUID,
                BluetoothGattDescriptor.PERMISSION_READ | BluetoothGattDescriptor.PERMISSION_WRITE);
        sStatusChar.addDescriptor(cccd);
        sStatusChar.setValue(sStatusPayload.getBytes(StandardCharsets.UTF_8));

        sCommandChar = new BluetoothGattCharacteristic(
                UUID.fromString(COMMAND_CHAR_UUID),
                BluetoothGattCharacteristic.PROPERTY_WRITE
                        | BluetoothGattCharacteristic.PROPERTY_WRITE_NO_RESPONSE,
                BluetoothGattCharacteristic.PERMISSION_WRITE);
        service.addCharacteristic(sCommandChar);

        service.addCharacteristic(sStatusChar);
        sPendingAdvertiseAfterService = true;
        boolean added = sGattServer.addService(service);
        if (!added) {
            sPendingAdvertiseAfterService = false;
            fail("addService 失败");
            return false;
        }
        Log.i(TAG, "addService 已请求，等待 onServiceAdded 后开始广播");
        return true;
    }

    @SuppressLint("MissingPermission")
    private static void stopOnMain() {
        sWantRunning = false;
        stopAdvertisingAndGattOnly();
        unregisterBtReceiver();
        Log.i(TAG, "stopped");
    }

    /** 停广播与 GATT，但不清 sWantRunning（供蓝牙重开后恢复）。 */
    @SuppressLint("MissingPermission")
    private static void stopAdvertisingAndGattOnly() {
        sAdvertising = false;
        sPendingAdvertiseAfterService = false;
        synchronized (sNotifyDevices) {
            sNotifyDevices.clear();
        }

        try {
            if (sAdvertiser != null) {
                sAdvertiser.stopAdvertising(sAdvertiseCallback);
            }
        } catch (Exception e) {
            Log.w(TAG, "stopAdvertising: " + e.getMessage());
        }
        sAdvertiser = null;

        try {
            if (sGattServer != null) {
                sGattServer.close();
            }
        } catch (Exception e) {
            Log.w(TAG, "gattServer.close: " + e.getMessage());
        }
        sGattServer = null;
        sStatusChar = null;
        sCommandChar = null;
        resetPrepCommand();
    }

    @SuppressLint("MissingPermission")
    private static void notifySubscribers(byte[] value) {
        if (sGattServer == null || sStatusChar == null || value == null) return;
        Set<BluetoothDevice> copy;
        synchronized (sNotifyDevices) {
            copy = new HashSet<>(sNotifyDevices);
        }
        byte[] full = sStatusPayload.getBytes(StandardCharsets.UTF_8);
        for (BluetoothDevice d : copy) {
            try {
                sStatusChar.setValue(value);
                sGattServer.notifyCharacteristicChanged(d, sStatusChar, false);
            } catch (Exception e) {
                Log.w(TAG, "notify " + d + " failed: " + e.getMessage());
            }
        }
        try {
            sStatusChar.setValue(full);
        } catch (Exception ignored) {
        }
    }

    private static void dispatchCommandPayload(String payload) {
        if (payload == null) payload = "";
        payload = payload.trim();
        if (payload.isEmpty()) return;
        Log.i(TAG, "command payload=" + payload);
        sendUnity("OnBleCommand", payload);
    }

    private static void resetPrepCommand() {
        sPrepCommandActive = false;
        sPrepCommandBuf.reset();
    }

    private static byte[] chunkOrEmpty(byte[] value) {
        return value != null ? value : new byte[0];
    }

    private static void fail(String msg) {
        sLastError = msg != null ? msg : "unknown";
        sAdvertising = false;
        Log.e(TAG, sLastError);
        sendUnity("OnBleError", sLastError);
    }

    private static void sendUnity(String method, String message) {
        try {
            UnityPlayer.UnitySendMessage(UNITY_GO, method, message != null ? message : "");
        } catch (Exception ignored) {
        }
    }

    private static final AdvertiseCallback sAdvertiseCallback = new AdvertiseCallback() {
        @Override
        public void onStartSuccess(AdvertiseSettings settingsInEffect) {
            sAdvertising = true;
            sLastError = "";
            Log.i(TAG, "advertise success attempt=" + sAdvertiseAttempt
                    + " localName=" + sLocalName + " logical=" + sLogicalName);
            sendUnity("OnBleAdvertisingStarted", SERVICE_UUID);
        }

        @Override
        public void onStartFailure(int errorCode) {
            sAdvertising = false;
            if (errorCode == AdvertiseCallback.ADVERTISE_FAILED_DATA_TOO_LARGE
                    && sAdvertiseAttempt < ADVERTISE_ATTEMPT_MAX) {
                sAdvertiseAttempt++;
                Log.w(TAG, "advertise DATA_TOO_LARGE, retry attempt=" + sAdvertiseAttempt);
                try {
                    startAdvertisingAttempt();
                    return;
                } catch (Exception e) {
                    fail("advertise 降级重试异常: " + e.getMessage());
                    return;
                }
            }
            String msg = "advertise fail code=" + errorCode + " (" + advertiseErrorName(errorCode) + ")";
            fail(msg);
        }
    };

    private static String advertiseErrorName(int code) {
        switch (code) {
            case AdvertiseCallback.ADVERTISE_FAILED_DATA_TOO_LARGE: return "DATA_TOO_LARGE";
            case AdvertiseCallback.ADVERTISE_FAILED_TOO_MANY_ADVERTISERS: return "TOO_MANY_ADVERTISERS";
            case AdvertiseCallback.ADVERTISE_FAILED_ALREADY_STARTED: return "ALREADY_STARTED";
            case AdvertiseCallback.ADVERTISE_FAILED_INTERNAL_ERROR: return "INTERNAL_ERROR";
            case AdvertiseCallback.ADVERTISE_FAILED_FEATURE_UNSUPPORTED: return "FEATURE_UNSUPPORTED";
            default: return "UNKNOWN";
        }
    }

    private static final BluetoothGattServerCallback sGattCallback = new BluetoothGattServerCallback() {
        @Override
        public void onServiceAdded(int status, BluetoothGattService service) {
            Log.i(TAG, "onServiceAdded status=" + status
                    + " uuid=" + (service != null ? service.getUuid() : "null")
                    + " pendingAdvertise=" + sPendingAdvertiseAfterService);
            if (!sPendingAdvertiseAfterService) return;
            sPendingAdvertiseAfterService = false;
            if (status != BluetoothGatt.GATT_SUCCESS) {
                fail("onServiceAdded 失败 status=" + status);
                return;
            }
            if (!sWantRunning) return;
            sAdvertiseAttempt = 0;
            startAdvertisingAttempt();
        }

        @Override
        public void onConnectionStateChange(BluetoothDevice device, int status, int newState) {
            Log.i(TAG, "conn " + device + " status=" + status + " state=" + newState);
            if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                synchronized (sNotifyDevices) {
                    sNotifyDevices.remove(device);
                }
                sendUnity("OnBleDisconnected", safeAddress(device));
            } else if (newState == BluetoothProfile.STATE_CONNECTED) {
                sendUnity("OnBleConnected", safeAddress(device));
            }
        }

        @Override
        public void onCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset,
                                                BluetoothGattCharacteristic characteristic) {
            if (sGattServer == null) return;
            byte[] full = sStatusPayload.getBytes(StandardCharsets.UTF_8);
            byte[] value;
            if (offset >= full.length) {
                value = new byte[0];
            } else {
                value = Arrays.copyOfRange(full, offset, full.length);
            }
            int result = BluetoothGatt.GATT_SUCCESS;
            if (!STATUS_CHAR_UUID.equalsIgnoreCase(characteristic.getUuid().toString())) {
                result = BluetoothGatt.GATT_FAILURE;
                value = new byte[0];
            }
            sGattServer.sendResponse(device, requestId, result, offset, value);
        }

        @Override
        public void onCharacteristicWriteRequest(BluetoothDevice device, int requestId,
                                                   BluetoothGattCharacteristic characteristic,
                                                   boolean preparedWrite, boolean responseNeeded,
                                                   int offset, byte[] value) {
            if (sGattServer == null) return;

            int result = BluetoothGatt.GATT_SUCCESS;
            if (COMMAND_CHAR_UUID.equalsIgnoreCase(characteristic.getUuid().toString())) {
                byte[] chunk = value != null ? value : new byte[0];
                if (preparedWrite) {
                    try {
                        if (!sPrepCommandActive) {
                            sPrepCommandBuf.reset();
                            sPrepCommandActive = true;
                        }
                        // 按 offset 写入（简单场景多为顺序追加）
                        while (sPrepCommandBuf.size() < offset) {
                            sPrepCommandBuf.write(0);
                        }
                        if (offset == sPrepCommandBuf.size()) {
                            sPrepCommandBuf.write(chunk);
                        } else if (offset < sPrepCommandBuf.size()) {
                            byte[] prev = sPrepCommandBuf.toByteArray();
                            sPrepCommandBuf.reset();
                            sPrepCommandBuf.write(prev, 0, offset);
                            sPrepCommandBuf.write(chunk);
                            if (offset + chunk.length < prev.length) {
                                sPrepCommandBuf.write(prev, offset + chunk.length,
                                        prev.length - offset - chunk.length);
                            }
                        } else {
                            sPrepCommandBuf.write(chunk);
                        }
                    } catch (Exception e) {
                        Log.w(TAG, "prepare write fail: " + e.getMessage());
                        result = BluetoothGatt.GATT_FAILURE;
                        resetPrepCommand();
                    }
                } else {
                    resetPrepCommand();
                    dispatchCommandPayload(new String(chunk, StandardCharsets.UTF_8));
                }
            } else {
                result = BluetoothGatt.GATT_FAILURE;
            }

            if (responseNeeded) {
                sGattServer.sendResponse(device, requestId, result, offset, chunkOrEmpty(value));
            }
        }

        @Override
        public void onExecuteWrite(BluetoothDevice device, int requestId, boolean execute) {
            if (sGattServer == null) return;
            if (execute && sPrepCommandActive) {
                dispatchCommandPayload(
                        new String(sPrepCommandBuf.toByteArray(), StandardCharsets.UTF_8));
            }
            resetPrepCommand();
            sGattServer.sendResponse(device, requestId, BluetoothGatt.GATT_SUCCESS, 0, null);
        }

        @Override
        public void onDescriptorWriteRequest(BluetoothDevice device, int requestId,
                                             BluetoothGattDescriptor descriptor,
                                             boolean preparedWrite, boolean responseNeeded,
                                             int offset, byte[] value) {
            if (sGattServer == null) return;
            if (CCCD_UUID.equals(descriptor.getUuid())) {
                boolean enable = value != null && value.length >= 2
                        && (value[0] & 0x01) == 0x01;
                synchronized (sNotifyDevices) {
                    if (enable) sNotifyDevices.add(device);
                    else sNotifyDevices.remove(device);
                }
                Log.i(TAG, "CCCD notify=" + enable + " device=" + device);
                if (responseNeeded) {
                    sGattServer.sendResponse(device, requestId, BluetoothGatt.GATT_SUCCESS, offset, value);
                }
                if (enable && sStatusChar != null) {
                    byte[] full = sStatusPayload.getBytes(StandardCharsets.UTF_8);
                    sStatusChar.setValue(full);
                    notifySubscribers(buildNotifyTip(sStatusPayload));
                    sStatusChar.setValue(full);
                }
            } else if (responseNeeded) {
                sGattServer.sendResponse(device, requestId, BluetoothGatt.GATT_FAILURE, offset, null);
            }
        }

        @Override
        public void onDescriptorReadRequest(BluetoothDevice device, int requestId, int offset,
                                            BluetoothGattDescriptor descriptor) {
            if (sGattServer == null) return;
            byte[] value = BluetoothGattDescriptor.DISABLE_NOTIFICATION_VALUE;
            synchronized (sNotifyDevices) {
                if (sNotifyDevices.contains(device)) {
                    value = BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE;
                }
            }
            if (offset > 0) {
                value = offset >= value.length ? new byte[0] : Arrays.copyOfRange(value, offset, value.length);
            }
            sGattServer.sendResponse(device, requestId, BluetoothGatt.GATT_SUCCESS, offset, value);
        }
    };

    private static String safeAddress(BluetoothDevice device) {
        if (device == null) return "";
        try {
            if (Build.VERSION.SDK_INT >= 31) {
                return device.getAddress();
            }
            return device.getAddress();
        } catch (SecurityException e) {
            return "unknown";
        }
    }
}
