using UnityEngine;

namespace _KMH_Framework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T tInstance = FindFirstObjectByType(typeof(T), FindObjectsInactive.Include) as T;
                    if (tInstance == null)
                    {
                        GameObject newObj = new GameObject(typeof(T).Name);
                        tInstance = newObj.AddComponent<T>();
                    }

                    _instance = tInstance;
                }

                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }
    }
}