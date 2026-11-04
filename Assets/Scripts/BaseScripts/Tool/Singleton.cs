using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class SingletonBase<T> where T : new()
    {
        private static T instance;

        public static T S
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }
}
