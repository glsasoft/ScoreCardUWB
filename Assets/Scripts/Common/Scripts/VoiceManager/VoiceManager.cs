using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define.ModelDefine;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.VoiceManager;
using Plugins;
using UnityEngine;

namespace LetsPiu
{
    
    public class VoiceManager : SingletonComponent<VoiceManager>
    {
        private bool initSuccess = false;
        private int minFreq, maxFreq;//最小和最大频率
        private DateTime? startRecordTime = null;
        private AudioClip recordedClip;
        private float m_microphoneVolume = 1;

        private CancellationTokenSource _cancellationTokenSource;

        private void OnEnable()
        {
            Debug.Log("VoiceManager OnEnable...");
            // 对象启用时启动任务
            _cancellationTokenSource = new CancellationTokenSource();
            CheckRecordingDurationAsync(_cancellationTokenSource.Token).Forget();
        }

        private void OnDisable()
        {
            Debug.Log("VoiceManager OnDisable...");
            // 对象禁用时取消任务
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void Start()
        {
            Debug.Log("VoiceManager Start...");
        }

        private async UniTask CheckRecordingDurationAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.WaitForSeconds(1, cancellationToken: cancellationToken);
                if (!IsRecording() || startRecordTime == null) continue;
                var duration = DateTime.Now - startRecordTime.Value;
                if (!(duration.TotalSeconds >= 60)) continue;
                Debug.LogWarning("VoiceManager 录音时间超过1分钟，主动结束录音");
                StopRecording();
            }
        }

        public void SetMicrophoneVolume(float volume)
        {
            m_microphoneVolume = Mathf.Clamp01(volume);
        }

        public async UniTask<VoiceManagerStartResult> Init()
        {
            if(initSuccess) return VoiceManagerStartResult.Success;
            var hasPermission = await RequestPermission();

            if (!hasPermission)
            {
                return VoiceManagerStartResult.ErrorNoPermission;
            }
            
            var microphoneConnected = Microphone.devices.Length > 0;
            if (!microphoneConnected)
            {
                return VoiceManagerStartResult.ErrorNoDevice;
            }
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
            if (minFreq == 0 && maxFreq == 0)
            {
                maxFreq = 44100;
            }
            
            initSuccess = true;
            return VoiceManagerStartResult.Success;
        }

        public void StartRecording()
        {
            Debug.Log("VoiceManager StartRecording...");
            StartRecordingAsync().Forget();   
        }

        public async UniTask<VoiceManagerStartResult> StartRecordingAsync()
        {
            Debug.Log("VoiceManager StartRecordingAsync...");
            var result = await Init();
            Debug.Log("VoiceManager Init Result:"+result);
            if (!initSuccess)
            {
                Debug.LogWarning("VoiceManager 初始化失败");
                EventManager.Instance.Broadcast(EventMsg.VoiceManagerRecordingStartResult, result);
                return result;
            }
            Debug.Log("VoiceManager 初始化成功...");
            if (IsRecording())
            {
                Debug.LogWarning("VoiceManager 正在录音中，无法启动录音...");
                EventManager.Instance.Broadcast(EventMsg.VoiceManagerRecordingStartResult, VoiceManagerStartResult.ErrorAlreadyStarted);
                return VoiceManagerStartResult.ErrorAlreadyStarted;
            }

            try
            {
                
#if UNITY_IOS
                // 解决IOS语音和蓝牙冲突的问题
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    //解决IOS语音和录屏冲突，录屏下不录音
                    
                    //if (ScreenRecorder.Instance.IsRecording())
                    //{
                    //    UIToast.Instance.Show("录屏过程中不可录音，录音启动失败");
                    //    return false;
                    //}
                    //else if (PermissionUtil.IsScreenRecordAuthorized())
                    //{
                    //    Debug.Log("录屏 已授权，录音抢权限开始");
                    //    ScreenRecorder.Instance.recorder.Flush();
                    //    ScreenRecorder.Instance.InitRecorder(false, (success, err) =>
                    //    {
                    //        Debug.Log("录音抢权限结束");
                    //        if (!BlueTooth.CheckAudioSessionOutputVolume())
                    //        {
                    //            // 选择麦克风
                    //            string selectedDevice = devices[0];
                    //            recordedClip = Microphone.Start(selectedDevice, false, 60, maxFreq);
                    //        }
                    //    });
                        
                    //} 
                    //else
                    if (!BlueTooth.CheckAudioSessionOutputVolume())
                    {
                        // 选择麦克风
                        string selectedDevice = Microphone.devices[0];
                        recordedClip = Microphone.Start(null, false, 60, maxFreq);

                    } else {
                        Debug.LogWarning("VoiceManager IOS初始化失败");
                        EventManager.Instance.Broadcast(EventMsg.VoiceManagerRecordingStartResult, result);
                        return result;
                    }
                    
                }
#elif (UNITY_ANDROID && !UNITY_EDITOR)
                recordedClip = Microphone.Start(null, false, 60, maxFreq);
#else
                Debug.Log("VoiceManager PC端开始录音...");
                recordedClip = Microphone.Start(null, false, 60, maxFreq);
#endif
                startRecordTime = DateTime.Now;
            }
            catch (Exception e)
            {
                Debug.LogError("VoiceManager 录音启动异常:"+ e);
                EventManager.Instance.Broadcast(EventMsg.VoiceManagerRecordingStartResult, VoiceManagerStartResult.Error);
                return VoiceManagerStartResult.Error;
            }
            
            Debug.Log("VoiceManager 录音启动成功...");
            EventManager.Instance.Broadcast(EventMsg.VoiceManagerRecordingStartResult, VoiceManagerStartResult.Success);
            return VoiceManagerStartResult.Success;
        }

        public bool IsRecording()
        {
            return initSuccess && Microphone.IsRecording(null);
        }

        public void StopRecording()
        {
            Debug.Log("VoiceManager StopRecording...");
            if (!IsRecording())
            {
                Debug.LogWarning("VoiceManager 没有录音，无法结束!");
                EventManager.Instance.Broadcast<RecordingResult>(EventMsg.VoiceManagerRecordingEnd, null);
                return;
            }
            // 停止录制音频
            Microphone.End(null);
            var duration = DateTime.Now - startRecordTime;
            var durationInSeconds = 0f;
            if (duration != null)
            {
                durationInSeconds = (float)duration.Value.TotalSeconds;
            }
            if (durationInSeconds < 0.96)
            {
                Debug.Log("VoiceManager 录音时间"+durationInSeconds+"秒,太短!");
                EventManager.Instance.Broadcast<RecordingResult>(EventMsg.VoiceManagerRecordingEnd, null);
                return;
            }
            Debug.Log("VoiceManager 结束录音，时长:"+durationInSeconds+"秒.");
            
            EventManager.Instance.Broadcast(EventMsg.VoiceManagerRecordingEnd, new RecordingResult(recordedClip, durationInSeconds));
            
            startRecordTime = null;
            recordedClip = null;
        }

        private async UniTask<bool> RequestPermission()
        {
            await Application.RequestUserAuthorization(UserAuthorization.Microphone);
            var isMicroPhoneAuthorized = Application.HasUserAuthorization(UserAuthorization.Microphone);
#if (UNITY_ANDROID && !UNITY_EDITOR)
            //在Android中申请UserAuthorization.Microphone后，并不会弹出授权麦克风对话框，需要再次处理
            var devices = Microphone.devices;
            isMicroPhoneAuthorized = devices.Length > 0;
#endif
            Debug.Log("VoiceManager 麦克风是否授权:"+isMicroPhoneAuthorized);
            return isMicroPhoneAuthorized;
        }
        
        /// <summary>
        /// 上传录制的语音文件
        /// </summary>
        /// <param name="recordingResult"></param>
        /// <param name="microphoneVolume">音量</param>
        /// <returns></returns>
        public async UniTask<FileUploadResult> UploadAudioAsync(RecordingResult recordingResult, float microphoneVolume)
        {
            var fileData = await GetAudioClipFileAsync(recordingResult, microphoneVolume);
            //保存文件
            var filepath =
                $"{Application.persistentDataPath}/recordings/{DateTime.UtcNow:yyMMdd-HHmmss-fff}.wav";
            var dir = Path.GetDirectoryName(filepath);
            if (dir == null)
            {
                Debug.LogError("VoiceManager 录音文件保存失败:"+filepath);
                return new FileUploadResult(null, FileUploadError.UploadFail);
            }
            Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(filepath, fileData);
            Debug.Log("VoiceManager 录音文件写入成功:"+filepath+" 开始上传...");
            var result = await CosFileUploader.Instance.UploadVoiceAsync(filepath);
            Debug.Log("VoiceManager 录音文件上传结果:"+filepath+" "+result+" 是否成功:"+(result.error == FileUploadError.Success));
            return result;
        }
        
        /// <summary>
        /// 上传语音文件
        /// </summary>
        /// <param name="recordingResult"></param>
        /// <param name="microphoneVolume">音量</param>
        /// <param name="callback"></param>
        public void UploadAudio(RecordingResult recordingResult, float microphoneVolume, Action<FileUploadResult> callback = null)
        {
            UniTask.Post(async () =>
            {
                try
                {
                    var result = await UploadAudioAsync(recordingResult, microphoneVolume);
                    Debug.Log("VoiceManager UploadAudio() 上传结果:"+result);
                    callback?.Invoke(result);
                    EventManager.Instance.Broadcast(EventMsg.VoiceManagerUploadResult, result);
                }
                catch (Exception e)
                {
                    try
                    {
                        EventManager.Instance.Broadcast(EventMsg.VoiceManagerUploadResult, new FileUploadResult(null, FileUploadError.UploadFail));
                    }
                    catch (Exception e1)
                    {
                        Debug.Log("VoiceManager UploadAudio() VoiceManagerUploadResult广播失败:"+e1);
                    }
                    Debug.Log("VoiceManager UploadAudio() 上传失败:"+e);
                }
            });
        }
        
        /// <summary>
        /// 获取裁剪后的音频文件
        /// </summary>
        /// <param name="microphoneVolume">音量</param>
        /// <returns></returns>
        private async UniTask<byte[]> GetAudioClipFileAsync(RecordingResult result, float microphoneVolume)
        {
            if (result.audioClip == null)
            {
                return null;
            }
            var data = await TrimAudioAsync(result.audioClip, result.durationSeconds, microphoneVolume * m_microphoneVolume);
            if (data != null)
            {
                Debug.Log("VoiceManager 音频裁剪完成，时长:"+result.durationSeconds+"秒. 大小:"+data.Length);
            }
            else
            {
                Debug.LogError("VoiceManager 音频裁剪完成，时长:"+result.durationSeconds+"秒. data为空!");
            }
            return data;
        }
        
        /// <summary>
        /// 截取音频，保存到wav文件
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="duration"></param>
        /// <param name="volume"></param>
        /// <returns>wav的文件字节数组</returns>
        private static async UniTask<byte[]> TrimAudioAsync(AudioClip audioClip, float duration, float volume)
        {
            // 在子线程中处理数据（UniTask.Run 会在线程池运行）
            var frequency = audioClip.frequency;
            var samples = audioClip.samples;
            // 获取原始数据并处理
            var audioData = new float[samples];
            audioClip.GetData(audioData, 0); // 注意：GetData 只能在主线程调用！
            var (trimmedData, length) = await UniTask.RunOnThreadPool(() => 
            {
                // 计算截取范围（这部分可以在子线程运行）
                var startSample = Mathf.FloorToInt(0f * frequency);
                var endSample = Mathf.FloorToInt(duration * frequency);
                var length = endSample - startSample;
                
                var trimmedData = new float[length];
                for (var i = 0; i < length; i++)
                {
                    trimmedData[i] = audioData[startSample + i] * volume;
                }

                return (trimmedData, length);
            });
            var trimmedClip = AudioClip.Create("TrimmedClip", length, 1, frequency, false);
            trimmedClip.SetData(trimmedData, 0);
            var data = WavUtility.FromAudioClip(trimmedClip, out var filepath, false);
            
            return data;
        }
    }
}