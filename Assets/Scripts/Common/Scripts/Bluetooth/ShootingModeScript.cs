using System;
using LetsPiu.Define;
using UnityEngine;

namespace LetsPiu.Main.Common.Scripts.Bluetooth
{
    /// <summary>
    /// 单击和连击状态判断
    /// 向外发送的事件： EventMsg.GunSingeShootEvent, EventMsg.GunBurstShootEvent, EventMsg.GunBurstStartEvent, EventMsg.GunBurstEndEvent
    /// </summary>
    public class ShootingModeScript : MonoBehaviour
    {
        /// <summary>
        /// 当前射击状态
        /// </summary>
        private ShootingMode currentMode = ShootingMode.Single;

        /// <summary>
        /// 连发状态判定时间(毫秒)
        /// </summary>
        private const float BURST_MODE_THRESHOLD = 310f;

        /// <summary>
        /// 最后一次射击时间
        /// </summary>
        private float lastShotTime;

        /// <summary>
        /// 射击计数器(用于连发判定)
        /// </summary>
        private int shotCounter;

        private void OnEnable()
        {
            EventManager.Instance.AddEvent(EventMsg.GunShootEvent, HandleShootEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveEvent(EventMsg.GunShootEvent, HandleShootEvent);
        }

        /// <summary>
        /// 蓝牙设备发送过来的射击消息
        /// </summary>
        private void HandleShootEvent()
        {
            var currentTime = Time.time;
            var timeSinceLastShot = (currentTime - lastShotTime) * 1000f;
            lastShotTime = currentTime;

            if (timeSinceLastShot <= BURST_MODE_THRESHOLD)
            {
                shotCounter++;
                if (currentMode != ShootingMode.Burst)
                {
                    currentMode = ShootingMode.Burst;
                    Debug.Log($"切换到连发模式 计数:{shotCounter}");
                    // 发送连发开始事件
                    EventManager.Instance.Broadcast(EventMsg.GunBurstStartEvent);
                }
            }
            else
            {
                currentMode = ShootingMode.Single;
                shotCounter = 1;
                Debug.Log($"切换到单发模式 计数:{shotCounter}");
            }

            ExecuteShot();
        }

        private void Update()
        {
            if (lastShotTime != 0)
            {
                float timeSinceLastShot = (Time.time - lastShotTime) * 1000f;
                if (timeSinceLastShot > BURST_MODE_THRESHOLD)
                {
                    shotCounter = 0;
                    Debug.Log("发射结束");
                    lastShotTime = 0;
                    EventManager.Instance.Broadcast(EventMsg.GunShootFinishEvent);
                    // 发送连发结束事件
                    if (currentMode == ShootingMode.Burst)
                    {
                        EventManager.Instance.Broadcast(EventMsg.GunBurstEndEvent);
                    }
                }
            }
        }

        /// <summary>
        /// 根据当前模式执行射击
        /// </summary>
        private void ExecuteShot()
        {
            if (currentMode == ShootingMode.Single)
            {
                SingleShot();
            }
            else
            {
                BurstShot();
            }
        }

        /// <summary>
        /// 执行单发射击逻辑
        /// </summary>
        private void SingleShot()
        {
            //Debug.Log($"执行单发射击 计数:{shotCounter}");
            // 发送单发射击事件
            EventManager.Instance.Broadcast(EventMsg.GunSingeShootEvent);
        }

        /// <summary>
        /// 执行连发射击逻辑
        /// </summary>
        private void BurstShot()
        {
            //Debug.Log($"执行连发射击 计数:{shotCounter}");
            // 发送连发射击事件
            EventManager.Instance.Broadcast(EventMsg.GunBurstShootEvent);
        }
    }
}
