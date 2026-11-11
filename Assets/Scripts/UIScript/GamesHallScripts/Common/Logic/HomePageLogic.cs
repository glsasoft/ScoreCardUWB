using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI;
using LetsPiu.UIDefine;
using UnityEngine;

namespace LetsPiu.UI.UIScript.GamesHallScripts.Common.Logic
{
    public class HomePageLogic : AbstractPanel
    {

        protected override void OnEnable()
        {
            base.OnEnable();

        }

        protected override void OnDisable()
        {
            base.OnDisable();

        }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);

#if UNITY_ANDROID && !UNITY_EDITOR
            // 触发 LoraSerialBridge 单例实例化 → OnEnable → 自动打开串口
            _ = LoraSerialBridge.Instance;
#endif
        }
    }
}
