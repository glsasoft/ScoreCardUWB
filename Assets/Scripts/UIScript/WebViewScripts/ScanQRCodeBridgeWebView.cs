using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LetsPiu.UI.UIScript.WebViewScripts
{
    public class ScanQRCodeBridgeWebView : MonoBehaviour
    {
        [SerializeField] CompactWebView compactWebView;
        public event Action WebViewLoadEvent;
        
        /// <summary>
        /// 等待扫描的图片路径
        /// </summary>
        private string imagePathToScan = null;

        /// <summary>
        /// 点击了切换头像按钮事件
        /// </summary>
        public event Action<string> QRCodeScanResultEvent;

        private void Awake()
        {
            compactWebView = GetComponentInChildren<CompactWebView>();
            compactWebView.PageLoadEvent += () =>
            {
                Debug.Log("ScanQRCodeBridgeWebView PageLoadEvent...");

                if (imagePathToScan != null)
                {
                    Debug.Log("ScanQRCodeBridgeWebView PageLoadEvent 触发等待扫描图片:"+imagePathToScan);
                    ScanImageFromUrl(imagePathToScan);
                }
            };
            
            compactWebView.DataEvent += data =>
            {
                Debug.Log("ScanQRCodeBridgeWebView 接收到消息: type="+data.Type+" data="+JsonConvert.SerializeObject(data));
                if (data.Type == JTokenType.Object)
                {
                    var obj = data.Value<JObject>();
                    var dataType = obj["dataType"]?.Value<string>();
                    
                    if (dataType == "qrCode")
                    {
                        var qrCode = obj["data"]?.Value<string>();
                        QRCodeScanResultEvent?.Invoke(qrCode);
                    }
                }
                Debug.LogWarning("ScanQRCodeBridgeWebView OnMessageReceived 未知消息! "+data);
            };
        }

        private void Start()
        {
            Debug.Log("ScanQRCodeBridgeWebView 开始时加载...");
            UniTask.Post(async () =>
            {
                var resLoader = new ResLoader();
                resLoader.AddRes("ScanQRCodeBridge", ResType.Text);
                await resLoader.StartLoad();
                var handle = resLoader.GetAsset("ScanQRCodeBridge");
                var textAsset = handle.AssetObject as TextAsset;
                var html = textAsset.text;
                handle.Dispose();
                compactWebView.LoadContent(html);
                //unitWebview设置背景透明
                compactWebView.SetBackgroundColor(Color.clear);
            });
        }
        
        /// <summary>
        /// 调用js扫描二维码
        /// </summary>
        /// <param name="imagePath"></param>
        public void ScanImageFromUrl(string imagePath)
        {
            imagePathToScan = imagePath;
            if (!IsPageLoaded())
            {
                Debug.LogWarning("ScanQRCodeBridgeWebView ScanImageFromUrl() 页面未加载完成!");
                return;
            }

            imagePathToScan = null;
            
            var js = $@"
                scanImageFromUrl('{imagePath}')
            ";
            Debug.Log("ScanQRCodeBridgeWebView ScanImageFromUrl() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("ScanQRCodeBridgeWebView ScanImageFromUrl() 返回:"+result);
            });
        }

        public CompactWebView GetCompactWebView()
        {
            return compactWebView;
        }

        public bool IsPageLoaded()
        {
            return GetCompactWebView() != null && GetCompactWebView().IsPageLoaded();
        }
    }
}
