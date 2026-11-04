using System;
using System.Collections.Generic;
using System.Linq;
using Common.Network.Model.Http;
using Common.Utils;
using Cysharp.Threading.Tasks;
using LetsPiu;
using LetsPiu.Base;
using LetsPiu.Base.BaseScripts.Define;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App;
using LetsPiu.Common.Network.Model.Http.Play;
using LetsPiu.Define;
using LetsPiu.Main.Common.Scripts.Bluetooth;
using UnityEngine;

public static class AppDataUtil
{
    /// <summary>
    /// 获取队伍击杀比分
    /// </summary>
    /// <returns>a 红色方击杀数  b 蓝色方击杀数</returns>
    public static (int blueScore, int redScore) GetTeamKillScore()
    {
        if (AppStateManager.Instance.PlayerList is null)
        {
            return (0, 0);
        }

        var playerList = AppStateManager.Instance.PlayerList;

        int blueKill = 0;
        int redKill = 0;

        for (int i = 0; i < playerList.Count; i++)
        {
            var playerData = playerList[i];
            if (playerData.team == (int)TeamType.Blue)
            {
                redKill += playerData.deathNumber;
            }
            else if (playerData.team == (int)TeamType.Red)
            {
                blueKill += playerData.deathNumber;
            }
            else
            {
                Debug.Log("AppDataUtil getplayers error!!");
            }
        }

        return (blueKill, redKill);
    }

    public static (int blueScore, int redScore) GetTeamKillScore(List<Player> playerList)
    {
        if (playerList.Count ==0)
        {
            return (0, 0);
        }
        int blueKill = 0;
        int redKill = 0;
        for (int i = 0; i < playerList.Count; i++)
        {
            var playerData = playerList[i];
            if (playerData.team == (int)TeamType.Blue)
            {
                redKill += playerData.deathNumber;
            }
            else if (playerData.team == (int)TeamType.Red)
            {
                blueKill += playerData.deathNumber;
            }
            else
            {
                Debug.Log("AppDataUtil getplayers error!!");
            }
        }
        return (blueKill, redKill);
    }

    /// <summary>
    /// 全场最佳排序方法
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int SettlementGameBestSort(Player x, Player y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        // MVP排在最前面
        if (x.mvpStatus==1 )
        {
            return -1; // x是MVP，排在前面
        }
        else if (y.mvpStatus == 1)
        {
            return 1; // y是MVP，排在前面
        }
        var xPlayScore = x.playScore;
        var yPlayScore = y.playScore;
        //根据playScore排序
        return yPlayScore.CompareTo(xPlayScore); // 数值越高排名越高
    }

    /// <summary>
    /// 排序顺序：击杀、死亡、助攻、伤害、治疗、承伤、能量点、金色卡牌数、紫色卡牌数、蓝色卡牌数、绿色卡牌数、名字
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int SettlementSortPlayer(Player x, Player y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        //排序顺序：击杀、死亡、助攻、伤害、治疗、承伤、能量点、金色卡牌数、紫色卡牌数、蓝色卡牌数、绿色卡牌数、名字
        //其中死亡数越多排名越低，其他数字是越高排名越高

        // 比较击杀数
        if (x.killNumber != y.killNumber)
            return y.killNumber.CompareTo(x.killNumber); // 数值越高排名越高

        // 比较死亡数
        if (x.deathNumber != y.deathNumber)
            return x.deathNumber.CompareTo(y.deathNumber); // 数值越低排名越高

        // 比较助攻数
        if (x.assistNumber != y.assistNumber)
            return y.assistNumber.CompareTo(x.assistNumber); // 数值越高排名越高

        // 比较伤害值
        if (x.harm != y.harm)
            return y.harm.CompareTo(x.harm); // 数值越高排名越高

        // 比较治疗值
        if (x.treat != y.treat)
            return y.treat.CompareTo(x.treat); // 数值越高排名越高

        // 比较承伤值
        if (x.strikenScore != y.strikenScore)
            return y.strikenScore.CompareTo(x.strikenScore); // 数值越高排名越高

        // 比较能量点（处理可能为null的情况）
        if ((x.totalSp ?? 0) != (y.totalSp ?? 0))
            return (y.totalSp ?? 0).CompareTo(x.totalSp ?? 0); // 数值越高排名越高
        
        // 所有比较项都相同的情况下，按名字排序
        return string.Compare(x.name, y.name, StringComparison.Ordinal);
    }

    public static GameModeInfo GetModeInfo(GameMode gameMode)
    {
        var modeList = AppStateManager.Instance.GameModeInfoList;
        if (modeList is null)
        {
            return null;
        }

        for (int i = 0; i < modeList.Count; i++)
        {
            if (modeList[i].id == ((int)gameMode).ToString())
            {
                return modeList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 检查玩家在当前场次中是否被点赞过(临时存储)
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool IsPlayerLikedInPlay(Player player)
    {
        var play = AppStateManager.Instance.Play;
        if (play == null) return false;
        
        var likedList = AppStateManager.Instance.GetVariable<List<Player>>("PlayerLikedList");
        if (likedList == null)
        {
            return false;
        }
        foreach (var p in likedList)
        {
            if (p.playId == play.id && p.shortId == player.shortId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 设置玩家在当前场次中被点赞(临时存储)
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool SetPlayerLikedInPlay(Player player)
    {
        var play = AppStateManager.Instance.Play;
        if (play == null) return false;
        var likedList = AppStateManager.Instance.GetVariable<List<Player>>("PlayerLikedList");
        if (likedList == null)
        {
            likedList = new List<Player>();
            AppStateManager.Instance.SetVariable("PlayerLikedList", likedList);
        }
        foreach (var p in likedList)
        {
            if (p.playId == player.playId && p.shortId == player.shortId)
            {
                return false; // 已经点赞过了
            }
        }
        likedList.Add(player);
        //删除非当前场次的玩家数据
        likedList.RemoveAll(p => p.playId != play.id);
        return true;
    }

    /// <summary>
    /// 检查玩家是否还在房间中
    /// </summary>
    /// <param name="callback"></param>
    public static void CheckCanReturnRoom(Action<bool> callback = null)
    {
        HttpConnector.Instance.PlayerGetPlayerInfo(res =>
        {
            if (!res.success || res.data == null)
            {
                Debug.Log("CheckCanReturnRoom 房间信息查询失败 无法返回房间");
                callback?.Invoke(false);
            }
            else
            {
                var player = res.data;
                if (player.roomId != null)
                {
                    // 玩家仍在房间中
                    Debug.Log("CheckCanReturnRoom 玩家仍在房间中");
                    callback?.Invoke(true);
                }
                else
                {
                    // 玩家不在房间中
                    Debug.Log("CheckCanReturnRoom 玩家不在房间中 无法返回房间");
                    callback?.Invoke(false);
                }
            }
        });
    }

    /// <summary>
    /// 可以使用的职业列表
    /// </summary>
    /// <param name="list"></param>
    /// <param name="lineType"></param>
    public static void GetValidProfessionList(List<Profession> list, LineType lineType = LineType.Default)
    {
        if (list == null) return;
        list.Clear();

        foreach (var profession in AppStateManager.Instance.ProfessionList)
        {
            if (profession.lineType == lineType || profession.lineType == LineType.Default)
            {
                list.Add(profession);
            }
        }

        list.Sort((x, y) =>
        {
            // 按照职业名称排序
            return string.Compare(x.id, y.id, StringComparison.Ordinal);
        });
    }

    public static string GetOverrideControllerByShortId(int shortId, bool isFp = false)
    {
        var player = AppStateManager.Instance.GetPlayerByShortId(shortId);
        var profession = AppStateManager.Instance.GetProfession(player);
        string modelName = null;
        modelName = Helper.GetFPSModelResName(profession.Type);
        var gunId = AppDataUtil.GetGunIdByShortId(shortId);
        if (gunId == "0") gunId = "2";
        string ps = isFp ? "FPS" : "TPS";
        return $"AOC_{modelName}_{ps}_0{gunId}";
    }
    
    public static string GetGunResByShortId(int shortId)
    {
        var player = AppStateManager.Instance.GetPlayerByShortId(shortId);
        if (player is not null)
        {
            var gunList = AppStateManager.Instance.GunTypeList;
            var cnt = gunList.Count;
            for (int i = 0; i < cnt; i++)
            {
                if (gunList[i].id == player.gunVersion)
                {
                    if (player.gunVersion == SubDeviceType.None)
                    {
                        return gunList[i].avatarName;
                    }
                    else
                    {
                        return $"{gunList[i].avatarName}_{Helper.GetDeviceColorResName(player.gunColor)}";
                    }
                }
            }
        }

        return string.Empty;
    }
    
    public static string GetGunResByShortId1(int shortId)
    {
        Player player = AppStateManager.Instance.GetPlayerByShortId(shortId);
        String color = Helper.GetDeviceColorResName(player.gunColor);
        string id = GetGunIdByShortId(shortId);
        if (id == "0") return $"Prefab_Piu02_Skin_Virtual";
        else if (id == "1") return $"Prefab_Piu01_Skin_{color}";
        else return $"Prefab_Piu0{id}_Skin";
    }

    public static string GetGunIdByShortId(int shortId)
    {
        var player = AppStateManager.Instance.GetPlayerByShortId(shortId);
        if (player is not null)
        {
            var gunList = AppStateManager.Instance.GunTypeList;
            var cnt = gunList.Count;
            for (int i = 0; i < cnt; i++)
            {
                if (gunList[i].id == player.gunVersion)
                {
                    if (player.gunVersion == SubDeviceType.None)
                    {
                        return "0";
                    }
                    int versionValue = (int)player.gunVersion;
                    return versionValue.ToString();
                }
            }
        }

        return string.Empty;
    }
    
    /// <summary>
    /// 获取指定子物体
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="childName"></param>
    /// <returns></returns>
    public static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
    
    public static string GetAvatarNameByPlayer1(int shortId)
    {
        // var self = AppStateManager.Instance.GetPlayer();
        var player = AppStateManager.Instance.GetPlayerByShortId(shortId);
        // var isFPS = player.shortId == self.shortId;
        var isFPS = false;
        var profession = AppStateManager.Instance.GetProfession(player);

        var modelName = isFPS ? Helper.GetFPSModelRes1(profession.Type) : Helper.GetTPSModelRes1(profession.Type);
        
        return modelName;
    }

    public static string GetTPSAvatarNameByPlayer(int shortId)
    {
        var player = AppStateManager.Instance.GetPlayerByShortId(shortId);
        var profession = AppStateManager.Instance.GetProfession(player);
        
        var modelName = Helper.GetTPSModelRes1(profession.Type);
        
        return modelName;
    }
    
    /// <summary>
    /// 读取历史登录数据
    /// </summary>
    /// <returns></returns>
    public static List<LoginHistory> GetLoginHistory()
    {
        var loginHistory = PlayerPrefsUtils.GetObject(Constants.KEY_PREFS_LOGIN_HISTORY, new List<LoginHistory>()) ??
                           new List<LoginHistory>();
        return loginHistory;
    }

    /// <summary>
    /// 追加历史登录数据
    /// </summary>
    /// <param name="token"></param>
    /// <param name="customer"></param>
    public static void AppendLoginHistory(string token, Customer customer)
    {
        if (customer == null || token == null) return;
        var history = GetLoginHistory();
        var loginHistory = new LoginHistory
        {
            token = token,
            customer = customer,
            loginTime = DateTime.Now
        };
        //如果已经存在，则更新
        for (var i = 0; i < history.Count; i++)
        {
            if (history[i].customer.id != loginHistory.customer.id) continue;
            history[i] = loginHistory;
            PlayerPrefsUtils.SetObject(Constants.KEY_PREFS_LOGIN_HISTORY, history);
            return;
        }
        //如果不存在，则添加
        history.Add(loginHistory);

        // 如果历史记录超过10条，删除最老的记录
        const int maxHistoryCount = 10;
        if (history.Count > maxHistoryCount)
        {
            // 根据登录时间排序，最老的在前面
            history.Sort((x, y) => x.loginTime.CompareTo(y.loginTime));
            // 删除最老的记录直到数量符合限制
            while (history.Count > maxHistoryCount)
            {
                history.RemoveAt(0);
            }
        }

        PlayerPrefsUtils.SetObject(Constants.KEY_PREFS_LOGIN_HISTORY, history);
    }

    /// <summary>
    /// 删除最老的一个历史登录数据
    /// </summary>
    public static void DeleteLastLoginHistory()
    {
        var history = GetLoginHistory();
        if (history == null || history.Count == 0) return;

        // 根据登录时间对登录历史进行排序，最老的在前面
        history.Sort((x, y) => x.loginTime.CompareTo(y.loginTime));

        // 删除最老的一个历史登录数据
        history.RemoveAt(0);

        // 保存更新后的登录历史
        PlayerPrefsUtils.SetObject(Constants.KEY_PREFS_LOGIN_HISTORY, history);
    }

    /// <summary>
    /// 手机号加星号
    /// </summary>
    /// <param name="phone"></param>
    /// <returns></returns>
    public static string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 7)
            return phone;
        return phone.Substring(0, 3) + "****" + phone.Substring(7);
    }
    
    /// <summary>
    /// 获取队伍的2代枪灯光颜色
    /// </summary>
    /// <param name="playerTeam"></param>
    /// <returns></returns>
    public static BlueToothLightColor GetBlueToothLightTeamColor(int playerTeam)
    {
        var gameMode = AppStateManager.Instance.Play.type;
        var teamColor = BlueToothLightColor.Green;
        if (gameMode == GameMode.SPEED_MODE || gameMode == GameMode.HARD_CORE_MODE || gameMode == GameMode.ARENA_MODE || gameMode == GameMode.TEAM_SPORTS_MODE)
        {
            //红蓝队，按照红蓝显示
            if (playerTeam == 1)
            {
                teamColor = BlueToothLightColor.Red;
            }
            else
            {
                teamColor = BlueToothLightColor.Blue;
            }
        }

        return teamColor;
    }

    public static int GetFightEffectLayer(int senderShortId)
    {
        var player = AppStateManager.Instance.GetPlayerByShortId(senderShortId);
        if (player == null || player.battleStatus == PlayerBattleStatus.Dead)
        {
            return Layer.Default;
        }

        // var self = AppStateManager.Instance.GetPlayer();
        // if (self.shortId == senderShortId)
        // {
        //     return Layer.MainPlayer;
        // }
        
        // return player.team == self.team ? Layer.PLAYER : Layer.EnermyPlayer;
        return Layer.MainPlayer;
    }
}

