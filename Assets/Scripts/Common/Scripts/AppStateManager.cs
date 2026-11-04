using System;
using System.Collections.Generic;
using System.Linq;
using Common.Network.Model.Http;
using Common.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LetsPiu;
using LetsPiu.Base;
using LetsPiu.Base.BaseScripts.Define;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read;
using LetsPiu.Common.Network.Model.Http.Play;
using LetsPiu.Define;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using DeviceType = LetsPiu.Define.DeviceType;

public class AppStateManager : SingletonComponent<AppStateManager>
{
    public enum UserAuth
    {
        Admin,
        Customer,
        Manager,
    }

    /// <summary>
    /// Token
    /// </summary>
    private string token;

    /// <summary>
    /// 场次信息
    /// </summary>
    private Play play;

    /// <summary>
    /// 场次结算数据
    /// </summary>
    private PlaySummaryDetail playSummaryDetail;

    /// <summary>
    /// 当前场次中玩家的卡牌数量(目前在打开比分面板时会更新)
    /// </summary>
    private Dictionary<string, int> m_PlayersCardCount = new Dictionary<string, int>();
    
    private List<Player> _playerList;
    private int _playerListCount = 0;
    private Dictionary<string, Player> _playerCustomerIdMap = new();
    private Dictionary<int, Player> _playerShortIdMap = new();

    private List<Team> teamList;

    ///<summary>
    ///好友列表
    /// </summary>
    private List<Customer> friendList = new List<Customer>();
    
    /// <summary>
    /// 枪械类型列表项
    /// </summary>
    private List<GunType> gunTypeList;

    /// <summary>
    /// 击杀记录
    /// </summary>
    private List<KillRecord> killRecordList;
    /// <summary>
    /// 角色列表
    /// </summary>
    private List<Profession> professionList;
    /// <summary>
    /// 技能列表
    /// </summary>
    private List<Skill> skillList;

    private List<GameModeInfo> gameModeInfoList;

    /// <summary>
    /// 队伍击杀排行榜
    /// </summary>
    private List<Team> teamKillList;

    private DeviceInfo5S _deviceInfo5S;

    /// <summary>
    /// 用户权限类型
    /// </summary>
    private UserAuth auth = UserAuth.Admin;

    /// <summary>
    /// 本地时间跟服务器的时间差
    /// </summary>
    private TimeSpan serverTimeOffset = TimeSpan.FromSeconds(0);
    
    /// <summary>
    /// 枪械类型列表
    /// </summary>
    public List<GunType> GunTypeList
    {
        get
        {
            gunTypeList ??= new List<GunType>();
            return gunTypeList;
        }
        set => gunTypeList = value;
    }

    public DeviceInfo5S DeviceInfo5S
    {
        get => _deviceInfo5S;
        set
        {
            const string logPrefix = "[DeviceInfo5S.Persist]";
            _deviceInfo5S = value;
            if (_deviceInfo5S == null)
            {
                DeviceInfoStorage.Delete();
                Debug.LogWarning($"{logPrefix} 已清除本地缓存。appId={Application.identifier}");
                return;
            }

            string serialized;
            try
            {
                serialized = JsonConvert.SerializeObject(_deviceInfo5S);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{logPrefix} 序列化失败，未写入: {ex.Message}");
                return;
            }

            if (!DeviceInfoStorage.Write(serialized))
            {
                Debug.LogError($"{logPrefix} 写入外部存储失败，path={DeviceInfoStorage.MainPath}");
            }

            _deviceInfo5S.PrintDeviceInfo($"{logPrefix} 内存对象(已写入文件)");
            Debug.Log(
                $"{logPrefix} 写入序列化 JSON 长度={serialized.Length}，IsValid={_deviceInfo5S.IsValid()}，字段诊断={_deviceInfo5S.GetInvalidDataInfo()}，path={DeviceInfoStorage.MainPath}");
            Debug.Log($"{logPrefix} 完整 JSON（排查字段名/缺项）: {serialized}");

            LogDeviceInfo5SStorageSnapshot($"{logPrefix} 写入后立即回读校验");
        }
    }

    /// <summary>
    /// 读取外部存储文件中当前存的 DeviceInfo5S 快照（用于注册成功后核对是否落盘）。
    /// </summary>
    public void LogDeviceInfo5SStorageSnapshot(string tag)
    {
        var raw = DeviceInfoStorage.ReadValid(_ => true);
        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning($"[{tag}] 外部存储文件为空或不存在，path={DeviceInfoStorage.MainPath}");
            return;
        }

        Debug.Log($"[{tag}] 外部存储 path={DeviceInfoStorage.MainPath} 回读 raw 长度={raw.Length}");
        var previewLen = Mathf.Min(400, raw.Length);
        Debug.Log($"[{tag}] raw 前{previewLen}字符: {raw.Substring(0, previewLen)}");

        try
        {
            var fromDisk = JsonConvert.DeserializeObject<DeviceInfo5S>(raw);
            if (fromDisk == null)
            {
                Debug.LogError($"[{tag}] 反序列化结果为 null（JSON 可能为字面量 null 或无法映射到类型）");
                return;
            }

            Debug.Log(
                $"[{tag}] 回读反序列化成功，IsValid={fromDisk.IsValid()}，字段诊断={fromDisk.GetInvalidDataInfo()}");
            fromDisk.PrintDeviceInfo($"[{tag}] 回读对象");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{tag}] 回读反序列化失败: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 玩家列表
    /// </summary>
    public List<Player> PlayerList
    {
        get
        {
            if (_playerList == null)
            {
                _playerList = new List<Player>();
            }
            return _playerList;
        }
        set
        {
            _playerList = value;
            _playerListCount = value == null ? 0 : value.Count;

            _playerCustomerIdMap.Clear();
            _playerShortIdMap.Clear();
            if (value != null)
            {
                foreach (Player player in value)
                {
                    if (player != null)
                    {
                        if (!string.IsNullOrEmpty(player.customerId))
                        {
                            _playerCustomerIdMap[player.customerId] = player;
                        }
                        _playerShortIdMap[player.shortId] = player;
                    }
                }
            }
            EventManager.Instance.Broadcast(EventMsg.SetPlayerList);
        }
    }

    private Dictionary<string, Player> m_PlayerCustomerIdMap
    {
        get
        {
            if (PlayerList.Count != _playerListCount)
                PlayerList = PlayerList;

            return _playerCustomerIdMap;
        }
    }

    private Dictionary<int, Player> m_PlayerShortIdMap
    {
        get
        {
            if (PlayerList.Count != _playerListCount)
                PlayerList = PlayerList;

            return _playerShortIdMap;
        }
    }

    /// <summary>
    /// 队伍列表
    /// </summary>
    public List<Team> TeamList
    {
        get
        {
            teamList ??= new List<Team>();
            return teamList;
        }
        set => teamList = value;
    }

    /// <summary>
    /// 队伍排名列表
    /// </summary>
    public List<Team> TeamKillList
    {
        get
        {
            teamKillList ??= new List<Team>();
            return teamKillList;
        }
        set => teamKillList = value;
    }

    /// <summary>
    /// 技能列表
    /// </summary>
    public List<Skill> SkillList
    {
        get
        {
            skillList ??= new List<Skill>();
            return skillList;
        }
        set => skillList = value;
    }

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<Profession> ProfessionList
    {
        get
        {
            professionList ??= new List<Profession>();
            return professionList;
        }
        set => professionList = value;
    }

    /// <summary>
    /// 击杀记录列表
    /// </summary>
    public List<KillRecord> KillRecordList
    {
        get
        {
            killRecordList ??= new List<KillRecord>();
            return killRecordList;
        }
        set => killRecordList = value;
    }

    /// <summary>
    /// 获取模式列表
    /// </summary>
    public List<GameModeInfo> GameModeInfoList
    {
        get
        {
            gameModeInfoList ??= new List<GameModeInfo>();
            return gameModeInfoList;
        }
        set => gameModeInfoList = value;
    }

    /// <summary>
    /// 通过Id获取当前模式的Group信息
    /// </summary>
    /// <param name="id">GameModeInfoList中的某条Item的唯一id</param>
    /// <returns></returns>
    public GameModeInfo GetCurrentGameModeInfoById(string id)
    {
        var allGameModeInfos = Instance.GameModeInfoList;
        var curGroup = allGameModeInfos.FirstOrDefault(x => x.id == id);
        return curGroup;
    }

    /// <summary>
    /// 当前打靶模式的Id
    /// </summary>
    public string CurrentTargetShootingModeId { get; set; }
    public TargetShootingMode CurrentTargetShootingMode
    {
        get
        {
            if (Enum.TryParse(CurrentTargetShootingModeId, out TargetShootingMode result))
                return result;
            return TargetShootingMode.None;
        }
    }

    /// <summary>
    /// 当前登录的Token
    /// </summary>
    public string Token
    {
        get => token;
        set
        {
            token = value;
            PlayerPrefsUtils.SetObject(Constants.KEY_PREFS_TOKEN_INFO, token);
        }
    }

    /// <summary>
    /// 场次信息
    /// </summary>
    public Play Play
    {
        get => play;
        set => play = value;
    }

    /// <summary>
    /// 场次结算信息
    /// </summary>
    public PlaySummaryDetail PlaySummaryDetail
    {
        get => playSummaryDetail;
        set => playSummaryDetail = value;
    }
    
    /// <summary>
    /// 当前场次中玩家的卡牌数量
    /// </summary>
    public Dictionary<string, int> PlayersCardCounts
    {
        get => m_PlayersCardCount;
        set => m_PlayersCardCount = value;
    }

    /// <summary>
    /// 场次信息
    /// </summary>
    public UserAuth Auth
    {
        get => auth;
        set => auth = value;
    }

    private readonly Dictionary<string, object> variables = new Dictionary<string, object>();

    private void Awake()
    {
        Debug.Log("AppStateManager Awake...");
        LoadStorage();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddEvent<SceneType>(EventMsg.SceneSwitchSuccess, HandleSceneSwitchSuccess);
    }
    
    private void OnDisable()
    {
        EventManager.Instance.RemoveEvent<SceneType>(EventMsg.SceneSwitchSuccess, HandleSceneSwitchSuccess);
    }
    
    private void HandleSceneSwitchSuccess(SceneType sceneType)
    {
        Debug.Log("AppStateManager HandleSceneSwitchSuccess sceneType=" + sceneType);
        // 防止屏幕休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        Debug.Log("AppStateManager Start方法....");
        // 启用传感器
        Input.compass.enabled = true;
        // 暂时关闭，避免启动时触发定位/GPS 相关权限申请（需要时再取消注释）
        // Input.location.Start();
        //同步服务器时间
        // SyncServerTime().Forget();
        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    /// <summary>
    /// 获取玩家绑定的枪，没有返回null
    /// </summary>
    /// <returns></returns>
    public DeviceInfo GetPlayerGunInfo(Player player)
    {
        if (player?.gunCode == null || player.gunCode.Trim().Length == 0)
        {
            return null;
        }
        return new DeviceInfo
        {
            type = DeviceType.Gun,
            code = player.gunCode,
            colour = player.gunColor,
            subType = player.gunVersion ?? SubDeviceType.FirstGen
        };
    }

    /// <summary>
    /// 从外部存储文件加载 DeviceInfo5S（主文件损坏时自动回退备份），并打印：无数据 / 反序列化失败 / 缺字段。
    /// </summary>
    private void LoadDeviceInfo5SFromStorage()
    {
        const string logPrefix = "[DeviceInfo5S.LoadStorage]";
        _deviceInfo5S = null;

        Debug.Log(
            $"{logPrefix} 开始读取 appId={Application.identifier} platform={Application.platform} path={DeviceInfoStorage.MainPath}");

        var raw = DeviceInfoStorage.ReadValid(IsParseableDeviceInfo5S);

        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning($"{logPrefix} 无本地数据：外部存储文件为空或不存在（从未写入或已被清除）");
            return;
        }

        Debug.Log($"{logPrefix} 读到原始 JSON 长度={raw.Length}");
        var previewLen = Mathf.Min(500, raw.Length);
        Debug.Log($"{logPrefix} raw 前{previewLen}字符: {raw.Substring(0, previewLen)}");

        try
        {
            _deviceInfo5S = JsonConvert.DeserializeObject<DeviceInfo5S>(raw);
        }
        catch (Exception e)
        {
            _deviceInfo5S = null;
            Debug.LogError(
                $"{logPrefix} JSON 反序列化失败: {e.GetType().Name}: {e.Message}\n{e.StackTrace}\n（请核对 JSON 字段名是否为 umeng_flag/uri/device_name 等 snake_case）");
            return;
        }

        if (_deviceInfo5S == null)
        {
            Debug.LogError($"{logPrefix} 反序列化结果为 null（例如 JSON 为字面量 null）");
            return;
        }

        var valid = _deviceInfo5S.IsValid();
        Debug.Log(
            $"{logPrefix} 反序列化成功，IsValid={valid}，字段诊断={_deviceInfo5S.GetInvalidDataInfo()}");
        _deviceInfo5S.PrintDeviceInfo($"{logPrefix} 内存对象");
        if (!valid)
        {
            Debug.LogWarning(
                $"{logPrefix} 对象存在但 IsValid=false，LoginPanel 会走注册流程。请对照上方字段诊断补齐服务端/表单返回字段。");
        }
    }

    /// <summary>
    /// 读取持久化存储的数据
    /// </summary>
    private void LoadStorage()
    {
        LoadDeviceInfo5SFromStorage();
        token = PlayerPrefsUtils.GetObject<string>(Constants.KEY_PREFS_TOKEN_INFO);
    }

    /// <summary>
    /// 仅判断 JSON 能否反序列化成非空 DeviceInfo5S（用于主/备文件选择，区分"损坏"与"字段不全"）。
    /// </summary>
    private static bool IsParseableDeviceInfo5S(string raw)
    {
        try
        {
            return JsonConvert.DeserializeObject<DeviceInfo5S>(raw) != null;
        }
        catch
        {
            return false;
        }
    }

    /*
        //设置一个全局变量
        AppStateManager.Instance.SetVariable(Constants.KEY_VARIABLE_FROM_ROOM_SCENE, true);
        AppStateManager.Instance.SetVariable("varString", "字符串临时变量");
        
        //获取一个全局变量
        var fromRoom = AppStateManager.Instance.GetVariable<bool>(Constants.KEY_VARIABLE_FROM_ROOM_SCENE);
        var str = AppStateManager.Instance.GetVariable<string>("varString");
     */

    /// <summary>
    /// 设置一个全局变量(如果不同模块都要访问同一个key，可以把key定义在Constants中，防止拼写错误)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <typeparam name="T"></typeparam>
    public void SetVariable<T>(string key, T val)
    {
        variables[key] = val;
    }

    /// <summary>
    /// 获取游戏状态(从房间或者游戏中获取)
    /// </summary>
    /// <returns></returns>
    public GameState GetGameState()
    {
        if (Play != null)
        {
            //Debug.Log("AppStateManager GetGameState playStatus=" + Play.playStatus);
            return Play.playStatus;
        }
        else
        {
            return GameState.END;
        }
    }

    /// <summary>
    /// 是否正在游戏中
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        var playTest = Play;
        if (playTest == null) return false;
        switch (playTest.playStatus)
        {
            case GameState.END:
            case GameState.PREPARING:
            case GameState.LOCK:
                return false;
            default:
                return true;
        }
    }

    /// <summary>
    /// 获取游戏状态(从房间或者游戏中获取)
    /// </summary>
    /// <returns></returns>
    public GameMode GetGameMode()
    {
        return GameMode.ARENA_MODE;
    }
    
    /// <summary>
    /// 获取除了观战者以外的玩家列表
    /// </summary>
    /// <param name="playerList"></param>
    public void GetGamePlayerList(List<Player> playerList)
    {
        if (playerList is null)
        {
            return;
        }
        playerList.Clear();
        foreach (var player in PlayerList)
        {
            if (player.watchFlg != WatchFlag.Watching)
            {
                playerList.Add(player);
            }
        }
    }

    /// <summary>
    /// 传一个Team的下标获取Team的PlayerList
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="playerList"></param>
    /// <param name="from">从那个列表中获取，默认是PlayerList</param>
    public void GetTeamPlayerList(int teamId, List<Player> playerList, List<Player> from = null)
    {
        if (playerList is null)
        {
            return;
        }
        playerList.Clear();
        var fromList = from ?? PlayerList;
        foreach (var player in fromList)
        {
            if (player.team == teamId)
            {
                playerList.Add(player);
            }
        }
    }

    /// <summary>
    /// 传一个Team的下标获取非Team的PlayerList
    /// </summary>
    /// <param name="teamType">红蓝队</param>
    /// <param name="playerList"></param>
    /// <param name="from">从那个列表中获取，默认是PlayerList</param>
    public void GetTeamPlayerListOther(int teamType, List<Player> playerList, List<Player> from = null)
    {
        if (playerList is null)
        {
            Debug.LogError("AppStateManager GetTeamPlayerListOther playerList is null");
            return;
        }
        playerList.Clear();
        var fromList = from ?? PlayerList;
        foreach (var player in fromList)
        {
            //Debug.LogWarning("AppStateManager GetTeamPlayerListOther player.team=" + player.team + " teamId=" + teamType);
            if (player.team != teamType)
            {
                playerList.Add(player);
            }
        }
    }

    /// <summary>
    /// 获取指定id的Player
    /// </summary>
    public Player GetPlayer(string CustomerId)
    {
        if (string.IsNullOrEmpty(CustomerId))
            return null;
        if (m_PlayerCustomerIdMap.TryGetValue(CustomerId, out var player))
            return player;
        return null;
    }

    /// <summary>
    /// 获取玩家的角色
    /// </summary>
    /// <returns></returns>
    [CanBeNull]
    public Profession GetProfession(Player player)
    {
        if (player is null) return null;
        return GetProfession(player.professionId);
    }

    /// <summary>
    /// 获取玩家职业ID
    /// </summary>
    /// <param name="professionId"></param>
    /// <returns></returns>
    [CanBeNull]
    public Profession GetProfession(string professionId)
    {
        for (int i = 0; i < ProfessionList.Count; i++)
        {
            if (ProfessionList[i].id == professionId)
            {
                return professionList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 根据角色类型获取角色
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Profession GetProfession(ProfessionType type)
    {
        for (int i = 0; i < ProfessionList.Count; i++)
        {
            var professicon = ProfessionList.Get(i);
            if (professicon.Type == type)
            {
                return professicon;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 获取玩家当前大技能/小技能是否在激活中
    /// </summary>
    /// <param name="type"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool IsSkillActive(SkillType type, Player player = null)
    {
        if (player == null)
        {
            Debug.LogError("AppStateManager GetPlayerSkillStatus player is null");
            return false;
        }

        if (type == SkillType.SmallSkill)
        {
            //获取小技能的持续结束时间
            var smallSkillEndTime = player.skillEndTime;
            if (smallSkillEndTime == null)
            {
                return false;
            }
            var currentTime = GetServerTime();
            if (currentTime < smallSkillEndTime)
            {
                Debug.LogWarning("AppStateManager GetPlayerSkillStatus 小技能释放中...");
                // 小技能正在释放中
                return true;
            }
            else
            {
                Debug.LogWarning("AppStateManager GetPlayerSkillStatus 小技能已结束...");
                // 小技能未在释放中
                return false;
            }
        }else if (type == SkillType.UltimateSkill)
        {
            //获取终极技能的持续结束时间
            var ultimateSkillEndTime = player.ultimateSkillEndTime;
            if (ultimateSkillEndTime == null)
            {
                return false;
            }
            var currentTime = GetServerTime();
            if (currentTime < ultimateSkillEndTime)
            {
                // 终极技能正在释放中
                Debug.LogWarning("AppStateManager GetPlayerSkillStatus 终极技能释放中...");
                return true;
            }
            else
            {
                // 终极技能未在释放中
                Debug.LogWarning("AppStateManager GetPlayerSkillStatus 终极技能已结束...");
                return false;
            }
        }
        Debug.LogWarning("AppStateManager GetPlayerSkillStatus 技能处于其他状态: type="+type);
        return false;
    }
    
    /// <summary>
    /// 技能是否冷却中
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsSkillCooling(SkillType type, Player player = null)
    {
        var skill = GetPlayerSkill(player, type);
        if (skill == null)
        {
            Debug.LogError("AppStateManager IsSkillCooling skill==null type=" + type);
            return false;
        }
        if (player == null)
        {
            Debug.LogError("AppStateManager IsSkillCooling self==null");
            return false;
        }
        if (type == SkillType.UltimateSkill)
        {
            // 大招
            return player.ultimateEnergy < skill.ChargingPoints;
        }
        else
        {
            // 小技能
            return player.coolDownEndTime != null && GetServerTime() < player.coolDownEndTime.Value;
        }
    }

    /// <summary>
    /// 获取玩家的技能
    /// </summary>
    /// <returns></returns>
    [CanBeNull]
    public Skill GetPlayerSkill(Player player, SkillType type)
    {
        return SkillList.FirstOrDefault(skill => skill.ProfessionId == player.professionId && skill.Type == type);
    }

    /// <summary>
    /// 获取Player
    /// </summary>
    public Player GetPlayerByShortId(int shortId)
    {
        if (m_PlayerShortIdMap.TryGetValue(shortId, out var player))
            return player;
        return null;
    }

    /// <summary>
    /// 读取一个全局变量
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetVariable<T>(string key)
    {
        if (!variables.ContainsKey(key))
        {
            return default;
        }
        return (T)variables[key];
    }
    
    /// <summary>
    /// 根据teamlist里的id按顺序从小到大排序返回一个新的list
    /// </summary>
    public List<Team> GetSortedTeamListById()
    {
        //过滤掉空的队伍
        TeamList.RemoveAll(item => item == null);
        return TeamList?.OrderBy(team => team.teamId).ToList() ?? new List<Team>();
    }

    ///<summary>
    ///根据teamlist里的id按顺序从小到大排序返回一个新的list,然后传入team的id，返回team
    ///</summary>
    public Team GetTeamById(int id)
    {
        return TeamList?.Find(team => team.teamId == id);
    }

    /// <summary>
    /// 获取并更新用户的小组、房间、队伍、玩家列表等数据
    /// </summary>
    public async UniTask UpdatePlayerInfo()
    {
        var playerInfo = await HttpConnector.Instance.PlayerGetPlayerInfoAsync();
        if (!playerInfo.success) return;
        if (playerInfo.data == null)
        {
            Debug.LogWarning("AppStateManager UpdatePlayerInfo 玩家信息不存在! 删除房间和Play数据");
            Instance.Play = null;
            Instance.PlayerList = new List<Player>();
            return;
        }
        var player = playerInfo.data;
        Debug.Log("LoginPanel GetPlayerInfo() 玩家信息获取成功...");
        var players = new List<Player>() { player };
        var teams = new List<Team>();
        Play tmpPlay = null;
        PlayerList = players;
        TeamList = teams;
        Play = tmpPlay;
    }

    /// <summary>
    /// 获取服务器时间跟本地时间的差值，考虑网络延迟
    /// </summary>
    public async UniTask SyncServerTime()
    {
        //确保服务器时间至少同步成功了一次
        for (var i = 0; i < 100; i++)
        {
            try
            {
                var localStart = DateTime.Now;
                var serverTime = await HttpConnector.Instance.GetServerTime();
                var localEnd = DateTime.Now;
                // 取本地请求前后时间的中值，减小网络延迟误差
                var localMid = localStart + TimeSpan.FromMilliseconds((localEnd - localStart).TotalMilliseconds / 2);
                serverTimeOffset = serverTime - localMid;
                //时间同步成功，结束循环
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError("SyncServerTime 获取服务器时间失败: " + ex);
            }
            await UniTask.WaitForSeconds(10);
        }
    }

    /// <summary>
    /// 获取服务器当前时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetServerTime()
    {
        return DateTime.Now + serverTimeOffset;
    }
    
    /// <summary>
    /// 点击指定UI事件
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="callback"></param>
    public void OnClickUIEvent(RectTransform rectTransform, Action callback)
    {
        if (!IsPointerOverUIObject(rectTransform))
        {
            callback?.Invoke();
        }

    }
    private bool IsPointerOverUIObject(RectTransform rectTransform)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        if (results.Count <= 0) return false;
        var clickedObject = results[0].gameObject;
        if (clickedObject == null) return false;

        Transform currentTransform = clickedObject.transform;
        while (currentTransform != null)
        {
            if (currentTransform == rectTransform)
            {
                return true;
            }
            currentTransform = currentTransform.parent;
        }

        return false;
    }

    /// <summary>
    /// 根据角色id获取技能列表
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public void GetSkillByJob(ProfessionType job, LineType lineType, List<Skill> datalist)
    {
        if (datalist == null)
        {
            return;
        }

        datalist.Clear();

        for (int i = 0; i < SkillList.Count; i++)
        {
            var skillItem = SkillList.Get(i);
            if (skillItem.ProfessionId == ((int)job).ToString() && skillItem.LineType == lineType)
            {
                datalist.Add(skillItem);
            }
        }

        datalist.Sort((x, y) =>
        {
            return x.Type.CompareTo(y.Type);
        });
    }

    /// <summary>
    /// 是否有经济系统
    /// </summary>
    /// <returns></returns>
    public bool HasEconomy()
    {
        var play = Play;
        if (play == null)
        {
            Debug.LogError("AppStateManager HasEconomy Play is null");
            return false;
        }
        //极速模式、死斗模式、硬核模式没有经济系统
        return play.type != GameMode.SPEED_MODE && play.type != GameMode.DEAD_FIGHT_MODE && play.type != GameMode.HARD_CORE_MODE && play.type != GameMode.SINGLE_TARGET_SHOOTING;
    }

    /// <summary>
    /// 是否有经济系统的购物面板
    /// </summary>
    /// <returns></returns>
    public bool HasEconomyShoppingPanel()
    {
        var play = Play;
        if (play == null)
        {
            Debug.LogError("AppStateManager HasEconomyShoppingPanel Play is null");
            return false;
        }
        //硬核模式、大逃杀模式、死斗模式、极速模式没有经济系统的购物面板
        return play.type != GameMode.HARD_CORE_MODE && play.type != GameMode.DEAD_FIGHT_MODE && play.type != GameMode.SPEED_MODE && play.type != GameMode.ESCAPE_KILL_MODE_ONE && play.type != GameMode.ESCAPE_KILL_MODE_TWO && play.type != GameMode.ESCAPE_KILL_MODE_FOUR && play.type != GameMode.SINGLE_TARGET_SHOOTING;
    }
    private string currentRoundLikeCount;
    public string CurrentRoundLikeCount
    {
        get => currentRoundLikeCount;
        set
        {
            currentRoundLikeCount = value;
        }
    }
}
