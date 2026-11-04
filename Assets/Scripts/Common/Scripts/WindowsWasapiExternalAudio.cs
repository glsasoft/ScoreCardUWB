#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Windows 下通过 WASAPI 将 PCM 混音输出到指定渲染设备（与 Unity 默认音频完全独立）。
///
/// 使用纯 vtable 委托调用 COM 方法，规避 Unity/Mono 对 COM RCW 编组的限制：
///   - Mono 的 Marshal.GetObjectForIUnknown 返回裸 __ComObject，无法隐式 QI；
///   - 所有 COM 对象保持为 IntPtr，通过 GetDelegateForFunctionPointer 直接调用。
/// </summary>
internal static class WindowsWasapiExternalAudio
{
    // ─────────────────────── 常量 ───────────────────────────────────────
    private const int    S_OK               = 0;
    private const uint   CLSCTX_ALL         = 0x17;
    private const int    DEVICE_STATE_ACTIVE = 0x1;
    private const int    EDataFlowRender    = 0;   // eRender
    private const int    eMultimedia        = 0;   // eMultimedia role

    // vtable slot offsets（IUnknown 占 0-2；各接口自 3 起）
    private const int SLOT_RELEASE = 2;

    // IMMDeviceEnumerator
    private const int SLOT_ENUM_EnumAudioEndpoints      = 3;
    private const int SLOT_ENUM_GetDefaultAudioEndpoint = 4;
    private const int SLOT_ENUM_GetDevice               = 5;

    // IMMDeviceCollection
    private const int SLOT_COLL_GetCount = 3;
    private const int SLOT_COLL_Item     = 4;

    // IMMDevice
    private const int SLOT_DEV_Activate           = 3;
    private const int SLOT_DEV_OpenPropertyStore  = 4;
    private const int SLOT_DEV_GetId              = 5;

    // IPropertyStore
    private const int SLOT_PS_GetValue = 5;

    // IAudioClient
    private const int SLOT_AC_Initialize       = 3;
    private const int SLOT_AC_GetBufferSize    = 4;
    private const int SLOT_AC_GetStreamLatency = 5;
    private const int SLOT_AC_GetCurrentPadding= 6;
    private const int SLOT_AC_GetMixFormat     = 8;
    private const int SLOT_AC_Start            = 10;
    private const int SLOT_AC_Stop             = 11;
    private const int SLOT_AC_Reset            = 12;
    private const int SLOT_AC_GetService       = 14;

    // IAudioRenderClient
    private const int SLOT_ARC_GetBuffer     = 3;
    private const int SLOT_ARC_ReleaseBuffer = 4;

    // ─────────────────────── GUIDs ──────────────────────────────────────
    private static Guid s_ClsidMmDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
    private static Guid s_IidMmDeviceEnumerator   = new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6");
    private static Guid s_IidIAudioClient         = new Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2");
    private static Guid s_IidIAudioRenderClient   = new Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");

    // PKEY_Device_FriendlyName = {A45C254E-DC1D-4380-AFB4-99BD4C248E31}, pid=14
    // PKEY_Device_FriendlyName: {A45C254E-DF1C-4EFD-8020-67D146A850E0}, pid=14
    private static Guid  s_PKEYFmtidFriendlyName  = new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0");
    private const  uint  PKEY_FriendlyName_PID    = 14;

    // ─────────────────────── 状态 ───────────────────────────────────────
    private static readonly object DeviceGate = new object();
    private static readonly object VoiceGate  = new object();

    private static string  _activeDeviceId;
    private static IntPtr  _pAudioClient  = IntPtr.Zero;
    private static IntPtr  _pRenderClient = IntPtr.Zero;
    private static IntPtr  _mixFormatPtr  = IntPtr.Zero;
    private static WaveFormatInfo _outFormat;
    private static uint    _bufferFrameCount;
    private static volatile bool _engineRunning;
    private static Thread  _engineThread;

    // 热路径委托缓存（避免每帧重建）
    private static D_AC_GetCurrentPadding _fnGetCurrentPadding;
    private static D_ARC_GetBuffer        _fnGetBuffer;
    private static D_ARC_ReleaseBuffer    _fnReleaseBuffer;
    private static D_AC_Stop              _fnStop;
    private static D_AC_Reset             _fnReset;

    private static readonly Dictionary<string, VoiceEntry> Voices = new Dictionary<string, VoiceEntry>();
    private static float _masterVolume = 1f;

    // ─────────────────────── 内部数据类型 ───────────────────────────────
    private sealed class VoiceEntry
    {
        public byte[]  PcmS16;
        public int     SampleRate;
        public int     Channels;
        public bool    Loop;
        public bool    Paused;
        public float   Volume = 1f;
        public float[] MixStereoFloat48;
        public int     MixPosition;
    }

    public struct DeviceEntry
    {
        public string DeviceId;
        public string FriendlyName;
    }

    // ─────────────────────── COM vtable 调度核心 ────────────────────────

    /// <summary>从 COM 对象的 vtable 读取第 slot 个函数并包装为委托。</summary>
    private static T Vt<T>(IntPtr comObj, int slot) where T : Delegate
    {
        IntPtr vtable  = Marshal.ReadIntPtr(comObj);
        IntPtr funcPtr = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
    }

    private static void ComRelease(IntPtr p)
    {
        if (p == IntPtr.Zero) return;
        Vt<D_Release>(p, SLOT_RELEASE)(p);
    }

    /// <summary>
    /// 将 Guid 写入 HGlobal 缓冲区，以裸指针形式传给 COM 方法（REFIID / REFCLSID）。
    /// 调用方负责 Marshal.FreeHGlobal。
    /// </summary>
    private static IntPtr GuidToIntPtr(Guid g)
    {
        byte[] b = g.ToByteArray();
        IntPtr p = Marshal.AllocHGlobal(16);
        Marshal.Copy(b, 0, p, 16);
        return p;
    }

    // ─────────────────────── COM 委托定义 ───────────────────────────────
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_Release(IntPtr self);

    // IMMDeviceEnumerator
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_EnumAudioEndpoints(IntPtr self, int dataFlow, int stateMask, out IntPtr ppDevices);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_GetDefaultAudioEndpoint(IntPtr self, int dataFlow, int role, out IntPtr ppDevice);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_GetDevice(IntPtr self, [MarshalAs(UnmanagedType.LPWStr)] string id, out IntPtr ppDevice);

    // IMMDeviceCollection
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_GetCount(IntPtr self, out uint count);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_CollItem(IntPtr self, uint index, out IntPtr ppDevice);

    // IMMDevice：iid 用 IntPtr 避免 Mono 对 ref Guid 编组问题
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_Activate(IntPtr self, IntPtr iid, uint dwClsCtx,
        IntPtr pActivationParams, out IntPtr ppInterface);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_OpenPropertyStore(IntPtr self, uint stgmAccess, out IntPtr ppStore);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_GetId(IntPtr self, out IntPtr ppstrId);  // LPWSTR via CoTaskMem

    // IPropertyStore：PROPVARIANT 用裸 IntPtr，避免 Mono 对 out-struct 编组的不可靠行为
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_PS_GetValue(IntPtr self, IntPtr pKey, IntPtr pv);

    // IAudioClient：sessionGuid 用 IntPtr 避免 Mono 对 ref Guid 编组问题（传 IntPtr.Zero 表示使用默认会话）
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_Initialize(IntPtr self, int shareMode, uint streamFlags,
        long hnsBufferDuration, long hnsPeriodicity, IntPtr pFormat, IntPtr sessionGuid);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_GetBufferSize(IntPtr self, out uint bufferSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_GetCurrentPadding(IntPtr self, out int numPaddingFrames);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_GetMixFormat(IntPtr self, out IntPtr ppDeviceFormat);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_Start(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_Stop(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_Reset(IntPtr self);

    // riid 用 IntPtr 避免 Mono 对 ref Guid 编组的问题
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_AC_GetService(IntPtr self, IntPtr riid, out IntPtr ppv);

    // IAudioRenderClient
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_ARC_GetBuffer(IntPtr self, int numFramesRequested, out IntPtr ppData);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int D_ARC_ReleaseBuffer(IntPtr self, int numFramesWritten, int dwFlags);

    // ─────────────────────── 辅助结构 ───────────────────────────────────

    // PROPERTYKEY { GUID fmtid; DWORD pid; }
    [StructLayout(LayoutKind.Sequential)]
    private struct PropKey
    {
        public Guid fmtid;
        public uint pid;
    }

    // PROPVARIANT 不再作为托管结构体传递；改用手动内存读写
    // 64-bit: sizeof=24，32-bit: sizeof=16
    private const int PROPVARIANT_SIZE = 24;
    private const ushort VT_LPWSTR = 31;
    private const ushort VT_BSTR   = 8;

    // ─────────────────────── P/Invoke ───────────────────────────────────
    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern int CoCreateInstance(
        ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext,
        ref Guid riid, out IntPtr ppv);

    [DllImport("ole32.dll")]
    private static extern void CoTaskMemFree(IntPtr ptr);

    // 传入裸指针，避免 Mono 对 ref struct 编组的问题
    [DllImport("ole32.dll", EntryPoint = "PropVariantClear")]
    private static extern int PropVariantClearPtr(IntPtr pvar);

    // ─────────────────────── 枚举设备 ───────────────────────────────────

    private static IntPtr CreateEnumeratorRaw()
    {
        Guid clsid = s_ClsidMmDeviceEnumerator;
        Guid iid   = s_IidMmDeviceEnumerator;
        int  hr    = CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX_ALL, ref iid, out IntPtr p);
        if (hr != S_OK || p == IntPtr.Zero)
            throw new InvalidOperationException($"CoCreateInstance(MMDeviceEnumerator) hr=0x{hr:X8}");
        return p;
    }

    private static string GetDeviceIdString(IntPtr pDevice)
    {
        int hr = Vt<D_GetId>(pDevice, SLOT_DEV_GetId)(pDevice, out IntPtr ppId);
        if (hr != S_OK || ppId == IntPtr.Zero) return null;
        string s = Marshal.PtrToStringUni(ppId);
        CoTaskMemFree(ppId);
        return s;
    }

    /// <summary>
    /// 读取 PKEY_Device_FriendlyName（例如 "扬声器 (USB Audio Device)"）。
    /// 使用裸 IntPtr 传递 PROPERTYKEY 和 PROPVARIANT，避免 Mono 对 ref/out struct 编组不可靠。
    /// </summary>
    private static string GetDeviceFriendlyName(IntPtr pDevice)
    {
        IntPtr pStore  = IntPtr.Zero;
        IntPtr pKeyBuf = IntPtr.Zero;
        IntPtr pvBuf   = IntPtr.Zero;
        try
        {
            // ── 诊断：打印设备指针 ──
            Debug.Log($"[Wasapi][Diag] GetDeviceFriendlyName pDevice={pDevice}");

            const uint STGM_READ = 0;
            int hr = Vt<D_OpenPropertyStore>(pDevice, SLOT_DEV_OpenPropertyStore)
                         (pDevice, STGM_READ, out pStore);
            Debug.Log($"[Wasapi][Diag] OpenPropertyStore hr=0x{hr:X8} pStore={pStore}");
            if (hr != S_OK || pStore == IntPtr.Zero) return null;

            // ── 手动布局 PROPERTYKEY（GUID 16 字节 + uint 4 字节 = 20 字节）──
            pKeyBuf = Marshal.AllocHGlobal(20);
            byte[] guidBytes = s_PKEYFmtidFriendlyName.ToByteArray();
            Marshal.Copy(guidBytes, 0, pKeyBuf, 16);
            Marshal.WriteInt32(pKeyBuf, 16, (int)PKEY_FriendlyName_PID);

            // 打印 PROPERTYKEY 原始字节，验证 GUID/pid 是否正确写入
            var keyDump = new byte[20];
            Marshal.Copy(pKeyBuf, keyDump, 0, 20);
            Debug.Log($"[Wasapi][Diag] PropKey GUID={s_PKEYFmtidFriendlyName} pid={PKEY_FriendlyName_PID} " +
                      $"rawBytes={BitConverter.ToString(keyDump)}");

            // ── 手动布局 PROPVARIANT（24 字节，零初始化 = VT_EMPTY）──
            pvBuf = Marshal.AllocHGlobal(PROPVARIANT_SIZE);
            for (int i = 0; i < PROPVARIANT_SIZE; i++) Marshal.WriteByte(pvBuf, i, 0);

            hr = Vt<D_PS_GetValue>(pStore, SLOT_PS_GetValue)(pStore, pKeyBuf, pvBuf);

            // 打印 PROPVARIANT 全部原始字节，以及 hr / vt / pStr
            var pvDump = new byte[PROPVARIANT_SIZE];
            Marshal.Copy(pvBuf, pvDump, 0, PROPVARIANT_SIZE);
            ushort vt   = (ushort)Marshal.ReadInt16(pvBuf, 0);
            IntPtr pStr = Marshal.ReadIntPtr(pvBuf, 8);
            Debug.Log($"[Wasapi][Diag] GetValue hr=0x{hr:X8} vt={vt} pStr={pStr} " +
                      $"pvBytes={BitConverter.ToString(pvDump)}");

            if (hr != S_OK) return null;

            string name = null;
            if (vt == VT_LPWSTR && pStr != IntPtr.Zero)
                name = Marshal.PtrToStringUni(pStr);
            else if (vt == VT_BSTR && pStr != IntPtr.Zero)
                name = Marshal.PtrToStringBSTR(pStr);
            else
                Debug.LogWarning($"[Wasapi] FriendlyName vt={vt} pStr={pStr} (expected VT_LPWSTR=31 or VT_BSTR=8)");

            Debug.Log($"[Wasapi][Diag] FriendlyName result=\"{name}\"");
            PropVariantClearPtr(pvBuf);
            return name;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Wasapi] GetDeviceFriendlyName exception: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
        finally
        {
            if (pKeyBuf != IntPtr.Zero) Marshal.FreeHGlobal(pKeyBuf);
            if (pvBuf   != IntPtr.Zero) Marshal.FreeHGlobal(pvBuf);
            ComRelease(pStore);
        }
    }

    public static void EnumerateRenderDevices(List<DeviceEntry> sink)
    {
        sink.Clear();
        IntPtr pEnum = IntPtr.Zero;
        IntPtr pColl = IntPtr.Zero;
        try
        {
            pEnum = CreateEnumeratorRaw();
            int hr = Vt<D_EnumAudioEndpoints>(pEnum, SLOT_ENUM_EnumAudioEndpoints)
                         (pEnum, EDataFlowRender, DEVICE_STATE_ACTIVE, out pColl);
            if (hr != S_OK || pColl == IntPtr.Zero)
            {
                Debug.LogWarning($"[Wasapi] EnumAudioEndpoints hr=0x{hr:X8}");
                return;
            }

            Vt<D_GetCount>(pColl, SLOT_COLL_GetCount)(pColl, out uint n);
            Debug.Log($"[Wasapi] 枚举到 {n} 个渲染端点");

            for (uint i = 0; i < n; i++)
            {
                IntPtr pDev = IntPtr.Zero;
                try
                {
                    hr = Vt<D_CollItem>(pColl, SLOT_COLL_Item)(pColl, i, out pDev);
                    if (hr != S_OK || pDev == IntPtr.Zero) continue;
                    string id = GetDeviceIdString(pDev);
                    if (!string.IsNullOrEmpty(id))
                    {
                        string friendly = GetDeviceFriendlyName(pDev);
                        if (string.IsNullOrEmpty(friendly))
                            friendly = id.Length > 64 ? "..." + id.Substring(id.Length - 60) : id;
                        sink.Add(new DeviceEntry { DeviceId = id, FriendlyName = friendly });
                    }
                }
                finally { ComRelease(pDev); }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Wasapi] EnumerateRenderDevices: {ex.Message}");
        }
        finally
        {
            ComRelease(pColl);
            ComRelease(pEnum);
        }
    }

    // ─────────────────────── 初始化 ─────────────────────────────────────

    public static int Init(string selector)
    {
        string id;
        lock (DeviceGate)
        {
            StopEngineLocked();
            id = ResolveDeviceId(selector);
            if (!string.IsNullOrEmpty(id)) _activeDeviceId = id;
        }
        lock (VoiceGate) { Voices.Clear(); }
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("[Wasapi] Init: 未找到匹配设备");
            return -1;
        }
        lock (DeviceGate)
        {
            return StartEngineLocked() ? 0 : -1;
        }
    }

    public static string ActiveDeviceId => _activeDeviceId;

    private static string ResolveDeviceId(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
            return GetDefaultRenderDeviceId();

        var list = new List<DeviceEntry>();
        EnumerateRenderDevices(list);

        foreach (var e in list)
            if (string.Equals(e.DeviceId, selector, StringComparison.Ordinal))
                return e.DeviceId;

        foreach (var e in list)
            if (e.DeviceId.IndexOf(selector, StringComparison.OrdinalIgnoreCase) >= 0)
                return e.DeviceId;

        return GetDefaultRenderDeviceId();
    }

    private static string GetDefaultRenderDeviceId()
    {
        IntPtr pEnum = IntPtr.Zero;
        IntPtr pDev  = IntPtr.Zero;
        try
        {
            pEnum = CreateEnumeratorRaw();
            int hr = Vt<D_GetDefaultAudioEndpoint>(pEnum, SLOT_ENUM_GetDefaultAudioEndpoint)
                         (pEnum, EDataFlowRender, eMultimedia, out pDev);
            if (hr != S_OK || pDev == IntPtr.Zero) return null;
            return GetDeviceIdString(pDev);
        }
        catch { return null; }
        finally
        {
            ComRelease(pDev);
            ComRelease(pEnum);
        }
    }

    // ─────────────────────── 引擎生命周期 ───────────────────────────────

    private static bool StartEngineLocked()
    {
        IntPtr pEnum   = IntPtr.Zero;
        IntPtr pDevice = IntPtr.Zero;
        try
        {
            pEnum = CreateEnumeratorRaw();

            // GetDevice
            int hr = Vt<D_GetDevice>(pEnum, SLOT_ENUM_GetDevice)
                         (pEnum, _activeDeviceId, out pDevice);
            if (hr != S_OK || pDevice == IntPtr.Zero)
            {
                Debug.LogError($"[Wasapi] GetDevice 失败 hr=0x{hr:X8}");
                return false;
            }

            // Activate → IAudioClient（IID 用 IntPtr 避免 Mono ref Guid 编组问题）
            IntPtr iidAcBuf = GuidToIntPtr(s_IidIAudioClient);
            hr = Vt<D_Activate>(pDevice, SLOT_DEV_Activate)
                     (pDevice, iidAcBuf, CLSCTX_ALL, IntPtr.Zero, out _pAudioClient);
            Marshal.FreeHGlobal(iidAcBuf);
            if (hr != S_OK || _pAudioClient == IntPtr.Zero)
            {
                Debug.LogError($"[Wasapi] Activate(IAudioClient) 失败 hr=0x{hr:X8}");
                _pAudioClient = IntPtr.Zero;
                return false;
            }

            // GetMixFormat (slot 8)
            hr = Vt<D_AC_GetMixFormat>(_pAudioClient, SLOT_AC_GetMixFormat)
                     (_pAudioClient, out _mixFormatPtr);
            if (hr != S_OK || _mixFormatPtr == IntPtr.Zero)
            {
                Debug.LogError($"[Wasapi] GetMixFormat 失败 hr=0x{hr:X8}");
                TeardownClient(); return false;
            }

            _outFormat = WaveFormatInfo.Read(_mixFormatPtr);
            Debug.Log($"[Wasapi] MixFormat: {_outFormat}");

            if (!_outFormat.IsFloat32 && !_outFormat.IsPcm16)
            {
                Debug.LogError($"[Wasapi] 不支持的输出格式: {_outFormat}");
                TeardownClient(); return false;
            }

            // Initialize (slot 3): shared mode, 50ms（sessionGuid=IntPtr.Zero 表示使用默认会话）
            long hnsBuffer  = 50L * 10000L;
            hr = Vt<D_AC_Initialize>(_pAudioClient, SLOT_AC_Initialize)
                     (_pAudioClient, 0, 0, hnsBuffer, 0, _mixFormatPtr, IntPtr.Zero);
            if (hr != S_OK)
            {
                Debug.LogError($"[Wasapi] Initialize 失败 hr=0x{hr:X8}");
                TeardownClient(); return false;
            }
            CoTaskMemFree(_mixFormatPtr);
            _mixFormatPtr = IntPtr.Zero;

            // GetBufferSize (slot 4)
            hr = Vt<D_AC_GetBufferSize>(_pAudioClient, SLOT_AC_GetBufferSize)
                     (_pAudioClient, out _bufferFrameCount);
            if (hr != S_OK)
            {
                Debug.LogError($"[Wasapi] GetBufferSize 失败 hr=0x{hr:X8}");
                TeardownClient(); return false;
            }

            // GetService → IAudioRenderClient (slot 14)（IID 用 IntPtr 避免 Mono ref Guid 编组问题）
            IntPtr iidRcBuf = GuidToIntPtr(s_IidIAudioRenderClient);
            hr = Vt<D_AC_GetService>(_pAudioClient, SLOT_AC_GetService)
                     (_pAudioClient, iidRcBuf, out _pRenderClient);
            Marshal.FreeHGlobal(iidRcBuf);
            if (hr != S_OK || _pRenderClient == IntPtr.Zero)
            {
                Debug.LogError($"[Wasapi] GetService(IAudioRenderClient) 失败 hr=0x{hr:X8}");
                _pRenderClient = IntPtr.Zero;
                TeardownClient(); return false;
            }

            // 缓存热路径委托
            _fnGetCurrentPadding = Vt<D_AC_GetCurrentPadding>(_pAudioClient, SLOT_AC_GetCurrentPadding);
            _fnGetBuffer         = Vt<D_ARC_GetBuffer>(_pRenderClient,  SLOT_ARC_GetBuffer);
            _fnReleaseBuffer     = Vt<D_ARC_ReleaseBuffer>(_pRenderClient, SLOT_ARC_ReleaseBuffer);
            _fnStop              = Vt<D_AC_Stop>(_pAudioClient, SLOT_AC_Stop);
            _fnReset             = Vt<D_AC_Reset>(_pAudioClient, SLOT_AC_Reset);

            // Start (slot 10)
            hr = Vt<D_AC_Start>(_pAudioClient, SLOT_AC_Start)(_pAudioClient);
            if (hr != S_OK)
            {
                Debug.LogError($"[Wasapi] Start 失败 hr=0x{hr:X8}");
                TeardownClient(); return false;
            }

            _engineRunning = true;
            _engineThread  = new Thread(EngineLoop)
                { IsBackground = true, Name = "WasapiExternalMix" };
            _engineThread.Start();
            Debug.Log("[Wasapi] 引擎已启动");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Wasapi] StartEngine: {ex}");
            TeardownClient();
            return false;
        }
        finally
        {
            ComRelease(pDevice);
            ComRelease(pEnum);
        }
    }

    private static void StopEngineLocked()
    {
        _engineRunning = false;
        if (_engineThread != null)
        {
            if (!_engineThread.Join(2000))
                Debug.LogWarning("[Wasapi] 混音线程未在 2s 内结束");
            _engineThread = null;
        }
        TeardownClient();
    }

    private static void TeardownClient()
    {
        if (_pAudioClient != IntPtr.Zero)
        {
            try { _fnStop?.Invoke(_pAudioClient); }  catch { }
            try { _fnReset?.Invoke(_pAudioClient); } catch { }
        }
        _fnGetCurrentPadding = null;
        _fnGetBuffer         = null;
        _fnReleaseBuffer     = null;
        _fnStop              = null;
        _fnReset             = null;

        if (_pRenderClient != IntPtr.Zero) { ComRelease(_pRenderClient); _pRenderClient = IntPtr.Zero; }
        if (_pAudioClient  != IntPtr.Zero) { ComRelease(_pAudioClient);  _pAudioClient  = IntPtr.Zero; }
        if (_mixFormatPtr  != IntPtr.Zero) { CoTaskMemFree(_mixFormatPtr); _mixFormatPtr = IntPtr.Zero; }
    }

    // ─────────────────────── 混音循环 ───────────────────────────────────

    private static void EngineLoop()
    {
        int    ch      = Mathf.Max(1, _outFormat.Channels);
        var    tmpFlt  = new float[Mathf.Max(1, (int)_bufferFrameCount) * ch];
        bool   isFloat = _outFormat.IsFloat32;

        while (_engineRunning)
        {
            try
            {
                int hr = _fnGetCurrentPadding(_pAudioClient, out int padding);
                if (hr != S_OK) { Thread.Sleep(5); continue; }

                int frames = (int)_bufferFrameCount - padding;
                if (frames <= 0) { Thread.Sleep(3); continue; }
                if (frames > tmpFlt.Length / ch) frames = tmpFlt.Length / ch;

                hr = _fnGetBuffer(_pRenderClient, frames, out IntPtr pBuf);
                if (hr != S_OK) { Thread.Sleep(3); continue; }

                MixToBuffer(frames, ch, tmpFlt, isFloat, pBuf);

                _fnReleaseBuffer(_pRenderClient, frames, 0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Wasapi] EngineLoop: {ex}");
                Thread.Sleep(20);
            }
        }
    }

    private static void MixToBuffer(int frames, int ch, float[] tmp, bool isFloat, IntPtr dest)
    {
        int n = frames * ch;
        Array.Clear(tmp, 0, n);
        var done = new List<string>();

        lock (VoiceGate)
        {
            float m = Mathf.Clamp(_masterVolume, 0f, 1.5f);
            for (int f = 0; f < frames; f++)
            {
                float sumL = 0f, sumR = 0f;
                foreach (var kv in Voices)
                {
                    var v = kv.Value;
                    if (v.Paused || v.MixStereoFloat48 == null) continue;
                    int sf = v.MixStereoFloat48.Length / 2;
                    if (v.MixPosition >= sf)
                    {
                        if (v.Loop) v.MixPosition = 0;
                        else { done.Add(kv.Key); continue; }
                    }
                    int o = v.MixPosition * 2;
                    sumL += v.MixStereoFloat48[o]     * v.Volume;
                    sumR += v.MixStereoFloat48[o + 1] * v.Volume;
                    v.MixPosition++;
                    if (!v.Loop && v.MixPosition >= sf) done.Add(kv.Key);
                }
                sumL = Mathf.Clamp(sumL * m, -1f, 1f);
                sumR = Mathf.Clamp(sumR * m, -1f, 1f);
                if (ch >= 2) { tmp[f * ch] = sumL; tmp[f * ch + 1] = sumR; }
                else          tmp[f * ch] = (sumL + sumR) * 0.5f;
            }
            foreach (var k in done) Voices.Remove(k);
        }

        if (isFloat)
        {
            Marshal.Copy(tmp, 0, dest, n);
        }
        else
        {
            var s16 = new short[n];
            for (int i = 0; i < n; i++)
                s16[i] = (short)Mathf.RoundToInt(Mathf.Clamp(tmp[i], -1f, 1f) * 32767f);
            Marshal.Copy(s16, 0, dest, n);
        }
    }

    // ─────────────────────── 公共播放 API ───────────────────────────────

    public static void Play(string key, byte[] pcm, int sampleRate, int channels, bool loop)
    {
        if (pcm == null || pcm.Length == 0) return;
        lock (DeviceGate) { EnsureEngineForPlay(); }
        lock (VoiceGate)
        {
            Voices[key] = new VoiceEntry
            {
                PcmS16          = pcm,
                SampleRate      = sampleRate,
                Channels        = channels,
                Loop            = loop,
                Paused          = false,
                Volume          = 1f,
                MixStereoFloat48= BuildStereoFloat48(pcm, sampleRate, channels),
                MixPosition     = 0
            };
        }
    }

    private static void EnsureEngineForPlay()
    {
        if (_pAudioClient != IntPtr.Zero && _pRenderClient != IntPtr.Zero && _engineRunning)
            return;
        StopEngineLocked();
        if (string.IsNullOrEmpty(_activeDeviceId))
            _activeDeviceId = GetDefaultRenderDeviceId();
        if (string.IsNullOrEmpty(_activeDeviceId))
        {
            Debug.LogError("[Wasapi] 无可用默认播放设备");
            return;
        }
        lock (VoiceGate) { Voices.Clear(); }
        if (!StartEngineLocked())
            Debug.LogError("[Wasapi] StartEngine 失败");
    }

    public static void PlayFromClip(string key, AudioClip clip, bool loop,
                                    string pcmCachePath = null, bool usePcmCache = true)
    {
        PlayFromClipAsync(key, clip, loop, pcmCachePath, usePcmCache).Forget();
    }

    public static async UniTask PlayFromClipAsync(string key, AudioClip clip, bool loop,
                                                  string pcmCachePath = null, bool usePcmCache = true)
    {
        if (clip == null) return;

        string cacheKey = usePcmCache && !string.IsNullOrEmpty(pcmCachePath)
            ? AudioClipPcmConverter.MakeFileCacheKey(pcmCachePath)
            : null;

        try
        {
            var pcm = await AudioClipPcmConverter.ConvertAsync(clip, cacheKey, usePcmCache);
            await UniTask.SwitchToMainThread();
            Play(key, pcm.Data, pcm.SampleRate, pcm.Channels, loop);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Wasapi] PlayFromClip 失败 key={key}: {e.Message}");
        }
    }

    public static void Pause(string key)
    {
        lock (VoiceGate)
        {
            if (Voices.TryGetValue(key, out var v))
            { v.Paused = true; v.MixStereoFloat48 = null; }
        }
    }

    public static void Resume(string key)
    {
        lock (VoiceGate)
        {
            if (!Voices.TryGetValue(key, out var v) || !v.Paused) return;
            v.MixStereoFloat48 = BuildStereoFloat48(v.PcmS16, v.SampleRate, v.Channels);
            v.MixPosition = 0;
            v.Paused = false;
        }
    }

    public static void Stop(string key)
    {
        lock (VoiceGate) { Voices.Remove(key); }
    }

    public static void StopAll()
    {
        lock (VoiceGate) { Voices.Clear(); }
    }

    /// <summary>
    /// 停掉所有 voice 并彻底关闭引擎（混音线程 + COM 对象）。
    /// <see cref="StopAll"/> 只清 voice，引擎线程仍在跑；这个静态类不挂在任何 GameObject 上，
    /// Editor 退出 Play 模式时没人回收它，不关引擎的话线程会跨播放模式一直存活。
    /// 下次 <see cref="Play"/> 会自行重启引擎，所以关掉是安全的。
    /// </summary>
    public static void Shutdown()
    {
        lock (VoiceGate) { Voices.Clear(); }
        lock (DeviceGate) { StopEngineLocked(); }
    }

    public static bool IsPlaying(string key)
    {
        lock (VoiceGate)
        {
            if (!Voices.TryGetValue(key, out var v) || v.Paused) return false;
            return v.MixStereoFloat48 != null
                   && (v.Loop || v.MixPosition < v.MixStereoFloat48.Length / 2);
        }
    }

    public static bool IsPaused(string key)
    {
        lock (VoiceGate) { return Voices.TryGetValue(key, out var v) && v.Paused; }
    }

    public static bool IsNamedVoicePlaying(string name) => IsPlaying(name);

    public static void SetMasterVolume(float volume)
    {
        lock (VoiceGate) { _masterVolume = volume; }
    }

    public static float GetMasterVolume()
    {
        lock (VoiceGate) { return _masterVolume; }
    }

    public static bool SetVoiceVolume(string key, float volume)
    {
        lock (VoiceGate)
        {
            if (!Voices.TryGetValue(key, out var v)) return false;
            v.Volume = Mathf.Clamp01(volume);
            return true;
        }
    }

    // ─────────────────────── PCM 重采样工具 ─────────────────────────────

    private static float[] BuildStereoFloat48(byte[] pcmS16, int inRate, int inCh)
    {
        int inFrames = pcmS16.Length / (2 * inCh);
        if (inFrames <= 0) return Array.Empty<float>();

        const int outRate = 48000;
        int outFrames = Mathf.Max(1, Mathf.RoundToInt(inFrames * (outRate / (float)inRate)));
        var dst = new float[outFrames * 2];

        float ReadSample(int frame, int c)
        {
            int idx = frame * inCh + c;
            int off = idx * 2;
            if (off + 1 >= pcmS16.Length) return 0f;
            return (short)(pcmS16[off] | (pcmS16[off + 1] << 8)) / 32768f;
        }

        for (int j = 0; j < outFrames; j++)
        {
            float srcPos = j * (inRate / (float)outRate);
            int   i0     = Mathf.Min((int)srcPos, inFrames - 1);
            int   i1     = Mathf.Min(i0 + 1,      inFrames - 1);
            float frac   = srcPos - i0;

            float L0, R0, L1, R1;
            if (inCh == 1)
            {
                L0 = R0 = ReadSample(i0, 0);
                L1 = R1 = ReadSample(i1, 0);
            }
            else
            {
                L0 = ReadSample(i0, 0); R0 = ReadSample(i0, 1);
                L1 = ReadSample(i1, 0); R1 = ReadSample(i1, 1);
            }
            dst[j * 2]     = Mathf.Lerp(L0, L1, frac);
            dst[j * 2 + 1] = Mathf.Lerp(R0, R1, frac);
        }
        return dst;
    }

    // ─────────────────────── WAVEFORMATEX 解析 ──────────────────────────

    private struct WaveFormatInfo
    {
        public int  SampleRate;
        public int  Channels;
        public bool IsFloat32;
        public bool IsPcm16;

        public static WaveFormatInfo Read(IntPtr p)
        {
            ushort tag  = (ushort)Marshal.ReadInt16(p);
            ushort ch   = (ushort)Marshal.ReadInt16(p + 2);
            int    rate = Marshal.ReadInt32(p + 4);
            ushort bits = (ushort)Marshal.ReadInt16(p + 14);

            const ushort PCM        = 1;
            const ushort IEEE_FLOAT = 3;
            const ushort EXTENSIBLE = 0xFFFE;

            bool isFloat = bits == 32 && (tag == IEEE_FLOAT || tag == EXTENSIBLE);
            bool isPcm16 = bits == 16 && tag == PCM;

            return new WaveFormatInfo
            {
                SampleRate = rate,
                Channels   = Mathf.Max(1, ch),
                IsFloat32  = isFloat,
                IsPcm16    = isPcm16
            };
        }

        public override string ToString() =>
            $"{SampleRate}Hz ch={Channels} float32={IsFloat32} pcm16={IsPcm16}";
    }
}
#endif
