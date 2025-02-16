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
        [SerializeField]
        protected float noiseAmount = 0.75f;

        [Space(10)]
        [SerializeField]
        protected float headShakePower;

        [Space(10)]
        [SerializeField]
        protected float maxSpeed;

        [Space(10)]
        [SerializeField]
        protected Vector3 localLinearDrag = new Vector3(0.75f, 10f, 0.75f);
        [SerializeField]
        protected float tiltSpeed = 100f;

        [Space(10)]
        [SerializeField]
        protected Vector3 defaultLinearDrag = new Vector3(0.75f, 10f, 0.75f);
        [SerializeField]
        protected Vector3 flapLinearDrag = new Vector3(0.25f, 2f, 0.25f);
        [SerializeField]
        protected Vector3 landingGearLinearDrag = new Vector3(0.5f, 1f, 0.5f);

        [Space(10)]
        [SerializeField]
        protected float defaultAngularDrag = 2f;
        [SerializeField]
        protected float flapAngularDrag = 0.5f;
        [SerializeField]
        protected float landingGearAngularDrag = 0.5f;

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
        protected bool isFlapOn = false;
        protected bool isLandingGearOn = false;

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
            KeyType.Toggle_Boot.RegisterEventAsync(OnValueChangedBootToggle).Forget();
            KeyType.Toggle_Flap.RegisterEventAsync(OnValueChangedFlapToggle).Forget();
            KeyType.Toggle_Landing_Gear.RegisterEventAsync(OnValueChangedLandingGearToggle).Forget();

            _rigidbody = handler._Rigidbody;
            _rigidbody.angularDamping = defaultAngularDrag + flapAngularDrag + landingGearAngularDrag;
            localLinearDrag = defaultLinearDrag + flapLinearDrag + landingGearLinearDrag;

            _windBlowAudioSource.clip = windClip;
            _shakingAudioSource.clip = shakingClip;

#if DEBUG
            if (DEBUG_isShowOnGUIField == true)
            {
                _guiStyle.normal.textColor = Color.red;
                _guiStyle.fontSize = 20;
            }
#endif

            OnAwakeAsync().Forget();
        }

        protected async UniTaskVoid OnAwakeAsync()
        {
            await UniTask.WaitWhile(() => KeyCodeManager.Instance == null);

            if (KeyType.Toggle_Boot.IsInputDown() == true)
            {
                KeyType.Toggle_Boot.SetToggleValue(false);
            }

            if (KeyType.Toggle_Flap.IsInputDown() == false)
            {
                KeyType.Toggle_Flap.SetToggleValue(true);
            }

            if (KeyType.Toggle_Landing_Gear.IsInputDown() == false)
            {
                KeyType.Toggle_Landing_Gear.SetToggleValue(true);
            }
        }

        protected void OnValueChangedBootToggle(bool isEngineOn)
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

        protected void OnValueChangedFlapToggle(bool isOn)
        {
            this.isFlapOn = isOn;

            if (isOn == true)
            {
                _rigidbody.angularDamping += flapAngularDrag;
                localLinearDrag += flapLinearDrag;
            }
            else
            {
                _rigidbody.angularDamping -= flapAngularDrag;
                localLinearDrag -= flapLinearDrag;
            }
        }

        protected void OnValueChangedLandingGearToggle(bool isOn)
        {
            this.isLandingGearOn = isOn;

            if (isOn == true)
            {
                _rigidbody.angularDamping += landingGearAngularDrag;
                localLinearDrag += landingGearLinearDrag;
            }
            else
            {
                _rigidbody.angularDamping -= landingGearAngularDrag;
                localLinearDrag -= landingGearLinearDrag;
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

        public void OnUpdate()
        {
            // local drag
            float timeFactor = Time.deltaTime * 200;

            // get local linear drag force
            Vector3 localLinearVelocity = _handler.transform.InverseTransformDirection(_rigidbody.linearVelocity);
            Vector3 localLinearDragForce = new Vector3(-localLinearDrag.x * localLinearVelocity.x * Mathf.Abs(localLinearVelocity.x),
                                                       -localLinearDrag.y * localLinearVelocity.y * Mathf.Abs(localLinearVelocity.y),
                                                       -localLinearDrag.z * localLinearVelocity.z * Mathf.Abs(localLinearVelocity.z)) * timeFactor;

            Vector3 worldDragForce = _handler.transform.TransformDirection(localLinearDragForce);
            _rigidbody.AddForce(worldDragForce);

            // tilt from center of mass
            Vector3 diffWithCOM = _rigidbody.worldCenterOfMass - this._handler.transform.position;
            Vector3 cross = Vector3.Cross(-diffWithCOM.normalized, Vector3.up) * diffWithCOM.sqrMagnitude * timeFactor;
            _rigidbody.AddTorque(cross * tiltSpeed);
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

            _engineAudioSource.pitch = actualPower + StraightStability;
            _shakingAudioSource.volume = StraightSpread * 2f;
            _windBlowAudioSource.volume = _rigidbody.linearVelocity.magnitude / maxSpeed;

            float headShakeAmount = StraightSpread * headShakePower;

            return (actualPower, headShakeAmount);
        }

        public void OnFixedUpdate()
        {
            float xNoise = (Mathf.PerlinNoise1D((Time.time + 0) * 0.3f) * 2) - 1;
            float yNoise = (Mathf.PerlinNoise1D((Time.time + 100) * 0.3f) * 2) - 1;
            float zNoise = (Mathf.PerlinNoise1D((Time.time + 200) * 0.3f) * 2) - 1;

            Vector3 noise = new Vector3(xNoise, yNoise, zNoise) * StraightSpread * noiseAmount;

            if (StraightSpread == 1f)
            {
                _rigidbody.AddTorque(noise * 10000f);
            }
            else
            {
                _rigidbody.AddTorque(noise * 0.75f * _rigidbody.linearVelocity.sqrMagnitude);
            }
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