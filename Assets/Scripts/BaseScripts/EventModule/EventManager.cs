using LetsPiu.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class EventManager
    {
        private static EventManager instance;

        public static EventManager Instance
        {
            get
            {
                instance ??= new EventManager();
                return instance;
            }
        }

        /// <summary>
        /// key = EventMsg,用int是减少装箱拆箱压力
        /// 不支持未知参数数量的委托注册，否则在发送事件时，Delegate的DynamicInvoke方法参数存在装箱拆箱行为。
        /// </summary>
        private Dictionary<int, List<Delegate>> m_dicListener = new();
        private Dictionary<int, List<int>> m_dicListenerOrder = new();

        public void AddEvent(EventMsg msgId, Action action, int order = 0) => AddEvent(msgId, (Delegate)action, order);
        public void AddEvent<T>(EventMsg msgId, Action<T> action, int order = 0) => AddEvent(msgId, (Delegate)action, order);
        public void AddEvent<T1, T2>(EventMsg msgId, Action<T1, T2> action, int order = 0) => AddEvent(msgId, (Delegate)action, order);
        public void AddEvent<T1, T2, T3>(EventMsg msgId, Action<T1, T2, T3> action, int order = 0) => AddEvent(msgId, (Delegate)action, order);
        public void AddEvent<T1, T2, T3, T4>(EventMsg msgId, Action<T1, T2, T3, T4> action, int order = 0) => AddEvent(msgId, (Delegate)action, order);

        public void RemoveEvent(EventMsg msgId, Action action) => RemoveEvent(msgId, (Delegate)action);
        public void RemoveEvent<T>(EventMsg msgId, Action<T> action) => RemoveEvent(msgId, (Delegate)action);
        public void RemoveEvent<T1, T2>(EventMsg msgId, Action<T1, T2> action) => RemoveEvent(msgId, (Delegate)action);
        public void RemoveEvent<T1, T2, T3>(EventMsg msgId, Action<T1, T2, T3> action) => RemoveEvent(msgId, (Delegate)action);
        public void RemoveEvent<T1, T2, T3, T4>(EventMsg msgId, Action<T1, T2, T3, T4> action) => RemoveEvent(msgId, (Delegate)action);

        public void Broadcast(EventMsg msgId)
        {
            var intId = (int)msgId;

            if (m_dicListener.TryGetValue(intId, out var m_TempList) && m_TempList != null)
            {
                var cnt = m_TempList.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var data = m_TempList.Get(i);
                    if (data == null)
                    {
                        continue;
                    }
                    var action = data as Action;
                    if (action == null)
                    {
                        Debug.LogErrorFormat($"{msgId} cannot send success!!!");
                        continue;
                    }
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void Broadcast<T>(EventMsg msgId, T t)
        {
            var intId = (int)msgId;

            if (m_dicListener.TryGetValue(intId, out var m_TempList) && m_TempList != null)
            {
                var cnt = m_TempList.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var data = m_TempList.Get(i);
                    if (data == null)
                    {
                        continue;
                    }
                    var action = data as Action<T>;
                    if (action is null)
                    {
                        Debug.LogErrorFormat($"{msgId} cannot send success!!!,it need one Param {typeof(T)}");
                        continue;
                    }
                    try
                    {
                        action.Invoke(t);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void Broadcast<T1, T2>(EventMsg msgId, T1 t1, T2 t2)
        {
            var intId = (int)msgId;

            if (m_dicListener.TryGetValue(intId, out var m_TempList) && m_TempList != null)
            {
                var cnt = m_TempList.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var data = m_TempList.Get(i);
                    if (data == null)
                    {
                        continue;
                    }
                    var action = data as Action<T1, T2>;
                    if (action == null)
                    {
                        Debug.LogErrorFormat($"{msgId} cannot send success!!!,it need two Param {typeof(T1)},{typeof(T2)}");
                        continue;
                    }
                    try
                    {
                        action.Invoke(t1, t2);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void Broadcast<T1, T2, T3>(EventMsg msgId, T1 t1, T2 t2, T3 t3)
        {
            var intId = (int)msgId;

            if (m_dicListener.TryGetValue(intId, out var m_TempList) && m_TempList != null)
            {
                var cnt = m_TempList.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var data = m_TempList.Get(i);
                    if (data == null)
                    {
                        continue;
                    }
                    var action = data as Action<T1, T2, T3>;
                    if (action == null)
                    {
                        Debug.LogErrorFormat($"{msgId} cannot send success!!!,it need one Param {typeof(T1)},{typeof(T2)},{typeof(T3)}");
                        continue;
                    }
                    try
                    {
                        action.Invoke(t1, t2, t3);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void Broadcast<T1, T2, T3, T4>(EventMsg msgId, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            var intId = (int)msgId;

            if (m_dicListener.TryGetValue(intId, out var m_TempList) && m_TempList != null)
            {
                var cnt = m_TempList.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var data = m_TempList.Get(i);
                    if (data == null)
                    {
                        continue;
                    }
                    var action = data as Action<T1, T2, T3, T4>;
                    if (action == null)
                    {
                        Debug.LogErrorFormat($"{msgId} cannot send success!!!,it need one Param {typeof(T1)},{typeof(T2)},{typeof(T3)},{typeof(T4)}");
                        continue;
                    }
                    try
                    {
                        action.Invoke(t1, t2, t3, t4);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void AddEvent(EventMsg msgId, Delegate action, int order = 0)
        {
            var intId = (int)msgId;

            if (!m_dicListener.TryGetValue(intId, out var m_TempList))
            {
                m_TempList = new List<Delegate>();
                m_dicListener.Add(intId, m_TempList);
                m_dicListenerOrder.Add(intId, new List<int>());
            }

            if (!m_TempList.Contains(action))
            {
                int index = 0;
                var orderList = m_dicListenerOrder[intId];
                for (int i = orderList.Count - 1; i >= 0; i--)
                {
                    if (order >= orderList[i])
                    {
                        index = i + 1;
                        break;
                    }
                }
                orderList.Insert(index, order);
                m_TempList.Insert(index, action);
            }
        }

        public void RemoveEvent(EventMsg msgId, Delegate action)
        {
            var intId = (int)msgId;

            if (!m_dicListener.TryGetValue(intId, out var m_TempList))
            {
                return;
            }

            int index = m_TempList.IndexOf(action);
            if (index >= 0)
            {
                m_TempList.RemoveAt(index);
                m_dicListenerOrder[intId].RemoveAt(index);
            }

            if (m_TempList.Count <= 0)
            {
                m_dicListener.Remove(intId);
                m_dicListenerOrder.Remove(intId);
            }
        }
    }
}
