using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public struct SteeringVehicleWheel : BaseWheel
    {
        public bool isFront;

        [Space(5)]
        public GameObject _object;
        public WheelCollider _collider;
    }

    public class SteeringWheelController : BaseWheelController<SteeringVehicleWheel>
    {
        private const string LOG_FORMAT = "<color=white><b>[SteeringWheelController]</b></color> {0}";

        [Header("Steerings")]
        [SerializeField]
        protected float maxSteerAngle;
        [ReadOnly]
        [SerializeField]
        protected float currentSteerAngle;

        protected override void _OnIsBreakingValueChanged(bool _value)
        {
            Debug.LogFormat(LOG_FORMAT, "_On<color=white><b>IsBreaking</b></color>ValueChanged(), _value : <color=green><b>" + _value + "</b></color>");

            if (_value == true)
            {
                for (int i = 0; i < wheels.Capacity; i++)
                {
                    wheels[i]._collider.brakeTorque = breakForce;
                }
            }
            else
            {
                for (int i = 0; i < wheels.Capacity; i++)
                {
                    wheels[i]._collider.brakeTorque = 0;
                }
            }
        }

        protected override void Update()
        {
            GetInput();
        }

        protected override void FixedUpdate()
        {
            SetMotorTorque();
            UpdateWheels();
            SetSteering();
        }

        protected override void GetInput()
        {
            inputHorizontal = Input.GetAxis("Horizontal");
            inputVertical = Input.GetAxis("Vertical");

            IsBreaking = Input.GetKey(KeyCode.Space);
        }

        protected override void SetMotorTorque()
        {
            for (int i = 0; i < wheels.Capacity; i++)
            {
                if (wheels[i].isFront == true)
                {
                    wheels[i]._collider.motorTorque = inputVertical * motorForce;
                }
                else
                {
                    // do nothing
                }
            }
        }

        protected override void UpdateWheels()
        {
            for (int i = 0; i < wheels.Capacity; i++)
            {
                Vector3 _position;
                Quaternion _rotation;

                wheels[i]._collider.GetWorldPose(out _position, out _rotation);

                wheels[i]._object.transform.rotation = _rotation;
                wheels[i]._object.transform.position = _position;
            }
        }

        protected virtual void SetSteering()
        {
            currentSteerAngle = maxSteerAngle * inputHorizontal;

            for (int i = 0; i < wheels.Capacity; i++)
            {
                if (wheels[i].isFront == true)
                {
                    wheels[i]._collider.steerAngle = currentSteerAngle;
                }
                else
                {
                    // do nothing
                }
            }
        }
    }
}