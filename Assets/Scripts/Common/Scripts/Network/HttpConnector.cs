using System;
using System.Collections.Generic;
using Common.Network.Model.Http;
using Common.Network.Model.Http.Login;
using Common.Utils;
using Cysharp.Threading.Tasks;
using LetsPiu;
using LetsPiu.Base;
using LetsPiu.Base.BaseScripts.Define;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Players;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read;
using LetsPiu.Base.BaseScripts.Utils;
using LetsPiu.Common.Network.Model.Http;
using LetsPiu.Common.Network.Model.Http.App;
using LetsPiu.Common.Network.Model.Http.Customer;
using LetsPiu.Common.Network.Model.Http.Play;
using LetsPiu.Define;
using LetsPiu.Main.Common.Scripts;
using Newtonsoft.Json;
using UnityEngine;
using DeviceType = LetsPiu.Define.DeviceType;
using Object = System.Object;

/// <summary>
/// http接口通信
/// </summary>
public class HttpConnector : SingletonComponent<HttpConnector>
{
    private string baseUrl = null;
    private string baseStaticUrl = null;

    Dictionary<string, string> m_TempDataMap = new Dictionary<string, string>();

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        baseUrl = PlayerPrefsUtils.GetObject(Constants.KEY_PREFS_SERVER_ADDRESS, Constants.HTTP_SERVER_URL);
    }

    /// <summary>
    /// 获取服务器时间，如果失败，返回当前设备时间
    /// </summary>
    /// <returns></returns>
    public async UniTask<DateTime> GetServerTime()
    {
        var resp = await SendHttpPost<string>(baseUrl + "/app/sysTime", null);
        return resp.success ? DateTime.Parse(resp.data) : DateTime.Now;
    }
    
    /// <summary>
    /// 获取用户信息(无token时用于验证token是否有效)
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Customer>> TestGetCustomerInfoAsync(string token)
    {
        var headers = new Dictionary<string, string>
        {
            ["TOKEN"] = token
        };
        return await SendRawHttpPost<Customer>(baseUrl + "/customer/getCustomerInfo", headers);
    }
    
    /// <summary>
    /// 获取用户信息(无token时用于验证token是否有效)
    /// </summary>
    /// <param name="token"></param>
    /// <param name="callback"></param>
    public void TestGetCustomerInfo(string token, Action<HttpJsonResponse<Customer>> callback)
    {
        StartCoroutine(TestGetCustomerInfoAsync(token).ToCoroutine(resp => callback?.Invoke(resp)));
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Customer>> GetCustomerInfoAsync()
    {
        return await SendHttpPost<Customer>(baseUrl + "/customer/getCustomerInfo");
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="callback"></param>
    public void GetCustomerInfo(Action<HttpJsonResponse<Customer>> callback)
    {
        StartCoroutine(GetCustomerInfoAsync().ToCoroutine(resp => callback?.Invoke(resp)));
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    public async UniTask<HttpJsonResponse<List<Profession>>> ProfessionGetListAsync()
    {
        return await SendHttpPost<List<Profession>>(baseUrl + "/profession/getList");
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    public void ProfessionGetList(string playId, Action<HttpJsonResponse<List<Profession>>> callback)
    {
        StartCoroutine(ProfessionGetListAsync().ToCoroutine(callback));
    }

    /// <summary>
    /// 获取技能列表
    /// </summary>
    public async UniTask<HttpJsonResponse<List<Skill>>> SkillGetListAsync()
    {
        return await SendHttpPost<List<Skill>>(baseUrl + "/skill/getList");
    }

    /// <summary>
    /// 获取技能列表
    /// </summary>
    public void SkillGetList(string playId, Action<HttpJsonResponse<List<Skill>>> callback)
    {
        StartCoroutine(SkillGetListAsync().ToCoroutine(callback));
    }
    

    #region 登录相关接口

    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="countryCode"></param>
    /// <param name="phone"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> SendPhoneCodeAsync(string countryCode, string phone)
    {
        var formData = new Dictionary<string, string>
        {
            ["countryCode"] = countryCode,
            ["phone"] = phone
        };
        var resp = await SendHttpPost<Empty>(baseUrl + "/login/sendCheckMessage", formData);
        return resp;
    }

    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="countryCode"></param>
    /// <param name="phone"></param>
    /// <param name="callback"></param>
    public void SendPhoneCode(string countryCode, string phone, Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(SendPhoneCodeAsync(countryCode, phone).ToCoroutine(resp => callback?.Invoke(resp)));
    }

    /// <summary>
    /// 手机验证码登录
    /// </summary>
    /// <param name="countryCode"></param>
    /// <param name="phone"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<LoginResponse>> LoginWithPhoneCodeAsync(string countryCode, string phone, string code)
    {
        var formData = new Dictionary<string, string>
        {
            ["countryCode"] = countryCode,
            ["phone"] = phone,
            ["code"] = code,
        };
        return await SendHttpPost<LoginResponse>(baseUrl + "/login/checkPhoneCode", formData);
    }

    /// <summary>
    /// 手机验证码登录
    /// </summary>
    /// <param name="countryCode"></param>
    /// <param name="phone"></param>
    /// <param name="code"></param>
    /// <param name="callback"></param>
    public void LoginWithPhoneCode(string countryCode, string phone, string code, Action<HttpJsonResponse<LoginResponse>> callback)
    {
        StartCoroutine(LoginWithPhoneCodeAsync(countryCode, phone, code).ToCoroutine(resp => callback?.Invoke(resp)));
    }

    /// <summary>
    /// 获取国家地区编码列表
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<List<CountryCode>>> GetCountryCodeListAsync()
    {
        return await SendHttpPost<List<CountryCode>>(baseUrl + "/countryCode/noLogin/getList");
    }

    /// <summary>
    /// 获取国家地区编码列表
    /// </summary>
    /// <returns></returns>
    public void GetCountryCodeList(Action<HttpJsonResponse<List<CountryCode>>> callback)
    {
        StartCoroutine(GetCountryCodeListAsync().ToCoroutine(callback));
    }

    /// <summary>
    /// 微信登录接口
    /// </summary>
    /// <param name="code">微信sdk获取到的登陆码</param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<WxLoginResponse>> LoginWithWeiXinAsync(string code)
    {
        var formData = new Dictionary<string, string>
        {
            ["code"] = code,
        };
        return await SendHttpPost<WxLoginResponse>(baseUrl + "/login/wxLogin", formData);
    }

    /// <summary>
    /// 微信登录接口
    /// </summary>
    /// <param name="code"></param>
    /// <param name="callback">微信sdk获取到的登陆码</param>
    public void LoginWithWeiXin(string code, Action<HttpJsonResponse<WxLoginResponse>> callback)
    {
        StartCoroutine(LoginWithWeiXinAsync(code).ToCoroutine(callback));
    }

    #endregion
    

    #region 语音相关接口(Voice开头)

    /// <summary>
    /// 拉取房间历史语音列表
    /// </summary>
    public async UniTask<HttpJsonResponse<List<VoiceReadMessage>>> VoiceRoomHistoryListAsync(string roomId)
    { 
        var formData = new Dictionary<string, string>
        {
            ["roomId"] = roomId
        };
        return await SendHttpPost<List<VoiceReadMessage>>(baseUrl + "/voice/getListByRoomId", formData);
    }

    /// <summary>
    /// 拉取房间历史语音列表
    /// </summary>
    public void VoiceRoomHistoryList(string roomId, Action<HttpJsonResponse<List<VoiceReadMessage>>>callback)
    {
        StartCoroutine(VoiceRoomHistoryListAsync(roomId).ToCoroutine(callback));
    }


    #endregion
    

    #region 用户相关接口(Customer开头)

    /// <summary>
    /// 设置用户信息
    /// </summary>
    /// <param name="params">参数</param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Customer>> CustomerSetUserInfoAsync(SetUserInfoParams @params)
    {
        return await SendHttpPost<Customer>(baseUrl + "/customer/setUserInfo", ObjectToDict(@params));
    }

    /// <summary>
    /// 通过uid查询用户
    /// </summary>
    public async UniTask<HttpJsonResponse<Customer>> CustomerGetCustomerByUidAsync(string uid)
    {
        m_TempDataMap.Clear();
        m_TempDataMap.Add("uid", uid);
        return await SendHttpPost<Customer>(baseUrl + "/customer/queryByUID", m_TempDataMap);
    }

    /// <summary>
    /// 通过uid查询用户
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="callback"></param>
    public void CustomerGetCustomerByUid(string uid, Action<HttpJsonResponse<Customer>> callback)
    {
        StartCoroutine(CustomerGetCustomerByUidAsync(uid).ToCoroutine(callback));
    }

    /// <summary>
    /// 设置用户信息
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void CustomerSetUserInfo(SetUserInfoParams @params, Action<HttpJsonResponse<Customer>> callback)
    {
        StartCoroutine(CustomerSetUserInfoAsync(@params).ToCoroutine(callback));
    }

    /// <summary>
    /// 更换角色
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> CustomerSelectProfessionAsync(SelectProfessionParams @params)
    {
        return await SendHttpPost<Empty>(baseUrl + "/customer/selectProfession", ObjectToDict(@params));
    }

    /// <summary>
    /// 更换角色
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void CustomerSelectProfession(SelectProfessionParams @params, Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(CustomerSelectProfessionAsync(@params).ToCoroutine(callback));
    }

    /// <summary>
    /// 设置个性签名
    /// </summary>
    /// <param name="params">参数</param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Customer>> CustomerSetSignAsync(SetUserSignParams @params)
    {
        return await SendHttpPost<Customer>(baseUrl + "/customer/setSign", ObjectToDict(@params));
    }

    /// <summary>
    /// 设置个性签名
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void CustomerSetSign(SetUserSignParams @params, Action<HttpJsonResponse<Customer>> callback)
    {
        StartCoroutine(CustomerSetSignAsync(@params).ToCoroutine(callback));
    }

    /// <summary>
    /// 删除用户数据
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> CustomerDeleteUserDataAsync()
    {
        return await SendHttpPost<Empty>(baseUrl + "/customer/deleteCustomer");
    }

    /// <summary>
    /// 删除用户数据
    /// </summary>
    /// <param name="callback"></param>
    public void CustomerDeleteUserData(Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(CustomerDeleteUserDataAsync().ToCoroutine(callback));
    }

    #endregion


    #region 玩家相关接口(Player开头)

    /// <summary>
    /// 同意准备(强制准备消息后)
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> PlayerForceReadyAsync()
    {
        return await SendHttpPost<Empty>(baseUrl + "/player/forceReady");
    }

    /// <summary>
    /// 同意准备(强制准备消息后)
    /// </summary>
    /// <param name="callback"></param>
    public void PlayerForceReady(Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(PlayerForceReadyAsync().ToCoroutine(callback));
    }

    /// <summary>
    /// 推送强制准备消息后不同意准备
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> PlayerUnReadyAsync()
    {
        return await SendHttpPost<Empty>(baseUrl + "/player/unready");
    }

    /// <summary>
    /// 推送强制准备消息后不同意准备
    /// </summary>
    /// <param name="callback"></param>
    public void PlayerUnReady(Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(PlayerUnReadyAsync().ToCoroutine(callback));
    }

    /// <summary>
    /// 回到等待席
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> PlayerReturnWaitAsync()
    {
        return await SendHttpPost<Empty>(baseUrl + "/player/returnWait");
    }

    /// <summary>
    /// 回到等待席
    /// </summary>
    /// <param name="callback"></param>
    public void PlayerReturnWait(Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(PlayerReturnWaitAsync().ToCoroutine(callback));
    }

    /// <summary>
    /// 取消准备/准备
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> PlayerUpdateReadyAsync(PlayerUpdateReadyParams @params)
    {
        return await SendHttpPost<Empty>(baseUrl + "/player/updateReady", ObjectToDict(@params));
    }

    /// <summary>
    /// 取消准备/准备
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void PlayerUpdateReady(PlayerUpdateReadyParams @params, Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(PlayerUpdateReadyAsync(@params).ToCoroutine(callback));
    }

    /// <summary>
    /// 更换角色
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> PlayerSelectProfessionAsync(SelectProfessionParams @params)
    {
        return await SendHttpPost<Empty>(baseUrl + "/player/selectProfession", ObjectToDict(@params));
    }

    /// <summary>
    /// 更换角色
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void PlayerSelectProfession(SelectProfessionParams @params, Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(PlayerSelectProfessionAsync(@params).ToCoroutine(callback));
    }

    /// <summary>
    /// 预选角色
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> PlayerChooseProfessionAsync(SelectProfessionParams @params)
    {
        return await SendHttpPost<Empty>(baseUrl + "/player/chooseProfession", ObjectToDict(@params));
    }

    /// <summary>
    /// 预选角色
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void PlayerChooseProfession(SelectProfessionParams @params, Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(PlayerChooseProfessionAsync(@params).ToCoroutine(callback));
    }

    /// <summary>
    /// 扫码复活
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> PlayerReviveAsync(ReviveParams @params)
    {
        return await SendHttpPost<Empty>(baseUrl + "/player/revive", ObjectToDict(@params));
    }

    /// <summary>
    /// 扫码复活
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void PlayerRevive(ReviveParams @params, Action<HttpJsonResponse<Empty>> callback)
    {
        StartCoroutine(PlayerReviveAsync(@params).ToCoroutine(callback));
    }

    /// <summary>
    /// 获取当前用户的玩家信息
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Player>> PlayerGetPlayerInfoAsync()
    {
        return await SendHttpPost<Player>(baseUrl + "/player/getPlayerInfo");
    }

    /// <summary>
    /// 获取当前用户的玩家信息
    /// </summary>
    /// <param name="callback"></param>
    public void PlayerGetPlayerInfo(Action<HttpJsonResponse<Player>> callback)
    {
        StartCoroutine(PlayerGetPlayerInfoAsync().ToCoroutine(callback));
    }

    /// <summary>
    /// 获取玩家战绩
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<PageData<Player>>> PlayerGetPlayerRecordAsync()
    {
        return await SendHttpPost<PageData<Player>>(baseUrl + "/player/getPlayerRecord");
    }

    /// <summary>
    /// 获取玩家战绩
    /// </summary>
    /// <param name="callback"></param>
    public void PlayerGetPlayerRecord(Action<HttpJsonResponse<PageData<Player>>> callback)
    {
        StartCoroutine(PlayerGetPlayerRecordAsync().ToCoroutine(callback));
    }


    #endregion
    
    #region 场次相关接口(Play开头)

    /// <summary>
    /// 获取场次信息
    /// </summary>
    public async UniTask<HttpJsonResponse<QueryPlayResponse>> PlayGetPlayInfoAsync(string playId)
    {
        m_TempDataMap.Clear();
        m_TempDataMap["playId"] = playId;
      
        return await SendHttpPost<QueryPlayResponse>(baseUrl + "/play/getPlayInfo", m_TempDataMap);
    }

    /// <summary>
    /// 获取场次信息
    /// </summary>
    public void PlayGetPlayInfo(string playId, Action<HttpJsonResponse<QueryPlayResponse>> callback)
    {
        StartCoroutine(PlayGetPlayInfoAsync(playId).ToCoroutine(callback));
    }

    /// <summary>
    /// 获取击杀记录
    /// </summary>
    public async UniTask<HttpJsonResponse<KillRecordResponse>> PlayGetKillRecordsAsync(string playId)
    {
        m_TempDataMap.Clear();
        m_TempDataMap["playId"] = playId;
        
        return await SendHttpPost<KillRecordResponse>(baseUrl + "/play/getKillRecords", m_TempDataMap);
    }
 
    /// <summary>
    /// 获取击杀记录
    /// </summary>
    public void PlayGetKillRecords(string playId, Action<HttpJsonResponse<KillRecordResponse>> callback)
    {
        StartCoroutine(PlayGetKillRecordsAsync(playId).ToCoroutine(callback));
    }

    /// <summary>
    /// 结束场次
    /// </summary>
    public async UniTask<HttpJsonResponse<QueryPlayResponse>> PlayEndPlayAsync(string playId)
    {
        m_TempDataMap.Clear();
        m_TempDataMap["playId"] = playId;

        return await SendHttpPost<QueryPlayResponse>(baseUrl + "/play/endPlay", m_TempDataMap);
    }

    /// <summary>
    /// 结束场次
    /// </summary>
    public void PlayEndPlay(string playId, Action<HttpJsonResponse<QueryPlayResponse>> callback)
    {
        StartCoroutine(PlayEndPlayAsync(playId).ToCoroutine(callback));
    }
    
    /// <summary>
    /// 处理结算
    /// </summary>
    /// <param name="playId"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<PlaySummaryDetail>> PlayGetPlaySummaryAsync(string playId)
    {
        m_TempDataMap.Clear();
        m_TempDataMap["playId"] = playId;
       
        return await SendHttpPost<PlaySummaryDetail>(baseUrl + "/play/getPlaySummary", m_TempDataMap);
    }
    
    /// <summary>
    /// 处理结算
    /// </summary>
    /// <param name="playId"></param>
    /// <param name="callback"></param>
    public void PlayGetPlaySummary(string playId, Action<HttpJsonResponse<PlaySummaryDetail>> callback)
    {
        StartCoroutine(PlayGetPlaySummaryAsync(playId).ToCoroutine(callback));
    }

    #endregion


    #region 系统相关接口(App开头)

    /// <summary>
    /// 获取腾讯云cos上传密钥
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<CosAuthResponse>> AppGetCosCredentialsAsync()
    {
        return await SendHttpPost<CosAuthResponse>(baseUrl + "/app/cosAuth", null);
    }

    /// <summary>
    /// 获取系统时间
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<DateTime>> AppGetSystemTimeAsync()
    {
        return await SendHttpPost<DateTime>(baseUrl + "/app/sysTime");
    }

    /// <summary>
    /// 获取默认头像
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<List<HeadImages>>> AppGetDefaultHeadImagesAsync()
    {
        return await SendHttpPost<List<HeadImages>>(baseUrl + "/app/getDefaultHeadImages");
    }

    /// <summary>
    /// 获取app常量
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<CommonConstant>> AppGetCommonConstantAsync(GetCommonConstantParams @params)
    {
        return await SendHttpPost<CommonConstant>(baseUrl + "/app/getCommonConstant", ObjectToDict(@params));
    }

    /// <summary>
    /// 线上线下模式切换
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> AppSetOnlineAndOfflineAsync(ChangeOnlineAndOfflineParams @params)
    {
        return await SendHttpPost<Empty>(baseUrl + "/app/onlineAndOffline", ObjectToDict(@params));
    }
    
    /// <summary>
    /// 获取游戏模式列表
    /// </summary>
    /// <param name="params">参数</param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<List<GameModeInfo>>> GetGameModeListAsync()
    {
        return await SendHttpPost<List<GameModeInfo>>(baseUrl + "/gameMode/getGameModeList");
    }

    /// <summary>
    /// 获取游戏模式列表
    /// </summary>
    /// <param name="params"></param>
    /// <param name="callback"></param>
    public void GetGameModeList(Action<HttpJsonResponse<List<GameModeInfo>>> callback)
    {
        StartCoroutine(GetGameModeListAsync().ToCoroutine(callback));
    }

    #endregion


    #region 装备相关接口
    
    /// <summary>
    /// 注册5S设备(如果存在直接返回)
    /// 对应 Android 端 Application.queryAccount()，使用 GET 请求
    /// 接口地址：Constants.HTTP_REGISTER_5S_DEVICE_URL
    /// </summary>
    /// <param name="code">设备编号(目前为时间戳,格式:yyyyMMddHHmmss)</param>
    /// <param name="type">设备类型 4代表比分牌</param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<DeviceInfo5S>> RegisterDeviceAsync(string code, DeviceType5S type)
    {
        var dict = new Dictionary<string, string>
        {
            { "code", code },
            { "type", ((int)type).ToString() }
        };
        return await SendHttpGetRawJson<DeviceInfo5S>(Constants.HTTP_REGISTER_5S_DEVICE_URL, dict);
    }

    /// <summary>
    /// 绑定设备
    /// </summary>
    /// <param name="qrCode"></param>
    /// <param name="macAddress"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<DeviceInfo>> BindDeviceAsync(string qrCode, string macAddress)
    {
        var dict = new Dictionary<string, string>
        {
            { "qrCode", qrCode },
            { "macAddress", macAddress }
        };
        return await SendHttpPost<DeviceInfo>(baseUrl + "/customerDevice/bindingDevice", dict);
    }
    
    /// <summary>
    /// 解绑设备
    /// </summary>
    /// <param name="deviceType"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<DeviceInfo>> UnBindDeviceAsync(DeviceType deviceType)
    {
        var deviceTypeValue = (int)deviceType;
        var dict = new Dictionary<string, string> { { "deviceType", deviceTypeValue.ToString() } };
        return await SendHttpPost<DeviceInfo>(baseUrl + "/customerDevice/unBindingDevice", dict);
    }
    
    /// <summary>
    /// 获取设备最新的固件版本
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<List<DeviceVersionInfo>>> GetDeviceVersionInfoAsync()
    {
        return await SendHttpPost<List<DeviceVersionInfo>>(baseUrl + "/device/getInstPackage");
    }
    
    /// <summary>
    /// 验证升级OTA的pinCode
    /// </summary>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<Empty>> DeviceCheckPinCodeAsync(string pinCode)
    {
        var dict = new Dictionary<string, string> { { "pinCode", pinCode } };
        return await SendHttpPost<Empty>(baseUrl + "/device/pinCodeCheck", dict);
    }

    /// <summary>
    /// 更新装备
    /// </summary>
    /// <param name="qrCode"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<DeviceInfo>> UpdatePlayerDeviceAsync(string qrCode, string headCode)
    {
        var dict = new Dictionary<string, string>
        {
            { "qrCode", qrCode },
            { "headCode", headCode },
        };
        return await SendHttpPost<DeviceInfo>(baseUrl + "/device/updatePlayerDevice", dict);
    }

    /// <summary>
    /// 获取装备详情
    /// </summary>
    /// <param name="qrCode"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<DeviceInfo>> GetDeviceDetailAsync(string qrCode)
    {
        var dict = new Dictionary<string, string> { { "qrCode", qrCode } };
        return await SendHttpPost<DeviceInfo>(baseUrl + "/device/getDeviceDetail", dict);
    }
    
    /// <summary>
    /// 获取装备详情(根据设备编号)
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async UniTask<HttpJsonResponse<DeviceInfo>> GetDeviceDetailByCodeAsync(string code)
    {
        var dict = new Dictionary<string, string> { { "code", code } };
        return await SendHttpPost<DeviceInfo>(baseUrl + "/device/getDeviceDetailByCode", dict);
    }

    #endregion


    private Dictionary<string, string> GetHttpHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            ["TOKEN"] = AppStateManager.Instance.Token
            //["TOKEN"] = "ufEvuFec3SNX6QkMQZAO8A=="
        };
        return headers;
    }

    private async UniTask<HttpJsonResponse<T>> SendHttpJsonPost<T>(string url, Object jsonObject)
    {
        var resp = await HttpClientUtils.PostJson(url, jsonObject, GetHttpHeaders());

        Debug.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: success=" + resp.success, GameDataManager.ColLogResponseOk));
        Debug.Log("HttpConnector fuckToken:" + AppStateManager.Instance.Token);

        if (!resp.success)
        {
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = resp.error + ""
            };
            return json;
        }

        try
        {
            Debug.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: resp.result=" + resp.result, GameDataManager.ColLogResponseOk));
            var jsonResp = JsonHelper.DeserializeObject<HttpJsonResponse<T>>(resp.result);

            //判断是否登录超时，发送登录超时消息
            if (jsonResp.return_code == Constants.HttpReturnCodeLoginTimeout)
            {
                EventManager.Instance.Broadcast(EventMsg.LoginTokenTimeout);
                EventManager.Instance.Broadcast(EventMsg.RequestLoginOut);
            }

            CheckAppVersion(jsonResp.version);
            
            return jsonResp;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: 出现异常:" + e.Message, GameDataManager.ColLogResponseErr));
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = e + ""
            };
            return json;
        }
    }

    private void CheckAppVersion(string lastAppVersion)
    {
        //判断是否需要更新app
        Debug.Log("HttpConnector NeedUpdateApp 当前版本号:"+Application.version+" 服务器版本号:"+lastAppVersion);
        if (!string.IsNullOrEmpty(lastAppVersion))
        {
            var c = AppUtils.CompareVersions(Application.version, lastAppVersion);
            //如果app当前版本小于服务器版本
            if (c < 0)
            {
                UniTask.Post(async () =>
                {
                    await UniTask.WaitForSeconds(0.3f);
                    Debug.Log("HttpConnector NeedUpdateApp 需要更新app 最新版本号:" + lastAppVersion+" 当前版本号:"+Application.version);
                    EventManager.Instance.Broadcast(EventMsg.NeedUpdateApp, lastAppVersion); 
                });
            }
        }
    }
    
    /// <summary>
    /// 发送原始的Http请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="formData"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private async UniTask<HttpJsonResponse<T>> SendRawHttpPost<T>(string url, Dictionary<string, string> headers, Dictionary<string, string> formData = null)
    {
        var resp = await HttpClientUtils.Post(url, formData, headers);
        Debug.Log(GameDataManager.Colorize("[Response] HttpConnector SendRawHttpPost: success=" + resp.success, GameDataManager.ColLogResponseOk));
        if (!resp.success)
        {
            Debug.LogError(GameDataManager.Colorize("[Response] SendRawHttpPost 接口请求失败 url=" + url + " Response:" + resp.result, GameDataManager.ColLogResponseErr));
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = resp.error + ""
            };
            return json;
        }

        try
        {
            Debug.Log(GameDataManager.Colorize("[Response] HttpConnector SendRawHttpPost: resp.result=" + resp.result, GameDataManager.ColLogResponseOk));
            //指定解析的日期格式
            var jsonResp = JsonHelper.DeserializeObject<HttpJsonResponse<T>>(resp.result);
            if (!jsonResp.success)
            {
                Debug.LogError(GameDataManager.Colorize("[Response] HttpConnector SendRawHttpPost 接口请求失败 url=" + url + " Response:" + resp.result, GameDataManager.ColLogResponseErr));
            }
            return jsonResp;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError(GameDataManager.Colorize("[Response] HttpConnector SendRawHttpPost: 出现异常:" + e.Message, GameDataManager.ColLogResponseErr));
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = e + ""
            };
            return json;
        }
    }

    private async UniTask<HttpJsonResponse<T>> SendHttpPost<T>(string url, Dictionary<string, string> formData = null)
    {
        var resp = await HttpClientUtils.Post(url, formData, GetHttpHeaders());

        Debug.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: success=" + resp.success, GameDataManager.ColLogResponseOk));
        Debug.Log("HttpConnector fuckToken:" + AppStateManager.Instance.Token);

        if (!resp.success)
        {
            Debug.LogError(GameDataManager.Colorize("[Response] 接口请求失败 url=" + url + " Response:" + resp.result, GameDataManager.ColLogResponseErr));
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = resp.error + ""
            };
            return json;
        }

        try
        {
            LongLogger.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: resp.result=" + resp.result, GameDataManager.ColLogResponseOk));
            // Debug.Log("<color=green>Response</color> HttpConnector SendHttpPost: resp.result=" + resp.result);
            //指定解析的日期格式
            var jsonResp = JsonHelper.DeserializeObject<HttpJsonResponse<T>>(resp.result);

            //判断是否登录超时，发送登录超时消息
            if (jsonResp.return_code == Constants.HttpReturnCodeLoginTimeout)
            {
                EventManager.Instance.Broadcast(EventMsg.LoginTokenTimeout);
                EventManager.Instance.Broadcast(EventMsg.RequestLoginOut);
            }
            
            CheckAppVersion(jsonResp.version);
            
            if (!jsonResp.success)
            {
                Debug.LogError(GameDataManager.Colorize("[Response] 接口请求失败 url=" + url + " Response:" + resp.result, GameDataManager.ColLogResponseErr));
            }
            return jsonResp;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: 出现异常:" + e.Message, GameDataManager.ColLogResponseErr));
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = e + ""
            };
            return json;
        }
    }
    
    private async UniTask<HttpJsonResponse<T>> SendHttpPostRawJson<T>(string url, Dictionary<string, string> formData = null)    {
        var resp = await HttpClientUtils.Post(url, formData, GetHttpHeaders());

        Debug.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: success=" + resp.success, GameDataManager.ColLogResponseOk));
        Debug.Log("HttpConnector fuckToken:" + AppStateManager.Instance.Token);

        if (!resp.success)
        {
            Debug.LogError(GameDataManager.Colorize("[Response] 接口请求失败 url=" + url + " Response:" + resp.result, GameDataManager.ColLogResponseErr));
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = resp.error + ""
            };
            return json;
        }

        try
        {
            LongLogger.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: resp.result=" + resp.result, GameDataManager.ColLogResponseOk));
            //指定解析的日期格式
            var jsonResp = JsonHelper.DeserializeObject<T>(resp.result);
            var json = new HttpJsonResponse<T>
            {
                success = true,
                msg = resp.error + "",
                data = jsonResp
            };
            return json;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError(GameDataManager.Colorize("[Response] HttpConnector SendHttpPost: 出现异常:" + e.Message, GameDataManager.ColLogResponseErr));
            var json = new HttpJsonResponse<T>
            {
                success = false,
                msg = e + ""
            };
            return json;
        }
    }

    private Dictionary<string, string> ObjectToDict(object @params)
    {
        if (@params == null) return null;
        //转换成JSON
        var json = JsonConvert.SerializeObject(@params);
        // 将 JSON 字符串反序列化为 Dictionary<string, object>
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }

    /// <summary>
    /// 发送 GET 请求并将响应体直接反序列化为 T（适用于返回平铺 JSON 的 5S 设备注册接口）
    /// </summary>
    private async UniTask<HttpJsonResponse<T>> SendHttpGetRawJson<T>(string url, Dictionary<string, string> queryParams = null)
    {
        var resp = await HttpClientUtils.Get(url, queryParams, null);

        Debug.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpGetRawJson: success=" + resp.success, GameDataManager.ColLogResponseOk));

        if (!resp.success)
        {
            Debug.LogError(GameDataManager.Colorize("[Response] GET 接口请求失败 url=" + url + " error=" + resp.error, GameDataManager.ColLogResponseErr));
            return new HttpJsonResponse<T> { success = false, msg = resp.error + "" };
        }

        try
        {
            LongLogger.Log(GameDataManager.Colorize("[Response] HttpConnector SendHttpGetRawJson: resp.result=" + resp.result, GameDataManager.ColLogResponseOk));
            var jsonResp = JsonHelper.DeserializeObject<T>(resp.result);
            return new HttpJsonResponse<T> { success = true, data = jsonResp };
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError(GameDataManager.Colorize("[Response] HttpConnector SendHttpGetRawJson: 解析异常:" + e.Message, GameDataManager.ColLogResponseErr));
            return new HttpJsonResponse<T> { success = false, msg = e.Message };
        }
    }
}