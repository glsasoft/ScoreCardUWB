using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandParticleSystem
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class NewSubParticleSystem : MonoBehaviour
    {
        /// <summary>
        /// 使用此系统
        /// </summary>
        public bool useNewSystem = false;
        /// <summary>
        /// 使用碰撞
        /// </summary>
        public bool useCollide = false;

        public ParticleSystemSubEmitterType triggerType;//出现方式

        public float size;//碰撞大小
        public List<ParticleSystem> subParticleSystems;//子系统

        public LayerMask layerMask;//碰撞层

        public bool isColliderLayer = false;//是否根据层级碰撞


        private ParticleSystem particle;//自己
        private void Start()
        {
            hideFlags = HideFlags.HideInInspector;
        }
        public void OnEnable()
        {
            if (subParticleSystems == null)
            {
                subParticleSystems = new List<ParticleSystem>();
                subParticleSystems.Add(null);
            }
            particle = GetComponent<ParticleSystem>();
            OpenOldSubSystem();

        }
        public void OnDisable()
        {
            useCollide = false;
            CloseSubSystem(false);
        }
        /// <summary>
        /// 设置久的碰撞器
        /// </summary>
        public void OpenOldCollide()
        {
            var c = particle.collision;
            c.enabled = useCollide && useNewSystem;
            if (useCollide && isColliderLayer)
            {
                c.collidesWith = layerMask;
            }
            else
            {
                c.collidesWith = LayerMask.NameToLayer("Everything");
            }
            c.type = ParticleSystemCollisionType.World;
            c.lifetimeLoss = 1f;
            c.radiusScale = size;
            c.sendCollisionMessages = true;
        }
        /// <summary>
        /// 设置久的子发射器
        /// </summary>
        public void OpenOldSubSystem(int index = -1)
        {
            if (index >= 0 && subParticleSystems[index].Equals(particle))
            {
                Debug.LogError("别,一天天这么皮,还想自己射自己");
                subParticleSystems[index] = null;
                return;

            }
            CloseSubSystem();
            for (int i = 0; i < subParticleSystems.Count; i++)
            {
                if (subParticleSystems[i] == null)
                {
                    continue;
                }
                //sub.AddSubEmitter(subParticleSystems[i], ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritNothing,1);
                ParticleSystem[] ps = subParticleSystems[i].GetComponentsInChildren<ParticleSystem>();
                for (int x = 0; x < ps.Length; x++)
                {
                    #if UNITY_2018
                        particle.subEmitters.AddSubEmitter(ps[x], triggerType, ParticleSystemSubEmitterProperties.InheritNothing);
                    #else 
                        particle.subEmitters.AddSubEmitter(ps[x], triggerType, ParticleSystemSubEmitterProperties.InheritNothing, 1);
                    #endif
                }
            }
        }
        public void SetParent(GameObject gameObject)
        {
            gameObject.transform.SetParent(particle.transform);
            gameObject.transform.localPosition = Vector3.zero;
        }
        /// <summary>
        /// 清空所有的子类
        /// </summary>
        private void CloseSubSystem(bool isClose = true)
        {
            var sub = particle.subEmitters;
            sub.enabled = isClose && useNewSystem;
            int countSubEmitters = sub.subEmittersCount;
            for (int index = 0; index < countSubEmitters; index++)
            {
                sub.RemoveSubEmitter(0);
            }
        }
        /// <summary>
        /// 设置生成方式
        /// </summary>
        public void SetParticleSystemSubEmitterType()
        {
            var sub = particle.subEmitters;
            for (int i = 0; i < sub.subEmittersCount; i++)
            {
                sub.SetSubEmitterType(i, triggerType);
            }

        }
        public void RemoveSubParticleSystem(ParticleSystem particleSystem)
        {
            ParticleSystem[] ps = particleSystem.GetComponentsInChildren<ParticleSystem>();

            var sub = particle.subEmitters;
            for (int i = 0; i < sub.subEmittersCount; i++)
            {
                for (int x = 0; x < ps.Length; x++)
                {
                    if (sub.GetSubEmitterSystem(i).Equals(ps[x]))
                    {
                        sub.RemoveSubEmitter(i);
                    }
                }
            }
        }
        //private void OnParticleCollision(GameObject other)
        //{
        //    ParticleSystem.Particle[]  particles = new ParticleSystem.Particle[particle.particleCount];
        //    particle.GetParticles(particles);
        //    if (triggerType == TriggerType.Trigger)
        //    {
        //        for (int i = 0; i < particles.Length; i++)
        //        {
        //            if (particles[i].remainingLifetime <= 0.1f)
        //            {
        //                foreach (ParticleSystem p in subParticleSystems)
        //                {
        //                    ParticleSystem sub = Instantiate(p).gameObject.GetComponent<ParticleSystem>();
        //                    sub.transform.SetParent(particle.transform);
        //                    sub.transform.position = particles[i].position + particle.transform.position;
        //                    sub.Play();
        //                    sub.gameObject.AddComponent<AutoDestory>();
        //                }
        //            }
        //        }

        //    }
        //}
        //public void CreateSubParticle(GameObject pos)
        //{
        //    for(int i=0;i< subParticleSystems.Count;i++)
        //    {
        //        ParticleSystem sub = Instantiate(subParticleSystems[i]).gameObject.GetComponent<ParticleSystem>();
        //        sub.transform.SetParent(particle.transform);
        //        sub.transform.position = pos.transform.position;
        //        sub.Play();
        //        sub.gameObject.AddComponent<AutoDestory>();
        //    }

        //}
        public void Update()
        {
            //ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particle.particleCount];
            //particle.GetParticles(particles);

            //if (subParticleSystems == null || subParticleSystems[0] == null)
            //{
            //    return;
            //}
            //if(triggerType == TriggerType.Death)
            //{

            //    for (int i = 0; i < particles.Length; i++)
            //    {
            //        if (particles[i].remainingLifetime <= 0.02f)
            //        {
            //            for (int x = 0; x < subParticleSystems.Count; x++)
            //            {
            //                ParticleSystem sub = Instantiate(subParticleSystems[x]).gameObject.GetComponent<ParticleSystem>();
            //                sub.transform.SetParent(particle.transform);
            //                sub.transform.position = particles[i].position + particle.transform.position;
            //                sub.Play();
            //                sub.gameObject.AddComponent<AutoDestory>();
            //            }
            //            particles[i].remainingLifetime = -0.1f;

            //        }
            //    }

            //}

        }
    }
    //[DisallowMultipleComponent]
    //[ExecuteInEditMode]
    //public class AutoDestory : MonoBehaviour
    //{
    //    public ParticleSystem particle;
    //    private void Start()
    //    {
    //        particle = GetComponent<ParticleSystem>();
    //        this.gameObject.hideFlags = HideFlags.DontSaveInEditor| HideFlags.DontSave| HideFlags.DontSaveInBuild;
    //    }
    //    private void Update()
    //    {
    //        if(particle.isStopped)
    //        {
    //            DestroyImmediate(gameObject);
    //        }
    //    }
    //}

}
