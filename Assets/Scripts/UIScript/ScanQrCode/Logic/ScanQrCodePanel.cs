using System;
using System.IO;
using Cysharp.Threading.Tasks;
using LetsPiu.Common.UI;
using LetsPiu.Define;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI.UIScript.Common.UI;
using LetsPiu.UI.UIScript.ScanQrCode.UI;
using LetsPiu.UI.UIScript.WebCamScripts.Logic;
using LetsPiu.UIDefine;
using UnityEngine;
using ZXing;

namespace LetsPiu.UI.UIScript.ScanQrCode.Logic
{
    public class ScanQrCodePanel : AbstractPanel
    {
        [SerializeField] private ScanQrCodePanelUI scanQrCodePanelUI;
        [SerializeField] private QrCodeScanWebCamLogic qrCodeScanWebCamLogic;

        public event Action<Result> ScanCompleteEvent;
        private bool isFlashOn = false;

        /// <summary>
        /// 当前扫码类型
        /// </summary>
        public ScanCodeType scanCodeType;

        public override void OnCreate()
        {
            base.OnCreate();
            qrCodeScanWebCamLogic.ScanResultEvent += HandleScanResult;
            scanQrCodePanelUI.BackPressedEvent += OnBackPressed;
            scanQrCodePanelUI.ToggleFlashEvent += ToggleFlash;
            scanQrCodePanelUI.SelectAlbumEvent += SelectAlbum;
            scanQrCodePanelUI.ManualInputClickEvent += OnManualInputClick;
        }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            if (args.Length == 0 || args[0] is not ScanCodeType)
            {
                Debug.LogError("OfflineScanCodePanel 扫码类型参数错误!");
                CloseSelf();
                return;
            }
            scanCodeType = (ScanCodeType)args[0];
            Debug.Log("OfflineScanCodePanel 扫码界面打开 扫码类型:"+scanCodeType);
        }

        private void HandleScanResult(Result scanResult)
        {
            ScanCompleteEvent?.Invoke(scanResult);
            //扫描成功自动关闭
            base.CloseSelf();
        }

        /// <summary>
        /// 用户点击了返回按钮，没有正常扫描
        /// </summary>
        private new void OnBackPressed()
        {
            Debug.Log("ScanQrCodePanel 点击了返回按钮");
            ScanCompleteEvent?.Invoke(null);
        }
        
        public void ToggleFlash()
        {
            Debug.Log("ScanQrCodePanel 切换手电筒状态 当前状态: " + (isFlashOn ? "开启" : "关闭"));
            isFlashOn = !isFlashOn;
#if UNITY_ANDROID && !UNITY_EDITOR
            //Android打开关闭手电筒
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    using var cameraManager = activity.Call<AndroidJavaObject>("getSystemService", "camera");
                    var cameraId = cameraManager.Call<string[]>("getCameraIdList")[0];
                    cameraManager.Call("setTorchMode", cameraId, isFlashOn);
                }
                Debug.Log("ScanQrCodePanel 手电筒控制成功: " + (isFlashOn ? "开启" : "关闭"));
            }
            catch (Exception e)
            {
                Debug.LogError("ScanQrCodePanel 手电筒控制失败: " + e.Message);
            }
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS代码打开关闭手电筒,调用CameraFocus.cs的以下两个方法
            
#endif
        }
        
        /// <summary>
        /// 从相册选择并扫描
        /// </summary>
        public void SelectAlbum()
        {
            UniTask.Post(async () =>
            {
                //扫描二维码时，读取最大分辨率图片，防止无法识别
                await ImagePicker.Instance.PickImagePathAsync(resp =>
                {
                    if (resp.path == null)
                    {
                        Debug.LogError("OfflineScanCodePanel 相册选择失败!");
                        return;
                    }
                    
                    //将图片复制到Application.temporaryCachePath
                    Debug.Log("ScanQrCodePanel 选择相册图片路径: " + resp.path);

                    var sourcePath = resp.path;
                    var cacheDir = Application.temporaryCachePath;
                    var extension = Path.GetExtension(sourcePath);
                    if (string.IsNullOrEmpty(extension)) extension = ".jpg"; // 兜底扩展名
                    var destFileName = "picked_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + extension;
                    var destPath = Path.Combine(cacheDir, destFileName);
                    
                    //如果目标文件已存在，删除
                    if (File.Exists(destPath))
                    {
                        try
                        {
                            File.Delete(destPath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("ScanQrCodePanel 删除已存在目标文件失败: " + e.Message);
                        }
                    }

                    var finalPathForScan = sourcePath; // 默认回退
                    try
                    {
                        File.Copy(sourcePath, destPath, true);
                        finalPathForScan = destPath;
                        Debug.Log($"ScanQrCodePanel 已复制图片到缓存: {destPath}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("ScanQrCodePanel 复制到缓存失败, 使用原路径扫描: " + e.Message);
                    }
                    Debug.Log("ScanQrCodePanel 最终用于扫描的图片路径: " + finalPathForScan);
                    //这里只取文件名，不取路径
                    var fileNameOnly = Path.GetFileName(finalPathForScan);
                    Debug.Log("ScanQrCodePanel 最终用于扫描的图片文件名: " + fileNameOnly);
                    qrCodeScanWebCamLogic.ScanImageFromUrl("./"+fileNameOnly);
                });
            });
        }

        private void OnManualInputClick()
        {
           
        }

        public void SetTitleText(string title)
        {
            scanQrCodePanelUI.titleText.text = title ?? "";
        }
    }
}
