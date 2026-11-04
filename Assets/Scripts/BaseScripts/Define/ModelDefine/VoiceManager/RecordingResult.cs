using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.VoiceManager
{
    public class RecordingResult
    {
        public AudioClip audioClip;
        public float durationSeconds;

        public RecordingResult(AudioClip audioClip, float durationSeconds)
        {
            this.audioClip = audioClip;
            this.durationSeconds = durationSeconds;
        }

        public float GetDurationSeconds()
        {
            return durationSeconds;
        }
    }
}