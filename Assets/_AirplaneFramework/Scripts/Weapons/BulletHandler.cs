using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class BulletHandler : MonoBehaviour
    {
        [SerializeField]
        protected ProjectileType projectileType;
        public ProjectileType ProjectileType
        {
            get
            {
                return projectileType;
            }
        }

        [SerializeField]
        protected ImpactType impactType;
        [SerializeField]
        protected LayerMask layerMask;
        public LayerMask LayerMask
        {
            get
            {
                return layerMask;
            }
        }

        [SerializeField]
        protected bool isDrawPredictionRay;

        protected Rigidbody _rigidbody;
        protected TrailRenderer _trailRenderer;

        [SerializeField]
        protected float _damage;
        [SerializeField]
        protected float overlapRadius = 1f;
        public float OverlapRadius
        {
            get
            {
                return overlapRadius;
            }
        }

        [Space(10)]
        [SerializeField]
        protected float velocity;
        public float Velocity
        {
            get
            {
                return velocity;
            }
        }

        [SerializeField]
        protected float lifeTime;

        [Space(10)]
        [SerializeField]
        protected float angleThreshold;

        protected bool isCollided = false;
        protected Vector3 currentPos;

        protected List<Collider> ignoreColliderList = new List<Collider>();

        public System.Action<BulletHandler, RaycastHit> OnHitAction;

        protected void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
            _trailRenderer = this.GetComponentInChildren<TrailRenderer>();
        }

        public void Initialize(Transform fireTransform, Vector3 additionalVelocity, params Collider[] ignoreColliders)
        {
            isCollided = false;
            currentPos = Vector3.zero;

            foreach (Collider ignoreCollider in ignoreColliders)
            {
                ignoreColliderList.Add(ignoreCollider);
            }

            float _x = Random.Range(-1f, 1f);
            float _y = Random.Range(-1f, 1f);
            float _z = Random.Range(-1f, 1f);
            Vector3 randomizedAngle = new Vector3(_x, _y, _z).normalized * angleThreshold;

            this.transform.forward = randomizedAngle + fireTransform.forward;
            this.transform.position = fireTransform.position;

            _rigidbody.linearVelocity = this.transform.forward * this.Velocity + additionalVelocity;
            _trailRenderer.enabled = true;
        }

        protected void OnEnable()
        {
            StartCoroutine(CheckLifetimeAsync());
            CheckRaycastAsync().Forget();
        }

        protected IEnumerator CheckLifetimeAsync()
        {
            yield return new WaitForSeconds(lifeTime);

            if (isCollided == false)
            {
                _trailRenderer.enabled = false;
                this.gameObject.ReturnPool(projectileType);

                OnHitAction = null;
            }
        }

        protected async UniTaskVoid CheckRaycastAsync()
        {
            await UniTask.Yield();

            if (this.gameObject == null)
            {
                return;
            }

            while (this.gameObject.activeInHierarchy == true)
            {
                if (currentPos != Vector3.zero)
                {
                    Vector3 posDelta = this.transform.position - currentPos;

                    if (Physics.Raycast(currentPos, posDelta.normalized, out RaycastHit hit, posDelta.magnitude) == true)
                    {
                        if (ignoreColliderList.Contains(hit.collider) == false && this.gameObject.activeInHierarchy == true)
                        {
                            isCollided = true;

                            impactType.EnablePool(OnBeforeEnableAction);
                            void OnBeforeEnableAction(GameObject poolObj)
                            {
                                poolObj.transform.position = hit.point;
                                poolObj.transform.rotation = this.transform.rotation;
                            }

                            this.gameObject.ReturnPool(projectileType);

                            if (hit.collider.TryGetComponent<Damagable>(out Damagable damagable) == true)
                            {
                                damagable.OnDamagaed(_damage);
                            }
                            else
                            {
                                Collider[] overlapColliders = Physics.OverlapSphere(hit.point, overlapRadius);
                                foreach (Collider overlapCollider in overlapColliders)
                                {
                                    if (overlapCollider.TryGetComponent<Damagable>(out Damagable _damagable) == true)
                                    {
                                        float distanceFromHit = Vector3.Distance(hit.point, overlapCollider.transform.position);
                                        float distanceNormal = distanceFromHit / overlapRadius;
                                        float normalizedDamage = Mathf.Lerp(_damage, 0f, distanceNormal);

                                        _damagable.OnDamagaed(normalizedDamage);
                                    }
                                }
                            }

                            OnHitAction?.Invoke(this, hit);
                            OnHitAction = null;
                        }
                    }
                }
               
                currentPos = this.transform.position;
                
                await UniTask.Yield();
            }
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, overlapRadius);
        }

        protected void OnDrawGizmos()
        {
            if (isDrawPredictionRay == true)
            {
                if (_rigidbody == null)
                {
                    _rigidbody = this.GetComponent<Rigidbody>();
                }

                Vector3 predictPos = Predictor.PredictWithSingleCollision(_rigidbody.linearDamping, _rigidbody.linearVelocity, layerMask, out _, this.transform.position, Physics.gravity, 0.9f, 200, true);
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawSphere(predictPos, overlapRadius);
            } 
        }
    }
}