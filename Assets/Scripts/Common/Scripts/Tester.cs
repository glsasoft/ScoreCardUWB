using System;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel;
using UnityEngine;

namespace LetsPiu.Common.Scripts
{
    /// <summary>
    /// 提供一些测试用的流程
    /// </summary>
    public class Tester : SingletonComponent<Tester>
    {
        private void Start()
        {
           
        }
        
        /// <summary>
        /// 测试自动登录，并且绑定武器
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="gunQrCode"></param>
        /// <returns></returns>
        public async UniTask<bool> TestLoginAndRegisterGunAsync(string phone, string gunQrCode)
        {
            return true;
        }
        
        /// <summary>
        /// 测试自动登录，并且绑定武器
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> TestLoginAndRegisterGunAsync()
        {
            var phone = "56565656565";
            var gunQrCode = "http://piupiu.glsa.com.cn/device/1/1/PQ00014205";
            return await TestLoginAndRegisterGunAsync(phone, gunQrCode);
        }
        
        /// <summary>
        /// 测试自动登录，并且绑定武器
        /// </summary>
        /// <returns></returns>
        public void TestLoginAndRegisterGun(Action<bool> callback)
        {
            TestLoginAndRegisterGunAsync().Forget();
        }
    }
}