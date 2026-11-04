using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    /// <summary>
    /// 粒子碰撞检测：在粒子碰到碰撞体时，于碰撞点激活目标特效。
    /// 挂载到带 Collider 的对象上，由外部粒子系统（需开启 Collision 模块）撞击触发。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ParticleCollisionEffect : MonoBehaviour
    {
        [Tooltip("碰撞时激活的目标特效预制体（可为空，留空则仅输出碰撞信息）")]
        [SerializeField] private GameObject targetEffectPrefab;

        [Tooltip("目标特效是否随碰撞点法线朝向")]
        [SerializeField] private bool alignToNormal = true;

        [Tooltip("特效激活后自动回收的时长（秒），<= 0 表示不自动回收")]
        [SerializeField] private float autoDestroyTime = 3f;

        [Tooltip("同一帧内最多激活的特效数量，避免粒子密集时性能炸裂")]
        [SerializeField] private int maxEffectsPerFrame = 10;

        // 活跃特效回收定时器（Update 统一管理，零协程分配）
        private struct EffectTimer
        {
            public int StartIndex;
            public int Count;
            public float ReturnTime;
        }

        // 复用碰撞事件列表，避免每次回调都产生 GC
        private readonly List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();

        // 对象池：复用已实例化的特效，减少 Instantiate/Destroy 开销
        private readonly List<GameObject> _activeEffectPool = new List<GameObject>();
        private readonly List<GameObject> _inactiveEffectPool = new List<GameObject>();

        // 活跃定时器列表（替换协程，由 Update 统一驱动）
        private readonly List<EffectTimer> _activeTimers = new List<EffectTimer>();
        private EffectTimer _currentTimer;

        // 标记 targetEffectPrefab 是否为场景中已实例化的物体（而非预制体资源）
        private bool _isSceneInstance;

        private void Awake()
        {
            if (targetEffectPrefab == null)
                return;

            // 判断是场景实例还是预制体资源：场景物体的 scene 是有效的，预制体则不是
            if (targetEffectPrefab.scene.IsValid())
            {
                _isSceneInstance = true;
                targetEffectPrefab.SetActive(false);
                _inactiveEffectPool.Add(targetEffectPrefab);
            }
        }

        /// <summary>
        /// 当粒子与碰撞体发生碰撞时由 Unity 自动调用。
        /// 此时脚本挂在 Collider 上，other 为发射粒子的 GameObject。
        /// </summary>
        /// <param name="other">发射粒子的 GameObject</param>
        private void OnParticleCollision(GameObject other)
        {
            if (this.enabled == false)
                return;

            // other 是粒子系统所在对象，从中获取 ParticleSystem
            ParticleSystem ps = other.GetComponent<ParticleSystem>();
            if (ps == null)
                return;

            // 获取本次碰撞的所有碰撞事件（传入本对象作为被碰撞方）
            int eventCount = ps.GetCollisionEvents(gameObject, _collisionEvents);
            if (eventCount <= 0)
                return;

            int spawned = 0;
            int index = -1, startIndex = -1;
            for (int i = 0; i < eventCount; i++)
            {
                // 限制单帧激活数量
                if (spawned >= maxEffectsPerFrame)
                    break;

                ParticleCollisionEvent evt = _collisionEvents[i];
                Vector3 position = evt.intersection;       // 碰撞点位置
                Vector3 normal = evt.normal;               // 碰撞点法线
                index = ActivateEffect(position, normal);
                if (index >= 0)
                {
                    if (startIndex == -1) startIndex = index;
                    spawned++;
                }
            }

            if (autoDestroyTime > 0f && spawned > 0)
            {
                _activeTimers.Add(new EffectTimer
                {
                    StartIndex = startIndex,
                    Count = spawned,
                    ReturnTime = Time.time + autoDestroyTime
                });

                if (_currentTimer.ReturnTime == 0)
                {
                    _currentTimer = _activeTimers[0];
                    _activeTimers.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// 在指定位置激活目标特效。
        /// </summary>
        /// <param name="position">碰撞点世界坐标</param>
        /// <param name="normal">碰撞点法线方向</param>
        private int ActivateEffect(Vector3 position, Vector3 normal)
        {
            if (targetEffectPrefab == null)
            {
                Debug.LogWarning("[ParticleCollisionEffect] 未指定目标特效预制体，跳过激活。");
                return -1;
            }

            // 从对象池取一个可用特效
            GameObject effect = GetFromInactivePool();
            if (effect == null)
                return -1;

            // 设置位置与朝向
            effect.transform.position = position;
            if (alignToNormal && normal.sqrMagnitude > 0.0001f)
            {
                effect.transform.rotation = Quaternion.LookRotation(normal);
            }
            else
            {
                effect.transform.rotation = Quaternion.identity;
            }

            // 播放特效
            effect.SetActive(true);

            _activeEffectPool.Add(effect);

            return _activeEffectPool.Count - 1;
        }

        /// <summary>
        /// 从对象池获取一个特效实例，池为空时新建。
        /// 若 targetEffectPrefab 是场景实例，仅复用该单一实例，不会实例化新对象。
        /// </summary>
        private GameObject GetFromInactivePool()
        {
            GameObject effect = null;
            int count = _inactiveEffectPool.Count;
            while (count > 0 && effect == null)
            {
                effect = _inactiveEffectPool[count - 1];
                _inactiveEffectPool.RemoveAt(count - 1);
                count--;
            }
            if (effect != null)
                return effect;

            // 池中没有可用的，实例化新的
            GameObject newEffect = Instantiate(targetEffectPrefab, transform);
            newEffect.SetActive(false);
            return newEffect;
        }

        /// <summary>
        /// Update 统一驱动所有特效的回收定时器，取代逐个启动协程。
        /// 无论同时存在多少个活跃特效，每帧只执行一次遍历，零 GC 分配。
        /// </summary>
        private void Update()
        {
            if (_currentTimer.ReturnTime == 0)
                return;

            if (Time.time < _currentTimer.ReturnTime)
                return;

            // 当前定时器到期，回收本批特效
            int start = _currentTimer.StartIndex;
            int end = start + _currentTimer.Count;
            for (int i = start; i < end; i++)
            {
                if (_activeEffectPool[i] != null)
                {
                    _activeEffectPool[i].SetActive(false);
                    _inactiveEffectPool.Add(_activeEffectPool[i]);
                }
            }
            _activeEffectPool.RemoveRange(start, _currentTimer.Count);

            // 调整剩余定时器的起始索引
            for (int i = 0; i < _activeTimers.Count; i++)
            {
                EffectTimer timer = _activeTimers[i];
                timer.StartIndex -= _currentTimer.Count;
                _activeTimers[i] = timer;
            }

            // 推进到下一个定时器
            if (_activeTimers.Count > 0)
            {
                _currentTimer = _activeTimers[0];
                _activeTimers.RemoveAt(0);
            }
            else
            {
                _currentTimer = default;
            }
        }

        private void OnDisable()
        {
            // 回收对象池中所有特效
            for (int i = 0; i < _activeEffectPool.Count; i++)
            {
                if (_activeEffectPool[i] != null)
                {
                    _activeEffectPool[i].SetActive(false);
                    _inactiveEffectPool.Add(_activeEffectPool[i]);
                }
            }
            _activeEffectPool.Clear();
            _activeTimers.Clear();
            _currentTimer = default;
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _activeEffectPool.Count; i++)
            {
                if (_activeEffectPool[i] != null)
                {
                    if (_isSceneInstance)
                    {
                        if (_activeEffectPool[i] != targetEffectPrefab) Destroy(_activeEffectPool[i]);
                    }
                    else
                    {
                        Destroy(_activeEffectPool[i]);
                    }
                }
            }
            _activeEffectPool.Clear();

            for (int i = 0; i < _inactiveEffectPool.Count; i++)
            {
                if (_inactiveEffectPool[i] != null)
                {
                    if (_isSceneInstance)
                    {
                        if (_inactiveEffectPool[i] != targetEffectPrefab) Destroy(_inactiveEffectPool[i]);
                    }
                    else
                    {
                        Destroy(_inactiveEffectPool[i]);
                    }
                }
            }
            _inactiveEffectPool.Clear();
        }
    }
}
