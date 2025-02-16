using _KMH_Framework;
using UnityEngine;

namespace AFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class AirplaneHandler : MonoBehaviour
    {
        [SerializeField]
        protected AirplaneCamHandler camHandler;
        [SerializeField]
        protected AirplaneAxisInputHandler axisHandler;
        [SerializeField]
        protected AirplaneRigidHandler rigidHandler;
        [SerializeField]
        protected AirplaneAnimationController animationController;
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

        protected float xRot;
        protected float yRot;

        protected void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            camHandler.OnAwake(this);
            rigidHandler.OnAwake(this);
            animationController.OnAwake();
            panelHandler.OnAwake(this);
            weaponHandler.OnAwake(this);
        }

        protected void Update()
        {
            (Vector3 rotationInput, float throttleInput) = axisHandler.GetKeyInput();

            float weaponReboundAmount = weaponHandler.UpdateAndGetWeaponReboundAmount();
            camHandler.OnUpdate(weaponReboundAmount);
            rigidHandler.OnUpdate();
            rigidHandler.SetInput(rotationInput, throttleInput);
            animationController.SetInput(rotationInput, throttleInput);

            if (Input.GetKeyDown(KeyCode.V))
            {
                isNoclipMode = !isNoclipMode;

                _Rigidbody.isKinematic = isNoclipMode;
                axisHandler.IsEnabled = !isNoclipMode;
                rigidHandler.IsEnabled = !isNoclipMode;

                if (panelHandler._Propeller.NonSpinRenderer != null)
                {
                    panelHandler._Propeller.NonSpinRenderer.enabled = !isNoclipMode;
                }
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

            Collider[] overlapColliders = Physics.OverlapSphere(this._Rigidbody.worldCenterOfMass, 20f);
            foreach (Collider overlapCollider in overlapColliders)
            {
                if (overlapCollider.TryGetComponent(out BulletHandler bullet) == true &&
                    bullet.ProjectileType == FPS_Framework.Pool.ProjectileType._50cal_Enemy)
                {
                    BlendableSoundManager.Instance.PlaySound(SoundType.BulletRicochet, bullet.transform.position);
                    break;
                }
            }
        }

        protected void FixedUpdate()
        {
            (float actualPower, float headShakeAmount) = rigidHandler.CalculateAndGetResult();
            rigidHandler.OnFixedUpdate();

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