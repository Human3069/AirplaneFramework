using _KMH_Framework;
using AFramework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    [ExecuteAlways]
    public class PredictableShooter : MonoBehaviour
    {
        [SerializeField]
        protected ProjectileType type;
        [SerializeField]
        protected Transform shootPoint;
        [SerializeField]
        protected Transform targetPoint;

        [Space(10)]
        [SerializeField]
        protected float maxDistance = 1000;

        [Space(10)]
        [SerializeField]
        protected List<PredictableData> predictableDataList = new List<PredictableData>();

        protected Vector3 straightPredictPoint;

        [ReadOnly]
        [SerializeField]
        protected bool isFiring = false;
        protected float timer = 0f;

        [Space(10)]
        [SerializeField]
        protected int iterateCount = 20;
        [SerializeField]
        protected int iterateMeter = 150;

        protected void Awake()
        {
            if (Application.isPlaying == true)
            {
                gizmoPosList.Clear();
            }
        }

        protected void Update()
        {
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, maxDistance) == true)
            {
                straightPredictPoint = hit.point;
                Debug.DrawLine(shootPoint.position, hit.point, Color.red);
            }
            else
            {
                straightPredictPoint = shootPoint.position + shootPoint.forward * maxDistance;
                Debug.DrawLine(shootPoint.position, straightPredictPoint, Color.green);
            }
            

            if (Application.isPlaying == true)
            {
                // if (Input.GetMouseButtonDown(0) == true && isFiring == false)
                // {
                //     Shoot();
                // }

                if (Input.GetMouseButtonDown(1) == true && isFiring == false)
                {
                    StartShootIteration().Forget();
                }

                if (isFiring == true)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer = 0f;
                }
            }
        }

        protected async UniTaskVoid StartShootIteration()
        {
            for (int i = 0; i < iterateCount; i++)
            {
                if (i == 0)
                {
                    float startVelocity = type.PeekObj().GetComponent<BulletHandler>().Velocity;
                    predictableDataList.Add(new PredictableData(0f, 0f, startVelocity, 0f));
                }
                else
                {
                    float currentMeter = i * iterateMeter;
                    targetPoint.transform.localPosition = new Vector3(0f, 0f, currentMeter);

                    Shoot();
                    await UniTask.WaitUntil(() => isFiring == false);
                }

                await UniTask.Yield();
            }
        }

        protected void Shoot()
        {
            isFiring = true;

            BulletHandler shootedBullet = type.EnablePool<BulletHandler>(OnBeforeShoot);
            void OnBeforeShoot(BulletHandler shootedBullet)
            {
                shootedBullet.Initialize(this.transform, Vector3.zero);
                shootedBullet.OnHitAction = OnShootedBulletHit;

                void OnShootedBulletHit(BulletHandler bullet, RaycastHit hit)
                {
                    gizmoPosList.Add(hit.point);

                    float distance = (straightPredictPoint - shootPoint.position).magnitude;
                    float additionalHeight = Mathf.Abs((hit.point - straightPredictPoint).magnitude);
                    float speedOnHit = bullet.GetComponent<Rigidbody>().linearVelocity.magnitude;

                    float floatTime = timer;

                    Debug.Log("additionalHeight : " + additionalHeight + ", spdOnHit : " + speedOnHit + ", distance : " + distance + ", floatTime : " + floatTime);
                    Debug.DrawLine(straightPredictPoint, hit.point, Color.blue, 10f);

                    PredictableData predictableData = new PredictableData(distance, floatTime, speedOnHit, additionalHeight);
                    predictableDataList.Add(predictableData);

                    isFiring = false;
                }
            }
        }

        protected List<Vector3> gizmoPosList = new List<Vector3>();

        protected void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            foreach (Vector3 gizmoPos in gizmoPosList)
            {
                Gizmos.DrawSphere(gizmoPos, 0.1f);
            }
        }
    }
}