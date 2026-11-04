// JNI 绑定层：把 Java 端 com.serenegiant.unity.uvc.UvcUnityPlugin 的 native 方法
// 实现转发到 V4l2Capture 实例。
//
// 设计要点（M0：保证 Unity 主线程 60fps 不被任何 V4L2/USB 协商阻塞）：
//   1. nativeOpenDevice  立即返回；真正的 V4L2 init/STREAMON 在 V4l2Capture worker 线程
//   2. nativeCloseDevice 立即返回；把实例放入 g_reclaimQueue，由 reclaimer 后台线程
//      慢慢 join + munmap + close（可能耗 ~100-200ms），不阻塞调用方
//   3. nativeHasFirstFrame / nativeGetState 用于 C# 端轮询启动进度
//   4. nativeUploadFrame 由 GlTextureManager 的 GL handler 线程异步调用，不参与主线程

#include "v4l2_capture.h"

#include <android/log.h>
#include <jni.h>

#include <condition_variable>
#include <deque>
#include <memory>
#include <mutex>
#include <string>
#include <thread>
#include <unordered_map>

#define LOG_TAG "UvcJni"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,  LOG_TAG, __VA_ARGS__)
#define LOGW(...) __android_log_print(ANDROID_LOG_WARN,  LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

namespace {

// textureId → V4l2Capture 实例
std::mutex                                                  g_capturesMtx;
std::unordered_map<jint, std::unique_ptr<uvc::V4l2Capture>> g_captures;

// reclaimer 后台线程：消费已 requestStop 的 capture 实例，调用 joinAndRelease
std::mutex                                          g_reclaimMtx;
std::condition_variable                             g_reclaimCv;
std::deque<std::unique_ptr<uvc::V4l2Capture>>       g_reclaimQueue;
std::thread                                         g_reclaimThread;
bool                                                g_reclaimStarted = false;
bool                                                g_reclaimShouldExit = false;

void reclaimThreadFn() {
    LOGI("reclaim thread: started");
    for (;;) {
        std::unique_ptr<uvc::V4l2Capture> victim;
        {
            std::unique_lock<std::mutex> lk(g_reclaimMtx);
            g_reclaimCv.wait(lk, [] {
                return !g_reclaimQueue.empty() || g_reclaimShouldExit;
            });
            if (g_reclaimShouldExit && g_reclaimQueue.empty()) {
                break;
            }
            victim = std::move(g_reclaimQueue.front());
            g_reclaimQueue.pop_front();
        }
        if (victim) {
            victim->joinAndRelease();
            victim.reset();
        }
    }
    LOGI("reclaim thread: exit");
}

void ensureReclaimRunning() {
    std::lock_guard<std::mutex> lk(g_reclaimMtx);
    if (g_reclaimStarted) return;
    g_reclaimStarted = true;
    g_reclaimShouldExit = false;
    g_reclaimThread = std::thread(reclaimThreadFn);
    g_reclaimThread.detach();
}

void enqueueReclaim(std::unique_ptr<uvc::V4l2Capture> victim) {
    if (!victim) return;
    ensureReclaimRunning();
    std::lock_guard<std::mutex> lk(g_reclaimMtx);
    g_reclaimQueue.push_back(std::move(victim));
    g_reclaimCv.notify_one();
}

uvc::V4l2Capture* findLocked(jint textureId) {
    auto it = g_captures.find(textureId);
    return it == g_captures.end() ? nullptr : it->second.get();
}

std::string jstr(JNIEnv* env, jstring s) {
    if (s == nullptr) return {};
    const char* c = env->GetStringUTFChars(s, nullptr);
    std::string r = c ? c : "";
    if (c) env->ReleaseStringUTFChars(s, c);
    return r;
}

}  // namespace

extern "C" {

JNIEXPORT jstring JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeQueryCapabilities(
        JNIEnv* env, jclass /*cls*/, jstring jDevPath) {
    std::string dev = jstr(env, jDevPath);
    uvc::V4l2Capture::Caps caps = uvc::V4l2Capture::queryCaps(dev);
    char buf[160];
    snprintf(buf, sizeof(buf),
             "{\"width\":%d,\"height\":%d,\"fps\":%d}",
             caps.width, caps.height, caps.fps);
    return env->NewStringUTF(buf);
}

/**
 * 异步打开摄像头：立即返回，真正初始化在 V4l2Capture 的 worker 线程。
 * 旧实例若存在则转交 reclaimer 异步回收，避免阻塞主线程。
 */
JNIEXPORT jboolean JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeOpenDevice(
        JNIEnv* env, jclass /*cls*/, jstring jDevPath,
        jint textureId, jint width, jint height) {
    std::string dev = jstr(env, jDevPath);
    if (dev.empty() || textureId <= 0 || width <= 0 || height <= 0) {
        LOGE("nativeOpenDevice: 参数非法 dev=%s id=%d %dx%d",
             dev.c_str(), textureId, width, height);
        return JNI_FALSE;
    }

    auto cap = std::make_unique<uvc::V4l2Capture>();
    if (!cap->startAsync(dev, width, height)) {
        LOGE("nativeOpenDevice: startAsync 失败 %s", dev.c_str());
        return JNI_FALSE;
    }

    std::unique_ptr<uvc::V4l2Capture> oldOne;
    {
        std::lock_guard<std::mutex> lk(g_capturesMtx);
        auto it = g_captures.find(textureId);
        if (it != g_captures.end()) {
            oldOne = std::move(it->second);
            g_captures.erase(it);
        }
        g_captures.emplace(textureId, std::move(cap));
    }
    if (oldOne) {
        oldOne->requestStop();
        enqueueReclaim(std::move(oldOne));
    }
    LOGI("nativeOpenDevice: textureId=%d dev=%s %dx%d 启动派发完成（异步）",
         textureId, dev.c_str(), width, height);
    return JNI_TRUE;
}

/**
 * 异步关闭：发送停止信号后把实例转交 reclaimer，立即返回。
 * reclaimer 后台线程负责真正的 thread.join + munmap + close。
 */
JNIEXPORT void JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeCloseDevice(
        JNIEnv* /*env*/, jclass /*cls*/, jint textureId) {
    std::unique_ptr<uvc::V4l2Capture> victim;
    {
        std::lock_guard<std::mutex> lk(g_capturesMtx);
        auto it = g_captures.find(textureId);
        if (it != g_captures.end()) {
            victim = std::move(it->second);
            g_captures.erase(it);
        }
    }
    if (victim) {
        victim->requestStop();
        enqueueReclaim(std::move(victim));
        LOGI("nativeCloseDevice: textureId=%d 已转交 reclaimer", textureId);
    }
}

JNIEXPORT jboolean JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeHasNewFrame(
        JNIEnv* /*env*/, jclass /*cls*/, jint textureId) {
    std::lock_guard<std::mutex> lk(g_capturesMtx);
    auto* cap = findLocked(textureId);
    return (cap && cap->hasNewFrame()) ? JNI_TRUE : JNI_FALSE;
}

/**
 * 首帧是否已就绪。C# 端启动协程用它判断"预热完成"。
 */
JNIEXPORT jboolean JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeHasFirstFrame(
        JNIEnv* /*env*/, jclass /*cls*/, jint textureId) {
    std::lock_guard<std::mutex> lk(g_capturesMtx);
    auto* cap = findLocked(textureId);
    return (cap && cap->hasFirstFrame()) ? JNI_TRUE : JNI_FALSE;
}

/**
 * 0=NotStarted 1=Initializing 2=Running 3=Failed 4=Stopped 99=NotFound
 */
JNIEXPORT jint JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeGetState(
        JNIEnv* /*env*/, jclass /*cls*/, jint textureId) {
    std::lock_guard<std::mutex> lk(g_capturesMtx);
    auto* cap = findLocked(textureId);
    if (!cap) return 99;
    return static_cast<jint>(cap->state());
}

JNIEXPORT void JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeUploadFrame(
        JNIEnv* /*env*/, jclass /*cls*/, jint textureId) {
    uvc::V4l2Capture* cap;
    {
        std::lock_guard<std::mutex> lk(g_capturesMtx);
        cap = findLocked(textureId);
    }
    if (!cap) return;
    cap->uploadToTexture(static_cast<GLuint>(textureId));
}

JNIEXPORT jfloat JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeGetFps(
        JNIEnv* /*env*/, jclass /*cls*/, jint textureId) {
    std::lock_guard<std::mutex> lk(g_capturesMtx);
    auto* cap = findLocked(textureId);
    return cap ? cap->fps() : 0.0f;
}

}  // extern "C"
