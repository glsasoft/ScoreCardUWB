using LetsPiu.Define;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public static partial class Helper
    {
        public static string GetRevivalDes(RevivalType reviveType)
        {
            return reviveType switch
            {
                RevivalType.NoRevival => "无法复活",
                RevivalType.UnconditionalRevival => "原地复活",
                RevivalType.QrCodeRevival => "扫码复活",
                RevivalType.FixedPointRevival => "定点复活",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// 获取资源展示模型名称
        /// </summary>
        /// <param name="profession"></param>
        /// <returns></returns>
        public static string GetShowModelRes(ProfessionType profession)
        {
            var modeName = profession switch
            {
                ProfessionType.Engineer => "Beekpr",
                ProfessionType.Recon => "Prism",
                ProfessionType.Medic => "Servant",
                ProfessionType.Assault => "Warden",
                _ => string.Empty,
            };
            return $"Prefab_{modeName}_Entrance";
        }

        /// <summary>
        /// 根据职业类型获取模型名称
        /// </summary>
        /// <param name="profession">职业类型</param>
        /// <param name="isAI">是否是AI</param>
        /// <returns></returns>
        public static string GetTPSModelRes(ProfessionType profession, bool isAI = false)
        {
            var modeName = profession switch
            {
                ProfessionType.Engineer => "Beekpr",
                ProfessionType.Recon => "Prism",
                ProfessionType.Medic => "Servant",
                ProfessionType.Assault => "Warden",
                _ => string.Empty,
            };
            return $"Prefab_{modeName}_tps_{(isAI ? "AI" : "Web")}";
        }

        public static string GetTPSModelRes1(ProfessionType profession)
        {
            var modeName = profession switch
            {
                ProfessionType.Engineer => "Beekpr",
                ProfessionType.Recon => "Prism",
                ProfessionType.Medic => "Servant",
                ProfessionType.Assault => "Warden",
                _ => string.Empty,
            };
            
            return $"Prefab_AC_Prism_TPS_Skin";
            //return $"AC_{modeName}_TPS_Skin";
        }
        
        /// <summary>
        /// 获取第一人称资源名称
        /// </summary>
        /// <param name="profession"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public static string GetFPSModelRes(ProfessionType profession, SubDeviceType deviceType)
        {
            var modeName = profession switch
            {
                ProfessionType.Engineer => "Beekpr",
                ProfessionType.Recon => "Prism",
                ProfessionType.Medic => "Servant",
                ProfessionType.Assault => "Warden",
                _ => string.Empty,
            };

            var gunType = deviceType switch
            {
                SubDeviceType.FirstGen => "piu01",
                SubDeviceType.SecondGen => "piu02",
                _ => "piu01",
            };
            return $"Prefab_{modeName}_fps_{gunType}";
        }

        public static string GetFPSModelResName(ProfessionType profession)
        {
            var modeName = profession switch
            {
                ProfessionType.Engineer => "Prism",
                ProfessionType.Recon => "Prism",
                ProfessionType.Medic => "Prism",
                ProfessionType.Assault => "Prism",
                _ => string.Empty,
            };
            
            return modeName;
        }
        
        public static string GetFPSModelRes1(ProfessionType profession)
        {
            var modeName = profession switch
            {
                ProfessionType.Engineer => "Beekpr",
                ProfessionType.Recon => "Prism",
                ProfessionType.Medic => "Servant",
                ProfessionType.Assault => "Warden",
                _ => string.Empty,
            };
            
            return $"Prefab_AC_Prism_FPS_Skin";
            //return $"AC_{modeName}_FPS_Skin";
        }
        
        /// <summary>
        /// 获取角色人格种类
        /// </summary>
        /// <param name="profession"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public static string GetPrefessionPersonality(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => "工程",
                ProfessionType.Recon => "侦察",
                ProfessionType.Medic => "医疗",
                ProfessionType.Assault => "突击",
                _ => string.Empty,
            };
        }

        ///<summary>
        ///获取线下模式的角色展示站立的位置
        /// </summary>
        public static Vector3 GetOfflineModelPosition(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(0.074f, 9.7f, 68f),
                ProfessionType.Recon => new Vector3(0.074f, 9.7f, 68f),
                ProfessionType.Medic => new Vector3(0.074f, 9.7f, 68f),
                ProfessionType.Assault => new Vector3(0.074f, 9.7f, 67f),
                _ => Vector3.zero,
            };
        }

        ///<summary>
        ///获取线下模式的角色展示旋转角度
        ///</summary>
        public static Vector3 GetOfflineModelRotation(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(0f, -10f, 0f),
                ProfessionType.Recon => new Vector3(0f, -10f, 0f),
                ProfessionType.Medic => new Vector3(0f, -10f, 0f),
                ProfessionType.Assault => new Vector3(0f, -10f, 0f),
                _ => Vector3.zero,
            };
        }

        ///<summary>
        ///获取个人中心站立得位置
        ///</summary>
        public static Vector3 GetPersonalCenterPosition(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(1f, 33f, 70f),
                ProfessionType.Recon => new Vector3(1f, 33f, 70f),
                ProfessionType.Medic => new Vector3(1f, 33f, 70f),
                ProfessionType.Assault => new Vector3(1f, 33f, 70f),
                _ => Vector3.zero,
            };
        }
        /// <summary>
        /// 获取个人中心展示站立得位置
        /// </summary>
        /// <param name="profession"></param>
        /// <returns></returns>
        public static Vector3 GetPersonalCenterShowPosition(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(0.0745f, 33f, 70f),
                ProfessionType.Recon => new Vector3(0.0745f, 33f, 70f),
                ProfessionType.Medic => new Vector3(0.0745f, 33f, 70f),
                ProfessionType.Assault => new Vector3(0.0745f, 33f, 70f),
                _ => Vector3.zero,
            };
        }

        ///<summary>
        ///获取选人展示站立得位置
        ///</summary>
        public static Vector3 GetSelectRolePosition(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(-1000f, 17f, 6f),
                ProfessionType.Recon => new Vector3(-1000f, 17f, 6f),
                ProfessionType.Medic => new Vector3(-1000f, 17f, 6f),
                ProfessionType.Assault => new Vector3(-1000f, 16.92f, 6f),
                _ => Vector3.zero,
            };
        }

        ///<summary>
        ///获取个人中心展示旋转角度
        ///</summary>
        public static Vector3 GetSelectRoleRotation(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(0, -180f, 0),
                ProfessionType.Recon => new Vector3(0, -180f, 0),
                ProfessionType.Medic => new Vector3(0, -180f, 0),
                ProfessionType.Assault => new Vector3(0, -180f, 0),
                _ => Vector3.zero,
            };
        }


        ///<summary>
        ///获取个人中心拉近后的位置
        ///</summary>
        /// <param name="profession"></param>
        /// <returns></returns>
        public static Vector3 GetPersonalCenterClosePosition(ProfessionType profession)
        {
            return profession switch
            {       
                ProfessionType.Engineer => new Vector3(0.5f, 33.4f, 74f),
                ProfessionType.Recon => new Vector3(0.5f, 33.4f, 74f),
                ProfessionType.Medic => new Vector3(0.5f, 33.4f, 74f),
                ProfessionType.Assault => new Vector3(0.5f, 33.5f, 74f),
                _ => Vector3.zero,
            };
        }

        ///<summary>
        ///获取个人中心展示旋转角度
        ///</summary>
        public static Vector3 GetPersonalCenterRotation(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(0, 0, -180f),
                ProfessionType.Recon => new Vector3(0, 0, -180f),
                ProfessionType.Medic => new Vector3(0, 0, -180f),
                ProfessionType.Assault => new Vector3(0, 0, -180f),
                _ => Vector3.zero,
            };
        }

        ///<summary>
        ///获取线上模式的单人角色展示站立的位置
        /// </summary>
        public static Vector3 GetOnlineModelPosition(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Engineer => new Vector3(0.074f, 33.11f, 69f),
                ProfessionType.Recon => new Vector3(0.074f, 33.11f, 69f),
                ProfessionType.Medic => new Vector3(0.074f, 33.11f, 69f),
                ProfessionType.Assault => new Vector3(0.074f, 33.27f, 69f),
                _ => Vector3.zero,
            };
        }

        ///<summary>
        ///获取线上模式小组成员五个组员站立的位置
        /// </summary>
        public static Vector3 GetOnlineModelPosition(int index)
        {
            return index switch
            {
                0 => new Vector3(0.0745f, 33.11f, 69f),
                1 => new Vector3(-1.226f, 33.11f, 68.5f),
                2 => new Vector3(-0.576f, 33.11f, 68.75f),
                3 => new Vector3(0.724f, 33.11f, 68.75f),
                4 => new Vector3(1.374f, 33.11f, 68.5f),
                _ => Vector3.zero,
            };
        }

        /// <summary>
        /// 获取线上模式小组成员五个组员站立的旋转角度
        /// </summary>
        public static Vector3 GetOnlineModelRotation(int index)
        {
            return index switch
            {
                0 => new Vector3(0, 0, -180f),
                1 => new Vector3(0, 0, -180f),
                2 => new Vector3(0, 0, -180f),
                3 => new Vector3(0, 0, -180f),
                4 => new Vector3(0, 0, -180f),
                _ => Vector3.zero,
            };
        }

        /// <summary>
        /// 获取角色立绘资源名称
        /// </summary>
        /// <param name="profession"></param>
        /// <returns></returns>
        public static string GetPortraitName(ProfessionType profession)
        {
            var modeName = profession switch
            {
                ProfessionType.Engineer => "Beekpr",
                ProfessionType.Recon => "Prism",
                ProfessionType.Medic => "Servant",
                ProfessionType.Assault => "Warden",
                _ => string.Empty,
            };
            return $"{modeName}Role";
        }

        /// <summary>
        /// 获取设备颜色对应的资源名称
        /// </summary>
        /// <param name="deviceColor"></param>
        /// <returns></returns>
        public static string GetDeviceColorResName(DeviceColor deviceColor)
        {
            return deviceColor switch
            {
                DeviceColor.ObsidianBlack => "Black",
                DeviceColor.MoonWhite => "White",
                DeviceColor.RoseQuartz => "Pink",
                DeviceColor.TiffanyBlue => "Blue",
                DeviceColor.ChinaRed => "Red",

                DeviceColor.HeiBai => "Black",
                DeviceColor.PearlWhite => "White",
                DeviceColor.PhantomPurple => "Blue",
                DeviceColor.MeteoriteWhite => "White",

                _ => string.Empty,
            };
        }
    }
}
