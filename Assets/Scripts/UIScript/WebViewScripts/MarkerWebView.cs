using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LetsPiu.Base.BaseScripts.Define;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI.UIScript.WebViewScripts.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using YooAsset;

namespace LetsPiu.UI.UIScript.WebViewScripts
{
    public class MarkerWebView : MonoBehaviour
    {
        private CompactWebView compactWebView;
        public event Action WebViewLoadEvent;
        private readonly int TimeoutMilliseconds = 500;

        /// <summary>
        /// images是否已经写入到
        /// </summary>
        private static bool isImageWriteSuccess = false;
        
        /// <summary>
        /// 点击了切换头像按钮事件
        /// </summary>
        public event Action SwitchHeadButtonClickEvent;

        private void Awake()
        {
            compactWebView = GetComponentInChildren<CompactWebView>();
            compactWebView.PageLoadEvent += () =>
            {
                Debug.Log("MarkerWebView PageLoadEvent 写入图片...");
                UniTask.Post(async () =>
                {
                    if (!isImageWriteSuccess)
                    {
                        isImageWriteSuccess = await WriteImages();
                    }
                    
                    WebViewLoadEvent?.Invoke();
                    Debug.Log("MarkerWebView 页面加载完成!");
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
                    // AddZoomControl();
#else
#endif 
                });
            };
            
            compactWebView.DataEvent += data =>
            {
                Debug.Log("CreateElectronicFenceWebView 接收到消息: type="+data.Type+" data="+JsonConvert.SerializeObject(data));
                if (data.Type == JTokenType.Object)
                {
                    var obj = data.Value<JObject>();
                    var dataType = obj["dataType"]?.Value<string>();
                    var button = obj["button"]?.Value<string>();
                    if (dataType == "clickEvent" && button == "switchHead") 
                    {
                        SwitchHeadButtonClickEvent?.Invoke();
                        return;
                    }
                }
                Debug.LogWarning("CreateElectronicFenceWebView OnMessageReceived 未知消息! "+data);
            };
        }

        private void Start()
        {
            Debug.Log("MarkerWebView 开始时加载...");
            
            UniTask.Post(async () =>
            {
                var resLoader = new ResLoader();
                resLoader.AddRes("CustomMarkerMap", ResType.Text);
                await resLoader.StartLoad();
                var handle = resLoader.GetAsset("CustomMarkerMap");
                var textAsset = handle.AssetObject as TextAsset;
                var html = textAsset.text;
                handle.Dispose();
                compactWebView.LoadContent(html);
                //unitWebview设置背景透明，以实现圆形webview
                compactWebView.SetBackgroundColor(Color.clear);
            });
        }
        
        /// <summary>
        /// 设置地图中心点，以及缩放
        /// </summary>
        /// <param name="point"></param>
        /// <param name="zoom">3~19</param>
        public void SetMapCenterAndZoom(MapPoint point, int zoom = 18, [CanBeNull] Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView SetMapCenterAndZoom() 页面未加载完成，无法设置地图中心点和缩放!");
                return;
            }
            // Debug.Log("MarkerWebView SetMapCenterAndZoom: zoom="+zoom);
            var pointJson = JsonConvert.SerializeObject(point);
            var js = $@"
                _SetMapCenterAndZoom({pointJson}, {zoom})
            ";
            // Debug.Log("MarkerWebView SetMapCenterAndZoom() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                // Debug.Log("MarkerWebView SetMapCenterAndZoom() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 显示围栏(电子栅栏、毒圈)
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        public void ShowPolyHole(string markerName, PolyHoleParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowPolyHole() 页面未加载完成，无法显示多边形!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowPolyHole('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowPolyHole() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView ShowPolyHole() 返回:"+result);
            });
        }
        
        /// <summary>
        /// 显示折线
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        /// <param name="callback"></param>
        public async UniTask ShowPolyLineAsync(string markerName, PolyLineParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowPolyLine() 页面未加载完成，无法显示折线!");
                return;
            }
            var success = false;
            ShowPolyLine(markerName, @params, () => { success = true; });
            try
            {
                await UniTask.WaitUntil(() => success)
                    .Timeout(TimeSpan.FromMilliseconds(TimeoutMilliseconds));
            }
            catch (TimeoutException)
            {
                Debug.LogError("ShowPolyLine timed out!");
            }
        }
        
        /// <summary>
        /// 显示折线
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        /// <param name="callback"></param>
        public void ShowPolyLine(string markerName, PolyLineParams @params, Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowPolyLine() 页面未加载完成，无法显示折线!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowPolyline('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowPolyLine() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                callback?.Invoke();
                Debug.Log("MarkerWebView ShowPolyLine() 返回:"+result);
            });
        }

        public void ShowOrUpdateIconMarker(string markerName, IconMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowOrUpdateIconMarker() 页面未加载完成，无法显示图标Marker!");
                return;
            }
            MarkerExist(markerName, exist =>
            {
                if (exist)
                {
                    UpdateIconMarker(markerName, @params);
                }
                else
                {
                    ShowIconMarker(markerName, @params);
                }
            });
        }
        
        /// <summary>
        /// 显示图标Marker
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        public async UniTask ShowIconMarkerAsync(string markerName, IconMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowIconMarkerAsync() 页面未加载完成，无法显示图标Marker!");
                return;
            }
            var success = false;
            ShowIconMarker(markerName, @params, () => { success = true; });
            try
            {
                await UniTask.WaitUntil(() => success)
                    .Timeout(TimeSpan.FromMilliseconds(TimeoutMilliseconds));
            }
            catch (TimeoutException)
            {
                Debug.LogError("ShowIconMarker timed out!");
            }
        }
        
        /// <summary>
        /// 显示图标Marker
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        /// <param name="callback"></param>
        public void ShowIconMarker(string markerName, IconMarkerParams @params, Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowImageIconMarker() 页面未加载完成，无法显示图标Marker!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowImageIconMarker('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowImageIconMarker() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                callback?.Invoke();
                Debug.Log("MarkerWebView ShowImageIconMarker() 返回:"+result);
            });
        }
        
        /// <summary>
        /// 显示或更新带文本标签的标记点，自动判断是创建还是更新
        /// </summary>
        /// <param name="markerName">标记点名称</param>
        /// <param name="params">标签标记点参数</param>
        public void ShowOrUpdateLabeledMarker(string markerName, LabeledMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowOrUpdateLabeledMarker() 页面未加载完成，无法显示带文本标签的标记点!");
                return;
            }
            MarkerExist(markerName, exist =>
            {
                if (exist)
                {
                    UpdateLabeledMarker(markerName, @params);
                }
                else
                {
                    ShowLabeledMarker(markerName, @params);
                }
            });
        }

        /// <summary>
        /// 异步显示带文本标签的标记点
        /// </summary>
        /// <param name="markerName">标记点名称</param>
        /// <param name="params">标签标记点参数</param>
        public async UniTask ShowLabeledMarkerAsync(string markerName, LabeledMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowLabeledMarkerAsync() 页面未加载完成，无法显示带文本标签的标记点!");
                return;
            }
            var success = false;
            ShowLabeledMarker(markerName, @params, () => { success = true; });
            try
            {
                await UniTask.WaitUntil(() => success)
                    .Timeout(TimeSpan.FromMilliseconds(TimeoutMilliseconds));
            }
            catch (TimeoutException)
            {
                Debug.LogError("ShowLabeledMarker timed out!");
            }
        }

        /// <summary>
        /// 显示带文本标签的标记点
        /// </summary>
        /// <param name="markerName">标记点名称</param>
        /// <param name="params">标签标记点参数</param>
        /// <param name="callback">回调函数</param>
        public void ShowLabeledMarker(string markerName, LabeledMarkerParams @params, Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowLabeledMarker() 页面未加载完成，无法显示带文本标签的标记点!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _CreateLabeledMarker('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowLabeledMarker() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                callback?.Invoke();
                Debug.Log("MarkerWebView ShowLabeledMarker() 返回:"+result);
            });
        }

        /// <summary>
        /// 更新带文本标签的标记点
        /// </summary>
        /// <param name="markerName">标记点名称</param>
        /// <param name="params">要更新的参数</param>
        public void UpdateLabeledMarker(string markerName, LabeledMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView UpdateLabeledMarker() 页面未加载完成，无法更新带文本标签的标记点!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _UpdateLabeledMarker('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView UpdateLabeledMarker() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView UpdateLabeledMarker() 返回:"+result);
            });
        }
        
        /// <summary>
        /// 显示贴图渐显动画
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        public async UniTask ShowGroundFadeInAnimationAsync(string markerName, GroundFadeInAnimParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowGroundFadeInAnimationAsync() 页面未加载完成，无法显示贴图渐显动画!");
                return;
            }
            var success = false;
            ShowGroundFadeInAnimation(markerName, @params, () => { success = true; });
            try
            {
                await UniTask.WaitUntil(() => success)
                    .Timeout(TimeSpan.FromMilliseconds(TimeoutMilliseconds));
            }
            catch (TimeoutException)
            {
                Debug.LogError("ShowGroundFadeInAnimation timed out!");
            }
        }
        
        /// <summary>
        /// 更新贴图
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        public async UniTask UpdateGroundOverlayBoundsAsync(string markerName, GroundFadeInAnimParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView UpdateGroundOverlayBoundsAsync() 页面未加载完成，无法更新贴图!");
                return;
            }
            var success = false;
            UpdateGroundOverlayBounds(markerName, @params, () => { success = true; });
            try
            {
                await UniTask.WaitUntil(() => success)
                    .Timeout(TimeSpan.FromMilliseconds(TimeoutMilliseconds));
            }
            catch (TimeoutException)
            {
                Debug.LogError("ShowGroundFadeInAnimation timed out!");
            }
        }
        
        /// <summary>
        /// 更新贴图
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        /// <param name="callback"></param>
        public void UpdateGroundOverlayBounds(string markerName, GroundFadeInAnimParams @params, [CanBeNull] Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView UpdateGroundOverlayBounds() 页面未加载完成，无法更新贴图!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _UpdateGroundOverlayBounds('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView UpdateGroundOverlayBounds() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView UpdateGroundOverlayBounds() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 显示贴图渐显动画
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        /// <param name="callback"></param>
        public void ShowGroundFadeInAnimation(string markerName, GroundFadeInAnimParams @params, [CanBeNull] Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowGroundFadeInAnimation() 页面未加载完成，无法显示贴图渐显动画!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowGroundImageFadeInAnimation('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowGroundFadeInAnimation() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView ShowGroundFadeInAnimation() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 隐藏贴图渐显动画
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="callback"></param>
        public void HideGroundFadeInAnimation(string markerName, [CanBeNull] Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView HideGroundFadeInAnimation() 页面未加载完成，无法隐藏贴图渐显动画!");
                return;
            }
            var js = $@"
                _HideGroundImageFadeInAnimation('{markerName}')
            ";
            // Debug.Log("MarkerWebView HideGroundFadeInAnimation() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                // Debug.Log("MarkerWebView HideGroundFadeInAnimation() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 显示带边框的图像marker, 如果图像和大小不变，进更新位置和角度
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        public void ShowImageMarker(string markerName, ImageMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowImageMarker() 页面未加载完成，无法显示图像Marker!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowImageMarker('{markerName}', {param})
            ";
            // Debug.Log("MarkerWebView ShowImageMarker() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                // Debug.Log("MarkerWebView ShowImageMarker() 返回:"+result);
            });
        }

        public async UniTask<string> ShowImageMarkerSync(string markerName, ImageMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView OfflineGameMapUI ShowImageMarker() 页面未加载完成，无法显示图像Marker!");
                return null;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowImageMarker('{markerName}', {param})
            ";
            bool complete = false;
            string res = null;
            compactWebView.EvaluateJavaScript(js, result =>
            {
                complete = true;
                res = result;
            });
            try
            {
                await UniTask.WaitUntil(() => complete)
                    .Timeout(TimeSpan.FromSeconds(0.5)); // 0.5秒超时
            }
            catch (TimeoutException)
            {
                Debug.LogError("MarkerWebView OfflineGameMapUI ShowImageMarkerSync() 执行超时(1s)!");
                return null;
            }
            return res;
        }
        
        /// <summary>
        /// 隐藏图标Marker
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        public void UpdateIconMarker(string markerName, IconMarkerParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView UpdateImageIconMarker() 页面未加载完成，无法更新图标Marker!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _UpdateImageIconMarker('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView UpdateImageIconMarker() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView UpdateImageIconMarker() 返回:"+result);
            });
        }
            
        /// <summary>
        /// 显示地图贴图动画
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="params"></param>
        public void ShowGroundAnimation(string markerName, GroundAnimParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowGroundAnimation() 页面未加载完成，无法显示贴图动画!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowGroundImageAnimation('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowGroundAnimation() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView ShowGroundAnimation() 返回:"+result);
            });
        }
        
        /// <summary>
        /// 显示Marker图像帧动画
        /// </summary>
        public void ShowMarkerImageAnimation(string markerName, MarkerImageAnimParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowMarkerImageAnimation() 页面未加载完成，无法显示Marker图像动画!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowMarkerImageAnimation('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowMarkerImageAnimation() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView ShowMarkerImageAnimation() 返回:"+result);
            });
        }
        
        /// <summary>
        /// 显示或者更新Marker图像渐隐渐显动画
        /// </summary>
        public void ShowOrUpdateMarkerImageFadeInOutAnimation(string markerName, IconMarkerFadeInOutParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowOrUpdateMarkerImageFadeInOutAnimation() 页面未加载完成，无法显示Marker图像渐隐渐显动画!");
                return;
            }
            MarkerExist(markerName, exist =>
            {
                if (exist)
                {
                    UpdateMarkerPosition(markerName, @params.point, b => { });
                }
                else
                {
                    ShowMarkerImageFadeInOutAnimation(markerName, @params);
                }
            });
        }
        
        /// <summary>
        /// 显示Marker图像渐隐渐显动画
        /// </summary>
        public void ShowMarkerImageFadeInOutAnimation(string markerName, IconMarkerFadeInOutParams @params)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowMarkerImageFadeInOutAnimation() 页面未加载完成，无法显示Marker图像渐隐渐显动画!");
                return;
            }
            var param = JsonConvert.SerializeObject(@params);
            var js = $@"
                _ShowMarkerImageFadeInOutAnimation('{markerName}', {param})
            ";
            Debug.Log("MarkerWebView ShowMarkerImageFadeInOutAnimation() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView ShowMarkerImageFadeInOutAnimation() 返回:"+result);
            });
        }
            
        /// <summary>
        /// 删除覆盖物
        /// </summary>
        /// <param name="markerName"></param>
        public void RemoveMarker(string markerName, [CanBeNull] Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView RemoveMarker() 页面未加载完成，无法删除Marker!");
                return;
            }
            var js = $@"
                _RemoveMarker('{markerName}')
            ";
            Debug.Log("MarkerWebView RemoveMarker() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView RemoveMarker() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 添加缩放工具条
        /// </summary>
        public void AddZoomControl()
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView AddZoomControl() 页面未加载完成，无法添加缩放工具条!");
                return;
            }
            var js = $@"
                map.addControl(new MyBMap.NavigationControl());
            ";
            Debug.Log("MarkerWebView AddZoomControl() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView AddZoomControl() 返回:"+result);
            });
        }
        
        /// <summary>
        /// 获取地图中心点
        /// </summary>
        public void GetMapCenter(Action<MapPoint> callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView GetMapCenter() 页面未加载完成，无法获取地图中心点!");
                return;
            }
            var js = $@"map.getCenter()";
            Debug.Log("MarkerWebView GetMapCenter() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView GetMapCenter() 返回:"+result);
                callback?.Invoke(JsonConvert.DeserializeObject<MapPoint>(result));
            });
        }
        
        /// <summary>
        /// Marker是否存在
        /// </summary>
        public void MarkerExist(string markerName, Action<bool> callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView MarkerExist() 页面未加载完成，无法判断Marker是否存在!");
                return;
            }
            var js = $@"_MarkerExist('{markerName}')";
            // Debug.Log("MarkerWebView GetMapCenter() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                // Debug.Log("MarkerWebView MarkerExist() 返回:"+result);
                callback?.Invoke(result == "true");
            });
        }
        
        /// <summary>
        /// 获取Marker
        /// </summary>
        public void GetMarker(string markerName, Action<MarkerTypeInfo> callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView GetMarker() 页面未加载完成，无法获取Marker信息!");
                return;
            }
            var js = $@"_GetMarker('{markerName}')";
            // Debug.Log("MarkerWebView GetMarker() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView GetMarker() 返回:"+result);
                if (string.IsNullOrEmpty(result))
                {
                    callback?.Invoke(null);
                    return;
                }
                var jsonObj = JsonConvert.DeserializeObject<MarkerTypeInfo>(result);
                callback?.Invoke(jsonObj);
            });
        }
        
        /// <summary>
        /// 更新marker的位置
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="point"></param>
        public async UniTask UpdateMarkerPositionAsync(string markerName, MapPoint point)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView UpdateMarkerPositionAsync() 页面未加载完成，无法更新Marker位置!");
                return;
            }
            var success = false;
            UpdateMarkerPosition(markerName, point, b => { success = true; });
            try
            {
                await UniTask.WaitUntil(() => success)
                    .Timeout(TimeSpan.FromMilliseconds(TimeoutMilliseconds));
            }
            catch (TimeoutException)
            {
                Debug.LogError("ShowPolyLine timed out!");
            }
        }
        
        /// <summary>
        /// 更新marker的位置
        /// </summary>
        /// <param name="markerName"></param>
        /// <param name="point"></param>
        /// <param name="callback"></param>
        public void UpdateMarkerPosition(string markerName, MapPoint point, Action<bool> callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView UpdateMarkerPosition() 页面未加载完成，无法更新Marker位置!");
                return;
            }
            var pointJson = JsonConvert.SerializeObject(point);
            var js = $@"
                _UpdateMarkerPosition('{markerName}', {pointJson})
            ";
            // Debug.Log("MarkerWebView UpdateMarkerPosition() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                // Debug.Log("MarkerWebView UpdateMarkerPosition() 返回:"+result);
                callback?.Invoke(result == "true");
            });
        }
        
        /// <summary>
        /// 获取地图缩放级别
        /// </summary>
        public void GetMapZoom(Action<int> callback, int defaultZoom = 17)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView GetMapZoom() 页面未加载完成，无法获取缩放级别!");
                return;
            }
            var js = $@"map.getZoom()";
            // Debug.Log("MarkerWebView GetMapZoom() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                //例如18.99855658313033
                // Debug.Log("MarkerWebView GetMapZoom() 返回:"+result);
                if (result == null)
                {
                    Debug.LogWarning("MarkerWebView GetMapZoom() 返回结果为空!");
                    callback?.Invoke(defaultZoom);
                }
                else
                {
                    // 先反序列化为double，再四舍五入为int
                    var zoomDouble = JsonConvert.DeserializeObject<double>(result);
                    // Debug.Log("MarkerWebView GetMapZoom() 反序列化后的缩放级别:"+zoomDouble);
                    callback?.Invoke(Mathf.RoundToInt((float)zoomDouble));
                    // Debug.Log("MarkerWebView GetMapZoom() 最终缩放级别:"+Mathf.RoundToInt((float)zoomDouble));   
                }
            });
        }
        
        /// <summary>
        /// 显示切换头像对话框
        /// </summary>
        public void ShowSwitchHeadButton()
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowSwitchHeadButton() 页面未加载完成!");
                return;
            }
            const string js = "_ShowSwitchHeadButton()";
            Debug.Log("MarkerWebView ShowSwitchHeadButton() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
            });
        }
        
        /// <summary>
        /// 隐藏切换头像对话框
        /// </summary>
        public void HideSwitchHeadButton()
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView HideSwitchHeadButton() 页面未加载完成!");
                return;
            }
            const string js = "_HideSwitchHeadButton()";
            Debug.Log("MarkerWebView HideSwitchHeadButton() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
            });
        }
        
        /// <summary>
        /// 显示方形的无信号遮挡
        /// </summary>
        public void ShowSquareDisable(Action callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowSquareDisable() 页面未加载完成，无法显示无信号遮挡!");
                return;
            }
            var js = $@"_ShowSquareDisable()";
            Debug.Log("MarkerWebView ShowSquareDisable() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView ShowSquareDisable() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 是否显示圆形地图
        /// </summary>
        /// <param name="isCircle"></param>
        public void SetIsCircle(bool isCircle)
        {
            Debug.Log("MarkerWebView SetCircle > 初始化圆形地图 isCircle="+isCircle);
            compactWebView.SetIsCircle(isCircle);
            SetBorderRadius(isCircle ? "50%" : "0%");
        }
        
        /// <summary>
        /// 隐藏无信号遮挡
        /// </summary>
        public void HideDisable(Action callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView HideDisable() 页面未加载完成，无法隐藏无信号遮挡!");
                return;
            }
            var js = $@"_HideDisable()";
            Debug.Log("MarkerWebView HideDisable() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView HideDisable() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 显示圆形的无信号遮挡
        /// </summary>
        public void ShowCircleDisable(Action callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView ShowCircleDisable() 页面未加载完成，无法显示无信号遮挡!");
                return;
            }
            var js = $@"_ShowCircleDisable()";
            Debug.Log("MarkerWebView ShowCircleDisable() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView ShowCircleDisable() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 设置边框角度
        /// </summary>
        /// <param name="borderRadius"></param>
        /// <param name="callback"></param>
        public void SetBorderRadius(string borderRadius, Action callback = null)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView SetBorderRadius() 页面未加载完成，无法设置边框角度!");
                return;
            }
            var js = $@"_SetBorderRadius('{borderRadius}');";
            Debug.Log("MarkerWebView SetBorderRadius() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView SetBorderRadius() 返回:"+result);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 获取边框角度
        /// </summary>
        public void GetBorderRadius(Action<string> callback)
        {
            if (!IsPageLoaded())
            {
                Debug.LogWarning("MarkerWebView _GetBorderRadius() 页面未加载完成，无法获取边框角度!");
                return;
            }
            var js = $@"_GetBorderRadius()";
            Debug.Log("MarkerWebView _GetBorderRadius() EvaluateJavaScript:"+js);
            compactWebView.EvaluateJavaScript(js, result =>
            {
                Debug.Log("MarkerWebView _GetBorderRadius() 返回:"+result);
                callback?.Invoke(result);
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

        private async UniTask<bool> WriteImages()
        {
            Debug.Log("MarkerWebView WriteImages() 开始写入图片...");
            var start = DateTime.Now;
            await UniTask.SwitchToTaskPool();
            var m_DefaultPackage = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            Debug.Log("MarkerWebView WriteImages() 获取默认资源包: " + m_DefaultPackage.PackageName);
            var asset = m_DefaultPackage.GetAssetInfos("HTMlIMG");
            Debug.Log("MarkerWebView WriteImages() 获取HTMlIMG资源数量: " + asset.Length);
            var loader = new ResLoader();
            for (var i = 0; i < asset.Length; i++)
            {
                loader.AddRes(asset[i].Address, ResType.Texture2D);
            }
            Debug.Log("MarkerWebView WriteImages() 开始加载资源...");
            await loader.StartLoad();
            Debug.Log("MarkerWebView WriteImages() 资源加载完成，开始写入图片...");
            await UniTask.SwitchToMainThread();
            foreach (var res in asset)
            {
                Debug.Log("MarkerWebView WriteImages() 写入图片: " + res.Address);
                var handle = loader.GetAsset(res.Address);
                var tex = handle.AssetObject as Texture2D;
                tex = AppUtils.MakeTextureReadable(tex);
                handle.Dispose();
                Debug.Log("MarkerWebView WriteImages() 获取Texture2D成功 text==null?: " + (tex==null));
                var resName = res.Address;
                var tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "images/"+resName+".png");
                var dir = System.IO.Path.GetDirectoryName(tempPath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                // 创建一个临时纹理并转换为未压缩格式
                var tempTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                tempTexture.SetPixels(tex.GetPixels());
                tempTexture.Apply();
                var bytes = tempTexture.EncodeToPNG();
                //写入文件
                try
                {
                    await System.IO.File.WriteAllBytesAsync(tempPath, bytes);
                    Debug.Log($"MarkerWebView WriteImages() 写入图片成功: {tempPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"MarkerWebView WriteImages() 写入图片失败: {tempPath}, 错误: {e.Message}");
                    return false;
                }
                // 清理
                Destroy(tempTexture);
            }
            var timeElapsed = DateTime.Now - start;
            Debug.Log("MarkerWebView WriteImages() 所有图片写入完成 耗时: " + timeElapsed.TotalSeconds + "秒");
            await UniTask.SwitchToMainThread();
            return true;
        }
    }
}
