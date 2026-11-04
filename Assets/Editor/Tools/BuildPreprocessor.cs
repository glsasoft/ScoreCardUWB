//using UnityEditor;
//using UnityEditor.Build;
//using UnityEditor.Build.Reporting;

///// <summary>
///// 简单自动宏切换工具
///// - 根据 Development Build 自动添加/去掉 DEBUG 宏
///// - 根据你在 BuildPreprocessor.CurrentEnv 里设置的环境自动添加环境宏
///// </summary>
//[InitializeOnLoad] // 确保编辑器启动也能生效
//public class BuildPreprocessor : IPreprocessBuildWithReport
//{
//    public int callbackOrder => 0;

//    public enum BuildEnv
//    {
//        LOCAL,
//        TEST_SERVER,
//        STAGING,
//        PRODUCTION,
//        APP_REVIEW
//    }

//    // 你可以在代码里直接改这个字段控制当前环境
//    public static BuildEnv CurrentEnv = BuildEnv.TEST_SERVER;

//    public void OnPreprocessBuild(BuildReport report)
//    {
//        ApplyDefines();
//    }

//    /// <summary>
//    /// 更新宏定义
//    /// </summary>
//    public static void ApplyDefines()
//    {
//        BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
//        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

//        // 清理旧环境宏
//        foreach (var env in System.Enum.GetNames(typeof(BuildEnv)))
//        {
//            defines = RemoveDefine(defines, env);
//        }

//        // 添加当前环境宏
//        defines = AddDefine(defines, CurrentEnv.ToString());

//        // Development Build 自动加/去掉 DEBUG 宏
//        if (EditorUserBuildSettings.development)
//            defines = AddDefine(defines, "DEBUG");
//        else
//            defines = RemoveDefine(defines, "DEBUG");

//        // 设置回去
//        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
//    }

//    private static string AddDefine(string defines, string symbol)
//    {
//        if (!defines.Contains(symbol))
//        {
//            if (!string.IsNullOrEmpty(defines))
//                defines += ";";
//            defines += symbol;
//        }
//        return defines;
//    }

//    private static string RemoveDefine(string defines, string symbol)
//    {
//        return defines.Replace(symbol, "").Replace(";;", ";").Trim(';');
//    }
//}
