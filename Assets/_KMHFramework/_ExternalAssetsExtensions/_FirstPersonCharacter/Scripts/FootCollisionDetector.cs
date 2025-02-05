using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class FootCollisionDetector : MonoBehaviour
    {
        [ReadOnly]
        [SerializeField]
        protected bool _isCollisioning;
        public bool IsCollisioning
        {
            get
            {
                return _isCollisioning;
            }
            protected set
            {
                _isCollisioning = value;
            }
        }

        protected List<Collider> collidedList = new List<Collider>();

        protected void OnTriggerEnter(Collider other)
        {
            collidedList.Add(other);

            IsCollisioning = true;
        }

        protected void OnTriggerExit(Collider other)
        {
            collidedList.Remove(other);

            if (collidedList.Count == 0)
            {
                IsCollisioning = false;
            }
        }
    }
}