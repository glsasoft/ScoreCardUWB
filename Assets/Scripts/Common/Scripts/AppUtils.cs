using Cysharp.Threading.Tasks;
using LetsPiu.Common.Network.Model.Http.Play;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LetsPiu.Main.Common.Scripts
{
    using LetsPiu.Define;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using UnityEngine;

    public static class AppUtils
    {
        /// <summary>
        /// 从GPU中复制Texture2D到CPU中，以便可以进行转码等其他操作
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Texture2D MakeTextureReadable(Texture source)
        {
            if (source == null) return null;
            var renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            var previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            var readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static Color ParseColor(string colorStr)
        {
            if (!ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                Debug.LogError("Utils Invalid hex color!");
            }
            return color;
        }

        public static int MillisecondsInterval(DateTime date1, DateTime date2)
        {
            var dValue = date2.Subtract(date1);
            var nReturn = (int)dValue.TotalMilliseconds;
            return nReturn;
        }

        /// <summary>
        /// 秒数格式化成 00:00
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public static string FormatTime(int totalSeconds)
        {
            if (totalSeconds < 0)
            {
                totalSeconds = 0;
            }
            var time = TimeSpan.FromSeconds(totalSeconds);
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }


        /// <summary>
        /// 根据两个玩家经纬度，计算后者位于前者的哪个方位(8个方位：正前方、右前方、正右方、右后方、正后方、左后方、正左方、左前方)， 8个方位用数字代表，0~7
        /// </summary>
        /// <param name="self"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public static int GetEnemyMapPointDirection(Player self, Player enemy)
        {
            if (self == null || enemy == null)
            {
                Debug.LogWarning("AppUtils GetEnemyMapPointDirection: self 或 enemy 为空");
                return -1;
            }
            if (self.latitude == null || self.longitude == null || enemy.latitude == null || enemy.longitude == null)
            {
                Debug.LogWarning("AppUtils GetEnemyMapPointDirection: 经纬度为空");
                return -1;
            }
            if (self.degree == null)
            {
                Debug.LogWarning("AppUtils GetEnemyMapPointDirection: 自身朝向为空");
                return -1;
            }
            var selfLat = self.latitude.Value;
            var selfLng = self.longitude.Value;
            var enemyLat = enemy.latitude.Value;
            var enemyLng = enemy.longitude.Value;
            var dist = MapPointDistance(selfLat, selfLng, enemyLat, enemyLng);
            if (dist < 0.1f) return -1;
            var selfDegree = (self.degree.Value % 360 + 360) % 360;
            var bearing = MapPointBearing(selfLat, selfLng, enemyLat, enemyLng);
            var relative = (bearing - selfDegree + 360) % 360;
            return (int)((relative + 22.5) / 45) % 8;
        }

        private const string HUAWEI_PACKAGE = "com.huawei.arengine.service";
        private const string GOOGLE_PACKAGE = "com.google.ar.core";

        /// <summary>
        /// 检测apk是否安装
        /// </summary>
        /// <returns></returns>
        public static CheckResult CheckARCoreService()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return CheckResult.NotAndroid;
            }

            try
            {
                bool isHuaweiDevice = IsHuaweiOrHarmonyOS();
                string targetPackage = isHuaweiDevice ? HUAWEI_PACKAGE : GOOGLE_PACKAGE;
                bool isInstalled = IsPackageInstalled(targetPackage);

                if (isHuaweiDevice)
                {
                    return isInstalled
                        ? CheckResult.HuaweiPackageInstalled
                        : CheckResult.HuaweiPackageMissing;
                }
                else
                {
                    return isInstalled
                        ? CheckResult.GooglePackageInstalled
                        : CheckResult.GooglePackageMissing;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"检测失败: {e.Message}");
                return CheckResult.GooglePackageMissing;
            }
        }

        public static bool IsHuaweiOrHarmonyOS()
        {
            AndroidJavaClass buildClass = new AndroidJavaClass("android.os.Build");
            string manufacturer = buildClass.GetStatic<string>("MANUFACTURER").ToLower();

            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");

            bool isHarmonyOS = false;
            try { isHarmonyOS = activity.Call<bool>("isHarmonyOS"); }
            catch { /* 非鸿蒙系统 */ }

            return isHarmonyOS || manufacturer.Contains("huawei");
        }

        private static bool IsPackageInstalled(string packageName)
        {
            try
            {
                AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
                packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int GetEnemyMapPointDirectionOnline(Vector3 selfPos, Vector3 enemyPos, Vector3 selfForward)
        {
            // 计算敌人相对自身的8方向 (0前 1右前 2右 3右后 4后 5左后 6左 7左前)
            // 规则与 GetEnemyMapPointDirection 保持一致：顺时针递增
            var toEnemy = enemyPos - selfPos;
            // 忽略高度差，只按水平面判断方向
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude < 0.0001f) return -1; // 重合或过近

            var forward = selfForward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return -1; // 无效前向

            forward.Normalize();
            toEnemy.Normalize();

            // Unity 的 SignedAngle 为逆时针为正角度(-180~180)，我们需要顺时针角度(0~360)
            var ccwAngle = Vector3.SignedAngle(forward, toEnemy, Vector3.up); // 逆时针
            var cwAngle = (-ccwAngle + 360f) % 360f; // 转为顺时针
            var dirIndex = (int)((cwAngle + 22.5f) / 45f) % 8;
            return dirIndex;
        }

        /// <summary>
        /// 依据自身(lat,lng,朝向)与目标(lat,lng)计算目标相对自身的8方向 (0前 1右前 2右 3右后 4后 5左后 6左 7左前)
        /// 返回-1表示无效(点重合或参数异常)。 selfDegree: 0=正北, 顺时针递增。
        /// </summary>
        public static int GetEnemyMapPointDirection(double selfLat, double selfLng, double enemyLat, double enemyLng, double selfDegree, double samePosThresholdMeters = 0.1)
        {
            if (double.IsNaN(selfLat) || double.IsNaN(selfLng) || double.IsNaN(enemyLat) || double.IsNaN(enemyLng) || double.IsNaN(selfDegree))
                return -1;
            var dist = MapPointDistance(selfLat, selfLng, enemyLat, enemyLng);
            if (dist < samePosThresholdMeters) return -1;
            selfDegree = (selfDegree % 360 + 360) % 360;
            var bearing = MapPointBearing(selfLat, selfLng, enemyLat, enemyLng); // 相对正北
            var relative = (bearing - selfDegree + 360) % 360; // 相对自身前向
            return (int)((relative + 22.5) / 45) % 8;
        }

        /// <summary>
        /// 根据传入的经纬度，生成一个偏移指定米数的坐标点
        /// </summary>
        /// <param name="lat">lat</param>
        /// <param name="lng">lng</param>
        /// <param name="offsetX">x偏移 单位：米</param>
        /// <param name="offsetY">y偏移 单位：米</param>
        /// <returns>偏移后的经纬度坐标(lat,lng) {lat, lng}</returns>
        public static (double, double) GetMapPointByOffset(double lat, double lng, double offsetX = 0, double offsetY = 0)
        {
            const double earthRadius = 6378137; // 地球半径，单位：米

            // 将经纬度转换为弧度
            var lat2 = lat * (Math.PI / 180);
            var lng2 = lng * (Math.PI / 180);

            // 计算纬度偏移
            var newLat = lat2 + (offsetY / earthRadius);

            // 计算经度偏移
            var newLng = lng2 + (offsetX / (earthRadius * Math.Cos(lat2)));

            // 将弧度转换回经纬度
            return (newLat * (180 / Math.PI), newLng * (180 / Math.PI));
        }

        // 地球半径（单位：米）
        private const double EarthRadius = 6371000;

        // 将角度转换为弧度
        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Haversine公式计算两点之间的距离
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public static double MapPointDistance(double lat1, double lng1, double lat2, double lng2)
        {
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLng = DegreesToRadians(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadius * c;
            return distance;
        }

        /// <summary>
        /// 计算有经纬度点所围成的多边形区域，在包围它的最小四边形区域的长和宽
        /// </summary>
        /// <param name="mapPoints"></param>
        /// <returns></returns>
        public static (double, double) GetMapPointsAreaSize(List<(double, double)> mapPoints)
        {
            if (mapPoints == null || mapPoints.Count < 2)
                return (0, 0);

            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLng = double.MaxValue, maxLng = double.MinValue;

            foreach (var pt in mapPoints)
            {
                if (pt.Item1 < minLat) minLat = pt.Item1;
                if (pt.Item1 > maxLat) maxLat = pt.Item1;
                if (pt.Item2 < minLng) minLng = pt.Item2;
                if (pt.Item2 > maxLng) maxLng = pt.Item2;
            }

            // 计算外接矩形的宽和高（实际距离，单位米）
            double width = MapPointDistance(minLat, minLng, minLat, maxLng);
            double height = MapPointDistance(minLat, minLng, maxLat, minLng);
            return (width, height);
        }

        /// <summary>
        /// 计算有经纬度点所围成的多边形区域的中心点经纬度
        /// </summary>
        /// <param name="mapPoints"></param>
        /// <returns>(lat,lng)</returns>
        public static (double, double) GetMapPointsCenter(List<(double, double)> mapPoints)
        {
            if (mapPoints.Count <= 2)
            {
                Debug.LogWarning("GetMapPointsCenter mapPoints count <= 2, return first point");
                return (mapPoints[0].Item1, mapPoints[0].Item2);
            }
            // 计算所有点的经纬度平均值
            double sumLat = 0, sumLng = 0;
            foreach (var pt in mapPoints)
            {
                sumLat += pt.Item1;
                sumLng += pt.Item2;
            }
            double centerLat = sumLat / mapPoints.Count;
            double centerLng = sumLng / mapPoints.Count;
            return (centerLat, centerLng);
        }

        /// <summary>
        /// 检查任意两个圆心点之间，圆心距离小于直径时返回 false，否则返回true
        /// </summary>
        /// <param name="mapPoints">圆心点坐标</param>
        /// <param name="radius">圆的半径</param>
        /// <returns></returns>
        public static bool CheckPointDistance(List<(double, double)> mapPoints, float radius)
        {
            if (mapPoints == null || mapPoints.Count < 2)
                return true;
            double minDistance = double.MaxValue;
            for (int i = 0; i < mapPoints.Count - 1; i++)
            {
                for (int j = i + 1; j < mapPoints.Count; j++)
                {
                    double dist = MapPointDistance(mapPoints[i].Item1, mapPoints[i].Item2, mapPoints[j].Item1, mapPoints[j].Item2);
                    if (dist < 2 * radius)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 计算有经纬度点所围成的多边形区域，在包围它的最小四边形区域面积(米)
        /// </summary>
        /// <param name="mapPoints"></param>
        /// <returns></returns>
        public static int GetMapPointsArea(List<(double, double)> mapPoints)
        {
            if (mapPoints == null || mapPoints.Count < 2)
                return 0;

            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLng = double.MaxValue, maxLng = double.MinValue;

            foreach (var pt in mapPoints)
            {
                if (pt.Item1 < minLat) minLat = pt.Item1;
                if (pt.Item1 > maxLat) maxLat = pt.Item1;
                if (pt.Item2 < minLng) minLng = pt.Item2;
                if (pt.Item2 > maxLng) maxLng = pt.Item2;
            }

            // 计算外接矩形的宽和高（实际距离，单位米）
            double width = MapPointDistance(minLat, minLng, minLat, maxLng);
            double height = MapPointDistance(minLat, minLng, maxLat, minLng);

            return (int)(width * height);
        }

        /// <summary>
        /// 检查所有的经纬度点的连线是否有交叉点
        /// </summary>
        /// <param name="mapPoints"></param>
        /// <returns>true表示有交叉点（多边形自相交），false表示无交叉点</returns>
        public static bool CheckMapPointIntersect(List<(double, double)> mapPoints)
        {
            if (mapPoints == null || mapPoints.Count < 4)
                return false; // 少于4个点不可能形成自相交的多边形

            int n = mapPoints.Count;

            // 检查所有线段对是否相交
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 2; j < n; j++)
                {
                    // 跳过相邻线段的检查（相邻线段在端点处相交是正常的）
                    if (i == 0 && j == n - 1) continue; // 第一条线段和最后一条线段也是相邻的

                    var p1 = mapPoints[i];
                    var p2 = mapPoints[(i + 1) % n];
                    var p3 = mapPoints[j];
                    var p4 = mapPoints[(j + 1) % n];

                    if (DoLinesIntersect(p1, p2, p3, p4))
                    {
                        return true; // 发现交叉点
                    }
                }
            }

            return false; // 没有交叉点
        }

        /// <summary>
        /// 判断两条线段是否相交
        /// </summary>
        /// <param name="p1">线段1的起点</param>
        /// <param name="p2">线段1的终点</param>
        /// <param name="p3">线段2的起点</param>
        /// <param name="p4">线段2的终点</param>
        /// <returns>true表示相交，false表示不相交</returns>
        private static bool DoLinesIntersect((double, double) p1, (double, double) p2, (double, double) p3, (double, double) p4)
        {
            double x1 = p1.Item1, y1 = p1.Item2;
            double x2 = p2.Item1, y2 = p2.Item2;
            double x3 = p3.Item1, y3 = p3.Item2;
            double x4 = p4.Item1, y4 = p4.Item2;

            // 计算方向
            double d1 = GetDirection(x3, y3, x4, y4, x1, y1);
            double d2 = GetDirection(x3, y3, x4, y4, x2, y2);
            double d3 = GetDirection(x1, y1, x2, y2, x3, y3);
            double d4 = GetDirection(x1, y1, x2, y2, x4, y4);

            // 检查一般情况：两条线段相交
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }

            // 检查特殊情况：点在线段上
            if (Math.Abs(d1) < 1e-10 && IsOnSegment(x3, y3, x4, y4, x1, y1)) return true;
            if (Math.Abs(d2) < 1e-10 && IsOnSegment(x3, y3, x4, y4, x2, y2)) return true;
            if (Math.Abs(d3) < 1e-10 && IsOnSegment(x1, y1, x2, y2, x3, y3)) return true;
            if (Math.Abs(d4) < 1e-10 && IsOnSegment(x1, y1, x2, y2, x4, y4)) return true;

            return false;
        }

        /// <summary>
        /// 计算点相对于直线的方向
        /// </summary>
        /// <param name="x1">直线起点x</param>
        /// <param name="y1">直线起点y</param>
        /// <param name="x2">直线终点x</param>
        /// <param name="y2">直线终点y</param>
        /// <param name="x">点x</param>
        /// <param name="y">点y</param>
        /// <returns>正数表示左侧，负数表示右侧，0表示在直线上</returns>
        private static double GetDirection(double x1, double y1, double x2, double y2, double x, double y)
        {
            return (x2 - x1) * (y - y1) - (y2 - y1) * (x - x1);
        }

        /// <summary>
        /// 判断点是否在线段上
        /// </summary>
        /// <param name="x1">线段起点x</param>
        /// <param name="y1">线段起点y</param>
        /// <param name="x2">线段终点x</param>
        /// <param name="y2">线段终点y</param>
        /// <param name="x">点x</param>
        /// <param name="y">点y</param>
        /// <returns>true表示点在线段上</returns>
        private static bool IsOnSegment(double x1, double y1, double x2, double y2, double x, double y)
        {
            return Math.Min(x1, x2) <= x && x <= Math.Max(x1, x2) &&
                   Math.Min(y1, y2) <= y && y <= Math.Max(y1, y2);
        }

        /// <summary>
        /// 检查所有经纬度点所围成的多边形区域，在包围它的最小四边形区域的最短边，不能小于50米
        /// </summary>
        /// <param name="mapPoints">(lat,lng)列表</param>
        /// <returns></returns>
        public static bool CheckMapPointMinDiameter(List<(double, double)> mapPoints)
        {
            if (mapPoints == null || mapPoints.Count < 2)
                return false;

            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLng = double.MaxValue, maxLng = double.MinValue;

            foreach (var pt in mapPoints)
            {
                if (pt.Item1 < minLat) minLat = pt.Item1;
                if (pt.Item1 > maxLat) maxLat = pt.Item1;
                if (pt.Item2 < minLng) minLng = pt.Item2;
                if (pt.Item2 > maxLng) maxLng = pt.Item2;
            }

            // 计算外接矩形的宽和高（实际距离，单位米）
            double width = MapPointDistance(minLat, minLng, minLat, maxLng);
            double height = MapPointDistance(minLat, minLng, maxLat, minLng);

            double minEdge = Math.Min(width, height);

            return minEdge >= 50.0;
        }

        /// <summary>
        /// 计算方位角
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public static double MapPointBearing(double lat1, double lng1, double lat2, double lng2)
        {
            var dLng = DegreesToRadians(lng2 - lng1);
            var y = Math.Sin(dLng) * Math.Cos(DegreesToRadians(lat2));
            var x = Math.Cos(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) -
                    Math.Sin(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * Math.Cos(dLng);
            var bearing = Math.Atan2(y, x);
            return (bearing * 180 / Math.PI + 360) % 360; // 转换为正角度
        }

        /// <summary>
        /// 根据方位角和距离计算新的坐标点
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="bearing"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static (double lat, double lng) MapPointCalculateNewCoordinate(double lat, double lng, double bearing, double distance)
        {
            var angularDistance = distance / EarthRadius;
            var latRad = DegreesToRadians(lat);
            var lngRad = DegreesToRadians(lng);
            var bearingRad = DegreesToRadians(bearing);

            var newLatRad = Math.Asin(Math.Sin(latRad) * Math.Cos(angularDistance) +
                                      Math.Cos(latRad) * Math.Sin(angularDistance) * Math.Cos(bearingRad));

            var newLngRad = lngRad + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(angularDistance) * Math.Cos(latRad),
                                               Math.Cos(angularDistance) - Math.Sin(latRad) * Math.Sin(newLatRad));

            var newLat = newLatRad * 180 / Math.PI;
            var newLng = newLngRad * 180 / Math.PI;

            return (newLat, newLng);
        }

        /// <summary>
        /// 限制坐标点在圆形范围内
        /// </summary>
        /// <param name="centerLat"></param>
        /// <param name="centerLng"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static (double lat, double lng) MapPointLimitCoordinateToRadius(double centerLat, double centerLng, double lat, double lng, double radius)
        {
            var distance = MapPointDistance(centerLat, centerLng, lat, lng);
            Debug.LogWarning("MapPointLimitCoordinateToRadius 距离中心点:" + distance + "米 半径:" + radius);
            if (distance > radius)
            {
                var bearing = MapPointBearing(centerLat, centerLng, lat, lng);
                return MapPointCalculateNewCoordinate(centerLat, centerLng, bearing, radius);
            }

            return (lat, lng);
        }

        public static string GetMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 申请摄像头权限
        /// </summary>
        public static void RequestWebCamPermission(Action<bool> callback)
        {
            UniTask.Post(async () =>
            {
                callback(await RequestWebCamPermissionAsync());
            });
        }

        /// <summary>
        /// 申请摄像头权限
        /// </summary>
        /// <returns></returns>
        public static async UniTask<bool> RequestWebCamPermissionAsync()
        {
            Debug.Log("WebCamLogic 开始申请摄像头权限...");
#if UNITY_ANDROID && !UNITY_EDITOR
            if(UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera)){
                Debug.Log("WebCamLogic 摄像头权限已授权。不再申请...");
            }else{
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            }
#else 
            await Application.RequestUserAuthorization(UserAuthorization.WebCam);
#endif
            await UniTask.WaitForSeconds(0.1f);
            var devices = WebCamTexture.devices;
            //等待设备列表更新
            if (devices.Length == 0)
            {
                Debug.Log("WebCamLogic 摄像头权限申请失败 ，设备列表为空!");
                return false;
            }
            Debug.Log("WebCamLogic 摄像头权限申请成功!");
            return true;
        }

        /// <summary>
        /// 是否有摄像头权限
        /// </summary>
        /// <returns></returns>
        public static bool HasWebCamPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
#else
            return Application.HasUserAuthorization(UserAuthorization.WebCam);
#endif

        }
        
        /// <summary>
        /// 载入图片并根据 JPEG EXIF Orientation 进行旋转（轻量级实现，无第三方依赖）。
        /// 不支持的或无 EXIF 情况下视为 orientation=1（不旋转）。
        /// </summary>
        /// <param name="data">图片二进制（建议 JPEG）</param>
        public static Texture2D LoadTextureWithOrientation(byte[] data)
        {
            int orientation = ReadJpegOrientation(data); // 仅解析 JPEG APP1 Exif 的 0x0112 标签

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(data);
            if (orientation == 1) return tex;
            return ApplyOrientation(tex, orientation);
        }

        // 调试/外部日志使用：公开读取 JPEG Orientation 的入口（1~8，失败返回1）
        public static int GetJpegOrientation(byte[] data)
        {
            return ReadJpegOrientation(data);
        }

        // 解析 JPEG 中的 EXIF Orientation（1~8）。失败/不支持时返回1。
        private static int ReadJpegOrientation(byte[] data)
        {
            try
            {
                if (data == null || data.Length < 4) return 1;
                // JPEG SOI
                if (data[0] != 0xFF || data[1] != 0xD8) return 1;
                int index = 2;
                while (index + 3 < data.Length)
                {
                    if (data[index] != 0xFF) break; // 非法结构
                    byte marker = data[index + 1];
                    index += 2;
                    // SOS(0xDA) 或 EOI(0xD9) 结束
                    if (marker == 0xDA || marker == 0xD9) break;
                    if (index + 2 > data.Length) break;
                    int segLen = (data[index] << 8) | data[index + 1];
                    if (segLen < 2 || index + segLen > data.Length) break;

                    // APP1 (Exif)
                    if (marker == 0xE1 && segLen >= 10)
                    {
                        int exifHeader = index + 2; // 跳过长度字段
                        // 检查 "Exif\0\0"
                        if (exifHeader + 6 <= data.Length &&
                            data[exifHeader] == (byte)'E' && data[exifHeader + 1] == (byte)'x' &&
                            data[exifHeader + 2] == (byte)'i' && data[exifHeader + 3] == (byte)'f' &&
                            data[exifHeader + 4] == 0 && data[exifHeader + 5] == 0)
                        {
                            int tiffStart = exifHeader + 6;
                            return ParseTiffForOrientation(data, tiffStart);
                        }
                    }
                    index += segLen;
                }
            }
            catch { /* 忽略异常，按无旋转处理 */ }
            return 1;
        }

        // 在给定的 TIFF 起始位置解析 IFD0，读取 Orientation(0x0112)
        private static int ParseTiffForOrientation(byte[] data, int tiffStart)
        {
            if (tiffStart + 8 > data.Length) return 1;

            bool littleEndian;
            ushort byteOrder = (ushort)((data[tiffStart] << 8) | data[tiffStart + 1]);
            if (byteOrder == 0x4949) littleEndian = true;          // 'II'
            else if (byteOrder == 0x4D4D) littleEndian = false;    // 'MM'
            else return 1;

            ushort ReadU16(int offset)
            {
                if (offset + 1 >= data.Length) return 0;
                return littleEndian
                    ? (ushort)(data[offset] | (data[offset + 1] << 8))
                    : (ushort)((data[offset] << 8) | data[offset + 1]);
            }

            uint ReadU32(int offset)
            {
                if (offset + 3 >= data.Length) return 0;
                return littleEndian
                    ? (uint)(data[offset] |
                              (data[offset + 1] << 8) |
                              (data[offset + 2] << 16) |
                              (data[offset + 3] << 24))
                    : (uint)((data[offset] << 24) |
                              (data[offset + 1] << 16) |
                              (data[offset + 2] << 8) |
                              data[offset + 3]);
            }

            // 固定 0x002A
            if (ReadU16(tiffStart + 2) != 0x002A) return 1;
            uint ifd0Offset = ReadU32(tiffStart + 4);
            int ifd0Pos = tiffStart + (int)ifd0Offset;
            if (ifd0Pos + 2 > data.Length) return 1;
            ushort entryCount = ReadU16(ifd0Pos);
            int entryBase = ifd0Pos + 2;
            for (int i = 0; i < entryCount; i++)
            {
                int entry = entryBase + i * 12;
                if (entry + 11 >= data.Length) break;
                ushort tag = ReadU16(entry);
                if (tag == 0x0112) // Orientation
                {
                    ushort value = ReadU16(entry + 8); // SHORT 且 count=1 时直接内嵌
                    if (value >= 1 && value <= 8) return value;
                    return 1;
                }
            }
            return 1;
        }

        static Texture2D ApplyOrientation(Texture2D src, int orientation)
        {
            // orientation 1: 原样；2-8 需处理（EXIF 标准）
            if (orientation < 2 || orientation > 8)
                return src;

            int w = src.width;
            int h = src.height;
            var srcPixels = src.GetPixels32();

            bool swap = orientation is 5 or 6 or 7 or 8;
            int dstW = swap ? h : w;
            int dstH = swap ? w : h;
            var dstPixels = new Color32[dstW * dstH];

            try
            {
                for (int y = 0; y < dstH; y++)
                {
                    int dstRow = y * dstW;
                    for (int x = 0; x < dstW; x++)
                    {
                        int srcX, srcY;
                        switch (orientation)
                        {
                            case 2: // 水平翻转
                                srcX = w - 1 - x; srcY = y; break;
                            case 3: // 180°
                                srcX = w - 1 - x; srcY = h - 1 - y; break;
                            case 4: // 垂直翻转
                                srcX = x; srcY = h - 1 - y; break;
                            case 5: // 过主对角线 (transpose)
                                srcX = y; srcY = x; break;
                            case 6: // 90° CW
                                srcX = y; srcY = h - 1 - x; break;
                            case 7: // 过副对角线 (transverse)
                                srcX = w - 1 - y; srcY = h - 1 - x; break;
                            case 8: // 270° CW (90° CCW)
                                srcX = w - 1 - y; srcY = x; break;
                            default:
                                srcX = x; srcY = y; break;
                        }

                        if ((uint)srcX >= (uint)w || (uint)srcY >= (uint)h)
                            continue;
                        dstPixels[dstRow + x] = srcPixels[srcY * w + srcX];
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ApplyOrientation 处理失败 orientation={orientation} 回退原图: {e.Message}");
                return src;
            }

            var dst = new Texture2D(dstW, dstH, TextureFormat.RGBA32, false);
            dst.SetPixels32(dstPixels);
            dst.Apply();
            UnityEngine.Object.Destroy(src);
            return dst;
        }
        
        /// <summary>
        /// 负数表示第一个版本号小于第二个，零表示相等，正数表示第一个大于第二个
        /// </summary>
        /// <param name="versionA"></param>
        /// <param name="versionB"></param>
        /// <returns></returns>
        public static int CompareVersions(string versionA, string versionB)
        {
            var partsA = versionA.Split('.');
            var partsB = versionB.Split('.');

            for (var i = 0; i < Math.Max(partsA.Length, partsB.Length); i++)
            {
                var partA = i < partsA.Length ? int.Parse(partsA[i]) : 0;
                var partB = i < partsB.Length ? int.Parse(partsB[i]) : 0;

                if (partA > partB)
                {
                    return 1;
                }
                else if (partA < partB)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
    
    public class ImagePicker : SingletonComponent<ImagePicker>
    {
        public enum ImagePickError
        {
            NoPermission,
            NoImagePath,
            TextureLoadError,
            Success
        }

        public class ImagePickResult
        {
            public Texture2D texture;
            public ImagePickError error;

            public ImagePickResult(Texture2D texture, ImagePickError error)
            {
                this.texture = texture;
                this.error = error;
            }
        }
        
        public class ImagePathPickResult
        {
            public string path;
            public ImagePickError error;

            public ImagePathPickResult(string path, ImagePickError error)
            {
                this.path = path;
                this.error = error;
            }
        }

        /// <summary>
        /// 从相册选择图片
        /// </summary>
        /// <param name="callback"></param>
        public void PickImage(Action<ImagePickResult> callback)
        {
            PickImageAsync(callback).Forget();
        }
        
        /// <summary>
        /// 从相册选择图片
        /// </summary>
        /// <returns></returns>
        public async UniTask PickImageAsync(Action<ImagePickResult> callback, int maxSize = 1024) {
            Debug.Log("ImagePicker PickImage...");
            //检查权限
            var permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
            if (permission != NativeGallery.Permission.Granted)
            {
                Debug.Log("ImagePicker 申请权限...");
                var requestResult = await NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
                if (requestResult != NativeGallery.Permission.Granted)
                {
                    callback?.Invoke(new ImagePickResult(null, ImagePickError.NoPermission));
                    return;
                }
            }
            
            var resp = NativeGallery.GetImageFromGallery(path =>
            {
                UniTask.Post(async () =>
                {
                    Debug.Log("ImagePicker NativeGallery.GetImageFromGallery path: " + path);
                    if (path == null)
                    {
                        Debug.LogError("ImagePicker PickImage path 路径为空!");
                        callback?.Invoke(new ImagePickResult(null, ImagePickError.NoImagePath));
                        return;
                    }
            
                    var texture = await NativeGallery.LoadImageAtPathAsync(path, maxSize );
                    if( texture == null )
                    {
                        Debug.Log( "ImagePicker 无法加载纹理 " + path );
                        callback?.Invoke(new ImagePickResult(null, ImagePickError.TextureLoadError));
                        return;
                    }
                    Debug.Log("ImagePicker LoadImageAtPathAsync 图片格式: " + texture.format+" 像素格式:+"+texture.graphicsFormat);
                    Debug.Log("ImagePicker LoadImageAtPathAsync 成功, 图片大小: " + texture.width + "x" + texture.height);
                    var readableTexture = AppUtils.MakeTextureReadable(texture);
                    Destroy(texture);
                    Debug.Log("ImagePicker 通知回调函数 大小: " + readableTexture.width + "x" + readableTexture.height);
                    callback?.Invoke(new ImagePickResult(readableTexture, ImagePickError.Success));
                });
            } );
            
            //等待getImageFromGalleryComplete=true,最多等待5秒
            if (resp != NativeGallery.Permission.Granted)
            {
                Debug.LogError("ImagePicker PickImage 没有权限!");
                callback?.Invoke(new ImagePickResult(null, ImagePickError.NoPermission));
            }
        }
        
        /// <summary>
        /// 从相册选择图片路径
        /// </summary>
        /// <returns></returns>
        public async UniTask PickImagePathAsync(Action<ImagePathPickResult> callback) {
            Debug.Log("ImagePicker PickImagePathAsync...");
            //检查权限
            var permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
            if (permission != NativeGallery.Permission.Granted)
            {
                Debug.Log("ImagePicker PickImagePathAsync 申请权限...");
                var requestResult = await NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
                if (requestResult != NativeGallery.Permission.Granted)
                {
                    callback?.Invoke(new ImagePathPickResult(null, ImagePickError.NoPermission));
                    return;
                }
            }
            
            var resp = NativeGallery.GetImageFromGallery(path =>
            {
                
                if (path == null)
                {
                    Debug.LogError("ImagePicker PickImage path 路径为空!");
                    callback?.Invoke(new ImagePathPickResult(null, ImagePickError.NoImagePath));
                    return;
                }
                
                callback?.Invoke(new ImagePathPickResult(path, ImagePickError.Success));
            } );
            
            //等待getImageFromGalleryComplete=true,最多等待5秒
            if (resp != NativeGallery.Permission.Granted)
            {
                Debug.LogError("ImagePicker PickImage 没有权限!");
                callback?.Invoke(new ImagePathPickResult(null, ImagePickError.NoPermission));
            }
        }
    }
    
    public class WebImageLoader : MonoBehaviour
    {
        private Queue<string> urlQueue;  // 存储图片 URL 的队列
        private Dictionary<string, Texture2D> imageCache; // 图片缓存字典
        private Dictionary<string, Texture2D> squareImageCache; // 居中裁剪为正方形后的头像缓存字典
        private Dictionary<string, Sprite> squareSpriteCache; // 头像 Sprite 缓存，避免反复 Sprite.Create 泄漏 Native 内存
        public int maxQueueSize = 80; // 队列的最大大小
        private TextureLoader _textureLoader;
        private static WebImageLoader instance;
        
        class TextureLoaderRequest
        {
            public readonly string url;
            public readonly Action<(string, Texture2D, string)> action;

            public TextureLoaderRequest(string url, Action<(string, Texture2D, string)> action)
            {
                this.url = url;
                this.action = action;
            }
        }

        class TextureLoader
        {
            private readonly Queue<TextureLoaderRequest> requestQueue;  // 用于存储等待下载的请求URL
            private readonly int maxConcurrentRequests;  // 最大同时下载请求数量
            private int currentRequests;  // 当前正在进行的下载请求数量
            private readonly WebImageLoader _webImageLoader;

            public TextureLoader(WebImageLoader monoBehaviour, int maxConcurrentRequests)
            {
                this.maxConcurrentRequests = maxConcurrentRequests;
                requestQueue = new Queue<TextureLoaderRequest>();
                _webImageLoader = monoBehaviour;
            }

            public TextureLoaderRequest QueryRequest(string url)
            {
                return requestQueue.FirstOrDefault(req => req.url.Equals(url));
            }

            // 添加下载请求到队列中
            public void AddRequest(TextureLoaderRequest request)
            {
                requestQueue.Enqueue(request);
                TryStartNextRequest();
            }

            // 尝试开始下一个下载请求
            private void TryStartNextRequest()
            {
                if (currentRequests >= maxConcurrentRequests || requestQueue.Count <= 0) return;
                var request = requestQueue.Dequeue();
                _webImageLoader.StartCoroutine(DownloadTexture(request));
            }

            // 下载图片的协程方法
            private IEnumerator DownloadTexture(TextureLoaderRequest req)
            {
                currentRequests++;
                
                var localFilePath = Path.Combine(Application.persistentDataPath, GetMD5Hash(req.url)+".png");

                if (_webImageLoader.imageCache.ContainsKey(req.url))
                {
                    req.action.Invoke((req.url, _webImageLoader.imageCache[req.url], null));
                }
                else
                {
                    //首先从文件读取图片
                    var loadFromFileSuccess = false;
                    using var fileRequest = UnityWebRequestTexture.GetTexture(localFilePath);
                    yield return fileRequest.SendWebRequest();
                    if (fileRequest.result == UnityWebRequest.Result.Success)
                    {
                        var texture = DownloadHandlerTexture.GetContent(fileRequest);
                        if (texture != null)
                        {
                            Debug.Log("WebImageLoader 图片文件读取成功:"+req.url+"\n"+localFilePath);
                            req.action.Invoke((req.url, texture, null));
                            loadFromFileSuccess = true;
                        }
                    }
                    if (!loadFromFileSuccess)
                    {
                        using var request = UnityWebRequest.Get(req.url);
                        yield return request.SendWebRequest();

                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            var texture = CreateTexture(request.downloadHandler.data, req.url);
                            if (texture != null)
                            {
                                Debug.Log("WebImageLoader 图片下载成功:"+req.url);
                                texture = ResizeTexture(texture, 512);
                                if (texture.mipmapCount == 1)
                                {
                                    texture.Apply(true);
                                }
                                texture = FixTextureForCompression(texture);
                                texture.Compress(true);
                                texture.Apply(false, true);
                            }
                            req.action.Invoke((req.url, texture, null));
                        }
                        else
                        {
                            req.action.Invoke((req.url, null, request.error));
                        }
                    }
                }

                currentRequests--;
                TryStartNextRequest();
            }
            static Texture2D CreateTexture(byte[] data, string url)
            {
                if (data == null || data.Length == 0)
                {
                    return null;
                }

                if (GifFrameDecoder.IsGif(data))
                {
                    var frame = GifFrameDecoder.DecodeVisibleFrame(data);
                    if (frame != null)
                    {
                        Debug.Log("WebImageLoader GIF 已取一帧: " + url + " " + frame.width + "x" + frame.height);
                        return frame;
                    }

                    Debug.LogWarning("WebImageLoader GIF 解码失败: " + url);
                    return null;
                }

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(data))
                {
                    UnityEngine.Object.Destroy(texture);
                    return null;
                }

                return texture;
            }

            private Texture2D FixTextureForCompression(Texture2D texture)
            {
                int width = texture.width;
                int height = texture.height;

                int newWidth = Mathf.CeilToInt(width / 4f) * 4;
                int newHeight = Mathf.CeilToInt(height / 4f) * 4;

                if (newWidth == width && newHeight == height)
                    return texture; // 宽高已经是4的倍数，无需调整

                RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;

                Texture2D newTex = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, true);
                newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
                newTex.Apply(true); // 生成 mipmaps

                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);

                UnityEngine.Object.Destroy(texture); // 销毁旧纹理

                return newTex;
            }

            /// <summary>
            /// 缩放纹理到最大边长 maxSize，保持宽高比
            /// </summary>
            private Texture2D ResizeTexture(Texture2D source, int maxSize)
            {
                if (source == null) return null;

                int width = source.width;
                int height = source.height;

                // 如果已经小于等于 maxSize，则直接返回
                if (width <= maxSize && height <= maxSize)
                {
                    //Debug.Log($"[bdl_ResizeTexture] 无需缩放: {width}x{height}");
                    return source;
                }

                float scale = Mathf.Min((float)maxSize / width, (float)maxSize / height);
                int newWidth = Mathf.RoundToInt(width * scale);
                int newHeight = Mathf.RoundToInt(height * scale);

                Debug.Log($"[bdl_ResizeTexture] 缩放 {width}x{height} -> {newWidth}x{newHeight}");

                RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
                rt.filterMode = FilterMode.Bilinear;

                Graphics.Blit(source, rt);
                RenderTexture.active = rt;

                Texture2D newTex = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, true);
                newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
                newTex.Apply(true); // 生成 mipmaps

                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);

                UnityEngine.Object.Destroy(source); // 销毁旧贴图，释放内存

                Debug.Log($"[bdl_ResizeTexture] 完成: {newTex.width}x{newTex.height}");
                return newTex;
            }


            private static IEnumerator WriteToFileCoroutine(string path, byte[] content)
            {
                var task = Task.Run(() => File.WriteAllBytes(path, content));
                yield return new WaitUntil(() => task.IsCompleted);
                Debug.Log("WebImageLoader 图片写入成功:"+path+" 字节数:"+content);
            }
            
            /// <summary>
            /// 获取字符串的MD5哈希值（16进制字符串形式）
            /// </summary>
            /// <param name="input">输入字符串</param>
            /// <returns>16进制字符串形式的MD5哈希值</returns>
            private static string GetMD5Hash(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    throw new ArgumentNullException(nameof(input), "Input cannot be null or empty.");
                }

                using var md5 = MD5.Create();
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // 将字节数组转换为16进制字符串
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        
        private void Awake()
        {
            urlQueue = new Queue<string>();
            imageCache = new Dictionary<string, Texture2D>();
            squareImageCache = new Dictionary<string, Texture2D>();
            squareSpriteCache = new Dictionary<string, Sprite>();
            _textureLoader = new TextureLoader(this, 3);
        }

        // 添加图片 URL 到队列
        public void LoadImage(string url, Action<Texture2D> callback)
        {
            if (url == null)
            {
                callback.Invoke(null);
                return;
            }
            
            if (imageCache.TryGetValue(url, out var value))
            {
                if (value is not null)
                {
                    callback.Invoke(value);
                    return;
                }
            }
            
            //图片正在加载中
            if (urlQueue.Contains(url))
            {
                _textureLoader.AddRequest(new TextureLoaderRequest(url, action =>
                {
                    var (_, texture, _) = action;
                    callback.Invoke(texture);
                }));
                return;
            }
            
            urlQueue.Enqueue(url);
            if (urlQueue.Count > maxQueueSize)
            {
                var removedUrl = urlQueue.Dequeue();
                imageCache.Remove(removedUrl);
            }
            
            _textureLoader.AddRequest(new TextureLoaderRequest(url, action =>
            {
                var (_, texture, _) = action;
                // 将下载的图片添加到缓存字典
                if (texture != null)
                {
                    imageCache.TryAdd(url, texture);
                }
                callback.Invoke(texture);
            }));
        }

        /// <summary>
        /// 加载头像并预处理为正方形：下载后先在内存中判断原图是否正方形，
        /// 非正方形则居中裁剪出最大的正方形（不写入磁盘），裁剪结果按 url 单独缓存后再回调给 UI。
        /// 比分牌等所有玩家头像应统一走此方法，避免因原图比例不一导致头像拉伸变形。
        /// </summary>
        public void LoadSquareImage(string url, Action<Texture2D> callback)
        {
            if (url == null)
            {
                callback?.Invoke(null);
                return;
            }

            if (squareImageCache.TryGetValue(url, out var cachedSquare) && cachedSquare != null)
            {
                callback?.Invoke(cachedSquare);
                return;
            }

            LoadImage(url, tex =>
            {
                if (tex == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                Texture2D square = CenterCropToSquare(tex);
                if (square != null)
                {
                    squareImageCache[url] = square;
                }
                callback?.Invoke(square);
            });
        }

        /// <summary>
        /// 与 <see cref="LoadSquareImage"/> 相同，但回调直接给出可赋给 Image 的 Sprite，并按 url 缓存。
        ///
        /// Sprite.Create 产生的是 Unity 原生对象，托管 GC 不会回收，必须显式 Destroy 才释放。
        /// 战斗页每收到一条 SHOOT 就会重新 Bind 全部玩家卡片，若每次都新建 Sprite，
        /// Native 堆会只增不减（实测连续投屏 S4 两天涨了约 900MB）。
        /// 所有头像 Image 都应走这里，不要自己 Sprite.Create。
        /// </summary>
        public void LoadSquareSprite(string url, Action<Sprite> callback)
        {
            if (url == null)
            {
                callback?.Invoke(null);
                return;
            }

            if (squareSpriteCache.TryGetValue(url, out var cachedSprite) && cachedSprite != null)
            {
                callback?.Invoke(cachedSprite);
                return;
            }

            LoadSquareImage(url, tex =>
            {
                if (tex == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                if (!squareSpriteCache.TryGetValue(url, out var sprite) || sprite == null)
                {
                    sprite = Sprite.Create(
                        tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect);
                    squareSpriteCache[url] = sprite;
                }

                callback?.Invoke(sprite);
            });
        }

        /// <summary>
        /// 在内存中把纹理居中裁剪为最大正方形；已是正方形则原样返回。
        /// 用 RenderTexture + Blit 读取像素，避免原纹理经压缩/标记不可读导致 GetPixels 报错。
        /// </summary>
        private Texture2D CenterCropToSquare(Texture2D source)
        {
            if (source == null) return null;
            if (source.width == source.height) return source;

            int size = Mathf.Min(source.width, source.height);
            int x = (source.width - size) / 2;
            int y = (source.height - size) / 2;

            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;

            Texture2D cropped = new Texture2D(size, size, TextureFormat.RGBA32, true);
            cropped.ReadPixels(new Rect(x, y, size, size), 0, 0);
            cropped.Apply(true);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return cropped;
        }
        
        // 添加图片 URL 到队列
        public void LoadImage2(string url, Action<string, Texture2D> callback)
        {
            if (url == null)
            {
                callback.Invoke(null, null);
                return;
            }
            
            if (imageCache.TryGetValue(url, out var value))
            {
                if (value is not null)
                {
                    callback.Invoke(url, value);
                    return;
                }
            }
            
            //图片正在加载中
            if (urlQueue.Contains(url))
            {
                _textureLoader.AddRequest(new TextureLoaderRequest(url, action =>
                {
                    var (_, texture, _) = action;
                    callback.Invoke(url, texture);
                }));
                return;
            }
            
            urlQueue.Enqueue(url);
            if (urlQueue.Count > maxQueueSize)
            {
                var removedUrl = urlQueue.Dequeue();
                imageCache.Remove(removedUrl);
            }
            
            _textureLoader.AddRequest(new TextureLoaderRequest(url, action =>
            {
                var (_, texture, _) = action;
                // 将下载的图片添加到缓存字典
                if (texture != null)
                {
                    imageCache.TryAdd(url, texture);
                }
                callback.Invoke(url, texture);
            }));
        }
        
        //下载图片，不获取内容
        public IEnumerator LoadImageAsync(string url)
        {
            var isCompleted = new[] {false };
            LoadImage(url, t =>
            {
                isCompleted[0] = true;
                _ = t;
            });
            yield return new WaitUntil(() => isCompleted[0]);
        }
        
        /// <summary>
        /// 删除内存中缓存的图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public void DeleteImage(string url)
        {
            if (!imageCache.TryGetValue(url, out var value)) return;
            if (value is null) return;
            imageCache.Remove(url);
            Destroy(value);
        }
        
        /// <summary>
        /// 异步下载图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void LoadImage(string url, Action<string, Texture2D> callback)
        {
            LoadImage(url, texture =>
            {
                callback?.Invoke(url, texture);
            });
        }
        
        /// <summary>
        /// 下载并显示图片
        /// </summary>
        /// <param name="url">图片url</param>
        /// <param name="rawImage">展示到的RawImage</param>
        /// <param name="defaultImage">加载失败默认图片</param>
        /// <returns></returns>
        public void DisplayImage(string url, RawImage rawImage, Texture defaultImage)
        {
            rawImage.texture = defaultImage;
            LoadImage(url, texture =>
            {
                if (texture != null)
                {
                    rawImage.texture = texture;   
                }
            });
        }
        
        /// <summary>
        /// 下载并显示图片
        /// </summary>
        /// <param name="url">图片url</param>
        /// <param name="rawImage">展示到的RawImage</param>
        /// <param name="defaultImage">加载失败默认图片</param>
        /// <returns></returns>
        public void DisplayImage(string url, RawImage rawImage, Texture2D defaultImage)
        {
            Debug.Log("WebImageLoader DisplayImage url="+url+" rawImage==null?"+(rawImage == null));
            rawImage.texture = defaultImage;
            LoadImage(url, texture =>
            {
                if (texture != null)
                {
                    rawImage.texture = texture;   
                }
            });
        }
        
        /// <summary>
        /// 下载并显示图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="rawImage"></param>
        /// <returns></returns>
        public void DisplayImage(string url, RawImage rawImage)
        {
            LoadImage(url, texture =>
            {
                if (texture == null || rawImage == null) return;
                try
                {
                    rawImage.texture = texture;
                }
                catch (Exception e)
                {
                    Debug.Log("WebImageLoader 图片显示失败:"+e);
                }
            });
        }
        
        public static WebImageLoader Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("ImageCacheQueue");
                instance = go.AddComponent<WebImageLoader>();
                DontDestroyOnLoad(go);
                return instance;
            }
        }

        /// <summary>诊断代码用：查询单例是否已存在，避免采样时意外触发首次创建。</summary>
        public static bool HasInstance => instance != null;

        /// <summary>诊断用：返回三个头像/图片缓存字典的当前条目数，供 MemoryDiagnosticsLogger 采样，不影响正常逻辑。</summary>
        public (int imageCache, int squareImageCache, int squareSpriteCache) GetCacheDebugCounts()
        {
            return (imageCache?.Count ?? 0, squareImageCache?.Count ?? 0, squareSpriteCache?.Count ?? 0);
        }
    }
}

