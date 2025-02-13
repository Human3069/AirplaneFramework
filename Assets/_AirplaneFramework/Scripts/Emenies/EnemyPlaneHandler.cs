using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEngine;

namespace AFramework
{
    public enum EnemyPlaneState
    {
        StraightAheading,
        Following,
    }

    public enum AttackState
    {
        NotFiring,
        Firing,
    }

    [RequireComponent(typeof(Rigidbody))]
    public class EnemyPlaneHandler : Damagable
    {
        [Header("=== EnemyPlaneHandler ===")]
        [Space(10)]
        [SerializeField]
        protected Rigidbody targetRigidbody;

        [Space(10)]
        [SerializeField]
        protected float movePower;
        [SerializeField]
        protected float lookAtPower;
        [SerializeField]
        protected float rollStabilizationPower;

        [Space(10)]
        [SerializeField]
        protected float gravityRotationForce;

        [Header("Particles")]
        [SerializeField]
        protected ParticleSystem onDamagedTrail;
        [SerializeField]
        protected ParticleSystem onDeadTrail;
        [SerializeField]
        protected ParticleSystem onDeadExplosion;

        [Space(10)]
        [SerializeField]
        protected float maxAngleToShoot = 5f;
        [SerializeField]
        protected float straightAheadingTime = 20f;
        [SerializeField]
        protected float followingTime = 30f;

        [Space(10)]
        [SerializeField]
        protected EnemyPlaneState planeState;

        [SerializeField]
        protected AttackState _attackState;
        protected AttackState AttackState
        {
            get
            {
                return _attackState;
            }
            set
            {
                if (_attackState != value)
                {
                    _attackState = value;

                    AttackAsync().Forget();
                }
            }
        }

        protected async UniTaskVoid AttackAsync()
        {
            while (AttackState == AttackState.Firing)
            {
                ProjectileType._50cal_Enemy.EnablePool<BulletHandler>(OnBeforeEnablePool);
                void OnBeforeEnablePool(BulletHandler bullet)
                {
                    bullet.Initialize(this.transform, Vector3.zero, colliders);
                }

                await UniTask.WaitForSeconds(0.1f);
            }
        }

        [Space(10)]
        [SerializeField]
        protected Transform indicatorT;
        [SerializeField]
        protected LineRenderer lineRenderer;

        protected bool _isIndicatorActive = false;
        protected bool IsIndicatorActive
        {
            get
            {
                return _isIndicatorActive && IsDead == false;
            }
            set
            {
                if (_isIndicatorActive != value)
                {
                    _isIndicatorActive = value && IsDead == false;

                    indicatorT.gameObject.SetActive(_isIndicatorActive);
                    lineRenderer.enabled = IsIndicatorActive;
                }
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float angleFromPlayer = 180f; // 
        protected Vector3? resultPredictPos;

        protected Collider[] colliders;

        protected override void Awake()
        {
            base.Awake();

            colliders = this.GetComponents<Collider>();

            FlyingRoutine().Forget();
            AttackingRoutine().Forget();
        }

        protected async UniTaskVoid FlyingRoutine()
        {
            await UniTask.WaitForSeconds(Random.Range(0f, 20f));

            while (IsDead == false)
            {
                planeState = EnemyPlaneState.StraightAheading;
                await UniTask.WaitForSeconds(straightAheadingTime);

                planeState = EnemyPlaneState.Following;
                await UniTask.WaitForSeconds(followingTime);
            }
        }

        protected async UniTaskVoid AttackingRoutine()
        {
            while (IsDead == false)
            {
                if (resultPredictPos != null)
                {
                    angleFromPlayer = Vector3Ex.GetAngleFromThreePositions(this.transform.position + this.transform.forward * 1000f, this.transform.position, resultPredictPos.Value);
                    float distance = Vector3.Distance(targetRigidbody.worldCenterOfMass, this._rigidbody.worldCenterOfMass);

                    AttackState = (angleFromPlayer < 5f && distance < 1000f) ? AttackState.Firing : AttackState.NotFiring;
                }
                else
                {
                    AttackState = AttackState.NotFiring;
                }
             
                await UniTask.WaitForSeconds(0.1f);
            }
        }

        protected void Update()
        {
            // indicator
            PredictDataBundle predictBundle = PredictDataBundle.GetPredictData(ProjectileType._50cal_Friendly);
            float distance = Vector3.Distance(targetRigidbody.worldCenterOfMass, this._rigidbody.worldCenterOfMass);
            float? heightAmount = predictBundle.GetPredictedGravityHeightAmount(targetRigidbody.transform.eulerAngles.x, distance);

            float? linearProjectileSpeed = predictBundle.GetLinearProjectileSpeed(distance);
            IsIndicatorActive = linearProjectileSpeed != null;

            if (linearProjectileSpeed != null)
            {
                Vector3 predictPos = Vector3Ex.GetPredictPosition(targetRigidbody.worldCenterOfMass, this._rigidbody.worldCenterOfMass, this._rigidbody.linearVelocity, linearProjectileSpeed.Value);
                resultPredictPos = predictPos + new Vector3(0f, heightAmount.Value, 0f);

                if (heightAmount != null)
                {
                    indicatorT.position = resultPredictPos.Value;
                    lineRenderer.SetPosition(0, this.transform.position);
                    lineRenderer.SetPosition(1, resultPredictPos.Value);
                }
            }
        }

        protected void FixedUpdate()
        {
            _rigidbody.AddForce(this.transform.forward * movePower);

            Vector3 _length = targetRigidbody.worldCenterOfMass - _rigidbody.worldCenterOfMass;
            Vector3 targetPredict = -_rigidbody.worldCenterOfMass + (targetRigidbody.worldCenterOfMass + (targetRigidbody.linearVelocity * _length.magnitude / 1050f));

            // Vector3 playerPredictPos = Vector3Ex.GetPredictPosition(_rigidbody.worldCenterOfMass, targetRigidbody.worldCenterOfMass, targetRigidbody.linearVelocity, _rigidbody.linearVelocity.magnitude);

            if (IsDead == true)
            {
                _rigidbody.AddTorque(Vector3.Cross(-this.transform.forward, new Vector3(0, gravityRotationForce, 0)));
            }
            else
            {
                // Look at target
                if (planeState == EnemyPlaneState.Following)
                {
                    _rigidbody.AddTorque(Vector3.Cross(this.transform.forward, targetPredict.normalized) * lookAtPower);
                }

                // Roll stabilization
                Vector3 rollCorrection = Vector3.Cross(this.transform.up, Vector3.up);
                _rigidbody.AddTorque(rollCorrection * rollStabilizationPower);
            }
        }


        protected override void OnDamagedOneShot()
        {
            onDamagedTrail.Play();
        }

        protected override void OnDead()
        {
            onDamagedTrail.Stop();
            onDeadExplosion.Play();
            onDeadTrail.Play();

            indicatorT.gameObject.SetActive(false);

            BlendableSoundManager.Instance.PlaySound(SoundType.EnemyAirplaneExplosion, this.transform.position);
        }

        protected void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject.isStatic == true)
            {
                Destroy(this.gameObject);
            }
        }
    }
}