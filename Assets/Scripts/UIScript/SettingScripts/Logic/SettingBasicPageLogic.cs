using LetsPiu.Common.UI;
using LetsPiu.UI.UIScript.SettingScripts.UI;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.SettingScripts.Logic
{
    public class SettingBasicPageLogic : AbstractPage
    {
        [FormerlySerializedAs("basicSettingPageUI")] public SettingBasicPageUI settingBasicPageUI;

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);

            if (settingBasicPageUI)
            { 
                if(settingBasicPageUI.textSmooth && settingBasicPageUI.textOverload)
                {
                    bool overloaded = IsOverloaded();
                    settingBasicPageUI.textSmooth.gameObject.SetActive(!overloaded);
                    settingBasicPageUI.textOverload.gameObject.SetActive(overloaded);
                }
            }
        }

        public bool IsOverloaded()
        {
            return Time.deltaTime > (1f / Application.targetFrameRate) * 1.3f
                   || Profiler.GetTotalAllocatedMemoryLong() > SystemInfo.systemMemorySize * 0.6f;
        }
    }
}
