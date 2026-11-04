using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LetsPiu
{
    public class UIMonoBehaviour : UIBehaviour
    {
        Timer m_Timer;

        public void AddTimer(string timerName, OnTimer cb, float interval)
        {
            if (m_Timer == null)
            {
                m_Timer = new Timer();
            }

            if (m_Timer.FindTimer(timerName))
            {
                return;
            }

            m_Timer.Add(timerName, cb, interval);
        }

        public void AddCountTimer(string timerName, OnTimer cb, float interval, int count)
        {
            if (m_Timer == null)
            {
                m_Timer = new Timer();
            }

            if (m_Timer.FindTimer(timerName))
            {
                return;
            }

            m_Timer.AddCount(timerName, cb, interval, count);
        }

        public bool RemoveTimer(string timerName)
        {
            if (m_Timer == null)
            {
                return false;
            }

            return m_Timer.RemoveTimer(timerName);
        }

        public bool FindTimer(string timerName)
        {
            if (m_Timer == null)
            {
                return false;
            }

            return m_Timer.FindTimer(timerName);
        }

        public void ClearTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.ClearTimer();
            }
        }

        public void UpdateTimer(float time)
        {
            if (m_Timer != null)
            {
                m_Timer.Update(time);
            }
        }
    }
}
