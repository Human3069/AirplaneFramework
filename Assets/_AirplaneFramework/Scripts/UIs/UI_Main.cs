using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEngine;

namespace AFramework
{
    public class UI_Main : MonoBehaviour
    {
        [SerializeField]
        protected AirplaneHandler _handler;
        [SerializeField]
        protected Camera mainCamera;

        [Space(10)]
        [SerializeField]
        protected RectTransform bombPredictRect;

        protected void Awake()
        {
            AwakeAsync().Forget();
        }

        protected async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitWhile(() => ObjectPoolManager.Instance == null);

            GameObject bombObj = ProjectileType._50kg_Bomb_Friendly.PeekObj();
            Rigidbody bombRigidbody = bombObj.GetComponent<Rigidbody>();
            BulletHandler bombBullet = bombObj.GetComponent<BulletHandler>();

            while (true)
            {
                Vector3 predictPos = Predictor.PredictWithSingleCollision(bombRigidbody.linearDamping, _handler._Rigidbody.linearVelocity, bombBullet.LayerMask, out _, _handler.transform.position, Physics.gravity, 0.9f, 200, true);
                bombPredictRect.position = mainCamera.WorldToScreenPoint(predictPos);

                await UniTask.Yield();
            }
        }
    }
}