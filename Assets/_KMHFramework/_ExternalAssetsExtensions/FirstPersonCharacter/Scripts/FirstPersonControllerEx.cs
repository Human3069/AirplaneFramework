using System;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace _KMH_Framework
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonControllerEx : MonoBehaviour // FirstPersonControllerEx
    {
        [Header("Mouse Look")]
        [SerializeField]
        protected Camera thisCamera;
        [SerializeField]
        protected MouseLook thisMouseLook;

        protected Vector3 originCameraPosition;

        [Header("Physical Move Settings")]
        [SerializeField]
        protected CharacterController thisCharacterController;
        [ReadOnly]
        [SerializeField]
        protected bool isWalking;
        [SerializeField]
        protected float m_WalkSpeed;

        [SerializeField]
        protected float m_RunSpeed;
        [SerializeField]
        [Range(0f, 1f)]
        protected float m_RunstepLenghten;

        [SerializeField]
        protected float moveLerpThreshold;

        [SerializeField]
        protected float m_JumpSpeed;

        [SerializeField]
        protected float m_StickToGroundForce;

        [SerializeField]
        protected float m_GravityMultiplier;

        [SerializeField]
        protected bool m_UseFovKick;
        [SerializeField]
        protected FOVKick m_FovKick = new FOVKick();

        [SerializeField]
        protected bool m_UseHeadBob;

        [SerializeField]
        protected CurveControlledBob headBobClass = new CurveControlledBob();
        [SerializeField]
        protected LerpControlledBob jumpBobClass = new LerpControlledBob();
        [SerializeField]
        protected float m_StepInterval;

        protected Vector2 _2DInput;
        protected Vector3 _2DMoveDirection = Vector3.zero;

        protected CollisionFlags m_CollisionFlags;

        protected bool isPreviouslyGrounded;

        protected bool isJump;
        protected bool isJumping;

        protected float m_StepCycle;
        protected float m_NextStep;

        [Header("Sounds")]
        [SerializeField]
        protected AudioSource thisAudioSource;
        [SerializeField]
        protected AudioClip[] footstepAudioClips;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField]
        protected AudioClip jumpAudioClip;           // the sound played when character leaves the ground.
        [SerializeField]
        protected AudioClip landAudioClip;           // the sound played when character touches back on ground.

        protected virtual void Start()
        {
            originCameraPosition = thisCamera.transform.localPosition;

            m_FovKick.Setup(thisCamera);
            headBobClass.Setup(thisCamera, m_StepInterval);

            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;

            isJumping = false;

            thisMouseLook.Init(transform, thisCamera.transform);
        }

        protected virtual void Update()
        {
            RotateView();

            // the jump state needs to read here to make sure it is not missed
            if (isJump == false)
            {
                isJump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if ((isPreviouslyGrounded == false) && (thisCharacterController.isGrounded))
            {
                StartCoroutine(jumpBobClass.DoBobCycle());

                _2DMoveDirection.y = 0f;

                isJumping = false;

                PlayLandingSound();
            }

            if ((thisCharacterController.isGrounded == false) && (isJumping == false) && (isPreviouslyGrounded == true))
            {
                _2DMoveDirection.y = 0f;
            }

            isPreviouslyGrounded = thisCharacterController.isGrounded;
        }

        protected virtual void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * _2DInput.y + transform.right * _2DInput.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, thisCharacterController.radius, Vector3.down, out hitInfo,
                               thisCharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            if (thisCharacterController.isGrounded)
            {
                _2DMoveDirection.y = -m_StickToGroundForce;

                //_2DMoveDirection.x = desiredMove.x * speed;
                //_2DMoveDirection.z = desiredMove.z * speed;

                _2DMoveDirection.x = Mathf.Lerp(_2DMoveDirection.x, desiredMove.x * speed, Time.deltaTime * moveLerpThreshold);
                _2DMoveDirection.z = Mathf.Lerp(_2DMoveDirection.z, desiredMove.z * speed, Time.deltaTime * moveLerpThreshold);

                if (isJump == true)
                {
                    _2DMoveDirection.y = m_JumpSpeed;

                    isJump = false;
                    isJumping = true;

                    PlayJumpSound();
                }
            }
            else
            {
                _2DMoveDirection += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = thisCharacterController.Move(_2DMoveDirection * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            thisMouseLook.UpdateCursorLock();
        }

        protected virtual void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");
            
            bool wasWalking = isWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            isWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            if (isWalking == true)
            {
                speed = m_WalkSpeed;
            }
            else
            {
                speed = m_RunSpeed;
            }

            _2DInput = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (_2DInput.sqrMagnitude > 1)
            {
                _2DInput.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (isWalking != wasWalking && m_UseFovKick && thisCharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();

                if (isWalking == false)
                {
                    StartCoroutine(m_FovKick.FOVKickUp());
                }
                else
                {
                    StartCoroutine(m_FovKick.FOVKickDown());
                }
            }
        }

        protected virtual void ProgressStepCycle(float speed)
        {
            if (thisCharacterController.velocity.sqrMagnitude > 0 && (_2DInput.x != 0 || _2DInput.y != 0))
            {
                float speedThreshold;
                if (isWalking == true)
                {
                    speedThreshold = 1f;
                }
                else
                {
                    speedThreshold = m_RunstepLenghten;
                }

                m_StepCycle += (thisCharacterController.velocity.magnitude + (speed * speedThreshold)) * Time.fixedDeltaTime;
            }

            if (m_StepCycle <= m_NextStep)
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }

        protected virtual void UpdateCameraPosition(float speed) // use head bob
        {
            Vector3 newCameraPosition;

            if (m_UseHeadBob == false)
            {
                return;
            }
            if ((thisCharacterController.velocity.magnitude > 0.5f) && (thisCharacterController.isGrounded))
            {
                float speedThreshold;
                if (isWalking == true)
                {
                    speedThreshold = 1f;
                }
                else
                {
                    speedThreshold = m_RunstepLenghten;
                }

                thisCamera.transform.localPosition = headBobClass.DoHeadBob(thisCharacterController.velocity.magnitude + (speed * speedThreshold));

                newCameraPosition = thisCamera.transform.localPosition;
                newCameraPosition.y = thisCamera.transform.localPosition.y - jumpBobClass.Offset();
            }
            else
            {
                newCameraPosition = thisCamera.transform.localPosition;
                newCameraPosition.y = originCameraPosition.y - jumpBobClass.Offset();
            }
            thisCamera.transform.localPosition = newCameraPosition;
        }

        protected virtual void RotateView()
        {
            thisMouseLook.LookRotation(transform, thisCamera.transform);
        }

        protected virtual void PlayFootStepAudio()
        {
            if (!thisCharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, footstepAudioClips.Length);

            thisAudioSource.clip = footstepAudioClips[n];
            thisAudioSource.PlayOneShot(thisAudioSource.clip);

            // move picked sound to index 0 so it's not picked next time
            footstepAudioClips[n] = footstepAudioClips[0];
            footstepAudioClips[0] = thisAudioSource.clip;
        }

        protected virtual void PlayJumpSound()
        {
            thisAudioSource.clip = jumpAudioClip;
            thisAudioSource.Play();
        }

        protected virtual void PlayLandingSound()
        {
            thisAudioSource.clip = landAudioClip;
            thisAudioSource.Play();
            m_NextStep = m_StepCycle + 0.5f;
        }

        protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(thisCharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
