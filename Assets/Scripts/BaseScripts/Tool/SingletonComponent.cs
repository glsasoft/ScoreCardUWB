using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// 单例组件，全局唯一
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    T[] managers = GameObject.FindObjectsOfType(typeof(T)) as T[];
                    if (managers != null)
                    {
                        if (managers.Length == 1)
                        {
                            instance = managers[0];
                            GameObject.DontDestroyOnLoad(instance.gameObject);
                            return instance;
                        }
                        else if (managers.Length > 1)
                        {
                            Debug.LogError("You have more than one " +
                              typeof(T).Name + "in the Scene. You only need one - it's a singleton!");
                            for (int i = 0; i < managers.Length; ++i)
                            {
                                T manager = managers[i];
                                Destroy(manager.gameObject);
                            }
                        }
                    }
                    GameObject go = new GameObject(typeof(T).Name + "_Singleton", typeof(T));
                    instance = go.GetComponent<T>();
                    GameObject.DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }
    }
}
