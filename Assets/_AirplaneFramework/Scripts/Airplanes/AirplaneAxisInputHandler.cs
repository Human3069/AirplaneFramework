using _KMH_Framework;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplaneAxisInputHandler
    {
        [HideInInspector]
        public bool IsEnabled = true;

        [ReadOnly]
        [SerializeField]
        protected Vector3 rotationInput;
        [ReadOnly]
        [SerializeField]
        protected float throttleInput;

        protected Vector3 lerpedMouseInput;

        public (Vector3, float) GetKeyInput()
        {
            if (IsEnabled == false || KeyCodeManager.Instance == null)
            {
                return (Vector2.zero, 0f);
            }

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 mouseInput = Vector3.zero;
            if (KeyType.Look_Around.IsInput() == false)
            {
                mouseInput = new Vector2(mouseX, -mouseY) * 7.5f;
            }

            lerpedMouseInput = Vector2.Lerp(lerpedMouseInput, mouseInput, Time.deltaTime * 10f);
            Vector3 clampedMouseInput = new Vector2(Mathf.Clamp(lerpedMouseInput.x, -1f, 1f),
                                                    Mathf.Clamp(lerpedMouseInput.y, -1f, 1f));

            rotationInput.x = clampedMouseInput.y;
            rotationInput.y = clampedMouseInput.x;

            if (KeyType.Roll_Left.IsInput() == true)
            {
                rotationInput.z = 1f;
            }
            else if (KeyType.Roll_Right.IsInput() == true)
            {
                rotationInput.z = -1f;
            }
            else
            {
                rotationInput.z = 0f;
            }

            if (KeyType.Throttle_Up.IsInput() == true)
            {
                throttleInput += 1f * Time.deltaTime;
            }
            else if (KeyType.Throttle_Down.IsInput() == true)
            {
                throttleInput -= 1f * Time.deltaTime;
            }

            throttleInput = Mathf.Clamp01(throttleInput);

            return (rotationInput, throttleInput);
        }
    }
}