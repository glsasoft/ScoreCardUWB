using System.Linq;
using LetsPiu.Common.Charts.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Package_Initial.Settings.Scripts
{
    public class SettingTest : MonoBehaviour
    {

        public RawImage chartImage;
        // Start is called before the first frame update
        void Start()
        {
            // Tester.Instance.TestLoginAndRegisterGun(b =>
            // {
            //     Debug.Log("SettingTest 登录和绑定抢成功!");
            // });

            var options = new ChartOptions()
            {
                //角度线透明
                angleLinesColor = "transparent",
                //背景表格线透明
                gridColor = "transparent",
                //分类标签颜色
                pointLabelsColor = "transparent",
                //隐藏刻度数字
                showRTicks = false,
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
                    //填充的背景色
                    backgroundColor = "rgba(255, 99, 132, 0.5)",
                    //填充的边框色
                    borderColor =  "rgb(255, 99, 132)",
                    //点的背景色
                    pointBackgroundColor = "rgb(255, 99, 132)",
                    //点的边框色
                    pointBorderColor =  "rgb(255, 99, 132)",
                },
                // new Dataset()
                // {
                //     label = "My Second Dataset",
                //     data = new int[]{ 28, 48, 40, 19, 96, 27, 100},
                //     fill = true,
                //     backgroundColor = "rgba(54, 162, 235, 0.2)",
                //     borderColor = "rgb(54, 162, 235)",
                //     pointBackgroundColor = "rgb(54, 162, 235)",
                //     pointBorderColor = "#fff"
                // }
            };
            
            StaticChart.Instance.DrawRadar(options, labels.ToList(), dataset.ToList(), 3, tex =>
            {
                Debug.Log("图标绘制成功:"+(tex !=null));
                chartImage.texture = tex;
            });
        }
    }
}
