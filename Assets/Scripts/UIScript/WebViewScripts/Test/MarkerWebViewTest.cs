using System;
using System.Collections.Generic;
using LetsPiu.UI.UIScript.WebViewScripts;
using LetsPiu.UI.UIScript.WebViewScripts.Model;
using Newtonsoft.Json;
using UnityEngine;
using MapPoint = LetsPiu.UI.UIScript.WebViewScripts.Model.MapPoint;

namespace LetsPiu.Common.WebView.Samples
{
    public class MarkerWebViewTest : MonoBehaviour
    {

        public MarkerWebView markerWebView;
        private bool isMarkerShow = false;

        private void Awake()
        {
            markerWebView.WebViewLoadEvent += (() =>
            {
                markerWebView.SetMapCenterAndZoom(new MapPoint(121.43979789596133d, 31.1664080521096));
            });
        }

        public void TestPolyHole()
        {
            if (isMarkerShow)
            {
                isMarkerShow = false;
                markerWebView.RemoveMarker("TestPolyHole");
                return;
            }
            var param = new PolyHoleParams
            {
                points = new List<MapPoint>(),
                fillColor = "blue",
                fillOpacity = 0.5f,
                strokeColor = "yellow",
                strokeWeight = 5
            };
            const double lat = 31.1664080521096;
            const double lng = 121.43979789596133;
            const int numberOfPoints = 10;
            const double radius = 100;

            // 存储圆周上的点
            // 地球半径（米）
            const int earthRadius = 6378137;

            for (var i = 0; i < numberOfPoints; i++) {
                // 计算角度
                var angle = ((float)i / numberOfPoints) * (2 * Math.PI);
                // 计算新的经纬度
                var newLat = lat + (radius / earthRadius) * (180 / Math.PI) * Math.Sin(angle);
                var newLng = lng + (radius / earthRadius) * (180 / Math.PI) * Math.Cos(angle) / Math.Cos(lat * Math.PI / 180);
                // 将新点添加到数组中
                param.points.Add(new MapPoint(newLng, newLat));
            }
            markerWebView.ShowPolyHole("TestPolyHole", param);
            isMarkerShow = true;
        }
        
        /// <summary>
        /// 显示图像Marker
        /// </summary>
        public void TestImageMarker()
        {
            if (isMarkerShow)
            {
                isMarkerShow = false;
                markerWebView.RemoveMarker("TestImageMarker");
                return;
            }
            
            const double lat = 31.1664080521096;
            const double lng = 121.43979789596133;
            
            var param = new IconMarkerParams
            {
                point = new MapPoint(lng, lat),
                imageUrl = "./images/Car.png",
                width = 37*2,
                height = 18*2,
            };
            markerWebView.ShowIconMarker("TestImageMarker", param);
            isMarkerShow = true;
        }
        
        /// <summary>
        /// 更新图像Marker
        /// </summary>
        public void TestUpdateImageMarker()
        {
            if (!isMarkerShow) return;
            var param = new IconMarkerParams()
            {
                rotation = 60
            };
            markerWebView.UpdateIconMarker("TestImageMarker", param);
        }
        
        public void TestGroundAnimation()
        {
            if (isMarkerShow)
            {
                isMarkerShow = false;
                markerWebView.RemoveMarker("TestGroundAnimation");
                return;
            }
            
            const double lat = 31.1664080521096;
            const double lng = 121.43979789596133;
            
            var param = new GroundAnimParams()
            {
                point = new MapPoint(lng, lat),
                imageUrl = "./images/Blobbo.png",
                frameHeight = 34,
                fps = 10,
                width = 64,
                height = 68,
                repeat = true,
            };
            markerWebView.ShowGroundAnimation("TestGroundAnimation", param);
            isMarkerShow = true;
        }
        
        public void TestMarkerAnimation()
        {
            if (isMarkerShow)
            {
                isMarkerShow = false;
                markerWebView.RemoveMarker("TestMarkerAnimation");
                return;
            }
            
            const double lat = 31.1664080521096;
            const double lng = 121.43979789596133;
            
            var param = new MarkerImageAnimParams()
            {
                point = new MapPoint(lng, lat),
                imageUrl = "./images/Blobbo.png",
                frameHeight = 34,
                fps = 10,
                width = 64,
                height = 68,
                repeat = true,
            };
            markerWebView.ShowMarkerImageAnimation("TestMarkerAnimation", param);
            isMarkerShow = true;
        }
        
        /// <summary>
        /// 缩小
        /// </summary>
        public void ZoomOut()
        {
            markerWebView.GetMapZoom(zoom =>
            {
                Debug.Log("当前缩放级别:"+JsonConvert.SerializeObject(zoom));
                if (zoom > 3)
                {
                    var center = new MapPoint(121.43979789596133, 31.1664080521096);
                    zoom -= 1;
                    markerWebView.SetMapCenterAndZoom(center, zoom);
                }
            });
        }
        
        /// <summary>
        /// 放大
        /// </summary>
        public void ZoomIn()
        {
            markerWebView.GetMapZoom(zoom =>
            {
                Debug.Log("当前缩放级别:"+JsonConvert.SerializeObject(zoom));
                if (zoom < 19)
                {
                    var center = new MapPoint(121.43979789596133, 31.1664080521096);
                    zoom += 1;
                    markerWebView.SetMapCenterAndZoom(center, zoom);
                }
            });
        }
    }
}
