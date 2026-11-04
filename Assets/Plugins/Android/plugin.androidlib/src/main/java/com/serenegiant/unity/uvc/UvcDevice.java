package com.serenegiant.unity.uvc;

/**
 * UVC 设备元数据。
 *
 * 由 {@link UvcHelper#enumerateDevices()} 通过 /sys/class/video4linux 扫描
 * 并结合 native ioctl 查询得到。
 */
public final class UvcDevice {
    /** Unity 侧使用的稳定 ID，目前等同 devPath 末尾的 "videoN" */
    public final String id;
    /** /dev/videoN 完整路径 */
    public final String devPath;
    /** USB vendor id（4 位十六进制，如 "2bdf"），未知为空串 */
    public final String vendor;
    /** USB product id（4 位十六进制，如 "028a"），未知为空串 */
    public final String product;
    /** USB product 描述（如 "5MP USB Camera"），未知为空串 */
    public final String productName;
    /** 该设备 MJPEG 支持的最高分辨率宽度 */
    public final int bestWidth;
    /** 该设备 MJPEG 支持的最高分辨率高度 */
    public final int bestHeight;
    /** 在最佳分辨率下能达到的最高帧率（fps） */
    public final int bestFps;

    public UvcDevice(String id, String devPath,
                     String vendor, String product, String productName,
                     int bestWidth, int bestHeight, int bestFps) {
        this.id = id;
        this.devPath = devPath;
        this.vendor = vendor == null ? "" : vendor;
        this.product = product == null ? "" : product;
        this.productName = productName == null ? "" : productName;
        this.bestWidth = bestWidth;
        this.bestHeight = bestHeight;
        this.bestFps = bestFps;
    }

    @Override
    public String toString() {
        return "UvcDevice{id=" + id
            + " dev=" + devPath
            + " vid=" + vendor + ":" + product
            + " name='" + productName + "'"
            + " best=" + bestWidth + "x" + bestHeight + "@" + bestFps + "}";
    }

    /** 转 JSON 单对象（不含外层数组） */
    public String toJson() {
        return "{\"id\":\"" + escape(id) + "\""
            + ",\"devPath\":\"" + escape(devPath) + "\""
            + ",\"vendor\":\"" + escape(vendor) + "\""
            + ",\"product\":\"" + escape(product) + "\""
            + ",\"name\":\"" + escape(productName) + "\""
            + ",\"width\":" + bestWidth
            + ",\"height\":" + bestHeight
            + ",\"fps\":" + bestFps
            + "}";
    }

    private static String escape(String s) {
        if (s == null) return "";
        return s.replace("\\", "\\\\").replace("\"", "\\\"");
    }
}
