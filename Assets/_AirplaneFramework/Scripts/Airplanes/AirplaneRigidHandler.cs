using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using System.Collections;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplaneRigidHandler
    {
        // private const string LOG_FORMAT = "<color=white><b>[AirplaneRigidHandler]</b></color> {0}";

        [HideInInspector]
        public bool IsEnabled = true;

        [SerializeField]
        protected float _moveSpeed;
        [SerializeField]
        protected float _rotateSpeed;

        [Header("Info")]
        [ReadOnly]
        [SerializeField]
        protected float inputNormal = 0f;
        [SerializeField]
        protected float inputNormalLerpValue;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float actualPower;

        [SerializeField]
        protected float actualPowerLerpValue;
        [SerializeField]
        protected float actualPowerLerpOnAirValue;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float _straightStability;
        public float StraightStability
        {
            get
            {
                return _straightStability;
            }
            protected set
            {
                _straightStability = Mathf.Clamp01(value);
            }
        }
        public float StraightSpread
        {
            get
            {
                return 1f - StraightStability;
            }
        }

        [SerializeField]
        protected float straightStabilitySenstivity;

        [Space(10)]
        [SerializeField]
        protected float headShakePower;

        [Space(10)]
        [SerializeField]
        protected float maxSpeed;
        [SerializeField]
        protected float gravityRotationForce;

        [Space(10)]
        [SerializeField]
        protected WheelCollider rightWheelCollider;
        [SerializeField]
        protected WheelCollider leftWheelCollider;

        protected bool isGrounded
        {
            get
            {
                return (rightWheelCollider.isGrounded == true) && (leftWheelCollider.isGrounded == true);
            }
        }

        [Header("Sounds")]
        [SerializeField]
        protected AudioSource _engineAudioSource;
        protected Coroutine engineSoundCoroutine;

        [SerializeField]
        protected AudioSource _windBlowAudioSource;
        [SerializeField]
        protected AudioSource _shakingAudioSource;

        [Space(10)]
        [SerializeField]
        protected AudioClip engineStartClip;
        [SerializeField]
        protected AudioClip engineIdleClip;
        [SerializeField]
        protected AudioClip engineEndClip;

        [Space(10)]
        [SerializeField]
        protected AudioClip windClip;
        [SerializeField]
        protected AudioClip shakingClip;

        protected AirplaneHandler _handler;
        protected Rigidbody _rigidbody;

        protected bool isEngineOn = false;
   
#if DEBUG
        [Space(10)]
        [SerializeField]
        protected bool DEBUG_isShowOnGUIField;

        protected GUIStyle _guiStyle = new GUIStyle();
#endif

        protected Vector3 _rotationInput;
        protected float _throttleInput;

        public void SetInput(Vector3 rotationInput, float throttleInput)
        {
            this._rotationInput = rotationInput;
            this._throttleInput = throttleInput;
        }
        
        public void OnAwake(AirplaneHandler handler)
        {
            this._handler = handler;
            KeyType.Toggle_Boot.RegisterEventAsync(OnEngineValueChanged).Forget();

            _rigidbody = handler._Rigidbody;
            _windBlowAudioSource.clip = windClip;
            _shakingAudioSource.clip = shakingClip;

#if DEBUG
            if (DEBUG_isShowOnGUIField == true)
            {
                _guiStyle.normal.textColor = Color.red;
                _guiStyle.fontSize = 20;
            }
#endif
        }

        protected void OnEngineValueChanged(bool isEngineOn)
        {
            if (IsEnabled == false)
            {
                return;
            }

            this.isEngineOn = isEngineOn;

            _engineAudioSource.loop = false;
            _engineAudioSource.Stop();

            if (isEngineOn == true)
            {
                _engineAudioSource.clip = engineStartClip;
                _engineAudioSource.Play();

                engineSoundCoroutine = _handler.StartCoroutine(PostOnBootValueChangedTrue());
            }
            else
            {
                _engineAudioSource.clip = engineEndClip;
                _engineAudioSource.Play();

                if (engineSoundCoroutine != null)
                {
                    _handler.StopCoroutine(engineSoundCoroutine);
                }
            }
        }

        protected IEnumerator PostOnBootValueChangedTrue()
        {
            if (IsEnabled == false)
            {
                yield break;
            }

            while (_engineAudioSource.isPlaying == true)
            {
                yield return null;
            }

            _engineAudioSource.loop = true;
            _engineAudioSource.clip = engineIdleClip;
            _engineAudioSource.Play();
        }

        public (float, float) CalculateAndGetResult()
        {
            if (IsEnabled == false)
            {
                return (0f, 0f);
            }

            float mps = _rigidbody.linearVelocity.magnitude;

            if (isEngineOn == true)
            {
                actualPower = Mathf.Lerp(actualPower, _throttleInput, actualPowerLerpValue);
                inputNormal = Mathf.Lerp(inputNormal, 1f, inputNormalLerpValue);
            }
            else
            {
                if (isGrounded == true)
                {
                    actualPower = Mathf.Lerp(actualPower, 0f, actualPowerLerpValue);
                }
                else
                {
                    actualPower = Mathf.Lerp(actualPower, 0.4f, actualPowerLerpOnAirValue);
                }
                inputNormal = Mathf.Lerp(inputNormal, 0f, inputNormalLerpValue);
            }

            StraightStability = 1f;
            if (mps > 1f)
            {
                StraightStability = 1f - ((_handler.transform.forward - _rigidbody.linearVelocity.normalized).magnitude * straightStabilitySenstivity);
            }

            _rigidbody.AddForce(_handler.transform.forward * actualPower * StraightStability * _moveSpeed);
            _rigidbody.AddTorque(_handler.transform.TransformDirection(_rotationInput * inputNormal * StraightStability * mps * _rotateSpeed));
            _rigidbody.AddTorque(Vector3.Cross(-_handler.transform.forward, new Vector3(0, gravityRotationForce / (actualPower + 1), 0)));

            _engineAudioSource.pitch = actualPower + StraightStability;
            _shakingAudioSource.volume = StraightSpread * 2f;
            _windBlowAudioSource.volume = _rigidbody.linearVelocity.magnitude / maxSpeed;

            float headShakeAmount = StraightSpread * headShakePower;
            return (actualPower, headShakeAmount);
        }

#if DEBUG
        public void OnGUI()
        {
            if (DEBUG_isShowOnGUIField == true)
            {
                GUI.Label(new Rect(20f, 20f, 200f, 100f), "Throttle : " + _throttleInput.ToString("F2") + ", actualPower : " + actualPower.ToString("F2"), _guiStyle);
                GUI.Label(new Rect(20f, 40f, 200f, 100f), "MPS : " + _rigidbody.linearVelocity.magnitude.ToString("F0") + ", KPH : " + (_rigidbody.linearVelocity.magnitude * 3.6f).ToString("F0"), _guiStyle);
                GUI.Label(new Rect(20f, 60f, 200f, 100f), "Height : " + _handler.transform.position.y.ToString("F0"), _guiStyle);
                GUI.Label(new Rect(20f, 80f, 200f, 100f), "StraightStability : " + StraightStability.ToString("F2") + " StraightSpread : " + StraightSpread.ToString("F2"), _guiStyle);
            }
        }

        public void OnDrawGizmos()
        {
           
        }
#endif
    }
}