using System;
using Cysharp.Threading.Tasks;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI.UIScript.WebCamScripts.UI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Plugins;
using Plugins.CameraFocus;

namespace LetsPiu.UI.UIScript.WebCamScripts.Logic
{
    public class WebCamLogic : MonoBehaviour
    {
        private WebCamTexture webCamTexture;
        private bool isWebCamPlaying = false;
        private WebCamUI webCamUI;
        private ScreenOrientation lastOrientation;
        /// <summary>
        /// 摄像头帧数据回调
        /// </summary>
        public event Action<WebCamTexture> FrameDataEvent;
        /// <summary>
        /// 用户点击了相机
        /// </summary>
        public event Action WebCamClickEvent;

        public enum StartWebCamResult
        {
            Success,
            NoPermission,
            NoWebCamDevice,
        }

        private void InitWebCam()
        {
            if(webCamUI == null) webCamUI = GetComponent<WebCamUI>();
            webCamUI.OnWebCamClickEvent += HandleWebCamClick;
            lastOrientation = Screen.orientation;
            StartWebCam(result =>
            {
                if(Application.HasUserAuthorization(UserAuthorization.WebCam))
                {
                    Debug.Log("WebCamLogic 摄像头权限申请成功!");
                }
                else
                {
                    Debug.Log("WebCamLogic 摄像头权限申请失败!");
                }
                Debug.Log("WebCamLogic 相机启动结果:"+result);
            });
        }

        private void Update()
        {
            // 检查屏幕方向变化
            if (Screen.orientation != lastOrientation)
            {
                lastOrientation = Screen.orientation;
                if (isWebCamPlaying)
                {
                    webCamUI.FlipCamera();
                    Debug.Log("WebCamLogic 屏幕方向变化，已调用 FlipCamera");
                }
            }
            // 检查是否有新帧
            if (isWebCamPlaying && webCamTexture.didUpdateThisFrame)
            {
                FrameDataEvent?.Invoke(webCamTexture);
            }
        }
        
        /// <summary>
        /// 点击时自动对焦
        /// </summary>
        private void HandleWebCamClick(RawImage webCamRawImage)
        {
            WebCamClickEvent?.Invoke();
            if (webCamRawImage == null) return;
            var canvas = webCamRawImage.canvas;
            if (canvas == null) return;

            // 转换到 RawImage 的局部坐标
            var rawImageRectTransform = webCamRawImage.GetComponent<RectTransform>();
            if (rawImageRectTransform == null) return;
        
            //输出RawImage的大小
            var rect = rawImageRectTransform.rect;
            var rectPos = rect.position;
            var rectSize = rect.size;
            Debug.Log("大小 rect.pos:"+rectPos+" rectSize="+rectSize);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRectTransform, Input.mousePosition, canvas.worldCamera, out var localPosition))
            {
                //这是实际上相机显示区域的点击坐标
                var rawX = localPosition.x + rectSize.x / 2;
                var rawY = localPosition.y + rectSize.y / 2;
                Debug.Log("UI上点击的坐标:"+rawX+"x"+rawY);
                //这是实际相机的坐标
                var scale = webCamTexture.width / rectSize.x;
                var realX = scale * rawX;
                var realY = scale * rawY;
                Debug.Log("相机上点击的坐标:"+realX+"x"+realY);
                //API说明: https://docs.unity.cn/cn/2019.4/ScriptReference/WebCamTexture-autoFocusPoint.html
                var realXPercent = realX / webCamTexture.width;
                var realYPercent = realY / webCamTexture.height;
                Debug.Log("相机上点击autoFocusPoint:("+realXPercent+","+realYPercent+")");
                webCamTexture.autoFocusPoint = new Vector2(realXPercent, realYPercent);
            }
            else
            {
                Debug.Log("Click was outside the RawImage.");
            }
        }
    
        /// <summary>
        /// 启动摄像头
        /// </summary>
        public async UniTask<StartWebCamResult> StartWebCamAsync()
        {
            StopWebCam();
            
            Debug.Log("WebCamLogic 启动相机...");
            
            if (!await AppUtils.RequestWebCamPermissionAsync())
            {
                Debug.Log("WebCamLogic 启动相机失败，没有摄像头权限!");
                return StartWebCamResult.NoPermission;
            }
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.Log("WebCamLogic 启动相机失败，没有找到摄像头设备!");
                return StartWebCamResult.NoWebCamDevice;
            }
            // foreach (var webCamDevice in devices)
            // {
            //     Debug.Log("WebCamLogic 相机属性: name="+webCamDevice.name+" 类型:"+webCamDevice.kind+" 自动对焦:"+webCamDevice.isAutoFocusPointSupported+" 是否前置:"+webCamDevice.isFrontFacing+" 分辨率:"+JsonConvert.SerializeObject(webCamDevice.availableResolutions));
            //     //循环输出摄像头支持的分辨率
            //     if(webCamDevice.availableResolutions == null) continue;
            //     foreach (var resolution in webCamDevice.availableResolutions)
            //     {
            //         Debug.Log("WebCamLogic 分辨率: " + resolution.width + "x" + resolution.height);
            //     }
            // }

            var deviceName = devices[0].name;
#if UNITY_IOS
            // 选择摄像头（后置或前置）
            CameraFocus.SwitchCamera(1); // 0表示后置摄像头，1表示后置超广角相机
            foreach (var device in devices)
            {
                Debug.Log("WebCamLogic 设备: " + device.name);
                if (device.name=="后置超广角相机")
                {
                    deviceName = device.name;
                }
            }
            Debug.Log("WebCamLogic 选择摄像头: " + deviceName);
            if (string.IsNullOrEmpty(deviceName))
            {
                deviceName = devices[0].name; // 使用第一个可用设备
            }
#endif
            Debug.Log("WebCamLogic 创建WebCamTexture...");
            webCamTexture = new WebCamTexture(deviceName, 1920, 1080)
            {
                wrapMode = TextureWrapMode.Repeat
            };
            webCamTexture.Play();
            await webCamUI.OnWebCamReady(webCamTexture);
            isWebCamPlaying = true;
            webCamUI.FlipCamera();
            return StartWebCamResult.Success;
        }
    
        /// <summary>
        /// 启动相机
        /// </summary>
        /// <param name="callback"></param>
        public void StartWebCam(Action<StartWebCamResult> callback)
        {
            StartCoroutine(StartWebCamAsync().ToCoroutine(callback));
        }

        public void StopWebCam()
        {
            Debug.Log("WebCamLogic 停止相机...");
            if(webCamTexture != null) webCamTexture.Stop();
            webCamTexture = null;
            isWebCamPlaying = false;
        }
        
        public void PauseWebCam()
        {
            if (webCamTexture != null && isWebCamPlaying)
            {
                webCamTexture.Pause();
                Debug.Log("WebCamLogic 暂停相机");
            }
        }
        
        public void ResumeWebCam()
        {
            if (webCamTexture != null && isWebCamPlaying)
            {
                webCamTexture.Play();
                Debug.Log("WebCamLogic 恢复相机");
            }
        }

        private void OnDisable()
        {
            StopWebCam();
        }

        private void OnEnable()
        {
            InitWebCam();
        }

        /// <summary>
        /// 应用暂停的时候暂停camera，恢复的时候继续使用
        /// </summary>
        /// <param name="pause"></param>
        private void OnApplicationPause(bool pause)
        {
            Debug.Log("WebCamLogic OnApplicationPause pause="+pause);
            if (webCamTexture == null) return;
            if (pause)
            {
                webCamTexture.Pause();
                isWebCamPlaying = false;
            }
            else
            {
                webCamTexture.Play();
                isWebCamPlaying = true;
            }
        }

        private void OnDestroy()
        {
            StopWebCam();
        }
        
        
    }
}
