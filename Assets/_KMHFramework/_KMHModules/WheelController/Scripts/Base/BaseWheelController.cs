using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public interface BaseWheel
    {
        //
    }

    public abstract class BaseWheelController<T> : MonoBehaviour where T : BaseWheel
    {
        // private const string LOG_FORMAT = "<color=white><b>[WheelHandler]</b></color> {0}";

        [SerializeField]
        protected List<T> wheels;

        [Header("Movement Settings")]
        [SerializeField]
        protected float motorForce;
        [SerializeField]
        protected float breakForce;

        protected bool isBreaking;
        protected bool IsBreaking
        {
            get
            {
                return isBreaking;
            }
            set
            {
                if (value != isBreaking)
                {
                    isBreaking = value;

                    _OnIsBreakingValueChanged(value);
                }
            }
        }

        protected float inputHorizontal;
        protected float inputVertical;

        protected abstract void _OnIsBreakingValueChanged(bool _value);
        /*
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
        */

        protected virtual void Update()
        {
            GetInput();
        }

        protected virtual void FixedUpdate()
        {
            SetMotorTorque();
            UpdateWheels();
        }

        protected abstract void GetInput();
        /*
        {
            inputHorizontal = Input.GetAxis("Horizontal");
            inputVertical = Input.GetAxis("Vertical");

            Input.GetKey(KeyCode.Space) = IsBreaking;
        }
        */

        protected abstract void SetMotorTorque();
        /*
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

            SetWheelBreaking();
        }
        */

        protected abstract void UpdateWheels();
        /*
        {
            for (int i = 0; i < wheels.Capacity; i++)
            {
                Vector3 _position;
                Quaternion _rotation;

                wheelCollider.GetWorldPose(out _position, out _rotation);

                wheelTransform.rotation = _rotation;
                wheelTransform.position = _position;
            }
        }
        */
    }
}