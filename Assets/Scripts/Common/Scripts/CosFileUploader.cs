using System;
using System.Globalization;
using System.IO;
using Common.Network.Model.Http;
using COSXML;
using COSXML.Auth;
using COSXML.Transfer;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define.ModelDefine;
using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// 腾讯云COS文件上传工具
    /// </summary>
    public class CosFileUploader : SingletonComponent<CosFileUploader>
    {
        /// <summary>
        /// 上传用户图片
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <param name="destroyTexture"></param>
        public void UploadImage(string customerId, Texture2D texture, ImageUploadType type, Action<FileUploadResult> callback, bool destroyTexture = true)
        {
            StartCoroutine(UploadImageAsync(customerId, texture, type, destroyTexture).ToCoroutine(callback));
        }

        /// <summary>
        /// 上传用户图片
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="type"></param>
        /// <param name="destroyTexture"></param>
        /// <returns></returns>
        public async UniTask<FileUploadResult> UploadImageAsync(string customerId, Texture2D texture, ImageUploadType type, bool destroyTexture = true)
        {
            // string fileName;
            // switch (type)
            // {
            //     case ImageUploadType.UserHead:
            //         fileName = "HEAD_IMAGE_" + AppStateManager.Instance.Customer.id + "_" + DateTime.Now.Millisecond + ".jpg"; ;
            //         break;
            //     case ImageUploadType.UserImage:
            //         fileName = "USER_IMAGE_" + AppStateManager.Instance.Customer.id + "_" + DateTime.Now.Millisecond + ".jpg";
            //         break;
            //     default:
            //         return new FileUploadResult(null, FileUploadError.TypeNotSupport);
            // }
            //
            // var bytes = texture.EncodeToJPG();
            // var filePath = Path.Combine(Application.persistentDataPath, fileName);
            // await File.WriteAllBytesAsync(filePath, bytes);
            // //释放资源
            // if (destroyTexture) Destroy(texture);
            //
            // var cosAuthResp = await HttpConnector.Instance.AppGetCosCredentialsAsync();
            // if (!cosAuthResp.success || cosAuthResp.data == null)
            // {
            //     Debug.Log("COSFileUploader 腾讯云cos密钥获取失败:" + cosAuthResp.msg);
            //     return new FileUploadResult(null, FileUploadError.CosAuthError);
            // }
            //
            // var cosAuth = cosAuthResp.data;
            //
            // Debug.Log("COSFileUploader 获取腾讯云cos密钥: " + cosAuth.credentials.tmpSecretKey);
            //
            // try
            // {
            //     const string region = "ap-shanghai";
            //     var config = new CosXmlConfig.Builder()
            //         .IsHttps(true)  //设置默认 HTTPS 请求
            //         .SetRegion(region)  //设置一个默认的存储桶地域
            //         .Build();  //创建 CosXmlConfig 对象
            //     var cosCredentialProvider = new CustomQCloudCredentialProvider(cosAuth);
            //     //使用 CosXmlConfig 与 QCloudCredentialProvider 初始化 CosXmlServer 服务类。服务类建议在程序中作为单例使用。
            //     CosXml cosXml = new CosXmlServer(config, cosCredentialProvider);
            //
            //     // 初始化 TransferConfig
            //     var transferConfig = new TransferConfig();
            //     // 初始化 TransferManager
            //     var transferManager = new TransferManager(cosXml, transferConfig);
            //
            //     const string bucket = "glsa-piupiu-1305354732"; //存储桶，格式：BucketName-APPID
            //     var date = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            //
            //     var cosPath = "head/" + date + "/" + fileName; //对象在存储桶中的位置标识符，即称对象键
            //     Debug.Log("COSFileUploader cosPath:" + cosPath);
            //     Debug.Log("COSFileUploader bucket:" + bucket);
            //     Debug.Log("COSFileUploader region:" + region);
            //     // 上传对象
            //     var uploadTask = new COSXMLUploadTask(bucket, cosPath);
            //     uploadTask.SetSrcPath(filePath);
            //     uploadTask.progressCallback = delegate (long completed, long total)
            //     {
            //         // Console.WriteLine(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
            //     };
            //
            //     var result = await transferManager.UploadAsync(uploadTask);
            //
            //     var resultInfo = result.GetResultInfo();
            //     var eTag = result.eTag;
            //     Debug.Log("COSFileUploader 图片上传结果:resultInfo=" + resultInfo + " eTag=" + eTag);
            //
            //     if (result.IsSuccessful())
            //     {
            //         var url = "https://" + bucket + ".cos." + region + ".myqcloud.com/" + cosPath;
            //         Debug.Log("COSFileUploader 头像上传成功, url拼接:" + url);
            //         return new FileUploadResult(url, FileUploadError.Success);
            //     }
            //
            //     Debug.LogWarning("ImageUploader 头像上传失败 " + result.httpMessage);
            //     return new FileUploadResult(null, FileUploadError.UploadFail);
            // }
            // catch (Exception e)
            // {
            //     Debug.Log("COSFileUploader 头像上传失败: " + e);
            //     return new FileUploadResult(null, FileUploadError.UploadFail);
            // }
            Debug.Log("COSFileUploader 头像上传失败: 代码已注释");
            return new FileUploadResult(null, FileUploadError.UploadFail);
        }

        public async UniTask<FileUploadResult> UploadVoiceAsync(string filePath)
        {
            // try
            // {
            //     if (AppStateManager.Instance.Customer == null)
            //     {
            //         Debug.LogError("COSFileUploader 用户未登录!");
            //         return new FileUploadResult(null, FileUploadError.NotLogin);
            //     }
            //
            //     var cosAuthResp = await HttpConnector.Instance.AppGetCosCredentialsAsync();
            //     if (!cosAuthResp.success || cosAuthResp.data == null)
            //     {
            //         Debug.LogError("COSFileUploader 腾讯云cos密钥获取失败:" + cosAuthResp.msg);
            //         return new FileUploadResult(null, FileUploadError.CosAuthError);
            //     }
            //
            //     var cosAuth = cosAuthResp.data;
            //
            //     const string region = "ap-shanghai";
            //     var config = new CosXmlConfig.Builder()
            //         .IsHttps(true)  //设置默认 HTTPS 请求
            //         .SetRegion(region)  //设置一个默认的存储桶地域
            //         .Build();  //创建 CosXmlConfig 对象
            //     var cosCredentialProvider = new CustomQCloudCredentialProvider(cosAuth);
            //     //使用 CosXmlConfig 与 QCloudCredentialProvider 初始化 CosXmlServer 服务类。服务类建议在程序中作为单例使用。
            //     CosXml cosXml = new CosXmlServer(config, cosCredentialProvider);
            //
            //     // 初始化 TransferConfig
            //     var transferConfig = new TransferConfig();
            //     // 初始化 TransferManager
            //     var transferManager = new TransferManager(cosXml, transferConfig);
            //
            //     var bucket = "glsa-piupiu-1305354732"; //存储桶，格式：BucketName-APPID
            //     var date = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            //
            //     var cosPath = "voice/" + date + "/" + AppStateManager.Instance.Customer.id + "-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".wav"; //对象在存储桶中的位置标识符，即称对象键
            //     Debug.Log("COSFileUploader cosPath:" + cosPath);
            //     Debug.Log("COSFileUploader bucket:" + bucket);
            //     Debug.Log("COSFileUploader region:" + region);
            //     // 上传对象
            //     var uploadTask = new COSXMLUploadTask(bucket, cosPath);
            //     uploadTask.SetSrcPath(filePath);
            //     uploadTask.progressCallback = delegate (long completed, long total)
            //     {
            //         // Console.WriteLine(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
            //     };
            //
            //     var result = await transferManager.UploadAsync(uploadTask);
            //
            //     var resultInfo = result.GetResultInfo();
            //     var eTag = result.eTag;
            //     Debug.Log("COSFileUploader 语音文件上传结果:resultInfo=" + resultInfo + " eTag=" + eTag);
            //
            //     if (result.IsSuccessful())
            //     {
            //         var url = "https://" + bucket + ".cos." + region + ".myqcloud.com/" + cosPath;
            //         Debug.Log("COSFileUploader 语音上传成功, url拼接:" + url);
            //         return new FileUploadResult(url, FileUploadError.Success);
            //     }
            //
            //     Debug.LogError("COSFileUploader 语音上传失败 " + result.httpMessage);
            //     return new FileUploadResult(null, FileUploadError.UploadFail);
            // }
            // catch (Exception e)
            // {
            //     Debug.LogError("COSFileUploader 语音文件上传失败: " + e);
            //     return new FileUploadResult(null, FileUploadError.UploadFail);
            // }
            Debug.LogError("COSFileUploader 语音文件上传失败: 代码已注释");
            return new FileUploadResult(null, FileUploadError.UploadFail);
        }

        class CustomQCloudCredentialProvider : DefaultSessionQCloudCredentialProvider
        {
            private readonly CosAuthResponse resp;
            // 这里假设开始没有密钥，也可以用初始的临时密钥来初始化
            public CustomQCloudCredentialProvider(CosAuthResponse r) : base(null, null, 0L, null)
            {
                resp = r;
            }

            public override void Refresh()
            {
                var tmpSecretId = resp.credentials.tmpSecretId;
                var tmpSecretKey = resp.credentials.tmpSecretKey;
                var tmpToken = resp.credentials.sessionToken;
                var tmpStartTime = resp.startTime;
                var tmpExpiredTime = resp.expiredTime;
                // 调用接口更新密钥
                SetQCloudCredential(tmpSecretId, tmpSecretKey,
                    $"{tmpStartTime};{tmpExpiredTime}", tmpToken);
            }
        }
    }
}