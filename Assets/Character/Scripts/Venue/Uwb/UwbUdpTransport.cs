using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// Nooploop IPS UDP 推送监听。
    /// </summary>
    public sealed class UwbUdpTransport : IUwbTransport
    {
        readonly int _port;
        readonly string _localAddress;
        UdpClient _udp;
        Thread _thread;
        volatile bool _running;

        public bool IsRunning => _running;
        public event Action<string> RawMessageReceived;

        public UwbUdpTransport(int port, string localAddress = null)
        {
            _port = port;
            _localAddress = localAddress;
        }

        public void Start()
        {
            if (_running)
                return;
            _running = true;
            _thread = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "UwbUdp"
            };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            try { _udp?.Close(); } catch { /* ignore */ }
            _udp = null;
            try { _thread?.Join(500); } catch { /* ignore */ }
        }

        public void Tick(float deltaTime)
        {
        }

        public void Dispose()
        {
            Stop();
        }

        void ListenLoop()
        {
            try
            {
                if (!string.IsNullOrEmpty(_localAddress) && IPAddress.TryParse(_localAddress, out IPAddress ip))
                    _udp = new UdpClient(new IPEndPoint(ip, _port));
                else
                    _udp = new UdpClient(_port);

                Debug.Log($"[UwbUdp] listening :{_port}");
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                while (_running)
                {
                    try
                    {
                        byte[] data = _udp.Receive(ref remote);
                        if (data == null || data.Length == 0)
                            continue;
                        string text = Encoding.UTF8.GetString(data);
                        RawMessageReceived?.Invoke(text);
                    }
                    catch (SocketException)
                    {
                        if (!_running)
                            break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        if (_running)
                            Debug.LogWarning("[UwbUdp] " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[UwbUdp] bind failed: " + e.Message);
            }
        }
    }
}
