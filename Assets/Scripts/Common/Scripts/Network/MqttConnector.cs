using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LetsPiu.Base;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Event;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Send;
using LetsPiu.Base.BaseScripts.Utils;
using LetsPiu.Common.Network.Model.Http.Play;
using LetsPiu.Common.Network.Model.Mqtt;
using LetsPiu.Common.Network.Model.Mqtt.Read;
using LetsPiu.Define;
using LetsPiu.Main.Common.Scripts;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.VFX;

namespace LetsPiu
{
    /// <summary>
    /// mqtt通信管理器
    /// </summary>
    public class MqttConnector : SingletonComponent<MqttConnector>
    {
        private class PingIndex
        {
            public int index;
            public DateTime? sent = null;
            public DateTime? recv = null;

            public PingIndex(int i)
            {
                this.index = i;
                this.sent = DateTime.Now;
            }
        }

        public enum MqttConnectState
        {
            NotConnect,
            Connecting,
            Connected,
            Disconnected,
        }

        /// <summary>
        /// 连接类型
        /// </summary>
        public enum ConnectType
        {
            /// <summary>
            /// 连接大厅mqtt
            /// </summary>
            Hall,
            /// <summary>
            /// 连接游戏mqtt
            /// </summary>
            Play,
            /// <summary>
            /// 连接比分牌
            /// </summary>
            ScoreCard,
        }

        public enum DisconnectReason
        {
            Exception,
            Timeout,
            BadUserNameOrPassword,
            Other,
        }

        private readonly MqttFactory mqttFactory = new MqttFactory();
        private IMqttClient client;
        private MqttConnectState connectState = MqttConnectState.NotConnect;
        private Exception error;

        private WaitForSeconds m_PreSecondWait;

        // 缓存所有ping列表
        private readonly List<PingIndex> pingIndexCache = new List<PingIndex>();
        // 当前发送的ping索引 0~99
        private int sendPingIndex = 0;
        private DateTime disconnectDateTime;
        private readonly int pingIntervalSeconds = 5;

        private string mqttUrl;
        private string mqttClientId;
        private string mqttUsername;
        private string mqttPassword;
        private ConnectType _connectType = ConnectType.Hall;
        private bool pingCheck = true;

        private List<string> mqttTopics = new List<string>();
        private Coroutine pingCoroutine;

        public MqttConnectState GetConnectState()
        {
            return connectState;
        }

        public Exception GetLastError()
        {
            return error;
        }

        private void OnEnable()
        {
            Debug.Log("MqttConnector OnEnable...");
            // EventManager.Instance.AddEvent<UploadMap>(EventMsg.MqttUploadMapMessageEvent, HandleUploadMap);
        }

        private void OnDisable()
        {
            Debug.Log("MqttConnector OnDisable...");
            // EventManager.Instance.RemoveEvent<UploadMap>(EventMsg.MqttUploadMapMessageEvent, HandleUploadMap);
        }

        private void Start()
        {

            Debug.Log("MqttConnector Start 线程ID:" + Thread.CurrentThread.ManagedThreadId);
            //MQTT心跳检查
            if (pingCoroutine != null) StopCoroutine(pingCoroutine);
            pingCoroutine = StartCoroutine(PingIndexHartBreathTask());
        }

        // /// <summary>
        // /// 连接成功后自动订阅
        // /// </summary>
        // private void HandleUploadMap(UploadMap uploadMap)
        // {
        //
        // }

        private void Awake()
        {
            Debug.LogWarning("MqttConnector Awake");
            m_PreSecondWait = new WaitForSeconds(pingIntervalSeconds);
            // 监听后台线程未处理异常，用于诊断 MQTTnet 内部抛出的无堆栈 NullReferenceException
            AppDomain.CurrentDomain.UnhandledException += OnBackgroundThreadUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDestroy()
        {
            Debug.LogWarning("MqttConnector OnDestroy 断开连接!!!");
            AppDomain.CurrentDomain.UnhandledException -= OnBackgroundThreadUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            if (_loraBridge != null)
            {
                _loraBridge.OnFrameReadyForMqtt -= PublishLoraUplink;
                _loraBridge = null;
            }
            DisconnectClient();
        }

        private void DisconnectClient()
        {
            Debug.LogWarning("MqttConnector DisconnectClient断开连接!!!");
            client?.Dispose();
            client = null;
            disconnectDateTime = DateTime.Now;
        }

        private static void OnBackgroundThreadUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Debug.LogError("[MqttConnector] 后台线程未处理异常 " + ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception?.GetBaseException();
            Debug.LogError("[MqttConnector] UnobservedTaskException " + ex?.GetType().Name + ": " + ex?.Message + "\n" + ex?.StackTrace);
            e.SetObserved();
        }

        // 每秒钟发送的ping消息
        private IEnumerator PingIndexHartBreathTask()
        {
            while (true)
            {
                yield return m_PreSecondWait;
                if (!pingCheck) continue;
                
                if (connectState == MqttConnectState.NotConnect)
                {
                    //未连接，不发送ping消息
                    Debug.LogWarning("MqttConnector MQTT尚未初始化!");
                    continue;
                }
                
                if (client == null || connectState == MqttConnectState.Disconnected)
                {
                    //连接已断开，重新连接
                    Reconnect();
                    continue;
                }
                
                if (client != null && connectState == MqttConnectState.Connecting)
                {
                    //连接中，不发送ping消息
                    Debug.Log("MqttConnector 连接中，不发送Ping...");
                    continue;
                }
                
                // 比分牌模式沿用 Android 原版：不发送游戏侧 pingIndex 心跳包。
                // 连接保活只依赖 MQTT 协议自身 KeepAlive 和断线回调。
                if (_connectType == ConnectType.ScoreCard)
                {
                    continue;
                }

                // 与 Android 版本保持一致：不基于 ping 回复超时来主动断连，
                // 断连判断完全依赖 MQTT 库的 DisconnectedAsync 回调（对应 Android 的 connectionLost）
                SendPingIndexMessage();

                pingIndexCache.Add(new PingIndex(sendPingIndex));
                sendPingIndex += 1;
                if (sendPingIndex >= 100)
                {
                    pingIndexCache.Clear();
                    sendPingIndex = 0;
                }
                Debug.Log("MqttConnector 发送Ping消息... sendPingIndex=" + sendPingIndex);
            }
        }

        private async UniTask<bool> ConnectAsync(string host, int port, string clientId, string userName, string password, [CanBeNull] MqttClientTlsOptions tlsOptions, float timeoutSeconds)
        {
            Debug.Log("MqttConnector 创建MQTT连接 host=" + host + " port=" + port + " clientId=" + clientId+" userName="+userName+" password="+password);
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port) // 要访问的mqtt服务端的 ip 和 端口号
                                           // .WithCredentials(username, password) // 要访问的mqtt服务端的用户名和密码
                .WithClientId(clientId) // 设置客户端id
                ;
            // 如果提供了用户名和密码，添加凭据
            if (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(password))
            {
                builder = builder.WithCredentials(userName, password);
            }
            // 继续设置其他选项
            builder = builder
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(timeoutSeconds))
                .WithTimeout(TimeSpan.FromSeconds(timeoutSeconds))
                .WithCleanSession();
            if (tlsOptions != null)
            {
                builder = builder.WithTlsOptions(tlsOptions);
            }
            var clientOptions = builder.Build();
            clientOptions.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            clientOptions.KeepAlivePeriod = TimeSpan.FromSeconds(timeoutSeconds);
            client = mqttFactory.CreateMqttClient();

            client.DisconnectedAsync += args =>
            {

                if (connectState == MqttConnectState.Connecting)
                {
                    Debug.LogWarning("MqttConnector 正在连接中，不处理DisconnectedAsync...");
                    return Task.CompletedTask;
                }

                connectState = MqttConnectState.Disconnected;
                error = args.Exception;

                UniTask.Post(Action);
                Debug.LogWarning("MqttConnector DisconnectedAsync 回调，断开连接！");
                DisconnectClient();
                return Task.CompletedTask;

                async void Action()
                {
                    await UniTask.SwitchToMainThread();
                    Debug.Log("MqttConnector IMqttClient Disconnected Reason:" + " " + args.ReasonString + " exception:" + args.Exception + " 线程ID:" + Thread.CurrentThread.ManagedThreadId);
                    UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttDisconnectEvent, args.Reason));
                }
            };

            client.ApplicationMessageReceivedAsync += args =>
            {
                UniTask.Post(() =>
                {
                    try
                    {
                        var payloadArray = args.ApplicationMessage?.PayloadSegment.Array;
                        var str = Encoding.UTF8.GetString(GZipUtil.ZlibInflate(payloadArray));
                        //过滤掉频繁打印的消息
                        if (LogMqttConnectorPublishAndReceiveMessage(str))
                        {
                            Debug.Log("MqttConnector MessageReceived: 主题=" + args.ApplicationMessage.Topic);
                            LongLogger.Log("MqttConnector MessageReceived:" + str);
                        }

                        if (string.IsNullOrEmpty(str))
                        {
                            Debug.LogError("MQTT消息不为空");
                            return;
                        }

                        // 比分牌5S服务器使用独立的扁平 JSON 格式，与游戏 type/data 外壳不同
                        if (_connectType == ConnectType.ScoreCard)
                        {
                            Debug.Log("[ScoreCard] 原始MQTT消息: " + str);

                            var scoreCardRoot = JsonHelper.DeserializeObject<JObject>(str);
                            if (scoreCardRoot == null)
                            {
                                Debug.LogWarning("[ScoreCard] 消息反序列化失败，str=" + str);
                                return;
                            }

                            return;
                        }

                        var msg = JsonHelper.DeserializeObject<MqttReadMessage<JToken>>(str);
                        if (msg == null)
                        {
                            Debug.LogWarning("MqttConnector 消息反序列化失败，str=" + str);
                            return;
                        }
                        //Debug.Log("MqttConnector 消息类型:" + msg.type + " " + str);
                        switch (msg.type)
                        {
                            case MqttMessageType.ping:
                                //Debug.Log("MqttConnector 收到Ping消息:" + str);
                                var pingMsg = msg.data?.ToObject<PingMessage>();
                                PopulatePingMessage(pingMsg);
                                EventManager.Instance.Broadcast(EventMsg.MqttPingMessageEvent, pingMsg);
                                break;
                            case MqttMessageType.replaceLogin:
                                EventManager.Instance.Broadcast(EventMsg.MqttReplaceLoginMessageEvent, msg.data?.ToObject<ReplaceLogin>());
                                break;
                            case MqttMessageType.playDetail:
                                var scorePlayDetail = msg.data?.ToObject<PlayDetail>();
                                PopulatePlayDetail(scorePlayDetail);
                                EventManager.Instance.Broadcast(EventMsg.MqttPlayDetailMessageEvent);
                                break;
                            case MqttMessageType.battlePlayer:
                                var battlePlayerList = msg.data?.ToObject<List<Player>>();
                                if (AppStateManager.Instance.Play != null && AppStateManager.Instance.Play.playStatus != GameState.END)
                                {
                                    PopulateBattlePlayer(battlePlayerList);
                                    EventManager.Instance.Broadcast(EventMsg.MqttBattlePlayerMessageEvent, battlePlayerList);
                                }
                                else
                                {
                                    Debug.LogWarning("MqttConnector 收到BattlePlayer消息，但游戏已结束，忽略此消息。");
                                }
                                break;
                        case MqttMessageType.voice:
                            EventManager.Instance.Broadcast(EventMsg.MqttVoiceMessageEvent, msg.data?.ToObject<VoiceReadMessage>());
                            break;
                        case MqttMessageType.playerProfession:
                            var professionMsg = msg.data?.ToObject<PlayerProfession>();
                            PopulatePlayerProfessionMessage(professionMsg);
                            break;
                        case MqttMessageType.skill:
                            var skillMessage = msg.data?.ToObject<MqttSkillMessage>();
                            PopulateSkillMessage(skillMessage);
                            EventManager.Instance.Broadcast(EventMsg.MqttSkillMessageEvent, skillMessage);
                            break;
                        case MqttMessageType.hit:
                            EventManager.Instance.Broadcast(EventMsg.MqttHitMessageEvent, msg.data?.ToObject<HitMessage>());
                            break;
                        case MqttMessageType.attackRecord:
                            var attackRecord = msg.data?.ToObject<AttackRecord>();
                            PopulateAttackInfo(attackRecord);
                            EventManager.Instance.Broadcast(EventMsg.MqttAttackRecordMessageEvent, attackRecord);
                            break;
                        case MqttMessageType.sufferRecord:
                            var sufferRecord = msg.data?.ToObject<SufferRecord>();
                            PopulateSufferInfo(sufferRecord);
                            EventManager.Instance.Broadcast(EventMsg.MqttSufferRecordMessageEvent, sufferRecord);
                            break;
                        case MqttMessageType.kill:
                            var killMsg = msg.data?.ToObject<KillMessage>();
                            PopulateKillInfo(killMsg);
                            EventManager.Instance.Broadcast(EventMsg.MqttKillMessageEvent, killMsg);
                            break;
                        case MqttMessageType.revive:
                            var revivePlayer = msg.data?.ToObject<Player>();
                            PopulatePlayerReviveInfo(revivePlayer);
                            EventManager.Instance.Broadcast(EventMsg.MqttReviveMessageEvent, revivePlayer);
                            break;
                        case MqttMessageType.fence:
                            var fenceMsg = msg.data?.ToObject<Fence>();
                            PopulateFenceMessage(fenceMsg);
                            EventManager.Instance.Broadcast(EventMsg.MqttFenceMessageEvent, fenceMsg);
                            break;
                        case MqttMessageType.poisonCircle:
                            EventManager.Instance.Broadcast(EventMsg.MqttPoisonCircleMessageEvent, msg.data?.ToObject<PoisonCircle>());
                            break;
                        case MqttMessageType.downMap:
                            var posList = msg.data?.ToObject<List<DownMap>>();
                            PopulateDownMap(posList);
                            EventManager.Instance.Broadcast(EventMsg.MqttDownMapMessageEvent, posList);
                            break;
                        case MqttMessageType.visualEffect:
                            EventManager.Instance.Broadcast(EventMsg.MqttVisualEffectMessageEvent, msg.data?.ToObject<VisualEffect>());
                            break;
                        case MqttMessageType.armor:
                            var armorValue = msg.data?.ToObject<ArmorData>();
                            PopulateArmorInfo(armorValue);
                            EventManager.Instance.Broadcast(EventMsg.MqttArmorMessageEvent, armorValue);
                            break;
                        case MqttMessageType.shield:
                            var shieldMsg = msg.data?.ToObject<MqttShieldMessage>();
                            PopulateShieldInfo(shieldMsg);
                            EventManager.Instance.Broadcast(EventMsg.MqttShieldMessageEvent, shieldMsg);
                            break;
                        // 处理其他类型的消息
                        default:
                            Debug.LogWarning("Unknown message type: " + msg.type);
                            break;
                    }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[MqttConnector] 消息处理异常: " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
                    }
                });
                return Task.CompletedTask;
            };
            connectState = MqttConnectState.Connecting;
            UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttConnectingEvent));
            try
            {
                var r = await client.ConnectAsync(clientOptions);
                Debug.Log("MqttConnector 连接结束:" + r + " 返回码:" + r.ResultCode + " ReasonString:" + r.ReasonString);
                if (r.ResultCode != MqttClientConnectResultCode.Success)
                {
                    connectState = MqttConnectState.Disconnected;
                    UniTask.Post(() =>
                    {
                        EventManager.Instance.Broadcast(EventMsg.MqttDisconnectEvent, MqttClientDisconnectReason.UnspecifiedError);
                    });
                }
                else
                {
                    Debug.Log("MqttConnector Mqtt连接成功! 开始订阅.");
                    await UniTask.WaitForSeconds(0.2f);
                    await SubScribeAsync();
                    await UniTask.WaitForSeconds(0.2f);
                    connectState = MqttConnectState.Connected;
                    Debug.Log("MqttConnector IMqttClient Connected 线程ID:" + Thread.CurrentThread.ManagedThreadId);
                    UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttConnectedEvent));
                }
                return r.ResultCode == MqttClientConnectResultCode.Success;
            }
            catch (Exception e)
            {
                error = e;
                Debug.LogError("MqttConnector 连接异常:" + e);
                connectState = MqttConnectState.Disconnected;
                DisconnectClient();
                //注意其他ui方法必须在主线程执行
                UniTask.Post(() =>
                {
                    EventManager.Instance.Broadcast(EventMsg.MqttDisconnectEvent, e.ToString().Contains("BadUserNameOrPassword")
                        ? MqttClientDisconnectReason.NotAuthorized
                        : MqttClientDisconnectReason.UnspecifiedError);
                });
                
                //如果是用户名密码错误，跳转到登录界面
                if(e.Message.Contains("BadUserNameOrPassword"))
                {
                    EventManager.Instance.Broadcast(EventMsg.LoginTokenTimeout);
                    EventManager.Instance.Broadcast(EventMsg.RequestLoginOut);
                }
                return false;
            }
        }

        private void PopulateShieldInfo(MqttShieldMessage mqttShieldMessage)
        {
            if (mqttShieldMessage != null)
            {
                var players = AppStateManager.Instance.PlayerList;
                if(players == null) return;
                var player = players.Find(p => p.shortId == mqttShieldMessage.shortId);
                if (player != null)
                {
                    player.playerShieldsValue = (int)mqttShieldMessage.shieldValue;
                }
            }
        }

        private async UniTask<bool> ConnectAsync(string url, string clientId, string username, string password)
        {
            if (url == null)
            {
                Debug.LogError("MqttConnector url为空!!!");
                return false;
            }

            var formattedUrl = url.Replace("ssl://", "https://");
            // 解析 URL
            var uri = new Uri(formattedUrl);

            // 提取主机和端口
            var host = uri.Host;
            var port = uri.Port;
            MqttClientTlsOptions tlsOptions = null;
            // 判断是否使用了 HTTPS（SSL/TLS）
            var isHttps = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

            if (isHttps)
            {
                var certText = "-----BEGIN CERTIFICATE-----\nMIIG4TCCBMmgAwIBAgIQCZg3qqLnmfV50WRyMC8kODANBgkqhkiG9w0BAQsFADBb\nMQswCQYDVQQGEwJDTjElMCMGA1UEChMcVHJ1c3RBc2lhIFRlY2hub2xvZ2llcywg\nSW5jLjElMCMGA1UEAxMcVHJ1c3RBc2lhIERWIFRMUyBSU0EgQ0EgMjAyNTAeFw0y\nNTA3MjQwMDAwMDBaFw0yNTEwMjEyMzU5NTlaMB4xHDAaBgNVBAMTE3BpdW1xdHQu\nZ2xzYS5jb20uY24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDjyRJT\nXhWzrtYonK1V/Hfj4YDzARKw/1AflU9UxvpxkoUhAv1rOgboHzpyMGUn+e/sVaRe\nzq/xVi7ZS6y3Rf9NG7HYhzfAA3ONr5FpmYCE7J2uo0y6xn3G5y6+QqUMnbUezQdX\nhwrmnQTJCuHKNjcuDOXhzMb3Ri8Am4OoVLO5Ocvtah/Neyi6U3xEpbzwu/7fqU0h\nC5xJysj+qJO1SRfRbZLztwR3DShO9FFmkKkaIcctn9d/i5JeXrLNeu4pWMnrnHTt\nkB8hnB70C3n9tZPSCQoQo/on05BDuFW8D1+vYy5JjDg44l0rtOkgOJQXPCAnRcWf\nuTQZfNk6Uc6wgBARAgMBAAGjggLcMIIC2DAfBgNVHSMEGDAWgBS0EiiltMAdnylx\naTzZEZZKdWlQwDAdBgNVHQ4EFgQUhQmyF7acvxxMjKw7vV2IlZWl4E0wHgYDVR0R\nBBcwFYITcGl1bXF0dC5nbHNhLmNvbS5jbjA+BgNVHSAENzA1MDMGBmeBDAECATAp\nMCcGCCsGAQUFBwIBFhtodHRwOi8vd3d3LmRpZ2ljZXJ0LmNvbS9DUFMwDgYDVR0P\nAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjB5BggrBgEF\nBQcBAQRtMGswJAYIKwYBBQUHMAGGGGh0dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBD\nBggrBgEFBQcwAoY3aHR0cDovL2NhY2VydHMuZGlnaWNlcnQuY29tL1RydXN0QXNp\nYURWVExTUlNBQ0EyMDI1LmNydDAMBgNVHRMBAf8EAjAAMIIBfAYKKwYBBAHWeQIE\nAgSCAWwEggFoAWYAdQAS8U40vVNyTIQGGcOPP3oT+Oe1YoeInG0wBYTr5YYmOgAA\nAZg8jyIJAAAEAwBGMEQCIDmA1PwOATaHib3mMIE7MMqkdhCdgCo/BGCQaaL9cCI2\nAiAlhx9LBSno3kLKXL5lSQzT1b/AHKd50II0tozYgQurAwB2AO08S9boBsKkogBX\n28sk4jgB31Ev7cSGxXAPIN23Pj/gAAABmDyPIkcAAAQDAEcwRQIgaJaBorhGbrg9\nuc4xPUKU/K9SeeNJGmuHfn+/ci+Q0n0CIQCtbMy4CJvx0oRPDZZwSP6zVVeRqX8e\nnCfGPPb2iNHkgwB1AKRCxQZJYGFUjw/U6pz7ei0mRU2HqX8v30VZ9idPOoRUAAAB\nmDyPIlkAAAQDAEYwRAIgLVOPIZvGRXypUQ78G05ZGDfW2LZV9qDexmtes0N9yf4C\nIFiZ0kzQiemM/gbM/g327zXvO2h98RlHIofqw8LSIBOqMA0GCSqGSIb3DQEBCwUA\nA4ICAQAjy//gGtslXEVBGbCB2MrWSPD61G08Oxr1ZJjrl+owIHsr4r6L5UhvjCin\nDWKFeiWpu4G51bq2cWwbO4Xj57rghnLltrrF7KYbfwOUVTsUBpLnsLcWislPvppy\nZvJDZ000QkBqepxKdMYP9mhS/oUaLjPGrb5YZ6qos/R3gO70Rvdp8k6tuIu3z+z6\nA5Amp1kED3moOS0wxS4RiuMCVrH70/o3PCqitWuTd+i1Bgc1tUlM+Rui2l2zGQJ6\nYhjgEok/ehLuMRg0cRzx0hn/NYIUqhK4zmmfUP1sOOKECFsIea5COUFkJnYuH8/8\ni2P7pBwS+ZRh61SQQ7QtZSm1IZpv1BfR4eCmH+/ovBcW+7d0kq6ONdI7JxXojYtb\nqArnn7oqcCXqVXixyDViNZMmMklmYZN2d92/CKhk44Xa4z8cW1gDJNeLX5OsZens\nQd9HDAyV1EAt2fnOJ9pe3mh701tpZrPaULHjcNMphh76JJYw7nEQXET373rCyr0Z\nPVqpVG+FF/KYKwhk+pymnA+OwO1tqtTdWvn3Rg6ohkVoF2yjy05y25ngBrUjmmZT\nfmXZznZyYCSJXm3PYnTBhDkmX30JSqb9J4+qQbdLQUHLhDHHUpd75pvEDjF/hLos\nBIAwrilC5l8ol0GKch11yXXBhgsmZk+y0wPWG5iPFTiMbDvIfQ==\n-----END CERTIFICATE-----\n-----BEGIN CERTIFICATE-----\nMIIFnjCCBIagAwIBAgIQCSYyO0lk42hGFRLe8aXVLDANBgkqhkiG9w0BAQsFADBh\nMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3\nd3cuZGlnaWNlcnQuY29tMSAwHgYDVQQDExdEaWdpQ2VydCBHbG9iYWwgUm9vdCBH\nMjAeFw0yNTAxMDgwMDAwMDBaFw0zNTAxMDcyMzU5NTlaMFsxCzAJBgNVBAYTAkNO\nMSUwIwYDVQQKExxUcnVzdEFzaWEgVGVjaG5vbG9naWVzLCBJbmMuMSUwIwYDVQQD\nExxUcnVzdEFzaWEgRFYgVExTIFJTQSBDQSAyMDI1MIICIjANBgkqhkiG9w0BAQEF\nAAOCAg8AMIICCgKCAgEA0fuEmuBIsN6ZZVq+gRobMorOGIilTCIfQrxNpR8FUZ9R\n/GfbiekbiIKphQXEZ7N1uBnn6tXUuZ32zl6jPkZpHzN/Bmgk1BWSIzVc0npMzrWq\n/hrbk5+KddXJdsNpeG1+Q8lc8uVMBrztnxaPb7Rh7yQCsMrcO4hgVaqLJWkVvEfW\nULtoCHQnNaj4IroG6VxQf1oArQ8bPbwpI02lieSahRa78FQuXdoGVeQcrkhtVjZs\nON98vq5fPWZX2LFv7e5J6P9IHbzvOl8yyQjv+2/IOwhNSkaXX3bI+//bqF9XW/p7\n+gsUmHiK5YsvLjmXcvDmoDEGrXMzgX31Zl2nJ+umpRbLjwP8rxYIUsKoEwEdFoto\nAid59UEBJyw/GibwXQ5xTyKD/N6C8SFkr1+myOo4oe1UB+YgvRu6qSxIABo5kYdX\nFodLP4IgoVJdeUFs1Usa6bxYEO6EgMf5lCWt9hGZszvXYZwvyZGq3ogNXM7eKyi2\n20WzJXYMmi9TYFq2Fa95aZe4wki6YhDhhOO1g0sjITGVaB73G+JOCI9yJhv6+REN\nD40ZpboUHE8JNgMVWbG1isAMVCXqiADgXtuC+tmJWPEH9cR6OuJLEpwOzPfgAbnn\n2MRu7Tsdr8jPjTPbD0FxblX1ydW3RG30vwLF5lkTTRkHG9epMgpPMdYP7nY/08MC\nAwEAAaOCAVYwggFSMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFLQSKKW0\nwB2fKXFpPNkRlkp1aVDAMB8GA1UdIwQYMBaAFE4iVCAYlebjbuYP+vq5Eu0GF485\nMA4GA1UdDwEB/wQEAwIBhjAdBgNVHSUEFjAUBggrBgEFBQcDAQYIKwYBBQUHAwIw\ndgYIKwYBBQUHAQEEajBoMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5kaWdpY2Vy\ndC5jb20wQAYIKwYBBQUHMAKGNGh0dHA6Ly9jYWNlcnRzLmRpZ2ljZXJ0LmNvbS9E\naWdpQ2VydEdsb2JhbFJvb3RHMi5jcnQwQgYDVR0fBDswOTA3oDWgM4YxaHR0cDov\nL2NybDMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0R2xvYmFsUm9vdEcyLmNybDARBgNV\nHSAECjAIMAYGBFUdIAAwDQYJKoZIhvcNAQELBQADggEBAJ4a3svh316GY2+Z7EYx\nmBIsOwjJSnyoEfzx2T699ctLLrvuzS79Mg3pPjxSLlUgyM8UzrFc5tgVU3dZ1sFQ\nI4RM+ysJdvIAX/7Yx1QbooVdKhkdi9X7QN7yVkjqwM3fY3WfQkRTzhIkM7mYIQbR\nr+y2Vkju61BLqh7OCRpPMiudjEpP1kEtRyGs2g0aQpEIqKBzxgitCXSayO1hoO6/\n71ts801OzYlqYW9OQQQ2GCJyFbD6XHDjdpn+bWUxTKWaMY0qedSCbHE3Kl2QEF0C\nynZ7SbC03yR+gKZQDeTXrNP1kk5Qhe7jSXgw+nhbspe0q/M1ZcNCz+sPxeOwdCcC\ngJE=\n-----END CERTIFICATE-----";
                //文本转换成字节
                var certBytes = Encoding.UTF8.GetBytes(certText);
                var certificateFromAsset = new X509Certificate2(certBytes);
                var list = new List<X509Certificate2> { certificateFromAsset };
                // 创建一个TLS选项
                tlsOptions = new MqttClientTlsOptionsBuilder()
                    .WithSslProtocols(SslProtocols.Tls12)
                    .WithClientCertificates(list)
                    .Build();
                // var pemAsset = await Resources.LoadAsync<TextAsset>("Certs/mqtt.glsa.com.cn_bundle.pem");
                // var asset = pemAsset as TextAsset;
                // if (asset != null)
                // {
                //     var certificateFromAsset = new X509Certificate2(asset.bytes);
                //     var list = new List<X509Certificate2> { certificateFromAsset };
                //     // 创建一个TLS选项
                //     tlsOptions = new MqttClientTlsOptionsBuilder()
                //         .WithSslProtocols(SslProtocols.Tls12)
                //         .WithClientCertificates(list)
                //         .Build();
                // }
                // else
                // {
                //     Debug.LogError("证书文件读取失败!!");
                // }
                Debug.Log("MqttConnector SSL连接启用证书!");
            }
            else
            {
                Debug.LogWarning("MqttConnector 非SSL连接!");
            }
            return await ConnectAsync(host, port, clientId, username, password, tlsOptions, 10f);
        }

        /// <summary>
        /// 连接MQTT服务器
        /// </summary>
        /// <param name="type">subscribeType</param>
        public async UniTask<bool> ConnectAsync(ConnectType type)
        {
            Debug.Log("MqttConnector ConnectAsync type=" + type);

            //确保玩家登录s
            if (type != ConnectType.ScoreCard)
            {
                Debug.LogError("MqttConnector mqtt连接失败: 未登录!");
                UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttDisconnectEvent, MqttClientDisconnectReason.NotAuthorized));
                return false;
            }
            
            if(AppStateManager.Instance.DeviceInfo5S == null)
            {
                Debug.LogError("MqttConnector mqtt连接失败: DeviceInfo5S为空!");
                UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttDisconnectEvent, MqttClientDisconnectReason.NotAuthorized));
                return false;
            }

            // 比分牌 LORA 上行：串口收到完整帧后发布到服务器（只挂一次）
            WireLoraUplinkOnce();

            //检测是否连接中
            if (connectState == MqttConnectState.Connecting)
            {
                //等待连接完成
                var dur = TimeSpan.Zero;
                while (connectState == MqttConnectState.Connecting)
                {
                    await UniTask.WaitForSeconds(0.3f);
                    dur += TimeSpan.FromSeconds(0.3f);

                    if (dur.TotalSeconds > 10)
                    {
                        //强制断开
                        Debug.LogError("MqttConnector ConnectAsync调用时，仍然连接中超过10秒!!");
                        break;
                    }
                }
            }

            CollectTopics(type);

            var newMqttUrl = "";
            newMqttUrl = AppStateManager.Instance.DeviceInfo5S.GetUri();
            Debug.Log("MqttConnector ConnectAsync ScoreCard 连接比分牌MQTT地址 newMqttUrl=" + newMqttUrl);

            mqttUrl = AppStateManager.Instance.DeviceInfo5S.GetUri();
            Debug.Log("MqttConnector ConnectAsync ScoreCard 连接比分牌MQTT地址 mqttUrl=" + mqttUrl);

            Debug.Log("MqttConnector ConnectAsync newMqttUrl=" + newMqttUrl);

            //如果要连接的新的mqtt ip和端口号不一致，断开现有连接
            if (connectState == MqttConnectState.Connected && !newMqttUrl.Equals(mqttUrl))
            {
                Debug.Log("MqttConnector 断开原有连接:" + mqttUrl);
                Disconnect();
            }

            mqttUrl = newMqttUrl;

            mqttClientId = AppStateManager.Instance.DeviceInfo5S.GetDeviceName();
            mqttUsername = AppStateManager.Instance.DeviceInfo5S.GetDeviceName();
            mqttPassword = AppStateManager.Instance.DeviceInfo5S.GetDevicePass();
            Debug.Log("MqttConnector ConnectAsync ScoreCard 连接比分牌 mqttClientId=" + mqttClientId + " mqttUsername=" + mqttUsername+" mqttPassword=" + mqttPassword);
            _connectType = type;
            return await ConnectAsync(mqttUrl, mqttClientId, mqttUsername, mqttPassword);
        }

        /// <summary>
        /// 连接类型没有改变时，重新订阅，如果连接方式变了，需要断开连接。
        /// </summary>
        public async UniTask ReSubScribe()
        {
            if (client == null)
            {
                Debug.LogError("MqttConnector ReSubScribe: client is null!");
                return;
            }
            foreach (var mqttTopic in mqttTopics)
            {
                try
                {
                    Debug.Log("MqttConnector 取消订阅:" + mqttTopic);
                    await client.UnsubscribeAsync(mqttTopic);
                }
                catch (Exception e)
                {
                    Debug.LogError("MqttConnector " + e.Message);
                }
            }
            CollectTopics(_connectType);
            await SubScribeAsync();
        }

        private void CollectTopics(ConnectType type)
        {
            mqttTopics.Clear();

            switch (type)
            {
                case ConnectType.Hall:
                    break;
                case ConnectType.ScoreCard:
                    // 下行订阅 topic = publish_topic(服务器视角命名, 即设备下行 .../android/update)
                    mqttTopics.Add(AppStateManager.Instance.DeviceInfo5S.GetPublishTopic());
                    Debug.Log("MqttConnector (CollectTopics) ScoreCard 订阅Topic:" + AppStateManager.Instance.DeviceInfo5S.GetPublishTopic());
                    break;
                default:
                    //游戏中订阅
                    Debug.LogError("MqttConnector CollectTopics: 未知的ConnectType:" + type);
                    break;
            }
        }

        private async UniTask SubScribeAsync()
        {
            foreach (var mqttTopic in mqttTopics)
            {
                await SubScribeAsync(mqttTopic);
            }
        }

        private async UniTask SubScribeAsync(string topic)
        {
            if (client == null) return;
            try
            {
                Debug.Log("MqttConnector 开始订阅:" + topic);
                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(topic);
                        })
                    .Build();
                var subscribeResult = await client.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                // 检查订阅结果
                foreach (var item in subscribeResult.Items)
                {
                    if (item.ResultCode is MqttClientSubscribeResultCode.GrantedQoS0 or MqttClientSubscribeResultCode.GrantedQoS1 or MqttClientSubscribeResultCode.GrantedQoS2)
                    {
                        Debug.Log($"MqttConnector 订阅成功 '{item.TopicFilter.Topic}' with QoS {item.ResultCode}.");
                        UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttSubscribeSuccessEvent, topic));
                    }
                    else
                    {
                        Debug.LogError($"MqttConnector 订阅失败 '{item.TopicFilter.Topic}'. Result code: {item.ResultCode}");
                        UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttSubscribeFailEvent, topic, new Exception("code:" + item.ResultCode)));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"MqttConnector 订阅失败 " + e.Message);
                UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttSubscribeFailEvent, topic, e));
            }
        }

        /// <summary>
        /// 断开MQTT连接，并且不再重连
        /// </summary>
        public void Disconnect()
        {
            Debug.LogWarning("MqttConnector 用户手动断开MQTT，删除连接数据...");
            DisconnectClient();
            connectState = MqttConnectState.NotConnect;
            UniTask.Post(() => EventManager.Instance.Broadcast(EventMsg.MqttDisconnectEvent, MqttClientDisconnectReason.NormalDisconnection));
        }

        public void Reconnect()
        {
            if (connectState == MqttConnectState.Connecting)
            {
                return;
            }
            Debug.Log("MqttConnector 开始重连... mqttUrl=" + mqttUrl + " mqttClientId=" + mqttClientId+" mqttUsername=" + mqttUsername+" mqttPassword=" + mqttPassword);
            ConnectAsync(mqttUrl, mqttClientId, mqttUsername, mqttPassword).Forget();
        }

        private void Publish(string topic, string payload)
        {
            if (client == null)
            {
                Debug.LogWarning("[MqttConnector] PublishAsync: MQTT未链接!!!");
                return;
            }
            if (connectState != MqttConnectState.Connected)
            {
                Debug.LogWarning("[MqttConnector] PublishAsync: MQTT MqttConnectState:" + connectState);
                return;
            }

#if UNITY_EDITOR
            
            //过滤掉频繁打印的消息
            if (LogMqttConnectorPublishAndReceiveMessage(payload))
            {
                Debug.Log("[MqttConnector] PublishAsync: topic=" + topic + "  payload=" + payload);
            }

#endif

            DoPublishAsync(topic, payload).Forget();
        }

        // 使用 UniTaskVoid + await 代替 Task.ContinueWith，
        // 确保结果回调始终在 Unity 主线程执行，避免后台线程调用 Debug.Log 触发 NullReferenceException
        private async UniTaskVoid DoPublishAsync(string topic, string payload)
        {
            try
            {
                var capturedClient = client;
                if (capturedClient == null) return;

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();
                var result = await capturedClient.PublishAsync(applicationMessage);
                if (result == null)
                {
                    Debug.LogError("[MqttConnector] 发送结果为空(null)");
                    return;
                }
                if (!result.IsSuccess)
                    Debug.LogError("[MqttConnector] 发送失败:" + result.ReasonString);
            }
            catch (Exception e)
            {
                Debug.LogError("[MqttConnector] 发送异常: " + e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 过滤掉频繁打印的消息
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private bool LogMqttConnectorPublishAndReceiveMessage(string payload)
        {
            if (payload.Contains("ping"))
            {
                return false;
            }

            if (payload.Contains("uploadMap"))
            {
                return false;
            }

            if (payload.Contains("downMap"))
            {
                return false;
            }

            if (payload.Contains("frameSync"))
            {
                return true;
            }
            return true;
        }

        /// <summary>
        /// 关闭ping连接检查
        /// </summary>
        public void ClosePingCheck()
        {
            pingCheck = false;
        }

        /// <summary>
        /// 发送mqtt Ping消息
        /// </summary>
        private void SendPingIndexMessage()
        {
            if (mqttTopics is { Count: 0 })
            {
                Debug.LogError("MqttConnector Ping消息发送失败，主题列表为空!!");
                return;
            }

            var pingMessage = new MqttSendMessage<PingIndexMessage>(MqttMessageType.ping, new PingIndexMessage(sendPingIndex, null, null));
            //Debug.Log("MqttConnector 发送Ping消息: " + pingMessage.payload.index + " bullet=" + pingMessage.payload.bullet + " bulletBackpack=" + pingMessage.payload.bulletBackpack);
            var json = JsonHelper.SerializeObject(pingMessage);
            var playerTopic = mqttTopics[0];
            Publish(playerTopic, json);
        }

        /// <summary>
        /// 发送mqtt消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="type">类型</param>
        /// <param name="msg">消息</param>
        /// <typeparam name="T"></typeparam>
        public void SendMqttMessage<T>(string topic, MqttMessageType type, T msg)
        {
            var pingMessage = new MqttSendMessage<T>(type, msg);
            var json = JsonHelper.SerializeObject(pingMessage);
            Publish(topic, json);
        }

        /// <summary>
        /// 更新对战数据
        /// </summary>
        /// <param name="playDetail"></param>
        private void PopulatePlayDetail(PlayDetail playDetail)
        {
            var oldState = AppStateManager.Instance.Play == null ? GameState.END : AppStateManager.Instance.Play.playStatus;
            AppStateManager.Instance.PlayerList = playDetail.players;
            AppStateManager.Instance.TeamKillList = playDetail.teams;
            if (oldState != playDetail.play.playStatus)
            {
                if (playDetail.play.playStatus == GameState.END)
                {
                }
                else
                {
                    AppStateManager.Instance.Play = playDetail.play;
                    EventManager.Instance.Broadcast(EventMsg.MqttGameStateChanged, playDetail.play.playStatus);
                }
            }
        }

        /// <summary>
        /// 同步玩家数据
        /// </summary>
        /// <param name="playersInfo"></param>
        private void PopulateBattlePlayer(List<Player> playersInfo)
        {
            //更新玩家数据
            var map = new Dictionary<int, Player>();
            foreach (var player in playersInfo)
            {
                map[player.shortId] = player;
            }
            var players = AppStateManager.Instance.PlayerList;
            // var self = AppStateManager.Instance.GetPlayer();
            foreach (var player in players)
            {
                Player newPlayerInfo;
                map.TryGetValue(player.shortId, out newPlayerInfo);
                if (newPlayerInfo != null)
                {
                    //玩家血量从0变为非0，要发送复活消息
                    var oldPlayerBlood = player.blood;
                    var newPlayerBlood = newPlayerInfo.blood;

                    //注意不存在的字段不能更新，否则会导致数据边空
                    player.name = newPlayerInfo.name;
                    player.imageUrl = newPlayerInfo.imageUrl;
                    player.blood = newPlayerInfo.blood;
                    player.degree = newPlayerInfo.degree;
                    //注意，battlePlayer中没有经纬度!
                    player.armor = newPlayerInfo.armor;
                    player.addBullet = newPlayerInfo.addBullet;
                    //背包和弹夹内子弹不更新，在本地控制!!!
                    player.adsBulletSpread = newPlayerInfo.adsBulletSpread;
                    player.armorValue = newPlayerInfo.armorValue;
                    player.armorReduceHarm = newPlayerInfo.armorReduceHarm;
                    player.playerArmorValue = newPlayerInfo.playerArmorValue;
                    player.shieldsValue = newPlayerInfo.shieldsValue;
                    player.playerShieldsValue = newPlayerInfo.playerShieldsValue;
                    Debug.Log(player.name+"基础护盾值："+ player.shieldsValue+"护盾值"+ player.playerShieldsValue);
                    player.battleStatus = newPlayerInfo.battleStatus;
                    player.weaponDamage = newPlayerInfo.weaponDamage;
                    player.sp = newPlayerInfo.sp;
                    player.totalSp = newPlayerInfo.totalSp;
                    player.headHarm = newPlayerInfo.headHarm;
                    player.name = newPlayerInfo.name;
                    player.continuousKillNumber = newPlayerInfo.continuousKillNumber;
                    player.skillEndTime = newPlayerInfo.skillEndTime;
                    player.skillCooldownMultiplier = newPlayerInfo.skillCooldownMultiplier;
                    player.skillShootSpeed = newPlayerInfo.skillShootSpeed;
                    player.skillHits = newPlayerInfo.skillHits;
                    player.ultimateSkillEndTime = newPlayerInfo.ultimateSkillEndTime;
                    player.ultimateEnergy = newPlayerInfo.ultimateEnergy;
                    player.ultimateEnergyMultiplier = newPlayerInfo.ultimateEnergyMultiplier;
                    player.torsoHarm = newPlayerInfo.torsoHarm;
                    player.assistNumber = newPlayerInfo.assistNumber;
                    player.killNumber = newPlayerInfo.killNumber;
                    player.deathNumber = newPlayerInfo.deathNumber;
                    player.strikeScore = newPlayerInfo.strikeScore;
                    player.strikenScore = newPlayerInfo.strikenScore;

                    if (oldPlayerBlood == 0 && newPlayerBlood != 0)
                    {
                        //玩家血量由0变为非零，发送玩家复活消息
                        Debug.Log("MqttConnector PopulateBattlePlayer 发送 OtherPlayerRevive 消息");
                        //其他玩家复活
                        EventManager.Instance.Broadcast(EventMsg.OtherPlayerRevive, new PlayerChangeMessage()
                        {
                            isRealTime = true,
                            player = player
                        });
                    }

                    if (player.battleStatus != newPlayerInfo.battleStatus)
                    {
                        player.battleStatus = newPlayerInfo.battleStatus;
                        if (newPlayerInfo.battleStatus == PlayerBattleStatus.Dead)
                        {
                            Debug.Log("MqttConnector PopulateBattlePlayer 发送 OtherPlayerDead 消息");
                            //其他玩家死亡
                            EventManager.Instance.Broadcast(EventMsg.OtherPlayerDead, new PlayerChangeMessage()
                            {
                                isRealTime = true,
                                player = player
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新玩家的技能状态(线下是自己，线上是所有人)
        /// </summary>
        /// <param name="skillMessage"></param>
        private void PopulateSkillMessage(MqttSkillMessage skillMessage)
        {
            var player = AppStateManager.Instance.GetPlayerByShortId(skillMessage.shortId);
            //skillMessage.status = 2 是冷却时间变更 / 大招能量条变更
            if (player == null) return;
            player.skillShootSpeed = skillMessage.skillShootSpeed;
            switch (skillMessage.type)
            {
                case SkillType.SmallSkill:
                    player.coolDownEndTime = skillMessage.coolDownEndTime;
                    player.skillEndTime = skillMessage.skillEndTime;
                    Debug.Log("MqttConnector PopulateSkillMessage SmallSkill: " + player.shortId + " 技能状态=" + skillMessage.status);
                    break;
                case SkillType.UltimateSkill:
                    player.ultimateEnergy = skillMessage.ultimateEnergy;
                    player.ultimateSkillEndTime = skillMessage.ultimateSkillEndTime;
                    Debug.Log("MqttConnector PopulateSkillMessage UltimateSkill: " + player.shortId + " 技能状态=" + skillMessage.status);
                    break;
            }
        }

        /// <summary>
        /// 更新玩家的角色状态
        /// </summary>
        /// <param name="playerProfession"></param>
        private void PopulatePlayerProfessionMessage(PlayerProfession playerProfession)
        {
            if (playerProfession is null)
            {
                return;
            }

            var playerData = AppStateManager.Instance.GetPlayerByShortId(playerProfession.shortId);
            
            if (playerData is not null)
            {
                //更新玩家能量条
                if(playerProfession.coolDownEndTime != null)  playerData.coolDownEndTime = playerProfession.coolDownEndTime;
                if(playerProfession.ultimateEnergy != null) playerData.ultimateEnergy = playerProfession.ultimateEnergy;
                
                if (playerData.professionId != playerProfession.professionId)
                {
                    playerData.professionId = playerProfession.professionId;
                    EventManager.Instance.Broadcast(EventMsg.MqttPlayerProfessionEvent, playerProfession);
                }

                if (playerData.beforehandProfessionId != playerProfession.beforehandProfessionId)
                {
                    playerData.beforehandProfessionId = playerProfession.beforehandProfessionId;
                    EventManager.Instance.Broadcast(EventMsg.MqttPlayerChoosseProfessionEvent, playerProfession);
                }
            }
        }

        /// <summary>
        /// 更新玩家位置
        /// </summary>
        /// <param name="downMapList"></param>
        private void PopulateDownMap(List<DownMap> downMapList)
        {
            if (downMapList == null) return;
            var listMap = new Dictionary<int, DownMap>();
            var index = 0;
            for (; index < downMapList.Count; index++)
            {
                var downMap = downMapList[index];
                listMap.Add(downMap.shortId, downMap);
            }

            foreach (var player in AppStateManager.Instance.PlayerList)
            {
                if (!listMap.TryGetValue(player.shortId, out var partInfo))
                {
                    // Debug.LogWarning("OfflineGameMapUI->UpdatePlayers 未找到玩家位置数据 shortId=" + player.shortId);
                    continue;
                }
                player.longitude = partInfo.longitude;
                player.latitude = partInfo.latitude;
                player.degree = partInfo.degree; }
        }

        /// <summary>
        /// 更新玩家数据
        /// </summary>
        /// <param name="revivePlayer"></param>
        private void PopulatePlayerReviveInfo(Player revivePlayer)
        {
            var oldPlayer = AppStateManager.Instance.GetPlayerByShortId(revivePlayer.shortId);

            var revivePlayerShortId = revivePlayer.shortId;

            //状态、血量、护甲等信息更新
            oldPlayer.blood = revivePlayer.blood;
            oldPlayer.status = revivePlayer.status;
            oldPlayer.deathNumber = revivePlayer.deathNumber;
            oldPlayer.killNumber = revivePlayer.killNumber;
            oldPlayer.battleStatus = revivePlayer.battleStatus;
            oldPlayer.bullet = revivePlayer.bullet;
            oldPlayer.bulletBackpack = revivePlayer.bulletBackpack;
            oldPlayer.bulletVolume = revivePlayer.bulletVolume;
            oldPlayer.addBullet = revivePlayer.addBullet;
            oldPlayer.bulletSpread = revivePlayer.bulletSpread;
            oldPlayer.harm = revivePlayer.harm;
            oldPlayer.sp = revivePlayer.sp;
            oldPlayer.armorValue = revivePlayer.armorValue;
            oldPlayer.armor = revivePlayer.armor;
            oldPlayer.shieldsValue = revivePlayer.shieldsValue;

            Debug.Log("MqttConnector PopulatePlayerReviveInfo 发送 OtherPlayerRevive 消息");
            //其他玩家复活
            EventManager.Instance.Broadcast(EventMsg.OtherPlayerRevive, new PlayerChangeMessage()
            {
                isRealTime = true,
                player = oldPlayer
            });
        }

        /// <summary>
        /// 更新玩家护甲数据
        /// </summary>
        /// <param name="armor">护甲</param>
        private void PopulateArmorInfo(ArmorData armordata)
        {
            var player = AppStateManager.Instance.GetPlayerByShortId(armordata.shortId);
            if (player is not null)
            {
                player.armorValue = armordata.armorValue;
            }
        }

        /// <summary>
        /// 更新玩家数据
        /// </summary>
        /// <param name="killMessage"></param>
        private void PopulateKillInfo(KillMessage killMessage)
        {
            if (killMessage.teamKills != null)
            {
                AppStateManager.Instance.TeamKillList = killMessage.teamKills;
            }
            
            if (killMessage.playerSps != null)
            {
                foreach (var killMessagePlayerSp in killMessage.playerSps)
                {
                    if (killMessagePlayerSp == null) continue;
                    var p = AppStateManager.Instance.GetPlayerByShortId(killMessagePlayerSp.shortId);
                    if (p != null)
                    {
                        p.sp = killMessagePlayerSp.sp;
                    }
                }
            }

            //添加到击杀历史记录中
            AppStateManager.Instance.KillRecordList.Add(killMessage.killRecord);

            var killRecord = killMessage.killRecord;
            var attacker = AppStateManager.Instance.GetPlayerByShortId(killRecord.attackerId);
            if (attacker != null)
            {
                attacker.killNumber = killRecord.killNumber;
                attacker.continuousKillNumber = killRecord.continuousKillNumber;
                attacker.sp = killRecord.sp ?? attacker.sp;
            }
            var sufferer = AppStateManager.Instance.GetPlayerByShortId(killRecord.suffererId);
            if (sufferer != null)
            {
                sufferer.deadTime = killRecord.killDate;
                sufferer.deathNumber = killRecord.deathNumber;
                sufferer.reviveTime = killRecord.reviveTime;
                sufferer.blood = 0;
                if (sufferer.battleStatus != PlayerBattleStatus.Dead)
                {
                    sufferer.battleStatus = PlayerBattleStatus.Dead;
                    //其他玩家死亡
                    EventManager.Instance.Broadcast(EventMsg.OtherPlayerDead, new PlayerChangeMessage()
                    {
                        isRealTime = true,
                        player = sufferer
                    });
                }
            }
        }

        private void PopulateFenceMessage(Fence fenceMsg)
        {
            
            Debug.LogWarning("MqttConnector PopulateFenceMessage 收到安全区数据，暂不处理! fenceMsg=" + JsonHelper.SerializeObject(fenceMsg));

        }

        private void PopulateSufferInfo(SufferRecord record)
        {
            var attacker = AppStateManager.Instance.GetPlayerByShortId(record.attackerId);
            if (attacker != null)
            {
                
            }
            var sufferer = AppStateManager.Instance.GetPlayerByShortId(record.suffererId);
            if (sufferer != null)
            {
                sufferer.playerShieldsValue = record.suffererShieldValue;
                Debug.Log(sufferer.name + "护盾值：" + sufferer.playerShieldsValue);
                sufferer.playerArmorValue = record.suffererArmorValue;
                sufferer.blood = record.suffererBlood;
                //被攻击方子弹?
                // sufferer.bullet = record.bullet;
                //对敌伤害值, 为正值则是伤害，为负数则是加血
                var attackerHarm = record.attackerHarm;
            }
        }

        private void PopulateAttackInfo(AttackRecord record)
        {
            var attacker = AppStateManager.Instance.GetPlayerByShortId(record.attackerId);
            if (attacker != null)
            {
                attacker.playerShieldsValue = record.attackerShieldValue;
                attacker.playerArmorValue = record.attackerArmorValue;
                attacker.blood = record.attackerBlood;
            }
            var sufferer = AppStateManager.Instance.GetPlayerByShortId(record.suffererId);
            if (sufferer != null)
            {
                sufferer.playerShieldsValue = record.suffererShieldValue;
                sufferer.playerArmorValue = record.suffererArmorValue;
                sufferer.blood = record.suffererBlood;
            }
        }

        private void PopulatePingMessage(PingMessage pingMsg)
        {
            if (pingMsg == null) return;
            
            //记录返回的ping索引
            if (pingMsg.index != null)
            {
                foreach (var pi in pingIndexCache.Where(pi => pi.index == pingMsg.index))
                {
                    pi.recv = DateTime.Now;
                    break;
                }
            }
            
            //判断游戏状态改变
            var play = AppStateManager.Instance.Play;
            
            if (play != null && pingMsg.playStatus == null && AppStateManager.Instance.Play.playStatus == GameState.FIGHTING)
            {
                pingMsg.playStatus = GameState.END;
                Debug.LogError("MqttConnector PopulatePingMessage 游戏已结束，但未收到消息！PlayStatus is null, set to END");
            }
            
            if (play == null && pingMsg.playStatus != null)
            {
                Debug.LogWarning("MqttConnector PopulatePingMessage Play为空，但是发现 PlayStatus=" + pingMsg.playStatus);
            }
            else if (play != null && pingMsg.playStatus != null && play.playStatus != pingMsg.playStatus)
            {

                if (pingMsg.playStatus == GameState.END)
                {
                    //游戏结算必须获取到结算数据才算成功
                    UniTask.Post(async () =>
                    {
                        //延迟1秒钟，防止END消息比比赛结算数据还早
                        await UniTask.WaitForSeconds(1);
                        //延迟1秒钟再获取结算数据
                        HttpConnector.Instance.PlayGetPlaySummary(play.id, res =>
                        {
                            if (res.success && res.data != null)
                            {
                                AppStateManager.Instance.Play = res.data.play;
                                AppStateManager.Instance.PlaySummaryDetail = res.data;
                                AppStateManager.Instance.TeamKillList = res.data.teams;
                                AppStateManager.Instance.TeamList = res.data.teams;
                                EventManager.Instance.Broadcast(EventMsg.MqttGameStateChanged, res.data.play.playStatus);
                            }
                            else
                            {
                                Debug.LogError("MqttConnector PopulatePlayDetail 获取结算数据失败:" + res.msg + " 等待下次心跳刷新!!");
                            }
                        });
                    });
                }
                else
                {
                    //由于消息丢失，发现场次状态发生改变。刷新play数据
                    HttpConnector.Instance.PlayGetPlayInfo(play.id, resp =>
                    {
                        if (resp.success)
                        {
                            //刷新场次数据
                            play = resp.data.play;
                            AppStateManager.Instance.Play = resp.data.play;
                            AppStateManager.Instance.PlayerList = resp.data.players;
                            AppStateManager.Instance.TeamKillList = resp.data.teams;
                            AppStateManager.Instance.TeamList = resp.data.teams;
                        }
                        else
                        {
                            play.playStatus = pingMsg.playStatus.Value;
                        }
                        Debug.Log("MqttConnector PopulatePingMessage PlayStatus=" + play.playStatus);
                        EventManager.Instance.Broadcast(EventMsg.MqttGameStateChanged, play.playStatus);
                    });
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  LORA 上行（设备 → 服务器）
        // ════════════════════════════════════════════════════════════════════

        private bool _loraUplinkWired;
        private LoraSerialBridge _loraBridge;

        /// <summary>
        /// 挂载串口收帧到 MQTT 上行的转发（幂等，整个生命周期只挂一次）。
        /// </summary>
        private void WireLoraUplinkOnce()
        {
            if (_loraUplinkWired) return;
            _loraUplinkWired = true;
            _loraBridge = LoraSerialBridge.Instance;
            _loraBridge.OnFrameReadyForMqtt += PublishLoraUplink;
            Debug.Log("MqttConnector 已挂载 LORA 上行转发 (串口收帧 -> MQTT)");
        }

        /// <summary>
        /// 串口收到完整 Lora 帧后，按上行协议发布到服务器。
        /// 上行 topic 取 <see cref="DeviceInfo5S.GetSubscribeTopic"/>（字段名为服务器视角，
        /// 实际即设备上行 topic <c>.../android/get</c>）；消息体 <c>{"type":"lora","value":"&lt;hex&gt;"}</c>。
        /// </summary>
        private void PublishLoraUplink(string hexFrame)
        {
            //if (string.IsNullOrWhiteSpace(hexFrame)) return;

            //var device = AppStateManager.Instance.DeviceInfo5S;
            //if (device == null)
            //{
            //    Debug.LogWarning("[MqttConnector] LORA 上行失败: DeviceInfo5S 为空, hex=" + hexFrame);
            //    return;
            //}

            //var topic = device.GetSubscribeTopic();
            //var json = JsonHelper.SerializeObject(new ScoreCardLoraUplinkMessage(hexFrame));
            //Debug.Log("[ScoreCard] lora 上行: topic=" + topic + " json=" + json);
            //Publish(topic, json);
        }

        /// <summary>
        /// 发送消息到玩家主题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="shortId"></param>
        /// <param name="msg"></param>
        public void SendMessageToPlayerTopic<T>(int shortId, MqttSendMessage<T> msg)
        {
            try
            {
                var playerTopic = "/glsa/player/" + AppStateManager.Instance.Play.id + "/" + shortId;
                
                Profiler.BeginSample($"SendMessageToPlayerTopic_JsonSerialize_{shortId}");
                var json = JsonHelper.SerializeObject(msg);
                Profiler.EndSample();
                
                Publish(playerTopic, json);
            }
            catch (Exception e)
            {
                Debug.LogError("MqttConnector 消息发送失败:" + e);
            }
        }
    }
}