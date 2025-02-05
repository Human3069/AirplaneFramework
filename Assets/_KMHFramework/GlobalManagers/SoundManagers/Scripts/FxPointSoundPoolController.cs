using UnityEngine;

namespace _KMH_Framework
{
    public class FxPointSoundPoolController : BaseObjectPoolController<FxPointSound>
    {
        private const string LOG_FORMAT = "<color=white><b>[FxSoundPoolController]</b></color> {0}";

        protected override void Initialize()
        {
            Debug.LogFormat(LOG_FORMAT, "Initialize()");

            string releasedName = "Released " + TypeOfPool.ToString() + "s"; // Released FxPointSounds
            string returnedName = "Returned " + TypeOfPool.ToString() + "s"; // Returned FxPointSounds

            releasedParent = new GameObject(releasedName);
            returnedParent = new GameObject(returnedName);

            for (int i = 0; i < initializeCount; i++)
            {
                FxPointSound initializedInstance = CreateObject();
                poolQueue.Enqueue(initializedInstance);
            }
        }

        protected override FxPointSound CreateObject()
        {
            string newName = TypeOfPool.ToString() + "_" + CurrentCount; // FxPointSound_0, FxPointSound_1, FxPointSound_2, ...

            GameObject newObj = new GameObject(newName, TypeOfPool);
            FxPointSound newInstance = newObj.GetComponent<FxPointSound>();

            newObj.SetActive(false);
            newObj.transform.SetParent(returnedParent.transform);

            return newInstance;
        }

        public override FxPointSound ReleaseObject()
        {
            FxPointSound releasedInstance;
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

        public override FxPointSound ReleaseObject(Vector3 _position)
        {
            FxPointSound releasedInstance;
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

        public override FxPointSound ReleaseObject(Vector3 _position, Vector3 _angle)
        {
            FxPointSound releasedInstance;
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

        public override FxPointSound ReleaseObject(Vector3 _position, Quaternion _rotation)
        {
            FxPointSound releasedInstance;
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

        public override void ReturnObject(FxPointSound pooled)
        {
            pooled.gameObject.SetActive(false);
            pooled.transform.SetParent(returnedParent.transform);

            poolQueue.Enqueue(pooled);
        }
    }
}