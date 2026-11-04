package cn.com.glsa.app.fightgame.oad;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothManager;
import android.bluetooth.le.BluetoothLeScanner;
import android.bluetooth.le.ScanCallback;
import android.bluetooth.le.ScanFilter;
import android.bluetooth.le.ScanResult;
import android.bluetooth.le.ScanSettings;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Environment;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;
import android.os.Message;
import android.util.Log;

import androidx.annotation.RequiresPermission;

import java.io.FileInputStream;
import java.io.InputStream;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import cn.com.glsa.app.fightgame.GLSABlueToothDevice;

/**
 * 设备OAD升级
 */
public class OAD {
    static final String TAG = "Unity "+OAD.class.getSimpleName();

    private static final int FILE_ACTIVITY_REQ = 0;

    // Programming parameters
    private static final short OAD_CONN_INTERVAL = 10; // 12.5 msec
    private static final short OAD_SUPERVISION_TIMEOUT = 100; // 1 second
    private static final int PKT_INTERVAL = 48; // Milliseconds
    private static final int GATT_WRITE_TIMEOUT = 100; // Milliseconds

    private static final int WRITE_TRY_TIMES = 5;

    private static final int FILE_BUFFER_SIZE = 0x40000;
    private static final String FW_CUSTOM_DIRECTORY = Environment.DIRECTORY_DOWNLOADS;
    private static final String FW_FILE_A = "Image-A.bin";
    private static final String FW_FILE_B = "Image-B.bin";

    public static final int OAD_BLOCK_SIZE = 16;
    public static final int HAL_FLASH_WORD_SIZE = 4;
    public static final int OAD_BUFFER_SIZE = 2 + OAD_BLOCK_SIZE;
    private static final int OAD_IMG_HDR_SIZE = 8;

    /**
     * 给设备发送升级指令，并开始升级
     * @param activity
     * @param deviceType
     * @param imagePath
     * @param oadCallback
     */
    @RequiresPermission(Manifest.permission.BLUETOOTH_CONNECT)
    public static void startOAD1(Activity activity, int deviceType, String imagePath, OADCallback oadCallback){
        //测试设备已经链接成功，开始升级
        GLSABlueToothDevice.BlueConnectInfo info = GLSABlueToothDevice.getBlueConnectInfo(deviceType);
        if(info == null){
            oadCallback.onError("设备未连接!");
            return;
        }
        Log.i(TAG, "发送启动OAD命令...");
        GLSABlueToothDevice.sendMessageToDevice(deviceType, "5A0007230510000000A5");

//        Log.i(TAG, "禁用GLSABlueToothDevice的自动重连 status=BlueToothStatusCutOff");
//        info.setDeviceStatus(GLSABlueToothDevice.BlueToothStatusCutOff);

        BluetoothManager bluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
        @SuppressLint("MissingPermission") List<BluetoothDevice> connectedDevices = bluetoothManager.getConnectedDevices(BluetoothGatt.GATT);
        if(connectedDevices.size() == 0){
            oadCallback.onError("设备未连接");
            return;
        }
        BluetoothDevice device = connectedDevices.get(0);
        if(device == null){
            oadCallback.onError("设备未连接");
            return;
        }
        String address = device.getAddress();
        Log.i(TAG, "3秒后开始链接OAD设倍 蓝牙地址:"+address);
        Handler handler1 = new Handler(Looper.getMainLooper());
        handler1.postDelayed(() -> {
            GLSABlueToothDevice.disconnect(deviceType);
            handler1.postDelayed(() -> {
                GLSABlueToothDevice.disconnect(deviceType);
                startOAD(activity, address, deviceType, imagePath, oadCallback);
            }, 2000);
        }, 1000);
    }

    /**
     * 根据Image A的名字找到设备，直接升级
     * @param activity
     * @param deviceName
     * @param deviceType
     * @param imagePath
     * @param oadCallback
     */
    public static void startOAD2(Activity activity, String deviceName, int deviceType, String imagePath, OADCallback oadCallback){
        startOAD(activity, deviceName, deviceType, imagePath, oadCallback);
    }

    /**
     * 执行oad升级
     * @param activity
     * @param oadDeviceName mac地址或设备名称
     * @param imagePath bin镜像的路径
     */
    private static void startOAD(Activity activity, String oadDeviceName, int deviceType, String imagePath, OADCallback oadCallback){
        //等待线程执行完毕
        Handler handler = new Handler(Looper.getMainLooper(), msg -> {
            if(msg.what == 0){
                Log.i(TAG, "升级成功!!!!");
                oadCallback.onSuccess();
            }else{
                Log.e(TAG, "升级失败！ "+msg.obj);
                oadCallback.onError(msg.obj.toString());
            }
            return true;
        });

        new Thread(() -> {
            //搜索所有设备，然后逐个升级
            //搜索设备，等待30秒
            final HashMap<String, BluetoothDevice> oadDevicesMap = new HashMap<>();
            final boolean[] isScanning = {true};
            ScanFilter.Builder builder = new ScanFilter.Builder();
            if(oadDeviceName.contains(":")){
                builder.setDeviceAddress(oadDeviceName);
            }else{
                builder.setDeviceName(oadDeviceName);
            }
            ScanFilter scanFilter = builder.build();
            BluetoothManager bluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
            BluetoothAdapter bluetoothAdapter = bluetoothManager.getAdapter();
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
                                oadDevicesMap.put(result.getDevice().getAddress(), result.getDevice());
                            }
                        }
                    }
                }

                @SuppressLint("MissingPermission")
                @Override
                public void onScanResult(int callbackType, ScanResult result) {
                    super.onScanResult(callbackType, result);
                    if (result != null && result.getDevice() != null) {
                        oadDevicesMap.put(result.getDevice().getAddress(), result.getDevice());
                    }
                }

                @Override
                public void onScanFailed(int errorCode) {
                    super.onScanFailed(errorCode);
                    isScanning[0] = false;
                }
            };

            Log.i(TAG, "OAD过程: 开始搜索设备->"+oadDeviceName+"...");
            scanner.startScan(Collections.singletonList(scanFilter), scanSettings, innerCallback);
            //等待搜索完成(30秒)
            for(int t=0; t<6; t++){
                try{
                    Thread.sleep(5000);
                }catch (Exception e){}
                Log.i(TAG, "OAD过程: 等待搜索结束"+(t+1)+"秒...");
                if(oadDevicesMap.size() >= 1){
                    scanner.stopScan(innerCallback);
                    isScanning[0] = false;
                    break;
                }
                if(!isScanning[0]){
                    break;
                }
            }
            List<BluetoothDevice> oadDevices = new ArrayList<>();
            for (Map.Entry<String, BluetoothDevice> entry : oadDevicesMap.entrySet()) {
                oadDevices.add(entry.getValue());
            }
            if(oadDevices.size() == 0){
                Message msg = Message.obtain();
                msg.what = -1;
                msg.obj = "设备未找到";
                handler.sendMessage(msg);
                return;
            }
            for(BluetoothDevice device : oadDevices){
                Log.i(TAG, "OAD过程: 设备列表: "+device+" 地址="+device.getAddress());
            }
            int successCount = 0;
            for(BluetoothDevice device : oadDevices){
                Log.i(TAG, "OAD过程: 进入 OADFunction 设备="+device+" 地址="+device.getAddress().toString());
                boolean success = OADFunction(handler, activity, device, deviceType, imagePath, oadCallback);
                if(success){
                    successCount += 1;
                    break;
                }
            }
            if(successCount == 0){
                //没有搜索到设备
                Message msg = Message.obtain();
                msg.what = -1;
                msg.obj = "没有搜索到设备";
                handler.sendMessage(msg);
            }
        }).start();
    }

    private static boolean OADFunction(Handler handler, Activity activity, BluetoothDevice device, int deviceType, String imagePath, OADCallback oadCallback){
        Message msg = Message.obtain();
        msg.what = -1;
        final BluetoothLeService[] mBluetoothLeService = {null};
        final BluetoothGattService[] oadServices = {null};
        final BluetoothGattService[] connControlServices = {null};

        IntentFilter bluetoothIntentFilter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
        bluetoothIntentFilter.addAction(BluetoothLeService.ACTION_GATT_CONNECTED);
        bluetoothIntentFilter.addAction(BluetoothLeService.ACTION_GATT_DISCONNECTED);

        BroadcastReceiver receiver = null;
        BroadcastReceiver gattUpdateReceiver = null;
        BroadcastReceiver oadGattUpdateReceiver = null;

        try{
            //检查蓝牙
            Log.i(TAG, "OAD过程: 检查蓝牙...");
            if (!activity.getPackageManager().hasSystemFeature(
                    PackageManager.FEATURE_BLUETOOTH_LE)) {
                throw new Exception("设备不支持蓝牙");
            }

            Log.i(TAG, "OAD过程: 检查蓝牙权限...");
            if(!GLSABlueToothDevice.checkPermission(activity)){
                throw new Exception("没有权蓝牙权限");
            }

            Log.i(TAG, "OAD过程: 初始化蓝牙适配器...");
            // Initializes a Bluetooth adapter. For API level 18 and above, get a
            // reference to BluetoothAdapter through BluetoothManager.
            BluetoothManager bluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
            BluetoothAdapter bluetoothAdapter = bluetoothManager.getAdapter();

            // Checks if Bluetooth is supported on the device.
            if (bluetoothAdapter == null) {
                throw new Exception("未找到蓝牙适配器");
            }

            Log.i(TAG, "OAD过程: 启动BluetoothLeService...");
            boolean[] isConnectingService = new boolean[]{true};

            Intent bindIntent = new Intent(activity, BluetoothLeService.class);
            activity.startService(bindIntent);

            if(!activity.bindService(bindIntent, new ServiceConnection() {
                @Override
                public void onServiceConnected(ComponentName name, IBinder service) {
                    mBluetoothLeService[0] = ((BluetoothLeService.LocalBinder) service)
                            .getService();
                    if (!mBluetoothLeService[0].initialize()) {
                        Log.e(TAG, "Unable to initialize BluetoothLeService");
                        mBluetoothLeService[0] = null;
                    }
                    isConnectingService[0] = false;
                }

                @Override
                public void onServiceDisconnected(ComponentName name) {
                    mBluetoothLeService[0] = null;
                    isConnectingService[0] = false;
                }
            }, Context.BIND_AUTO_CREATE)){
                throw new Exception("蓝牙服务启动失败");
            }
            //等待服务启动
            for(int t=0; t<10; t++){
                Thread.sleep(500);
                Log.i(TAG, "OAD过程: 等待BluetoothLeService服务启动"+(t*0.5)+"秒...");
                if(!isConnectingService[0]){
                    break;
                }
            }
            BluetoothLeService bluetoothLeService = mBluetoothLeService[0];
            if(bluetoothLeService == null){
                throw new Exception("蓝牙服务启动失败");
            }

            List<BluetoothDevice> connectedDevices = new ArrayList<>();
            for(int i=0; i<10; i++){
                Thread.sleep(1000);
                //检查是否是否已经链接
                connectedDevices = bluetoothManager.getConnectedDevices(BluetoothGatt.GATT);

                Log.i(TAG, "当前链接设备数量:"+connectedDevices.size());

                if(connectedDevices.size() == 0){
                    break;
                }

                for (BluetoothDevice dev : connectedDevices) {
                    dev.fetchUuidsWithSdp();
                    BluetoothGatt gatt = dev.connectGatt(activity, false, new BluetoothGattCallback() {
                        @Override
                        public void onConnectionStateChange(BluetoothGatt gatt, int status, int newState) {}
                    });
                    if(gatt != null){
                        gatt.disconnect();
                        gatt.close();
                        Log.i(TAG,"请求断开"+dev.getName());
                    }
                }
            }

            Log.i(TAG, "OAD过程: 链接设备"+device.getName()+"...");

            boolean[] isConnectingDevice = new boolean[]{true};
            boolean[] isConnectSuccess = new boolean[]{false};

            receiver = new BroadcastReceiver() {
                @Override
                public void onReceive(Context context, Intent intent) {
                    final String action = intent.getAction();
                    Log.i(TAG, "OAD接收到 service消息: action="+action);
                    if (BluetoothLeService.ACTION_GATT_CONNECTED.equals(action)) {
                        // GATT connect
                        int status = intent.getIntExtra(BluetoothLeService.EXTRA_STATUS, BluetoothGatt.GATT_FAILURE);
                        isConnectSuccess[0] = status == BluetoothGatt.GATT_SUCCESS;
                        isConnectingDevice[0] = false;
                    } else if (BluetoothLeService.ACTION_GATT_DISCONNECTED.equals(action)) {
                        // GATT disconnect
                        int status = intent.getIntExtra(
                                BluetoothLeService.EXTRA_STATUS,
                                BluetoothGatt.GATT_FAILURE);
                        //链接断开
                        isConnectingDevice[0] = false;
                    } else {
                        Log.w(TAG, "Unknown action: " + action);
                    }
                }
            };

            final IntentFilter gattUpdateIntentFilter = new IntentFilter();
            gattUpdateIntentFilter.addAction(BluetoothLeService.ACTION_GATT_SERVICES_DISCOVERED);
            gattUpdateIntentFilter.addAction(BluetoothLeService.ACTION_DATA_NOTIFY);
            gattUpdateIntentFilter.addAction(BluetoothLeService.ACTION_DATA_WRITE);
            gattUpdateIntentFilter.addAction(BluetoothLeService.ACTION_DATA_READ);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                activity.registerReceiver(receiver, bluetoothIntentFilter, Context.RECEIVER_EXPORTED);
            }else{
                activity.registerReceiver(receiver, bluetoothIntentFilter);
            }

            Log.i(TAG, "链接设备: address="+device.getAddress());

            boolean ok = bluetoothLeService.connect(device.getAddress());
            if (!ok) {
                throw new Exception("设备链接失败!");
            }
            //等待链接成功
            for(int t=0; t<10; t++){
                Thread.sleep(1000);
                Log.i(TAG, "OAD过程: 等待GATT Service 链接..."+(t+1)+"秒...");
                if(!isConnectingDevice[0]){
                    break;
                }
            }
            if (!isConnectSuccess[0]) {
                throw new Exception("GATT Service 链接失败!");
            }

            Log.i(TAG, "OAD过程: 查找GATT服务...");

            boolean[] isFindingServices = new boolean[]{true};
            List<BluetoothGattService> serviceList = new ArrayList<>();

            final String[] characteristicName = {""};

            gattUpdateReceiver = new BroadcastReceiver() {
                @Override
                public void onReceive(Context context, Intent intent) {
                    final String action = intent.getAction();
//                        Log.i(TAG, "gattUpdateReceiver返回:action="+action);
                    int status = intent.getIntExtra(BluetoothLeService.EXTRA_STATUS,
                            BluetoothGatt.GATT_SUCCESS);
//                        Log.i(TAG, "gattUpdateReceiver返回:status="+status);
                    if(BluetoothLeService.ACTION_DATA_READ.equals(action)){
                        //读取到了名字
                        if (status == BluetoothGatt.GATT_SUCCESS) {
                            byte[] value = intent.getByteArrayExtra(BluetoothLeService.EXTRA_DATA);
                            String uuid = intent.getStringExtra(BluetoothLeService.EXTRA_UUID);
                            String deviceName = new String(value, Charset.forName("UTF-8"));
                            if(deviceName.startsWith("TX") || deviceName.startsWith("QX")){
                                characteristicName[0] += deviceName;
                            }
                            Log.i(TAG,"尝试读取名字 onCharacteristicRead: uuid="+uuid+" deviceName="+deviceName);
                        }
                    }
                    if (BluetoothLeService.ACTION_GATT_SERVICES_DISCOVERED
                            .equals(action)) {
                        if (status == BluetoothGatt.GATT_SUCCESS) {
                            try {
                                serviceList.addAll(bluetoothLeService.getSupportedGattServices());
                            } catch (Exception e) {
                                e.printStackTrace();
                            }
                            Log.i(TAG, "serviceList.size="+serviceList.size());

                            for (int i = 0; i < serviceList.size(); i++) {
                                BluetoothGattService srv = serviceList.get(i);
                                Log.i(TAG, "BluetoothGattService="+srv.getUuid().toString());
                                if (srv.getUuid().equals(GattInfo.OAD_SERVICE_UUID)) {
                                    Log.i(TAG, "找到了GattInfo.OAD_SERVICE_UUID :" +GattInfo.OAD_SERVICE_UUID);
                                    oadServices[0] = srv;
                                }
                                if (srv.getUuid().equals(GattInfo.CC_SERVICE_UUID)) {
                                    Log.i(TAG, "找到了GattInfo.CC_SERVICE_UUID :" +GattInfo.CC_SERVICE_UUID);
                                    connControlServices[0] = srv;
                                }
                            }
                            isFindingServices[0] = false;
                        }
                    }
                }
            };
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                activity.registerReceiver(gattUpdateReceiver, gattUpdateIntentFilter, Context.RECEIVER_EXPORTED);
            }else{
                activity.registerReceiver(gattUpdateReceiver, gattUpdateIntentFilter);
            }
            BluetoothGatt gatt = BluetoothLeService.getBtGatt();
            Thread.sleep(1000);
            if(!gatt.discoverServices()){
                throw new Exception("GATT服务查找失败!");
            }
            Thread.sleep(300);
            if(!gatt.discoverServices()){
                throw new Exception("GATT服务查找失败!");
            }
            //等待10秒钟
            for(int t=0; t<20; t++){
                Thread.sleep(1000);
                Log.i(TAG, "OAD过程: 等待查找GATT服务结束..."+(t+1)+"秒...");
                if(!isFindingServices[0]){
                    break;
                }
            }
            if (serviceList.size() == 0) {
                throw new Exception("GATT服务数量为0!");
            }
            if(oadServices[0] == null){
                throw new Exception("未找到OAD_SERVICE_UUID:"+GattInfo.OAD_SERVICE_UUID.toString());
            }
            //不需要
//                if(connControlServices[0] == null){
//                    throw new Exception("未找到CC_SERVICE_UUID:"+GattInfo.CC_SERVICE_UUID.toString());
//                }

            //等待1秒钟，等待名字返回
            Thread.sleep(1200);
            boolean isHeadDevice = characteristicName[0].startsWith("TX");
            boolean isGunDevice = characteristicName[0].startsWith("QX");

            Log.i(TAG, "OAD过程: isHeadDevice="+isHeadDevice+" characteristicName[0]="+characteristicName[0]);
            Log.i(TAG, "OAD过程: isGunDevice="+isGunDevice+" characteristicName[0]="+characteristicName[0]);
            if(isHeadDevice && deviceType == 1){
                //需要扫描下一个设备
                Log.w(TAG, ">>>OAD过程: characteristicName不匹配，尝试升级下一个设备！");
                return false;
            }
            if(isGunDevice && deviceType == 2){
                //需要扫描下一个设备
                Log.w(TAG, ">>>OAD过程: characteristicName不匹配，尝试升级下一个设备！");
                return false;
            }

            IntentFilter oadIntentFilter = new IntentFilter();
            oadIntentFilter.addAction(BluetoothLeService.ACTION_DATA_NOTIFY);
            oadIntentFilter.addAction(BluetoothLeService.ACTION_DATA_WRITE);

            // Characteristics list
            List<BluetoothGattCharacteristic> charListOad = oadServices[0].getCharacteristics();

            if(charListOad.size() != 2){
                throw new Exception("OAD_SERVICE_UUID的特征值列表长度不为2");
            }

            BluetoothGattCharacteristic charIdentify = charListOad.get(0);
            BluetoothGattCharacteristic charBlock = charListOad.get(1);

            Log.i(TAG, "OAD过程: TI_OAD_IMAGE_NOTIFY:"+charIdentify.getUuid());
            Log.i(TAG, "OAD过程: TI_OAD_IMAGE_BLOCK_REQUEST:"+charBlock.getUuid());

            ImgHdr fileImgHdr = new ImgHdr();
            ImgHdr targImgHdr = new ImgHdr();
            boolean[] writeError = new boolean[]{false};

            oadGattUpdateReceiver = new BroadcastReceiver() {
                @Override
                public void onReceive(Context context, Intent intent) {

                    final String action = intent.getAction();
//                        Log.d(TAG, "action: " + action);

                    if (BluetoothLeService.ACTION_DATA_NOTIFY.equals(action)) {
                        byte[] value = intent
                                .getByteArrayExtra(BluetoothLeService.EXTRA_DATA);
                        String uuidStr = intent
                                .getStringExtra(BluetoothLeService.EXTRA_UUID);
                        Log.d(TAG, "loadFile 接收到数据: " + bytesToHexString(value));
                        Log.d(TAG, "loadFile 接收到数据: uuidStr=" + uuidStr);
                        if(uuidStr.equals(charBlock.getUuid().toString())){
                            Log.i(TAG, "loadFile 接收到FFC2数据");
                        }
                        if (uuidStr.equals(charIdentify.getUuid().toString())) {
                            Log.i(TAG, "loadFile 接收到FFC1数据");
                            // Image info notification
                            targImgHdr.ver = (short) Conversion.buildUint16(value[1],
                                    value[0]);
                            targImgHdr.imgType = ((targImgHdr.ver & 1) == 1) ? 'B'
                                    : 'A';
                            targImgHdr.len = Conversion
                                    .buildUint16(value[3], value[2]);
                            Log.d(TAG, "11targImgHdr.len: " + targImgHdr.len);
                            if (targImgHdr.len < 0)
                                targImgHdr.len = (long) (targImgHdr.len + 0x10000);
                            Log.d(TAG, "22targImgHdr.len: " + targImgHdr.len);

                            Log.d(TAG, "loadFile TI_OAD_IMAGE_NOTIFY: targImgHdr.ver=" + targImgHdr.ver);
                            Log.d(TAG, "loadFile TI_OAD_IMAGE_NOTIFY: targImgHdr.imgType=" + targImgHdr.imgType);
                            Log.d(TAG, "loadFile TI_OAD_IMAGE_NOTIFY: targImgHdr.len=" + targImgHdr.len);
                        }
                    } else if (BluetoothLeService.ACTION_DATA_WRITE.equals(action)) {
                        int status = intent.getIntExtra(
                                BluetoothLeService.EXTRA_STATUS,
                                BluetoothGatt.GATT_SUCCESS);
                        if (status != BluetoothGatt.GATT_SUCCESS) {
                            byte[] data = intent.getByteArrayExtra(BluetoothLeService.EXTRA_DATA);
                            Log.e(TAG, "Write failed: " + status+" 数据:"+bytesToHexString(data));
                            writeError[0] = true;
                        }else{
//                                Log.i(TAG, "Write OK.");
                        }
                    }
                }
            };
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                activity.registerReceiver(oadGattUpdateReceiver, oadIntentFilter, Context.RECEIVER_EXPORTED);
            }else{
                activity.registerReceiver(oadGattUpdateReceiver, oadIntentFilter);
            }

            Thread.sleep(500);

            Log.i(TAG, "loadFile 发送查询版本号请求...");

            // Enable notification
            ok = bluetoothLeService.setCharacteristicNotification(charIdentify, true);
            Log.i(TAG, "Enable notification:"+ok);
            Thread.sleep(GATT_WRITE_TIMEOUT);
            Thread.sleep(GATT_WRITE_TIMEOUT);
            if(!ok){
                throw new Exception("Enable charIdentify notification失败");
            }

            // Prepare data for request (try image A and B respectively, only one of
            // them will give a notification with the image info)
            charIdentify.setValue(new byte[]{0});
            ok = tryWriteCharacteristic(bluetoothLeService, charIdentify);
            Thread.sleep(GATT_WRITE_TIMEOUT);
            if (ok){
                charIdentify.setValue(new byte[]{1});
                ok = tryWriteCharacteristic(bluetoothLeService, charIdentify);
            }
            Thread.sleep(GATT_WRITE_TIMEOUT);
            if (!ok){
                throw new Exception("Failed to get target info");
            }
            //等待返回版本号
            Thread.sleep(1500);
            //如果版本号不为A返回失败
            if('A' != targImgHdr.imgType){
                throw new Exception("版本号错误! 不是A镜像");
            }

            Log.i(TAG, "OAD过程: 读取bin文件:"+imagePath);
            final byte[] fileBuffer = new byte[FILE_BUFFER_SIZE];
            final byte[] oadBuffer = new byte[OAD_BUFFER_SIZE];
            // Read the file raw into a buffer
            InputStream stream = new FileInputStream(imagePath);
            stream.read(fileBuffer, 0, fileBuffer.length);
            stream.close();

            // Show image info
            fileImgHdr.ver = (short) Conversion.buildUint16(fileBuffer[5], fileBuffer[4]);

            Log.i(TAG,"loadFile: fileImgHdr.ver:"+fileImgHdr.ver);

            Log.i(TAG, "buff7:" + fileBuffer[7] + ",buff6:" + fileBuffer[6]);
            fileImgHdr.len = Conversion
                    .buildUint16(fileBuffer[7], fileBuffer[6]);
            if (fileImgHdr.len < 0)
                fileImgHdr.len = (long) (fileImgHdr.len + 0x10000);
            Log.i(TAG, "len:" + fileImgHdr.len);

            Log.i(TAG,"loadFile: fileImgHdr.len:"+fileImgHdr.len);

            fileImgHdr.imgType = ((fileImgHdr.ver & 1) == 1) ? 'B' : 'A';

            Log.i(TAG,"loadFile: fileImgHdr.imgType:"+fileImgHdr.imgType);

            System.arraycopy(fileBuffer, 8, fileImgHdr.uid, 0, 4);

            Log.i(TAG,"loadFile: mTargImgHdr.imgType:"+targImgHdr.imgType);

            // Verify image types
            boolean ready = fileImgHdr.imgType != targImgHdr.imgType;

            if(!ready){
                throw new Exception("Incompatible image, select alternative!");
            }

            // Expected duration
            int estDuration = (int) (((PKT_INTERVAL * fileImgHdr.len * 4) / OAD_BLOCK_SIZE) / 1000);

            Log.i(TAG,"Image " + fileImgHdr.imgType + " selected.");
            Log.i(TAG,"Ready to program device!");

            ProgInfo mProgInfo = new ProgInfo();

            // Prepare image notification
            byte[] buf = new byte[OAD_IMG_HDR_SIZE + 2 + 2];
            buf[0] = Conversion.loUint16(fileImgHdr.ver);
            buf[1] = Conversion.hiUint16(fileImgHdr.ver);
            Log.i(TAG, "startProgramming-->len:" + fileImgHdr.len);
            buf[3] = Conversion.hiUint16(fileImgHdr.len);
            Log.i(TAG, "buf[2]:" + buf[2]);
            Log.i(TAG, "buf[3]:" + buf[3]);
            System.arraycopy(fileImgHdr.uid, 0, buf, 4, 4);

            Log.i(TAG, "写入镜像版本号和镜像大小...");
            // Send image notification
            charIdentify.setValue(buf);
            tryWriteCharacteristic(bluetoothLeService, charIdentify);
            Thread.sleep(GATT_WRITE_TIMEOUT);
            Log.i(TAG, "开始写入固件...");

            // Initialize stats
            mProgInfo.reset(fileImgHdr);

            // Start send the packet
            while (true){
                if(writeError[0]){
                    break;
                }
                mProgInfo.mTick++;
                if (mProgInfo.iBlocks < mProgInfo.nBlocks) {
                    // Prepare block
                    oadBuffer[0] = Conversion.loUint16(mProgInfo.iBlocks);
                    oadBuffer[1] = Conversion.hiUint16(mProgInfo.iBlocks);
                    System.arraycopy(fileBuffer, mProgInfo.iBytes, oadBuffer, 2, OAD_BLOCK_SIZE);

                    // Send block
                    charBlock.setValue(oadBuffer);
                    boolean success = tryWriteCharacteristic(bluetoothLeService, charBlock);

                    if (success) {
                        // Update stats
                        mProgInfo.iBlocks++;
                        mProgInfo.iBytes += OAD_BLOCK_SIZE;
                        oadCallback.onProgress(mProgInfo.iBlocks, mProgInfo.nBlocks);
                        if(mProgInfo.iBlocks % 100 == 0){
                            Log.i(TAG, "数据写入进度:"+((mProgInfo.iBlocks * 100) / mProgInfo.nBlocks));
                        }
                    } else {
                        // Check if the device has been prematurely disconnected
                        writeError[0] = true;
                        Log.e(TAG, "数据发送失败，结束!");
                        break;
                    }
                } else {
                    break;
                }
                mProgInfo.iTimeElapsed += PKT_INTERVAL;
                Thread.sleep(PKT_INTERVAL);
            }

            //数据写入完成
            //升级完毕，断开所有蓝牙设备
            connectedDevices = bluetoothManager.getConnectedDevices(BluetoothGatt.GATT);
            Log.i(TAG, "升级完毕。当前链接设备数量:"+connectedDevices.size());

            for (BluetoothDevice dev : connectedDevices) {
                if(dev == null) continue;
                dev.fetchUuidsWithSdp();
                gatt = dev.connectGatt(activity, false, new BluetoothGattCallback() {
                    @Override
                    public void onConnectionStateChange(BluetoothGatt gatt, int status, int newState) {
                        try{
                            gatt.disconnect();
                            gatt.close();
                        }catch (Exception e){e.printStackTrace();}
                    }
                });
                if(gatt != null){
                    try{
                        gatt.disconnect();
                        gatt.close();
                    }catch(Exception e){e.printStackTrace();}
                }
                Log.i(TAG,"升级完毕。请求断开"+dev.getName());
            }

            if(writeError[0]){
                msg.what = -1;
                msg.obj = "数据写入出错";
                handler.sendMessage(msg);
            }else{
                handler.sendEmptyMessage(0);
            }
        }catch (Exception e){
            Log.i(TAG,"OAD升级异常!");
            e.printStackTrace();
            if(e.getMessage() != null){
                Log.i(TAG+"Unity ", e.getMessage()+"");
            }
            msg.what = -1;
            msg.obj = "升级失败";
            handler.sendMessage(msg);
        }finally {
            //销毁
            if (mBluetoothLeService[0] != null) {
                mBluetoothLeService[0].close();
            }
            if(receiver != null){
                activity.unregisterReceiver(receiver);
            }
            if(gattUpdateReceiver != null){
                activity.unregisterReceiver(gattUpdateReceiver);
            }
            if(oadGattUpdateReceiver != null){
                activity.unregisterReceiver(oadGattUpdateReceiver);
            }
        }
        return true;
    }

//    private static boolean tryWriteCharacteristic(BluetoothLeService service, BluetoothGattCharacteristic c, byte b){
//        for(int i=0; i<WRITE_TRY_TIMES; i++){
//            boolean success = service.writeCharacteristic(c, b);
//            if(success) return true;
//            Log.e(TAG, "单个字节写入失败，重试"+(i+1)+"...");
//            try {
//                Thread.sleep(GATT_WRITE_TIMEOUT);
//            } catch (InterruptedException e) {
//                throw new RuntimeException(e);
//            }
//        }
//        return false;
//    }

    private static boolean tryWriteCharacteristic(BluetoothLeService service, BluetoothGattCharacteristic c){
        for(int i=0; i<WRITE_TRY_TIMES; i++){
            boolean success = service.writeCharacteristic(c);
            if(success) return true;
            Log.i(TAG, "写入失败，重试"+(i+1)+"...");
            try {
                Thread.sleep(GATT_WRITE_TIMEOUT);
            } catch (InterruptedException e) {
                throw new RuntimeException(e);
            }
        }
        return false;
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
}
