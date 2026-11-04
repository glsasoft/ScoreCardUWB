using UnityEditor;
[InitializeOnLoad]
public class GlobalConfig
{
    static GlobalConfig()
    {
        PlayerSettings.Android.keystorePass = "Wogemu2022";
        PlayerSettings.Android.keyaliasName = "wogemu";
        PlayerSettings.Android.keyaliasPass = "Wogemu2022";
    }
}
