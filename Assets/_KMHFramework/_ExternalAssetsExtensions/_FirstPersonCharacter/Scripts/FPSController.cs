using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _KMH_Framework
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class FPSController : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[FPSController]</b></color> {0}";

        [System.Serializable]
        public class HeadBob
        {
            public AnimationCurve _AnimationCurve;
            public float BobStrength;

            public float GetBobPoint(float currentStep, float walkingThreshold)
            {
                float bobResult = _AnimationCurve.Evaluate(currentStep) * BobStrength * walkingThreshold;

                return bobResult;
            }
        }

        protected Rigidbody _bodyRigidbody;
        protected FootCollisionDetector _footCollisionDetector;
        protected Animator poseAnimator;

        [SerializeField]
        protected FPSPlayerWeaponHandler playerWeaponHandler;

        [Space(10)]
        [SerializeField]
        protected Transform headTransform;
        [SerializeField]
        protected Transform bodyTransform;
        [SerializeField]
        protected Transform footTransform;

        [Header("Mouse Rotations")]
        [SerializeField]
        protected float maxPitchAngle;
        [SerializeField]
        protected float minPitchAngle;

        [Space(10)]
        [SerializeField]
        protected float rotateSpeed;

        protected Vector2 mouseRotateInput;
        protected Vector2 mouseRotation;

        [Header("Keyboard Movements")]
        [SerializeField]
        protected float moveSpeed;

        [Space(10)]
        [SerializeField]
        protected float slowSpeedMultiply;
        [SerializeField]
        protected float fastSpeedMultiply;

        [Space(10)]
        [SerializeField]
        protected float moveLerpPower;

        [Space(10)]
        [SerializeField]
        protected float jumpPower;

        protected Vector3 moveMotion;
        protected Vector3 lerpedMoveMotion;

        protected float currentSpeedMultiply;

        [Header("Posed Position")]
        [SerializeField]
        protected float poseLerpPower;

        [Space(10)]
        [SerializeField]
        protected Vector3 standPosedPos;
        [SerializeField]
        protected Vector3 crouchPosedPos;
        [SerializeField]
        protected Vector3 pronePosedPos;

        protected Vector3 currentPosedPosition;
        protected Coroutine poseLerpRoutine;

        [Header("HeadBob")]
        [SerializeField]
        protected float stepSpeed;
        [SerializeField]
        protected float fastBobMultiply;
        [SerializeField]
        protected float slowBobMultiply;

        protected Vector3 bobPoint;
        protected Vector3 bobRotation;

        [Space(10)]
        [SerializeField]
        protected HeadBob headBobHorizontalMove;
        [SerializeField]
        protected HeadBob headBobVerticalMove;

        [Space(10)]
        [SerializeField]
        protected HeadBob headBobHorizontalRotate;
        [SerializeField]
        protected HeadBob headBobVerticalRotate;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float currentStep;

        protected Vector3 recoverableReboundRotation;
        protected Vector3 nonRecoverableReboundRotation;

        protected float currentBobMultiply;
        protected float walkingThreshold;

        protected bool isMoveForward;
        protected bool isMoveBackward;
        protected bool isMoveRight;
        protected bool isMoveLeft;
        protected bool isRun;
        protected bool isJump;

        protected bool isStand_Radio = true;
        protected bool isCrouch_Radio;
        protected bool isProne_Radio;

        protected bool _isOnGround;
        protected bool IsOnGround
        {
            get
            {
                return _isOnGround;
            }
            set
            {
                if (_isOnGround != value)
                {
                    _isOnGround = value;

                    if (value == true)
                    {
                        _bodyRigidbody.linearDamping = 100f;
                    }
                    else
                    {
                        moveMotion = Vector3.zero;
                        _bodyRigidbody.inertiaTensor = Vector3.zero;

                        _bodyRigidbody.linearDamping = 0.1f;
                        _bodyRigidbody.AddForce(moveMotion * 500000);
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _bodyRigidbody = bodyTransform.GetComponent<Rigidbody>();
            _footCollisionDetector = footTransform.GetComponent<FootCollisionDetector>();
            poseAnimator = bodyTransform.GetComponent<Animator>();

            currentPosedPosition = standPosedPos;

            AwakeAsync().Forget();
        }

        protected virtual async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitWhile(() => KeyCodeManager.Instance == null);

            // KeyInputManager.Instance.KeyData["Jump"].OnValueChanged += OnJumpValueChanged;
            // KeyInputManager.Instance.KeyData["Crouch"].OnValueChanged += OnCrouchValueChanged;
            // KeyInputManager.Instance.KeyData["Prone"].OnValueChanged += OnProneValueChanged;
        }

        protected void OnJumpValueChanged(bool _value)
        {
            if (_value == true)
            {
                if (isStand_Radio == true)
                {
                    if (IsOnGround == true)
                    {
                        _bodyRigidbody.linearDamping = 0.1f;
                        _bodyRigidbody.AddForce(0f, jumpPower, 0f);
                    }
                }
                else
                {
                    isStand_Radio = true;
                    isCrouch_Radio = false;
                    isProne_Radio = false;

                    poseAnimator.SetTrigger("Stand");
                    if (poseLerpRoutine != null)
                    {
                        StopCoroutine(poseLerpRoutine);
                    }
                    poseLerpRoutine = StartCoroutine(LerpedPoserRoutine(standPosedPos));
                }
            }
        }

        protected void OnCrouchValueChanged(bool _value)
        {
            if (_value == true)
            {
                if (isCrouch_Radio == true)
                {
                    isStand_Radio = true;
                    isCrouch_Radio = false;
                    isProne_Radio = false;

                    poseAnimator.SetTrigger("Stand");
                    if (poseLerpRoutine != null)
                    {
                        StopCoroutine(poseLerpRoutine);
                    }
                    poseLerpRoutine = StartCoroutine(LerpedPoserRoutine(standPosedPos));
                }
                else
                {
                    isStand_Radio = false;
                    isCrouch_Radio = true;
                    isProne_Radio = false;

                    poseAnimator.SetTrigger("Crouch");
                    if (poseLerpRoutine != null)
                    {
                        StopCoroutine(poseLerpRoutine);
                    }
                    poseLerpRoutine = StartCoroutine(LerpedPoserRoutine(crouchPosedPos));
                }
            }
        }

        protected void OnProneValueChanged(bool _value)
        {
            if (_value == true)
            {
                if (isProne_Radio == true)
                {
                    isStand_Radio = true;
                    isCrouch_Radio = false;
                    isProne_Radio = false;

                    poseAnimator.SetTrigger("Stand");
                    if (poseLerpRoutine != null)
                    {
                        StopCoroutine(poseLerpRoutine);
                    }
                    poseLerpRoutine = StartCoroutine(LerpedPoserRoutine(standPosedPos));
                }
                else
                {
                    isStand_Radio = false;
                    isCrouch_Radio = false;
                    isProne_Radio = true;

                    poseAnimator.SetTrigger("Prone");
                    if (poseLerpRoutine != null)
                    {
                        StopCoroutine(poseLerpRoutine);
                    }
                    poseLerpRoutine = StartCoroutine(LerpedPoserRoutine(pronePosedPos));
                }
            }
        }

        protected IEnumerator LerpedPoserRoutine(Vector3 targetPose)
        {
            while (currentPosedPosition != targetPose)
            {
                currentPosedPosition = Vector3.Lerp(currentPosedPosition, targetPose, poseLerpPower * Time.deltaTime);
                yield return null;
            }
        }

        protected virtual void Update()
        {
            HandleMouseInput();
            HandleMoveInput();
            HandleHeadbob();

            IsOnGround = _footCollisionDetector.IsCollisioning;
        }

        protected virtual void HandleMouseInput()
        {
            float inputX = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            float inputY = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;

            mouseRotateInput = new Vector2(inputX, inputY);

            mouseRotation.x += mouseRotateInput.x + nonRecoverableReboundRotation.x;
            mouseRotation.x = Mathf.Clamp(mouseRotation.x, minPitchAngle, maxPitchAngle);

            mouseRotation.y += mouseRotateInput.y;
            mouseRotation.y %= 360f;

            bodyTransform.localEulerAngles = new Vector3(0, mouseRotation.y, 0);

            if (playerWeaponHandler.EquipedWeapon != null)
            {
                recoverableReboundRotation = playerWeaponHandler.EquipedWeapon.RecoverableRebound.Angle;
            }
            headTransform.localEulerAngles = new Vector3(mouseRotation.x, 0, 0) + bobRotation + recoverableReboundRotation;
            headTransform.localPosition = currentPosedPosition + bobPoint;
        }

        protected virtual void HandleMoveInput()
        {
            // isMoveForward = KeyInputManager.Instance.KeyData["Move Forward"].isInput;
            // isMoveBackward = KeyInputManager.Instance.KeyData["Move Backward"].isInput;
            // isMoveRight = KeyInputManager.Instance.KeyData["Move Right"].isInput;
            // isMoveLeft = KeyInputManager.Instance.KeyData["Move Left"].isInput;
            // 
            // isRun = KeyInputManager.Instance.KeyData["Run"].isInput;

            if (isRun == true && isStand_Radio == true)
            {
                currentSpeedMultiply = fastSpeedMultiply;
                currentBobMultiply = fastBobMultiply;
            }
            else if (isCrouch_Radio == true || isProne_Radio == true)
            {
                currentSpeedMultiply = slowSpeedMultiply;
                currentBobMultiply = slowBobMultiply;
            }
            else
            {
                currentSpeedMultiply = 1f;
                currentBobMultiply = 1f;
            }

            float zDirection;
            float xDirection;

            if (isMoveForward == true)
            {
                zDirection = 1;
            }
            else if (isMoveBackward == true)
            {
                zDirection = -1;
            }
            else
            {
                zDirection = 0f;
            }

            if (isMoveRight == true)
            {
                xDirection = 1;
            }
            else if (isMoveLeft == true)
            {
                xDirection = -1;
            }
            else
            {
                xDirection = 0f;
            }

            if (IsOnGround == true)
            {
                Vector3 sortedDirection = new Vector3(xDirection, 0, zDirection).normalized * moveSpeed * currentSpeedMultiply * Time.deltaTime;
                moveMotion = bodyTransform.TransformDirection(sortedDirection);
            }

            lerpedMoveMotion = Vector3.Lerp(lerpedMoveMotion, moveMotion, moveLerpPower);
            bodyTransform.Translate(lerpedMoveMotion, Space.World);
        }

        protected void HandleHeadbob()
        {
            if (IsOnGround == true)
            {
                if (isMoveForward == true ||
                    isMoveBackward == true ||
                    isMoveRight == true ||
                    isMoveLeft == true)
                {
                    currentStep += stepSpeed * currentBobMultiply * Time.deltaTime;
                    walkingThreshold = Mathf.Lerp(walkingThreshold, 1f, moveLerpPower);
                }
                else
                {
                    walkingThreshold = Mathf.Lerp(walkingThreshold, 0f, moveLerpPower);
                }
            }
            else
            {
                walkingThreshold = Mathf.Lerp(walkingThreshold, 0f, moveLerpPower);
            }

            if (currentStep >= 1)
            {
                currentStep -= 1;
            }

            float horizontalMove = headBobHorizontalMove.GetBobPoint(currentStep, walkingThreshold);
            float verticalMove = headBobVerticalMove.GetBobPoint(currentStep, walkingThreshold);

            bobPoint = new Vector3(horizontalMove, verticalMove, 0);

            float horizontalAngle = headBobHorizontalRotate.GetBobPoint(currentStep, walkingThreshold);
            float verticalAngle = headBobVerticalRotate.GetBobPoint(currentStep, walkingThreshold);

            bobRotation = new Vector3(horizontalAngle, verticalAngle, 0);
        }
    }
}