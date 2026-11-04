using System;
using UnityEngine;

namespace Common.Bluetooth.Model.Send
{
    public class GunInitMessage : BlueBaseMessage
    {

        public class GunInitParams
        {
            
            /// <summary>
            /// 将十六进制颜色字符串转换为 Unity Color。
            /// </summary>
            /// <param name="hexColor">十六进制颜色字符串 (例如: "FF0000")。</param>
            /// <returns>转换后的 Unity Color。</returns>
            public static Color HexToColor(string hexColor)
            {
                hexColor = hexColor.Replace("#", ""); // 移除可选的 "#" 符号
                byte r = byte.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                return new Color32(r, g, b, 255); // Color32 构造函数接受 byte 类型
            }

            // 预定义的颜色
            public static Color RedColor = HexToColor("FF0000");
            public static Color GreenColor = HexToColor("00FF00");
            public static Color BlueColor = HexToColor("0000FF");
            public static Color YellowColor = HexToColor("FFFF00");
            public static Color PurpleColor = HexToColor("FF00FF");
            
            /// <summary>
            /// 是否启动枪
            /// </summary>
            public bool start;
            /// <summary>
            /// 短编号(游戏中的席位编号)
            /// </summary>
            public int shortId;
            /// <summary>
            /// 场次ID(整除256取余，设置到枪内，防止不同场次的玩家可以互相射击)
            /// </summary>
            public string playId;
            /// <summary>
            /// 玩家队伍(队伍根技能前后拼接成一个字节)
            /// </summary>
            public int team;
            /// <summary>
            /// 玩家技能(队伍根技能前后拼接成一个字节)
            /// </summary>
            public int skill;
            /// <summary>
            /// 射速(每秒中多少发)
            /// </summary>
            public int shootSpeed = 7;
            /// <summary>
            /// 能否被击中
            /// </summary>
            public bool canBeHit;
            /// <summary>
            /// 能否射击
            /// </summary>
            public bool canShoot;
            /// <summary>
            /// 是否是大功率
            /// </summary>
            public bool highPower;
            /// <summary>
            /// 灯光颜色
            /// </summary>
            public Color color;
            /// <summary>
            /// 玩家是否死亡
            /// </summary>
            public bool isMainPlayerDead;

            /// <summary>
            /// 获取一个小于256的场次ID。因为硬件只能存储0~255的场次id
            /// </summary>
            /// <returns></returns>
            public int GetShortPlayId()
            {
                //场次号
                long shortPlayId = 1;
                try
                {
                    shortPlayId = long.Parse(playId);
                }
                catch (Exception e)
                {
                    Debug.LogError("GunInitMessage 场次ID转换错误:"+e.Message);
                }
                return (int)(shortPlayId % 256);
            }
        }
        
        public static GunInitMessage CreateCalibrationGunMessage()
        {
            var initMessage = new GunInitMessage
            {
                shortId = GetHexStringFromInt(1),
                cmd = "0A"
            };
            //队伍
            var team = "1";
            var skill = "1";
            initMessage.btye3 = team + skill;
            //射速
            initMessage.btye4 = GetHexStringFromInt(100 / 7);
            //是否启动
            initMessage.btye5 = "01";
            //是否可被击中，是否可开枪
            initMessage.btye6 = "03";
            //场次号
            initMessage.btye7 = GetHexStringFromInt(1);
            initMessage.btye8 = GetHexStringFromInt(1);
            return initMessage;
        }

        public static GunInitMessage CreateCantShootMessage(int shortId, int team)
        {
            var initMessage = new GunInitMessage
            {
                shortId = GetHexStringFromInt(shortId),
                cmd = "0A"
            };
            //队伍
            var skill = "1";
            initMessage.btye3 = team + skill;
            //射速
            initMessage.btye4 = GetHexStringFromInt(100 / 7);

            //是否启动
            initMessage.btye5 = "01";
            //是否可被击中，是否可开枪
            initMessage.btye6 = "01";
            //场次号
            initMessage.btye7 = GetHexStringFromInt(0);
            //远近程：0进程其他远程
            initMessage.btye8 = GetHexStringFromInt(0);
            return initMessage;
        }
        
        /// <summary>
        /// 初始化枪
        /// </summary>
        /// <param name="params"></param>
        /// <returns></returns>
        public static GunInitMessage CreateInitMessage(GunInitParams @params)
        {
            if (@params.shootSpeed <= 0)
            {
                Debug.LogWarning("GunInitMessage 射速错误:"+@params.shootSpeed+" 改为默认7发/秒");
                @params.shootSpeed = 7;
            }
            var initMessage = new GunInitMessage
            {
                shortId = GetHexStringFromInt(@params.shortId),
                cmd = "00",
                //队伍
                btye3 = @params.team.ToString()+@params.skill,
                //射速
                btye4 = GetHexStringFromInt(100 / @params.shootSpeed),
                //是否启动
                btye5 = @params.start? "01": "00"
            };

            //是否可被击中，是否可开枪
            var canBeHit = 0;
            var canShoot = 0;
            if (@params.canBeHit)
            {
                canBeHit = 1;
            }

            if (@params.canShoot)
            {
                canShoot = 2;
            }
            initMessage.btye6 = GetHexStringFromInt(canBeHit + canShoot);
            
            //场次号
            initMessage.btye7 = GetHexStringFromInt(@params.GetShortPlayId());
            //远近程：0进程 / 其他远程
            initMessage.btye8 = GetHexStringFromInt(@params.highPower?1:0);
            return initMessage;
        }

        /// <summary>
        /// 创建关闭枪消息
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static GunInitMessage CreateStopGunMessage(int number = 0)
        {
            var initMessage = new GunInitMessage
            {
                shortId = GetHexStringFromInt(number),
                cmd = "0A",
                //队伍
                btye3 = "00",
                //射速
                btye4 = GetHexStringFromInt(0),
                //是否启动
                btye5 = "00",
                //是否可被击中，是否可开枪
                btye6 = "00",
                //场次号
                btye7 = GetHexStringFromInt(0),
                //场次号
                btye8 = GetHexStringFromInt(0)
            };
            return initMessage;
        }
    }
}
