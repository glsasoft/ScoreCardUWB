using System.Runtime.InteropServices;
using UnityEngine;

namespace Plugins.CameraFocus
{
    public class CameraFocus : MonoBehaviour
    {
        // 手动切换摄像头
        public static void SwitchCamera(int type)
        {
#if UNITY_IOS 
            SwitchCameraNative(type);
#endif
        }
        
        
        // 原生方法声明
#if UNITY_IOS 

        [DllImport("__Internal")]
        private static extern void SwitchCameraNative(int type); // 0:主摄, 1:超广角


#endif
    }
}
