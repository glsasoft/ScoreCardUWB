namespace LetsPiu.Common.Network.Model.Mqtt
{
    public enum MqttMessageType
    {
        /// <summary>
        /// PING
        /// </summary>
        ping,
        /// <summary>
        /// 锁号
        /// </summary>
        replaceLogin,
        /// <summary>
        /// 场次详情
        /// </summary>
        playDetail,
        /// <summary>
        /// 对战玩家
        /// </summary>
        battlePlayer,
        /// <summary>
        /// 语音
        /// </summary>
        voice,
        /// <summary>
        /// [客户端是上报]我被击中了
        /// </summary>
        hit,
        /// <summary>
        /// [服务器下发]我击中了别人
        /// type 伤害类型 1:射击 2:技能效果 3:道具 4:卡片效果 5:毒圈 6:电子栅栏
        /// subType 子类型 未定义
        /// positionType 被击中部位 1: 枪 2: 头盔 3: 背部
        /// </summary>
        attackRecord,
        /// <summary>
        /// [服务器下发]我被击中了
        /// type 伤害类型 1:射击 2:技能效果 3:道具 4:卡片效果 5:毒圈 6:电子栅栏
        /// subType 子类型 未定义
        /// positionType 被击中部位 1: 枪 2: 头盔 3: 背部
        /// </summary>
        sufferRecord,
        /// <summary>
        /// 击杀
        /// </summary>
        kill,
        /// <summary>
        /// 电子围栏
        /// </summary>
        fence,
        /// <summary>
        /// 毒圈消息
        /// </summary>
        poisonCircle,
        /// <summary>
        /// 下发地图
        /// </summary>
        downMap,
        /// <summary>
        /// 技能特效生效
        /// </summary>
        visualEffect,
        /// <summary>
        /// 上传地图
        /// </summary>
        uploadMap,
        /// <summary>
        /// 技能
        /// 服务器下发消息
        /// 主题: /glsa/player/{房间 ID/{玩家短编号}
        /// 消息体:{"data":{"coolDownEndTime":"2025-04-1615:34:36.763","status":1,"type":2,"ultimateEnergy":5},"type":"skill"}
        /// type 技能类型 1:被动 2:小技能 3:终极技能 4:打靶模式技能
        /// status 状态 0:释放失败 1:释放成功 2:冷却时间变更
        /// coolDownEndTime 冷却结束时间
        /// ultimateEnergy终极技能能量条
        /// </summary>
        skill,
        /// <summary>
        /// 玩家角色变更消息
        /// </summary>
        playerProfession,
        /// <summary>
        /// 服务器下发的复活消息, 消息体是Player
        /// </summary>
        revive,
        /// <summary>
        /// 服务端一客户端 护甲
        /// </summary>
        armor,
        //【线上线下】购买护盾卡牌后，未在指定时间内缓慢消失
        shield,
        /// <summary>
        /// [客户端是上报]打靶模式击中消息
        /// </summary>
        tHit,
        /// <summary>
        /// [服务器下发]打靶模式击中记录
        /// </summary>
        tHitRecord,
        /// <summary>
        /// [服务器下发]打靶模式目标详情
        /// </summary>
        targetDeatils,
    }
}