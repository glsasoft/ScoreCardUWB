package cn.com.glsa.app.fightgame;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothManager;
import android.bluetooth.BluetoothProfile;
import android.bluetooth.le.BluetoothLeScanner;
import android.bluetooth.le.ScanCallback;
import android.bluetooth.le.ScanFilter;
import android.bluetooth.le.ScanResult;
import android.bluetooth.le.ScanSettings;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import androidx.annotation.RequiresPermission;

import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.TimeUnit;

import cn.com.glsa.app.fightgame.oad.OAD;
import cn.com.glsa.app.fightgame.oad.OADCallback;
import cn.com.glsa.app.fightgame.oad.BleOtaManager;

public class GLSABlueToothDevice {


    static final String TAG = "Unity Android 蓝牙";

    public static final int BlueToothStatusNone = 0;        //未连接
    public static final int BlueToothStatusConnected = 1;        //已连接
    public static final int BlueToothStatusDeviceConnected = 2;  //已连接并订阅成功
    public static final int BlueToothStatusConnectFailed = 3;        //连接失败
    public static final int BlueToothStatusDisConnect = 4;        //被动断开
    public static final int BlueToothStatusCutOff = 5;       //用户手动断开蓝牙
    public static final int BlueToothStatusConnecting = 6;        //链接中
    public static final int BlueToothStatusConnectOver = 7;        //链接超时
    public static final int BlueToothStatusPowerOff = 8;       //蓝牙已关闭
    public static final int BlueToothStatusUpdate = 9;       //蓝牙OAD升级中
    public static final int BlueToothStatusUpdated = 10;       //蓝牙OAD升级成功
    public static final int BlueToothStatusUpdateFail = 11;       //蓝牙OAD升级失败

    private final String GunHeadServiceUuid = "0000fff0-0000-1000-8000-00805f9b34fb";
    private final String GunHeadWriteUuid = "0000fff6-0000-1000-8000-00805f9b34fb";
    private final String GunHeadNotifyUuid = "0000fff6-0000-1000-8000-00805f9b34fb";
    private final String MatrixServiceUuid = "0000ffe0-0000-1000-8000-00805f9b34fb";
    private final String MatrixWriteUuid = "0000ffe1-0000-1000-8000-00805f9b34fb";
    private final String MatrixNotifyUuid = "0000ffe1-0000-1000-8000-00805f9b34fb";
    private final String CoinPusherServiceUuid = "0000ffb0-0000-1000-8000-00805f9b34fb";
    private final String CoinPusherWriteUuid = "0000ffb1-0000-1000-8000-00805f9b34fb";
    private final String CoinPusherNotifyUuid = "0000ffb2-0000-1000-8000-00805f9b34fb";

    private static boolean isReceiverRegistered = false;
    // 广播接收器实现
    private static final BroadcastReceiver mBluetoothStateReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            final String action = intent.getAction();
            if(action == null) return;
            if (action.equals(BluetoothAdapter.ACTION_STATE_CHANGED)) {
                final int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE,
                        BluetoothAdapter.ERROR);
                switch (state) {
                    case BluetoothAdapter.STATE_OFF:
                        // 蓝牙已关闭
                        Log.w(TAG, "蓝牙适配器已关闭!! 重置所有设备状态!!");
                        for(BlueConnectInfo dev : getDeviceMap().values()){
                            dev.deviceStatus = BlueToothStatusDisConnect;
                            _updateAndSendBackStatus(dev.deviceType, dev.deviceStatus, "蓝牙适配器已关闭!");
                        }
                        break;
                    case BluetoothAdapter.STATE_ON:
                        // 蓝牙已打开
                        Log.w(TAG, "蓝牙适配器已打开!!");
                        for(BlueConnectInfo dev : getDeviceMap().values()){
                            dev.deviceStatus = BlueToothStatusDisConnect;
                            _updateAndSendBackStatus(dev.deviceType, dev.deviceStatus, "蓝牙适配器重新打开!");
                        }
                        break;
                }
            }
        }
    };

    private static Handler _handler = null;

    public static Handler getHandler(){
        if(_handler == null) _handler = new Handler(Looper.getMainLooper());
        return _handler;
    }


    public interface PermissionCallback {
        void onResult(String[] permissions, int[] grantResults);
    }

    public interface EnableBluetoothCallback {
        void onResult();
    }

    public interface Callback {
        void onStatusChange(int deviceType, int deviceStatus, String messageContent);

        void onMessage(int deviceType, int deviceStatus, String messageContent);
    }

    public static class BlueConnectInfo {
        public String[] serviceUuid = {"0000fff0-0000-1000-8000-00805f9b34fb", "0000ffe0-0000-1000-8000-00805f9b34fb"};
        public String[] writeUuid = {"0000fff6-0000-1000-8000-00805f9b34fb", "0000ffe1-0000-1000-8000-00805f9b34fb"};
        public String[] notifyUuid = {"0000fff6-0000-1000-8000-00805f9b34fb", "0000ffe2-0000-1000-8000-00805f9b34fb"};
        private BluetoothGatt gatt;
        private String deviceName;
        private int deviceType;
        private Activity context;
        private int deviceStatus = BlueToothStatusNone;
        private Callback callback;
        private Callback intervalCallback;
        //开始链接的时间戳
        private long startConnectTime;
        private int timeoutMillis;
        ScheduledExecutorService taskExecutor;
        ScheduledFuture<?> mScheduledFuture;
        // 记录当前使用的UUID组合索引
        private int currentUuidIndex = -1;

        public BlueConnectInfo(Activity context, String deviceName, int deviceType, int timeoutMillis, Callback callback) {
            deviceStatus = BlueToothStatusNone;
            this.context = context;
            this.deviceName = deviceName;
            this.deviceType = deviceType;
            this.callback = callback;
            this.timeoutMillis = timeoutMillis;
        }

        public BluetoothGatt getGatt() {
            return gatt;
        }

        public void setGatt(BluetoothGatt gatt) {
            this.gatt = gatt;
        }

        public int getDeviceStatus() {
            return deviceStatus;
        }

        public void setDeviceStatus(int deviceStatus) {
            this.deviceStatus = deviceStatus;
        }
    }

    private static final HashMap<Integer, BlueConnectInfo> _DEVICE_MAP = new HashMap<>();

    private static synchronized HashMap<Integer, BlueConnectInfo> getDeviceMap(){
        return _DEVICE_MAP;
    }

    //---------------------------------- 对外接口 --------------------------------


    /**
     * 获取蓝牙需要的权限列表
     * @return
     */
    public static List<String> getPermissions() {
        List<String> permissions = new ArrayList<>();

        if (Build.VERSION.SDK_INT <= Build.VERSION_CODES.R) {
            permissions.add(Manifest.permission.BLUETOOTH);
            permissions.add(Manifest.permission.BLUETOOTH_ADMIN);
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            permissions.add(Manifest.permission.BLUETOOTH_CONNECT);
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            permissions.add(Manifest.permission.BLUETOOTH_SCAN);
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            permissions.add(Manifest.permission.BLUETOOTH_ADVERTISE);
        }
        permissions.add(Manifest.permission.ACCESS_COARSE_LOCATION);
        permissions.add(Manifest.permission.ACCESS_FINE_LOCATION);

        return permissions;
    }

    /**
     * 检查蓝牙权限
     * @param activity Activity
     * @return 是否授权
     */
    public static boolean checkPermission(Activity activity) {
        boolean hasBlePermission = true;

        if(activity == null) return hasBlePermission;

        List<String> permissions = getPermissions();
        Debug("Unity Android", "蓝牙权限数量:" + permissions.size());

        for (String p : permissions) {
            int res = activity.checkSelfPermission(p);
            if (res != PackageManager.PERMISSION_GRANTED) {
                hasBlePermission = false;
            }
        }
        return hasBlePermission;
    }

    @RequiresPermission(allOf = {Manifest.permission.BLUETOOTH_SCAN, Manifest.permission.BLUETOOTH_CONNECT})
    public static void startConnect(Activity context, int deviceType, String deviceName, int timeoutMillis, Callback callback) {

        // 注册广播接收器
        if(!isReceiverRegistered){
            IntentFilter filter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
            context.registerReceiver(mBluetoothStateReceiver, filter);
            isReceiverRegistered = true;
        }

        if(!checkPermission(context)){
            callback.onStatusChange(deviceType, BlueToothStatusConnectFailed, "startConnect 未授权蓝牙!");
            return;
        }

        _startConnect(context, deviceType, deviceName, timeoutMillis, callback);
    }

    public static BlueConnectInfo getBlueConnectInfo(int deviceType) {
        return getDeviceMap().get(deviceType);
    }

    private static BlueConnectInfo getBlueConnectInfo(String deviceName) {
        for(BlueConnectInfo info : getDeviceMap().values()){
            if(info.deviceName.equals(deviceName)){
                return info;
            }
        }
        return null;
    }

    public static int getDeviceStatus(int type) {
        BlueConnectInfo info = getBlueConnectInfo(type);
        if(info == null) return BlueToothStatusNone;
        return info.deviceStatus;
    }

    public static String getDeviceName(int type) {
        BlueConnectInfo info = getBlueConnectInfo(type);
        if(info == null) return "";
        return info.deviceName;
    }

    /**
     * 开始OAD升级
     * @param context
     * @param deviceType 设备类型
     * @param binFileName bin文件路径
     */
    public static void startUpload(Activity context, int deviceType, String binFileName, Callback uploadCallback){

        if(!checkPermission(context)){
            uploadCallback.onStatusChange(deviceType, BlueToothStatusConnectFailed, "startUpload 未授权蓝牙!");
            return;
        }

        final float[] progress = {0};
        OADCallback callback = new OADCallback() {
            @Override
            public void onSuccess() {
                Debug("Unity Android", "蓝牙 startUpload.onSuccess():"+deviceType+" uploadCallback="+uploadCallback);
                //升级成功，发送 BlueToothStatusUpdated
                if(uploadCallback != null){
                    uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdated, "");
                }
            }

            @Override
            public void onError(String error) {
                Debug("Unity Android", "蓝牙 startUpload.onError():"+deviceType+" uploadCallback="+uploadCallback);
                //升级成功，发送 BlueToothStatusUpdated
                if(uploadCallback != null){
                    uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdateFail, error);
                }
            }

            @Override
            public void onProgress(int sentBlocks, int totalBlocks) {
                //升级中，发送 BlueToothStatusUpdate
                float currentProgress = (float)sentBlocks / (float)totalBlocks;
                if(currentProgress - progress[0] > 0.01 || currentProgress>=1){
                    progress[0] = currentProgress;
                    if(uploadCallback != null){
                        uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdate, String.valueOf(progress[0]));
                    }
                }
            }
        };

        //设备已连接，开始升级
        OAD.startOAD1(context, deviceType, binFileName, callback);
    }

    /**
     * 开始强制OAD升级
     * @param context
     * @param deviceType 设备类型
     * @param binFileName bin文件路径
     */
    public static void startForceUpload(Activity context, int deviceType, String deviceName, String binFileName, Callback uploadCallback){
        if(!checkPermission(context)){
            uploadCallback.onStatusChange(deviceType, BlueToothStatusConnectFailed, "startForceUpload 未授权蓝牙!");
            return;
        }

        final float[] progress = {0};
        OADCallback callback = new OADCallback() {
            @Override
            public void onSuccess() {
                Debug("Unity Android", "蓝牙 startUpload.onSuccess():"+deviceType+" uploadCallback="+uploadCallback);
                //升级成功，发送 BlueToothStatusUpdated
                if(uploadCallback != null){
                    uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdated, "");
                }
            }

            @Override
            public void onError(String error) {
                Debug("Unity Android", "蓝牙 startUpload.onError():"+deviceType+" uploadCallback="+uploadCallback);
                //升级成功，发送 BlueToothStatusUpdated
                if(uploadCallback != null){
                    uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdateFail, error);
                }
            }

            @Override
            public void onProgress(int sentBlocks, int totalBlocks) {
                //升级中，发送 BlueToothStatusUpdate
                float currentProgress = (float)sentBlocks / (float)totalBlocks;
                if(currentProgress - progress[0] > 0.01 || currentProgress >=1){
                    progress[0] = currentProgress;
                    if(uploadCallback != null){
                        uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdate, String.valueOf(progress[0]));
                    }
                }
            }
        };

        //设备未连接，强制升级
        OAD.startOAD2(context, deviceName, deviceType, binFileName, callback);
    }

    /**
     * 开始V2硬件设备固件升级
     * @param context
     * @param deviceType 设备类型
     * @param deviceName 蓝牙设备名称
     * @param binFileName bin文件路径
     * @param uploadCallback 升级回调
     */
    public static void startUploadV2(Activity context, int deviceType, String deviceName, String binFileName, Callback uploadCallback){
        if(!checkPermission(context)){
            uploadCallback.onStatusChange(deviceType, BlueToothStatusConnectFailed, "startUploadV2 未授权蓝牙!");
            return;
        }

        // 设置BleOtaManager的Context
        BleOtaManager.setContext(context);

        final float[] progress = {0};
        BleOtaManager.OTACallback otaCallback = new BleOtaManager.OTACallback() {
            @Override
            public void onProgress(int currentSent, int total) {
                //升级中，发送 BlueToothStatusUpdate
                float currentProgress = (float)currentSent / (float)total;
                if(currentProgress - progress[0] > 0.01 || currentProgress >= 1){
                    progress[0] = currentProgress;
                    if(uploadCallback != null){
                        uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdate, String.valueOf(progress[0]));
                    }
                }
            }

            @Override
            public void onStatusUpdate(String status) {
                Debug("Unity Android", "蓝牙 startUploadV2.onStatusUpdate(): " + deviceType + " status=" + status);
                // 可以根据需要将状态信息传递给Unity层
            }

            @Override
            public void onSuccess() {
                Debug("Unity Android", "蓝牙 startUploadV2.onSuccess(): " + deviceType + " uploadCallback=" + uploadCallback);
                //升级成功，发送 BlueToothStatusUpdated
                if(uploadCallback != null){
                    uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdated, "");
                }
            }

            @Override
            public void onFailed(int errorCode, String errorMessage) {
                Debug("Unity Android", "蓝牙 startUploadV2.onFailed(): " + deviceType + " errorCode=" + errorCode + " errorMessage=" + errorMessage);
                //升级失败，发送 BlueToothStatusUpdateFail
                // BleOtaManager的错误码信息：
                // ErrorCode.DEVICE_SCAN_TIMEOUT = 303 (搜索超时)
                // ErrorCode.DEVICE_CONNECT_FAILED = 304 (连接失败)
                // ErrorCode.DEVICE_DISCONNECTED = 305 (连接断开)
                // 等等...
                if(uploadCallback != null){
                    uploadCallback.onStatusChange(deviceType, BlueToothStatusUpdateFail, "错误码:" + errorCode + " " + errorMessage);
                }
            }
        };

        //调用V2硬件设备升级
        BleOtaManager.StartOTA(binFileName, deviceName, true, otaCallback);
    }

    @SuppressLint("MissingPermission")
    public static void disconnect(int deviceType) {
        _disconnectAndRemove(deviceType);
    }

    @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
    public static void disconnectAll() {
        Debug("Unity Android", "蓝牙 disconnectAll");
        HashMap<Integer, BlueConnectInfo> map = getDeviceMap();

        synchronized (map) {
            // 遍历map中的所有对象并执行destroy方法
            for (BlueConnectInfo info : map.values()) {
                if (info != null) {
                    try{
                        _disconnectAndReconnect(info.deviceType, BlueToothStatusCutOff);
                        info.mScheduledFuture.cancel(true);
                        info.taskExecutor.shutdownNow();
                        Log.w(TAG,  "结束了 taskExecutor!! 结束了 mScheduledFuture"+info.deviceName);
                    }catch (Exception e){
                        e.printStackTrace();
                    }
                    //注意发送状态消息
                    _updateAndSendBackStatus(info.deviceType, BlueToothStatusCutOff, "手动断开");
                }
            }
            // 清空map
            map.clear();
        }
    }

    @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
    public static void sendMessageToDevice(int type, String message) {
        BlueConnectInfo info = getDeviceMap().get(type);
        if (info == null || info.gatt == null || info.deviceStatus != BlueToothStatusDeviceConnected) {
            Log.w("Unity Android", "蓝牙 SendMessageToDevice gatt = null，装备未连接");
            return;
        }
        Debug("Unity Android", "蓝牙 SendMessageToDevice msg=" + message);
        try {
            _writeData(info, hexStringToByteArray(message));
        } catch (Exception e) {
            Debug("Unity Android", "蓝牙 SendMessageToDevice 出错=" + e.getMessage());
        }
    }

    //-----------------------------------------------------------------------------

    @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
    private static void _writeData(BlueConnectInfo info, byte[] data) {
        try {
            Debug("Unity Android", "写入蓝牙数据 gatt=" + info.gatt + " data=" + data + " dataHex=" + bytesToHexString(data));

            BluetoothGattService service = info.gatt.getService(UUID.fromString(info.serviceUuid[info.currentUuidIndex]));
            BluetoothGattCharacteristic writeCharacteristic = service.getCharacteristic(UUID.fromString(info.writeUuid[info.currentUuidIndex]));
            writeCharacteristic.setValue(data);
            boolean writeSuccess = info.gatt.writeCharacteristic(writeCharacteristic);
            Debug("Unity Android", "写入蓝牙数据 writeSuccess:"+writeSuccess);
            if(!writeSuccess){
                //延迟10毫秒再次发送
                getHandler().postDelayed(() -> {
                    boolean writeSuccess1 = info.gatt.writeCharacteristic(writeCharacteristic);
                    Debug("Unity Android", "写入蓝牙数据【重试】 writeSuccess:"+writeSuccess1);
                }, 30);
            }
        } catch (Exception e) {
            Debug("Unity Android", "写入蓝牙数据 失败" + e.getMessage());
            e.printStackTrace();
        }
    }

    /**
     * 类型以及名字相同的设备是否正在连接
     * @param deviceType
     * @param deviceName
     * @return
     */
    public static boolean _isConnecting(int deviceType, String deviceName){
        if(getDeviceMap().containsKey(deviceType)){
            BlueConnectInfo dev = getDeviceMap().get(deviceType);
            if(dev == null){
                return false;
            }
            return (dev.deviceStatus == BlueToothStatusConnecting || dev.deviceStatus == BlueToothStatusConnected) && deviceName.equals(dev.deviceName);
        }else{
            return false;
        }
    }

    /**
     * 类型及名字相同的设备是否已经连接
     * @param deviceType
     * @param deviceName
     * @return
     */
    public static boolean _isConnected(int deviceType, String deviceName){
        if(getDeviceMap().containsKey(deviceType)){
            BlueConnectInfo dev = getDeviceMap().get(deviceType);
            if(dev == null){
                return false;
            }
            return dev.deviceStatus == BlueToothStatusDeviceConnected && deviceName.equals(dev.deviceName);
        }else{
            return false;
        }
    }

    public static boolean _isConnected(int deviceType){
        if(getDeviceMap().containsKey(deviceType)){
            BlueConnectInfo dev = getDeviceMap().get(deviceType);
            if(dev == null){
                return false;
            }
            return dev.deviceStatus == BlueToothStatusDeviceConnected;
        }else{
            return false;
        }
    }

    public static BlueConnectInfo _getDeviceInfo(int deviceType){
        return getDeviceMap().getOrDefault(deviceType, null);
    }

    /**
     * 更新设备状态，并发送给调用者
     * @param deviceType 设备类型
     * @param status 设备状态
     * @param msg 消息
     * @return
     */
    public static boolean _updateAndSendBackStatus(int deviceType, int status, String msg){
        if(getDeviceMap().containsKey(deviceType)){
            BlueConnectInfo dev = getDeviceMap().get(deviceType);
            if(dev == null){
                Log.w(TAG, "通知 callback.onStatusChange 失败，设备为空! deviceType="+deviceType+" status="+status+" msg="+msg);
                return false;
            }
            dev.deviceStatus = status;
            if(dev.callback == null){
                Log.w(TAG, "通知 callback.onStatusChange 失败，callback为空! deviceType="+deviceType+" status="+status+" msg="+msg);
                return false;
            }
            try{
                Log.i(TAG, "通知 callback.onStatusChange deviceType="+deviceType+" status="+status+" msg="+msg);
                getHandler().post(() -> dev.callback.onStatusChange(deviceType, status, msg));
                return true;
            }catch(Exception e){
                Log.e(TAG, "通知 callback.onStatusChange 异常，"+e.getMessage()+" deviceType="+deviceType+" status="+status+" msg="+msg);
                e.printStackTrace();
                return false;
            }
        }else{
            return false;
        }
    }

    public static boolean _sendBackMessage(int deviceType, int status, String msg){
        if(getDeviceMap().containsKey(deviceType)){
            BlueConnectInfo dev = getDeviceMap().get(deviceType);
            if(dev == null){
                Log.w(TAG, "通知 callback.onMessage 失败，设备为空! deviceType="+deviceType+" status="+status+" msg="+msg);
                return false;
            }
            if(dev.callback == null){
                Log.w(TAG, "通知 callback.onMessage 失败，callback为空! deviceType="+deviceType+" status="+status+" msg="+msg);
                return false;
            }
            try{
                Log.i(TAG, "通知 callback.onMessage deviceType="+deviceType+" status="+status+" msg="+msg);
                getHandler().post(() -> dev.callback.onMessage(deviceType, status, msg));
                return true;
            }catch(Exception e){
                Log.e(TAG, "通知 callback.onMessage 异常，"+e.getMessage()+" deviceType="+deviceType+" status="+status+" msg="+msg);
                e.printStackTrace();
                return false;
            }
        }else{
            return false;
        }
    }

    /**
     * 立即断开设备已经gatt连接(如果已连接)
     * @param deviceType 设备类型
     * @return 是否成功断开
     */
    @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
    public static boolean _disconnectAndReconnect(int deviceType, int status){
        if(getDeviceMap().containsKey(deviceType)){
            BlueConnectInfo dev = getDeviceMap().get(deviceType);
            if(dev == null){
                return false;
            }
            // 更新状态
            dev.deviceStatus = status;
            // 关闭Gatt连接
            try {
                if (dev.gatt != null) {
                    // 先断开再关闭
                    dev.gatt.disconnect();
                    // 添加延迟确保断开完成
                    new Handler(Looper.getMainLooper()).postDelayed(() -> {
                        try {
                            dev.gatt.close();
                        } catch (Exception e) {
                            Log.e(TAG, "Error closing gatt", e);
                        }
                    }, 200); // 200ms延迟
                }
            } catch (Exception e) {
                Log.e(TAG, "Error disconnecting gatt", e);
            }
            forceCleanupStaleConnections(dev.context, dev.deviceName);
            return true;
        }else{
            return false;
        }
    }

    /**
     * 立即断开设备已经gatt连接(如果已连接)
     * @param deviceType 设备类型
     * @return 是否成功断开
     */
    @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
    public static boolean _disconnectAndRemove(int deviceType){
        HashMap<Integer, BlueConnectInfo> map = getDeviceMap();
        synchronized (map) {
            if (!map.containsKey(deviceType)) {
                //打印日志，使用 Debug("Unity Android...
                Debug("Unity Android", "蓝牙 disconnectAndRemove() 设备未连接，不需要断开! deviceType=" + deviceType); 
                return false;
            }

            BlueConnectInfo dev = map.get(deviceType);
            if (dev == null) {
                //打印日志
                Debug("Unity Android", "蓝牙 disconnectAndRemove() 设备信息为空! deviceType=" + deviceType);
                return false;
            }

            // 更新状态
            dev.deviceStatus = BlueToothStatusCutOff;
            _updateAndSendBackStatus(dev.deviceType, dev.deviceStatus, "手动断开");

            // 停止所有相关任务
            try {
                if (dev.mScheduledFuture != null) {
                    dev.mScheduledFuture.cancel(true);
                    Log.w(TAG,  "结束了 mScheduledFuture"+dev.deviceName);
                }
                if (dev.taskExecutor != null) {
                    dev.taskExecutor.shutdownNow();
                    Log.w(TAG,  "结束了 taskExecutor!! "+dev.deviceName);
                }
            } catch (Exception e) {
                Log.e(TAG, "Error stopping tasks", e);
            }

            // 处理Gatt连接
            try {
                if (dev.gatt != null) {
                    // 先断开再关闭
                    dev.gatt.disconnect();
                    // 添加延迟确保断开完成
                    new Handler(Looper.getMainLooper()).postDelayed(() -> {
                        try {
                            dev.gatt.close();
                        } catch (Exception e) {
                            Log.e(TAG, "Error closing gatt", e);
                        }
                    }, 200); // 200ms延迟
                }
            } catch (Exception e) {
                Log.e(TAG, "Error disconnecting gatt", e);
            } finally {
                map.remove(deviceType);
            }

            // 强制清理残留连接
            forceCleanupStaleConnections(dev.context, dev.deviceName);

            return true;
        }
    }

    @RequiresPermission(allOf = {Manifest.permission.BLUETOOTH_SCAN, Manifest.permission.BLUETOOTH_CONNECT})
    public static synchronized void _startConnect(Activity activity, int deviceType, String deviceName, int timeoutMillis, Callback callback) {
        //正在连接中，不执行操作
        if(_isConnecting(deviceType, deviceName)){
            Debug("Unity Android", "蓝牙 正在连接中，不做任何响应:["+deviceName+"] type="+deviceType);
            return;
        }

        if(_isConnected(deviceType, deviceName)){
            Debug("Unity Android", "蓝牙 已经连接，不再重复连接，仅发送通知:["+deviceName+"] type="+deviceType);
            _updateAndSendBackStatus(deviceType, BlueToothStatusDeviceConnected, "重复连接");
            return;
        }

        //开始连接，关闭现有连接
        boolean disconnectResult = _disconnectAndRemove(deviceType);
        Debug("Unity Android", "蓝牙 startConnect() disconnectResult:"+disconnectResult);
        //设置状态为连接中
        BlueConnectInfo blueConnectInfo = new BlueConnectInfo(activity, deviceName, deviceType, timeoutMillis, callback);
        blueConnectInfo.deviceStatus = BlueToothStatusConnecting;
        blueConnectInfo.taskExecutor  = Executors.newSingleThreadScheduledExecutor();
        blueConnectInfo.mScheduledFuture = blueConnectInfo.taskExecutor.scheduleWithFixedDelay(new Runnable() {
            @RequiresPermission(allOf = {Manifest.permission.BLUETOOTH_SCAN, Manifest.permission.BLUETOOTH_CONNECT})
            @Override
            public void run() {
                // 检查是否需要继续执行
                if (Thread.currentThread().isInterrupted()) {
                    Debug("Unity Android", "蓝牙 连接任务结束 deviceType="+deviceType+" deviceName="+deviceName);
                    return;
                }
                if(_getDeviceInfo(deviceType) == null){
                    Debug("Unity Android", "蓝牙 连接任务结束 设备信息为空!");
                    return;
                }
                boolean connectSyncResult = _connectSync(activity, deviceType);
                Debug("Unity Android", "蓝牙 connectSync() connectSyncResult = "+connectSyncResult+" deviceType="+deviceType+" deviceName="+deviceName);
            }
        }, 0, 4, TimeUnit.SECONDS); // 初始延迟0，每隔4秒执行
        getDeviceMap().put(deviceType, blueConnectInfo);
    }

    /**
     * 这个方法会重复调用，直到用户手动断开设备连接(4秒钟检查一次，是否需要重连)
     * @param activity
     * @param deviceType 需要持续连接的设备类型
     */
    @RequiresPermission(allOf = {Manifest.permission.BLUETOOTH_CONNECT, Manifest.permission.BLUETOOTH_SCAN})
    public static boolean _connectSync(Activity activity, int deviceType){
        final long CurrentThreadId = Thread.currentThread().getId();
        try{
            BlueConnectInfo deviceInfo = null;

            //========================== 防备连接时，已经断开连接 ==================================
            deviceInfo = _getDeviceInfo(deviceType);
            if(deviceInfo == null){
                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 连接时出现未知异常! connectSync() 设备信息为空，不做任何响应!");
                return false;
            }

            if (deviceInfo.deviceStatus == BlueToothStatusCutOff) {
                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 connectSync() 用户手动断开，不再重连:" + deviceInfo+" 重新断开设备!");
                _disconnectAndRemove(deviceType);
                return false;
            }
            //==================================================================================


            //如果蓝牙已经关闭，断开所有连接
            // 首先检查设备是否支持蓝牙
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
            if (bluetoothAdapter == null) {
                // 设备不支持蓝牙
                Log.i(TAG, " connectSync() 蓝牙适配器为空! 设置设备状态为断开 "+deviceInfo.deviceName);
                deviceInfo.deviceStatus = BlueToothStatusConnectFailed;
                _updateAndSendBackStatus(deviceInfo.deviceType, deviceInfo.deviceStatus, "蓝牙适配器为空!");
                return false;
            } else {
                if (bluetoothAdapter.isEnabled()) {
                    // 蓝牙已打开
                } else {
                    // 蓝牙已关闭
                    Log.i(TAG, " connectSync() 蓝牙适配器已关闭! 设置设备状态为断开 "+deviceInfo.deviceName);
                    deviceInfo.deviceStatus = BlueToothStatusConnectFailed;
                    _updateAndSendBackStatus(deviceInfo.deviceType, deviceInfo.deviceStatus, "蓝牙适配器未打开!");
                    return false;
                }
            }

            if (deviceInfo.deviceStatus == BlueToothStatusDeviceConnected) {
                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 connectSync() 设备已连接，不做任何响应! "+deviceInfo.deviceName);
                return true;
            }

            //搜索设备
            final HashMap<String, BluetoothDevice> findDevicesMap = new HashMap<>();
            final boolean[] isScanning = {true};
            ScanFilter.Builder builder = new ScanFilter.Builder();
            builder.setDeviceName(deviceInfo.deviceName);
            ScanFilter scanFilter = builder.build();
            BluetoothManager bluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
            //高功耗模式，扫描速度快
            ScanSettings scanSettings = new ScanSettings.Builder().setScanMode(ScanSettings.SCAN_MODE_LOW_LATENCY).build();
            BluetoothLeScanner scanner = bluetoothManager.getAdapter().getBluetoothLeScanner();
            ScanCallback innerCallback = new ScanCallback() {
                @Override
                public void onBatchScanResults(List<ScanResult> results) {
                    super.onBatchScanResults(results);
                    if (results != null) {
                        for(ScanResult result : results){
                            if(result != null && result.getDevice() != null){
                                findDevicesMap.put(result.getDevice().getAddress(), result.getDevice());
                            }
                        }
                    }
                }

                @SuppressLint("MissingPermission")
                @Override
                public void onScanResult(int callbackType, ScanResult result) {
                    super.onScanResult(callbackType, result);
                    if (result != null && result.getDevice() != null) {
                        findDevicesMap.put(result.getDevice().getAddress(), result.getDevice());
                    }
                }

                @Override
                public void onScanFailed(int errorCode) {
                    super.onScanFailed(errorCode);
                    isScanning[0] = false;
                }
            };

            Log.i(TAG+"(CurrentThreadId:"+CurrentThreadId+")", "Unity Android connectSync() 开始搜索设备->"+deviceInfo.deviceName+"...");
            scanner.startScan(Collections.singletonList(scanFilter), scanSettings, innerCallback);
            //等待搜索完成
            for(int t=0; t<deviceInfo.timeoutMillis; t+=300){
                try{
                    // 检查是否需要继续执行
                    if (deviceInfo.mScheduledFuture.isCancelled() || deviceInfo.taskExecutor.isTerminated()) {
                        Log.w(TAG, "连接任务结束 Interrupted() deviceType="+deviceType);
                        scanner.flushPendingScanResults(innerCallback);
                        scanner.stopScan(innerCallback);
                        return false;
                    }
                    Thread.sleep(300);
                }catch (Exception ignored){}
                Log.i(TAG+"(CurrentThreadId:"+CurrentThreadId+")", "Unity Android connectSync() 等待搜索结束"+(t/1000f)+"秒... "+deviceInfo.deviceName);
                if(!findDevicesMap.isEmpty()){
                    scanner.flushPendingScanResults(innerCallback);
                    scanner.stopScan(innerCallback);
                    isScanning[0] = false;
                    break;
                }
                if(!isScanning[0]){
                    break;
                }
            }

            //========================== 防备连接时，已经断开连接 ==================================
            deviceInfo = _getDeviceInfo(deviceType);
            if(deviceInfo == null){
                scanner.flushPendingScanResults(innerCallback);
                scanner.stopScan(innerCallback);
                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 连接时出现未知异常! connectSync() 设备信息为空，不做任何响应!");
                return false;
            }

            if (deviceInfo.deviceStatus == BlueToothStatusCutOff) {
                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 connectSync() 用户手动断开，不再重连:" + deviceInfo+" 重新断开设备!");
                _disconnectAndRemove(deviceType);
                return false;
            }
            //==================================================================================

            List<BluetoothDevice> devices = new ArrayList<>();
            for (Map.Entry<String, BluetoothDevice> entry : findDevicesMap.entrySet()) {
                devices.add(entry.getValue());
            }
            if(devices.isEmpty()){
                scanner.flushPendingScanResults(innerCallback);
                scanner.stopScan(innerCallback);
                Log.i(TAG+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 未找到设备"+deviceInfo.deviceName);
                _updateAndSendBackStatus(deviceType, BlueToothStatusConnectOver, "设备搜索超时!");
                return false;
            }
            for(BluetoothDevice device : devices){
                Log.i(TAG+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 找到的设备: 地址="+device.getAddress()+" name="+device.getName());
            }
            BluetoothDevice device = devices.get(0);

            //开始连接
            final boolean[] servicesDiscoveredSuccess = {false};
            //赋值gatt
            deviceInfo.gatt = device.connectGatt(activity, false, new BluetoothGattCallback() {
                @Override
                public void onPhyUpdate(BluetoothGatt gatt, int txPhy, int rxPhy, int status) {
                    super.onPhyUpdate(gatt, txPhy, rxPhy, status);
                }

                @SuppressLint("MissingPermission")
                @Override
                public void onCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
                    super.onCharacteristicChanged(gatt, characteristic);

                    BlueConnectInfo deviceInfo = _getDeviceInfo(deviceType);
                    if(deviceInfo == null){
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 连接时出现未知异常! onCharacteristicChanged() 设备信息为空，不做任何响应!");
                        try{
                            gatt.disconnect();
                            gatt.close();
                        }catch(Exception e){
                            e.printStackTrace();
                        }
                        return;
                    }
                    final byte[] data = characteristic.getValue();
                    if (data != null && data.length > 0) {
                        String dataStr = bytesToHexString(data);
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "原始蓝牙消息:" + gatt.getDevice().getName() + " ->" + dataStr);
                        _sendBackMessage(deviceInfo.deviceType, deviceInfo.deviceStatus, dataStr);
                    }
                }

                @Override
                public void onCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, int status) {
                    Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 onCharacteristicWrite status=" + status + " characteristic=" + characteristic.getUuid());
                    super.onCharacteristicWrite(gatt, characteristic, status);
                    String hex = bytesToHexString(characteristic.getValue());
                    if(status == BluetoothGatt.GATT_SUCCESS){//写入成功
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 onCharacteristicWrite 数据写入成功:"+hex);
                    }else if (status == BluetoothGatt.GATT_FAILURE){
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 onCharacteristicWrite 数据写入失败:"+hex);
                    }else if (status == BluetoothGatt.GATT_WRITE_NOT_PERMITTED){
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 onCharacteristicWrite 禁止写入:"+hex);
                    }
                }

                @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
                @Override
                public void onServicesDiscovered(BluetoothGatt gatt, int status) {
                    super.onServicesDiscovered(gatt, status);
                    Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 蓝牙 onServicesDiscovered status=" + status);
                    //========================== 防备连接时，已经断开连接 ==================================
                    BlueConnectInfo deviceInfo = _getDeviceInfo(deviceType);
                    if(deviceInfo == null){
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 连接时出现未知异常! onServicesDiscovered() 设备信息为空，不做任何响应!");
                        return;
                    }

                    if (deviceInfo.deviceStatus == BlueToothStatusCutOff) {
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 onServicesDiscovered() 用户手动断开，不再重连:" + deviceInfo+" 重新断开设备!");
                        _disconnectAndRemove(deviceType);
                        return;
                    }
                    //==================================================================================

                    if (status == BluetoothGatt.GATT_SUCCESS) {
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙 status=GATT_SUCCESS");
                        BluetoothGattService service = null;
                        BluetoothGattCharacteristic notifyCharacteristic = null;
                        BluetoothGattCharacteristic writeCharacteristic = null;
                        //打印 deviceInfo.serviceUuid.length 的长度以及内容
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙 serviceUuid.length=" + deviceInfo.serviceUuid.length + " 设备名:" + deviceInfo.deviceName);
                        // 尝试每个UUID组合，直到找到匹配的
                        int groupIndex = 0;
                        for (int i = 0; i < deviceInfo.serviceUuid.length; i++) {
                            //打印日志，正在获取 service uuid=...
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙 正在尝试获取 service uuid=" + deviceInfo.serviceUuid[i] + " 设备名:" + deviceInfo.deviceName + " UUID组合索引:" + i + " groupIndex:" + groupIndex);
                            service = gatt.getService(UUID.fromString(deviceInfo.serviceUuid[i]));
                            if (service != null) {
                                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙 找到了蓝牙Service:" + deviceInfo.serviceUuid[i] + " 设备名:" + deviceInfo.deviceName + " UUID组合索引:" + i);
                                //找到服务，继续查找特征值
                                notifyCharacteristic = service.getCharacteristic(UUID.fromString(deviceInfo.notifyUuid[i]));
                                writeCharacteristic = service.getCharacteristic(UUID.fromString(deviceInfo.writeUuid[i]));
                                
                                // 检查是否找到了完整的UUID组合
                                if (notifyCharacteristic != null && writeCharacteristic != null) {
                                    deviceInfo.currentUuidIndex = i; // 记录使用的UUID组合索引
                                    Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙 成功匹配UUID组合索引:" + i);
                                    break;
                                } else {
                                    // 当前组合不完整，继续尝试下一个
                                    service = null;
                                    notifyCharacteristic = null;
                                    writeCharacteristic = null;
                                }
                            }else{
                                //打印错误日志，service uuid获取失败
                                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙 未找到该Service:" + deviceInfo.serviceUuid[i] + " 设备名:" + deviceInfo.deviceName + " UUID组合索引:" + i);
                            }
                            groupIndex += 1;
                        }

                        if (notifyCharacteristic != null) {
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙notifyCharacteristic找到" + notifyCharacteristic.getUuid() + " 设备名:" + deviceInfo.deviceName);
                            //使能Notify
                            Debug("Unity Android", "调用 setCharacteristicNotification 使能Notify!");
                            gatt.setCharacteristicNotification(notifyCharacteristic, true);
                            //打印出来 Build.VERSION.SDK_INT > Build.VERSION_CODES.Q 的对比数据
                            Debug("Unity Android", "Build.VERSION.SDK_INT = " + Build.VERSION.SDK_INT + ", Build.VERSION_CODES.Q = " + Build.VERSION_CODES.Q);
                            //打印Android系统名称以及系统版本
                            Debug("Unity Android", "Android系统名称 = " + Build.VERSION.RELEASE + ", 系统版本 = " + Build.VERSION.SDK_INT);
                            //打印Android品牌名称和型号
                            Debug("Unity Android", "Android品牌名称 = " + Build.BRAND + ", 型号 = " + Build.MODEL);
                            // This is specific to BLE SPP Notify.
                            final BluetoothGattCharacteristic finalNotifyCharacteristic = notifyCharacteristic;
                            Debug("Unity Android", "Build.VERSION.SDK_INT > Build.VERSION_CODES.Q This is specific to BLE SPP Notify.");
                            // 自动适配0x2902/0x2901
                            BluetoothGattDescriptor targetDesc = null;
                            for (BluetoothGattDescriptor desc : finalNotifyCharacteristic.getDescriptors()) {
                                String uuidStr = desc.getUuid().toString().toLowerCase();
                                Debug("Unity Android", "发现Descriptor: " + uuidStr);
                                if (uuidStr.contains("00002902")) {
                                    targetDesc = desc;
                                    Debug("Unity Android", "使用标准CCCD 0x2902");
                                    break;
                                } else if (uuidStr.contains("00002901")) {
                                    targetDesc = desc;
                                    Debug("Unity Android", "使用自定义Descriptor 0x2901");
                                    // 不break，优先用0x2902
                                }
                            }
                            if (targetDesc != null) {
                                boolean descWrite = targetDesc.setValue(BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE);
                                boolean writeResult = gatt.writeDescriptor(targetDesc);
                                Debug("Unity Android", "已尝试写入Descriptor: " + targetDesc.getUuid() + "，setValue结果: " + descWrite + ", writeDescriptor结果: " + writeResult);
                            } else {
                                Debug("Unity Android", "未找到可用的通知Descriptor (0x2902/0x2901)");
                            }
                        }
                        Debug("Unity Android", "检查服务是否成功找到....");

                        if (writeCharacteristic != null) {
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙writeCharacteristic找到" + writeCharacteristic.getUuid());
                        } else {
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙writeCharacteristic未找到 通知并关闭设备");
                        }
                        if (notifyCharacteristic == null) {
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙notifyCharacteristic未找到 通知并关闭设备");
                        }

                        if (service == null || notifyCharacteristic == null || writeCharacteristic == null) {
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙Service不存在 连接失败 通知并关闭设备");
                            gatt.close();
                        } else {
                            servicesDiscoveredSuccess[0] = true;
                        }
                    } else {
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "onServicesDiscovered() 蓝牙Service发现未成功:" + status);
                    }
                }

                @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
                @Override
                public void onConnectionStateChange(BluetoothGatt gatt, int status, int newState) {
                    super.onConnectionStateChange(gatt, status, newState);

                    //========================== 防备连接时，已经断开连接 ==================================
                    BlueConnectInfo deviceInfo = _getDeviceInfo(deviceType);
                    if(deviceInfo == null){
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 连接时出现未知异常! onServicesDiscovered() 设备信息为空，不做任何响应!");
                        return;
                    }

                    if (deviceInfo.deviceStatus == BlueToothStatusCutOff) {
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 onServicesDiscovered() 用户手动断开，不再重连:" + deviceInfo+" 重新断开设备!");
                        _disconnectAndRemove(deviceType);
                        return;
                    }
                    //==================================================================================

                    if(gatt.getDevice().getName() == null){
                        Log.e(TAG+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() device.connectGatt 连接错误, 设备名称为空!");
                        _updateAndSendBackStatus(deviceType, BlueToothStatusConnectFailed, "设备名称为空!");
                        return;
                    }

                    if (status == BluetoothGatt.GATT_SUCCESS) {
                        Log.i(TAG+"(CurrentThreadId:"+CurrentThreadId+")", " > 蓝牙链接状态改变 status=BluetoothGatt.GATT_SUCCESS:" + deviceInfo.deviceName);
                        if (newState == BluetoothProfile.STATE_CONNECTED) {
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 蓝牙链接状态改变 newState=BluetoothProfile.STATE_CONNECTED:" + deviceInfo.deviceName+" 开始调用 discoverServices...");
                            //开始搜索服务，搜索到服务并且使能特征值，才算是链接成功
                            gatt.discoverServices();
                        } else if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                            _updateAndSendBackStatus(deviceType, BlueToothStatusDisConnect, "BluetoothProfile.STATE_DISCONNECTED");
                            Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 蓝牙链接状态改变 newState=BluetoothProfile.STATE_DISCONNECTED:" + deviceInfo.deviceName);
                        }
                    } else if (status == BluetoothGatt.GATT_INSUFFICIENT_AUTHORIZATION) {
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 蓝牙链接状态改变 GATT 授权不足, 等待重连!");
                        _updateAndSendBackStatus(deviceType, BlueToothStatusConnectFailed, "BluetoothGatt.GATT_INSUFFICIENT_AUTHORIZATION");
                        _disconnectAndReconnect(deviceType, BlueToothStatusConnectFailed);
                    } else if (status == BluetoothGatt.GATT_READ_NOT_PERMITTED) {
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 蓝牙链接状态改变 GAT读未授权:" + deviceInfo.deviceName);
                        _updateAndSendBackStatus(deviceType, BlueToothStatusConnectFailed, "BluetoothGatt.GATT_READ_NOT_PERMITTED");
                        _disconnectAndReconnect(deviceType, BlueToothStatusConnectFailed);
                    } else if (status == BluetoothGatt.GATT_FAILURE) {
                        Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 蓝牙链接状态改变 连接失败:" + deviceInfo.deviceName);
                        _updateAndSendBackStatus(deviceType, BlueToothStatusConnectFailed, "BluetoothGatt.GATT_FAILURE");
                        _disconnectAndReconnect(deviceType, BlueToothStatusConnectFailed);
                    }
                }
            });

            //等待20秒钟发现服务
            for(int t=0; t<20*1000; t+=200){
                try{
                    // 检查是否需要继续执行
                    if (deviceInfo.mScheduledFuture.isCancelled() || deviceInfo.taskExecutor.isTerminated()) {
                        Log.w(TAG, "连接任务结束 Interrupted() deviceType="+deviceType);
                        return false;
                    }
                    Thread.sleep(200);
                }catch (Exception ignored){}
                Log.i(TAG+"(CurrentThreadId:"+CurrentThreadId+")", "connectSync() > 等待发现服务结束"+(t/1000f)+"秒... "+deviceInfo.deviceName);
                if(servicesDiscoveredSuccess[0]){
                    break;
                }
            }

            //========================== 防备连接时，已经断开连接 ==================================
            deviceInfo = _getDeviceInfo(deviceType);
            if(deviceInfo == null){
                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 连接时出现未知异常! onServicesDiscovered() 设备信息为空，不做任何响应!");
                return false;
            }

            if (deviceInfo.deviceStatus == BlueToothStatusCutOff) {
                Debug("Unity Android"+"(CurrentThreadId:"+CurrentThreadId+")", "蓝牙 onServicesDiscovered() 用户手动断开，不再重连:" + deviceInfo+" 重新断开设备!");
                _disconnectAndRemove(deviceType);
                return false;
            }
            //==================================================================================

            if(!servicesDiscoveredSuccess[0]){
                deviceInfo.gatt.disconnect();
                deviceInfo.gatt.close();
                deviceInfo.gatt = null;
                _updateAndSendBackStatus(deviceType, BlueToothStatusConnectFailed, "设备service搜索超时!");
                Log.i(TAG+"(CurrentThreadId:"+CurrentThreadId+")", "connectSync() > 蓝牙设备 service查找超时! "+deviceInfo.deviceName);
                _disconnectAndReconnect(deviceType, BlueToothStatusConnectFailed);
                return false;
            }
            //延迟半秒钟
            Thread.sleep(500);
            _updateAndSendBackStatus(deviceType, BlueToothStatusDeviceConnected, "设备连接成功! "+deviceInfo.deviceName);
            return true;
        }catch (Exception e){
            e.printStackTrace();
            _updateAndSendBackStatus(deviceType, BlueToothStatusConnectFailed, "设备连接异常:"+e.getMessage());
            _disconnectAndReconnect(deviceType, BlueToothStatusConnectFailed);
            Log.e(TAG+"(CurrentThreadId:"+CurrentThreadId+")", " > connectSync() 连接异常"+e.getMessage());
            return false;
        }
    }

    private static void Debug(String tag, String msg){
        Log.d(TAG+" > "+tag, msg);
    }


    private static final char[] HEX_ARRAY = "0123456789ABCDEF".toCharArray();

    private static String bytesToHexString(byte[] bytes) {
        if (bytes == null) {
            return null;
        }
        char[] hexChars = new char[bytes.length * 2];
        for (int j = 0; j < bytes.length; j++) {
            int v = bytes[j] & 0xFF;
            hexChars[j * 2] = HEX_ARRAY[v >>> 4];
            hexChars[j * 2 + 1] = HEX_ARRAY[v & 0x0F];
        }
        return new String(hexChars);
    }

    /**
     * 16进制字符串转字节数组
     * @param s must be an even-length string.
     * @return
     */
    public static byte[] hexStringToByteArray(String s) {
        int len = s.length();
        byte[] data = new byte[len / 2];
        for (int i = 0; i < len; i += 2) {
            data[i / 2] = (byte) ((Character.digit(s.charAt(i), 16) << 4)
                    + Character.digit(s.charAt(i + 1), 16));
        }
        return data;
    }

    /**
     * 强制关闭蓝牙连接
     * @param context Context
     * @param targetDeviceName 设备名
     */
    @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
    private static void forceCleanupStaleConnections(Context context, String targetDeviceName) {
        //打印日志. 使用 UnityLog
        Debug("Unity Android", "forceCleanupStaleConnections() 尝试清理残留连接: " + targetDeviceName);
        BluetoothManager bluetoothManager = (BluetoothManager) context.getSystemService(Context.BLUETOOTH_SERVICE);
        if (bluetoothManager == null){
            //打印日志
            Debug("Unity Android", "forceCleanupStaleConnections() BluetoothManager 为空!");
            return;
        }

        List<BluetoothDevice> connectedDevices = bluetoothManager.getConnectedDevices(BluetoothProfile.GATT);
        Log.i(TAG, "Connected devices count: " + connectedDevices.size());

        for (BluetoothDevice device : connectedDevices) {
            String deviceName = device.getName();
            if (deviceName == null || !deviceName.equals(targetDeviceName)) {
                continue;
            }

            Log.i(TAG, "Found stale connection to: " + deviceName);

            // 方法1: 使用反射强制关闭
            try {
                Method removeBondMethod = BluetoothDevice.class.getMethod("removeBond");
                removeBondMethod.invoke(device);
                Log.i(TAG, "Invoked removeBond on: " + deviceName);
            } catch (Exception e) {
                Log.e(TAG, "Reflection failed for removeBond", e);
            }

            // 方法2: 创建临时Gatt连接强制断开
            BluetoothGatt tempGatt = device.connectGatt(context, false, new BluetoothGattCallback() {
                @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
                @Override
                public void onConnectionStateChange(BluetoothGatt gatt, int status, int newState) {
                    if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                        try {
                            gatt.close();
                            Log.i(TAG, "Temp gatt closed for: " + gatt.getDevice().getName());
                        } catch (Exception e) {
                            Log.e(TAG, "Error closing temp gatt", e);
                        }
                    }
                }
            }, BluetoothDevice.TRANSPORT_LE);

            if (tempGatt != null) {
                tempGatt.disconnect();
                new Handler(Looper.getMainLooper()).postDelayed(() -> {
                    try {
                        tempGatt.close();
                    } catch (Exception e) {
                        Log.e(TAG, "Error closing temp gatt in handler", e);
                    }
                }, 300);
            }
        }
    }
}
