using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="ArtTools/基础位移")]
public class MoveDefault_SO : MoveBase
{
    public enum MoveType
    {
        Time,
        Distance,
    }
    [Tooltip("是否向左侧移动（勾选后往左，不勾往右）")]
    public bool lift;
    [Tooltip("移动类型")]
    public MoveType moveType;
    private Action<GameObject> MoveFun;//委托
    private float variable_Distance;//变速移动的距离
    private float constant_Distance;//匀速移动的距离
    private float constant_Time;//匀速移动的时间
    /// <summary>
    /// 位移是否已经播完
    /// </summary>
    public override bool IsFinished
    {
        get { return MoveFun == null; }
    }
    public override void StartValue()
    {
        currentVelocity = speedStart;
        currentTime = 0;
        constant_Time = 0;
        variable_Distance = 0;
        constant_Distance = 0;
        TypeSwitch();
    }
    private void TypeSwitch()
    {
        switch (moveType)
        {
            case MoveType.Distance:
                {
                    MoveFun = DistanceType;
                    break;
                }
            case MoveType.Time:
                {
                    MoveFun = TimeType;
                    break;
                }
        }
    }
    public override void MoveBehaviour(GameObject gameObject)
    {
        MoveFun?.Invoke(gameObject);
    }
    /// <summary>
    /// 按时间移动
    /// </summary>
    /// <param name="gameObject">移动的对象</param>
    private void TimeType(GameObject gameObject)
    {
        if (constant_Time >= t)
        {
            MoveFun = null;
            return;
        }
        Behaviour(gameObject);

    }
    /// <summary>
    /// 按距离移动
    /// </summary>
    /// <param name="gameObject">移动的对象</param>
    private void DistanceType(GameObject gameObject)
    {
        if (variable_Distance + constant_Distance >= displacement)
        {
            MoveFun = null;
            return;
        }
        Behaviour(gameObject);


    }
    /// <summary>
    /// 移动的基础逻辑
    /// </summary>
    /// <param name="gameObject">移动的对象</param>
    private void Behaviour(GameObject gameObject)
    {
        if (currentVelocity == finalVelocity)
        {
            constant_Time += Time.deltaTime;
            constant_Distance = finalVelocity * constant_Time;//匀速移动
        }
        else
        {
            currentTime += Time.deltaTime;
            variable_Distance = (speedStart + currentVelocity) / 2 * currentTime;//变速移动
        }

        currentVelocity = speedStart + accleration * currentTime;
        currentVelocity = Mathf.Clamp(currentVelocity, speedStart, finalVelocity);
        //移动
        gameObject.transform.Translate(lift ? Vector3.left : Vector3.right * currentVelocity * Time.deltaTime);
    }

}
