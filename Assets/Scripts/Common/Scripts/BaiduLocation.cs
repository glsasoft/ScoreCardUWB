using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App;
using Plugins;
using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// 百度定位
    /// </summary>
    public class BaiduLocation : SingletonComponent<BaiduLocation>
    {

        private Coroutine simulationGpsTask = null;
        
        private void OnEnable()
        {
            BDLocation.HeadingMessageAction += OnHeadingChanged;
            BDLocation.LocationMessageAction += OnLocationChanged;
            EventManager.Instance.AddEvent(EventMsg.StartBdLocation, StartLocation);
            EventManager.Instance.AddEvent(EventMsg.StopBdLocation, StopLocation);
        }

        private void OnDisable()
        {
            BDLocation.HeadingMessageAction -= OnHeadingChanged;
            BDLocation.LocationMessageAction -= OnLocationChanged;
            EventManager.Instance.RemoveEvent(EventMsg.StartBdLocation, StartLocation);
            EventManager.Instance.RemoveEvent(EventMsg.StopBdLocation, StopLocation);
        }

        /// <summary>
        /// 开始定位
        /// </summary>
        public void StartLocation()
        {
#if (UNITY_ANDROID || UNITY_IOS) && UNITY_EDITOR
            Debug.Log("BaiduLocation 开始模定位... ");
            if (simulationGpsTask != null)
            {
                StopCoroutine(simulationGpsTask);
            }
            simulationGpsTask = StartCoroutine(StartSimulateLocation());
            return;
#endif
            //获取定位授权
            BDLocation.RegisterHandler(); //iOS端需要注册定位服务
            Input.location.Start();
            Input.location.Stop();
            var info = Input.location.lastData;
            var isLocationAuthorized = Input.location.isEnabledByUser;
            Debug.Log("定位是否授权:"+isLocationAuthorized+" Location:"+info.latitude+","+info.longitude);
            if (!isLocationAuthorized)
            {
                EventManager.Instance.Broadcast(EventMsg.BdLocationStartFailed);
                return;
            }
            BDLocation.StartLocation();
            EventManager.Instance.Broadcast(EventMsg.BdLocationStarted);
        }
        
#if (UNITY_ANDROID || UNITY_IOS) && UNITY_EDITOR

        public static double SimulateLocationLat = 31.1664080521096;
        public static double SimulateLocationLng = 121.43979789596133;
        
        private IEnumerator StartSimulateLocation()
        {
            while (true)
            {
                // Debug.Log("BaiduLocation 更新模拟的位置 currentPos:"+currentPos.longitude+","+currentPos.latitude);
                OnLocationChanged(SimulateLocationLng+"", SimulateLocationLat+"");
                yield return new WaitForSeconds(2);
            }
        }
        
        /// <summary>
        /// 设置模拟的经纬度
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetSimulateLocation(double latitude, double longitude)
        {
            SimulateLocationLat = latitude;
            SimulateLocationLng = longitude;
        }
#endif

        private void OnHeadingChanged(string rotate)
        {
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                float fRotate = float.Parse(rotate);
                if (fRotate > 180)
                {
                    fRotate -= 180;
                }
                else {
                    fRotate += 180;
                }
                rotate = fRotate.ToString();
            }
        }
#endif
            #if UNITY_ANDROID
            UniTask.Post(async () =>
            {
                await UniTask.SwitchToMainThread();
                //头像背景角度调整
                var degree = float.Parse(rotate);
                if (Screen.orientation == ScreenOrientation.LandscapeRight)
                {
                    degree = -degree - 20;
                }
                else
                {
                    degree = -degree - 40;
                }
            });
            #endif
            // EventManager.Instance.Broadcast(EventMsg.BdRotationChanged,double.Parse(rotate));
        }

        private void OnLocationChanged(string lng, string lat)
        {
            if (lng == null || lat == null) return;
            if (!double.TryParse(lng, out var doubleLng) || !double.TryParse(lat, out var doubleLat))
            {
                return;
            }
            UniTask.Post(async () =>
            {
                await UniTask.SwitchToMainThread();
                EventManager.Instance.Broadcast(EventMsg.BdLocationChanged, new LatLng(doubleLng,doubleLat)); 
            });
        }

        /// <summary>
        /// 结束定位
        /// </summary>
        public void StopLocation()
        {
            BDLocation.StopLocation();
            EventManager.Instance.Broadcast(EventMsg.BdLocationStopped);
#if UNITY_ANDROID && UNITY_EDITOR
            if (simulationGpsTask != null)
            {
                StopCoroutine(simulationGpsTask);
            }
#endif
        }
        
    }
}
