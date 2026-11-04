package cn.com.glsa.app.fightgame;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Surface;
import android.view.View;
import android.view.WindowManager;
import android.view.WindowInsets;

import androidx.annotation.NonNull;

import com.baidu.location.BDAbstractLocationListener;
import com.baidu.location.BDLocation;
import com.baidu.location.LocationClient;
import com.baidu.location.LocationClientOption;;
import java.util.List;
import com.unity3d.player.UnityPlayerActivity;

public class MainActivity extends UnityPlayerActivity {
    static final String TAG = "Unity Android "+MainActivity.class.getSimpleName();

    // 供 WatchdogReceiver 判断本 Activity 是否还存活（进程被杀后重新拉起的进程里该字段会重置为 false）
    public static volatile boolean isRunning = false;
    // 供 BootLaunchReceiver 判断是否已经进入前台主界面（onPause 后为 false）
    public static volatile boolean isForeground = false;

    private LocationClient locationClient;
    private SensorServiceUtil sensorServiceUtil;
    private BaiduLocationCallback locationCallback;
    //当前角度变化小于这个值不通知
    private float minDiffDegree = 5;
    
    public static int safeInsetsLeft = 0;
    public static int safeInsetsTop = 0;
    public static int safeInsetsRight = 0;
    public static int safeInsetsBottom = 0;

    public interface BaiduLocationCallback{
        void onStartError(String message);
        void onReceiveLocation(double latitude, double longitude);
        void onReceiveDegree(double degree);
    }

    private GLSABlueToothDevice.PermissionCallback callback = null;
    private GLSABlueToothDevice.EnableBluetoothCallback enableBluetoothCallback = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.i(TAG, "MainActivity 启动");
        isRunning = true;
        WatchdogReceiver.scheduleNext(this);
        WatchdogReceiver.scheduleBackup(this);
        sensorServiceUtil = new SensorServiceUtil(this);
        setFullScreenAndHorizontal();
        getBaiduMetaDataFromManifest();

        // 屏幕安全区处理
        View rootView = findViewById(android.R.id.content);
        rootView.setOnApplyWindowInsetsListener(new View.OnApplyWindowInsetsListener() {
            @Override
            public WindowInsets onApplyWindowInsets(View v, WindowInsets insets) {
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.P) {
                    if (insets != null && insets.getDisplayCutout() != null) {
                        safeInsetsLeft = insets.getDisplayCutout().getSafeInsetLeft();
                        safeInsetsTop = insets.getDisplayCutout().getSafeInsetTop();
                        safeInsetsRight = insets.getDisplayCutout().getSafeInsetRight();
                        safeInsetsBottom = insets.getDisplayCutout().getSafeInsetBottom();
                        Log.d(TAG, "UIScreenAdapter OnApplyWindowInsets()回调 WindowInsets安全区: left=" + safeInsetsLeft + ", top=" + safeInsetsTop + ", right=" + safeInsetsRight + ", bottom=" + safeInsetsBottom);
                    }
                }
                return insets;
            }
        });
    }

    public void requestBluetoothPermission(GLSABlueToothDevice.PermissionCallback callback){
        Log.i(TAG, "Unity 蓝牙 requestBluetoothPermission callback="+callback);
        this.callback = callback;
        List<String> permissions = GLSABlueToothDevice.getPermissions();
        String[] permissionsArray = permissions.toArray(new String[0]);
        requestPermissions(permissionsArray, 1001);
    }

    /**
     * 开启蓝牙
     * @param activity
     * @param requestCode
     */
    @SuppressLint("MissingPermission")
    public void enableBluetooth(GLSABlueToothDevice.EnableBluetoothCallback callback) {
        this.enableBluetoothCallback = callback;
        Log.i(TAG, "Unity 蓝牙 enableBluetooth callback="+callback);
        Intent intent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
        startActivityForResult(intent, 1001);
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        Log.i(TAG, "Unity 蓝牙 onRequestPermissionsResult callback="+callback+" requestCode="+requestCode);
        if(requestCode == 1001){
            if(callback != null){
                callback.onResult(permissions, grantResults);
            }
        }
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    @Override
    protected void onResume() {
        super.onResume();
        isForeground = true;
        if(enableBluetoothCallback != null){
            enableBluetoothCallback.onResult();
            enableBluetoothCallback = null;
        }
    }

    @Override
    protected void onPause() {
        isForeground = false;
        super.onPause();
    }

    @Override
    protected void onDestroy() {
        isRunning = false;
        isForeground = false;
        stopLocation();
        super.onDestroy();
    }

    public void stopLocation(){
        Log.e(TAG, "百度定位 stopLocation");
        if(locationClient != null) locationClient.stop();
        locationClient = null;
        if(sensorServiceUtil != null){
            sensorServiceUtil.stopSensorService();
        }
    }

    public void startLocation(int scanSpan, float minDiffDegree, BaiduLocationCallback callback){
        try{
            Log.e(TAG, "百度定位初始化 step 1");
            if(locationClient != null){
                locationClient.stop();
            }
            this.minDiffDegree = minDiffDegree;
            locationCallback = callback;
            sensorServiceUtil.startSensorService();
            //定位初始化
            locationClient = new LocationClient(this);

            Log.e(TAG, "百度定位初始化 step 2");

            //通过LocationClientOption设置LocationClient相关参数
            LocationClientOption option = new LocationClientOption();
            option.setLocationMode(LocationClientOption.LocationMode.Hight_Accuracy);
            option.setScanSpan(scanSpan);
            option.setCoorType("bd09ll"); // 设置坐标类型

            //设置locationClientOption
            locationClient.setLocOption(option);

            Log.e(TAG, "百度定位初始化 step 3");

            locationClient.registerLocationListener(new BDAbstractLocationListener() {
                @Override
                public void onReceiveLocation(BDLocation bdLocation) {
                    double longitude = bdLocation.getLongitude();
                    double latitude = bdLocation.getLatitude();
                    Log.e(TAG, "百度定位: (latitude,longitude)="+latitude+","+longitude);
                    if(locationCallback != null){
                        locationCallback.onReceiveLocation(latitude, longitude);
                    }
                }
            });

            Log.e(TAG, "百度定位初始化 step 4");

            //开启地图定位图层
            locationClient.start();
            Log.e(TAG, "百度定位初始化 已启动..");
        }catch (Exception e){
            e.printStackTrace();
            try{
                if(locationClient != null) locationClient.stop();
            }catch (Exception e1){
                e1.printStackTrace();
            }
            if(e.getMessage() != null){
                Log.e(TAG, "百度定位异常: "+e.getMessage());
                callback.onStartError(e.getMessage());
            }else{
                callback.onStartError("百度定位启动异常");
            }
        }
    }


    private class SensorServiceUtil {
        Context context;
        SensorManager sensorManager;
        private long lastSendTime = 0;

        private SensorServiceUtil(Context context) {
            this.context = context.getApplicationContext();
            sensorManager = (SensorManager) context.getSystemService(Context.SENSOR_SERVICE);
        }

        public void startSensorService() {
            sensorManager.registerListener(sensorEventListener, sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER), SensorManager.SENSOR_DELAY_GAME);
            sensorManager.registerListener(sensorEventListener, sensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD), SensorManager.SENSOR_DELAY_GAME);
        }

        public void stopSensorService(){
            sensorManager.unregisterListener(sensorEventListener);
        }

        private SensorEventListener sensorEventListener = new SensorEventListener() {
            float[] accelerometerValues = new float[3];
            float[] magneticFieldValues = new float[3];
            float olDegree;

            @Override
            public void onSensorChanged(SensorEvent event) {
                //每秒钟最多更新15次(太少了不流畅)
                if(System.currentTimeMillis() - lastSendTime < 66){
                    return;
                }
                lastSendTime = System.currentTimeMillis();
                if (event.sensor.getType() == Sensor.TYPE_MAGNETIC_FIELD) {
                    magneticFieldValues = event.values.clone();
                }
                if (event.sensor.getType() == Sensor.TYPE_ACCELEROMETER) {
                    accelerometerValues = event.values.clone();
                }

                float[] values = new float[3];
                float[] R = new float[9];
                SensorManager.getRotationMatrix(R, null, accelerometerValues, magneticFieldValues);
                SensorManager.getOrientation(R, values);
                float degree = (float) Math.toDegrees(values[0]);

                if (Math.abs(degree - olDegree) > minDiffDegree) {
                    olDegree = degree;
                    if(locationCallback != null){
                        int rotation = ((WindowManager)getSystemService(Context.WINDOW_SERVICE))
                                .getDefaultDisplay().getRotation();
                        //正向1=>Surface.ROTATION_90，反向3=>Surface.ROTATION_270
                        float rotateDegree = 0;
                        if(rotation == Surface.ROTATION_90){
                            rotateDegree = 180-degree - 90; //减去90度，因为手机是横向拿着的
                        }else{
                            rotateDegree = -degree - 45;
                        }
                        locationCallback.onReceiveDegree(rotateDegree);
                    }
                }
            }

            @Override
            public void onAccuracyChanged(Sensor sensor, int accuracy) {
            }
        };
    }

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if(keyCode == KeyEvent.KEYCODE_BACK){
            return true;
        }
        return super.onKeyDown(keyCode, event);
    }

    @Override
    public boolean onKeyUp(int keyCode, KeyEvent event) {
        if(keyCode == KeyEvent.KEYCODE_BACK){
            return true;
        }
        return super.onKeyUp(keyCode, event);
    }

    /**
     * 设置全屏和强制横屏
     */
    private void setFullScreenAndHorizontal(){
        if (getWindow() != null) {
            getWindow().addFlags(WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS);
        }
        //始终横屏，由重力传感器决定是哪个方向的横屏
        if(getRequestedOrientation() != ActivityInfo.SCREEN_ORIENTATION_SENSOR_LANDSCAPE){
            setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_SENSOR_LANDSCAPE);
        }
    }
    
    public void getBaiduMetaDataFromManifest() {
        try {
            // 获取 PackageManager 实例
            android.content.pm.PackageManager packageManager = getPackageManager();



            // 获取当前应用的 ApplicationInfo

            android.content.pm.ApplicationInfo applicationInfo = packageManager.getApplicationInfo(getPackageName(), android.content.pm.PackageManager.GET_META_DATA);



            // 从 ApplicationInfo 中获取 MetaData

            if (applicationInfo.metaData != null) {

                // 根据键名获取对应的值

                Object value = applicationInfo.metaData.get("com.baidu.lbsapi.API_KEY");



                // 如果值不为空且是字符串类型，返回字符串值

                Log.i("Unity", "百度API_KEY找到了:"+value);

            }else{
                Log.i("Unity", "百度API_KEY未找到:");
            }

        } catch (android.content.pm.PackageManager.NameNotFoundException e) {

            e.printStackTrace();
            Log.i("Unity", "百度API_KEY未找到:"+e.getMessage());
        }
    }
}

