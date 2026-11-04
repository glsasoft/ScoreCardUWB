using Common.Network.Model.Http;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Object = System.Object;

namespace Common.Utils
{
    public static class HttpClientUtils
    {
        public static async UniTask<HttpTextResponse> PostJson(string url, Object o, Dictionary<string, string> headers)
        {
            var beginTime = DateTime.Now;
            var resp = new HttpTextResponse();
            
            // 使用 LogColor 中集中管理的颜色/Colorize（避免程序集循环依赖）
            string Colorize(string msg, string hex) => LogColor.Colorize(msg, hex);

            var jsonStr = JsonConvert.SerializeObject(o);
            var headersSnapshot = headers != null ? new Dictionary<string, string>(headers) : null;

            Debug.Log(Colorize("[Request] PostJson " + url + " > requestBody = " + jsonStr, LogColor.Request));
            Debug.Log(Colorize("[Request] PostJson " + url + " > headers = " + JsonConvert.SerializeObject(headersSnapshot), LogColor.Request));

            using var request = new UnityWebRequest(url, "POST");
            if (headersSnapshot != null)
            {
                foreach (var entry in headersSnapshot)
                {
                    if (string.IsNullOrEmpty(entry.Key) || string.IsNullOrEmpty(entry.Value))
                    {
                        continue;
                    }
                    Debug.Log(Colorize("[Request] PostJson " + url + " > 添加Header Key=" + entry.Key + " Value=" + entry.Value, LogColor.Request));
                    request.SetRequestHeader(entry.Key, entry.Value);
                }
            }
                
            var bodyRaw = Encoding.UTF8.GetBytes(jsonStr);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log(Colorize("[Request] PostJson " + url + " > Sending...", LogColor.Request));
                
            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError(Colorize("[Request] PostJson " + url + " 网络请求异常：Exception =" + e.Message, LogColor.ResponseErr));
            }
            
            resp.responseCode = request.responseCode;
            if (request.result == UnityWebRequest.Result.Success)
            {
                resp.success = true;
                resp.result = request.downloadHandler.text;
                LongLogger.Log("[Response] PostJson " + url + " success responseCode=" + resp.responseCode + " body=" + resp.result);
                Debug.Log(Colorize("[Response] PostJson " + url + " success responseCode=" + resp.responseCode + " body=" + resp.result, LogColor.ResponseOk));
            }
            else
            {
                var errMsg = "[Response] PostJson " + url + " 网络请求异常：error=" + request.error + " result=" + request.result;
                Debug.LogError(Colorize(errMsg, LogColor.ResponseErr));
            }
            
            request.Dispose();
            
            // 异步更新UI
            await UniTask.SwitchToMainThread(); // 切换到主线程更新UI
            var respTime = (DateTime.Now - beginTime).TotalMilliseconds;
            Debug.Log(Colorize("[Info] PostJson " + url + " > 网络请求响应耗时：" + respTime + "ms", LogColor.Info));
            return resp;
        }

        public static async UniTask<HttpTextResponse> Get(string url, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            var beginTime = DateTime.Now;
            var resp = new HttpTextResponse();

            string Colorize(string msg, string hex) => LogColor.Colorize(msg, hex);

            if (queryParams != null && queryParams.Count > 0)
            {
                var sb = new StringBuilder(url);
                sb.Append('?');
                bool first = true;
                foreach (var kv in queryParams)
                {
                    if (kv.Key == null || kv.Value == null) continue;
                    if (!first) sb.Append('&');
                    sb.Append(UnityWebRequest.EscapeURL(kv.Key));
                    sb.Append('=');
                    sb.Append(UnityWebRequest.EscapeURL(kv.Value));
                    first = false;
                }
                url = sb.ToString();
            }

            Debug.Log(Colorize("[Request] Get " + url, LogColor.Request));

            using var request = UnityWebRequest.Get(url);
            if (headers != null)
            {
                foreach (var entry in headers)
                {
                    if (string.IsNullOrEmpty(entry.Key) || string.IsNullOrEmpty(entry.Value)) continue;
                    request.SetRequestHeader(entry.Key, entry.Value);
                }
            }

            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError(Colorize("[Request] Get " + url + " 网络请求异常：Exception =" + e.Message, LogColor.ResponseErr));
            }

            resp.responseCode = request.responseCode;
            if (request.result == UnityWebRequest.Result.Success)
            {
                resp.success = true;
                resp.result = request.downloadHandler.text;
                LongLogger.Log("[Response] Get " + url + " success responseCode=" + resp.responseCode + " body=" + resp.result);
                Debug.Log(Colorize("[Response] Get " + url + " success responseCode=" + resp.responseCode, LogColor.ResponseOk));
            }
            else
            {
                var errMsg = "[Response] Get " + url + " 网络请求异常：error=" + request.error + " result=" + request.result;
                Debug.LogError(Colorize(errMsg, LogColor.ResponseErr));
            }

            request.Dispose();
            await UniTask.SwitchToMainThread();
            var respTime = (DateTime.Now - beginTime).TotalMilliseconds;
            Debug.Log(Colorize("[Info] Get " + url + " > 网络请求响应耗时：" + respTime + "ms", LogColor.Info));
            return resp;
        }

        public static async UniTask<HttpTextResponse> Post(string url, Dictionary<string, string> formData, Dictionary<string, string> headers)
        {
            var beginTime = DateTime.Now;
            var resp = new HttpTextResponse();

            // 使用 LogColor 中集中管理的颜色/Colorize（避免程序集循环依赖）
            string Colorize(string msg, string hex) => LogColor.Colorize(msg, hex);

            // snapshot formData & headers，避免外部复用字典被篡改后日志混淆
            var formDataSnapshot = formData != null ? new Dictionary<string, string>(formData) : null;
            var headersSnapshot = headers != null ? new Dictionary<string, string>(headers) : null;

            Debug.Log(Colorize("[Request] Post " + url + " > formData = " + JsonConvert.SerializeObject(formDataSnapshot), LogColor.Request));
            Debug.Log(Colorize("[Request] Post " + url + " > headers = " + JsonConvert.SerializeObject(headersSnapshot), LogColor.Request));
            
            var form = new WWWForm();
            if (formDataSnapshot != null)
            {
                foreach (var entry in formDataSnapshot)
                {
                    if (entry.Key != null && entry.Value != null)
                    {
                        //Debug.Log(Colorize("[Request] Post " + url + " > 添加参数 Key=" + entry.Key + " Value=" + entry.Value, LogColor.Request));
                        form.AddField(entry.Key, entry.Value);   
                    }
                }
            }

            using var request = UnityWebRequest.Post(url, form);
            if (headersSnapshot != null)
            {
                foreach (var entry in headersSnapshot)
                {
                    if (string.IsNullOrEmpty(entry.Key) || string.IsNullOrEmpty(entry.Value))
                    {
                        continue;
                    }
                    //Debug.Log(Colorize("[Request] Post " + url + " > 添加Header Key=" + entry.Key + " Value=" + entry.Value, LogColor.Request));
                    request.SetRequestHeader(entry.Key, entry.Value);
                }
            }

            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            //Debug.Log(Colorize("[Request] Post " + url + " > Sending...", LogColor.Request));
            try
            {
                await request.SendWebRequest().ToUniTask(); // 将UnityWebRequest的异步操作转换为UniTask
            }
            catch (Exception e)
            {
                Debug.LogError(Colorize("[Request] Post " + url + " 网络请求异常：Exception =" + e.Message, LogColor.ResponseErr));
            }

            resp.responseCode = request.responseCode;
            if (request.result == UnityWebRequest.Result.Success)
            {
                resp.success = true;
                resp.result = request.downloadHandler.text;
                LongLogger.Log("[Response] Post " + url + " success responseCode=" + resp.responseCode + " body=" + resp.result + " 参数:" + JsonConvert.SerializeObject(formDataSnapshot));
                Debug.Log(Colorize("[Response] Post " + url + " success responseCode=" + resp.responseCode + " body=" + resp.result + " 参数:" + JsonConvert.SerializeObject(formDataSnapshot), LogColor.ResponseOk));
            }
            else
            {
                var errMsg = "[Response] Post " + url + " 网络请求异常：error=" + request.error + " result=" + request.result;
                Debug.LogError(Colorize(errMsg, LogColor.ResponseErr));
            }
            
            request.Dispose();
            
            // 异步更新UI
            await UniTask.SwitchToMainThread(); // 切换到主线程更新UI
            var respTime = (DateTime.Now - beginTime).TotalMilliseconds;
            Debug.Log(Colorize("[Info] Post " + url + " > 网络请求响应耗时：" + respTime + "ms", LogColor.Info));
            return resp;
        }
    }
}