package com.serenegiant.unity.uvc;

import android.util.Log;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

/**
 * 通过扫描 /sys/class/video4linux 并结合 native V4L2 ioctl 枚举所有 USB UVC 摄像头。
 *
 * 仅返回：
 *   1. 物理链路在 USB 总线上（/sys 真实路径包含 "/usb"）
 *   2. 支持 MJPEG 格式的 V4L2 capture 设备
 *
 * 同一 UVC 设备常对应多个 /dev/videoN（如一路 capture + 一路 metadata），
 * 这里按 USB 物理端口路径去重，每路设备只保留索引最小的 capture 节点。
 *
 * <p>必须在 system 签名（或具备读 /dev/video* 权限）的 App 中调用，
 * 否则 native 端 open 会失败。
 */
public final class UvcHelper {
    private static final String TAG = "UvcHelper";

    private static final String V4L_SYS_DIR = "/sys/class/video4linux";

    /**
     * 枚举当前可用的 USB UVC 摄像头。线程安全。
     */
    public static List<UvcDevice> enumerateDevices() {
        List<UvcDevice> result = new ArrayList<>();

        File v4l = new File(V4L_SYS_DIR);
        File[] entries = v4l.listFiles();
        if (entries == null || entries.length == 0) {
            Log.w(TAG, "enumerateDevices: /sys/class/video4linux 为空");
            return result;
        }

        Arrays.sort(entries, Comparator.comparing(File::getName));
        Set<String> seenUsbPorts = new HashSet<>();

        for (File entry : entries) {
            String name = entry.getName();
            if (!name.startsWith("video")) continue;

            String realPath;
            try {
                realPath = entry.getCanonicalPath();
            } catch (IOException e) {
                continue;
            }

            if (!realPath.contains("/usb")) {
                continue;
            }

            String usbPort = extractUsbPortPath(realPath);
            if (usbPort == null || !seenUsbPorts.add(usbPort)) {
                continue;
            }

            String devPath = "/dev/" + name;
            String capsJson = UvcUnityPlugin.nativeQueryCapabilities(devPath);
            int[] caps = parseCaps(capsJson);
            if (caps == null || caps[0] <= 0 || caps[1] <= 0) {
                Log.d(TAG, "skip " + devPath + " (no MJPEG capture): " + capsJson);
                continue;
            }

            String vendor = readSysAttr(entry, "../idVendor");
            String product = readSysAttr(entry, "../idProduct");
            String productName = readSysAttr(entry, "../product");

            UvcDevice dev = new UvcDevice(
                name, devPath,
                vendor, product, productName,
                caps[0], caps[1], caps[2]);

            Log.i(TAG, "发现 UVC: " + dev);
            result.add(dev);
        }

        return result;
    }

    /**
     * 从 v4l2 设备的真实 sysfs 路径中提取 USB 端口路径，用于去重。
     *
     * 例: /sys/devices/platform/fc800000.usb/usb1/1-1/1-1.4/1-4:1.0/video4linux/video32
     *   → /sys/devices/platform/fc800000.usb/usb1/1-1/1-1.4
     *
     * 取倒数第三层（去掉 .../1-4:1.0/video4linux/videoN）。
     */
    private static String extractUsbPortPath(String videoSysPath) {
        File p = new File(videoSysPath);
        for (int i = 0; i < 3 && p != null; i++) {
            p = p.getParentFile();
        }
        return p == null ? null : p.getAbsolutePath();
    }

    private static String readSysAttr(File videoNode, String relative) {
        File f = new File(new File(videoNode, "device"), relative);
        if (!f.exists()) return "";
        try (BufferedReader r = new BufferedReader(new FileReader(f))) {
            String line = r.readLine();
            return line == null ? "" : line.trim();
        } catch (IOException e) {
            return "";
        }
    }

    /** 解析 native 返回的 {"width":w,"height":h,"fps":f}。失败返回 null */
    private static int[] parseCaps(String json) {
        if (json == null || json.isEmpty()) return null;
        try {
            int w = readJsonInt(json, "width");
            int h = readJsonInt(json, "height");
            int f = readJsonInt(json, "fps");
            return new int[]{ w, h, f };
        } catch (Exception e) {
            return null;
        }
    }

    private static int readJsonInt(String json, String key) {
        String needle = "\"" + key + "\":";
        int p = json.indexOf(needle);
        if (p < 0) return 0;
        int start = p + needle.length();
        int end = start;
        while (end < json.length()) {
            char c = json.charAt(end);
            if ((c >= '0' && c <= '9') || c == '-') end++;
            else break;
        }
        if (end == start) return 0;
        return Integer.parseInt(json.substring(start, end));
    }

    /** 转 JSON 数组，供 Unity 直接读取 */
    public static String toJsonArray(List<UvcDevice> list) {
        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < list.size(); i++) {
            if (i > 0) sb.append(",");
            sb.append(list.get(i).toJson());
        }
        sb.append("]");
        return sb.toString();
    }

    private UvcHelper() {}
}
