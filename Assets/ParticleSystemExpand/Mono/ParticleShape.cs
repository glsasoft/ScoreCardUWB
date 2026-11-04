using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2018
[ExecuteInEditMode]
#else
    [ExecuteAlways]
#endif
public class ParticleShape : MonoBehaviour
{
    public enum ScaleType
    {
        Radius,
        Scale,
    }
    public AnimationCurve X_size = new AnimationCurve();//X 轴缩放曲线
    public AnimationCurve Y_size = new AnimationCurve();//Y 轴缩放曲线
    public AnimationCurve Z_size = new AnimationCurve();//Z 轴缩放曲线
    public AnimationCurve Radius_size = new AnimationCurve();//半径曲线
    public ScaleType scaleType;
    public bool useShape;//使用他
    private ParticleSystem particle;//自己
    private static Vector2 zeroOne = new Vector2(0,1);
    private Vector2 MaxTime;
    // 第一帧之前调用
    void Start()
    {
        hideFlags = HideFlags.HideInInspector;
#if UNITY_2018
        particle = GetComponent<ParticleSystem>();
#else
    TryGetComponent(out particle);
#endif
    MaxTime = new Vector2(0, particle.main.duration);
    }
    void Update()
    {
        //没勾选「发射器形状更改」时不要碰 shape，否则会把 radius 锁死
        if (!useShape)
        {
            return;
        }
        //因为在编辑时有可能更改所以不能放到Start
        if(particle.main.duration !=  MaxTime.y)
        {
            MaxTime = new Vector2(0,particle.main.duration);
        }
        float remapTime = Remap(particle.time, MaxTime, zeroOne);
        ParticleSystem.ShapeModule shape = particle.shape;

        switch (scaleType)
        {
            case ScaleType.Radius:
                {
                    shape.radius = Radius_size.Evaluate(remapTime);
                    break;
                }
            case ScaleType.Scale:
                {
                    shape.scale = new Vector3(X_size.Evaluate(remapTime), Y_size.Evaluate(remapTime), Z_size.Evaluate(remapTime));
                    break;
                }
        }

        //总生命
    }
    private float Remap(float inValue,Vector2 InMinMax, Vector2 outMinMax)
    {
        float outValue = outMinMax.x + (inValue - InMinMax.x) * (outMinMax.y - outMinMax.x) / (InMinMax.y - InMinMax.x);
        return outValue;
    }
    public void ResetShape()
    {
        ParticleSystem.ShapeModule shape = particle.shape;
        shape.scale = Vector3.one;
    }
}
