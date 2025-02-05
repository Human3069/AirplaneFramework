using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEngine;

namespace AFramework
{
    public class PivotGunUnitHandler : Damagable
    {
        [Header("=== PivotGunUnitHandler ===")]
        [SerializeField]
        protected float verticalMin;
        [SerializeField]
        protected float verticalMax;

        [Space(10)]
        [SerializeField]
        protected float maxAngleDelta;

        [Space(10)]
        [SerializeField]
        protected int maxMagCount;
        [SerializeField]
        protected float fireRate;
        [SerializeField]
        protected float reloadRate;

        [Space(10)]
        [SerializeField]
        protected float attackRange;

        [Space(10)]
        [SerializeField]
        protected Transform horizontalBaseT;
        [SerializeField]
        protected Transform verticalBaseT;

        [Space(10)]
        [SerializeField]
        protected Transform firePos;

        [Space(10)]
        [SerializeField]
        protected Transform targetT;

        protected Vector3 horizontalPos;
        protected bool isOutRanged = true;

        protected Rigidbody targetRigidbody;
        protected float bulletVelocity = 0f;

        [Header("Particles")]
        [SerializeField]
        protected ParticleSystem onDeadTrailParticle;
        [SerializeField]
        protected ParticleSystem onDeadExplosionParticle;

        protected Collider[] colliders;

        protected override void Awake()
        {
            base.Awake();
            targetRigidbody = targetT.GetComponent<Rigidbody>();
            colliders = this.GetComponents<Collider>();

            AwakeAsync().Forget();
        }

        protected async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitWhile(() => ObjectPoolManager.Instance == null);

            GameObject bulletObj = ProjectileType._50cal_Enemy.PeekObj();
            BulletHandler bulletHandler = bulletObj.GetComponent<BulletHandler>();
            bulletVelocity = bulletHandler.Velocity;

            await UniTask.WaitForSeconds(Random.Range(0f, 5f));

            while (IsDead == false)
            {
                for (int i = 0; i < maxMagCount; i++)
                {
                    if (isOutRanged == false)
                    {
                        ProjectileType._50cal_Enemy.EnablePool<BulletHandler>(BeforeEnableAction);
                        void BeforeEnableAction(BulletHandler bullet)
                        {
                            bullet.Initialize(firePos, Vector3.zero, colliders);
                        }
                    }

                    await UniTask.WaitForSeconds(fireRate);
                }

                await UniTask.WaitForSeconds(reloadRate);
            }
        }

        protected void FixedUpdate()
        {
            Vector3 targetPredictPos = Vector3Ex.GetPredictPosition(this.transform.position, targetT.position, targetRigidbody.linearVelocity, bulletVelocity);

            float magnitude = (this.transform.position - targetPredictPos).magnitude;
            horizontalPos = new Vector3(targetPredictPos.x, this.transform.position.y, targetPredictPos.z);

            Vector3 hNormalized = (horizontalPos - this.transform.position).normalized;
            Vector3 normalized = (targetPredictPos - this.transform.position).normalized;

            float currentHAngle = horizontalBaseT.localEulerAngles.y;
            float currentVAngle = verticalBaseT.localEulerAngles.x;

            float targetHAngle = Vector3.SignedAngle(this.transform.forward, hNormalized, this.transform.up);
            float targetVAngle = Vector3.SignedAngle(horizontalBaseT.forward, normalized, horizontalBaseT.right);

            isOutRanged = (targetVAngle <= -verticalMax) || (targetVAngle >= -verticalMin) || (magnitude >= attackRange);
            targetVAngle = Mathf.Clamp(targetVAngle, -verticalMax, -verticalMin);

            horizontalBaseT.localEulerAngles = new Vector3(0f, Mathf.MoveTowardsAngle(currentHAngle, targetHAngle, maxAngleDelta), 0f);
            verticalBaseT.localEulerAngles = new Vector3(Mathf.MoveTowardsAngle(currentVAngle, targetVAngle, maxAngleDelta), 0f, 0f);
        }

        protected override void OnDamagedOneShot()
        {
           
        }

        protected override void OnDead()
        {
            ImpactType._105mm_Explosion.EnablePool(OnBeforeEnableAction);
            void OnBeforeEnableAction(GameObject poolObj)
            {
                poolObj.transform.position = this.transform.position;
                poolObj.transform.rotation = this.transform.rotation;
            }

            this.gameObject.name += "_Dead";
            this.enabled = false;

            onDeadTrailParticle.Play();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying == true)
            {
                if (IsDead == false)
                {
                    if (isOutRanged == true)
                    {
                        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
                    }

                    Gizmos.DrawWireSphere(this.transform.position, attackRange);

                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

                    Vector3 targetPredictPos = Vector3Ex.GetPredictPosition(this.transform.position, targetT.position, targetRigidbody.linearVelocity, bulletVelocity);
                    Gizmos.DrawSphere(targetPredictPos, 10f);
                }
            }
        }
    }
}