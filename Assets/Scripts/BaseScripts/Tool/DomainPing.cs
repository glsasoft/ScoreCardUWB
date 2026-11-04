using Cysharp.Threading.Tasks;

namespace LetsPiu.Base.BaseScripts.Tool
{
    using UnityEngine;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Collections;

    public class DomainPing
    {
        public static async Task<PingResult> PingDomainAsync(string domain, float timeout = 1.0f)
        {
            try
            {
                // 1. 获取域名对应的 IP 地址
                var addresses = await UniTask.RunOnThreadPool(async () => await Dns.GetHostAddressesAsync(domain));
                
                if (addresses == null || addresses.Length == 0)
                {
                    return new PingResult { Success = false, Error = "无法解析域名" };
                }

                var targetIP = addresses[0]; // 使用第一个 IP 地址

                // 2. 执行 Ping 操作
                var result = await PingIPAddressAsync(targetIP.ToString(), timeout);
                result.Domain = domain; // 记录域名
                return result;
            }
            catch (Exception e)
            {
                return new PingResult { Success = false, Error = e.Message };
            }
        }

        private static async UniTask<PingResult> PingIPAddressAsync(string ipAddress, float timeout = 1.0f)
        {
            var result = new PingResult { Success = false };
            await PingCoroutine(ipAddress, timeout, res =>
            {
                result = res;
            });
            return result;
        }

        private static IEnumerator PingCoroutine(string ipAddress, float timeout, Action<PingResult> callback)
        {
            var ping = new Ping(ipAddress);
            var timePassed = 0.0f;

            while (!ping.isDone && timePassed < timeout)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }

            var result = new PingResult();

            if (ping.isDone)
            {
                if (ping.time >= 0)
                {
                    result.Success = true;
                    result.RoundTripTime = ping.time;
                }
                else
                {
                    result.Success = false;
                    result.Error = "Ping 失败 (time < 0)";
                }
            }
            else
            {
                result.Success = false;
                result.Error = "Ping 超时";
            }

            ping.DestroyPing();
            callback(result);
        }
    }

    // 用于返回 Ping 结果的类
    public class PingResult
    {
        public bool Success { get; set; }
        public string Domain { get; set; }
        public int RoundTripTime { get; set; }
        public string Error { get; set; }
    }
}