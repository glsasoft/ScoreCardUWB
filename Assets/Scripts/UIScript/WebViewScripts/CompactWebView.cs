using System;
using System.IO;
using Cysharp.Threading.Tasks;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI.UIScript.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LetsPiu.UI.UIScript.WebViewScripts
{
    /// <summary>
    /// WebView 截图错误码枚举
    /// </summary>
    public enum WebViewCaptureError
    {
        /// <summary>
        /// 截图成功
        /// </summary>
        Success = 0,
        
        /// <summary>
        /// 截图失败（通用错误）
        /// </summary>
        Failed = 1,
        
        /// <summary>
        /// 平台不支持
        /// </summary>
        PlatformNotSupported = 2,
        
        /// <summary>
        /// 没有可保存的图片
        /// </summary>
        NoImageAvailable = 3,
        
        /// <summary>
        /// 创建可读副本失败
        /// </summary>
        CreateReadableCopyFailed = 4,
        
        /// <summary>
        /// 写入文件失败
        /// </summary>
        WriteFileFailed = 5
    }
    
    public class CompactWebView : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler, IPointerExitHandler
    {
        // 随机字符池（可根据需要调整）
        private static readonly string RandomChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private UniWebView uniWebView;
        private bool pageLoad = false;
        public event Action PageLoadEvent;
        public event Action<JToken> DataEvent; 
        public event Action<string> ErrorEvent;
        private Image webviewImage;
        private RectTransform rectTransform;
        private long webviewPtr = 0;
        public Material circleMaterial;
        private Rect tabBounds = Rect.zero;
        private string oldUrl;
        /// <summary>
        /// 手动刷新安全区，而不是使用统一的UISafeAreaBreak脚本
        /// </summary>
        public UIBreakSafeAreaHelper uiBreakSafeAreaHelper;

        private void Awake()
        {
            webviewImage = GetComponentInChildren<Image>();
            rectTransform = gameObject.GetComponent<RectTransform>();
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            PageLoadEvent += () =>
            {
                Debug.Log("CompactWebView HeadLessChrome 页面加载完成");
            };
#else
            uniWebView = transform.AddComponent<UniWebView>();
            uniWebView.ReferenceRectTransform = webviewImage.rectTransform;
            uniWebView.SetAllowFileAccessFromFileURLs(true);
            uniWebView.SetAllowFileAccess(true);
            uniWebView.SetAllowHTTPAuthPopUpWindow(true);
            uniWebView.OnPageFinished += (view, code, s) =>
            {
                uniWebView.Show();
                Debug.Log("CompactWebView uniWebView页面加载完成");
                pageLoad = true;
                UniTask.Post(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    await UniTask.WaitForSeconds(0.1f);
                    PageLoadEvent?.Invoke(); 
                });
            };
            uniWebView.OnLoadingErrorReceived += (view, code, message, payload) =>
            {
                Debug.LogError("CompactWebView OnLoadingErrorReceived错误:"+message+" code="+code+" payload="+payload);
                ErrorEvent?.Invoke(message);
            };
            uniWebView.OnMessageReceived += (view, message) =>
            {
                Debug.Log("CompactWebView 接收到消息:"+message.RawMessage+" path="+message.Path+" scheme:"+message.Scheme+" args:"+message.Args);
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(
                    Newtonsoft.Json.JsonConvert.SerializeObject(message.Args));
                var json = obj["data"]?.Value<string>();
                if (json == null)
                {
                    Debug.LogError("CompactWebView 数据格式不正确!");
                    return;
                }
                DataEvent?.Invoke(Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json));
            };
#endif
        }

        private void Start()
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            
            var scale = webviewImage.rectTransform.localScale;
            scale.y = -1;
            webviewImage.rectTransform.localScale = scale;
            CaptureImage().Forget();
#else
#endif
        }

        public void LoadContent(string html)
        {
            var md5 = AppUtils.GetMD5Hash(html);
            //把html保存到临时的文件夹目录，并获取到文件路径
            var tempPath = System.IO.Path.Combine(Application.temporaryCachePath, md5+".html");
            //检查 tempPath是否存在
            if (System.IO.File.Exists(tempPath))
            {
                Debug.Log("CompactWebView 已经存在临时文件:"+tempPath);
#if UNITY_IOS && !UNITY_EDITOR
            tempPath = "file://" + tempPath;
#endif
                Debug.Log("CompactWebView 准备加载临时文件:"+tempPath);
                LoadUrl(tempPath);
                return;
            }
            System.IO.File.WriteAllText(tempPath, html);
            Debug.Log("CompactWebView 创建临时文件:"+tempPath);
#if UNITY_IOS && !UNITY_EDITOR
            tempPath = "file://" + tempPath;
#endif
            Debug.Log("CompactWebView 准备加载临时文件:"+tempPath);
            LoadUrl(tempPath);
        }

        public void LoadUrl(string url)
        {
            pageLoad = false;
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if (webviewImage == null)
            {
                webviewImage = GetComponentInChildren<Image>();
            }
            webviewImage.sprite = null;

            if (webviewPtr != 0)
            {
                Debug.Log("CompactWebView 更新浏览器内容:"+url);
                LetsPiu.Plugins.Windows.HeadlessChrome.SetBrowserUrl(webviewPtr, url);
            }
            else
            {
                oldUrl = url;
                var rectWidth = (int)rectTransform.rect.width;
                var rectHeight = (int)rectTransform.rect.height;
                Debug.Log("CompactWebView 准备创建的浏览器窗口大小:"+rectWidth+"x"+rectHeight);
                if (rectWidth < 400)
                {
                    rectWidth = 400;
                }
                if (rectHeight < 400)
                {
                    rectHeight = 400;
                }
                rectWidth += 24;
                rectHeight += 138;
                Debug.LogWarning("CompactWebView 真实创建的浏览器窗口大小:"+rectWidth+"x"+rectHeight);
                LetsPiu.Plugins.Windows.HeadlessChrome.CreateBrowser(url, null, rectWidth, rectHeight, true, s =>
                {
                    Debug.Log("CompactWebView Browser Message:"+s);
                    
                    var obj = JsonConvert.DeserializeObject<JToken>(s);
                    Debug.Log("解析消息 start....");
                    if (obj["PageLoad"] != null)
                    {
                        webviewPtr = obj["PageLoad"]?.Value<long>() ?? 0;
                        Debug.Log("CompactWebView 保存了浏览器指针:"+webviewPtr);
                        Debug.Log("CompactWebView 页面加载完成 PageLoad. 调用PageLoadEvent");
                        pageLoad = true;
                        UniTask.Post(async () =>
                        {
                            await UniTask.SwitchToMainThread();
                            await UniTask.WaitForSeconds(0.1f);
                            PageLoadEvent?.Invoke(); 
                        });
                    }else if (obj["Data"] != null)
                    {
                        Debug.Log("解析消息 Data不为空");
                        var data = obj["Data"];
                        DataEvent?.Invoke(data);
                    }else if (obj["data"] != null && obj["dataType"] == null)
                    {
                        Debug.Log("解析消息 data不为空");
                        var data = obj["data"];
                        DataEvent?.Invoke(data);
                    }
                    else if(obj["dataType"] != null)
                    {
                        Debug.Log("解析消息 type不为空");
                        DataEvent?.Invoke(obj);
                    }
                }, ptr =>
                {
                    webviewPtr = ptr;
                    Debug.Log("CompactWebView createPtr:"+webviewPtr);
                });
            }
#else
            uniWebView.Load(url);
#endif
        }

        public void LoadAsset(string filePath)
        {
            pageLoad = false;
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            var url = Path.Combine(Application.streamingAssetsPath, filePath);
#else
            var url = UniWebViewHelper.StreamingAssetURLForPath(filePath);
#endif  
            LoadUrl(url);
        }
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
        private async UniTask CaptureImage()
        {
            while (true)
            {
                if (pageLoad)
                {
                    // Debug.Log("CompactWebView CaptureImage ptr="+webviewPtr);
                    var captureResult = await LetsPiu.Plugins.Windows.HeadlessChrome.CaptureImage(webviewPtr, 100);
                    if (captureResult != null)
                    {
                        tabBounds = captureResult.tabBounds;
                        var tex = captureResult.texture;
                        // Debug.Log("截图大小:"+tex.width+"x"+tex.height+" bounds:"+captureResult.tabBounds.width+"x"+tabBounds.height+" x:"+tabBounds.x+"y:"+tabBounds.y);
                        // 将Texture2D转换为Sprite
                        var newSprite = Sprite.Create(
                            tex,
                            new Rect(0, 0, tex.width, tex.height),
                            new Vector2(0.5f, 0.5f), // 轴心点
                            100, 0, SpriteMeshType.FullRect
                        );
                        if(tex != null && !tex.IsDestroyed() && !webviewImage.IsDestroyed()) webviewImage.sprite = newSprite;   
                    }
                }
                await UniTask.Delay(TimeSpan.FromMilliseconds(50));
            }
            // ReSharper disable once FunctionNeverReturns
        }
#endif
        private void OnDestroy()
        {
            Debug.LogWarning("CompactWebView OnDestroy() 销毁浏览器 指针:"+webviewPtr);
            DestroyBrowser();
        }

        public void DestroyBrowser()
        {
            pageLoad = false;
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            Debug.Log("CompactWebView 销毁浏览器 指针:"+webviewPtr);
            pageLoad = false;
            if (webviewPtr > 0)
            {
                LetsPiu.Plugins.Windows.HeadlessChrome.DestroyBrowser(webviewPtr);
            }
            Debug.Log("CompactWebView 销毁浏览器成功 webviewPtr:"+webviewPtr);
            webviewPtr = 0;
#else
#endif
        }
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
        public (int, int) OffsetMousePosition(int imgWidth, int imgHeight, int x, int y)
        {
            if (tabBounds.width != 0 && tabBounds.height != 0)
            {
                // 计算左右边框宽度（假设左右边框相等）
                var borderWidth = ((int)tabBounds.width - imgWidth) / 2;
                
                // 计算顶部区域高度（标题栏 + 上边框）
                // tabBounds.height 是完整窗口高度，imgHeight 是内容区域高度
                // 差值就是顶部区域 + 底部边框的高度
                // 假设底部边框和左右边框宽度相同
                var topOffset = (int)tabBounds.height - imgHeight - borderWidth;
                
                Debug.Log($"CompactWebView OffsetMousePosition - imgSize:({imgWidth}x{imgHeight}) tabBounds:({tabBounds.width}x{tabBounds.height}) " +
                          $"input:({x},{y}) borderWidth:{borderWidth} topOffset:{topOffset} " +
                          $"output:({x + borderWidth},{y + topOffset})");

                topOffset -= 100;
                return (x + borderWidth, y + topOffset);
            }
            else
            {
                Debug.LogWarning($"CompactWebView OffsetMousePosition - tabBounds未初始化，直接返回 input:({x},{y})");
                return (x, y);
            }
        }
#endif

        public void OnPointerMove(PointerEventData eventData)
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if (webviewPtr <= 0) return;
            if (webviewImage == null) return;
            if (webviewImage.sprite == null || webviewImage.sprite.texture == null) return;
            
            // 将屏幕坐标转换为 webviewImage 的局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                webviewImage.rectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector2 localPoint);
            
            // 将局部坐标转换为从左上角开始的坐标系
            // 注意：因为图像被垂直翻转（scale.y = -1），Y坐标需要特殊处理
            var x = (int)(localPoint.x + webviewImage.rectTransform.rect.width / 2);
            var y = (int)(webviewImage.rectTransform.rect.height / 2 + localPoint.y); // 加号，因为翻转
            
            var imgWidth = webviewImage.sprite.texture.width;
            var imgHeight = webviewImage.sprite.texture.height;
            var (rx, ry) = OffsetMousePosition(imgWidth, imgHeight, x, y);
            LetsPiu.Plugins.Windows.HeadlessChrome.MouseMove(webviewPtr, rx, ry);
#else
#endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("CompactWebView OnPointerDown事件触发...");
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if (webviewPtr <= 0) return;
            if (webviewImage == null) return;
            if (webviewImage.sprite == null || webviewImage.sprite.texture == null) return;
            
            // 将屏幕坐标转换为 webviewImage 的局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                webviewImage.rectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector2 localPoint);
            
            Debug.Log($"CompactWebView OnPointerDown - 屏幕坐标:{eventData.position} 局部坐标:{localPoint}");
            
            // 将局部坐标转换为从左上角开始的坐标系
            // 注意：因为图像被垂直翻转（scale.y = -1），Y坐标需要特殊处理
            var x = (int)(localPoint.x + webviewImage.rectTransform.rect.width / 2);
            var y = (int)(webviewImage.rectTransform.rect.height / 2 + localPoint.y); // 加号，因为翻转
            
            var imgWidth = webviewImage.sprite.texture.width;
            var imgHeight = webviewImage.sprite.texture.height;
            Debug.Log($"CompactWebView OnPointerDown - 转换后坐标:({x},{y}) ImageRect:({webviewImage.rectTransform.rect.width}x{webviewImage.rectTransform.rect.height}) Img:({imgWidth}x{imgHeight})");
            
            var (rx, ry) = OffsetMousePosition(imgWidth, imgHeight, x, y);
            LetsPiu.Plugins.Windows.HeadlessChrome.MousePress(webviewPtr, rx, ry);
#else
#endif  
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("CompactWebView OnPointerUp事件触发...");
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if (webviewPtr <= 0) return;
            if (webviewImage == null) return;
            if (webviewImage.sprite == null || webviewImage.sprite.texture == null) return;
            
            // 将屏幕坐标转换为 webviewImage 的局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                webviewImage.rectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector2 localPoint);
            
            // 将局部坐标转换为从左上角开始的坐标系
            // 注意：因为图像被垂直翻转（scale.y = -1），Y坐标需要特殊处理
            var x = (int)(localPoint.x + webviewImage.rectTransform.rect.width / 2);
            var y = (int)(webviewImage.rectTransform.rect.height / 2 + localPoint.y); // 加号，因为翻转
            
            var imgWidth = webviewImage.sprite.texture.width;
            var imgHeight = webviewImage.sprite.texture.height;
            var (rx, ry) = OffsetMousePosition(imgWidth, imgHeight, x, y);
            LetsPiu.Plugins.Windows.HeadlessChrome.MouseRelease(webviewPtr, rx, ry);
#else
#endif  
        }

        public void OnPointerExit(PointerEventData eventData)
        {
//             Debug.Log("CompactWebView OnPointerExit事件触发...");
// #if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
//             if (webviewPtr <= 0) return;
//             var y = (int)rectTransform.rect.height - (int)eventData.position.y;
//             LetsPiu.Plugins.Windows.HeadlessChrome.MouseRelease(webviewPtr, 10, 10);
// #else
// #endif  
        }
        
        /// <summary>
        /// 执行js
        /// </summary>
        /// <param name="js"></param>
        /// <param name="callback"></param>
        public void EvaluateJavaScript(string js, Action<string> callback)
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if (webviewPtr <= 0)
            {
                Debug.LogError("CompactWebView 执行js失败，webviewPtr为0，请先加载页面!");
                callback?.Invoke(null);
                return;
            }
            LetsPiu.Plugins.Windows.HeadlessChrome.Evaluate(webviewPtr, js, result =>
            {
                // Debug.Log("CompactWebView 执行js返回 result.data=【"+result+"】");
                UniTask.Post(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    await UniTask.WaitForSeconds(0.01f);
                    // 兼容：HeadlessChrome 可能直接返回原始字符串（未包 JSON 引号）。
                    // 优先尝试反序列化，失败则回退原始字符串（去除首尾引号）。
                    if (string.IsNullOrEmpty(result))
                    {
                        callback?.Invoke(null);
                        return;
                    }
                    string finalStr = null;
                    try
                    {
                        finalStr = JsonConvert.DeserializeObject<string>(result);
                    }
                    catch
                    {
                        // 回退：如果不是有效 JSON 字符串格式，直接使用原值。
                        finalStr = result.Trim('"');
                    }
                    callback?.Invoke(finalStr);
                });
            });
#else
            uniWebView.EvaluateJavaScript(js, result =>
            {
                UniTask.Post(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    await UniTask.WaitForSeconds(0.01f);
                    // Debug.Log("CompactWebView 执行js返回 result.data=【"+result.data+"】");
                    callback?.Invoke(result.data);
                });
            });
#endif
        }

        public void SetSize(int width, int height)
        {
            if (!pageLoad)
            {
                return;
            }

            if (webviewPtr == 0)
            {
                return;
            }
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            //重新打开浏览器
            DestroyBrowser();
            LoadUrl(oldUrl);
#else

#endif
        }

        public void SetRoundCorner()
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            
#else

#endif
        }
        
        public void SetBackgroundColor(Color color)
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
#else
         uniWebView.BackgroundColor = color;
#endif
        }

        public void SetIsCircle(bool isCircle)
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            webviewImage.material = isCircle ? circleMaterial : null;
#else
            
#endif
        }
        
        public void SetAlpha(float a)
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            var color = webviewImage.color;
            color.a = a;
            webviewImage.color = color;
#else

#endif
        }

        public void Hide()
        {
            UniTask.Post(async () =>
            {
                await UniTask.WaitForSeconds(0.1f);
                webviewImage.gameObject.SetActive(false);
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)

#else
            uniWebView.Hide();
#endif 
            });
        }

        public void Show()
        {
            UniTask.Post(async () =>
            {
                await UniTask.WaitForSeconds(0.1f);
                webviewImage.gameObject.SetActive(true);
                #if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
                
                #else
                            uniWebView.Show();
                #endif
            });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if (webviewPtr <= 0) return;
            if (webviewImage == null) return;
            if (webviewImage.sprite == null || webviewImage.sprite.texture == null) return;
            
            // 将屏幕坐标转换为 webviewImage 的局部坐标（注意：使用 webviewImage.rectTransform 而不是 rectTransform）
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                webviewImage.rectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector2 localPoint);
            
            Debug.Log($"CompactWebView OnPointerClick - 屏幕坐标:{eventData.position} 局部坐标:{localPoint} " +
                      $"ImageRect:({webviewImage.rectTransform.rect.width}x{webviewImage.rectTransform.rect.height}) " +
                      $"Scale:{webviewImage.rectTransform.localScale}");
            
            // 将局部坐标转换为从左上角开始的坐标系
            // 注意：因为 webviewImage.localScale.y = -1（图像被垂直翻转），localPoint.y 的方向也是反的
            var x = (int)(localPoint.x + webviewImage.rectTransform.rect.width / 2);
            var y = (int)(webviewImage.rectTransform.rect.height / 2 + localPoint.y); // 注意这里是加号，因为图像被翻转了
            
            Debug.Log($"CompactWebView OnPointerClick - 转换后坐标: ({x},{y})");
            
            var imgWidth = webviewImage.sprite.texture.width;
            var imgHeight = webviewImage.sprite.texture.height;
            var (rx, ry) = OffsetMousePosition(imgWidth, imgHeight, x, y);
            
            Debug.Log($"CompactWebView OnPointerClick - 最终坐标: ({rx},{ry}) 发送到HeadlessChrome");
            LetsPiu.Plugins.Windows.HeadlessChrome.MouseClick(webviewPtr, rx, ry);
#else
#endif
        }

        public bool IsPageLoaded()
        {
            // Debug.Log("CompactWebView IsPageLoaded() pageLoad:"+pageLoad);
            return pageLoad;
        }

        public void HideUniWebView()
        {
            if (uniWebView != null)
            {
                uniWebView.Hide();
            }
        }

        public void ShowUniWebView()
        {
            if (uniWebView != null)
            {
                uniWebView.Show();
            }
        }

#if UNITY_ANDROID || UNITY_IOS
        public UniWebView GetUniWebView()
        {
            return uniWebView;
        }
#endif

        public RectTransform GetWebviewImageRectTransform()
        {
            return webviewImage.rectTransform;
        }

        /// <summary>
        /// 截取网页图片（统一 JS 注入方式）
        /// 兼容旧接口：内部改为通过 Canvas (WebGL 可包含) 注入 JS -> toDataURL -> 保存 PNG。
        /// 默认选择第一个 <canvas>。如果需要指定，请直接调用 CaptureWebGLCanvasViaJS。
        /// 注意：若 WebGL 未启用 preserveDrawingBuffer 或存在跨域资源，可能失败。
        /// </summary>
        /// <param name="pngFileName"></param>
        /// <param name="callback">截图完成回调，参数为错误码和文件路径</param>
        public void CaptureWebViewImage(string pngFileName, Action<WebViewCaptureError, string> callback)
        {
            Debug.Log("CompactWebView CaptureWebViewImage: 通过 JS Canvas 截图 pngFileName=" + pngFileName);
            // 统一改为 JS Canvas 截图（默认第一个 canvas）。
            CaptureWebGLCanvasViaJS(pngFileName, callback, "canvas");
        }

        /// <summary>
        /// 通过注入 JS 捕获网页中指定 Canvas(含 WebGL 内容)的截图，并保存为 PNG。
        /// 使用场景：UniWebView 的 CaptureSnapshot 无法捕获 WebGL 渲染内容（如地图、Three.js、MapboxGL 等）。
        /// 原理：执行 JS -> 找到目标 canvas -> 生成 dataURL (PNG) -> 回传给 C# -> base64 解码保存。
        /// 注意:
        ///  1. 如果使用 WebGL 且未设置 preserveDrawingBuffer=true，某些浏览器可能无法直接 toDataURL，需要在创建上下文时设定。
        ///  2. 如果 canvas 上绘制了跨域图片且未正确设置 CORS，会导致 "SecurityError: The canvas has been tainted"。
        ///  3. 可通过传入 canvasSelector 精确选择，例如: "#mapCanvas", ".webgl-layer canvas"。
        ///  4. 返回错误前缀："ERROR:" -> 解析并映射到 Failed。
        /// </summary>
        /// <param name="pngFileName">保存文件名（可不带扩展名，自动补 .png）</param>
        /// <param name="callback">回调：错误码 + 文件路径（失败为空字符串或错误描述）</param>
        /// <param name="canvasSelector">CSS 选择器，默认 "canvas" 取第一个；如果有多个 WebGL 层，可自定义。</param>
        public void CaptureWebGLCanvasViaJS(string pngFileName, Action<WebViewCaptureError, string> callback, string canvasSelector = "canvas")
        {
            // 新实现：调用页面内的 html2canvas 异步截图流程，而不是直接同步 toDataURL。
            // 逻辑：
            //  1. 注入启动脚本：重置 window.__lastCaptureDataURL，调用 html2canvas(#container) 异步生成截图。
            //  2. 轮询 window.__lastCaptureDataURL，直到拿到 data:image/... 或 ERROR: 前缀，或超时。
            //  3. 保存 PNG，回调结果。
            // 这样可直接复用页面中已经引入的 html2canvas，以及用户添加的 preserveDrawingBuffer hack。

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (uniWebView == null)
            {
                Debug.LogError("CaptureWebGLCanvasViaJS: uniWebView 为空，无法执行 JS 截图");
                callback?.Invoke(WebViewCaptureError.Failed, string.Empty);
                return;
            }
#endif

            // 统一保存目录
            var baseDir = System.IO.Path.Combine(Application.temporaryCachePath, "CapturedWebViews");
            if (!System.IO.Directory.Exists(baseDir))
            {
                System.IO.Directory.CreateDirectory(baseDir);
                Debug.Log("CompactWebView CaptureWebGLCanvasViaJS: 创建目录 " + baseDir);
            }
            if (string.IsNullOrWhiteSpace(pngFileName))
            {
                pngFileName = "WebGLCanvas_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            }
            var fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(pngFileName);
            var pngPath = System.IO.Path.Combine(baseDir, fileNameNoExt + ".png");
            Debug.Log("CompactWebView CaptureWebGLCanvasViaJS: 最终保存路径 " + pngPath);

#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if (webviewPtr <= 0)
            {
                Debug.LogError("CompactWebView CaptureWebGLCanvasViaJS: webviewPtr 无效，请先加载页面");
                callback?.Invoke(WebViewCaptureError.Failed, string.Empty);
                return;
            }
#elif (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            // uniWebView 平台继续
#else
            Debug.Log("CompactWebView CaptureWebGLCanvasViaJS: 当前平台不支持 JS Canvas 截图");
            callback?.Invoke(WebViewCaptureError.PlatformNotSupported, string.Empty);
            return;
#endif

            // 启动异步截图脚本：不直接使用 captureMap() 的下载逻辑，而是改造为统一变量存储。
            // 如果页面未来自行增加 window.captureMapForUnity() 可直接调用；当前我们在注入脚本中实现。
            // 改进版启动脚本：
            //  1. 不再立即判定 html2canvas 是否存在，而是递归等待（最多 ~6 秒）。
            //  2. 使用 window.html2canvas 显式访问，避免局部作用域问题。
            //  3. 增加状态变量 window.__captureStatus 供轮询诊断。
            //  4. 若存在用户自定义 captureMapForUnity() / captureMap() 则优先尝试。
            // 重要修复：去除 JS 中无意义的 "\n" 转义符（之前作为普通字符插入可能导致脚本解析失败返回 null）。使用真实换行确保可读性。
            var startScript = (
                "(function(){try{" +
                "\nwindow.__lastCaptureDataURL='';" +
                "\nwindow.__captureStatus='INIT';" +
                "\nvar MAX_WAIT=40; var attempt=0;" +
                "\nif(!window.captureMapForUnity){" +
                "\n  window.captureMapForUnity=function(){" +
                "\n    var el=document.querySelector('#container');" +
                "\n    if(!el){window.__captureStatus='ERROR:ContainerNotFound';return;}" +
                "\n    window.__captureStatus='CAPTURING';" +
                "\n    if(typeof window.html2canvas==='function'){" +
                "\n      try{window.html2canvas(el,{useCORS:true}).then(function(canvas){" +
                "\n        try{window.__lastCaptureDataURL=canvas.toDataURL('image/png');window.__captureStatus='DONE';}" +
                "\n        catch(e){window.__lastCaptureDataURL='ERROR:'+e.message;window.__captureStatus='ERROR:'+e.message;}" +
                "\n      }).catch(function(e){window.__lastCaptureDataURL='ERROR:'+e.message;window.__captureStatus='ERROR:'+e.message;});}" +
                "\n      catch(e){window.__lastCaptureDataURL='ERROR:'+e.message;window.__captureStatus='ERROR:'+e.message;}" +
                "\n    } else {window.__captureStatus='WAIT_LIB_CAPTURE';}" +
                "\n  };" +
                "\n}" +
                "\nfunction beginCapture(){" +
                "\n  var el=document.querySelector('#container');" +
                "\n  if(!el){window.__captureStatus='ERROR:ContainerNotFound';return;}" +
                "\n  window.__captureStatus='CAPTURING';" +
                "\n  if(typeof window.captureMapForUnity==='function'){try{window.captureMapForUnity();}catch(e){}}" +
                "\n  else if(typeof window.captureMap==='function'){try{window.captureMap();}catch(e){}}" +
                "\n  if(typeof window.html2canvas==='function'){" +
                "\n    try{window.html2canvas(el,{useCORS:true}).then(function(canvas){" +
                "\n      try{window.__lastCaptureDataURL=canvas.toDataURL('image/png');window.__captureStatus='DONE';}" +
                "\n      catch(e){window.__lastCaptureDataURL='ERROR:'+e.message;window.__captureStatus='ERROR:'+e.message;}" +
                "\n    }).catch(function(e){window.__lastCaptureDataURL='ERROR:'+e.message;window.__captureStatus='ERROR:'+e.message;});}" +
                "\n    catch(e){window.__lastCaptureDataURL='ERROR:'+e.message;window.__captureStatus='ERROR:'+e.message;}" +
                "\n  }" +
                "\n}" +
                "\nfunction waitLib(){" +
                "\n  if(typeof window.html2canvas==='function' || typeof window.captureMap==='function' || typeof window.captureMapForUnity==='function'){beginCapture();return;}" +
                "\n  attempt++;" +
                "\n  if(attempt>MAX_WAIT){window.__captureStatus='ERROR:html2canvasTimeout';return;}" +
                "\n  window.__captureStatus='WAIT_LIB_'+attempt;" +
                "\n  setTimeout(waitLib,150);" +
                "\n}" +
                "\nwaitLib();" +
                "\nreturn 'STARTED';" + // 使用简单字符串返回，EvaluateJavaScript 里已经添加了容错
                "\n}catch(e){window.__captureStatus='ERROR:'+e.message;return 'ERROR:'+e.message}})();"
            );

            Debug.Log("CompactWebView CaptureWebGLCanvasViaJS: 注入启动脚本 -> " + startScript);

            // 异步流程封装
            UniTask.Void(async () =>
            {
                Debug.Log("CaptureWebGLCanvasViaJS: startScript length=" + startScript.Length);
                var startResult = await EvalAsync(startScript);
                Debug.Log("CompactWebView CaptureWebGLCanvasViaJS: 启动返回 -> " + startResult);
                startResult = startResult?.Trim('"');
                if (string.IsNullOrEmpty(startResult) || startResult.StartsWith("ERROR:"))
                {
                    Debug.LogError("CompactWebView CaptureWebGLCanvasViaJS: 启动失败 -> " + startResult);
                    callback?.Invoke(WebViewCaptureError.Failed, startResult ?? string.Empty);
                    return;
                }

                // 轮询结果
                const int maxAttempts = 40; // 40 * 200ms = 8秒超时
                const int intervalMs = 200;
                string dataUrl = null;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    await UniTask.Delay(intervalMs);
                    // 同时获取状态与数据，方便诊断
                    var pollScript = "(function(){return JSON.stringify({s:window.__captureStatus||'',d:window.__lastCaptureDataURL||''});})();";
                    var pollRaw = await EvalAsync(pollScript);
                    pollRaw = pollRaw?.Trim('"');
                    if (!string.IsNullOrEmpty(pollRaw))
                    {
                        try
                        {
                            var json = JsonConvert.DeserializeObject<JObject>(pollRaw);
                            var status = json? ["s"]?.Value<string>() ?? string.Empty;
                            var data = json? ["d"]?.Value<string>() ?? string.Empty;
                            if (!string.IsNullOrEmpty(status))
                            {
                                Debug.Log("CaptureWebGLCanvasViaJS 轮询 status=" + status + " len=" + data.Length + " attempt=" + attempt);
                            }
                            if (status.StartsWith("ERROR:"))
                            {
                                dataUrl = status; // 直接作为错误处理
                                break;
                            }
                            if (status == "DONE" && data.StartsWith("data:image"))
                            {
                                dataUrl = data;
                                break;
                            }
                            // 若已有 data:image 但状态未更新，也允许通过
                            if (data.StartsWith("data:image"))
                            {
                                dataUrl = data;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("CaptureWebGLCanvasViaJS 轮询解析失败:" + ex.Message + " 原始:" + pollRaw);
                        }
                    }
                }

                if (string.IsNullOrEmpty(dataUrl))
                {
                    Debug.LogError("CompactWebView CaptureWebGLCanvasViaJS: 轮询超时，未获取到截图数据");
                    callback?.Invoke(WebViewCaptureError.Failed, "ERROR:Timeout");
                    return;
                }

                // 处理最终结果
                HandleJsScreenshotResult(dataUrl, pngPath, callback);
            });
        }

        /// <summary>
        /// 将 EvaluateJavaScript 包装为可等待的 UniTask（方便轮询异步变量）
        /// </summary>
        private UniTask<string> EvalAsync(string js)
        {
            var tcs = new UniTaskCompletionSource<string>();
            EvaluateJavaScript(js, r => { tcs.TrySetResult(r); });
            return tcs.Task;
        }

        /// <summary>
        /// 处理 JS 返回的结果（dataURL 或 ERROR）并写入文件
        /// </summary>
        private void HandleJsScreenshotResult(string jsResult, string pngPath, Action<WebViewCaptureError, string> callback)
        {
            if (string.IsNullOrEmpty(jsResult))
            {
                Debug.LogError("CompactWebView HandleJsScreenshotResult: JS 返回空");
                callback?.Invoke(WebViewCaptureError.Failed, string.Empty);
                return;
            }

            // 可能包含引号（Evaluate 返回的反序列化字符串）
            var clean = jsResult.Trim('\"');
            if (clean.StartsWith("ERROR:"))
            {
                Debug.LogError("CompactWebView HandleJsScreenshotResult: JS 截图失败 -> " + clean);
                callback?.Invoke(WebViewCaptureError.Failed, clean);
                return;
            }
            if (!clean.StartsWith("data:image"))
            {
                Debug.LogError("CompactWebView HandleJsScreenshotResult: 非预期返回 -> " + clean);
                callback?.Invoke(WebViewCaptureError.Failed, string.Empty);
                return;
            }
            try
            {
                var commaIndex = clean.IndexOf(',');
                if (commaIndex < 0)
                {
                    Debug.LogError("CompactWebView HandleJsScreenshotResult: dataURL 结构不正确");
                    callback?.Invoke(WebViewCaptureError.Failed, string.Empty);
                    return;
                }
                var base64Data = clean.Substring(commaIndex + 1);
                var bytes = System.Convert.FromBase64String(base64Data);
                System.IO.File.WriteAllBytes(pngPath, bytes);
                Debug.Log("CompactWebView HandleJsScreenshotResult: 保存截图成功 -> " + pngPath);
                callback?.Invoke(WebViewCaptureError.Success, pngPath);
            }
            catch (Exception e)
            {
                Debug.LogError("CompactWebView HandleJsScreenshotResult: 写文件异常 -> " + e.Message);
                callback?.Invoke(WebViewCaptureError.WriteFileFailed, string.Empty);
            }
        }
    }
}
