using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LetsPiu.UI.UIScript.Common
{
    /// <summary>
    /// 此脚本挂在全屏背景上(仅IOS生效)
    /// </summary>
    public class UIBreakSafeAreaIOS : UIBreakSafeAreaHelper
    {
#if UNITY_IOS
        private void OnEnable()
        {
            OnSafeAreaRefreshed += RefreshBreakSafeArea;
        }

        private void OnDisable()
        {
            OnSafeAreaRefreshed -= RefreshBreakSafeArea;
        }

        /// <summary>
        /// 手动刷新安全区，防止某些情况下屏幕旋转后没有触发安全区变化
        /// </summary>
        public void ManualRefreshSafeArea()
        {
            if (IsInitialized())
            {
                RefreshBreakSafeArea(GetOffsetX(), GetOffsetY(), GetOrientation());
            }
        }

        private void RefreshBreakSafeArea(int offsetX, int offsetY, ScreenOrientation orientation)
        {
            //在屏幕状态发生变化后，间隔1帧后刷新布局，覆盖UIScreenAdapter调整
            UniTask.Post(async () =>
            {
                await UniTask.NextFrame();
                Refresh();
            });
        }
        
        public void Refresh()
        {
            RectTransform rectTrans = transform as RectTransform;
            if (rectTrans == null)
            {
                return;
            }

            rectTrans.offsetMax = Vector2.zero;
            rectTrans.offsetMin = Vector2.zero;
            
            /*
 offsetMin 表示锚点左下角的偏移（left, bottom）。
offsetMax 表示锚点右上角的偏移（right, top）。

offsetMin.x：距离左边的偏移（left），正值表示向右偏移，负值表示向左扩展。
offsetMin.y：距离下边的偏移（bottom），正值表示向上偏移，负值表示向下扩展。
offsetMax.x：距离右边的偏移（right），正值表示向左偏移，负值表示向右扩展。
offsetMax.y：距离上边的偏移（top），正值表示向下偏移，负值表示向上扩展。

 */
            var safeAreaOffsetX = GetOffsetX();
            var safeAreaOffsetY = GetOffsetY();
            Debug.Log("AndroidUIBreakSafeArea 减去 min之前safeAreaOffsetX:"+ safeAreaOffsetX+" safeAreaOffsetY:"+ safeAreaOffsetY+" MIN_SCREEN_OFFSET:"+ MIN_SCREEN_OFFSET);
            //保证一个最小的安全区，防止手机圆角遮挡
            safeAreaOffsetX -= MIN_SCREEN_OFFSET;
            
            //保证一个最小的安全区，防止手机圆角遮挡
            if (safeAreaOffsetX < MIN_SCREEN_OFFSET)
            {
                safeAreaOffsetX = MIN_SCREEN_OFFSET;
            }
            var orientation = GetOrientation();

            Debug.Log($"AndroidUIBreakSafeArea 减去min之后 Refresh safeAreaOffsetX:{safeAreaOffsetX} safeAreaOffsetY:{safeAreaOffsetY}");
            
            // 设置了应用的朝向，需要比对，比对成功才打破安全区域
            if (orientation is ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight) 
            {
                Debug.Log($"AndroidUIBreakSafeArea Refresh Landscape safeAreaOffsetX:{safeAreaOffsetX}");
                rectTrans.offsetMin = new Vector2(-safeAreaOffsetX, 0); //left,bottom
                rectTrans.offsetMax = new Vector2(safeAreaOffsetX, 0); //right,top
            }
            else if (orientation is ScreenOrientation.Portrait or ScreenOrientation.PortraitUpsideDown)
            {
                Debug.Log($"AndroidUIBreakSafeArea Refresh Portrait safeAreaOffsetY:{safeAreaOffsetY}");
                rectTrans.offsetMin = new Vector2(0, -safeAreaOffsetY);//left,bottom
                rectTrans.offsetMax = new Vector2(0, safeAreaOffsetY);//right,top
            }
        }
#endif
    }
}