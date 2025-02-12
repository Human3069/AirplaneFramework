using _KMH_Framework;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplanePanelHandler
    {
        [System.Serializable]
        public class Propeller
        {
            [SerializeField]
            private Transform transform;

            [Space(10)]
            [SerializeField]
            private float speed;
            [SerializeField]
            private float maxBlurSpeed;
            private float defaultRotSpeed;

            [Space(10)]
            [SerializeField]
            private MeshRenderer nonSpinRenderer;
            public MeshRenderer NonSpinRenderer
            {
                get
                {
                    return nonSpinRenderer;
                }
            }

            [SerializeField]
            private MeshRenderer spinRenderer;

            public void OnFixedUpdate(float mpsSpeed, float actualPower)
            {
                float spinColorThreshold = Mathf.Clamp(mpsSpeed, 0f, maxBlurSpeed) / maxBlurSpeed;
                float nonSpinColorThreshold = 1f - spinColorThreshold;

                spinRenderer.material.color = new Color(1f, 1f, 1f, spinColorThreshold);
                nonSpinRenderer.material.color = new Color(1f, 1f, 1f, nonSpinColorThreshold);

                if (KeyType.Toggle_Boot.ToggleValue() == true)
                {
                    defaultRotSpeed = Mathf.Lerp(defaultRotSpeed, (speed / 10f), 0.01f);
                }
                else
                {
                    defaultRotSpeed = Mathf.Lerp(defaultRotSpeed, 0f, 0.01f);
                }

                this.transform.Rotate((speed * actualPower) + defaultRotSpeed, 0, 0, Space.Self);
            }
        }

        protected AirplaneHandler _handler;
        protected Rigidbody _rigidbody;

        [Header("Inners")]
        [SerializeField]
        protected Transform speedoNeedleT;
        [SerializeField]
        protected Transform altitudeLongNeedleT;
        [SerializeField]
        protected Transform altitudeShortNeedleT;
        [SerializeField]
        protected Transform compassT;
        [SerializeField]
        protected Transform gyroAnglerT;
        [SerializeField]
        protected Transform gyroBallT;
        [SerializeField]
        protected Transform gearNeedleT;
        [SerializeField]
        protected Transform powerNeedleT;

        [Header("Outers - Propeller")]
        [SerializeField]
        protected Propeller _propeller;
        public Propeller _Propeller
        {
            get
            {
                return _propeller;
            }
        }

        protected float mpsSpeed;
        protected float kphSpeed
        {
            get
            {
                return mpsSpeed * 3.6f;
            }
        }

        public void OnAwake(AirplaneHandler handler)
        {
            this._handler = handler;
            _rigidbody = _handler._Rigidbody;

            Debug.Assert(_handler != null);
            Debug.Assert(_rigidbody != null);
        }

        public void OnFixedUpdate(float actualPower)
        {
            float altitude = _rigidbody.worldCenterOfMass.y;
            mpsSpeed = _rigidbody.linearVelocity.magnitude;
            float hNormal = Vector3Ex.GetHorizontalNormalUnclamped(_rigidbody.transform.eulerAngles.x);

            speedoNeedleT.localEulerAngles = new Vector3(0, 90, -kphSpeed / 2f);
            altitudeShortNeedleT.localEulerAngles = new Vector3(0, 90, -altitude * 360f / 10000f);
            altitudeLongNeedleT.localEulerAngles = new Vector3(0, 90, -altitude * 360f / 1000f);
            compassT.localEulerAngles = new Vector3(0f, -_handler.transform.eulerAngles.y, 0f);
            gyroAnglerT.localEulerAngles = new Vector3(0, 90, -_handler.transform.eulerAngles.z);
            gyroBallT.localEulerAngles = new Vector3(0, 90, -_handler.transform.eulerAngles.z);
            gyroBallT.localPosition = new Vector3(0.4288f, 0.354f - (hNormal * 0.1f), -0.2f);
            
            float normalizedAngle = Mathf.Lerp(45f, -45f, actualPower);
            powerNeedleT.localEulerAngles = new Vector3(0f, 90f, normalizedAngle); 

            _propeller.OnFixedUpdate(mpsSpeed, actualPower);
        }
    }
}