using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandParticleSystem
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class ParticleMove : MonoBehaviour
    {
        public MoveBase moveInformation;//移动信息
        public GameObject moveObj;//移动的对象
        public bool useMove;//开启移动
        public bool loop;//循环播放

        private MoveBase initedInformation;//上一次初始化用的配置
        [System.NonSerialized]
        public bool paused;//是否暂停（运行时状态，不存进场景）
        private Vector3 startPosition;//这一轮开始时的位置
        private bool hasStartPosition;//是否已经记录过起点

        private void OnEnable()
        {
            //组件被启用（勾上「粒子移动」）时重新初始化，并重新记录起点
            hasStartPosition = false;
            Init();
        }
        private void Start()
        {
            hideFlags = HideFlags.HideInInspector;
            Init();
        }
        private void Update()
        {
            //编辑模式下 FixedUpdate 不会执行，所以用 Update 驱动，Play 模式同样有效
            //没勾选「粒子移动」时不要动对象
            if (!useMove || moveInformation == null || moveObj == null)
            {
                return;
            }
            //换了配置（或第一次挂上配置）时重新初始化，编辑模式下也能直接看到位移
            if (initedInformation != moveInformation)
            {
                Init();
            }
            if (paused)
            {
                return;
            }
            //循环：播完自动回到起点重来
            if (loop && moveInformation.IsFinished)
            {
                Restart();
            }
            moveInformation.MoveBehaviour(moveObj);
        }
        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                Init();
            }
        }
        /// <summary>
        /// 当前是否暂停
        /// </summary>
        public bool Paused
        {
            get { return paused; }
        }
        /// <summary>
        /// 继续播放
        /// </summary>
        public void Play()
        {
            paused = false;
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            paused = true;
        }
        /// <summary>
        /// 回到起点并从头播放
        /// </summary>
        public void Restart()
        {
            paused = false;
            if (moveObj != null && hasStartPosition)
            {
                moveObj.transform.localPosition = startPosition;
            }
            Init();
        }
        /// <summary>
        /// 初始化移动配置，让位移从头开始
        /// </summary>
        public void Init()
        {
            if (moveInformation == null)
            {
                initedInformation = null;
                return;
            }
            //第一次初始化时记下起点，重播时用它回到原位
            if (moveObj != null && !hasStartPosition)
            {
                startPosition = moveObj.transform.localPosition;
                hasStartPosition = true;
            }
            moveInformation.StartValue();
            initedInformation = moveInformation;
        }
    }

}
