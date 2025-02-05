using _KMH_Framework;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;

namespace AFramework
{
    [System.Serializable]
    public class AirplaneCamHandler
    {
        [SerializeField]
        protected CinemachineVirtualCamera defaultCamera;
        protected CinemachineBasicMultiChannelPerlin defaultPerlin;

        [SerializeField]
        protected CinemachineVirtualCamera povCamera;
        protected CinemachinePOV pov;
        protected CinemachineBasicMultiChannelPerlin povPerlin;

        [Space(10)]
        [SerializeField]
        protected Camera lookAtCamera;

        [Space(10)]
        [SerializeField]
        protected float reboundLerpPower;

        protected AirplaneHandler handler;

        protected float currentReboundAmount;

        protected float _mouseWheelValue;
        protected float MouseWheelValue
        {
            get
            {
                return _mouseWheelValue;
            }
            set
            {
                _mouseWheelValue = value;

                if (value == 1f)
                {
                    defaultCamera.m_Lens.FieldOfView -= 5f;
                    povCamera.m_Lens.FieldOfView -= 5f;
                }
                else if (value == -1f)
                {
                    defaultCamera.m_Lens.FieldOfView += 5f;
                    povCamera.m_Lens.FieldOfView += 5f;
                }
                else
                {
                    //
                }

                defaultCamera.m_Lens.FieldOfView = Mathf.Clamp(defaultCamera.m_Lens.FieldOfView, 10f, 60f);
                povCamera.m_Lens.FieldOfView = Mathf.Clamp(povCamera.m_Lens.FieldOfView, 10f, 60f);
            }
        }

        protected float _headShakeAmount;

        public void SetHeadShakeAmount(float headShakeAmount)
        {
            this._headShakeAmount = headShakeAmount;
        }

        public void OnAwake(AirplaneHandler handler)
        {
            this.handler = handler;

            pov = povCamera.GetCinemachineComponent<CinemachinePOV>();

            Debug.Assert(defaultCamera != null);
            Debug.Assert(povCamera != null);
            Debug.Assert(pov != null);

            defaultPerlin = defaultCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            povPerlin = povCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            Debug.Assert(defaultPerlin != null);
            Debug.Assert(povPerlin != null);

            KeyType.Look_Around.RegisterEventAsync(OnLookAroundValueChanged).Forget();
        }

        public void OnUpdate(float weaponReboundAmount)
        {
            currentReboundAmount = Mathf.Lerp(currentReboundAmount, _headShakeAmount + weaponReboundAmount, Time.deltaTime * reboundLerpPower);
            defaultPerlin.AmplitudeGain = currentReboundAmount;
            povPerlin.AmplitudeGain = currentReboundAmount;

            lookAtCamera.transform.LookAt(defaultCamera.transform);

            if (Input.mouseScrollDelta != Vector2.zero)
            {
                MouseWheelValue = Input.mouseScrollDelta.y;
            }
        }

        protected void OnLookAroundValueChanged(bool isOn)
        {
            if (isOn == true)
            {
                povCamera.Priority = 1;
                defaultCamera.Priority = 0;
            }
            else
            {
                defaultCamera.Priority = 1;
                povCamera.Priority = 0;
            }
        }
    }
}