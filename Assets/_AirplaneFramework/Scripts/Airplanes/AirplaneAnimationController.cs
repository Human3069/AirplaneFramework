using _KMH_Framework;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplaneAnimationController 
    {
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected float towardDelta = 1f;

        protected Vector3 currentRotationInput = Vector3.zero;

        public void OnAwake()
        {
            animator.SetBool("IsEnabled", true);

            KeyType.Toggle_Landing_Gear.RegisterEventAsync(OnValueChangedLandingGearToggle).Forget();
            KeyType.Toggle_Flap.RegisterEventAsync(OnValueChangedFlapToggle).Forget();
        }

        public void SetInput(Vector3 rotationInput, float throttleInput)
        {
            currentRotationInput = Vector3.MoveTowards(currentRotationInput, rotationInput, towardDelta * Time.deltaTime);

            animator.SetFloat("Pitch", currentRotationInput.x);
            animator.SetFloat("Yaw", currentRotationInput.y);
            animator.SetFloat("Roll", currentRotationInput.z);
            animator.SetFloat("Throttle", throttleInput);
        }

        protected void OnValueChangedLandingGearToggle(bool isOn)
        {
            animator.SetBool("IsGearOn", isOn);
            animator.SetTrigger("IsGearValueChanged");
        }

        protected void OnValueChangedFlapToggle(bool isOn)
        {
            animator.SetBool("IsFlapOn", isOn);
            animator.SetTrigger("IsFlapValueChanged");
        }
    }
}