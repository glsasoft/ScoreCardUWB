using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RememberKeystore
{
    static RememberKeystore()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string keystore = Path.Combine(projectRoot, "Docs", "Cx3568_android12", "Cx3568.jks");
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keystore;
        PlayerSettings.Android.keystorePass = "Wogemu2022";
        PlayerSettings.Android.keyaliasName = "wogemu";
        PlayerSettings.Android.keyaliasPass = "Wogemu2022";
    }
}
