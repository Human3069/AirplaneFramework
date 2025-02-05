using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace _KMH_Framework
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentEx : MonoBehaviour
    {
        protected NavMeshAgent thisNavMeshAgent;

        [SerializeField]
        protected UnityEvent onMoveStarted;
        [SerializeField]
        protected UnityEvent onMoveStopped;

        protected bool _isStopped;
        public bool IsStopped
        {
            get
            {
                return _isStopped;
            }
            protected set
            {
                if (_isStopped != value)
                {
                    _isStopped = value;

                    if (value == true)
                    {
                        onMoveStopped.Invoke();
                    }
                    else
                    {
                        onMoveStarted.Invoke();
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            thisNavMeshAgent = this.GetComponent<NavMeshAgent>();
        }

        protected virtual void Update()
        {
            IsStopped = Vector3.Distance(transform.position, thisNavMeshAgent.destination) <= thisNavMeshAgent.stoppingDistance;
        }
    }
}