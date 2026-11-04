using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandParticleSystem
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class ParticleShapeScale : MonoBehaviour
    {
        private ParticleSystem particle;//自己
        void Start()
        {
            hideFlags = HideFlags.HideInInspector;
        }
        public void OnEnable()
        {
            particle = GetComponent<ParticleSystem>();
        }
        void Update()
        {

        }
    }

}
