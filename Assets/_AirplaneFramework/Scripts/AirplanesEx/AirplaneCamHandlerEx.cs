using _KMH_Framework;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplaneCamHandlerEx
    {
        private AirplaneData _data;

        [SerializeField]
        private Transform lookAtT;
        [SerializeField]
        private CinemachineCamera cinemachineCam;
        private CinemachineRotateWithFollowTarget rotateDampen;
        private CinemachineBasicMultiChannelPerlin noisePerlin;
     
        [Space(10)]
        [SerializeField]
        private float rotateSpeed = 1f;

        [Space(10)]
        [SerializeField]
        private float noisePower = 1f;
        [SerializeField]
        private float noiseGainLerpPower = 1f;
        private float currentNoiseAmplitude;

        [Space(10)]
        [SerializeField]
        private float defaultRotateDamping = 1f;
        [SerializeField]
        private float lookAroundRotateDamping = 0.5f;

        private float xRotate;
        private float yRotate;

        public void OnAwake(AirplaneData data)
        {
            this._data = data;
            KeyType.Look_Around.RegisterEventAsync(OnValueChangedLookAroundButton).Forget();

            rotateDampen = cinemachineCam.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachineRotateWithFollowTarget;
            noisePerlin = cinemachineCam.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        }

        private void OnValueChangedLookAroundButton(bool isOn)
        {
            if (isOn == true)
            {
                rotateDampen.Damping = lookAroundRotateDamping;
                OnValueChangedLookAroundButtonTrueAsync().Forget();
            }
            else
            {
                rotateDampen.Damping = defaultRotateDamping;
            }
        }

        private async UniTaskVoid OnValueChangedLookAroundButtonTrueAsync()
        {
            while (KeyType.Look_Around.IsInput() == true)
            {
                xRotate += -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
                xRotate = Mathf.Clamp(xRotate, -90, 90);

                yRotate += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
                yRotate %= 360;

                lookAtT.localEulerAngles = new Vector3(xRotate, yRotate, 0);

                await UniTask.Yield();
            }

            xRotate = 0;
            yRotate = 0;
            lookAtT.localEulerAngles = new Vector3(xRotate, yRotate, 0);
        }

        public void OnUpdate(float factor)
        {
         
        }

        public void OnFixedUpdate(float factor)
        {
            float instraightNormal = (1f - _data.Physics.StraightStabilityNormal);

            currentNoiseAmplitude = Mathf.Lerp(currentNoiseAmplitude, instraightNormal * noisePower, factor * noiseGainLerpPower);
            noisePerlin.AmplitudeGain = currentNoiseAmplitude;
        }

        public void OnDrawGUI()
        {
        
        }
    }
}