using Cysharp.Threading.Tasks;
using LetsPiu.Base;
using LetsPiu.Main.Common.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.GameLogic
{
    public partial class SettingMgr : SingletonBase<SettingMgr>
    {
        private Dictionary<SettingID, int> settingValues = new Dictionary<SettingID, int>();
        private HashSet<SettingID> needSaveSettings = new HashSet<SettingID>();
        private Dictionary<SettingID, SettingID> presetValueSettingIdMap = new Dictionary<SettingID, SettingID>();

        /// <summary>
        /// 值发生变化后立即执行的方法
        /// </summary>
        private Dictionary<SettingID, Action> settingExecuteFuncImmediate = new Dictionary<SettingID, Action>();
        /// <summary>
        /// 关闭设置界面后执行的方法
        /// </summary>
        private Dictionary<SettingID, Action> settingExecuteFuncSaved = new Dictionary<SettingID, Action>();

        public SettingMgr() 
        {
            foreach (var ids in presetValueSettingIDs)
            {
                foreach (var id in ids.Value)
                    presetValueSettingIdMap.Add(id, ids.Key);
            }

            AddExecuteFuncs();
        }

        public int[] GetSettingOptons(SettingID settingID)
        {
            if (settingOptions.TryGetValue(settingID, out var optionValues))
                return optionValues;
            return null;
        }

        public SettingID[] GetPresetValueSettingIDs(SettingID settingID)
        {
            if (presetValueSettingIDs.TryGetValue(settingID, out var ids))
                return ids;
            return null;
        }

        public int GetSettingPresetValue(SettingID settingID, SettingOption option, SettingID presetValueSettingID)
        {
            if (presetValueSettingIDs.TryGetValue(settingID, out var ids) && settingPresetValues.TryGetValue(settingID, out var valueMap))
            {
                if (valueMap.TryGetValue(option, out var values))
                {
                    int index = Array.IndexOf(ids, presetValueSettingID);
                    if (index >= 0)
                    {
                        return values[index];
                    }
                }
            }
            return 0;
        }

        public int GetSettingDefaultValue(SettingID settingID)
        {
            if (settingDefaultValues.TryGetValue(settingID, out var value))
                return value;
            return 0;
        }

        public Color GetSettingColorValue(SettingOption option)
        {
            if (settingColorValues.TryGetValue(option, out var value))
                return value;
            return Color.white;
        }

        public int GetSettingValue(SettingID settingID)
        {
            int value;
            if (settingValues.TryGetValue(settingID, out value))
            { 
                return value;
            }

            value = PlayerPrefs.GetInt("SettingID_" + settingID.ToString(), -1);
            if (value >= 0)
            {
                settingValues[settingID] = value;
                return value;
            }

            //是预设值SettingID
            if (presetValueSettingIdMap.TryGetValue(settingID, out var id) && !presetValueSettingIdMap.ContainsKey(id))
            {
                int v = GetSettingValue(id);
                if (v != (int)SettingOption.Custom)
                {
                    //非自定义则返回预设值
                    value = GetSettingPresetValue(id, (SettingOption)v, settingID);
                    settingValues[settingID] = value;
                    return value;
                }
            }

            if (settingDefaultValues.TryGetValue(settingID, out value))
            {
                settingValues[settingID] = value;
                return value;
            }

            settingValues[settingID] = 0;
            return 0;
        }

        public void SetSettingValue(SettingID settingID, int value)
        { 
            int originalValue = GetSettingValue(settingID);
            settingValues[settingID] = value;
            if (value != originalValue)
            {
                Debug.Log("SetSettingValue (" + settingID.ToString() + ", " + value + ")");
                needSaveSettings.Add(settingID);
                if (settingExecuteFuncImmediate.TryGetValue(settingID, out var func))
                {
                    func();
                }
            }
        }

        public async UniTaskVoid SaveSettings()
        {
            if (needSaveSettings.Count == 0)
                return;

            List<SettingID> changedIds = new List<SettingID>();
            int value, savedValue;
            foreach (SettingID id in needSaveSettings)
            {
                value = GetSettingValue(id);
                savedValue = PlayerPrefs.GetInt("SettingID_" + id.ToString(), -1);
                if (value != savedValue)
                {
                    PlayerPrefs.SetInt("SettingID_" + id.ToString(), value);
                    changedIds.Add(id);
                }
            }
            PlayerPrefs.Save();

            foreach (SettingID id in changedIds)
            {
                if (settingExecuteFuncSaved.TryGetValue(id, out var func))
                {
                    func();
                    await UniTask.Yield();
                }
            }

            needSaveSettings.Clear();
        }

        public async UniTaskVoid InitSettings()
        {
            foreach (var func in settingExecuteFuncImmediate.Values)
            { 
                func();
                await UniTask.Yield();
            }

            foreach (var func in settingExecuteFuncSaved.Values)
            {
                func();
                await UniTask.Yield();
            }
        }

        private void AddExecuteFuncs()
        {
            settingExecuteFuncImmediate.Add(SettingID.FrameRate, ExecuteFrameRateImmediate);
            settingExecuteFuncImmediate.Add(SettingID.ResolutionRatio, ExecuteResolutionRatioImmediate);
            
            settingExecuteFuncImmediate.Add(SettingID.GlobalSoundEnable, ExecuteGlobalSoundEnableImmediate);
            settingExecuteFuncImmediate.Add(SettingID.BackgroundMusicEnable, ExecuteBackgroundMusicEnableImmediate);
            settingExecuteFuncImmediate.Add(SettingID.BackgroundMusic, ExecuteBackgroundMusicImmediate);
            settingExecuteFuncImmediate.Add(SettingID.GameSoundEnable, ExecuteGameSoundEnableImmediate);
            settingExecuteFuncImmediate.Add(SettingID.GameSound, ExecuteGameSoundImmediate);
            settingExecuteFuncImmediate.Add(SettingID.CharacterSoundEnable, ExecuteCharacterSoundEnableImmediate);
            settingExecuteFuncImmediate.Add(SettingID.CharacterSound, ExecuteCharacterSoundImmediate);
            settingExecuteFuncImmediate.Add(SettingID.KeySoundEnable, ExecuteKeySoundEnableImmediate);
            settingExecuteFuncImmediate.Add(SettingID.KeySound, ExecuteKeySoundImmediate);

            settingExecuteFuncImmediate.Add(SettingID.Loudspeaker, ExecuteLoudspeakerImmediate);
            settingExecuteFuncImmediate.Add(SettingID.Microphone, ExecuteMicrophoneImmediate);

            settingExecuteFuncImmediate.Add(SettingID.FrontSightColor_Offline, ExecuteFrontSightColorImmediate);
            settingExecuteFuncImmediate.Add(SettingID.FrontSightColor_Online, ExecuteFrontSightColorImmediate);

            settingExecuteFuncSaved.Add(SettingID.GraphicsQuality, ExecuteGraphicsQualitySaved);
            settingExecuteFuncSaved.Add(SettingID.RealTimeShadowEnable, ExecuteRealTimeShadowEnableSaved);
            settingExecuteFuncSaved.Add(SettingID.AntiAliasingEnable, ExecuteAntiAliasingEnableSaved);
        }

        private void ExecuteFrameRateImmediate()
        {
            ApplyFrameRate(GetSettingValue(SettingID.FrameRate));
        }

        const int ForcedFrameRate = 30;

        /// <summary>
        /// 应用目标帧率。当前强制锁定为 ForcedFrameRate，忽略传入的 fps 与已保存的设置。
        /// 同时关闭垂直同步，防止安卓默认质量档 vSyncCount=1 覆盖 targetFrameRate 造成帧率被锁。
        /// </summary>
        public void ApplyFrameRate(int fps)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = ForcedFrameRate;
            Debug.Log("bdl_ Target Frame Rate set to: " + Application.targetFrameRate);
        }

        private void ExecuteResolutionRatioImmediate()
        {
            return;

            if (Application.isEditor)
                return;

            SettingOption option = (SettingOption)GetSettingValue(SettingID.ResolutionRatio);
            switch (option)
            {
                case SettingOption.ResolutionRatio_Standard:
                    Screen.SetResolution(720, 576, true);
                    break;
                case SettingOption.ResolutionRatio_High:
                    Screen.SetResolution(1920, 1080, true);
                    break;
                case SettingOption.ResolutionRatio_UltraHigh:
                    Screen.SetResolution(2560, 1440, true);
                    break;
            }
        }

        private void ExecuteGlobalSoundEnableImmediate()
        {
            int value = GetSettingValue(SettingID.GlobalSoundEnable);
            AudioListener.volume = value;
        }
        private void ExecuteBackgroundMusicEnableImmediate()
        {
            int value = GetSettingValue(SettingID.BackgroundMusicEnable);
            AudioPlayModule.S.SetAudioMuteByKind(TDAudioResource.AudioKind.BackgroundMusic, value != 1);
        }
        private void ExecuteBackgroundMusicImmediate()
        {
            int value = GetSettingValue(SettingID.BackgroundMusic);
            AudioPlayModule.S.SetAudioVolumeByKind(TDAudioResource.AudioKind.BackgroundMusic, value * 0.01f);
        }
        private void ExecuteGameSoundEnableImmediate()
        {
            int value = GetSettingValue(SettingID.GameSoundEnable);
            AudioPlayModule.S.SetAudioMuteByKind(TDAudioResource.AudioKind.GameSound, value != 1);
        }
        private void ExecuteGameSoundImmediate()
        {
            int value = GetSettingValue(SettingID.GameSound);
            AudioPlayModule.S.SetAudioVolumeByKind(TDAudioResource.AudioKind.GameSound, value * 0.01f);
        }
        private void ExecuteCharacterSoundEnableImmediate()
        {
            int value = GetSettingValue(SettingID.CharacterSoundEnable);
            AudioPlayModule.S.SetAudioMuteByKind(TDAudioResource.AudioKind.CharacterSound, value != 1);
        }
        private void ExecuteCharacterSoundImmediate()
        {
            int value = GetSettingValue(SettingID.CharacterSound);
            AudioPlayModule.S.SetAudioVolumeByKind(TDAudioResource.AudioKind.CharacterSound, value * 0.01f);
        }
        private void ExecuteKeySoundEnableImmediate()
        {
            int value = GetSettingValue(SettingID.KeySoundEnable);
            AudioPlayModule.S.SetAudioMuteByKind(TDAudioResource.AudioKind.KeySound, value != 1);
        }
        private void ExecuteKeySoundImmediate()
        {
            int value = GetSettingValue(SettingID.KeySound);
            AudioPlayModule.S.SetAudioVolumeByKind(TDAudioResource.AudioKind.KeySound, value * 0.01f);
        }

        private void ExecuteLoudspeakerImmediate()
        {
            int value = GetSettingValue(SettingID.Loudspeaker);
            MusicDownloadManager.Instance.SetBgmVolume(value * 0.01f);
        }
        private void ExecuteMicrophoneImmediate()
        {
            int value = GetSettingValue(SettingID.Microphone);
            VoiceManager.Instance.SetMicrophoneVolume(value * 0.01f);
        }

        private void ExecuteFrontSightColorImmediate()
        {
            EventManager.Instance.Broadcast(EventMsg.OnGunAimColorChanged);
        }

        private void ExecuteGraphicsQualitySaved()
        {
            SettingOption option = (SettingOption)GetSettingValue(SettingID.GraphicsQuality);
            switch (option)
            {
                case SettingOption.GraphicsQuality_Low:
                    QualitySettings.SetQualityLevel(0, false);
                    break;
                case SettingOption.GraphicsQuality_Medium:
                    QualitySettings.SetQualityLevel(Mathf.RoundToInt((QualitySettings.count - 1) / 3.0f), false);
                    break;
                case SettingOption.GraphicsQuality_High:
                    QualitySettings.SetQualityLevel(Mathf.RoundToInt((QualitySettings.count - 1) / 3.0f * 2), false);
                    break;
                case SettingOption.GraphicsQuality_UltraHigh:
                    QualitySettings.SetQualityLevel(QualitySettings.count - 1, false);
                    break;
                case SettingOption.Custom:
                    break;
            }
        }

        private void ExecuteRealTimeShadowEnableSaved()
        {
            int value = GetSettingValue(SettingID.RealTimeShadowEnable);
            if (value == 1)
                QualitySettings.shadows = ShadowQuality.All;
            else
                QualitySettings.shadows = ShadowQuality.Disable;
        }

        private void ExecuteAntiAliasingEnableSaved()
        {
            int value = GetSettingValue(SettingID.AntiAliasingEnable);
            if (value == 1)
                QualitySettings.antiAliasing = 4;
            else
                QualitySettings.antiAliasing = 0;
        }
    }
}
