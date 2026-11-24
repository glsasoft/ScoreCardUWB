using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// Nooploop IPS MQTT 推送。官方为明文 JSON（不做 zlib）。
    /// </summary>
    public sealed class UwbMqttTransport : IUwbTransport
    {
        readonly string _host;
        readonly int _port;
        readonly string _username;
        readonly string _password;
        readonly string _topic;
        readonly string _clientId;
        readonly MqttFactory _factory = new MqttFactory();
        IMqttClient _client;
        CancellationTokenSource _cts;
        int _started;
        int _failNotified;

        public bool IsRunning => _client != null && _client.IsConnected;
        public bool HasConnectedOnce { get; private set; }
        public string LastError { get; private set; }
        public event Action<string> RawMessageReceived;
        public event Action Connected;
        public event Action<string> ConnectFailed;

        public UwbMqttTransport(string host, int port, string username, string password, string topic, string clientId)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _topic = string.IsNullOrEmpty(topic) ? "#" : topic;
            _clientId = string.IsNullOrEmpty(clientId)
                ? "ScoreCardUWB_" + Guid.NewGuid().ToString("N").Substring(0, 8)
                : clientId;
        }

        public static bool TryParseBrokerUrl(string url, out string host, out int port, out string user, out string pass)
        {
            host = null;
            port = 1883;
            user = null;
            pass = null;
            if (string.IsNullOrWhiteSpace(url))
                return false;

            string s = url.Trim();
            int scheme = s.IndexOf("://", StringComparison.Ordinal);
            if (scheme >= 0)
                s = s.Substring(scheme + 3);

            int at = s.LastIndexOf('@');
            string hostPort;
            if (at >= 0)
            {
                string cred = s.Substring(0, at);
                hostPort = s.Substring(at + 1);
                int colon = cred.IndexOf(':');
                if (colon >= 0)
                {
                    user = Uri.UnescapeDataString(cred.Substring(0, colon));
                    pass = Uri.UnescapeDataString(cred.Substring(colon + 1));
                }
                else
                {
                    user = Uri.UnescapeDataString(cred);
                }
            }
            else
            {
                hostPort = s;
            }

            int slash = hostPort.IndexOf('/');
            if (slash >= 0)
                hostPort = hostPort.Substring(0, slash);

            int pc = hostPort.LastIndexOf(':');
            if (pc > 0 && int.TryParse(hostPort.Substring(pc + 1), out int p))
            {
                host = hostPort.Substring(0, pc);
                port = p;
            }
            else
            {
                host = hostPort;
            }

            return !string.IsNullOrEmpty(host);
        }

        public void Start()
        {
            if (Interlocked.Exchange(ref _started, 1) == 1)
                return;
            _cts = new CancellationTokenSource();
            _ = ConnectLoop(_cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            try
            {
                if (_client != null)
                {
                    if (_client.IsConnected)
                        _client.DisconnectAsync().GetAwaiter().GetResult();
                    _client.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[UwbMqtt] stop: " + e.Message);
            }

            _client = null;
            _started = 0;
        }

        public void Tick(float deltaTime)
        {
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }

        async Task ConnectLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await ConnectOnce(token);
                    HasConnectedOnce = true;
                    LastError = null;
                    try { Connected?.Invoke(); } catch { /* ignore */ }
                    while (!token.IsCancellationRequested && _client != null && _client.IsConnected)
                        await Task.Delay(500, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    LastError = e.Message;
                    Debug.LogWarning("[UwbMqtt] connect fail: " + e.Message);
                    if (Interlocked.Exchange(ref _failNotified, 1) == 0)
                    {
                        try { ConnectFailed?.Invoke(e.Message); } catch { /* ignore */ }
                    }
                }

                if (token.IsCancellationRequested)
                    break;
                try { await Task.Delay(2000, token); } catch { break; }
            }
        }

        async Task ConnectOnce(CancellationToken token)
        {
            _client?.Dispose();
            _client = _factory.CreateMqttClient();
            _client.ApplicationMessageReceivedAsync += args =>
            {
                try
                {
                    if (args.ApplicationMessage == null)
                        return Task.CompletedTask;
                    var seg = args.ApplicationMessage.PayloadSegment;
                    if (seg.Count <= 0 || seg.Array == null)
                        return Task.CompletedTask;
                    string text = Encoding.UTF8.GetString(seg.Array, seg.Offset, seg.Count);
                    RawMessageReceived?.Invoke(text);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[UwbMqtt] payload: " + e.Message);
                }

                return Task.CompletedTask;
            };

            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(_host, _port)
                .WithClientId(_clientId)
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .WithTimeout(TimeSpan.FromSeconds(8));
            if (!string.IsNullOrEmpty(_username) || !string.IsNullOrEmpty(_password))
                builder = builder.WithCredentials(_username, _password);

            Debug.Log($"[UwbMqtt] connecting {_host}:{_port} topic={_topic}");
            await _client.ConnectAsync(builder.Build(), token);
            await _client.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(_topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .Build(), token);
            Debug.Log("[UwbMqtt] connected & subscribed");
        }
    }
}
