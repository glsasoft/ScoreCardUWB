using System;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using LetsPiu.Define;
using Newtonsoft.Json;

namespace Common.Network.Model.Http
{
    public class Customer
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string id;

        /// <summary>
        /// 用户唯一标识符UID
        /// </summary>
        public string uid;

        /// <summary>
        /// 手机号码
        /// </summary>
        public string phone;

        /// <summary>
        /// 姓名
        /// </summary>
        public string name;

        /// <summary>
        /// 性别(女0/男1)
        /// </summary>
        public int sex;

        /// <summary>
        /// 签名
        /// </summary>
        public string sign;

        /// <summary>
        /// 可修改(名称)次数
        /// </summary>
        public int modifiableTimes;

        /// <summary>
        /// 微信UnionID (如果适用)
        /// </summary>
        public string unionid;

        /// <summary>
        /// 微信OpenID (如果适用)
        /// </summary>
        public string openid;

        /// <summary>
        /// 是否可邀请 0：不可邀请 1：可邀请
        /// </summary>
        public int? canBeInvite;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string email;

        /// <summary>
        /// 皮肤资源ID
        /// </summary>
        public string skinId;
        /// <summary>
        /// 皮肤数量
        /// </summary>
        public int skinCount;
        /// <summary>
        /// 武器皮肤数量
        /// </summary>
        public int weaponSkinCount; 

        /// <summary>
        /// 背景资源ID
        /// </summary>
        public string backgroundId;

        /// <summary>
        /// 实名认证状态（-1 未认证 0成功 1认证中 2失败）
        /// </summary>
        public int realNameStatus;

        /// <summary>
        /// 职称/职位
        /// </summary>
        public string designation;

        /// <summary>
        /// 头像URL
        /// </summary>
        public string imageUrl;

        /// <summary>
        /// 武器熟练度
        /// </summary>
        public int gunProVal;
        /// <summary>
        /// 枪熟练度等级
        /// </summary>
        public int gunProLevel;
        /// <summary>
        /// 角色熟练度
        /// </summary>
        public int professionProVal;
        /// <summary>
        /// 角色熟练度等级
        /// </summary>
        public int professionProLevel;

        /// <summary>
        /// 当前绑定的枪
        /// </summary>
        public string gunCode;
        /// <summary>
        /// 当前绑定的头
        /// </summary>
        public string headCode;
        /// <summary>
        /// 枪subType
        /// </summary>
        public SubDeviceType? gunType = 0;
        /// <summary>
        /// 枪颜色
        /// </summary>
        public DeviceColor? gunColor = 0;

        /// <summary>
        /// 第一个绑定设备的玩家名称
        /// </summary>
        public string gunFirstBinderName;

        /// <summary>
        /// 当前用户绑定这把枪所累计的击败数
        /// </summary>
        public long gunKillNum;

        /// <summary>
        /// 外设数量
        /// </summary>
        public int deviceCount;

        /// <summary>
        /// 当前角色ID
        /// </summary>
        public string professionId;

        /// <summary>
        /// 总共花费的金币
        /// </summary>
        public long totalGoldSpent;



        /// <summary>
        /// 总共获得的金币
        /// </summary>
        public long totalGoldEarned;

        /// <summary>
        /// 最大连胜次数
        /// </summary>
        public int maxConsecutiveWins;

        /// <summary>
        /// 当前连胜次数
        /// </summary>
        public int currentStreak;

        /// <summary>
        /// 最大连败次数
        /// </summary>
        public int maxConsecutiveLosses;

        /// <summary>
        /// 连胜/连败标识
        /// </summary>
        public int streakFlag;

        /// <summary>
        /// 爆头次数
        /// </summary>
        public long headshotCount;

        /// <summary>
        /// 身体击中次数
        /// </summary>
        public long bodyshotCount;

        /// <summary>
        /// 好友数量
        /// </summary>
        public int friendsCount;

        /// <summary>
        /// MVP 次数
        /// </summary>
        public int mvpCount;

        /// <summary>
        /// SVP 次数
        /// </summary>
        public int svpCount;

        /// <summary>
        /// 钻石数量
        /// </summary>
        public int diamondCount;

        /// <summary>
        /// 金币数量
        /// </summary>
        public int goldCount;

        /// <summary>
        /// 新手教程完成标识
        /// </summary>
        public int tutorialCompleteFlag;

        /// <summary>
        /// 账号等级(默认1)
        /// </summary>
        public int level;

        /// <summary>
        /// 账号经验值
        /// </summary>
        public long exp;

        /// <summary>
        /// 线上积分赛的积分
        /// </summary>
        public long score;

        /// <summary>
        /// 最高积分
        /// </summary>
        public long bestScore;

        /// <summary>
        /// 最佳角色
        /// </summary>
        public long bestCharacter;

        /// <summary>
        /// 最高击杀
        /// </summary>
        public long mostKill;

        /// <summary>
        /// 点赞数量
        /// </summary>
        public long likesCount;

        /// <summary>
        /// 总场次次数
        /// </summary>
        public long playCount;

        /// <summary>
        /// 胜利次数
        /// </summary>
        public long winCount;

        /// <summary>
        /// 击杀数量
        /// </summary>
        public long killCount;

        /// <summary>
        /// 死亡数量
        /// </summary>
        public long deathCount;

        /// <summary>
        /// 助攻数量
        /// </summary>
        public long assistsCount;

        /// <summary>
        /// 治疗数量
        /// </summary>
        public long treatCount;

        /// <summary>
        /// 救援数量
        /// </summary>
        public long rescuesCount;

        /// <summary>
        /// 平均等级
        /// </summary>
        public long levelAvg;

        /// <summary>
        /// 奖杯数
        /// </summary>
        public int cupNum;

        /// <summary>
        /// 奖杯等级
        /// </summary>
        public int cupLevel;

        /// <summary>
        /// 累计登录天数
        /// </summary>
        public int cumulativeLoginDays;

        /// <summary>
        /// 拥有的角色数量
        /// </summary>
        public int ownedRolesCount;

        /// <summary>
        /// 判断玩家是否在线(0:不在线 1:在线)
        /// </summary>
        public int? online;
        
        /// <summary>
        /// 0:不是主理人 1:是主理人
        /// </summary>
        public ManagePeoType? managePeo = ManagePeoType.None;
        
        /// <summary>
        /// 当前连接的mqtt地址
        /// 例如: tcp://test.glsa.com.cn:8880
        /// </summary>
        public string netAddress;

        /// <summary>
        /// 好友界面添加情况标识    
        /// 0可申请/1已申请/2已添加
        /// </summary>
        public int canAdd;

        [JsonIgnore] public DeviceInfo gunInfo;
        [JsonIgnore] public DeviceInfo headInfo;
    }
}