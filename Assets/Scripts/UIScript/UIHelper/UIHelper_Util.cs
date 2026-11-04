using System;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using LetsPiu.Define;
using UnityEngine;
using DeviceType = LetsPiu.Define.DeviceType;

namespace LetsPiu.UI
{
    public static partial class UIHelper
    {
        /// <summary>
        /// 该资源在CommonAtlas
        /// </summary>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string GetGenderSpriteName(int sex)
        {
            return sex == 0 ? "Gender Female" : "Gender Male";
        }

        /// <summary>
        /// 获取玩家的分数等级名称
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string GetPlayScoreLevelName(PlayScoreLevel level)
        {
            switch (level)
            {
                case PlayScoreLevel.SPlus:
                    return "S+";
                case PlayScoreLevel.S:
                    return "S";
                case PlayScoreLevel.A:
                    return "A";
                case PlayScoreLevel.B:
                    return "B";
                case PlayScoreLevel.C:
                    return "C";
                case PlayScoreLevel.D:
                    return "D";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取玩家角色职业图标名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetJobIcon(ProfessionType type)
        {
            switch (type)
            {
                case ProfessionType.Engineer:
                    return "juese_Ico2";
                case ProfessionType.Recon:
                    return "juese_Icon1";
                case ProfessionType.Medic:
                    return "juese_Icon";
                case ProfessionType.Assault:
                    return "juese_Ico3";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取玩家角色职业全身图名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetJobFullImage(ProfessionType type)
        {
            switch (type)
            {
                case ProfessionType.Engineer:
                    return "Beekprq";
                case ProfessionType.Recon:
                    return "juesequanshen";
                case ProfessionType.Medic:
                    return "Servantq";
                case ProfessionType.Assault:
                    return "Wardenq";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取玩家角色职业半身图名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetJobHalfImage(ProfessionType type)
        {
            switch (type)
            {
                case ProfessionType.Engineer:
                    return "Beekprb";
                case ProfessionType.Recon:
                    return "juese_zhong";
                case ProfessionType.Medic:
                    return "Servantb";
                case ProfessionType.Assault:
                    return "Wardenb";
                default:
                    return string.Empty;
            }
        }
        
        /// <summary>
        /// 获取玩家角色职业半身背影图名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetJobHalfImageBg(ProfessionType type)
        {
            switch (type)
            {
                case ProfessionType.Engineer:
                    return "Beekprb_banshen";
                case ProfessionType.Recon:
                    return "TJJS_juese_touying";
                case ProfessionType.Medic:
                    return "Servantb_banshen";
                case ProfessionType.Assault:
                    return "wxz_yy_banshen";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取玩家角色职业圆形头像名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetJobHeadIconCircle(ProfessionType type)
        {
            switch (type)
            {
                case ProfessionType.Engineer:
                    return "Beekprhc";
                case ProfessionType.Recon:
                    return "juese_touxiang_fang";
                case ProfessionType.Medic:
                    return "Servanthc";
                case ProfessionType.Assault:
                    return "Wardenhc";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取玩家角色职业头像名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetJobHeadIcon(ProfessionType type)
        {
            switch (type)
            {
                case ProfessionType.Engineer:
                    return "juese_touxiang_fang2";
                case ProfessionType.Recon:
                    return "juese_touxiang_fang1";
                case ProfessionType.Medic:
                    return "juese_touxiang_fang3";
                case ProfessionType.Assault:
                    return "juese_touxiang_fang4";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 将整数转换为英文序数词的缩写形式 (例如: 1 -> "1st", 2 -> "2nd", 3 -> "3rd", 4 -> "4th").
        /// </summary>
        /// <param name="number">要转换的整数.</param>
        /// <returns>表示序数词的字符串. 如果输入的数字小于等于0，则返回数字的字符串形式.</returns>
        public static string ToOrdinal(int number)
        {
            // 处理无效输入 (小于等于 0 的数字).  可以根据需要抛出异常或返回错误消息.
            if (number <= 0)
            {
                return number.ToString(); // 返回数字本身作为字符串
            }

            // 获取数字的个位数，用于确定后缀.
            var lastDigit = number % 10;

            // 获取数字的后两位数，用于处理 11th, 12th, 13th 这种特殊情况.
            var lastTwoDigits = number % 100;

            // 处理 11th, 12th, 13th 的特殊情况.  这些数字都以 "th" 结尾，不考虑个位数.
            if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
            {
                return number + "th";
            }

            // 根据个位数确定正确的后缀.
            switch (lastDigit)
            {
                case 1:
                    return number + "st"; // 以 1 结尾的数字 (除了 11)
                case 2:
                    return number + "nd"; // 以 2 结尾的数字 (除了 12)
                case 3:
                    return number + "rd"; // 以 3 结尾的数字 (除了 13)
                default:
                    return number + "th"; // 其他所有数字都以 "th" 结尾
            }
        }

        public static Color GetColorFromHex(string format)
        {
            if (ColorUtility.TryParseHtmlString(format, out Color c))
            {
                return c;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// 根据游戏模式获取模式背景图片名称
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static string GetModeBackground(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.BEGINNER_MODE:
                    return "BeginnerMode";
                case GameMode.SPEED_MODE:
                    return "SpeedMode";
                case GameMode.HARD_CORE_MODE:
                    return "HardCoreMode";
                case GameMode.ESCAPE_KILL_MODE_ONE:
                case GameMode.ESCAPE_KILL_MODE_TWO:
                case GameMode.ESCAPE_KILL_MODE_FOUR:
                    return "EscapeKillMode";
                case GameMode.ARENA_MODE:
                    return "ArenaMode";
                case GameMode.DEAD_FIGHT_MODE:
                    return "DeadFightMode";
                case GameMode.BLAST_MODE:
                    return "BlastMode";
                case GameMode.TEAM_SPORTS_MODE:
                    return "TeamSportsMode";
                default:
                    return string.Empty;
            }
        }
        public static string GetModeBackground_Icon(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.BEGINNER_MODE:
                    return "BeginnerMode_Icon";
                case GameMode.SPEED_MODE:
                    return "SpeedMode_Icon";
                case GameMode.HARD_CORE_MODE:
                    return "HardCoreMode_Icon";
                case GameMode.ESCAPE_KILL_MODE_ONE:
                case GameMode.ESCAPE_KILL_MODE_TWO:
                case GameMode.ESCAPE_KILL_MODE_FOUR:
                    return "EscapeKillMode_Icon";
                case GameMode.ARENA_MODE:
                    return "ArenaMode_Icon";
                case GameMode.DEAD_FIGHT_MODE:
                    return "DeadFightMode_Icon";
                case GameMode.BLAST_MODE:
                    return "BlastMode_Icon";
                case GameMode.TEAM_SPORTS_MODE:
                    return "TeamSportsMode_Icon";
                default:
                    return string.Empty;
            }
        }
        public static string GetModeBackground_RectIcon(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.BEGINNER_MODE:
                    return "BeginnerMode_RectIcon";
                case GameMode.SPEED_MODE:
                    return "SpeedMode_RectIcon";
                case GameMode.HARD_CORE_MODE:
                    return "HardCoreMode_RectIcon";
                case GameMode.ESCAPE_KILL_MODE_ONE:
                case GameMode.ESCAPE_KILL_MODE_TWO:
                case GameMode.ESCAPE_KILL_MODE_FOUR:
                    return "EscapeKillMode_RectIcon";
                case GameMode.ARENA_MODE:
                    return "ArenaMode_RectIcon";
                case GameMode.DEAD_FIGHT_MODE:
                    return "DeadFightMode_RectIcon";
                case GameMode.BLAST_MODE:
                    return "BlastMode_RectIcon";
                case GameMode.TEAM_SPORTS_MODE:
                    return "TeamSportsMode_RectIcon";
                default:
                    return string.Empty;
            }
        }


        public static string GetMapName(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.BLAST_MODE:
                    return "灵魂汲取";
                case GameMode.TEAM_SPORTS_MODE:
                    return "灵魂汲取";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取设备颜色字符串
        /// </summary>
        /// <param name="deviceColor"></param>
        /// <returns></returns>
        public static string GetDeviceColorName(DeviceColor deviceColor)
        {
            var color = "未知";
            if (deviceColor == DeviceColor.ChinaRed)
            {
                return "中国红";
            }
            else if (deviceColor == DeviceColor.ObsidianBlack)
            {
                return "曜石黑";
            }
            else if (deviceColor == DeviceColor.MoonWhite)
            {
                return "皓月白";
            }
            else if (deviceColor == DeviceColor.RoseQuartz)
            {
                return "粉晶色";
            }
            else if (deviceColor == DeviceColor.TiffanyBlue)
            {
                return "蒂芙尼蓝";
            }
            else
            {
                return color;
            }
        }

        public static string GetDeviceName(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null) return "未知设备";
            if (deviceInfo.type == DeviceType.Gun)
            {
                return GetGunName(deviceInfo);
            }
            else if (deviceInfo.type == DeviceType.Head)
            {
                return GetHeadName(deviceInfo);
            }
            else
            {
                return "未知设备";
            }
        }

        /// <summary>
        /// 获取枪的显示名字
        /// </summary>
        /// <returns></returns>
        public static string GetGunName(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null) return "PIUⁿ";
            if (deviceInfo.subType == SubDeviceType.SecondGen)
            {
                return "PIU^";
            }
            else
            {
                return "PIU\u00b2";
            }
        }

        /// <summary>
        /// 获取头的显示名字
        /// </summary>
        /// <returns></returns>
        public static string GetHeadName(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null) return "";
            if (deviceInfo.subType == SubDeviceType.SecondGen)
            {
                return "PIU^圆箍饰环";
            }
            else
            {
                return "PIU\u00b2圆箍饰环";
            }
        }

        public static string GetFactionName(FactionType type)
        {
            return type switch
            {
                FactionType.Ark => "无限方舟",
                FactionType.Farm => "深浅农场",
                FactionType.YYTH => "引业同行",
                _ => "",
            };
        }

        /// <summary>
        /// 设置屏幕为方向为横屏自动反转(禁止竖屏)
        /// </summary>
        public static void SetScreenRotationAutoLandscape(Action callback = null)
        {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.AutoRotation;
            
            //首先设置强制横屏
            Screen.orientation = Input.deviceOrientation switch
            {
                
                DeviceOrientation.LandscapeLeft => ScreenOrientation.LandscapeLeft,
                DeviceOrientation.LandscapeRight => ScreenOrientation.LandscapeRight,
                _ => ScreenOrientation.LandscapeLeft
            };
            
            //延迟一会儿，设置为自动横屏旋转
            UniTask.Post(async () =>
            {
                await UniTask.WaitForSeconds(0.5f);
                Screen.orientation = ScreenOrientation.AutoRotation;
                await UniTask.WaitForSeconds(0.5f);
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 设置屏幕为方向为竖屏自动反转(禁止横屏)
        /// </summary>
        public static void SetScreenRotationAutoPortrait(Action callback = null)
        {
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            
            //首先设置强制竖屏
            Screen.orientation = Input.deviceOrientation switch
            {
                
                DeviceOrientation.Portrait => ScreenOrientation.Portrait,
                DeviceOrientation.PortraitUpsideDown => ScreenOrientation.PortraitUpsideDown,
                _ => ScreenOrientation.Portrait
            };
            
            //延迟一会儿，设置为自动横屏旋转
            UniTask.Post(async () =>
            {
                await UniTask.WaitForSeconds(0.5f);
                Screen.orientation = ScreenOrientation.AutoRotation;
                await UniTask.WaitForSeconds(0.5f);
                callback?.Invoke();
            });
        }

        /// <summary>
        /// 获取颜色
        /// </summary>
        /// <param name="raceType"></param>
        /// <returns></returns>
        public static Color GetRaceColor(MaterialBoxSubType raceType) 
        {
            switch (raceType)
            {
                case MaterialBoxSubType.Green:
                    return UIHelper.GetColorFromHex("#4BFF93");
                case MaterialBoxSubType.Blue:
                    return UIHelper.GetColorFromHex("#46F3FF");
                case MaterialBoxSubType.Purple:
                    return UIHelper.GetColorFromHex("#F046FF");
                case MaterialBoxSubType.Gold:
                    return UIHelper.GetColorFromHex("#FFF946");
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 设置角色名称的背景
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetJobAlias(ProfessionType type)
        {
            switch (type)
            {
                case ProfessionType.Engineer:
                    return "BEEKPR";
                case ProfessionType.Recon:
                    return "PRISM";
                case ProfessionType.Medic:
                    return "SERVANT";
                case ProfessionType.Assault:
                    return "WARDEN";
                default:
                    return string.Empty;
            }
        }
    }
}