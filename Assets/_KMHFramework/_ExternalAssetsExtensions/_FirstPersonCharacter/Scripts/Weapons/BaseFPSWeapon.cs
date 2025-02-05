using System.Collections;
using UnityEngine;

namespace _KMH_Framework
{
    [RequireComponent(typeof(AudioSource))]
    public class BaseFPSWeapon : MonoBehaviour
    {
        [System.Serializable]
        public struct Rebound
        {
            [SerializeField]
            private float _amount;
            public float Amount
            {
                get
                {
                    return _amount;
                }
            }

            [SerializeField]
            private float _lerpPower;
            public float LerpPower
            {
                get
                {
                    return _lerpPower;
                }
            }

            [Space(10)]
            [ReadOnly]
            [SerializeField]
            private Vector3 _angle;
            public Vector3 Angle
            {
                get
                {
                    return _angle;
                }
                set
                {
                    _angle = value;
                }
            }
        }

        protected AudioSource audioSource;
        protected UI_FPSPlayer uiFPSHandler;
        protected Animator weaponAnimator;

        protected bool _isTriggering;
        public virtual bool IsTriggering
        {
            get
            {
                return _isTriggering;
            }
            set
            {
                if (_isTriggering != value)
                {
                    _isTriggering = value;

                    if (IsReloading == false)
                    {
                        if (value == true)
                        {
                            postOnTriggeringRoutine = StartCoroutine(PostOnTriggeringRoutine());
                        }
                        else
                        {
                            StopCoroutine(postOnTriggeringRoutine);
                        }
                    }
                }
            }
        }

        protected Coroutine postOnTriggeringRoutine;

        [Header("Weapons")]
        [SerializeField]
        protected float fireRate;

        public enum SelectionMode
        {
            SemiAuto = 1,
            _2Burst = 2,
            _3Burst = 3,
            FullAuto = int.MaxValue
        }
        [ReadOnly]
        [SerializeField]
        protected SelectionMode _selectionMode;
        public SelectionMode _SelectionMode
        {
            get
            {
                return _selectionMode;
            }
            set
            {
                _selectionMode = value;
            }
        }

        [Header("Rebounds")]
        [SerializeField]
        protected Rebound _recoverableRebound;
        public Rebound RecoverableRebound
        {
            get
            {
                return _recoverableRebound;
            }
        }
        [SerializeField]
        protected Rebound _nonRecoverableRebound;
        public Rebound NonRecoverableRebound
        {
            get
            {
                return _nonRecoverableRebound;
            }
        }

        protected Coroutine recoverableReboundCoroutine;
        protected Coroutine nonRecoverableReboundCoroutine;

        [Header("Bullet")]
        [ReadOnly]
        [SerializeField]
        protected int _ammoPerMag;
        public int AmmoPerMag
        {
            get
            {
                return _ammoPerMag;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected int _currentMagCount;
        public int CurrentMagCount
        {
            get
            {
                return _currentMagCount;
            }
            protected set
            {
                _currentMagCount = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected int _exceptMagCount;
        public int ExceptMagCount
        {
            get
            {
                return _exceptMagCount;
            }
            protected set
            {
                _exceptMagCount = value;
            }
        }

        [Header("Effects")]
        [SerializeField]
        protected ParticleSystem muzzleFlashParticle;
        [SerializeField]
        protected Light muzzleFlashLight;
        [SerializeField]
        protected AudioClip[] fireSoundClips;
        [SerializeField]
        protected AudioClip reloadSoundClip;

        protected bool _isReloading;
        public bool IsReloading
        {
            get
            {
                return _isReloading;
            }
            protected set
            {
                _isReloading = value;
            }
        }

        protected virtual void Awake()
        {
            muzzleFlashLight.enabled = false;

            audioSource = this.GetComponent<AudioSource>();
            weaponAnimator = this.GetComponent<Animator>();
        }

        public virtual void Initialize(UI_FPSPlayer uiHandler)
        {
            uiFPSHandler = uiHandler;

            weaponAnimator.SetTrigger("_Initialize");
        }

        protected virtual IEnumerator PostOnTriggeringRoutine()
        {
            for (int i = 0; i < (int)_selectionMode; i++)
            {
                if (CurrentMagCount > 0)
                {
                    DoFire();
                }

                yield return new WaitForSeconds(fireRate);
            }
        }

        protected virtual void DoFire()
        {
            muzzleFlashParticle.Play();
            StartCoroutine(LightEffectRoutine());

            int randomed = Random.Range(0, fireSoundClips.Length);
            audioSource.PlayOneShot(fireSoundClips[randomed]);
            weaponAnimator.SetTrigger("_Fire");

            CurrentMagCount--;
            uiFPSHandler.SetAmmoText(ExceptMagCount, CurrentMagCount);

            if (recoverableReboundCoroutine != null)
            {
                StopCoroutine(recoverableReboundCoroutine);
            }
            recoverableReboundCoroutine = StartCoroutine(RecoverableReboundScreenAsync());

            if (nonRecoverableReboundCoroutine != null)
            {
                StopCoroutine(nonRecoverableReboundCoroutine);
            }
            nonRecoverableReboundCoroutine = StartCoroutine(NonRecoverableReboundScreenAsync());
        }

        protected virtual IEnumerator LightEffectRoutine()
        {
            muzzleFlashLight.enabled = true;
            yield return new WaitForSeconds(0.05f);
            muzzleFlashLight.enabled = false;
        }

        public virtual IEnumerator ReloadAsync()
        {
            IsReloading = true;

            weaponAnimator.SetTrigger("_Reload");
            audioSource.PlayOneShot(reloadSoundClip);

            yield return new WaitForSeconds(3.5f);

            ExceptMagCount -= (AmmoPerMag - CurrentMagCount);
            CurrentMagCount = AmmoPerMag;

            uiFPSHandler.SetAmmoText(ExceptMagCount, CurrentMagCount);

            IsReloading = false;
        }

        protected virtual IEnumerator RecoverableReboundScreenAsync()
        {
            _recoverableRebound.Angle += new Vector3(-_recoverableRebound.Amount, 0f, 0f);
            while (_recoverableRebound.Angle != Vector3.zero)
            {
                _recoverableRebound.Angle = Vector3.Lerp(_recoverableRebound.Angle, Vector3.zero, Time.deltaTime * _recoverableRebound.LerpPower);
                yield return null;
            }
        }

        protected virtual IEnumerator NonRecoverableReboundScreenAsync()
        {
            _nonRecoverableRebound.Angle += new Vector3(-_nonRecoverableRebound.Amount, 0f, 0f);
            yield return null;
        }
    }
}