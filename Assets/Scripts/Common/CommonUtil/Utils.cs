using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
// using Plugins.IOSDiskSpaceUtil;
using UnityEngine;


namespace LetsPiu.Utils
{
    public static class Utils
    {
        /// <summary>
        /// 检查 Application.persistentDataPath 的剩余可用空间是否满足
        /// </summary>
        /// <param name="requiredSize">需要的字节数</param>
        /// <returns></returns>
        public static bool CheckStorageSpace(long requiredSize)
        {
            // 检查可用空间
            var availableSpace = GetAvailableStorageSpace();
            // 比较可用空间和所需空间
            return availableSpace > requiredSize;
        }
        
        /// <summary>
        /// 检查指定目录的可用空间是否满足
        /// </summary>
        /// <param name="path"></param>
        /// <param name="requiredSize">需要的字节数</param>
        /// <returns></returns>
        public static bool CheckStorageSpace(string path, long requiredSize)
        {
            // 检查可用空间
            var availableSpace = GetAvailableStorageSpace(path);
            // 比较可用空间和所需空间
            return availableSpace > requiredSize;
        }
        
        /// <summary>
        
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetAvailableStorageSpace(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // 使用 AndroidJavaObject 调用 StatFs
            using var statFs = new AndroidJavaObject("android.os.StatFs", path);
            var blockSize = statFs.Call<long>("getBlockSizeLong");
            var availableBlocks = statFs.Call<long>("getAvailableBlocksLong");
            return blockSize * availableBlocks;
#elif UNITY_IOS 
            // 调用原生插件
            Debug.Log("内存多少: " + IOSDiskSpaceUtil.GetFreeSpace(path));
            return  IOSDiskSpaceUtil.GetFreeSpace(path);
#else
            var drive = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(path));
            return drive.AvailableFreeSpace;
#endif
        }
        
        /// <summary>
        /// 获取设备剩余可用空间
        /// </summary>
        /// <returns></returns>
        public static long GetAvailableStorageSpace()
        {
            return GetAvailableStorageSpace(Application.persistentDataPath);
        }
    }
}
