using UnityEditor;
[InitializeOnLoad]
public class RememberKeystore
{
    static RememberKeystore()
    {
        PlayerSettings.Android.keystoreName = "Cx3568.jks"; // 相对路径
        PlayerSettings.Android.keystorePass = "Wogemu2022";
        PlayerSettings.Android.keyaliasName = "wogemu";
        PlayerSettings.Android.keyaliasPass = "Wogemu2022";
    }
}
