using System.Collections;
using System.Collections.Generic;
using LetsPiu.Effect;
using UnityEngine;

namespace LetsPiu.Base
{
    public class AlphaTransitionEffect : RepeatingEffectBase
    {
        [Header("透明度设置")]
        [Range(0, 1)] public float minAlpha = 0f;      
        [Range(0, 1)] public float maxAlpha = 1f;     
        public float fadeDuration = 0.5f; // 透明度变化持续时间
        
        public float holdDuration = 0.5f; // 保持最高亮度的时间
    
        private float _currentAlpha; 
        private Coroutine _holdCoroutine;       
        private Coroutine _fadeCoroutine;                       
        private List<ParticleSystem> _particleSystems = new List<ParticleSystem>();

        // 状态跟踪
        private bool _isHolding = false;  // 是否在保持状态
        private bool _isFading = false;   // 是否在变化状态
        private bool _isToMinAlpha = true;
        private float _remainingHoldTime = 0f;
        private float _remainingFadeTime = 0f;
        
        protected void Awake()
        {      
            GetComponentsInChildren<ParticleSystem>(true, _particleSystems);
            if (_particleSystems.Count == 0)
            {
                enabled = false;
                return;
            }
            
            // 初始化透明度
            //_currentAlpha = maxAlpha;
            //SetParticleAlpha(_currentAlpha);

        }

        private void OnEnable()
        {
            if (_isHolding && _holdCoroutine == null)
            {
                _holdCoroutine = StartCoroutine(HoldCoroutine());
            }
            if (_isFading && _fadeCoroutine == null)
            {
                _fadeCoroutine = StartCoroutine(FadeAlphaCoroutine(_currentAlpha, _isToMinAlpha ? minAlpha : maxAlpha));
            }
        }

        private void OnDisable()
        {
            // 停止所有协程
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        public override void Begin()
        {
            // 1. 处理保持状态
            if (_isHolding)
            {
                // 保持状态下再次触发 - 延长保持时间
                _remainingHoldTime += holdDuration;
                if(_remainingHoldTime > 1) _remainingHoldTime = 1;

                return;
            }
            
            // 2. 如果正在变化，停止变化
            if (_isFading && _fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _isFading = false;

            }
            
            // 3. 进入保持状态
            _isHolding = true;
            _remainingHoldTime = holdDuration;
            
            // 确保透明度为最高
            if (_currentAlpha != maxAlpha)
            {
                _currentAlpha = maxAlpha;
                SetParticleAlpha(_currentAlpha);
            }
            
            // 启动保持协程
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
            }
            _holdCoroutine = StartCoroutine(HoldCoroutine());
        }

        // 保持最高亮度协程
        private IEnumerator HoldCoroutine()
        { 
            while (_remainingHoldTime > 0)
            {
                yield return null;
                _remainingHoldTime -= Time.deltaTime;
            }
            
            // 保持结束，开始变化
            _isHolding = false;
            _remainingHoldTime = 0f;
            
            StartFadeToTransparent();
        }

        // 开始变化到透明
        private void StartFadeToTransparent()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            
            _isFading = true;
            _isToMinAlpha = true;
            _remainingFadeTime = fadeDuration;
            _fadeCoroutine = StartCoroutine(FadeAlphaCoroutine(_currentAlpha, minAlpha));
        }

        // 透明度变化协程
        private IEnumerator FadeAlphaCoroutine(float startAlpha, float targetAlpha)
        {
            float elapsedTime = 0f;
            float duration = _remainingFadeTime;
            while (elapsedTime < duration)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                _remainingFadeTime -= Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                _currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                SetParticleAlpha(_currentAlpha);
            }
            
            // 确保达到目标值
            _currentAlpha = targetAlpha;
            SetParticleAlpha(_currentAlpha);
            _isFading = false;
            _remainingFadeTime = 0f;
        }

        // 设置粒子系统透明度
        public void SetParticleAlpha(float alpha)
        {
            foreach (var ps in _particleSystems)
            {
                if (ps == null) continue;
                
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
                int count = ps.GetParticles(particles);
                
                for (int i = 0; i < count; i++)
                {
                    Color color = particles[i].startColor;
                    color.a = alpha;
                    particles[i].startColor = color;
                }
                
                ps.SetParticles(particles, count);
                
                var main = ps.main;
                if (main.startColor.mode == ParticleSystemGradientMode.Color)
                {
                    Color startColor = main.startColor.color;
                    startColor.a = alpha;
                    main.startColor = new ParticleSystem.MinMaxGradient(startColor);
                }
            }
        }

        public void FadeToMinAlpha()
        {
            float startAlpha = maxAlpha;
            if (_isFading && _fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                startAlpha = _currentAlpha; // 从当前透明度开始变化
            }
            else
            {
                _currentAlpha = maxAlpha;
                SetParticleAlpha(maxAlpha);
            }
            _isFading = true;
            _isToMinAlpha = true;
            _remainingFadeTime = fadeDuration;
            _fadeCoroutine = StartCoroutine(FadeAlphaCoroutine(startAlpha, minAlpha));
        }

        public void FadeToMaxAlpha()
        {
            float startAlpha = minAlpha;
            if (_isFading && _fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                startAlpha = _currentAlpha; // 从当前透明度开始变化
            }
            else
            {
                _currentAlpha = minAlpha;
                SetParticleAlpha(minAlpha);
            }
            _isFading = true;
            _isToMinAlpha = false;
            _remainingFadeTime = fadeDuration;
            _fadeCoroutine = StartCoroutine(FadeAlphaCoroutine(startAlpha, maxAlpha));
        }

        protected override void OnBeginning() { }
        protected override void OnEnding() { }
        public override float GetBeginningDuration() => fadeDuration;
        public override float GetEndingDuration() => fadeDuration;
    }
}
