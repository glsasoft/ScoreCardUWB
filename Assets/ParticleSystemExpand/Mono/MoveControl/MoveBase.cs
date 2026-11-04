using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 移动行为基类
/// </summary>
public abstract class MoveBase : ScriptableObject
{
    [Tooltip("位移时间")]
    public float t;//位移时间
    [Tooltip("距离")]
    public float displacement;//距离
    [Tooltip("起始速度")]
    public float speedStart;//起始速度
    [Tooltip("最终速度")]
    public float finalVelocity;//最终速度
    [Tooltip("加速度")]
    public float accleration;//加速度
    //[HideInInspector]
    public float currentVelocity;//当前速度
    public float currentTime;//当前时间
    /// <summary>
    /// 是否已经播完
    /// </summary>
    public abstract bool IsFinished { get; }
    /// <summary>
    /// 移动行为函数
    /// </summary>
    /// <param name="gameObject">移动的对象</param>
    public abstract void MoveBehaviour(GameObject gameObject );
    /// <summary>
    /// 初始化函数
    /// </summary>
    public abstract void StartValue();
}
