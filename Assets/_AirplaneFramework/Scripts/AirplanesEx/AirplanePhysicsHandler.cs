using _KMH_Framework;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplanePhysicsHandler
    {
        private AirplaneData data;

        [Header("Boots")]
        [ReadOnly]
        [SerializeField]
        private float bootNormal;
        [SerializeField]
        private float bootNormalLerpValue;

        [Header("Speeds")]
        [SerializeField]
        private float moveSpeed;
        [SerializeField]
        private float rotateSpeed;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        private float actualPower;
        [SerializeField]
        private float actualPowerLerpValue;

        [Space(10)]
        [ReadOnly]
        public float StraightStabilityNormal;
        [SerializeField]
        private float straightStabilitySenstivity;

        [Header("Drags")]
        [ReadOnly]
        [SerializeField]
        private Vector3 localLinearDrag;

        [Space(10)]
        [SerializeField]
        private Vector3 defaultLinearDrag = new Vector3(0.75f, 10f, 0.75f);
        [SerializeField]
        private Vector3 flapLinearDrag = new Vector3(0.25f, 2f, 0.25f);
        [SerializeField]
        private Vector3 landingGearLinearDrag = new Vector3(0.5f, 1f, 0.5f);

        [Space(10)]
        [SerializeField]
        private float floatingPower;

        [Header("Tilts")]
        [SerializeField]
        private float tiltSpeed;

        public void OnAwake(AirplaneData data)
        {
            this.data = data;
            this.data.Rigidbody.linearDamping = 0f;

            OnAwakeAsync().Forget();
        }

        protected async UniTaskVoid OnAwakeAsync()
        {
            await UniTask.WaitWhile(() => KeyCodeManager.Instance == null);

            localLinearDrag = defaultLinearDrag + flapLinearDrag + landingGearLinearDrag;

            KeyType.Toggle_Flap.SetToggleValue(true);
            KeyType.Toggle_Landing_Gear.SetToggleValue(true);

            KeyType.Toggle_Flap.RegisterEvent(OnValueChangedFlapToggle);
            KeyType.Toggle_Landing_Gear.RegisterEvent(OnValueChangedLandingGearToggle);
        }

        protected void OnValueChangedFlapToggle(bool isOn)
        {
            if (isOn == true)
            {
                localLinearDrag += flapLinearDrag;
            }
            else
            {
                localLinearDrag -= flapLinearDrag;
            }
        }

        protected void OnValueChangedLandingGearToggle(bool isOn)
        {
            if (isOn == true)
            {
                localLinearDrag += landingGearLinearDrag;
            }
            else
            {
                localLinearDrag -= landingGearLinearDrag;
            }
        }

        public void OnUpdate(float factor)
        {
            
        }

        public void OnFixedUpdate(float factor)
        {
            DoLocalDragAndTilt(factor);
            MoveAndRotate(factor);
        }

        private void DoLocalDragAndTilt(float factor)
        {
            // get local linear drag force
            Vector3 localLinearVelocity = data.Transform.InverseTransformDirection(data.Rigidbody.linearVelocity);
            Vector3 localLinearDragForce = new Vector3(-localLinearDrag.x * localLinearVelocity.x * Mathf.Abs(localLinearVelocity.x),
                                                       -localLinearDrag.y * localLinearVelocity.y * Mathf.Abs(localLinearVelocity.y),
                                                       -localLinearDrag.z * localLinearVelocity.z * Mathf.Abs(localLinearVelocity.z));

            Vector3 worldDragForce = data.Transform.TransformDirection(localLinearDragForce);
            data.Rigidbody.AddForce(worldDragForce * factor);

            // tilt from center of mass
            Vector3 diffWithCOM = data.Rigidbody.worldCenterOfMass - data.Transform.position;
            Vector3 cross = Vector3.Cross(-diffWithCOM.normalized, Vector3.up) * diffWithCOM.sqrMagnitude;
            data.Rigidbody.AddTorque(cross * tiltSpeed * factor);
        }

        private void MoveAndRotate(float factor)
        {
            float mps = data.Rigidbody.linearVelocity.magnitude;

            if (KeyType.Toggle_Boot.ToggleValue() == true)
            {
                actualPower = Mathf.Lerp(actualPower, data.Input.ThrottleInput, actualPowerLerpValue * factor);
                bootNormal = Mathf.Lerp(bootNormal, 1f, bootNormalLerpValue * factor);
            }
            else
            {
                actualPower = Mathf.Lerp(actualPower, 0f, actualPowerLerpValue * factor);
                bootNormal = Mathf.Lerp(bootNormal, 0f, bootNormalLerpValue * factor);
            }

            StraightStabilityNormal = 1f;
            if (mps > 1f)
            {
                float stability = 1f - ((data.Transform.forward - data.Rigidbody.linearVelocity.normalized).magnitude * straightStabilitySenstivity);
                StraightStabilityNormal = Mathf.Clamp01(stability);
            }

            Vector3 forwardForceFactor = data.Transform.forward * actualPower * StraightStabilityNormal * moveSpeed * factor;
            Vector3 upwardForceFactor = data.Transform.up * actualPower * floatingPower * mps * factor;
            Vector3 totalForceFactor;
            if (KeyType.Toggle_Flap.ToggleValue() == true)
            {
                totalForceFactor = forwardForceFactor + upwardForceFactor;
            }
            else
            {
                totalForceFactor = forwardForceFactor;
            }
            data.Rigidbody.AddForce(totalForceFactor);

            Vector3 torqueFactor = data.Transform.TransformDirection(data.Input.RotationInput * actualPower * StraightStabilityNormal * mps * rotateSpeed * factor);
            data.Rigidbody.AddTorque(torqueFactor);
        }

#if DEBUG
        private bool DEBUG_isShowOnGUIField = true;

        public void OnNoclipModeChanged(bool isOn)
        {
            data.Rigidbody.isKinematic = isOn;
        }

        public void OnDrawGUI()
        {
            if (DEBUG_isShowOnGUIField == true)
            {
                GUI.Label(new Rect(20f, 40f, 200f, 100f), "actualPower : " + actualPower.ToString("F2"), data.GUIStyle);
                GUI.Label(new Rect(20f, 60f, 200f, 100f), "MPS : " + data.Rigidbody.linearVelocity.magnitude.ToString("F0") + ", KPH : " + (data.Rigidbody.linearVelocity.magnitude * 3.6f).ToString("F0"), data.GUIStyle);
                GUI.Label(new Rect(20f, 80f, 200f, 100f), "Height : " + data.Transform.position.y.ToString("F0"), data.GUIStyle);
                GUI.Label(new Rect(20f, 100f, 200f, 100f), "StraightStability : " + StraightStabilityNormal.ToString("F2"), data.GUIStyle);
            }
        }
#endif
    }
}