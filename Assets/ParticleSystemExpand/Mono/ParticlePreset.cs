using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandParticleSystem
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class ParticlePreset : MonoBehaviour
    {
        private ParticleSystem particle;//自己
        private string pathSetFile;//配置文件路径
        public  ParticlePresetClass presetClass;
        public void OnEnable()
        {
            particle = GetComponent<ParticleSystem>();
            pathSetFile = "ParticlePresetClass";
        }
        public void Load()
        {
            presetClass = Resources.Load(pathSetFile) as ParticlePresetClass;
        }
        private void Start()
        {
            hideFlags = HideFlags.HideInInspector;
        }
        public void NullParticle()
        {
            ApplyEmptyParticle(particle);
            particle.transform.localPosition = Vector3.zero;
            particle.transform.localRotation = Quaternion.identity;
            particle.transform.localScale = Vector3.one;
        }
        /// <summary>
        /// 把粒子系统配置成空粒子，参数和场景里的空粒子模板一致
        /// </summary>
        /// <param name="particle">要配置的粒子系统</param>
        public static void ApplyEmptyParticle(ParticleSystem particle)
        {
            if (particle == null)
            {
                return;
            }
            ParticleSystem.EmissionModule emission = particle.emission;
            emission.enabled = false;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = particle.shape;
            shape.enabled = false;
            ParticleSystem.MainModule main = particle.main;
            main.loop = false;
            main.duration = 5f;
            main.startLifetime = 5f;
            main.startSpeed = 0f;
            main.startSize = 1f;
            main.maxParticles = 1000;
            ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = null;
                renderer.sortingOrder = 0;
                renderer.maxParticleSize = 0.5f;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
            }
        }
        public void OneParticle()
        {
            ParticleSystem.MainModule main = particle.main;
            main.startSpeed = 0;
            ParticleSystem.ShapeModule s = particle.shape;
            s.enabled = false;

            ParticleSystem.EmissionModule e = particle.emission;
            e.enabled = true;
            e.rateOverTime = 0;
            e.rateOverDistance = 0;
            ParticleSystem.Burst burst = new ParticleSystem.Burst();
            burst.time = 0;
            burst.count = 1;
            burst.cycleCount = 1;
            burst.repeatInterval = 0.01f;
            #if UNITY_2018
            #else
                 burst.probability = 1;
            #endif
            e.SetBursts(new ParticleSystem.Burst[] { burst });
        }
    }

}
