using System;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace LetsPiu
{
    /// <summary>
    /// 用 /serverTime 校验与服务器的时间差。采样 3 次取往返最短的一次。
    /// UWB 比分牌不修改系统时钟（不做 su / date）。
    /// </summary>
    public static class ServerTimeSync
    {
        const int SampleCount = 3;
        const int SampleGapMs = 200;
        const int MaxRttMs = 2000;
        const int RequestTimeoutSec = 5;
        const int MatchSyncMinAdjustMs = 300;

        static bool s_Syncing;

        class Envelope
        {
            public bool success;
            public int return_code;
            public string msg;
            public string data;
        }

        class Payload
        {
            public long serverTimeMs;
            public long clientSendMs;
        }

        class Sample
        {
            public int index;
            public long t0;
            public long t1;
            public long serverTimeMs;
            public long rttMs;
        }

        /// <summary>
        /// 比赛开始时调用。设备时钟每天会慢几秒，启动时同步一次不够，每场比赛开始前再对齐一次；
        /// 在后台执行，不阻塞调用方，上一次还没结束时忽略。
        /// </summary>
        public static void SyncInBackground(string reason)
        {
            if (s_Syncing)
            {
                Debug.Log("[ServerTimeSync] 上一次同步未结束，忽略本次请求 reason=" + reason);
                return;
            }

            Debug.Log("[ServerTimeSync] 后台同步 reason=" + reason);
            RunBackgroundSync().Forget();
        }

        static async UniTaskVoid RunBackgroundSync()
        {
            s_Syncing = true;
            try
            {
                await SyncAsync(MatchSyncMinAdjustMs);
            }
            finally
            {
                s_Syncing = false;
            }
        }

        /// <param name="minAdjustMs">偏差小于该值时不改系统时间，避免采样噪声带来多余调整；启动同步传 0</param>
        public static async UniTask<bool> SyncAsync(int minAdjustMs = 0)
        {
            var device = AppStateManager.Instance != null ? AppStateManager.Instance.DeviceInfo5S : null;
            string productKey = device != null ? device.GetProductKey() : null;
            Debug.Log("[ServerTimeSync] 开始。本地时间 " + FormatMs(UtcNowMs())
                + " product_key=" + (productKey ?? "null"));

            if (!TryParseStoreId(productKey, out int storeId))
            {
                Debug.LogError("[ServerTimeSync] 失败：无法从 product_key 解析 storeId，product_key=" + (productKey ?? "null"));
                return false;
            }

            Debug.Log("[ServerTimeSync] storeId=" + storeId);

            Sample best = null;
            for (int i = 1; i <= SampleCount; i++)
            {
                if (i > 1)
                {
                    await UniTask.Delay(SampleGapMs);
                }

                var sample = await TakeSampleAsync(i, storeId);
                if (sample == null)
                {
                    continue;
                }

                if (sample.rttMs > MaxRttMs)
                {
                    Debug.LogWarning("[ServerTimeSync] 第" + i + "次丢弃：rtt=" + sample.rttMs + "ms，超过 " + MaxRttMs + "ms");
                    continue;
                }

                Debug.Log("[ServerTimeSync] 第" + i + "次可用 rtt=" + sample.rttMs
                    + "ms 单程误差上界约 " + (sample.rttMs / 2) + "ms");
                if (best == null || sample.rttMs < best.rttMs)
                {
                    best = sample;
                }
            }

            if (best == null)
            {
                Debug.LogError("[ServerTimeSync] 失败：没有可用采样");
                return false;
            }

            long now = UtcNowMs();
            long targetMs = best.serverTimeMs + now - (best.t0 + best.t1) / 2;
            long offsetMs = targetMs - now;
            Debug.Log("[ServerTimeSync] 选用第" + best.index + "次 rtt=" + best.rttMs
                + "ms。本地 " + FormatMs(now)
                + " 目标 " + FormatMs(targetMs)
                + " 需要调整 " + offsetMs + "ms");

            if (minAdjustMs > 0 && Math.Abs(offsetMs) < minAdjustMs)
            {
                Debug.Log("[ServerTimeSync] 偏差 " + offsetMs + "ms 小于 " + minAdjustMs + "ms，本次不调整");
                return true;
            }

            // UWB 比分牌不改系统时钟（无 su / date），只校验与服务器的时间差并放行。
            Debug.Log("[ServerTimeSync] 采样成功，不修改系统时间。相对服务器偏差约 " + offsetMs + "ms");
            return true;
        }

        static async UniTask<Sample> TakeSampleAsync(int index, int storeId)
        {
            long t0 = UtcNowMs();
            string url = Constants.HTTP_SERVER_TIME_URL + "?storeId=" + storeId + "&clientSendMs=" + t0;
            Debug.Log("[ServerTimeSync] 第" + index + "次请求 " + url);

            using var request = UnityWebRequest.Get(url);
            request.timeout = RequestTimeoutSec;
            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError("[ServerTimeSync] 第" + index + "次请求异常 " + e.Message);
            }

            long t1 = UtcNowMs();
            long rtt = t1 - t0;
            string body = request.downloadHandler != null ? request.downloadHandler.text : "";
            Debug.Log("[ServerTimeSync] 第" + index + "次返回 code=" + request.responseCode
                + " result=" + request.result
                + " rtt=" + rtt + "ms"
                + " t0=" + FormatMs(t0)
                + " t1=" + FormatMs(t1)
                + " body=" + body);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[ServerTimeSync] 第" + index + "次失败 error=" + request.error);
                return null;
            }

            Envelope envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<Envelope>(body);
            }
            catch (Exception e)
            {
                Debug.LogError("[ServerTimeSync] 第" + index + "次外层 JSON 解析失败 " + e.Message);
                return null;
            }

            if (envelope == null || !envelope.success || string.IsNullOrEmpty(envelope.data))
            {
                Debug.LogError("[ServerTimeSync] 第" + index + "次业务失败 success="
                    + (envelope != null && envelope.success)
                    + " return_code=" + (envelope != null ? envelope.return_code.ToString() : "null")
                    + " msg=" + (envelope != null ? envelope.msg : "null"));
                return null;
            }

            Payload payload;
            try
            {
                payload = JsonConvert.DeserializeObject<Payload>(envelope.data);
            }
            catch (Exception e)
            {
                Debug.LogError("[ServerTimeSync] 第" + index + "次 data 解析失败 " + e.Message + " data=" + envelope.data);
                return null;
            }

            if (payload == null || payload.serverTimeMs <= 0)
            {
                Debug.LogError("[ServerTimeSync] 第" + index + "次没有 serverTimeMs data=" + envelope.data);
                return null;
            }

            if (payload.clientSendMs != t0)
            {
                Debug.LogError("[ServerTimeSync] 第" + index + "次 clientSendMs 对不上，发送 " + t0
                    + " 回显 " + payload.clientSendMs);
                return null;
            }

            Debug.Log("[ServerTimeSync] 第" + index + "次解析 serverTimeMs=" + FormatMs(payload.serverTimeMs)
                + " 回显 clientSendMs=" + payload.clientSendMs
                + " return_code=" + envelope.return_code);
            return new Sample
            {
                index = index,
                t0 = t0,
                t1 = t1,
                serverTimeMs = payload.serverTimeMs,
                rttMs = rtt,
            };
        }

        static bool TryParseStoreId(string productKey, out int storeId)
        {
            storeId = 0;
            if (string.IsNullOrWhiteSpace(productKey))
            {
                return false;
            }

            string text = productKey.Trim().Trim('/');
            if (text.StartsWith("store", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring("store".Length);
            }

            return int.TryParse(text, out storeId) && storeId > 0;
        }

        static long UtcNowMs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        static string FormatMs(long unixMs)
        {
            var local = DateTimeOffset.FromUnixTimeMilliseconds(unixMs).ToLocalTime();
            return local.ToString("yyyy-MM-dd HH:mm:ss.fff") + " unix=" + unixMs;
        }
    }
}
