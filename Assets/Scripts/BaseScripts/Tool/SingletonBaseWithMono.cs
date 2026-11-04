using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LetsPiu
{
    public class SingletonBaseWithMono<T> : MonoBehaviour where T : new()
    {
        static T instance;

        public static T S
        {
            get
            {
                return instance;
            }
        }

        protected virtual void Awake()
        {
            instance = this.GetComponent<T>();
        }
    }
}
