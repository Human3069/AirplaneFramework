using _KMH_Framework;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class AirplaneHandler : MonoBehaviour
    {
        protected float xRot;
        protected float yRot;

        [SerializeField]
        protected AirplaneCamHandler camHandler;
        [SerializeField]
        protected AirplaneAxisInputHandler axisHandler;
        [SerializeField]
        protected AirplaneRigidHandler rigidHandler;
        [SerializeField]
        protected AirplanePanelHandler panelHandler;
        [SerializeField]
        protected AirplaneWeaponHandler weaponHandler;

        protected Rigidbody _rigidbody;
        public Rigidbody _Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = this.GetComponent<Rigidbody>();
                }

                return _rigidbody;
            }
        }

        [Header("DEBUG_NoclipMode")]
        [ReadOnly]
        [SerializeField]
        protected bool isNoclipMode = false;
        [SerializeField]
        protected float noclipRotateSpeed = 1f;
        [SerializeField]
        protected float noclipMoveSpeed = 1f;

        protected void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            camHandler.OnAwake(this);
            rigidHandler.OnAwake(this);
            panelHandler.OnAwake(this);
            weaponHandler.OnAwake(this);
        }

        protected void Update()
        {
            (Vector3 rotationInput, float throttleInput) = axisHandler.GetKeyInput();

            float weaponReboundAmount = weaponHandler.UpdateAndGetWeaponReboundAmount();
            camHandler.OnUpdate(weaponReboundAmount);
            rigidHandler.SetInput(rotationInput, throttleInput);
      
            if (Input.GetKeyDown(KeyCode.V))
            {
                isNoclipMode = !isNoclipMode;

                _Rigidbody.isKinematic = isNoclipMode;
                axisHandler.IsEnabled = !isNoclipMode;
                rigidHandler.IsEnabled = !isNoclipMode;

                panelHandler._Propeller.NonSpinRenderer.enabled = !isNoclipMode;
            }

            if (isNoclipMode == true)
            {
                float mouseX = Input.GetAxisRaw("Mouse X") * noclipRotateSpeed * Time.deltaTime;
                float mouseY = Input.GetAxisRaw("Mouse Y") * noclipRotateSpeed * Time.deltaTime;

                yRot += mouseX;
                xRot -= mouseY;
                xRot = Mathf.Clamp(xRot, -90f, 90f);

                this.transform.rotation = Quaternion.Euler(xRot, yRot, 0);

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
                    this.transform.Translate(Vector3.forward * moveSpeed);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    this.transform.Translate(Vector3.back * moveSpeed);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    this.transform.Translate(Vector3.left * moveSpeed);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    this.transform.Translate(Vector3.right * moveSpeed);
                }
            }
        }

        protected void FixedUpdate()
        {
            (float actualPower, float headShakeAmount) = rigidHandler.CalculateAndGetResult();

            camHandler.SetHeadShakeAmount(headShakeAmount);
            panelHandler.OnFixedUpdate(actualPower);
        }

        protected void OnCollisionEnter(Collision collision)
        {
            float speedOnCollision = collision.relativeVelocity.magnitude;

            Debug.Log("speedOnCollision : " + speedOnCollision);
        }

#if UNITY_EDITOR
        protected void OnGUI()
        {
            rigidHandler.OnGUI();
        }

        protected void OnDrawGizmos()
        {
            rigidHandler.OnDrawGizmos();
        }
#endif
    }
}