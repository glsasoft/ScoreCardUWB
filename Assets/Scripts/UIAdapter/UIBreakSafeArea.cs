using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// 此脚本挂在全屏背景上
    /// </summary>
    public class UIBreakSafeArea : MonoBehaviour
    {
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
            
            // 设置了应用的朝向，需要比对，比对成功才打破安全区域
            if (UIScreenAdapter.orientation == ScreenOrientation.LandscapeLeft
                || UIScreenAdapter.orientation == ScreenOrientation.LandscapeRight) 
            {
                rectTrans.offsetMin = new Vector2(-UIScreenAdapter.offsetX, 0); //left,bottom
                rectTrans.offsetMax = new Vector2(UIScreenAdapter.offsetX, 0); //right,top
            }
            else if (UIScreenAdapter.orientation == ScreenOrientation.Portrait || UIScreenAdapter.orientation == ScreenOrientation.PortraitUpsideDown)
            {
                rectTrans.offsetMin = new Vector2(0, -UIScreenAdapter.offsetY);//left,bottom
                rectTrans.offsetMax = new Vector2(0, UIScreenAdapter.offsetY);//right,top
            }
        }

        void Start()
        {
            Refresh();
        }
    }
}
