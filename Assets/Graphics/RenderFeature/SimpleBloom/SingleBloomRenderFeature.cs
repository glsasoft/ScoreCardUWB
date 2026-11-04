using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SingleBloomRenderFeature : ScriptableRendererFeature
{
    [Serializable]
    public struct BloomSetting
    {
        public Material BloomMat;
        [Range(2, 14)] public int BlitStep;
        [Header("特效")]
        public Color BloomColor_Effect;
        [Range(0.0f, 2f)] public float LuminanceThreshold_Effect;
        [Range(0, 1)] public float ThresholdKnee_Effect;
        [Range(0, 10)] public int firstDownSampleRadius_Effect;
        [Range(0, 5)] public float firstDownSampleSigma_Effect;
        [Range(0, 10)] public int downSampleRadius_Effect;
        [Range(0, 5)] public float downSampleSigma_Effect;
        [Range(0, 10)] public int upSampleRadius_Effect;
        [Range(0, 5)] public float upSampleSigma_Effect;
        [Range(0, 10)] public float bloomIntensity_Effect;
        [Header("场景")]
        public Color BloomColor_Scene;
        [Range(0.0f, 5f)] public float LuminanceThreshold_Scene;
        [Range(0, 1)] public float ThresholdKnee_Scene;
        [Range(0, 10)] public int firstDownSampleRadius_Scene;
        [Range(0, 5)] public float firstDownSampleSigma_Scene;
        [Range(0, 10)] public int downSampleRadius_Scene;
        [Range(0, 5)] public float downSampleSigma_Scene;
        [Range(0, 10)] public int upSampleRadius_Scene;
        [Range(0, 5)] public float upSampleSigma_Scene;
        [Range(0, 5)] public float bloomIntensity_Scene;
    }

    public BloomSetting setting;

    class SingleBloomRenderPass : ScriptableRenderPass
    {
        Material bloomMat;
        private Color _bloomColor_Effect;
        int _blitStep = 7;
        float _luminanceThreshold_Effect = 1.0f;
        private float _thresholdKnee_Effect = 1.0f;
        private int _firstDownSampleRadius_Effect = 5;
        private float _firstDownSampleSigma_Effect = 1;
        private int _downSampleRadius_Effect = 5;
        private float _downSampleSigma_Effect = 1;
        private int _upSampleRadius_Effect = 5;
        private float _upSampleSigma_Effect = 1;
        private float _bloomIntensity_Effect = 1;
        
        private Color _bloomColor_Scene;
        float _luminanceThreshold_Scene = 1.0f;
        private float _thresholdKnee_Scene = 1.0f;
        private int _firstDownSampleRadius_Scene = 5;
        private float _firstDownSampleSigma_Scene = 1;
        private int _downSampleRadius_Scene = 5;
        private float _downSampleSigma_Scene = 1;
        private int _upSampleRadius_Scene = 5;
        private float _upSampleSigma_Scene = 1;
        private float _bloomIntensity_Scene = 1;

        private readonly int PASS_DOWNSIZE_FIRST = 3;
        private readonly int PASS_DOWNSIZE = 0;
        private readonly int PASS_UPSIZE = 1;
        private readonly int PASS_BLIT = 2;
        private readonly int PASS_THRESHOLD_MASK = 4;
        private readonly int PASS_THRESHOLD_MERGE_EFFECT = 5;
        private readonly int PASS_THRESHOLD_MERGE_SCENE = 6;
        
        readonly int Shader_PreMip = Shader.PropertyToID("_PrewMip");
        readonly int Shader_BloomTex = Shader.PropertyToID("_BloomTex");
        private readonly int Shader_ThresholdKnee = Shader.PropertyToID("_BloomThreshold");
        private readonly int Shader_firstDownSampleRadius = Shader.PropertyToID("firstDownSampleRadius");
        private readonly int Shader_firstDownSampleSigma = Shader.PropertyToID("firstDownSampleSigma");
        private readonly int Shader_downSampleRadius = Shader.PropertyToID("downSampleRadius");
        private readonly int Shader_downSampleSigma = Shader.PropertyToID("downSampleSigma");
        private readonly int Shader_upSampleRadius = Shader.PropertyToID("upSampleRadius");
        private readonly int Shader_upSampleSigma = Shader.PropertyToID("upSampleSigma");
        
        private readonly int Shader_bloomIntensityEffect = Shader.PropertyToID("bloomIntensityEffect");
        private readonly int Shader_bloomIntensityScene = Shader.PropertyToID("bloomIntensityScene");
        private readonly int Shader_bloomColorEffect = Shader.PropertyToID("bloomColorEffect");
        private readonly int Shader_bloomColorScene = Shader.PropertyToID("bloomColorScene");
        
        int RT_Tmp0 = Shader.PropertyToID("RT_Tmp0_Bloom"); // 无意义ID
        int RT_Tmp1 = Shader.PropertyToID("RT_Tmp1_Bloom");
        private int[] RT_DownsizeIds;
        private int[] RT_UpsizeIds;
        
        public void Setup(BloomSetting setting)
        {
            this.bloomMat = setting.BloomMat;
            this._blitStep = setting.BlitStep;

            this._bloomColor_Effect = setting.BloomColor_Effect;
            this._luminanceThreshold_Effect = setting.LuminanceThreshold_Effect;
            this._thresholdKnee_Effect = setting.ThresholdKnee_Effect;
            this._firstDownSampleRadius_Effect = setting.firstDownSampleRadius_Effect;
            this._firstDownSampleSigma_Effect = setting.firstDownSampleSigma_Effect;
            this._downSampleRadius_Effect = setting.downSampleRadius_Effect;
            this._downSampleSigma_Effect = setting.downSampleSigma_Effect;
            this._upSampleRadius_Effect = setting.upSampleRadius_Effect;
            this._upSampleSigma_Effect = setting.upSampleSigma_Effect;
            this._bloomIntensity_Effect = setting.bloomIntensity_Effect;

            this._bloomColor_Scene = setting.BloomColor_Scene;
            this._luminanceThreshold_Scene = setting.LuminanceThreshold_Scene;
            this._thresholdKnee_Scene = setting.ThresholdKnee_Scene;
            this._firstDownSampleRadius_Scene = setting.firstDownSampleRadius_Scene;
            this._firstDownSampleSigma_Scene = setting.firstDownSampleSigma_Scene;
            this._downSampleRadius_Scene = setting.downSampleRadius_Scene;
            this._downSampleSigma_Scene = setting.downSampleSigma_Scene;
            this._upSampleRadius_Scene = setting.upSampleRadius_Scene;
            this._upSampleSigma_Scene = setting.upSampleSigma_Scene;
            this._bloomIntensity_Scene = setting.bloomIntensity_Scene;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            int N = _blitStep;
            RT_DownsizeIds = new int[N];
            RT_UpsizeIds = new int[N - 1];

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.mipCount = 0;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            descriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            
            cmd.GetTemporaryRT(RT_Tmp0, descriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(RT_Tmp1, descriptor, FilterMode.Bilinear);
            int width = descriptor.width;
            int height = descriptor.height;
            int downSize = 2;
            
            for (int i = 0; i < N; i++)
            {
                int w = width / downSize;
                int h = height / downSize;
                if (w <= 0 || h <= 0) break;
                RenderTextureDescriptor desc = descriptor;
                desc.width = w;
                desc.height = h;
                RT_DownsizeIds[i] = Shader.PropertyToID("BloomDown" + i);
                cmd.GetTemporaryRT(RT_DownsizeIds[i], desc, FilterMode.Bilinear);
                if (i < N - 1)
                {
                    int up_i = RT_UpsizeIds.Length - i - 1;
                    RT_UpsizeIds[up_i] = Shader.PropertyToID("BloomUp" + up_i);
                    cmd.GetTemporaryRT(RT_UpsizeIds[up_i], desc, FilterMode.Bilinear);
                }
                downSize *= 2;
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("SingleBloom");

            int N = _blitStep;
            
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            cmd.SetGlobalFloat(Shader_bloomIntensityEffect, _bloomIntensity_Effect);
            cmd.SetGlobalFloat(Shader_bloomIntensityScene, _bloomIntensity_Scene);
            cmd.SetGlobalColor(Shader_bloomColorEffect, _bloomColor_Effect);
            cmd.SetGlobalColor(Shader_bloomColorScene, _bloomColor_Scene);

            cmd.SetGlobalInt(Shader_firstDownSampleRadius, _firstDownSampleRadius_Effect);
            cmd.SetGlobalFloat(Shader_firstDownSampleSigma, _firstDownSampleSigma_Effect);
            cmd.SetGlobalInt(Shader_downSampleRadius, _downSampleRadius_Effect);
            cmd.SetGlobalFloat(Shader_downSampleSigma, _downSampleSigma_Effect);
            cmd.SetGlobalInt(Shader_upSampleRadius, _upSampleRadius_Effect);
            cmd.SetGlobalFloat(Shader_upSampleSigma, _upSampleSigma_Effect);
            cmd.SetGlobalFloat(Shader_upSampleSigma, _upSampleSigma_Effect);

            Vector4 thresholdKnee;
            thresholdKnee.x = Mathf.GammaToLinearSpace(_luminanceThreshold_Effect);
            thresholdKnee.y = thresholdKnee.x * _thresholdKnee_Effect;
            thresholdKnee.z = 2f * thresholdKnee.y;
            thresholdKnee.w = 0.25f / (thresholdKnee.y + 0.00001f);
            thresholdKnee.y -= thresholdKnee.x;
            cmd.SetGlobalVector(Shader_ThresholdKnee, thresholdKnee);
            
            // Effect Threshold && Stencil
            cmd.Blit(DrawEffectRenderFeature.DrawEffectRenderPass._Bloom_Tex, RT_Tmp0, bloomMat, PASS_THRESHOLD_MASK);
            
            // Effect First DownSample
            cmd.Blit(RT_Tmp0, RT_DownsizeIds[0],
                                                bloomMat, PASS_DOWNSIZE_FIRST);
            
            // Effect DownSample
            for (int i = 1; i < RT_DownsizeIds.Length;i++)
            {
                cmd.Blit(RT_DownsizeIds[i - 1], RT_DownsizeIds[i], bloomMat, PASS_DOWNSIZE);
            }

            // Effect UpSample
            cmd.SetGlobalTexture(Shader_PreMip, RT_DownsizeIds[N - 1]);
            cmd.Blit(RT_DownsizeIds[N - 2], RT_UpsizeIds[0], bloomMat, PASS_UPSIZE);
            for (int i = 1; i < N - 1; i++)
            {
                int pre_mip = RT_UpsizeIds[i - 1];
                int curr_mip = RT_DownsizeIds[N - 2 - i];
                cmd.SetGlobalTexture(Shader_PreMip, pre_mip);
                cmd.Blit(curr_mip, RT_UpsizeIds[i], bloomMat, PASS_UPSIZE);
            }
            
            cmd.SetGlobalTexture(Shader_BloomTex, RT_UpsizeIds[N - 2]);
            cmd.Blit(DrawEffectRenderFeature.DrawEffectRenderPass._Bloom_Tex, RT_Tmp1, bloomMat, PASS_THRESHOLD_MERGE_EFFECT);
            
            cmd.SetGlobalInt(Shader_firstDownSampleRadius, _firstDownSampleRadius_Scene);
            cmd.SetGlobalFloat(Shader_firstDownSampleSigma, _firstDownSampleSigma_Scene);
            cmd.SetGlobalInt(Shader_downSampleRadius, _downSampleRadius_Scene);
            cmd.SetGlobalFloat(Shader_downSampleSigma, _downSampleSigma_Scene);
            cmd.SetGlobalInt(Shader_upSampleRadius, _upSampleRadius_Scene);
            cmd.SetGlobalFloat(Shader_upSampleSigma, _upSampleSigma_Scene);
            cmd.SetGlobalFloat(Shader_upSampleSigma, _upSampleSigma_Scene);
            
            thresholdKnee.x = Mathf.GammaToLinearSpace(_luminanceThreshold_Scene);
            thresholdKnee.y = thresholdKnee.x * _thresholdKnee_Scene;
            thresholdKnee.z = 2f * thresholdKnee.y;
            thresholdKnee.w = 0.25f / (thresholdKnee.y + 0.00001f);
            thresholdKnee.y -= thresholdKnee.x;
            cmd.SetGlobalVector(Shader_ThresholdKnee, thresholdKnee);
            
            // Scene Threshold && Stencil
            cmd.Blit(source, RT_Tmp0, bloomMat, PASS_THRESHOLD_MASK);
            
            // Scene First DownSample
            cmd.Blit(RT_Tmp0, RT_DownsizeIds[0],
                bloomMat, PASS_DOWNSIZE_FIRST);
            
            // Scene DownSample
            for (int i = 1; i < RT_DownsizeIds.Length;i++)
            {
                cmd.Blit(RT_DownsizeIds[i - 1], RT_DownsizeIds[i], bloomMat, PASS_DOWNSIZE);
            }
            
            // Scene UpSample
            cmd.SetGlobalTexture(Shader_PreMip, RT_DownsizeIds[N - 1]);
            cmd.Blit(RT_DownsizeIds[N - 2], RT_UpsizeIds[0], bloomMat, PASS_UPSIZE);
            for (int i = 1; i < N - 1; i++)
            {
                int pre_mip = RT_UpsizeIds[i - 1];
                int curr_mip = RT_DownsizeIds[N - 2 - i];
                cmd.SetGlobalTexture(Shader_PreMip, pre_mip);
                cmd.Blit(curr_mip, RT_UpsizeIds[i], bloomMat, PASS_UPSIZE);
            }
            
            cmd.SetGlobalTexture(Shader_BloomTex, RT_UpsizeIds[N - 2]);
            cmd.Blit(source, RT_Tmp0, bloomMat, PASS_THRESHOLD_MERGE_SCENE);

            // Output
            cmd.SetGlobalTexture("_EffectBloomTex", RT_Tmp1);
            cmd.Blit(RT_Tmp0, source, bloomMat, PASS_BLIT);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(RT_Tmp0);
            cmd.ReleaseTemporaryRT(RT_Tmp1);
            foreach (var id in RT_DownsizeIds)
            {
                cmd.ReleaseTemporaryRT(id);
            }
            foreach (var id in RT_UpsizeIds)
            {
                cmd.ReleaseTemporaryRT(id);
            }
        }
    }
    
    SingleBloomRenderPass m_ScriptablePass;
    public override void Create()
    {
        m_ScriptablePass = new SingleBloomRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_ScriptablePass.Setup(setting);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


