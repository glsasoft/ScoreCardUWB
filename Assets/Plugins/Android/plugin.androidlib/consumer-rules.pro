# UVC USB 摄像头插件入口与辅助类。
# 这些类被 Unity C# 侧通过 AndroidJavaClass.CallStatic 反射调用，
# 主工程若开启 minify / R8，必须 keep 住，否则签名会被改写或类被裁掉。
-keep public class com.serenegiant.unity.uvc.UvcUnityPlugin {
    native <methods>;
    public static *;
}
-keep public class com.serenegiant.unity.uvc.UvcPreviewManager {
    public *;
}
-keep public class com.serenegiant.unity.uvc.UvcHelper {
    public static *;
}
-keep public class com.serenegiant.unity.uvc.GlTextureManager {
    public *;
}
-keep public class com.serenegiant.unity.uvc.UvcDevice { *; }
