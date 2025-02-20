using _KMH_Framework;
using UnityEngine;

namespace AFramework
{
    [System.Serializable]
    public class AirplaneInputHandler
    {
        private AirplaneData data;

        [ReadOnly]
        public Vector3 RotationInput = Vector3.zero;
        [ReadOnly]
        public float ThrottleInput = 0f;

        private Vector2 lerpedMouseInput;

        public void OnAwake(AirplaneData data)
        {
            this.data = data;
        }

        public void OnUpdate(float factor)
        {
            if (DEBUG_isNoclip == true)
            {
                float mouseX = Input.GetAxisRaw("Mouse X") * noclipRotateSpeed * factor;
                float mouseY = Input.GetAxisRaw("Mouse Y") * noclipRotateSpeed * factor;

                yRot += mouseX;
                xRot -= mouseY;
                xRot = Mathf.Clamp(xRot, -90f, 90f);

                data.Transform.rotation = Quaternion.Euler(xRot, yRot, 0);

                float moveSpeed;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveSpeed = noclipMoveSpeed * 2f;
                }
                else if (Input.GetKey(KeyCode.LeftControl))
                {
                    moveSpeed = noclipMoveSpeed * 0.5f;
                }
                else
                {
                    moveSpeed = noclipMoveSpeed;
                }


                if (Input.GetKey(KeyCode.W))
                {
                    data.Transform.Translate(Vector3.forward * moveSpeed);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    data.Transform.Translate(Vector3.back * moveSpeed);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    data.Transform.Translate(Vector3.left * moveSpeed);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    data.Transform.Translate(Vector3.right * moveSpeed);
                }

                lerpedMouseInput = Vector3.zero;
                RotationInput = Vector3.zero;
            }
            else
            {
                Vector2 mouseInput = Vector2.zero;
                if (KeyType.Look_Around.IsInput() == false)
                {
                    mouseInput = new Vector2(Input.GetAxis("Mouse X"),
                                            -Input.GetAxis("Mouse Y")) * 7.5f;
                }
                lerpedMouseInput = Vector2.Lerp(lerpedMouseInput, mouseInput, factor * 0.05f);
                Vector3 clampedMouseInput = new Vector2(Mathf.Clamp(lerpedMouseInput.x, -1f, 1f),
                                                        Mathf.Clamp(lerpedMouseInput.y, -1f, 1f));

                RotationInput.x = clampedMouseInput.y;
                RotationInput.y = clampedMouseInput.x;

                if (KeyType.Roll_Left.IsInput() == true)
                {
                    RotationInput.z = 1f;
                }
                else if (KeyType.Roll_Right.IsInput() == true)
                {
                    RotationInput.z = -1f;
                }
                else
                {
                    RotationInput.z = 0f;
                }

                if (KeyType.Throttle_Up.IsInput() == true)
                {
                    ThrottleInput += 1f * Time.deltaTime;
                }
                else if (KeyType.Throttle_Down.IsInput() == true)
                {
                    ThrottleInput -= 1f * Time.deltaTime;
                }

                ThrottleInput = Mathf.Clamp01(ThrottleInput);
            }
        }

        public void OnFixedUpdate(float factor)
        {
        
        }

#if DEBUG
        [Header("Debug")]
        [SerializeField]
        private float noclipMoveSpeed;
        [SerializeField]
        private float noclipRotateSpeed;

        private float xRot;
        private float yRot;

        private bool DEBUG_isShowOnGUIField = true;
        private bool DEBUG_isNoclip = false;

        public void OnNoclipModeChanged(bool isOn)
        {
            DEBUG_isNoclip = isOn;
        }

        public void OnDrawGUI()
        {
            if (DEBUG_isShowOnGUIField == true)
            {
                GUI.Label(new Rect(20f, 20f, 200f, 100f), "Throttle : " + ThrottleInput.ToString("F2"), data.GUIStyle);
            }
        }
#endif
    }
}