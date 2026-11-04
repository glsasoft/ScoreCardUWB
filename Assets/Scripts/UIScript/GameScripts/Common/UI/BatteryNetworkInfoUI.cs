using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define;
using LetsPiu.Base.BaseScripts.Tool;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.GameScripts.Common.UI
{
    public class BatteryNetworkInfoUI : MonoBehaviour
    {
        public SpriteAtlas uiAtlas;

        [SerializeField] Image networkLevelImage;
        [SerializeField] Image batteryLevelImage;
        [SerializeField] TMP_Text fpsText;
        
        private Coroutine updatTask;

        #region FPS
        private float deltaTime = 0.0f;
        private List<float> fpsRecords = new List<float>();
        private float tenSecondTimer = 0.0f;
        #endregion

        private void Awake()
        {
            Debug.Log("BatteryNetworkInfoUI Awake...");
            InvokeRepeating(nameof(RefreshFPS), 0f, 0.5f);
        }

        private void OnDisable()
        {
            Debug.Log("BatteryNetworkInfoUI OnDisable...");
            if (updatTask != null)
            {
                StopCoroutine(updatTask);
                updatTask = null;
            }
        }

        private void OnEnable()
        {
            Debug.Log("BatteryNetworkInfoUI OnEnable...");
            updatTask = StartCoroutine(UpdateTask());
        }

        /// <summary>
        /// 更新电量
        /// </summary>
        private void UpdateBatteryState()
        {
            var batteryLevel = SystemInfo.batteryLevel<0? 1 : SystemInfo.batteryLevel;
            batteryLevelImage.fillAmount = batteryLevel;
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            float currentFPS = 1.0f / deltaTime;

            fpsRecords.Add(currentFPS);

            tenSecondTimer += Time.unscaledDeltaTime;

            while (tenSecondTimer > 0.5f && fpsRecords.Count > 0)
            {
                tenSecondTimer -= 0.5f / fpsRecords.Count;
                fpsRecords.RemoveAt(0);
            }
        }

        private void RefreshFPS()
        {
            if (fpsText != null)
            {
                float fps = 1.0f / deltaTime;
                float averageFPS = 0f;
                if (fpsRecords.Count > 0)
                {
                    foreach (float record in fpsRecords)
                    {
                        averageFPS += record;
                    }
                    averageFPS /= fpsRecords.Count;
                }
                // Debug.Log("BatteryNetworkInfoUI averageFPS="+averageFPS);
                fpsText.text = $"{averageFPS:F0}";
            }
            else
            {
                Debug.LogError("BatteryNetworkInfoUI RefreshFPS null...");
            }
        }

        private IEnumerator UpdateTask()
        {
            while (true)
            {
                UpdateNetworkState().Forget();
                UpdateBatteryState();
                yield return new WaitForSeconds(3f);
            }
        }

        private async UniTask UpdateNetworkState()
        {
            // Debug.Log("BatteryNetworkInfoUI UpdateNetworkState...");
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //无网络
                networkLevelImage.sprite = uiAtlas.GetSprite("wifiB");
                Debug.LogWarning("BatteryNetworkInfoUI 无网络状态");
            }
            else
            {
                string netAddress = AppStateManager.Instance.DeviceInfo5S?.GetUri();
                if (netAddress == null)
                {
                    networkLevelImage.sprite = uiAtlas.GetSprite("wifiB");
                    return;
                }
                var uri = new Uri(netAddress);
                var hostName = uri.Host;
                var port = uri.Port;

                // Debug.Log("BatteryNetworkInfoUI 主机名: " + hostName+" 端口:"+port);

                var pingReply = await DomainPing.PingDomainAsync(hostName);

                if (pingReply is { Success: true })
                {
                    if (networkLevelImage != null)
                    {
                        try
                        {
                            networkLevelImage.sprite = pingReply.RoundTripTime switch
                            {
                                > 100 => uiAtlas.GetSprite("wifi1"),
                                < 100 and > 60 => uiAtlas.GetSprite("wifi2"),
                                _ => uiAtlas.GetSprite("wifi3")
                            };   
                        }catch (Exception e)
                        {
                            Debug.LogError($"[{nameof(BatteryNetworkInfoUI)}] {nameof(UpdateNetworkState)} Error: {e.Message}");
                        }
                    }
                }
                else
                {
                    networkLevelImage.sprite = uiAtlas.GetSprite("wifiB");   
                }
            }
        }

        public void OnSettingButtonClicked()
        {
            //EventManager.Instance.Broadcast(EventMsg.GameSettingButtonClicked);
            UIModule.S.OpenPanel(UIDefine.UIID.SettingPanel);
        }
    }
}
