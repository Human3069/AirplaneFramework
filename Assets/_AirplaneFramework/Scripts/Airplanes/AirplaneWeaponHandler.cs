using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplaneWeaponHandler
    {
        [SerializeField]
        [SerializedDictionary("MouseButton", "Weapon")]
        protected SerializedDictionary<int, Weapon> weaponDic = new SerializedDictionary<int, Weapon>();

        protected AirplaneHandler _handler;
        protected float currentRebound;

        public float GetCurrentRebound()
        {
            return currentRebound;
        }

        public void OnAwake(AirplaneHandler handler)
        {
            this._handler = handler;
            foreach (KeyValuePair<int, Weapon> pair in weaponDic)
            {
                pair.Value.AssignHandler(_handler);
#if UNITY_EDITOR
                pair.Value.DoAssertion();
#endif
            }
        }

        public float UpdateAndGetWeaponReboundAmount()
        {
            currentRebound = 0f;
            foreach (KeyValuePair<int, Weapon> pair in weaponDic)
            {
                bool isFiring = Input.GetMouseButton(pair.Key);

                pair.Value.IsFiring = isFiring;
                if (isFiring == true)
                {
                    currentRebound += pair.Value.ReboundAmount;
                }
            }

            return currentRebound;
        }

        [System.Serializable]
        public class Weapon
        {
            [SerializeField]
            protected ProjectileType projectileType;
            [SerializeField]
            protected Transform[] fireTransforms;

            [Space(10)]
            [SerializeField]
            protected bool followVelocity;
            [SerializeField]
            protected float fireRate;
            [SerializeField]
            protected float _reboundAmount;
            public float ReboundAmount
            {
                get
                {
                    return _reboundAmount;
                }
            }

            [Header("Particles")]
            [SerializeField]
            protected ParticleSystem[] muzzleFlashParticles;

            [Header("Sounds")]
            [SerializeField]
            protected AudioSource audioSource;
            [SerializeField]
            protected AudioClip[] fireAudioClips;

            protected bool _isFiring = false;
            public bool IsFiring
            {
                get
                {
                    return _isFiring;
                }
                set
                {
                    if (_isFiring != value)
                    {
                        _isFiring = value;

                        if (value == true && fireWeaponAwator.IsCompleted == true)
                        {
                            fireWeaponAwator = FireWeaponAsync().GetAwaiter();
                        }
                    }
                }
            }

            protected UniTask.Awaiter fireWeaponAwator;

            protected Collider[] ignoreColliders;
            protected AirplaneHandler _handler;

            public void AssignHandler(AirplaneHandler handler)
            {
                _handler = handler;
                ignoreColliders = handler.GetComponents<Collider>();
            }

            protected async UniTask FireWeaponAsync()
            {
                while (IsFiring == true)
                {
                    for (int i = 0; i < fireTransforms.Length; i++)
                    {
                        projectileType.EnablePool<BulletHandler>(BeforeEnableAction);
                        void BeforeEnableAction(BulletHandler bullet)
                        {
                            Vector3 additionalVelocity = Vector3.zero;
                            if (followVelocity == true)
                            {
                                additionalVelocity = _handler._Rigidbody.linearVelocity;
                            }

                            bullet.Initialize(fireTransforms[i], additionalVelocity, ignoreColliders);
                        }

                        int audioIndex = Random.Range(0, fireAudioClips.Length);
                        audioSource.PlayOneShot(fireAudioClips[audioIndex]);
                        muzzleFlashParticles[i].Play();

                        await UniTask.WaitForSeconds(fireRate);
                    }
                }
            }

#if UNITY_EDITOR
            public void DoAssertion()
            {
                Debug.Assert(fireTransforms.Length != 0);
                Debug.Assert(audioSource != null);
                Debug.Assert(fireAudioClips.Length != 0);
            }
#endif
        }
    }
}