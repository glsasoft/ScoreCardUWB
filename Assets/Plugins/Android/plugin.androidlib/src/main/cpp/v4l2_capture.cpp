#include "v4l2_capture.h"

#include <android/imagedecoder.h>
#include <android/log.h>
#include <errno.h>
#include <fcntl.h>
#include <linux/videodev2.h>
#include <string.h>
#include <sys/ioctl.h>
#include <sys/mman.h>
#include <sys/select.h>
#include <sys/socket.h>
#include <unistd.h>

#include <algorithm>

#define LOG_TAG "UvcV4l2"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,  LOG_TAG, __VA_ARGS__)
#define LOGW(...) __android_log_print(ANDROID_LOG_WARN,  LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

namespace uvc {

namespace {

int xioctl(int fd, unsigned long request, void* arg) {
    int r;
    do { r = ioctl(fd, request, arg); }
    while (r < 0 && errno == EINTR);
    return r;
}

constexpr int kBufferCount = 4;
// select 超时时间。小一些以便 requestStop 后 capture loop 能快速注意到。
constexpr long kSelectTimeoutMs = 100;

}  // namespace

V4l2Capture::V4l2Capture() = default;

V4l2Capture::~V4l2Capture() {
    // 析构兜底：要求外部已经 joinAndRelease；若没有则在此尽力释放
    if (!joined_.load()) {
        requestStop();
        joinAndRelease();
    }
}

V4l2Capture::Caps V4l2Capture::queryCaps(const std::string& devPath) {
    Caps best{0, 0, 0};
    int fd = ::open(devPath.c_str(), O_RDWR | O_NONBLOCK);
    if (fd < 0) {
        LOGE("queryCaps: open(%s) failed: %s", devPath.c_str(), strerror(errno));
        return best;
    }

    v4l2_capability cap{};
    if (xioctl(fd, VIDIOC_QUERYCAP, &cap) < 0) {
        LOGE("queryCaps: VIDIOC_QUERYCAP failed: %s", strerror(errno));
        ::close(fd);
        return best;
    }
    bool isCapture = (cap.capabilities & V4L2_CAP_VIDEO_CAPTURE)
                  || (cap.device_caps & V4L2_CAP_VIDEO_CAPTURE);
    if (!isCapture) {
        ::close(fd);
        return best;
    }

    v4l2_frmsizeenum sz{};
    sz.pixel_format = V4L2_PIX_FMT_MJPEG;
    for (sz.index = 0; xioctl(fd, VIDIOC_ENUM_FRAMESIZES, &sz) == 0; ++sz.index) {
        if (sz.type != V4L2_FRMSIZE_TYPE_DISCRETE) continue;
        int w = static_cast<int>(sz.discrete.width);
        int h = static_cast<int>(sz.discrete.height);

        v4l2_frmivalenum fi{};
        fi.pixel_format = V4L2_PIX_FMT_MJPEG;
        fi.width = sz.discrete.width;
        fi.height = sz.discrete.height;
        int maxFps = 0;
        for (fi.index = 0; xioctl(fd, VIDIOC_ENUM_FRAMEINTERVALS, &fi) == 0; ++fi.index) {
            if (fi.type != V4L2_FRMIVAL_TYPE_DISCRETE) continue;
            uint32_t num = fi.discrete.numerator ? fi.discrete.numerator : 1;
            int fps = static_cast<int>(fi.discrete.denominator / num);
            if (fps > maxFps) maxFps = fps;
        }

        if (static_cast<int64_t>(w) * h > static_cast<int64_t>(best.width) * best.height) {
            best.width = w;
            best.height = h;
            best.fps = maxFps;
        }
    }

    ::close(fd);
    return best;
}

bool V4l2Capture::startAsync(const std::string& devPath, int width, int height) {
    if (state_.load() != CaptureState::NotStarted) {
        LOGW("startAsync: 已启动过");
        return false;
    }
    devPath_ = devPath;
    width_   = width;
    height_  = height;
    running_.store(true);
    state_.store(CaptureState::Initializing);
    try {
        thread_ = std::thread(&V4l2Capture::workerThread, this);
    } catch (const std::system_error& e) {
        LOGE("startAsync: 启动线程失败: %s", e.what());
        running_.store(false);
        state_.store(CaptureState::Failed);
        return false;
    }
    LOGI("startAsync: %s %dx%d 线程已派发，主线程返回", devPath.c_str(), width, height);
    return true;
}

void V4l2Capture::requestStop() {
    CaptureState s = state_.load();
    if (s == CaptureState::Stopped) return;

    running_.store(false);
    state_.store(CaptureState::Stopped);

    // 让阻塞中的 select / DQBUF 立即返回：对 video 设备无 shutdown，
    // 用 close-on-stop 不行（会和 worker 内的 ioctl 抢）。
    // V4L2 字符设备本身没法像 socket 一样 shutdown，但 select 内部基于 poll，
    // 我们靠下面这点：select 超时仅 kSelectTimeoutMs(100ms)，所以 worker 最多
    // 100ms 内会注意到 running_=false 并退出循环。
    LOGI("requestStop: 已发出停止信号 dev=%s (capture loop 最多 %ldms 内退出)",
         devPath_.c_str(), kSelectTimeoutMs);
}

void V4l2Capture::joinAndRelease() {
    bool already = joined_.exchange(true);
    if (already) return;

    if (thread_.joinable()) {
        thread_.join();
    }
    releaseV4l2();
    {
        std::lock_guard<std::mutex> lk(frameMtx_);
        frameBuf_.clear();
        frameBuf_.shrink_to_fit();
        frameStride_ = 0;
    }
    hasNewFrame_.store(false);
    hasFirstFrame_.store(false);
    fps_.store(0.0f);
    LOGI("joinAndRelease: dev=%s 释放完成", devPath_.c_str());
}

void V4l2Capture::workerThread() {
    // 阶段 1：初始化 V4L2
    if (!initV4l2()) {
        releaseV4l2();
        state_.store(CaptureState::Failed);
        running_.store(false);
        LOGE("workerThread: initV4l2 失败 dev=%s", devPath_.c_str());
        return;
    }
    state_.store(CaptureState::Running);
    LOGI("workerThread: %s 进入 Running，开始 capture loop", devPath_.c_str());

    // 阶段 2：capture loop
    captureLoop();

    LOGI("workerThread: dev=%s 已退出（state=%d）",
         devPath_.c_str(), static_cast<int>(state_.load()));
}

bool V4l2Capture::initV4l2() {
    fd_ = ::open(devPath_.c_str(), O_RDWR | O_NONBLOCK);
    if (fd_ < 0) {
        LOGE("initV4l2: open(%s) failed: %s", devPath_.c_str(), strerror(errno));
        return false;
    }

    v4l2_capability cap{};
    if (xioctl(fd_, VIDIOC_QUERYCAP, &cap) < 0) {
        LOGE("initV4l2: VIDIOC_QUERYCAP failed: %s", strerror(errno));
        return false;
    }

    v4l2_format fmt{};
    fmt.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
    fmt.fmt.pix.width = static_cast<uint32_t>(width_);
    fmt.fmt.pix.height = static_cast<uint32_t>(height_);
    fmt.fmt.pix.pixelformat = V4L2_PIX_FMT_MJPEG;
    fmt.fmt.pix.field = V4L2_FIELD_ANY;
    if (xioctl(fd_, VIDIOC_S_FMT, &fmt) < 0) {
        LOGE("initV4l2: VIDIOC_S_FMT failed: %s", strerror(errno));
        return false;
    }
    if (fmt.fmt.pix.pixelformat != V4L2_PIX_FMT_MJPEG) {
        LOGE("initV4l2: 驱动未协商为 MJPEG (got 0x%08x)", fmt.fmt.pix.pixelformat);
        return false;
    }
    width_ = static_cast<int>(fmt.fmt.pix.width);
    height_ = static_cast<int>(fmt.fmt.pix.height);

    v4l2_requestbuffers req{};
    req.count = kBufferCount;
    req.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
    req.memory = V4L2_MEMORY_MMAP;
    if (xioctl(fd_, VIDIOC_REQBUFS, &req) < 0) {
        LOGE("initV4l2: VIDIOC_REQBUFS failed: %s", strerror(errno));
        return false;
    }
    if (req.count < 2) {
        LOGE("initV4l2: 缓冲数量不足 %u", req.count);
        return false;
    }

    buffers_.resize(req.count);
    for (uint32_t i = 0; i < req.count; ++i) {
        v4l2_buffer buf{};
        buf.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
        buf.memory = V4L2_MEMORY_MMAP;
        buf.index = i;
        if (xioctl(fd_, VIDIOC_QUERYBUF, &buf) < 0) {
            LOGE("initV4l2: VIDIOC_QUERYBUF[%u] failed: %s", i, strerror(errno));
            return false;
        }
        buffers_[i].len = buf.length;
        buffers_[i].ptr = mmap(nullptr, buf.length, PROT_READ | PROT_WRITE,
                               MAP_SHARED, fd_, buf.m.offset);
        if (buffers_[i].ptr == MAP_FAILED) {
            LOGE("initV4l2: mmap[%u] failed: %s", i, strerror(errno));
            buffers_[i].ptr = nullptr;
            return false;
        }
        if (xioctl(fd_, VIDIOC_QBUF, &buf) < 0) {
            LOGE("initV4l2: VIDIOC_QBUF[%u] failed: %s", i, strerror(errno));
            return false;
        }
    }

    int type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
    if (xioctl(fd_, VIDIOC_STREAMON, &type) < 0) {
        LOGE("initV4l2: VIDIOC_STREAMON failed: %s", strerror(errno));
        return false;
    }

    fpsLast_ = std::chrono::steady_clock::now();
    fpsCount_ = 0;
    LOGI("initV4l2: dev=%s 协商完成 %dx%d MJPEG", devPath_.c_str(), width_, height_);
    return true;
}

void V4l2Capture::releaseV4l2() {
    if (fd_ >= 0) {
        int type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
        xioctl(fd_, VIDIOC_STREAMOFF, &type);
    }
    for (auto& b : buffers_) {
        if (b.ptr && b.ptr != MAP_FAILED) {
            munmap(b.ptr, b.len);
        }
    }
    buffers_.clear();
    if (fd_ >= 0) {
        ::close(fd_);
        fd_ = -1;
    }
}

void V4l2Capture::captureLoop() {
    while (running_.load()) {
        fd_set fds;
        FD_ZERO(&fds);
        FD_SET(fd_, &fds);
        timeval tv{};
        tv.tv_sec = 0;
        tv.tv_usec = kSelectTimeoutMs * 1000;

        int r = select(fd_ + 1, &fds, nullptr, nullptr, &tv);
        if (!running_.load()) break;
        if (r < 0) {
            if (errno == EINTR) continue;
            LOGE("captureLoop: select failed: %s", strerror(errno));
            break;
        }
        if (r == 0) {
            continue;  // 超时：复检 running_ 后继续
        }

        v4l2_buffer buf{};
        buf.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
        buf.memory = V4L2_MEMORY_MMAP;
        if (xioctl(fd_, VIDIOC_DQBUF, &buf) < 0) {
            if (errno == EAGAIN) continue;
            LOGE("captureLoop: VIDIOC_DQBUF failed: %s", strerror(errno));
            continue;
        }

        if (buf.index < buffers_.size() && buf.bytesused > 0) {
            decodeJpeg(buffers_[buf.index].ptr, buf.bytesused);
            tickFps();
        }

        if (xioctl(fd_, VIDIOC_QBUF, &buf) < 0) {
            LOGE("captureLoop: VIDIOC_QBUF failed: %s", strerror(errno));
        }
    }
    LOGI("captureLoop: dev=%s 退出", devPath_.c_str());
}

void V4l2Capture::decodeJpeg(const void* data, size_t size) {
    AImageDecoder* decoder = nullptr;
    int rc = AImageDecoder_createFromBuffer(data, size, &decoder);
    if (rc != ANDROID_IMAGE_DECODER_SUCCESS || decoder == nullptr) {
        LOGE("decodeJpeg: createFromBuffer rc=%d", rc);
        return;
    }
    AImageDecoder_setAndroidBitmapFormat(decoder, ANDROID_BITMAP_FORMAT_RGBA_8888);

    const AImageDecoderHeaderInfo* info = AImageDecoder_getHeaderInfo(decoder);
    int dw = AImageDecoderHeaderInfo_getWidth(info);
    int dh = AImageDecoderHeaderInfo_getHeight(info);
    if (dw != width_ || dh != height_) {
        LOGW("decodeJpeg: size mismatch %dx%d (期望 %dx%d)", dw, dh, width_, height_);
        AImageDecoder_delete(decoder);
        return;
    }

    size_t stride = AImageDecoder_getMinimumStride(decoder);
    size_t needed = stride * static_cast<size_t>(height_);

    std::lock_guard<std::mutex> lk(frameMtx_);
    if (frameBuf_.size() != needed) {
        frameBuf_.assign(needed, 0);
    }
    frameStride_ = static_cast<int>(stride);
    rc = AImageDecoder_decodeImage(decoder, frameBuf_.data(), stride, needed);
    AImageDecoder_delete(decoder);
    if (rc != ANDROID_IMAGE_DECODER_SUCCESS) {
        LOGE("decodeJpeg: decodeImage rc=%d", rc);
        return;
    }
    hasNewFrame_.store(true, std::memory_order_release);
    if (!hasFirstFrame_.load(std::memory_order_relaxed)) {
        hasFirstFrame_.store(true, std::memory_order_release);
        LOGI("decodeJpeg: dev=%s 首帧就绪 %dx%d", devPath_.c_str(), width_, height_);
    }
}

bool V4l2Capture::uploadToTexture(GLuint glTexId) {
    if (!hasNewFrame_.exchange(false, std::memory_order_acq_rel)) {
        return false;
    }
    std::lock_guard<std::mutex> lk(frameMtx_);
    if (frameBuf_.empty() || frameStride_ <= 0) return false;

    int rowPixels = frameStride_ / 4;
    glBindTexture(GL_TEXTURE_2D, glTexId);
    glPixelStorei(GL_UNPACK_ALIGNMENT, 4);
    static constexpr GLenum GL_UNPACK_ROW_LENGTH_ES3 = 0x0CF2;
    if (rowPixels != width_) {
        glPixelStorei(GL_UNPACK_ROW_LENGTH_ES3, rowPixels);
    }
    glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0,
                    width_, height_, GL_RGBA, GL_UNSIGNED_BYTE, frameBuf_.data());
    if (rowPixels != width_) {
        glPixelStorei(GL_UNPACK_ROW_LENGTH_ES3, 0);
    }
    glBindTexture(GL_TEXTURE_2D, 0);
    glFlush();
    return true;
}

void V4l2Capture::tickFps() {
    fpsCount_++;
    auto now = std::chrono::steady_clock::now();
    auto dtMs = std::chrono::duration_cast<std::chrono::milliseconds>(now - fpsLast_).count();
    if (dtMs >= 1000) {
        fps_.store(static_cast<float>(fpsCount_) * 1000.0f / static_cast<float>(dtMs),
                   std::memory_order_relaxed);
        fpsCount_ = 0;
        fpsLast_ = now;
    }
}

}  // namespace uvc
