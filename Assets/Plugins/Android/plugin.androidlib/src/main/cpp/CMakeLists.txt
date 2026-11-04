cmake_minimum_required(VERSION 3.22.1)
project(uvc_unity_plugin)

# 库名要与 Java 端 System.loadLibrary("uvc_unity_plugin") 一致
add_library(uvc_unity_plugin SHARED
    egl_capture.cpp
    v4l2_capture.cpp
    jni_uvc.cpp
)

target_compile_features(uvc_unity_plugin PRIVATE cxx_std_17)
target_compile_options(uvc_unity_plugin PRIVATE -Wall -Wextra -Wno-unused-parameter)

# AImageDecoder 在 API 30+ 提供；本插件 minSdk=24 全局生效，但目标设备是 RK3588/Android 12，
# 实际调用 AImageDecoder_* 时设备已支持。为防 NDK 头文件守卫拦截，显式声明 API 30。
target_compile_definitions(uvc_unity_plugin PRIVATE __ANDROID_API__=30)

target_link_libraries(uvc_unity_plugin
    EGL
    GLESv2
    android
    jnigraphics
    log
)
