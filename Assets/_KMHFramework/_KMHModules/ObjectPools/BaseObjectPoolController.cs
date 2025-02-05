using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public abstract class BaseObjectPoolController<PooledObject> : MonoBehaviour where PooledObject : MonoBehaviour
    {
        // private const string LOG_FORMAT = "<color=white><b>[BaseObjectPoolController]</b></color> {0}";

        protected Queue<PooledObject> poolQueue;

        [SerializeField]
        protected int initializeCount;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int releasedCount = 0;
        [ReadOnly]
        [SerializeField]
        protected int returnedCount = 0;

        // Instantiates On Awake()
        protected GameObject releasedParent = null;
        protected GameObject returnedParent = null;

        protected Type TypeOfPool
        {
            get
            {
                return typeof(PooledObject);
            }
        }

        public int CurrentCount
        {
            get
            {
                return poolQueue.Count;
            }
        }

        protected abstract void Initialize();
        /*
        {
            Debug.LogFormat(LOG_FORMAT, "Initialize()");

            string releasedName = "Released " + TypeOfPool.ToString() + "s"; // Released PooledObjects
            string returnedName = "Returned " + TypeOfPool.ToString() + "s"; // Returned PooledObjects

            releasedParent = new GameObject(releasedName);
            returnedParent = new GameObject(returnedName);

            for (int i = 0; i < initializeCount; i++)
            {
                PooledObject initializedInstance = CreateObject();
                poolQueue.Enqueue(initializedInstance);
            }
        }
        */

        protected abstract PooledObject CreateObject();
        /*
        {
            string newName = TypeOfPool.ToString() + "_" + CurrentCount; // PooledObject_0, PooledObject_1, PooledObject_2, ...

            GameObject newObj = new GameObject(newName, TypeOfPool);
            PooledObject newInstance = newObj.GetComponent<PooledObject>();

            newObj.SetActive(false);
            newObj.transform.SetParent(returnedParent.transform);

            return newInstance;
        }
        */

        public abstract PooledObject ReleaseObject();
        /*
        {
            PooledObject releasedInstance;
            if (CurrentCount > 0)
            {
                releasedInstance = poolQueue.Dequeue();
            }
            else
            {
                releasedInstance = CreateObject();
            }

            releasedInstance.transform.SetParent(releasedParent.transform);
            releasedInstance.gameObject.SetActive(true);

            return releasedInstance;
        }
        */

        public abstract PooledObject ReleaseObject(Vector3 _position);
        /*
        {
            PooledObject releasedInstance;
            if (CurrentCount > 0)
            {
                releasedInstance = poolQueue.Dequeue();
            }
            else
            {
                releasedInstance = CreateObject();
            }

            releasedInstance.transform.SetParent(releasedParent.transform);
            releasedInstance.transform.position = _position;
            releasedInstance.gameObject.SetActive(true);

            return releasedInstance;
        }
        */

        public abstract PooledObject ReleaseObject(Vector3 _position, Vector3 _angle);
        /*
        {
            PooledObject releasedInstance;
            if (CurrentCount > 0)
            {
                releasedInstance = poolQueue.Dequeue();
            }
            else
            {
                releasedInstance = CreateObject();
            }

            releasedInstance.transform.SetParent(releasedParent.transform);

            releasedInstance.transform.position = _position;
            releasedInstance.transform.eulerAngles = _angle;

            releasedInstance.gameObject.SetActive(true);

            return releasedInstance;
        }
        */

        public abstract PooledObject ReleaseObject(Vector3 _position, Quaternion _rotation);
        /*
        {
            PooledObject releasedInstance;
            if (CurrentCount > 0)
            {
                releasedInstance = poolQueue.Dequeue();
            }
            else
            {
                releasedInstance = CreateObject();
            }

            releasedInstance.transform.SetParent(releasedParent.transform);

            releasedInstance.transform.position = _position;
            releasedInstance.transform.rotation = _rotation;

            releasedInstance.gameObject.SetActive(true);

            return releasedInstance;
        }
        */

        public abstract void ReturnObject(PooledObject pooled);
        /*
        {
            pooled.gameObject.SetActive(false);
            pooled.transform.SetParent(returnedParent.transform);

            poolQueue.Enqueue(pooled);
        }
        */
    }
}