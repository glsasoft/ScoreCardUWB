package com.serenegiant.unity.uvc;

/**
 * 单路 UVC 预览的轻量状态对象。
 *
 * 真正的 capture / 解码 / 上传都在 native 与 GlTextureManager 线程，
 * 本类仅保留 devPath、textureId、尺寸等元数据，方便 Java 侧调试与日志。
 */
public final class UvcPreviewManager {
    private final String mDevPath;
    private final int    mTextureId;
    private final int    mWidth;
    private final int    mHeight;
    private volatile boolean mClosed;

    public UvcPreviewManager(String devPath, int textureId, int width, int height) {
        this.mDevPath   = devPath;
        this.mTextureId = textureId;
        this.mWidth     = width;
        this.mHeight    = height;
    }

    public String getDevPath()   { return mDevPath; }
    public int    getTextureId() { return mTextureId; }
    public int    getWidth()     { return mWidth; }
    public int    getHeight()    { return mHeight; }

    public boolean isClosed() { return mClosed; }
    void markClosed() { mClosed = true; }
}
