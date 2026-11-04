using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LetsPiu.UI.UIScript.WebViewScripts;
using UnityEngine;
using ZXing;

namespace LetsPiu.UI.UIScript.WebCamScripts.Logic
{
    class FrameData
    {
        public Color32[] data;
        public int width;
        public int height;
        public FrameData(Color32[] data, int width, int height)
        { this.data = data; this.width = width; this.height = height; }
    }

    /// <summary>
    /// 二维码扫描
    /// </summary>
    public class QrCodeScanWebCamLogic : MonoBehaviour
    {
        private CancellationTokenSource _cancellationTokenSource;
        private WebCamLogic webCamLogic;
        private FrameData frameData;
        private BarcodeReader barcodeReader;
        
        [SerializeField] private ScanQRCodeBridgeWebView scanQRCodeBridgeWebView;
    
        /// <summary>
        /// 扫描到二维码
        /// </summary>
        public event Action<Result> ScanResultEvent;

        private void Start()
        {
            webCamLogic = GetComponentInChildren<WebCamLogic>();
            webCamLogic.FrameDataEvent += t =>
            {
                frameData = new FrameData(t.GetPixels32(), t.width, t.height);
            };
            barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options =
                {
                    TryHarder = true,
                    TryInverted = true,
                    PossibleFormats = new[] {BarcodeFormat.QR_CODE}
                }
            };
            StartScanCode();
            
            
        }

        private void OnEnable()
        {
            scanQRCodeBridgeWebView.QRCodeScanResultEvent += OnWebViewScanResult;
        }
        
        private void OnDisable()
        {
            scanQRCodeBridgeWebView.QRCodeScanResultEvent -= OnWebViewScanResult;
        }
        
        private void OnWebViewScanResult(string qrCode)
        {
            Debug.Log("QrCodeScanWebCamLogic OnWebViewScanResult WebView扫描结果:"+qrCode);
            if (!string.IsNullOrEmpty(qrCode))
            {
                var result = new Result(qrCode, null, null, BarcodeFormat.QR_CODE);
                ScanResultEvent?.Invoke(result);
            }
            else
            {
                ScanResultEvent?.Invoke(null);
            }
        }

        private void BackgroundTask(CancellationToken cancellationToken)
        {
            Debug.Log("QrCodeScanWebCam BackgroundTask启动...");
            while (true)
            {
                Thread.Sleep(200);
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("QrCodeScanWebCam 结束扫码");
                    // ToastPanel.Show("用户取消扫码");
                    return;
                }

                if (frameData == null) continue;
                
                // Debug.Log("QrCodeScanWebCam 正在扫码...");
                
                //扫描二维码
                //获取摄像机画面的像素颜色数组信息
                //获取图片中的二维码信息
                var result = barcodeReader.Decode(frameData.data, frameData.width, frameData.height);

                if (result == null) continue;
                
                // 通知 Unity 主线程
                UniTask.Post(() =>
                {
                    Debug.Log("QrCodeScanWebCam 扫描结果:"+result.Text);
                    ScanResultEvent?.Invoke(result);
                    frameData = null;
                });
                StopScanCode();
                return;
            }
        }
        
        /// <summary>
        /// 使用WebView扫描图片中的二维码
        /// </summary>
        /// <param name="url"></param>
        public void ScanImageFromUrl(string url)
        {
            Debug.Log("QrCodeScanWebCam ScanImageFromUrl() url:"+url);
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("ScanImageFromUrl 传入url为空");
                return;
            }
            if (scanQRCodeBridgeWebView == null)
            {
                Debug.LogWarning("ScanImageFromUrl scanQRCodeBridgeWebView为空");
                return;
            }
            scanQRCodeBridgeWebView.ScanImageFromUrl(url);
        }
        
        private void OnDestroy()
        {
            StopScanCode();
        }

        public void StartScanCode()
        {
            Debug.Log("QrCodeScanWebCam 启动BackgroundTask...");
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => BackgroundTask(_cancellationTokenSource.Token));
        }

        public void StopScanCode()
        {
            Debug.Log("QrCodeScanWebCam 结束扫码...");
            frameData = null;
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
}