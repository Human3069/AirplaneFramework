using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public abstract class BaseObjectPoolManager<BaseHandler> : MonoBehaviour where BaseHandler : BaseObjectPoolHandler
    {
        // private const string LOG_FORMAT = "<color=white><b>[BaseObjectPoolManager]</b></color> {0}";

        protected static BaseObjectPoolManager<BaseHandler> _instance;
        public static BaseObjectPoolManager<BaseHandler> Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        public Dictionary<string, BaseHandler> PoolHandlerDictionary = new Dictionary<string, BaseHandler>();
        public InitInfo[] _InitInfos;

        protected abstract void Awake();
        /*
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            for (int i = 0; i < _InitInfos.Length; i++)
            {
                GameObject newObj = new GameObject(_InitInfos[i].Title + "PoolHandler");
                newObj.transform.SetParent(this.transform);

                BaseObjectPoolHandler newHandler = newObj.AddComponent<BaseObjectPoolHandler>();
                newHandler.Initialize();
                newHandler.ThisInitIndex = i;

                PoolHandlerDictionary.Add(_InitInfos[i].Title, newHandler);
            }
        }
        */
    }

    [System.Serializable]
    public struct InitInfo
    {
        public string Title;

        [Space(10)]
        public GameObject Obj;
        public int ObjInitCount;
    }

    public abstract class BaseObjectPoolHandler : MonoBehaviour
    {
        // private const string LOG_FORMAT = "<color=white><b>[BaseObjectPoolHandler]</b></color> {0}";

        [ReadOnly]
        public int ThisInitIndex;
        protected Queue<GameObject> poolingQueue = new Queue<GameObject>();

        protected GameObject enableObjectsParent;
        protected GameObject disableObjectsParent;

        public abstract void Initialize();
        /*
        {
            enableObjectsParent = new GameObject("Enables");
            enableObjectsParent.transform.SetParent(this.transform);

            disableObjectsParent = new GameObject("Disables");
            disableObjectsParent.transform.SetParent(this.transform);

            for (int i = 0; i < BaseObjectPoolHandler.Instance._InitInfos[ThisInitIndex].ObjInitCount; i++)
            {
                poolingQueue.Enqueue(CreateNewObject());
            }
        }
        */

        protected abstract GameObject CreateNewObject();
        /*
        {
            GameObject newObj = Instantiate(BaseObjectPoolHandler.Instance._InitInfos[ThisInitIndex].Obj);
            newObj.gameObject.SetActive(false);
            newObj.transform.SetParent(disableObjectsParent.transform);
            return newObj;
        }
        */

        public abstract GameObject EnableObject();
        /*
        {
            if (poolingQueue.Count > 0)
            {
                GameObject obj = poolingQueue.Dequeue();
                obj.transform.SetParent(enableObjectsParent.transform);
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject();
                newObj.gameObject.SetActive(true);
                newObj.transform.SetParent(enableObjectsParent.transform);
                return newObj;
            }
        }
        */

        public abstract GameObject EnableObject(Transform _transform);
        /*
        {
            if (poolingQueue.Count > 0)
            {
                GameObject obj = poolingQueue.Dequeue();

                obj.transform.SetPositionAndRotation(_transform.position, _transform.rotation);

                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject();

                newObj.transform.SetPositionAndRotation(_transform.position, _transform.rotation);

                newObj.gameObject.SetActive(true);
                newObj.transform.SetParent(enableObjectsParent.transform);
                return newObj;
            }
        }
        */

        public abstract GameObject EnableObject(Vector3 _position, Quaternion _rotation);
        /*
        {
            if (poolingQueue.Count > 0)
            {
                GameObject obj = poolingQueue.Dequeue();

                obj.transform.SetPositionAndRotation(_position, _rotation);

                obj.transform.SetParent(enableObjectsParent.transform);
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject();

                newObj.transform.SetPositionAndRotation(_position, _rotation);

                newObj.gameObject.SetActive(true);
                newObj.transform.SetParent(enableObjectsParent.transform);
                return newObj;
            }
        }
        */

        public abstract void ReturnObject(GameObject pool);
        /*
        {
            pool.gameObject.SetActive(false);
            pool.transform.SetParent(disableObjectsParent.transform);
            poolingQueue.Enqueue(pool);
        }
        */
    }
}