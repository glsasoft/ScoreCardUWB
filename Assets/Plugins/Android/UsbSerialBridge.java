package com.glsa.mqttscorecard5s.unity;

import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.hardware.usb.UsbConstants;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbEndpoint;
import android.hardware.usb.UsbInterface;
import android.hardware.usb.UsbManager;
import android.os.Build;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.util.HashMap;

/**
 * 自包含的 CP210x(CP2102/CP2102N) USB 转串口桥接，供 Unity C# 通过 AndroidJavaObject 调用。
 *
 * 设计目标：仅用 Android SDK 自带的 android.hardware.usb，零第三方依赖、零网络依赖。
 * 仅覆盖本项目用到的 CP210x 家族，控制协议来源于公开的 Linux cp210x 驱动。
 *
 * 与内核 cp210x 驱动方案不同，本类走 USB Host API，不需要 /dev/ttyUSB、不需要 root、
 * 不需要内核模块，并且能正确下发波特率（这正是通用 usbserial 驱动做不到的）。
 *
 * 收发约定（避免 AndroidJavaProxy 回调数组的兼容性问题）：
 *   - 接收：后台读线程把数据累积到内部缓冲，C# 端每帧调用 {@link #pollReceived()} 取走。
 *   - 发送：C# 直接调用 {@link #write(byte[])}。
 */
public class UsbSerialBridge {

    private static final String TAG = "UsbSerialBridge";
    private static final String ACTION_USB_PERMISSION = "com.glsa.mqttscorecard5s.unity.USB_PERMISSION";

    // CP210x 厂商控制请求（host->device, vendor, interface）
    private static final int REQTYPE_HOST_TO_DEVICE = 0x41;
    private static final int CP210X_IFC_ENABLE   = 0x00;
    private static final int CP210X_SET_LINE_CTL = 0x03;
    private static final int CP210X_SET_MHS      = 0x07;
    private static final int CP210X_SET_BAUDRATE = 0x1E;
    private static final int UART_ENABLE = 0x0001;
    // 8 数据位/无校验/1 停止位：value = (dataBits<<8)|(parity<<4)|stopBits = 0x0800
    private static final int LINE_CTL_8N1 = 0x0800;
    // DTR=1,RTS=1（低字节为状态，高字节为写掩码）
    private static final int MHS_DTR_RTS_ON = 0x0303;

    private final Context context;
    private final UsbManager usbManager;

    private UsbDevice device;
    private UsbDeviceConnection connection;
    private UsbInterface usbInterface;
    private UsbEndpoint epIn;
    private UsbEndpoint epOut;

    private int baudRate = 115200;
    private volatile boolean open = false;
    private volatile String lastError = "";

    private Thread readThread;
    private volatile boolean running = false;

    private final ByteArrayOutputStream rxBuf = new ByteArrayOutputStream(2048);
    private final Object rxLock = new Object();

    private BroadcastReceiver permReceiver;

    public UsbSerialBridge(Context ctx) {
        this.context = ctx;
        this.usbManager = (UsbManager) ctx.getSystemService(Context.USB_SERVICE);
    }

    /**
     * 查找并打开匹配的设备。权限申请是异步的，本方法只负责发起；
     * 实际是否连上请用 {@link #isOpen()} 轮询。
     *
     * @param vid 厂商ID（CP2102N=0x10c4）
     * @param pid 产品ID（0 表示不限制，匹配该 vid 下任意设备）
     * @param baud 波特率
     * @return 是否找到了匹配设备并已发起打开
     */
    public boolean requestOpen(int vid, int pid, int baud) {
        this.baudRate = baud;
        lastError = "";

        device = findDevice(vid, pid);
        if (device == null) {
            lastError = "未找到 USB 设备 vid=" + Integer.toHexString(vid) + " pid=" + Integer.toHexString(pid);
            Log.w(TAG, lastError);
            return false;
        }

        if (usbManager.hasPermission(device)) {
            return connectNow();
        }

        registerPermissionReceiver();
        int flags = Build.VERSION.SDK_INT >= 31 ? 0x02000000 /* FLAG_MUTABLE */ : 0;
        flags |= 0x08000000; // FLAG_UPDATE_CURRENT
        PendingIntent pi = PendingIntent.getBroadcast(
                context, 0, new Intent(ACTION_USB_PERMISSION), flags);
        usbManager.requestPermission(device, pi);
        Log.i(TAG, "已发起 USB 权限申请: " + device.getDeviceName());
        return true;
    }

    private UsbDevice findDevice(int vid, int pid) {
        HashMap<String, UsbDevice> list = usbManager.getDeviceList();
        if (list == null) return null;
        for (UsbDevice d : list.values()) {
            if (d.getVendorId() == vid && (pid == 0 || d.getProductId() == pid)) {
                return d;
            }
        }
        return null;
    }

    private void registerPermissionReceiver() {
        if (permReceiver != null) return;
        permReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context c, Intent intent) {
                if (!ACTION_USB_PERMISSION.equals(intent.getAction())) return;
                boolean granted = intent.getBooleanExtra(UsbManager.EXTRA_PERMISSION_GRANTED, false);
                Log.i(TAG, "USB 权限回调: granted=" + granted);
                if (granted) {
                    connectNow();
                } else {
                    lastError = "USB 权限被拒绝";
                }
            }
        };
        IntentFilter filter = new IntentFilter(ACTION_USB_PERMISSION);
        if (Build.VERSION.SDK_INT >= 33) {
            // Context.RECEIVER_NOT_EXPORTED = 0x4（API33 起注册非系统广播必须显式声明导出标志）
            context.registerReceiver(permReceiver, filter, 0x4);
        } else {
            context.registerReceiver(permReceiver, filter);
        }
    }

    private synchronized boolean connectNow() {
        try {
            connection = usbManager.openDevice(device);
            if (connection == null) {
                lastError = "openDevice 返回 null（无权限或设备已断开）";
                Log.e(TAG, lastError);
                return false;
            }

            usbInterface = device.getInterface(0);
            if (!connection.claimInterface(usbInterface, true)) {
                lastError = "claimInterface 失败";
                Log.e(TAG, lastError);
                cleanupConnection();
                return false;
            }

            epIn = null;
            epOut = null;
            for (int i = 0; i < usbInterface.getEndpointCount(); i++) {
                UsbEndpoint ep = usbInterface.getEndpoint(i);
                if (ep.getType() != UsbConstants.USB_ENDPOINT_XFER_BULK) continue;
                if (ep.getDirection() == UsbConstants.USB_DIR_IN) epIn = ep;
                else epOut = ep;
            }
            if (epIn == null || epOut == null) {
                lastError = "未找到 bulk in/out 端点";
                Log.e(TAG, lastError);
                cleanupConnection();
                return false;
            }

            if (!configureCp210x(baudRate)) {
                cleanupConnection();
                return false;
            }

            running = true;
            readThread = new Thread(this::readLoop, "UsbSerialReadThread");
            readThread.setDaemon(true);
            readThread.start();

            open = true;
            Log.i(TAG, "CP210x 已打开 @ " + baudRate + " baud, dev=" + device.getDeviceName());
            return true;
        } catch (Exception e) {
            lastError = "connectNow 异常: " + e.getMessage();
            Log.e(TAG, lastError, e);
            cleanupConnection();
            return false;
        }
    }

    /** 下发 CP210x 厂商命令：使能 UART → 设波特率 → 设 8N1 → 拉起 DTR/RTS。 */
    private boolean configureCp210x(int baud) {
        int interfaceNum = usbInterface.getId();

        if (ctrl(CP210X_IFC_ENABLE, UART_ENABLE, interfaceNum, null) < 0) {
            lastError = "IFC_ENABLE 失败";
            return false;
        }

        byte[] baudBytes = new byte[]{
                (byte) (baud & 0xFF),
                (byte) ((baud >> 8) & 0xFF),
                (byte) ((baud >> 16) & 0xFF),
                (byte) ((baud >> 24) & 0xFF)
        };
        if (ctrl(CP210X_SET_BAUDRATE, 0, interfaceNum, baudBytes) < 0) {
            lastError = "SET_BAUDRATE 失败";
            return false;
        }

        if (ctrl(CP210X_SET_LINE_CTL, LINE_CTL_8N1, interfaceNum, null) < 0) {
            lastError = "SET_LINE_CTL 失败";
            return false;
        }

        // DTR/RTS 失败不致命，部分模块不接这两根线
        ctrl(CP210X_SET_MHS, MHS_DTR_RTS_ON, interfaceNum, null);
        return true;
    }

    private int ctrl(int request, int value, int index, byte[] data) {
        int len = data == null ? 0 : data.length;
        int r = connection.controlTransfer(
                REQTYPE_HOST_TO_DEVICE, request, value, index, data, len, 2000);
        if (r < 0) Log.e(TAG, "controlTransfer 失败 req=0x" + Integer.toHexString(request) + " ret=" + r);
        return r;
    }

    private void readLoop() {
        int maxLen = epIn.getMaxPacketSize();
        if (maxLen <= 0) maxLen = 64;
        byte[] buf = new byte[Math.max(maxLen, 256)];
        while (running) {
            int n = connection.bulkTransfer(epIn, buf, buf.length, 200);
            if (n > 0) {
                synchronized (rxLock) {
                    rxBuf.write(buf, 0, n);
                }
            }
            // n<=0 多为超时（200ms），继续轮询；running 置 false 后退出
        }
    }

    /** 发送数据。返回实际写入字节数，未打开或失败返回 -1。线程安全。 */
    public synchronized int write(byte[] data) {
        if (!open || connection == null || epOut == null || data == null || data.length == 0) {
            return -1;
        }
        int sent = 0;
        // 大包按端点大小分片，保证写完整
        int chunk = epOut.getMaxPacketSize();
        if (chunk <= 0) chunk = 64;
        while (sent < data.length) {
            int toSend = Math.min(chunk, data.length - sent);
            byte[] part = new byte[toSend];
            System.arraycopy(data, sent, part, 0, toSend);
            int n = connection.bulkTransfer(epOut, part, toSend, 2000);
            if (n < 0) {
                return sent > 0 ? sent : -1;
            }
            sent += n;
        }
        return sent;
    }

    /** 取走并清空已接收的字节；无数据返回长度为 0 的数组。 */
    public byte[] pollReceived() {
        synchronized (rxLock) {
            if (rxBuf.size() == 0) return new byte[0];
            byte[] out = rxBuf.toByteArray();
            rxBuf.reset();
            return out;
        }
    }

    public boolean isOpen() {
        return open;
    }

    public String lastError() {
        return lastError == null ? "" : lastError;
    }

    public synchronized void close() {
        running = false;
        open = false;
        if (readThread != null) {
            try {
                readThread.join(500);
            } catch (InterruptedException ignored) {
            }
            readThread = null;
        }
        cleanupConnection();
        if (permReceiver != null) {
            try {
                context.unregisterReceiver(permReceiver);
            } catch (Exception ignored) {
            }
            permReceiver = null;
        }
        Log.i(TAG, "CP210x 已关闭");
    }

    private void cleanupConnection() {
        try {
            if (connection != null && usbInterface != null) {
                connection.releaseInterface(usbInterface);
            }
        } catch (Exception ignored) {
        }
        try {
            if (connection != null) connection.close();
        } catch (Exception ignored) {
        }
        connection = null;
        epIn = null;
        epOut = null;
        usbInterface = null;
    }
}
