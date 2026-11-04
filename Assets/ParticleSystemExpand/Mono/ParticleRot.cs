using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandParticleSystem
{
    [DisallowMultipleComponent]
#if UNITY_2018
    [ExecuteInEditMode]
#else
    [ExecuteAlways]
#endif
    public class ParticleRot : MonoBehaviour
    {
        public enum Direction
        {
            X,
            Y,
            Z,
        }
        public bool useRot;
        public Transform rotObject;//旋转轴
        public float speed;//速度
        public Direction direction;//方向
        [System.NonSerialized]
        public bool paused;//暂停预览（运行时状态，不存进场景）
        private void Start()
        {
            hideFlags = HideFlags.HideInInspector;
        }
        private void Update()
        {
            //编辑模式下 FixedUpdate 不会执行，用 Update 才能实时预览
            //没开启或没指定旋转轴时不转，避免对象被甩飞
            if (!useRot || rotObject == null || paused)
            {
                return;
            }
            transform.RotateAround(rotObject.position, DirectionReturn(), Time.deltaTime * speed);
        }
        private Vector3 DirectionReturn()
        {
            switch (direction)
            {
                case Direction.X:
                    {
                        return rotObject.right;
                    }
                case Direction.Y:
                    {
                        return rotObject.up;
                    }
                case Direction.Z:
                    {
                        return rotObject.forward;
                    }
            }
            return rotObject.right;
        }
    }
}

