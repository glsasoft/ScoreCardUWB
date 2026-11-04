// V4L2 UVC capture + MJPEG decode + GL upload.
//
// 每路 UVC 摄像头对应一个 V4l2Capture 实例。capture 线程持续从 /dev/videoN
// DQBUF 拿 MJPEG 帧，用 NDK AImageDecoder 解码为 RGBA8 存入 frameBuf_。
// 上传到 GL_TEXTURE_2D 由 GlTextureManager 的共享 EGL context 线程调用。
//
// **完全异步**生命周期：
//   startAsync()   - 主线程调用，立刻返回；后台 worker 线程做 initV4l2 + STREAMON
//   requestStop()  - 主线程调用，立刻返回；设标志 + shutdown(fd) 让 select 立即退出
//   joinAndRelease() - 后台 reclaimer 线程调用，阻塞 join worker、munmap、close
//
// 这保证 Unity 主线程在启动/关闭瞬间均不被任何 V4L2/USB 协商阻塞。

#pragma once

#include <GLES2/gl2.h>
#include <atomic>
#include <chrono>
#include <mutex>
#include <string>
#include <thread>
#include <vector>

namespace uvc {

enum class CaptureState : int {
    NotStarted   = 0,
    Initializing = 1,   // worker 线程已启动，正在跑 initV4l2 + STREAMON
    Running      = 2,   // STREAMON 完成，capture loop 正常出帧
    Failed       = 3,   // 初始化或运行期失败
    Stopped      = 4,   // requestStop 已发出，worker 即将退出（或已退出）
};

class V4l2Capture {
public:
    V4l2Capture();
    ~V4l2Capture();

    V4l2Capture(const V4l2Capture&) = delete;
    V4l2Capture& operator=(const V4l2Capture&) = delete;

    /**
     * 异步启动：仅记录参数 + 启动 worker 线程后立即返回。
     * worker 线程内部完成 V4L2 协商、mmap、STREAMON、首帧解码。
     * @return true 线程启动成功；false 仅在 thread 启动失败时返回
     */
    bool startAsync(const std::string& devPath, int width, int height);

    /**
     * 异步关闭：设 running=false，shutdown(fd) 让阻塞中的 select 立即返回；
     * 不 join worker。主线程立即返回。
     * 真正的资源释放（thread.join / munmap / close）必须随后由 reclaimer
     * 后台线程调用 joinAndRelease() 完成。
     */
    void requestStop();

    /**
     * 由 reclaimer 后台线程调用：阻塞等 worker 退出，释放 V4L2 与缓冲资源。
     * 可重入：第二次调用是 no-op。
     */
    void joinAndRelease();

    /**
     * 在已 makeCurrent 的 GL 线程上把最新一帧上传到指定 GL_TEXTURE_2D。
     * @return true 实际上传了新帧，false 无新帧
     */
    bool uploadToTexture(GLuint glTexId);

    /** 是否有新解码完成但尚未上传 GL 的帧 */
    bool hasNewFrame() const { return hasNewFrame_.load(std::memory_order_acquire); }

    /** 首帧是否已经解码完成、内容已写入 frameBuf_（不要求已上传 GL） */
    bool hasFirstFrame() const { return hasFirstFrame_.load(std::memory_order_acquire); }

    CaptureState state() const { return state_.load(std::memory_order_acquire); }

    /** capture 线程内统计的硬件帧率，每秒刷新 */
    float fps() const { return fps_.load(std::memory_order_relaxed); }

    int width()  const { return width_; }
    int height() const { return height_; }

    /**
     * 查询某 /dev/videoN 在 MJPEG 下能支持的最大分辨率与对应最高帧率。
     * 任何错误时返回 {0,0,0}。
     */
    struct Caps { int width; int height; int fps; };
    static Caps queryCaps(const std::string& devPath);

private:
    struct MmapBuf { void* ptr; size_t len; };

    void workerThread();
    bool initV4l2();
    void releaseV4l2();
    void captureLoop();
    void decodeJpeg(const void* data, size_t size);
    void tickFps();

    std::string devPath_;
    int fd_ = -1;
    int width_ = 0;
    int height_ = 0;
    std::vector<MmapBuf> buffers_;

    std::thread thread_;
    std::atomic<bool> running_{false};
    std::atomic<bool> joined_{false};
    std::atomic<CaptureState> state_{CaptureState::NotStarted};
    std::atomic<bool> hasFirstFrame_{false};

    // 单缓冲 + 互斥：capture 线程写、GL 线程读
    mutable std::mutex frameMtx_;
    std::vector<uint8_t> frameBuf_;
    int frameStride_ = 0;
    std::atomic<bool> hasNewFrame_{false};

    // FPS
    std::atomic<float> fps_{0.0f};
    std::chrono::steady_clock::time_point fpsLast_{};
    int fpsCount_ = 0;
};

}  // namespace uvc
