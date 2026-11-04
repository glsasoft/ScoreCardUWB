using System.Collections;
using System.Collections.Generic;
using LetsPiu.Define;
using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories
{
    public class GunType : BaseEntity
    {

        /// <summary>
        /// subType ID
        /// </summary>
        public new SubDeviceType id;
        
        /// <summary>
        /// 名称
        /// </summary>
        public string name;

        /// <summary>
        /// icon
        /// </summary>
        public string icon;
        /// <summary>
        /// image
        /// </summary>
        public string image;
        /// <summary>
        /// 资源名称
        /// </summary>
        public string avatarName;
        /// <summary>
        /// 等级
        /// </summary>
        public int level;

        /// <summary>
        /// 爆头基础伤害
        /// </summary>
        public int headHarm;

        /// <summary>
        /// 胸部基础伤害
        /// </summary>
        public int harm;

        /// <summary>
        /// 血相关属性
        /// </summary>
        public int bloodPlus;

        /// <summary>
        /// 子弹数
        /// </summary>
        public int initBullet;

        /// <summary>
        /// 弹夹满子弹数
        /// </summary>
        public int bulletVolume;

        /// <summary>
        /// 射速
        /// </summary>
        public int shootSpeed;

        /// <summary>
        /// 换弹时间
        /// </summary>
        public double reloadSpeed;

        /// <summary>
        /// 开镜时间
        /// </summary>
        public double adsTime;

        /// <summary>
        /// 开镜子弹散布
        /// </summary>
        public double adsBulletSpread;

        /// <summary>
        /// 腰射子弹散布
        /// </summary>
        public double bulletSpread;

        /// <summary>
        /// 枪的类型(手枪、步枪等)
        /// </summary>
        public GunTypeEnum kind;
    }
}
