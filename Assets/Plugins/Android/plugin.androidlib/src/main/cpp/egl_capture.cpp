/*
 * egl_capture.cpp
 *
 * 与 Unity EGL context 共享的后台 GL 环境：
 *   1. captureUnityEglContext(int)：C 函数，由 Unity C# 通过 GL.IssuePluginEvent
 *      在渲染线程调用，捕获 Unity EGLContext/EGLDisplay 句柄。
 *   2. nativeCreateSharedEglEnv()：在 GlTextureManager 后台线程创建共享 EGLContext
 *      + PBuffer EGLSurface，并 eglMakeCurrent。
 *   3. nativeMakeCurrent()：后续每次在后台线程恢复 EGL context 时调用。
 */

#include <jni.h>
#include <EGL/egl.h>
#include <android/log.h>
#include <stdint.h>

#define LOG_TAG "UvcEglCapture"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,  LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

// Unity 渲染线程捕获到的 EGL 句柄
static volatile EGLContext gUnityEglContext = EGL_NO_CONTEXT;
static volatile EGLDisplay gUnityEglDisplay = EGL_NO_DISPLAY;

// 为 GlTextureManager 后台线程创建的共享 EGL 环境
static EGLDisplay gSharedDisplay = EGL_NO_DISPLAY;
static EGLContext gSharedContext = EGL_NO_CONTEXT;
static EGLSurface gSharedSurface = EGL_NO_SURFACE;

/*
 * 由 Unity 渲染线程通过 GL.IssuePluginEvent 回调，签名为 void(int)。
 */
extern "C" void captureUnityEglContext(int /*eventId*/) {
    EGLContext ctx = eglGetCurrentContext();
    EGLDisplay dsp = eglGetCurrentDisplay();
    if (ctx == EGL_NO_CONTEXT) {
        LOGE("captureUnityEglContext: EGL_NO_CONTEXT，确认在 Unity 渲染线程调用");
        return;
    }
    gUnityEglContext = ctx;
    gUnityEglDisplay = dsp;
    LOGI("captureUnityEglContext: 成功 ctx=%p display=%p", (void*)ctx, (void*)dsp);
}

/*
 * 返回 captureUnityEglContext 函数指针供 C# GL.IssuePluginEvent 使用。
 * C# 签名：[DllImport("uvc_unity_plugin")] static extern IntPtr GetCaptureEglContextFuncPtr();
 */
extern "C" JNIEXPORT void* JNICALL GetCaptureEglContextFuncPtr() {
    return (void*)&captureUnityEglContext;
}

extern "C" JNIEXPORT jboolean JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeCreateSharedEglEnv(
        JNIEnv* /*env*/, jclass /*clazz*/) {

    if (gUnityEglContext == EGL_NO_CONTEXT) {
        LOGE("nativeCreateSharedEglEnv: Unity EGL context 未捕获");
        return JNI_FALSE;
    }

    gSharedDisplay = gUnityEglDisplay;

    int version[2] = {0, 0};
    eglInitialize(gSharedDisplay, &version[0], &version[1]);

    const EGLint configAttribs[] = {
        EGL_RED_SIZE,        8,
        EGL_GREEN_SIZE,      8,
        EGL_BLUE_SIZE,       8,
        EGL_ALPHA_SIZE,      8,
        EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
        EGL_SURFACE_TYPE,    EGL_PBUFFER_BIT,
        EGL_NONE
    };
    EGLConfig config = nullptr;
    EGLint    numCfg = 0;
    if (!eglChooseConfig(gSharedDisplay, configAttribs, &config, 1, &numCfg) || numCfg == 0) {
        LOGE("nativeCreateSharedEglEnv: eglChooseConfig 失败 error=0x%x", eglGetError());
        return JNI_FALSE;
    }

    // 优先 GLES3（V4L2 upload 需要 GL_UNPACK_ROW_LENGTH），失败降级 GLES2
    const EGLint ctxAttribs3[] = { EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE };
    gSharedContext = eglCreateContext(gSharedDisplay, config, gUnityEglContext, ctxAttribs3);
    if (gSharedContext == EGL_NO_CONTEXT) {
        LOGI("nativeCreateSharedEglEnv: GLES3 失败，降级 GLES2");
        const EGLint ctxAttribs2[] = { EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE };
        gSharedContext = eglCreateContext(gSharedDisplay, config, gUnityEglContext, ctxAttribs2);
    }
    if (gSharedContext == EGL_NO_CONTEXT) {
        LOGE("nativeCreateSharedEglEnv: eglCreateContext 失败 error=0x%x", eglGetError());
        return JNI_FALSE;
    }

    const EGLint pbAttribs[] = { EGL_WIDTH, 1, EGL_HEIGHT, 1, EGL_NONE };
    gSharedSurface = eglCreatePbufferSurface(gSharedDisplay, config, pbAttribs);
    if (gSharedSurface == EGL_NO_SURFACE) {
        LOGE("nativeCreateSharedEglEnv: eglCreatePbufferSurface 失败 error=0x%x", eglGetError());
        return JNI_FALSE;
    }

    if (!eglMakeCurrent(gSharedDisplay, gSharedSurface, gSharedSurface, gSharedContext)) {
        LOGE("nativeCreateSharedEglEnv: eglMakeCurrent 失败 error=0x%x", eglGetError());
        return JNI_FALSE;
    }

    LOGI("nativeCreateSharedEglEnv: 成功，共享 ctx=%p，shareWith Unity ctx=%p",
         (void*)gSharedContext, (void*)gUnityEglContext);
    return JNI_TRUE;
}

extern "C" JNIEXPORT jboolean JNICALL
Java_com_serenegiant_unity_uvc_UvcUnityPlugin_nativeMakeCurrent(
        JNIEnv* /*env*/, jclass /*clazz*/) {
    if (gSharedContext == EGL_NO_CONTEXT || gSharedSurface == EGL_NO_SURFACE) {
        LOGE("nativeMakeCurrent: 共享 EGL 环境未初始化");
        return JNI_FALSE;
    }
    if (!eglMakeCurrent(gSharedDisplay, gSharedSurface, gSharedSurface, gSharedContext)) {
        LOGE("nativeMakeCurrent: 失败 error=0x%x", eglGetError());
        return JNI_FALSE;
    }
    return JNI_TRUE;
}
