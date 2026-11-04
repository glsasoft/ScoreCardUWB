using System;
using UnityEngine;

namespace LetsPiu.Main.Common.Scripts.Bluetooth
{
    /*
     控制灯的指令说明:
    
     命令一共24个字节BYTE0~BYTE23。
     开头字节(BYTE0)固定是16进制值:5A
     结束字节(BYTE23)固定16进制值:A5。
     BYTE1~BYTE15用来控制30个灯的状态。
     BYTE15~BYTE20预留，全部填0。
     BYTE21代表亮度等级，00~FF。
     BYTE22代表闪烁频率，单位是10ms，范围是00~FF。
     
     其中，BYTE1~BYTE15每一个字节控制2个灯的显示。说明如下：					
     高位四位（左高右低） 和 低四位（左高右低），每个四位代表一个灯的状态。
     每四个位的bit：
     第1个位，0代表不闪烁，1代表闪烁。
     第2~3位，是一整个数字，代表灯的颜色，0=灭，1=红，2=绿，3=蓝4=黄，5=紫，6=橙，7=白
     
     完整的指令示例(16进制字符串):
     5A03030303030303030303030303030303030303030303A5
     5A222222666666666666aaaaaaaaaaaa0000000000bbbbA5
     5A333333666666666666bbbbbbbbbbbb0000000000aaaaA5
     5A111111777777777777ffffffffffff00000000000000A5
     5A222222777777777777ffffffffffff00000000002222A5
     -------------------------------------------------
     */
    
    /// <summary>
    /// 代表队伍颜色时，红队红色，蓝队蓝色，多支队伍选择绿色
    /// </summary>
    public enum BlueToothLightColor
    {
        Off = 0x00, // 灯灭
        Red = 0x01, // 红色
        Green = 0x02, // 绿色
        Blue = 0x03, // 蓝色
        Yellow = 0x04, // 黄色
        Purple = 0x05, // 紫色
        Orange = 0x06, // 橙色
        White = 0x07  // 白色
    }
    
    public class BluetoothLightState
    {
        public bool blink; // 是否闪烁
        public BlueToothLightColor color; // 灯的颜色
        
        public BluetoothLightState(bool blink = false, BlueToothLightColor color = BlueToothLightColor.Off)
        {
            this.blink = blink;
            this.color = color;
        }
    }
    
    public class BluetoothLightController
    {
        /// <summary>
        /// 发送灯的控制命令(16进制字符串)到设备上
        /// </summary>
        /// <param name="message">16进制字符串命令</param>
        private static void SendMessageToDevice(string message)
        {
            // DeviceConnector.Instance.SendRawMessageToGun(message);
            DeviceConnector.Instance.SendMessageToDeviceToQueue(DeviceConnector.DeviceTypeGun, message);
        }
        
        /// <summary>
        /// 控制30个灯的状态
        /// </summary>
        /// <param name="lightStates">30个灯的状态数组</param>
        /// <param name="brightness">亮度等级(0x00-0xFF)</param>
        /// <param name="blinkFrequency">闪烁频率(0x00-0xFF)，单位10ms</param>
        public static void ControlLights(BluetoothLightState[] lightStates, byte brightness = 0xFF, byte blinkFrequency = 0x00)
        {
            if (lightStates is not { Length: 30 })
            {
                Debug.LogError("BluetoothLightController 灯的状态数组必须包含30个灯的状态");
                return;
            }
            
            // 验证亮度值
            brightness = Math.Min(brightness, (byte)0xFF);
            
            // 构建指令
            var command = new byte[24];
            
            // 设置开头和结束字节
            command[0] = 0x5A; // 开头字节
            command[23] = 0xA5; // 结束字节
            
            // 编码15个灯的状态到BYTE1~BYTE15(每个字节控制2个灯)
            int lightCount = lightStates.Length;
            int byteCount = (lightCount + 1) / 2; // 30个灯，每2个灯1字节，共15字节
            for (int byteIndex = 1; byteIndex <= byteCount; byteIndex++)
            {
                int lightIndex = (byteIndex - 1) * 2;

                // 获取两个灯的状态
                var state1 = lightStates[lightIndex];
                var state2 = (lightIndex + 1 < lightCount) ? lightStates[lightIndex + 1] : new BluetoothLightState();

                // 编码第一个灯的状态(低4位)
                byte light1 = (byte)(
                    ((state1.blink ? 1 : 0) << 3) | // 闪烁标志(第1位)
                    ((byte)state1.color & 0x07)     // 颜色值(第2-3位)
                );

                // 编码第二个灯的状态(高4位)
                byte light2 = (byte)(
                    ((state2.blink ? 1 : 0) << 3) | // 闪烁标志(第1位)
                    ((byte)state2.color & 0x07)     // 颜色值(第2-3位)
                );

                // 合并两个灯状态到一个字节（低4位是第一个灯，高4位是第二个灯）
                // command[byteIndex] = (byte)((light2 << 4) | light1); // 早期的固件版本
                command[byteIndex] = (byte)((light1 << 4) | light2); // 0712的固件版本
            }
            
            // 填充预留字节(BYTE16~BYTE20)
            for (int i = 16; i <= 20; i++)
            {
                command[i] = 0x00;
            }
            
            // 设置亮度(BYTE21)和闪烁频率(BYTE22)
            command[21] = brightness;
            command[22] = blinkFrequency;
            
            // 转换为16进制字符串并发送
            var hexString = BitConverter.ToString(command).Replace("-", "");
            SendMessageToDevice(hexString);
        }
        
        /// <summary>
        /// 控制某个灯的状态
        /// </summary>
        /// <param name="lightIndex"></param>
        /// <param name="state"></param>
        /// <param name="brightness"></param>
        /// <param name="blinkFrequency"></param>
        public static void ControlLight(int lightIndex, BluetoothLightState state, byte brightness = 0xFF, byte blinkFrequency = 0x00)
        {
            Debug.Log("BluetoothLightController 控制灯的状态: " + lightIndex + ", " + state.blink + ", " + state.color);
            if (lightIndex is < 0 or >= 30)
            {
                Debug.LogError("BluetoothLightController 灯的索引必须在0到29之间");
                return;
            }
            
            // 创建30个灯的状态数组
            var lightStates = new BluetoothLightState[30];
            for (int i = 0; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState();
            }
            
            // 设置指定灯的状态
            lightStates[lightIndex] = state;
            
            // 调用控制灯的方法
            ControlLights(lightStates, brightness, blinkFrequency);
        }
        
        /// <summary>
        /// 设置玩家存活的灯的状态
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetPlayerAliveLightsState(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置玩家存活的灯状态: " + teamColor);
            // 第0~17号灯，每3个灯代表一格。前两格点亮为队伍颜色，其余熄灭。
            var lightStates = new BluetoothLightState[30];
            // 前6个灯（0~5）点亮为队伍颜色
            for (int i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(false, teamColor);
            }
            // 其余灯熄灭
            for (int i = 6; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState(false, BlueToothLightColor.Off);
            }
            // 发送灯状态
            ControlLights(lightStates);
        }
        
        /// <summary>
        /// 设置玩家死亡的灯的状态
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetPlayerDeadLightsState(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置玩家死亡的灯状态: " + teamColor);
            // 第0~17号灯，每3个灯代表一格。前两格点亮为队伍颜色，其余熄灭。
            var lightStates = new BluetoothLightState[30];
            // 前6个灯（0~5）点亮为队伍颜色
            for (int i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(true, teamColor);
            }
            // 四个状态灯及枪口灯显示紫色
            for (var i = 6; i < 24; i++)
            {
                lightStates[i] = new BluetoothLightState(true, BlueToothLightColor.Purple);
            }
            //其余灯熄灭
            for (var i = 24; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState();
            }
            // 发送灯状态
            ControlLights(lightStates);
        }
        
        /// <summary>
        /// 释放大招与大招BUFF存续状态：两个队伍灯固定常亮，后四个枪身灯与枪口灯为橙色常亮。
        /// </summary>
        /// <param name="teamColor"></param>
        /// <param name="skillColor">其余灯的颜色</param>
        public static void SetPlayerUltimateLightsState(BlueToothLightColor teamColor, BlueToothLightColor skillColor = BlueToothLightColor.Orange)
        {
            Debug.Log("BluetoothLightController 设置玩家大招的灯状态: " + teamColor + ", " + skillColor);
            // 前6个灯（0~5）点亮为队伍颜色
            var lightStates = new BluetoothLightState[30];
            for (var i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(false, teamColor);
            }
            // 四个状态灯及枪口灯显示橙色
            for (var i = 6; i < 24; i++)
            {
                lightStates[i] = new BluetoothLightState(true, skillColor);
            }
            //其余灯熄灭
            for (var i = 24; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState();
            }
            // 发送灯状态
            ControlLights(lightStates);
        }
        
        /// <summary>
        /// 释放技能与技能BUFF存续状态：枪身灯与枪口灯常亮，后四个枪身灯为黄色
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetPlayerSkillLightsState(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置玩家技能的灯状态: " + teamColor);
            SetPlayerUltimateLightsState(teamColor, BlueToothLightColor.Yellow);
        }
        
        /// <summary>
        /// 大招状态下投掷蓄力：四个状态灯及枪口灯快速闪烁。
        /// </summary>
        /// <param name="teamColor"></param>
        /// <param name="skillColor">其余灯的颜色</param>
        public static void SetUltimateChargeLightsState(BlueToothLightColor teamColor, BlueToothLightColor skillColor = BlueToothLightColor.Orange)
        {
            Debug.Log("BluetoothLightController 设置大招状态下投掷蓄力的灯状态: " + teamColor + ", " + skillColor);
            // 前6个灯（0~5）点亮为队伍颜色
            var lightStates = new BluetoothLightState[30];
            for (var i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(false, teamColor);
            }
            // 四个状态灯及枪口灯快速闪烁
            for (var i = 6; i < 24; i++)
            {
                lightStates[i] = new BluetoothLightState(true, skillColor);
            }
            //其余灯熄灭
            for (var i = 24; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState();
            }
            // 发送灯状态, 200ms闪烁频率
            ControlLights(lightStates, 0xFF, 20); // 200ms = 20 * 10ms
        }
        
        /// <summary>
        /// 技能状态下投掷蓄力：四个状态灯快速闪烁，枪口灯快速闪烁。
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetPlayerSkillBuffLightsState(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置技能状态下投掷蓄力的灯状态: " + teamColor);
            SetUltimateChargeLightsState(teamColor, BlueToothLightColor.Yellow);
        }
        
        /// <summary>
        /// 普通投掷蓄力：投掷蓄力时，四个状态灯快速闪烁队伍颜色直至投掷或取消投掷。其余灯熄灭。
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetNormalThrowLightsState(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置普通投掷蓄力的灯状态: " + teamColor);
            // 前6个灯（0~5）点亮为队伍颜色
            var lightStates = new BluetoothLightState[30];
            for (var i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(false, teamColor);
            }
            // 4四个状态灯快速闪烁队伍颜色，即第6~17号灯
            for (var i = 6; i < 18; i++)
            {
                lightStates[i] = new BluetoothLightState(true, teamColor);
            }
            // 其余灯熄灭
            for (var i = 18; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState();
            }
            // 发送灯状态
            ControlLights(lightStates, 0xFF, 20); // 200ms = 20 * 10ms
        }
        
        /// <summary>
        /// 普通投掷：投掷时，四个状态灯快速闪烁队伍颜色直至投掷或取消投掷，枪口灯爆闪队伍颜色。
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetNormalThrowLightsStateWithMuzzleFlash(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置普通投掷的灯状态: " + teamColor);
            // 前6个灯（0~5）点亮为队伍颜色
            var lightStates = new BluetoothLightState[30];
            for (var i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(false, teamColor);
            }
            // 4四个状态灯快速闪烁队伍颜色，即第6~17号灯
            for (var i = 6; i < 18; i++)
            {
                lightStates[i] = new BluetoothLightState(true, teamColor);
            }
            // 枪口灯爆闪队伍颜色，即第18~23号灯（枪口一共6个灯）
            for (var i = 18; i < 24; i++)
            {
                lightStates[i] = new BluetoothLightState(true, teamColor);
            }
            // 其余灯熄灭
            for (var i = 24; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState();
            }
            // 发送灯状态
            ControlLights(lightStates, 0xFF, 20); // 200ms = 20 * 10ms
        }
        
        /// <summary>
        /// 大招状态下投掷：四个枪身灯快速闪烁，枪口灯爆闪队伍颜色。
        /// </summary>
        /// <param name="teamColor"></param>
        /// <param name="skillColor">其余灯的颜色</param>
        public static void SetUltimateThrowLightsState(BlueToothLightColor teamColor, BlueToothLightColor skillColor = BlueToothLightColor.Orange)
        {
            Debug.Log("BluetoothLightController 设置大招状态下投掷的灯状态: " + teamColor + ", " + skillColor);
            // 前6个灯（0~5）点亮为队伍颜色
            var lightStates = new BluetoothLightState[30];
            for (var i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(false, teamColor);
            }
            // 4个枪身灯快速闪烁，即第6~17号灯
            for (var i = 6; i < 18; i++)
            {
                lightStates[i] = new BluetoothLightState(true, skillColor);
            }
            // 枪口灯爆闪队伍颜色，即第18~23号灯（枪口一共6个灯）
            for (var i = 18; i < 24; i++)
            {
                lightStates[i] = new BluetoothLightState(true, teamColor);
            }
            // 其余灯熄灭
            for (var i = 24; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState(false, BlueToothLightColor.Off);
            }
            // 发送灯状态
            ControlLights(lightStates, 0xFF, 20); // 200ms = 20 * 10ms
        }
        
        /// <summary>
        /// 技能状态下投掷：四个状态灯快速闪烁，枪口灯爆闪队伍颜色。
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetPlayerSkillThrowLightsState(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置技能状态下投掷的灯状态: " + teamColor);
            SetUltimateThrowLightsState(teamColor, BlueToothLightColor.Yellow);
        }
        
        /// <summary>
        /// 受击：被击中时，两个队伍灯固定常亮，后四个枪身灯与枪口灯为白色。所有状态灯光快速闪烁1次
        /// </summary>
        /// <param name="teamColor"></param>
        public static void SetPlayerHitLightsState(BlueToothLightColor teamColor)
        {
            Debug.Log("BluetoothLightController 设置玩家受击的灯状态: " + teamColor);
            // 前6个灯（0~5）点亮为队伍颜色
            var lightStates = new BluetoothLightState[30];
            for (var i = 0; i < 6; i++)
            {
                lightStates[i] = new BluetoothLightState(false, teamColor);
            }
            // 四个状态灯及枪口灯显示白色
            for (var i = 6; i < 30; i++)
            {
                lightStates[i] = new BluetoothLightState(true, BlueToothLightColor.White);
            }
            // 发送灯状态
            ControlLights(lightStates, 0xFF, 20); // 200ms = 20 * 10ms
        }
    }
}