using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

public class PostBuildiOSProcessor
{

    //private static string StrSystemFrameworkPath = "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk/System/Library/";
    //private static string StrSystemLibPath = "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk/usr/lib";
    private static string StrSystemFrameworkPath = "System/Library/Frameworks/";
    private static string StrSystemLibPath = "usr/lib";

    [PostProcessBuild]
    public static void OnIOSPostBuildProcess(BuildTarget buildTarget, string path)
    {
#if UNITY_IOS
        string projectPath = PBXProject.GetPBXProjectPath(path);
        PBXProject pbxProject = new PBXProject();
 
        pbxProject.ReadFromString(File.ReadAllText(projectPath));
 
        string target = pbxProject.GetUnityFrameworkTargetGuid();
 
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/OpenGLES.framework", "Frameworks/OpenGLES.framework", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/Accelerate.framework", "Frameworks/Accelerate.framework", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/CoreBluetooth.framework", "Frameworks/CoreBluetooth.framework", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/ReplayKit.framework", "Frameworks/ReplayKit.framework", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/CoreFoundation.framework", "Frameworks/CoreFoundation.framework", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/CoreTelephony.framework", "Frameworks/CoreTelephony.framework", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/MobileCoreServices.framework", "Frameworks/MobileCoreServices.framework", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/AVFAudio.framework", "Frameworks/AVFAudio.framework", PBXSourceTree.Sdk));
        
        //pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/AVFoundation.framework", "Frameworks/AVFoundation.framework", PBXSourceTree.Sdk));
        //pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemFrameworkPath + "/CoreLocation.framework", "Frameworks/AVFoundation.framework", PBXSourceTree.Sdk));

        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemLibPath + "/libc++.tbd", "Frameworks/libc++.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemLibPath + "/libc++.1.tbd", "Frameworks/libc++.1.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemLibPath + "/libsqlite3.0.tbd", "Frameworks/libsqlite3.0.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemLibPath + "/libsqlite3.tbd", "Frameworks/libsqlite3.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemLibPath + "/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));
        pbxProject.AddFileToBuild(target, pbxProject.AddFile(StrSystemLibPath + "/libresolv.tbd", "Frameworks/libresolv.tbd", PBXSourceTree.Sdk));

        string appTarget = pbxProject.GetUnityMainTargetGuid();
 
            File.WriteAllText(projectPath, pbxProject.WriteToString());
#endif
    }
}