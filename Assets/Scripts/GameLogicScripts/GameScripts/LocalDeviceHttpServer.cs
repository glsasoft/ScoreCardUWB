using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using Cysharp.Threading.Tasks;
using ZXing;
using ZXing.Common;

public class LocalDeviceHttpServer : MonoBehaviour
{
    // 当收到并成功解析表单时，触发此事件。订阅者会收到解析后的 DeviceInfo5S 对象。
    public event Action<LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device.DeviceInfo5S> DeviceSubmitted;

    [Tooltip("监听端口，确保在防火墙/路由可访问")]
    public int Port = 9999;

    [Tooltip("默认 umeng_flag，表单必填")]
    public string DefaultUmengFlag = "store1-1";

    [Tooltip("默认 uri，表单必填")]
    public string DefaultUri = "tcp://tx0000000000000003.wogemu.com:1883";

    [Tooltip("默认 device_name，表单必填")]
    public string DefaultDeviceName = "20251127094410";

    [Tooltip("默认 product_key，表单必填")]
    public string DefaultProductKey = "/store1";

    [Tooltip("默认 device_pass，表单必填")]
    public string DefaultDevicePass = "ffed2b";

    private HttpListener _listener;
    private CancellationTokenSource _cts;

    public string ServerUrl { get; private set; }
    // 是否正在运行（避免重复 Start 导致前缀冲突）
    public bool IsRunning { get; private set; }

    private void Start()
    {
        StartServer();
    }

    private void OnDestroy()
    {
        StopServer();
    }

    public void StartServer()
    {
        // 尝试启动监听器；如果端口被占用，尝试随机端口重试若干次
        if (IsRunning)
        {
            Debug.Log("LocalDeviceHttpServer: StartServer called but server is already running.");
            return;
        }

        if (!HttpListener.IsSupported)
        {
            Debug.LogError("HttpListener is not supported on this platform.");
            return;
        }

        const int maxAttempts = 20;
        var rnd = new System.Random();
        int attempt = 0;
        Exception lastEx = null;

        for (; attempt < maxAttempts; attempt++)
        {
            var tryPort = (attempt == 0) ? Port : rnd.Next(2000, 65000);
            CancellationTokenSource tryCts = null;
            HttpListener tryListener = null;
            try
            {
                tryCts = new CancellationTokenSource();
                tryListener = new HttpListener();
                var prefix = $"http://+:{tryPort}/";
                tryListener.Prefixes.Add(prefix);
                tryListener.Start();

                // 成功
                _cts = tryCts;
                _listener = tryListener;
                Port = tryPort; // 更新为实际监听端口
                ServerUrl = GetLocalUrl(Port);
                IsRunning = true;
                Debug.Log($"LocalDeviceHttpServer started. Open URL: {ServerUrl}");
                Debug.Log($"LocalDeviceHttpServer listening on port {Port}");

                // 启动监听循环
                Task.Run(() => ListenLoop(_cts.Token));
                return;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                try
                {
                    tryListener?.Close();
                }
                catch { }
                try
                {
                    tryCts?.Cancel();
                    tryCts?.Dispose();
                }
                catch { }

                // 如果是第一次尝试（使用原始 Port）且失败，打印警告并继续尝试随机端口
                if (attempt == 0)
                {
                    Debug.LogWarning($"LocalDeviceHttpServer: failed to bind to configured port {Port}, will try random ports. Error: {ex.Message}");
                }

                // 等短暂时间再尝试
                System.Threading.Thread.Sleep(50);
                continue;
            }
        }

        Debug.LogError("Failed to start LocalDeviceHttpServer after attempts: " + lastEx);
        StopServer();
    }

    public void StopServer()
    {
        try
        {
            _cts?.Cancel();
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Error stopping server: " + ex);
        }
        finally
        {
            _listener = null;
            _cts = null;
            IsRunning = false;
        }
    }

    private async Task ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            HttpListenerContext ctx = null;
            try
            {
                ctx = await _listener.GetContextAsync();
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested) break;
                Debug.LogWarning("Exception in GetContextAsync: " + ex);
                await Task.Delay(100);
                continue;
            }

            _ = Task.Run(() => HandleContext(ctx));
        }
    }

    private void HandleContext(HttpListenerContext ctx)
    {
        try
        {
            var req = ctx.Request;
            var res = ctx.Response;
            var remoteAddr = req.RemoteEndPoint != null ? req.RemoteEndPoint.ToString() : req.UserHostAddress;
            Debug.Log($"LocalDeviceHttpServer recv from {remoteAddr}: {req.HttpMethod} {req.Url}");

            if (req.HttpMethod == "GET")
            {
                if (req.Url.AbsolutePath == "/")
                {
                    var html = GetFormHtml();
                    var buffer = Encoding.UTF8.GetBytes(html);
                    res.ContentType = "text/html; charset=utf-8";
                    res.ContentLength64 = buffer.Length;
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    res.StatusCode = 404;
                }
            }
            else if (req.HttpMethod == "POST")
            {
                if (req.Url.AbsolutePath == "/submit")
                {
                    using var ms = new System.IO.MemoryStream();
                    req.InputStream.CopyTo(ms);
                    var body = Encoding.UTF8.GetString(ms.ToArray());
                    var form = ParseFormUrlEncoded(body);

                    // 验证并处理表单，返回结果给客户端
                    if (ProcessDeviceForm(form, out var msg, out var device))
                    {
                        var respHtml = "<html><body><h3>提交成功，设备信息已保存。</h3></body></html>";
                        var buffer = Encoding.UTF8.GetBytes(respHtml);
                        res.ContentType = "text/html; charset=utf-8";
                        res.ContentLength64 = buffer.Length;
                        res.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        var respHtml = $"<html><body><h3>提交失败：{WebUtility.HtmlEncode(msg)}</h3></body></html>";
                        var buffer = Encoding.UTF8.GetBytes(respHtml);
                        res.ContentType = "text/html; charset=utf-8";
                        res.ContentLength64 = buffer.Length;
                        res.StatusCode = 400;
                        res.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    res.StatusCode = 404;
                }
            }

            res.OutputStream.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error handling request: " + ex);
        }
    }

    /// <summary>
    /// 处理表单并返回是否成功、消息与解析得到的 device 对象。
    /// 所有字段均为必填，如果缺失则返回失败并携带提示信息。
    /// </summary>
    private bool ProcessDeviceForm(Dictionary<string, string> form, out string message, out DeviceInfo5S device)
    {
        device = null;
        message = "";
        try
        {
            var umeng = GetFormValue(form, "umeng_flag") ?? DefaultUmengFlag;
            var uri = GetFormValue(form, "uri") ?? DefaultUri;
            var deviceName = GetFormValue(form, "device_name") ?? DefaultDeviceName;
            var productKey = GetFormValue(form, "product_key") ?? DefaultProductKey;
            var devicePass = GetFormValue(form, "device_pass") ?? DefaultDevicePass;

            // 验证所有必填字段
            if (string.IsNullOrEmpty(umeng) || string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(deviceName)
                || string.IsNullOrEmpty(productKey) || string.IsNullOrEmpty(devicePass))
            {
                message = "缺少必填字段，请填写所有字段后重试。";
                return false;
            }

            var publish = "/" + productKey.Trim('/') + "/" + deviceName + "/user/get";
            var subscribe = "/" + productKey.Trim('/') + "/" + deviceName + "/user/update";

            var obj = new
            {
                umeng_flag = umeng,
                uri = uri,
                device_name = deviceName,
                product_key = productKey,
                device_pass = devicePass,
                publish_topic = publish,
                subscribe_topic = subscribe
            };

            var json = JsonConvert.SerializeObject(obj);
            device = JsonConvert.DeserializeObject<DeviceInfo5S>(json);

            if (device == null || !device.IsValid())
            {
                message = "解析的设备数据无效";
                return false;
            }

            // 将数据保存到 AppStateManager 并触发事件
            _ = SaveDeviceOnMainThread(device);
            try
            {
                var s = device;
                UniTask.Post(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    DeviceSubmitted?.Invoke(s);
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning("DeviceSubmitted handler threw: " + ex);
            }

            message = "成功";
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("ProcessDeviceForm error: " + ex);
            message = "处理表单时发生异常: " + ex.Message;
            return false;
        }
    }

    private async UniTaskVoid SaveDeviceOnMainThread(DeviceInfo5S device)
    {
        try
        {
            await UniTask.SwitchToMainThread();
            Debug.Log(
                "[LocalDeviceHttpServer.Save] 准备写入 AppStateManager.DeviceInfo5S，IsValid=" + device.IsValid() +
                "，字段诊断=" + device.GetInvalidDataInfo());
            try
            {
                Debug.Log("[LocalDeviceHttpServer.Save] 完整 JSON: " + JsonConvert.SerializeObject(device));
            }
            catch (Exception je)
            {
                Debug.LogError("[LocalDeviceHttpServer.Save] 序列化日志用 JSON 失败: " + je.Message);
            }

            device.PrintDeviceInfo("[LocalDeviceHttpServer.Save] 内存对象");
            AppStateManager.Instance.DeviceInfo5S = device;
            AppStateManager.Instance.LogDeviceInfo5SStorageSnapshot("[LocalDeviceHttpServer.Save] 写入后回读");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save DeviceInfo5S on main thread: " + ex);
        }
    }

    private static string GetFormValue(Dictionary<string, string> form, string key)
    {
        return form != null && form.TryGetValue(key, out var v) ? WebUtility.UrlDecode(v) : null;
    }

    private static Dictionary<string, string> ParseFormUrlEncoded(string body)
    {
        var dict = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(body)) return dict;
        var parts = body.Split('&');
        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (kv.Length >= 2)
            {
                var k = WebUtility.UrlDecode(kv[0]);
                var v = WebUtility.UrlDecode(kv[1]);
                dict[k] = v;
            }
        }
        return dict;
    }

    private string GetFormHtml()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset=\"utf-8\"><title>注册设备</title></head><body>");
        sb.AppendLine("<h3>请在下面填写设备信息并提交</h3>");
        sb.AppendLine("<form method=\"post\" action=\"/submit\">");
        var umengVal = WebUtility.HtmlEncode(DefaultUmengFlag ?? "");
        var uriVal = WebUtility.HtmlEncode(DefaultUri ?? "");
        var deviceNameVal = WebUtility.HtmlEncode(DefaultDeviceName ?? "");
        var productKeyVal = WebUtility.HtmlEncode(DefaultProductKey ?? "");
        var devicePassVal = WebUtility.HtmlEncode(DefaultDevicePass ?? "");

        sb.AppendLine($"umeng_flag: <input name=\"umeng_flag\" value=\"{umengVal}\" /><br/>");
        sb.AppendLine($"uri: <input name=\"uri\" value=\"{uriVal}\" style=\"width:400px\" /><br/>");
        sb.AppendLine($"device_name: <input name=\"device_name\" value=\"{deviceNameVal}\" /><br/>");
        sb.AppendLine($"product_key: <input name=\"product_key\" value=\"{productKeyVal}\" /><br/>");
        sb.AppendLine($"device_pass: <input name=\"device_pass\" value=\"{devicePassVal}\" /><br/>");
        sb.AppendLine("<input type=\"submit\" value=\"提交\" />");
        sb.AppendLine("</form>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static string GetLocalUrl(int port)
    {
        try
        {
            var hostName = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostName);

            IPAddress firstNonLoopback = null;
            foreach (var addr in addresses)
            {
                if (addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) continue;
                if (IPAddress.IsLoopback(addr)) continue;

                if (firstNonLoopback == null) firstNonLoopback = addr;

                var bytes = addr.GetAddressBytes();
                // 私有地址段：10.x.x.x, 172.16.x.x - 172.31.x.x, 192.168.x.x
                if (bytes[0] == 10
                    || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    || (bytes[0] == 192 && bytes[1] == 168))
                {
                    return $"http://{addr}:{port}/";
                }
            }

            if (firstNonLoopback != null)
            {
                return $"http://{firstNonLoopback}:{port}/";
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("GetLocalUrl failed: " + ex);
        }
        return $"http://127.0.0.1:{port}/";
    }

    /// <summary>
    /// 生成 ServerUrl 对应的二维码图像（Texture2D）。
    /// 如果 ServerUrl 为空，会使用当前本机局域网地址生成。
    /// </summary>
    /// <param name="size">二维码像素大小（正方形）</param>
    /// <returns>返回生成的 Texture2D，失败时返回 null</returns>
    public Texture2D GetServerQrTexture(int size = 512)
    {
        try
        {
            var text = ServerUrl;
            if (string.IsNullOrEmpty(text)) text = GetLocalUrl(Port);

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = size,
                    Width = size,
                    Margin = 1
                }
            };

            var pixels = writer.Write(text);
            if (pixels == null || pixels.Length == 0) return null;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }
        catch (Exception ex)
        {
            Debug.LogError("GetServerQrTexture failed: " + ex);
            return null;
        }
    }
}
