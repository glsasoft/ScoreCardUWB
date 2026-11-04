using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class Layer
    {
        public const int Default = 0;               //【默认层】，目前包含建筑和石头
        public const int TransparentFX = 1;         //【默认层】
        public const int IgnoreRaycast = 2;         //【默认层】
        public const int Water = 4;                 //【默认层】水
        public const int UI = 5;                    //【默认层】UI
        public const int HIDE_UI = 6;               //【自定义】隐藏的UI
        public const int AVATAR_SHOW = 7;           //【自定义】模型展示
        public const int MISSILE = 8;               //【自定义】投掷物
        public const int MAP = 9;                   //【自定义】地图场景模型
        public const int INVISIBLE = 10;            //【自定义】不可见
        public const int PLAYER = 11;               //【自定义】队友
        public const int Obstacles = 12;            //【自定义】障碍物
        public const int EnermyPlayer = 13;         //【自定义】敌人
        public const int MainPlayer = 14;           //【自定义】主角
        public const int RedSafeArea = 15;          //【自定义】红方安全区
        public const int BlueSafeArea = 16;         //【自定义】蓝方安全区
        public const int HomePage = 17;             //【自定义】首页模型
        public const int ShootingTarget = 18;       //【自定义】打靶目标
    }
}
