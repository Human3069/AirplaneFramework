using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEditor;
using UnityEngine;

namespace AFramework
{
    public enum EnemyPlaneState
    {
        StraightAheading,
        Following,
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

        [Space(10)]
        [SerializeField]
        protected Transform indicatorT;
        [SerializeField]
        protected LineRenderer lineRenderer;

        protected override void Awake()
        {
            base.Awake();
            AwakeAsync().Forget();
        }


        protected async UniTaskVoid AwakeAsync()
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

        protected void Update()
        {
            // indicator
            PredictDataBundle predictBundle = PredictDataBundle.GetPredictData(ProjectileType._50cal_Friendly);
            float distance = Vector3.Distance(targetRigidbody.worldCenterOfMass, this._rigidbody.worldCenterOfMass);
            float? heightAmount = predictBundle.GetPredictedGravityHeightAmount(targetRigidbody.transform.eulerAngles.x, distance);

            float? linearProjectileSpeed = predictBundle.GetLinearProjectileSpeed(distance);

            if (linearProjectileSpeed != null)
            {
                lineRenderer.enabled = true;
                Vector3 predictPos = Vector3Ex.GetPredictPosition(targetRigidbody.worldCenterOfMass, this._rigidbody.worldCenterOfMass, this._rigidbody.linearVelocity, linearProjectileSpeed.Value);
                Vector3 resultPredictPos = predictPos + new Vector3(0f, heightAmount.Value, 0f);

                if (heightAmount != null)
                {
                    indicatorT.position = resultPredictPos;
                    lineRenderer.SetPosition(0, this.transform.position);
                    lineRenderer.SetPosition(1, resultPredictPos);
                }
            }
            else
            {
                lineRenderer.enabled = false;
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

        protected void OnDrawGizmos()
        {
            if (EditorApplication.isPlaying == true)
            {
                PredictDataBundle predictBundle = PredictDataBundle.GetPredictData(ProjectileType._50cal_Friendly);
                float distance = Vector3.Distance(targetRigidbody.worldCenterOfMass, this._rigidbody.worldCenterOfMass);
                float? heightAmount = predictBundle.GetPredictedGravityHeightAmount(targetRigidbody.transform.eulerAngles.x, distance);

                float? linearProjectileSpeed = predictBundle.GetLinearProjectileSpeed(distance);

                if (linearProjectileSpeed != null)
                {
                    Vector3 predictPos = Vector3Ex.GetPredictPosition(targetRigidbody.worldCenterOfMass, this._rigidbody.worldCenterOfMass, this._rigidbody.linearVelocity, linearProjectileSpeed.Value);
                    Vector3 resultPredictPos = predictPos + new Vector3(0f, heightAmount.Value, 0f);

                    if (heightAmount != null)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(resultPredictPos, 1f);
                        Gizmos.DrawLine(_rigidbody.worldCenterOfMass, resultPredictPos);
                    }
                }

                Gizmos.color = Color.green;
                Vector3 playerPredictPos = Vector3Ex.GetPredictPosition(_rigidbody.worldCenterOfMass, targetRigidbody.worldCenterOfMass, targetRigidbody.linearVelocity, _rigidbody.linearVelocity.magnitude);
                Gizmos.DrawWireSphere(playerPredictPos, 10f);
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