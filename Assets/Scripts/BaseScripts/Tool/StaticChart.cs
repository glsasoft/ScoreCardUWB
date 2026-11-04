using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Common.Charts.Scripts
{
    public class Dataset
    {
        public string label;
        public int[] data;
        public bool fill = true;
        public string backgroundColor = "rgba(255, 99, 132, 0.2)";
        public string borderColor = "rgb(255, 99, 132)";
        public string pointBackgroundColor = "rgb(255, 99, 132)";
        public string pointBorderColor = "#fff";
    }

    public class ChartOptions
    {
        /// <summary>
        /// 绘制宽度
        /// </summary>
        public int width = 400;
        /// <summary>
        /// 绘制高度
        /// </summary>
        public int height = 400;
        /// <summary>
        /// 背景刻度线条颜色
        /// </summary>
        public string gridColor = "gray";
        /// <summary>
        /// 背景刻度角度线颜色
        /// </summary>
        public string angleLinesColor = "gray";
        /// <summary>
        /// 刻度标签文字颜色
        /// </summary>
        public string pointLabelsColor = "gray";
        /// <summary>
        /// 是否显示图例
        /// </summary>
        public bool showLegend = false;
        /// <summary>
        /// 是否显示雷达图刻度数字(r)
        /// </summary>
        public bool showRTicks = true;
    }
    
    /// <summary>
    /// 生成静态图
    /// </summary>
    public class StaticChart : SingletonComponent<StaticChart>
    {

        private UniWebView uniWebView;
        
        /// <summary>
        /// 绘制雷达图
        /// </summary>
        /// <param name="options"></param>
        /// <param name="labels"></param>
        /// <param name="datasets"></param>
        /// <param name="borderWidth"></param>
        /// <param name="callback"></param>
        public void DrawRadar(ChartOptions options, List<string> labels, List<Dataset> datasets, int borderWidth, Action<Texture2D> callback)
        {
            var datasetsJson = JsonConvert.SerializeObject(datasets);
            var labelsJson = JsonConvert.SerializeObject(labels);
            var config = $@"
                const config = {{
                    type: 'radar',
                    data: {{
                        labels: {labelsJson},
                        datasets: {datasetsJson}
                    }},
                    options: {{
                        devicePixelRatio: 1,
                        legend: {{
                            display: false
                        }},
                        scales: {{
                            r: {{
                                ticks: {{
                                  display: { options.showRTicks.ToString().ToLower() },
                                }},
                                grid: {{
                                    color: '{ options.gridColor }'
                                }},
                                angleLines: {{
                                    color: '{ options.angleLinesColor }'
                                }},
                                pointLabels: {{
                                    color: '{ options.pointLabelsColor }'
                                }}
                            }}
                        }},
                        animations: null,
                        elements: {{
                            line: {{
                                borderWidth: {borderWidth}
                            }}
                        }},
                        plugins: {{
                            legend: {{
                                display: '{ options.showLegend }'=='True'
                            }}
                        }}
                    }},
                    plugins: [charDrawListener]
                }};

                DrawChart({options.width}, {options.height}, config);
            ";
            StartCoroutine(DrawChartAsync(config).ToCoroutine(callback));
        }
        
        public void DrawChart(string config, Action<Texture2D> callback)
        {
            StartCoroutine(DrawChartAsync(config).ToCoroutine(callback));
        }
        
        public async UniTask<Texture2D> DrawChartAsync(string config)
        {
            var resLoader = new ResLoader();
            resLoader.AddRes("Charts", ResType.Prefab);
            var handle = resLoader.GetAsset("Charts");
            var textAsset = handle.AssetObject as TextAsset;
            var html = textAsset.text;
            handle.Dispose();
            var complete = false;
            Texture2D texture = null;
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            Debug.Log("StaticChart 加载网页:"+html);
            long browserPtr = 0;
            LetsPiu.Plugins.Windows.HeadlessChrome.CreateBrowser(null, html, 600, 600, true, message =>
            {
                Debug.Log("StaticChart 接收到消息:"+message);
                var obj = JsonConvert.DeserializeObject<JToken>(message);
                var dataType = obj["Data"]?["dataType"]?.Value<string>();
                if (dataType == "image")
                {
                    Debug.Log("StaticChart 页面加载完成, 图表绘制完成...");
                    LetsPiu.Plugins.Windows.HeadlessChrome.Evaluate(browserPtr, "GetImageBase64()", base64 =>
                    {
                        // Debug.Log("StaticChart 图片数据:"+base64);
                        if (base64.StartsWith("\""))
                        {
                            base64 = base64.Substring(1, base64.Length - 1);
                        }

                        if (base64.EndsWith("\""))
                        {
                            base64 = base64[..^1];
                        }
                        
                        UniTask.Post(async () =>
                        {
                            try
                            {
                                texture = await ConvertBase64ToTextureAsync(base64);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("DrawChart Base64转图片失败:"+e.Message);
                            }
                        
                            complete = true;
                            LetsPiu.Plugins.Windows.HeadlessChrome.DestroyBrowser(browserPtr);
                            browserPtr = 0;
                        });     
                    });
                }
            }, ptr =>
            {
                if (ptr == 0)
                {
                    Debug.LogError("DrawChart 浏览器启动失败!");
                    complete = true;
                }
                else
                {
                    Debug.Log("DrawChart 浏览器启动成功:"+ ptr);
                    browserPtr = ptr;
                    LetsPiu.Plugins.Windows.HeadlessChrome.Evaluate(browserPtr, config, message => { });
                }
            });
#else
            if (uniWebView == null)
            {
                uniWebView = gameObject.AddComponent<UniWebView>();
            }
            uniWebView.OnMessageReceived += (view, message) =>
            {
                Debug.Log("DrawChart 接收到消息:"+message.RawMessage+" path="+message.Path+" scheme:"+message.Scheme+" args:"+Newtonsoft.Json.JsonConvert.SerializeObject(message.Args));
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(
                    Newtonsoft.Json.JsonConvert.SerializeObject(message.Args));
                var json = obj["data"]?.Value<string>();
                if (json == null)
                {
                    Debug.LogError("DrawChart 数据格式不正确!");
                    return;
                }
                var dataJson = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                var dataType = dataJson["dataType"]?.Value<string>();
                if (dataType == "image")
                {
                    Debug.Log("StaticChart UniWebView页面加载完成, 图表绘制完成 json="+json);
                    uniWebView.EvaluateJavaScript("GetImageBase64()", payload =>
                    {
                        var base64 = payload.data;
                        Debug.Log("StaticChart UniWebView图片数据:"+base64);
                        if (base64.StartsWith("\""))
                        {
                            base64 = base64.Substring(1, base64.Length - 1);
                        }

                        if (base64.EndsWith("\""))
                        {
                            base64 = base64[..^1];
                        }
                        
                        UniTask.Post(async () =>
                        {
                            try
                            {
                                texture = await ConvertBase64ToTextureAsync(base64);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("DrawChart Base64转图片失败:"+e.Message);
                            }
                            complete = true;
                        });     
                    });
                }
            };
            uniWebView.OnPageFinished += (view, code, s) =>
            {
                Debug.Log("DrawChart uniWebView页面加载完成");
                uniWebView.EvaluateJavaScript(config, message => {});
            };
            uniWebView.LoadHTMLString(html, "http://localhost");
            uniWebView.Show();
#endif
            //5秒钟超时
            double waitTime = 0;
            while (waitTime < 5)
            {
                await UniTask.WaitForSeconds(0.1f);
                waitTime += 0.1;
                if (complete)
                {
                    Debug.Log("DrawChart complete = true!");
                    break;
                }
            }
            Debug.Log("DrawChart 绘制等待耗时:"+waitTime+" texture is null?"+(texture == null));
#if UNITY_EDITOR_WIN || (UNITY_ANDROID && UNITY_EDITOR)
            if(browserPtr < 0) LetsPiu.Plugins.Windows.HeadlessChrome.DestroyBrowser(browserPtr);
#endif
            return texture;
        }
        
        public async UniTask<Texture2D> ConvertBase64ToTextureAsync(string base64String)
        {
            try
            {
                Debug.Log("ConvertBase64ToTextureAsync:"+base64String);
                // 在子线程中执行 Base64 解码和 Texture2D 创建
                var imageBytes = await UniTask.RunOnThreadPool(() =>
                {
                    var imageBytes = Convert.FromBase64String(base64String);
                    return imageBytes;
                });
                
                var texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageBytes))
                {
                    return texture;
                }
                else
                {
                    Debug.LogError("DrawChart Failed to load texture from Base64 string");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("DrawChart Error converting Base64 to Texture2D: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// 测试绘制一个雷达图
        /// </summary>
        /// <param name="rawImage"></param>
        public void TestChart(RawImage rawImage)
        {
            var options = new ChartOptions()
            {
                
            };
            
            var labels = new string[]{ "Eating",
                "Drinking",
                "Sleeping",
                "Designing",
                "Coding",
                "Cycling",
                "Running" };
            
            var dataset = new Dataset[]
            {
                new Dataset()
                {
                    label = "My First Dataset",
                    data = new int []{65, 59, 90, 81, 56, 55, 40},
                    fill =  true,
                    backgroundColor = "rgba(255, 99, 132, 0.2)",
                    borderColor =  "rgb(255, 99, 132)",
                    pointBackgroundColor = "rgb(255, 99, 132)",
                    pointBorderColor =  "#fff",
                },
                new Dataset()
                {
                    label = "My Second Dataset",
                    data = new int[]{ 28, 48, 40, 19, 96, 27, 100},
                    fill = true,
                    backgroundColor = "rgba(54, 162, 235, 0.2)",
                    borderColor = "rgb(54, 162, 235)",
                    pointBackgroundColor = "rgb(54, 162, 235)",
                    pointBorderColor = "#fff"
                }
            };
            
            StaticChart.Instance.DrawRadar(options, labels.ToList(), dataset.ToList(), 3, tex =>
            {
                Debug.Log("图表绘制成功:"+(tex !=null));
                if (tex != null)
                {
                    Debug.Log("图表绘大小:"+tex.width+","+tex.height);
                    rawImage.texture = tex;
                }
            });
        }
    }
}