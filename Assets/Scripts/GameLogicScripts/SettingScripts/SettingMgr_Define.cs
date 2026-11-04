using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.GameLogic
{
    public partial class SettingMgr
    {
        private readonly Dictionary<SettingID, int[]> settingOptions = new Dictionary<SettingID, int[]>()
        {
            [SettingID.FrameRate] = new int[] { 30, 60 },
            [SettingID.ResolutionRatio] = new int[]
            {
                (int)SettingOption.ResolutionRatio_Standard,//标清
                (int)SettingOption.ResolutionRatio_High,//高清
                (int)SettingOption.ResolutionRatio_UltraHigh //超清
            },
            [SettingID.GraphicsQuality] = new int[]
            {
                (int)SettingOption.GraphicsQuality_Low,//低
                (int)SettingOption.GraphicsQuality_Medium,//中
                (int)SettingOption.GraphicsQuality_High,//高
                (int)SettingOption.GraphicsQuality_UltraHigh,//极高
                (int)SettingOption.Custom,//自定义
            },
            [SettingID.AutoDownloadRes] = new int[]
            {
                (int)SettingOption.AutoDownloadRes_WiFi,//仅WiFi
                (int)SettingOption.AutoDownloadRes_DataAndWiFi,//移动网络及WiFi
            },
            [SettingID.SafeArea] = new int[]
            {
                (int)SettingOption.SafeArea_Small,//极小
                (int)SettingOption.SafeArea_Medium,//中等
                (int)SettingOption.SafeArea_Large,//极大
                (int)SettingOption.Custom,//自定义
            },
            [SettingID.SafeAreaDirection] = new int[]
            {
                (int)SettingOption.SafeAreaDirection_Fixed,//固定方向
                (int)SettingOption.SafeAreaDirection_Follow,//跟随朝向
            },
            [SettingID.BaseOperation_Offline] = new int[]
            {
                (int)SettingOption.BaseOperation_RightFire,//右侧开火
                (int)SettingOption.BaseOperation_LeftFire,//左侧开火
            },
            [SettingID.BaseOperation_Online] = new int[]
            {
                (int)SettingOption.BaseOperation_RightFire,//右侧开火
                (int)SettingOption.BaseOperation_LeftFire,//左侧开火
            },
            [SettingID.MapShow] = new int[]
            {
                (int)SettingOption.MapShow_Radar,//雷达图
                (int)SettingOption.MapShow_Graph,//坐标图
            },
            [SettingID.EnemyOutline] = new int[]
            {
                (int)SettingOption.Color_Red,//红
                (int)SettingOption.Color_Yellow,//黄
                (int)SettingOption.Color_Purple,//紫
            },
            [SettingID.FrontSightStyle_Offline] = new int[]
            {
                (int)SettingOption.FrontSightStyle_ORIGIN,
                (int)SettingOption.FrontSightStyle_CLASSIC,
                (int)SettingOption.FrontSightStyle_NEW,
            },
            [SettingID.FrontSightStyle_Online] = new int[]
            {
                (int)SettingOption.FrontSightStyle_ORIGIN,
                (int)SettingOption.FrontSightStyle_CLASSIC,
                (int)SettingOption.FrontSightStyle_NEW,
            },
            [SettingID.FrontSightColor_Offline] = new int[]
            {
                (int)SettingOption.Color_Red,//红
                (int)SettingOption.Color_Yellow,//黄
                (int)SettingOption.Color_Purple,//紫
                (int)SettingOption.Color_Green,//绿
                (int)SettingOption.Color_Blue,//蓝
            },
            [SettingID.FrontSightColor_Online] = new int[]
            {
                (int)SettingOption.Color_Red,//红
                (int)SettingOption.Color_Yellow,//黄
                (int)SettingOption.Color_Purple,//紫
                (int)SettingOption.Color_Green,//绿
                (int)SettingOption.Color_Blue,//蓝
            },
            [SettingID.VideoQuality] = new int[]
            {
                (int)SettingOption.VideoQuality_Normal,//普通
                (int)SettingOption.VideoQuality_High,//高清
                (int)SettingOption.VideoQuality_UltraHigh,//超清
            },
        };

        private readonly Dictionary<SettingID, SettingID[]> presetValueSettingIDs = new Dictionary<SettingID, SettingID[]>()
        {
            [SettingID.GraphicsQuality] = new SettingID[]
            {
                SettingID.RealTimeShadowEnable,
                SettingID.AntiAliasingEnable,
                SettingID.VolumeLightEnable,
                SettingID.SharpenEnable,
                SettingID.SoftLightEnable,
                SettingID.AmbientOcclusionEnable,
                SettingID.ImageEnhancementEnable,
            },
            [SettingID.SafeArea] = new SettingID[]
            {
                SettingID.SAFE_ZONE,
                SettingID.PERIPHERAL_ZONE,
            },
        };

        private readonly Dictionary<SettingID, Dictionary<SettingOption, int[]>> settingPresetValues = new Dictionary<SettingID, Dictionary<SettingOption, int[]>>()
        {
            [SettingID.GraphicsQuality] = new Dictionary<SettingOption, int[]>()
            {
                //低画质
                [SettingOption.GraphicsQuality_Low] = new int[] { 0, 0, 0, 0, 0, 0, 0, },
                //中画质
                [SettingOption.GraphicsQuality_Medium] = new int[] { 1, 1, 0, 0, 0, 0, 0, },
                //高画质
                [SettingOption.GraphicsQuality_High] = new int[] { 1, 1, 1, 1, 0, 0, 0, },
                //极高画质
                [SettingOption.GraphicsQuality_UltraHigh] = new int[] { 1, 1, 1, 1, 1, 1, 1, },
            },
            [SettingID.SafeArea] = new Dictionary<SettingOption, int[]>()
            {
                //极小
                [SettingOption.SafeArea_Small] = new int[] { 20, 20, },
                //中等
                [SettingOption.SafeArea_Medium] = new int[] { 50, 50, },
                //极大
                [SettingOption.SafeArea_Large] = new int[] { 80, 80, },
            },
        };

        private readonly Dictionary<SettingID, int> settingDefaultValues = new Dictionary<SettingID, int>()
        {
            [SettingID.FrameRate] = 0,//0 表示不限制，跑满设备刷新率
            [SettingID.ResolutionRatio] = (int)SettingOption.ResolutionRatio_High,//高清
            [SettingID.GraphicsQuality] = (int)SettingOption.GraphicsQuality_High,//高

            [SettingID.HideUiEnable] = 0,
            [SettingID.RenderOptimizeEnable] = 0,
            [SettingID.AutoDownloadRes] = (int)SettingOption.AutoDownloadRes_WiFi,//仅WiFi
            [SettingID.GlobalSoundEnable] = 1,
            [SettingID.BackgroundMusicEnable] = 1,
            [SettingID.BackgroundMusic] = 80,
            [SettingID.GameSoundEnable] = 1,
            [SettingID.GameSound] = 80,
            [SettingID.CharacterSoundEnable] = 1,
            [SettingID.CharacterSound] = 80,
            [SettingID.KeySoundEnable] = 1,
            [SettingID.KeySound] = 80,
            [SettingID.Loudspeaker] = 80,
            [SettingID.Microphone] = 80,
            [SettingID.JumpSensitivity] = 50,
            [SettingID.CrouchSensitivity] = 50,
            [SettingID.SafeArea] = (int)SettingOption.SafeArea_Medium,//中等

            [SettingID.SafeAreaDirection] = (int)SettingOption.SafeAreaDirection_Fixed,//固定方向
            [SettingID.SafeAreaOffset] = 50,
            [SettingID.BaseOperation_Offline] = (int)SettingOption.BaseOperation_RightFire,//右侧开火
            [SettingID.BaseOperation_Online] = (int)SettingOption.BaseOperation_RightFire,//右侧开火
            [SettingID.DamageShowEnable] = 1,
            [SettingID.MapShow] = (int)SettingOption.MapShow_Radar,//雷达图,
            [SettingID.EnemyOutline] = (int)SettingOption.Color_Red,//红,
            [SettingID.TeammateOutlineEnable] = 1,
            [SettingID.TeammateLifebarEnable] = 0,
            [SettingID.FrontSightStyle_Offline] = (int)SettingOption.FrontSightStyle_ORIGIN,
            [SettingID.FrontSightStyle_Online] = (int)SettingOption.FrontSightStyle_ORIGIN,
            [SettingID.FrontSightColor_Offline] = (int)SettingOption.Color_White,//白
            [SettingID.FrontSightColor_Online] = (int)SettingOption.Color_White,//白
            [SettingID.GlobalPhoneShakeEnable] = 1,
            [SettingID.PhoneFireShakeEnable] = 0,
            [SettingID.PhoneDeathShakeEnable] = 0,
            [SettingID.PhoneHitShakeEnable] = 1,
            [SettingID.PhoneSkillShakeEnable] = 0,

            [SettingID.DeviceBacklashEnable] = 1,
            [SettingID.DeviceLoudspeakerEnable] = 0,
            [SettingID.DevicePhysicalHUDEnable] = 0,
            [SettingID.GlobalDeviceShakeEnable] = 1,
            [SettingID.DeviceFireShakeEnable] = 0,
            [SettingID.DeviceDeathShakeEnable] = 0,
            [SettingID.DeviceHitShakeEnable] = 1,
            [SettingID.DeviceSkillShakeEnable] = 0,
            [SettingID.PiuTimeRecordEnable] = 1,
            [SettingID.FreeRecordEnable] = 0,
            [SettingID.VideoQuality] = (int)SettingOption.VideoQuality_High,//高清
        };

        private readonly Dictionary<SettingOption, Color> settingColorValues = new Dictionary<SettingOption, Color>()
        {
            [SettingOption.Color_White] = Color.white,
            [SettingOption.Color_Red] = new Color(1, 75f / 255, 75f / 255, 1),
            [SettingOption.Color_Yellow] = new Color(1, 246f / 255, 75f / 255, 1),
            [SettingOption.Color_Purple] = new Color(222f / 255, 75f / 255, 1, 1),
            [SettingOption.Color_Green] = new Color(75f / 255, 1, 147f / 255, 1),
            [SettingOption.Color_Blue] = new Color(75f / 255, 222f / 255, 1, 1),
        };
    }

    public enum SettingID
    {
        None,

        /// <summary>
        /// 帧率
        /// </summary>
        FrameRate,

        /// <summary>
        /// 分辨率
        /// </summary>
        ResolutionRatio,

        /// <summary>
        /// 画质
        /// </summary>
        GraphicsQuality,

        /// <summary>
        /// 实时阴影开关
        /// </summary>
        RealTimeShadowEnable,

        /// <summary>
        /// 抗锯齿开关
        /// </summary>
        AntiAliasingEnable,

        /// <summary>
        /// 体积光开关
        /// </summary>
        VolumeLightEnable,

        /// <summary>
        /// 锐化效果开关
        /// </summary>
        SharpenEnable,

        /// <summary>
        /// 柔光效果开关
        /// </summary>
        SoftLightEnable,

        /// <summary>
        /// 环境光遮蔽开关
        /// </summary>
        AmbientOcclusionEnable,

        /// <summary>
        /// 画面增强开关
        /// </summary>
        ImageEnhancementEnable,

        /// <summary>
        /// 隐藏游戏UI开关
        /// </summary>
        HideUiEnable,

        /// <summary>
        /// 渲染性能优化开关
        /// </summary>
        RenderOptimizeEnable,

        /// <summary>
        /// 自动下载资源
        /// </summary>
        AutoDownloadRes,

        /// <summary>
        /// 总体音效开关
        /// </summary>
        GlobalSoundEnable,

        /// <summary>
        /// 背景音乐开关
        /// </summary>
        BackgroundMusicEnable,

        /// <summary>
        /// 背景音乐
        /// </summary>
        BackgroundMusic,

        /// <summary>
        /// 游戏音效开关
        /// </summary>
        GameSoundEnable,

        /// <summary>
        /// 游戏音效
        /// </summary>
        GameSound,

        /// <summary>
        /// 人物音效开关
        /// </summary>
        CharacterSoundEnable,

        /// <summary>
        /// 人物音效
        /// </summary>
        CharacterSound,

        /// <summary>
        /// 按键音效开关
        /// </summary>
        KeySoundEnable,

        /// <summary>
        /// 按键音效
        /// </summary>
        KeySound,

        /// <summary>
        /// 扬声器
        /// </summary>
        Loudspeaker,

        /// <summary>
        /// 麦克风
        /// </summary>
        Microphone,

        /// <summary>
        /// 跳跃灵敏度
        /// </summary>
        JumpSensitivity,

        /// <summary>
        /// 下蹲灵敏度
        /// </summary>
        CrouchSensitivity,

        /// <summary>
        /// 安全区选项
        /// </summary>
        SafeArea,

        /// <summary>
        /// 安全区
        /// </summary>
        SAFE_ZONE,

        /// <summary>
        /// 外围区
        /// </summary>
        PERIPHERAL_ZONE,

        /// <summary>
        /// 安全区方向
        /// </summary>
        SafeAreaDirection,

        /// <summary>
        /// 安全区偏移
        /// </summary>
        SafeAreaOffset,

        /// <summary>
        /// 基本操作模式：线下
        /// </summary>
        BaseOperation_Offline,

        /// <summary>
        /// 基本操作模式：线上
        /// </summary>
        BaseOperation_Online,

        /// <summary>
        /// 伤害显示开关
        /// </summary>
        DamageShowEnable,

        /// <summary>
        /// 地图显示选项
        /// </summary>
        MapShow,

        /// <summary>
        /// 敌人描边
        /// </summary>
        EnemyOutline,

        /// <summary>
        /// 队友描边开关
        /// </summary>
        TeammateOutlineEnable,

        /// <summary>
        /// 队友血条开关
        /// </summary>
        TeammateLifebarEnable,

        /// <summary>
        /// 准星样式：线下
        /// </summary>
        FrontSightStyle_Offline,

        /// <summary>
        /// 准星样式：线上
        /// </summary>
        FrontSightStyle_Online,

        /// <summary>
        /// 准星颜色：线下
        /// </summary>
        FrontSightColor_Offline,

        /// <summary>
        /// 准星颜色：线上
        /// </summary>
        FrontSightColor_Online,

        /// <summary>
        /// 手机震动总开关
        /// </summary>
        GlobalPhoneShakeEnable,

        /// <summary>
        /// 手机开火震动开关
        /// </summary>
        PhoneFireShakeEnable,

        /// <summary>
        /// 手机死亡震动开关
        /// </summary>
        PhoneDeathShakeEnable,

        /// <summary>
        /// 手机受击震动开关
        /// </summary>
        PhoneHitShakeEnable,

        /// <summary>
        /// 手机技能震动开关
        /// </summary>
        PhoneSkillShakeEnable,

        /// <summary>
        /// 设备后坐力开关
        /// </summary>
        DeviceBacklashEnable,

        /// <summary>
        /// 设备后扬声器开关
        /// </summary>
        DeviceLoudspeakerEnable,

        /// <summary>
        /// 设备物理HUD开关
        /// </summary>
        DevicePhysicalHUDEnable,

        /// <summary>
        /// 设备震动总开关
        /// </summary>
        GlobalDeviceShakeEnable,

        /// <summary>
        /// 设备开火震动开关
        /// </summary>
        DeviceFireShakeEnable,

        /// <summary>
        /// 设备死亡震动开关
        /// </summary>
        DeviceDeathShakeEnable,

        /// <summary>
        /// 设备受击震动开关
        /// </summary>
        DeviceHitShakeEnable,

        /// <summary>
        /// 设备技能震动开关
        /// </summary>
        DeviceSkillShakeEnable,

        /// <summary>
        /// PIUTIME录屏开关
        /// </summary>
        PiuTimeRecordEnable,

        /// <summary>
        /// 自由录屏开关
        /// </summary>
        FreeRecordEnable,

        /// <summary>
        /// 录屏视频质量
        /// </summary>
        VideoQuality,
    }

    public enum SettingOption
    {
        None,

        /// <summary>
        /// 自定义
        /// </summary>
        Custom,

        /// <summary>
        /// 标清
        /// </summary>
        ResolutionRatio_Standard,
        /// <summary>
        /// 高清
        /// </summary>
        ResolutionRatio_High,
        /// <summary>
        /// 超清
        /// </summary>
        ResolutionRatio_UltraHigh,

        /// <summary>
        /// 低
        /// </summary>
        GraphicsQuality_Low,
        /// <summary>
        /// 中
        /// </summary>
        GraphicsQuality_Medium,
        /// <summary>
        /// 高
        /// </summary>
        GraphicsQuality_High,
        /// <summary>
        /// 极高
        /// </summary>
        GraphicsQuality_UltraHigh,

        /// <summary>
        /// 仅WiFi
        /// </summary>
        AutoDownloadRes_WiFi,
        /// <summary>
        /// 移动网络及WiFi
        /// </summary>
        AutoDownloadRes_DataAndWiFi,

        /// <summary>
        /// 极小
        /// </summary>
        SafeArea_Small,
        /// <summary>
        /// 中等
        /// </summary>
        SafeArea_Medium,
        /// <summary>
        /// 极大
        /// </summary>
        SafeArea_Large,

        /// <summary>
        /// 固定方向
        /// </summary>
        SafeAreaDirection_Fixed,
        /// <summary>
        /// 跟随朝向
        /// </summary>
        SafeAreaDirection_Follow,

        /// <summary>
        /// 右侧开火
        /// </summary>
        BaseOperation_RightFire,
        /// <summary>
        /// 左侧开火
        /// </summary>
        BaseOperation_LeftFire,

        /// <summary>
        /// 雷达图
        /// </summary>
        MapShow_Radar,
        /// <summary>
        /// 坐标图
        /// </summary>
        MapShow_Graph,

        /// <summary>
        /// 白
        /// </summary>
        Color_White,
        /// <summary>
        /// 红
        /// </summary>
        Color_Red,
        /// <summary>
        /// 黄
        /// </summary>
        Color_Yellow,
        /// <summary>
        /// 紫
        /// </summary>
        Color_Purple,
        /// <summary>
        /// 绿
        /// </summary>
        Color_Green,
        /// <summary>
        /// 蓝
        /// </summary>
        Color_Blue,

        FrontSightStyle_ORIGIN,
        FrontSightStyle_CLASSIC,
        FrontSightStyle_NEW,

        /// <summary>
        /// 普通
        /// </summary>
        VideoQuality_Normal,
        /// <summary>
        /// 高清
        /// </summary>
        VideoQuality_High,
        /// <summary>
        /// 超清
        /// </summary>
        VideoQuality_UltraHigh,
    }
}
