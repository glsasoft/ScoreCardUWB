using LetsPiu;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;
using Common.Utils;
public class GameDataManager : SingletonComponent<GameDataManager>
{
    #region 日志颜色统一管理，供全局使用（Unity Console 富文本）
    // 转发到 Common.Utils.LogColor，避免在 GameDataManager 内直接定义导致程序集耦合问题
    public static string ColLogRequest => LogColor.Request;
    public static string ColLogResponseOk => LogColor.ResponseOk;
    public static string ColLogResponseErr => LogColor.ResponseErr;
    public static string ColLogInfo => LogColor.Info;
    public static string Colorize(string msg, string hex) => LogColor.Colorize(msg, hex);
    #endregion
    
    #region 格式化数字
    private static readonly (double Threshold, string Unit)[] _units =
    {
        (100_000_000, "y"), // 亿
        (10_000_000, "k"),  // 千万
        (1_000_000, "m"),   // 百万
        (100_000, "h"),     // 十万
        (10_000, "w")       // 万
    };
    private const double MinFormatThreshold = 0.000_000_1; // 1e-7
    private bool TryFormatWithUnits(double absNumber, bool isNegative, out string result)
    {
        if (absNumber >= _units[^1].Threshold)
        {
            foreach (var unit in _units)
            {
                if (absNumber >= unit.Threshold)
                {
                    double formattedValue = absNumber / unit.Threshold;
                    string format = formattedValue switch
                    {
                        >= 100 => "0",
                        >= 10 => "0.0",
                        _ => "0.00"
                    };
                    result = formattedValue.ToString(format) + unit.Unit;
                    if (isNegative) result = "-" + result;
                    return true;
                }
            }
        }
        result = null;
        return false;
    }
    private string FormatBasicNumber(double absNumber, bool isNegative, bool useCommas)
    {
        string result;
        if (useCommas)
        {
            string format = absNumber >= 1 ? "#,##0" : "0";
            result = absNumber.ToString(format + (absNumber % 1 == 0 ? "" : ".##"));
        }
        else
        {
            result = absNumber.ToString(absNumber < 0.1 ? "0.###" : "0.##");
        }
        return isNegative ? "-" + result : result;
    }
    public string FormatNumber(double number, bool useCommas = false)
    {
        if (double.IsNaN(number)) return "NaN";
        if (number == 0) return "0";

        bool isNegative = number < 0;
        double absNumber = isNegative ? -number : number;

        if (absNumber < MinFormatThreshold)
            return isNegative ? "-0" : "0";

        if (TryFormatWithUnits(absNumber, isNegative, out string unitResult))
            return unitResult;

        return FormatBasicNumber(absNumber, isNegative, useCommas);
    }
    /// <summary>
    /// 格式化数字
    /// </summary>
    /// <param name="number">要更改的具体数值</param>
    /// <param name="useCommas">是否使用千分符隔开(是否使用逗号分隔,如:12345->12,345)</param>
    /// <returns></returns>
    public string FormatNumber(int number, bool useCommas = false) =>
        FormatNumber((double)number, useCommas);
    public string FormatNumber(float number, bool useCommas = false) =>
        FormatNumber((double)number, useCommas);
    public string FormatNumber(long number, bool useCommas = false) =>
        FormatNumber((double)number, useCommas);
    public string FormatNumber(decimal number, bool useCommas = false) =>
        FormatNumber((double)number, useCommas);
    public string FormatNumber(string number, bool useCommas = false) =>
        double.TryParse(number, out var parsedNumber)
            ? FormatNumber(parsedNumber, useCommas)
            : HandleInvalidNumber(number);
    private string HandleInvalidNumber(string invalidNumber)
    {
        Debug.LogError($"Invalid string input for number: {invalidNumber}");
        return "Invalid";
    }
    #endregion


    public bool CanDrops_Sth = false;                       // 可以丢弃某些东西(大逃杀模式)
    public bool IsOpened_PersonalCenterRecordPage = false;  // 是否打开了个人对局界面记录页面
#if UIDEV
    public bool CanSkipBlueToothBind_ForBindGun = false;    // 是否可以跳过蓝牙绑枪验证
#endif

    #region 图集加载 Sprite
    /// <summary>
    /// 加载图集中的 Sprite
    /// </summary>
    /// <param name="atlasAddress">图集 Address</param>
    /// <param name="spriteName">Sprite 名称</param>
    /// <param name="onLoaded">回调</param>
    public void LoadSpriteAsync(string atlasAddress, string spriteName, Action<Sprite, AssetHandle> onLoaded)
    {
        var package = YooAssets.GetPackage("DefaultPackage");
        if (package == null)
        {
            Debug.LogError("[SpriteLoader] YooAsset 包未初始化或未找到：DefaultPackage");
            onLoaded?.Invoke(null, null);
            return;
        }

        var handle = package.LoadAssetAsync<SpriteAtlas>(atlasAddress);
        handle.Completed += (op) =>
        {
            if (op.Status == EOperationStatus.Succeed)
            {
                var atlas = op.AssetObject as SpriteAtlas;
                var sprite = atlas.GetSprite(spriteName);
                if (sprite != null)
                {
                    onLoaded?.Invoke(sprite, op);
                }
                else
                {
                    Debug.LogError($"[SpriteLoader] 图集中未找到 Sprite：{spriteName}, Address: {atlasAddress}");
                    op.Dispose();
                    onLoaded?.Invoke(null, null);
                }
            }
            else
            {
                Debug.LogError($"[SpriteLoader] 图集加载失败, Address: {atlasAddress}, Error: {op.LastError}");
                op.Dispose();
                onLoaded?.Invoke(null, null);
            }
        };
    }
    /*
    GameDataManager.Instance.LoadSpriteAsync("Assets/Art/UIAtlas/UIAtlas.spriteatlas", "icon_apple", sprite =>
    {
        if (sprite != null)
            myImage.sprite = sprite;
    });
    

    */
    #endregion

    #region 单图加载 Texture2D
    /// <summary>
    /// 加载单张大图（Texture2D / Default）
    /// </summary>
    /// <param name="textureAddress">图片 Address</param>
    /// <param name="onLoaded">回调</param>
    public void LoadBigTexture(string textureAddress, Action<Texture2D, AssetHandle> onLoaded)
    {
        var package = YooAssets.GetPackage("DefaultPackage");
        if (package == null)
        {
            Debug.LogError("[SpriteLoader] YooAsset 包未初始化或未找到：DefaultPackage");
            onLoaded?.Invoke(null, null);
            return;
        }

        var handle = package.LoadAssetAsync<Texture2D>(textureAddress);
        handle.Completed += (op) =>
        {
            if (op.Status == EOperationStatus.Succeed)
            {
                var texture = op.AssetObject as Texture2D;
                onLoaded?.Invoke(texture, op);
            }
            else
            {
                Debug.LogError($"[SpriteLoader] 大图加载失败, Address: {textureAddress}, Error: {op.LastError}");
                op.Dispose();
                onLoaded?.Invoke(null, null);
            }
        };
    }
    /*
       GameDataManager.Instance.LoadBigTexture("Assets/Art/UIAtlas/Big/big_icon.png", texture =>
       {
           if (texture != null)
           {
               // 可以赋值给 RawImage
               myRawImage.texture = texture;
           }
       });

    */
    #endregion

}