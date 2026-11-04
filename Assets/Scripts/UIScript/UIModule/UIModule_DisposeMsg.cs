using LetsPiu.UIDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public partial class UIModule
    {
        void RequestLoginOut()
        {
            Debug.Log("UIModule RequestLoginOut 断开硬件连接....");
            if (DeviceConnector.BluetoothHardwareEnabled)
            {
                DeviceConnector.Instance.Disconnect();
            }
            Debug.Log("UIModule RequestLoginOut 断开MQTT连接....");
            MqttConnector.Instance.Disconnect();
            Debug.Log("UIModule RequestLoginOut 清除登录数据....");
            this.OpenPanel(UIID.LoginPanel, a =>
            {
                this.ResetAllPanel(uid => uid == UIID.LoginPanel);
            });
        }
    }
}
