using System;
using UnityEngine;

namespace LetsPiu.Effect.Scripts
{
    public class IconSkillLoopEffect : ProgressBarEffectBase
    {
        public AnimationCurve curve;
        public new Renderer renderer;

        private Material material;
        
        private static readonly int MainTexSt = Shader.PropertyToID("_MainTex_ST");

        private void Awake()
        {
            material = new Material(renderer.material);
            renderer.material = material;
        }

        private void Update()
        {
            Vector4 tilling = material.GetVector(MainTexSt);
            tilling.y = curve.Evaluate(progress);
            material.SetVector(MainTexSt, tilling);
        }
    }
}