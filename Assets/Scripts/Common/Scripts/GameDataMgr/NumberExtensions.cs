public static class NumberExtensions
{
    public static string FormatNumber(this int number, bool useCommas = false)
    {
        return GameDataManager.Instance.FormatNumber(number, useCommas);
    }
    public static string FormatNumber(this float number, bool useCommas = false)
    {
        return GameDataManager.Instance.FormatNumber(number, useCommas);
    }
    public static string FormatNumber(this double number, bool useCommas = false)
    {
        return GameDataManager.Instance.FormatNumber(number, useCommas);
    }
    public static string FormatNumber(this string number, bool useCommas = false)
    {
        return GameDataManager.Instance.FormatNumber(number, useCommas);
    }
    public static string FormatNumber(this long number, bool useCommas = false)
    {
        return GameDataManager.Instance.FormatNumber(number, useCommas);
    }
    public static string FormatNumber(this decimal number, bool useCommas = false)
    {
        return GameDataManager.Instance.FormatNumber(number, useCommas);
    }
}
/* 使用样例
public class Example : MonoBehaviour
{
    void Start()
    {
        int     intValue        = 123456789;
        float   floatValue      = 123456789.123f;
        double  doubleValue     = 123456789.123456;
        string  stringValue     = "123456789.123";
        long    longValue       = 123456789L;
        decimal decimalValue    = 123456789.123456m;

        Debug.Log(intValue.     FormatNumber());        // 输出：123456789
        Debug.Log(floatValue.   FormatNumber(true));    // 输出：123,456,789.12
        Debug.Log(doubleValue.  FormatNumber());        // 输出：123456789.12
        Debug.Log(stringValue.  FormatNumber());        // 输出：123456789.12
        Debug.Log(longValue.    FormatNumber(true));    // 输出：123,456,789
        Debug.Log(decimalValue. FormatNumber());        // 输出：123456789.12
    }
}
*/