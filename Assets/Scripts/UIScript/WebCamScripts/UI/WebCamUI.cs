using System;
using System.Threading.Tasks;
using Cinemachine;
using Cysharp.Threading.Tasks;
using LetsPiu.GameLogic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.WebCamScripts.UI
{
    public class WebCamUI : MonoBehaviour
    {

        private RawImage webCamRawImage;
        /// <summary>
        /// 摄像头点击事件
        /// </summary>
        public event Action<RawImage> OnWebCamClickEvent;

        void Start()
        {
            webCamRawImage = GetComponentInChildren<RawImage>();
        }

        /// <summary>
        /// 调整相机翻转
        /// </summary>
        public void FlipCamera()
        {
            Vector3 scale = Vector3.one;
#if UNITY_ANDROID
            // 检查屏幕方向是否为横屏 Right，旋转相机对象180度
            webCamRawImage.transform.rotation = Screen.orientation == ScreenOrientation.LandscapeRight ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                scale = new Vector3(-1, 1, -1);
            }
#elif UNITY_IOS
            RectTransform rectTransform = webCamRawImage.rectTransform;
            Quaternion rotation = Quaternion.Euler(180.0f, 0.0f, 0.0f);
            scale = new Vector3(1, 1, -1);
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                rotation = Quaternion.Euler(180.0f, 0.0f, 180.0f);
                scale = new Vector3(-1, 1, 1);
            }
            rectTransform.localRotation = rotation;
#endif
        }

        public void OnWebCamClick()
        {
            OnWebCamClickEvent?.Invoke(webCamRawImage);
        }

        /// <summary>
        /// 等比例缩放摄像头webCamRawImage，保证摄像头画布显示完全等比例覆盖屏幕。
        /// </summary>
        /// <returns></returns>
        public async UniTask OnWebCamReady(WebCamTexture webCamTexture)
        {
            // 检查必要的组件
            if (webCamRawImage == null)
            {
                Debug.LogError("WebCamLogic WebCamUI: webCamRawImage 为空，无法设置摄像头纹理");
                return;
            }

            if (webCamTexture == null)
            {
                Debug.LogError("WebCamLogic WebCamUI: webCamTexture 为空");
                return;
            }

            webCamRawImage.canvasRenderer.SetTexture(webCamTexture);

            float time = Time.time;
            while (!webCamTexture.didUpdateThisFrame)
            {
                await UniTask.WaitForSeconds(0.1f);
                if (Time.time - time > 5)
                {
                    Debug.LogError("WebCamLogic webCamTexture 更新超时了!!");
                }
            }
            SetPlaneCameraShow(webCamTexture).Forget();

            var aspectRatio = webCamTexture.width / (float)webCamTexture.height;
            var aspectRatioObject = webCamRawImage.GetComponent<AspectRatioFitter>();
            aspectRatioObject.aspectRatio = aspectRatio;
            Debug.Log("WebCamLogic WebCamUI: 摄像头画面显示比例:" + aspectRatio);
        }

        async UniTask SetPlaneCameraShow(WebCamTexture webCamTexture)
        {
            System.Threading.CancellationTokenSource timeoutToken = new System.Threading.CancellationTokenSource();
            timeoutToken.CancelAfterSlim(2000);
        }
    }
}
