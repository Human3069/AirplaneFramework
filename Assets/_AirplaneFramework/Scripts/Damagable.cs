using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Damagable : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[Damagable]</b></color> {0}";

        protected Rigidbody _rigidbody;

        [Header("=== Damagable ===")]
        [SerializeField]
        protected float health;
        [ReadOnly]
        [SerializeField]
        protected bool _isDead = false;
        public bool IsDead
        {
            get
            {
                return _isDead;
            }
            protected set
            {
                _isDead = value;

                OnDead();
            }
        }

        protected bool isDamagedOneShotCalled = false;

        protected virtual void Awake()
        {
            isDamagedOneShotCalled = false;
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        public virtual void OnDamagaed(float damage)
        {
            if (isDamagedOneShotCalled == false)
            {
                isDamagedOneShotCalled = true;
                OnDamagedOneShot();
            }
            
            if (IsDead == false)
            {
                health -= damage;
                if (health <= 0f)
                {
                    health = 0f;
                    IsDead = true;
                }
            }
        }

        protected abstract void OnDamagedOneShot();

        protected abstract void OnDead();
    }
}