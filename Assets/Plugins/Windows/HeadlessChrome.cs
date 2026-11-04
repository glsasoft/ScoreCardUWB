#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)

using System;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LetsPiu.Plugins.Windows
{
    
    /// <summary>
    /// 适用于Windows的，用于测试的无头浏览器WebView
    /// </summary>
    public class HeadlessChrome : MonoBehaviour
    {
        // 声明DLL中的方法
        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mouse_click(long ptr, int x, int y);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mouse_move(long ptr, int x, int y);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mouse_press(long ptr, int x, int y);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mouse_release(long ptr, int x, int y);
        
        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int set_browser_size(long ptr, int width, int height);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int capture_jpg(long ptr, int quality, byte[] output, int outputSize);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_bounds(long ptr, int[] bounds);
        
        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int capture_rgba(long ptr, int quality, byte[] output, int outputSize, int[] imageSize);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int evaluate(long ptr, string js, byte[] txtBuf, int txtBufLen);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern long create_browser(string url, int width, int height, bool headless, Callback callback);
        
        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern long create_browser_with_html(string html, int width, int height, bool headless, Callback callback);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern long set_browser_content(long ptr, string html);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern long set_browser_url(long ptr, string url);

        [DllImport("HeadlessChrome.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int destroy_browser(long ptr);

        // 定义回调函数的委托
        private delegate void Callback(IntPtr message);

        public class CaptureResult
        {
            public Rect tabBounds;
            public Texture2D texture;
        }
        
        // 封装后的方法
        public static void MouseClick(long ptr, int x, int y)
        {
            // Debug.Log("HeadlessChrome MouseClick x="+x+", y="+y);
            UniTask.RunOnThreadPool(() => mouse_click(ptr, x, y)).Forget();
        }

        public static void MouseMove(long ptr, int x, int y)
        {
            // Debug.Log("HeadlessChrome MouseMove x="+x+", y="+y);
            UniTask.RunOnThreadPool(() => mouse_move(ptr, x, y)).Forget();
        }

        public static void MousePress(long ptr, int x, int y)
        {
            // Debug.Log("HeadlessChrome MousePress x="+x+", y="+y);
            // UniTask.RunOnThreadPool(() => mouse_press(ptr, x, y)).Forget();
        }

        public static void MouseRelease(long ptr, int x, int y)
        {
            // Debug.Log("HeadlessChrome MouseRelease x="+x+", y="+y);
            // UniTask.RunOnThreadPool(() => mouse_release(ptr, x, y)).Forget();
        }

        /// <summary>
        /// 截图（JPEG 格式）
        /// </summary>
        /// <param name="ptr">浏览器指针</param>
        /// <param name="quality">JPEG 质量（0-100）</param>
        /// <param name="callback">回调函数，返回生成的 Texture2D</param>
        public static void CaptureJpg(long ptr, int quality, Action<CaptureResult> callback)
        {
            UniTask.RunOnThreadPool(() =>
            {
                // 分配缓冲区（假设最大 JPEG 大小为 4MB）
                const int maxBufferSize = 4 * 1024 * 1024; // 4MB
                var outputBuffer = new byte[maxBufferSize];

                // 调用 capture_jpg
                var result = capture_jpg(ptr, quality, outputBuffer, maxBufferSize);
                var bounds = new int[4]; //left,top,width,height
                var getBoundsResult = get_bounds(ptr, bounds);
                if (getBoundsResult < 0)
                {
                    Debug.LogError($"HeadlessChrome GetBounds failed with error code: {getBoundsResult}");
                }
                // 检查是否成功
                if (result <= 0)
                {
                    Debug.LogError($"HeadlessChrome CaptureJpg failed with error code: {result}");
                    UniTask.Post(() => callback?.Invoke(null));
                    return;
                }

                UniTask.Post(() =>
                {
                    // 创建 Texture2D
                    var texture = new Texture2D(2, 2); // 初始大小不重要，会被 LoadImage 覆盖

                    // 将 JPEG 数据加载到 Texture2D
                    var success = texture.LoadImage(outputBuffer, markNonReadable: false);

                    if (!success)
                    {
                        Debug.LogError("HeadlessChrome Failed to load JPEG data into Texture2D");
                        callback?.Invoke(null); // 调用回调函数，传递 null 表示失败
                        return;
                    }

                    // 调用回调函数，返回生成的 Texture2D
                    callback?.Invoke(new CaptureResult()
                    {
                        texture = texture,
                        tabBounds = new Rect(bounds[0], bounds[1], bounds[2], bounds[3])
                    });
                });
            }).Forget();
        }
        
        /// <summary>
        /// 获取浏览器tab的边界
        /// </summary>
        /// <param name="ptr">浏览器指针</param>
        public static async UniTask<Rect> GetBounds(long ptr)
        {
            var bounds = new int[4]; //left,top,width,height
            // 调用 capture_rgba
            var result = await UniTask.RunOnThreadPool(() => get_bounds(ptr, bounds));
            // 检查是否成功
            if (result < 0)
            {
                Debug.LogError($"HeadlessChrome GetBounds failed with error code: {result}");
                return Rect.zero;
            }
            return new Rect(bounds[0], bounds[1], bounds[2], bounds[3]);
        }
        
        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="ptr">浏览器指针</param>
        /// <param name="quality">图像质量</param>
        public static async UniTask<CaptureResult> CaptureImage(long ptr, int quality)
        {
            // 分配缓冲区（假设最大图像大小为 4K，即 3840x2160，每个像素 4 字节）
            const int maxWidth = 3840;
            const int maxHeight = 2160;
            const int maxBufferSize = maxWidth * maxHeight * 4; // RGBA 每个像素 4 字节

            // 用于存储图像尺寸的数组
            var imageSize = new int[2]; // imageSize[0] = width, imageSize[1] = height
            
            var outputBuffer = new byte[maxBufferSize];
            // Debug.Log("CompactWebView CaptureImage ptr="+ptr);
            // 调用 capture_rgba
            var result = await UniTask.RunOnThreadPool(() =>
            {
                // Debug.Log("CompactWebView capture_rgba ptr="+ptr);
                return capture_rgba(ptr, quality, outputBuffer, maxBufferSize, imageSize);
            });
            // 检查是否成功
            if (result <= 0)
            {
                // Debug.LogError($"HeadlessChrome CaptureImage failed with error code: {result}");
                return null;
            }
            var bounds = await GetBounds(ptr);

            // 获取图像尺寸
            var width = imageSize[0];
            var height = imageSize[1];

            // 检查图像尺寸是否有效
            if (width <= 0 || height <= 0)
            {
                Debug.LogError($"HeadlessChrome Invalid image size: {width}x{height}");
                return null;
            }
            
            // 创建 Texture2D
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // 将 RGBA 数据加载到 Texture2D
            texture.LoadRawTextureData(outputBuffer[0..result]);
            texture.Apply(); // 应用更改

            // 调用回调函数，返回生成的 Texture2D
            return new CaptureResult()
            {
                texture = texture,
                tabBounds = bounds,
            };
        }
        
        /// <summary>
        /// 执行 JavaScript 代码
        /// </summary>
        /// <param name="ptr">浏览器指针</param>
        /// <param name="js">要执行的 JavaScript 代码</param>
        /// <param name="callback">回调函数，返回执行结果的 JSON 字符串</param>
        /// <returns>返回状态码（0 表示成功，负数表示失败）</returns>
        public static void Evaluate(long ptr, string js, Action<string> callback)
        {
            UniTask.RunOnThreadPool(() =>
            {
                // 分配缓冲区（假设最大 JSON 字符串大小为 1MB）
                const int maxBufferSize = 1024 * 1024; // 1MB
                var txtBuf = new byte[maxBufferSize];

                // 调用 evaluate
                var result = evaluate(ptr, js, txtBuf, maxBufferSize);

                // 检查是否成功
                if (result <= 0)
                {
                    Debug.LogError($"HeadlessChrome Evaluate failed with error code: {result}");
                    UniTask.Post(() => callback?.Invoke(null));
                    return;
                }

                // 将字节数组转换为字符串
                var jsonResult = Encoding.UTF8.GetString(txtBuf, 0, result);

                // 调用回调函数，返回 JSON 字符串
                UniTask.Post(() => callback?.Invoke(jsonResult));
            }).Forget();
        }
    
        /// <summary>
        /// 创建浏览器
        /// </summary>
        /// <param name="url">初始 URL</param>
        /// <param name="htmlContent">显示内容</param>
        /// <param name="width">浏览器宽度</param>
        /// <param name="height">浏览器高度</param>
        /// <param name="headless">是否无头模式</param>
        /// <param name="callback">回调函数，接收浏览器发送的消息</param>
        /// <param name="createPtr">回调函数，接受创建成功后的浏览器指针</param>
        public static void CreateBrowser(string url, string htmlContent, int width, int height, bool headless, Action<string> callback, Action<long> createPtr)
        {
            Debug.Log("HeadlessChrome CreateBrowser url="+url+", width="+width+", height="+height);
            UniTask.RunOnThreadPool(() =>
            {
                // 将 C# 的 Action<string> 转换为 Rust 回调函数
                Callback rustCallback = messagePtr =>
                {
                    // 将 C 字符串指针转换为 C# 字符串
                    var message = Marshal.PtrToStringAnsi(messagePtr);
                    Debug.LogWarning("接收到消息:"+message);
                    // 调用 C# 回调函数
                    UniTask.Post(() => callback?.Invoke(message));
                };

                // 防止回调函数被垃圾回收
                var callbackHandle = GCHandle.Alloc(rustCallback);

                // 调用 create_browser
                long result = 0;
                if (url != null)
                {
                    result = create_browser(url, width, height, headless, rustCallback);   
                }
                else
                {
                    Debug.Log("HeadlessChrome CreateBrowser with HTML content:"+htmlContent);
                    result = create_browser_with_html(htmlContent, width, height, headless, rustCallback);
                }

                // 检查是否成功
                if (result == 0)
                {
                    Debug.LogError($"HeadlessChrome CreateBrowser failed with error code: {result}");
                    callbackHandle.Free(); // 释放 GCHandle
                }
                else
                {
                    UniTask.Post(() => createPtr?.Invoke(result));
                }

                Debug.Log("HeadlessChrome 创建浏览器 指针="+result);
            }).Forget();
        }
        
        /// <summary>
        /// 设置浏览器显示内容
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public static void SetBrowserContent(long ptr, string html)
        {
            Debug.Log("HeadlessChrome 设置浏览器内容 指针="+ptr+" html="+html);
            UniTask.RunOnThreadPool(() => set_browser_content(ptr, html)).Forget();
        }
        
        /// <summary>
        /// 设置浏览器显示内容
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static void SetBrowserUrl(long ptr, string url)
        {
            Debug.Log("HeadlessChrome 设置浏览器内容 指针="+ptr+" url="+url);
            UniTask.RunOnThreadPool(() => set_browser_url(ptr, url)).Forget();
        }
        
        /// <summary>
        /// 设置浏览器大小
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetBrowserSize(long ptr, int width, int height)
        {
            Debug.Log("HeadlessChrome 设置浏览器内容 大小="+ptr+" width="+width+" height="+height);
            UniTask.RunOnThreadPool(() => set_browser_size(ptr, width, height)).Forget();
        }
        
        /// <summary>
        /// 删除单个浏览器
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static void DestroyBrowser(long ptr)
        {
            Debug.Log("HeadlessChrome 关闭浏览器 指针="+ptr);
            UniTask.RunOnThreadPool(() => destroy_browser(ptr)).Forget();
        }
    }
}

#endif