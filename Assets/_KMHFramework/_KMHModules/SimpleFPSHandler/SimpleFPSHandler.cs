using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace _KMH_Framework
{
    [RequireComponent(typeof(Camera))]
    public class SimpleFPSHandler : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[SimpleFPSHandler]</b></color> {0}";

        protected static SimpleFPSHandler _instance;
        public static SimpleFPSHandler Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [Header("Component")]
        [SerializeField]
        protected float rotateSpeed;
        [SerializeField]
        protected float returnSpeed;

        [Header("Read Only")]
        [ReadOnly]
        [SerializeField]
        protected float xRotate;
        [ReadOnly]
        [SerializeField]
        protected float yRotate;

        // Input Keys
        protected float inputX;
        protected float inputY;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected bool inputActive;
        public bool InputActive
        {
            get
            {
                return inputActive;
            }
            set
            {
                if (inputActive != value)
                {
                    inputActive = value;
                }
            }
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Instance Overlapped While On Awake()");
                Destroy(this);
                return;
            }

            StartCoroutine(PostAwake_SetRotation());
        }

        protected virtual IEnumerator PostAwake_SetRotation()
        {
            while (true)
            {
                // Set FPS Rotation
                if (InputActive == true)
                {
                    xRotate += inputX;
                    xRotate = Mathf.Clamp(xRotate, -90, 90);

                    yRotate += inputY;
                    yRotate %= 360;
                }

                // Set Default Rotation
                else
                {
                    xRotate = Mathf.Lerp(xRotate, 0, returnSpeed);
                    yRotate = Mathf.Lerp(yRotate, 0, returnSpeed);
                }

                this.transform.localEulerAngles = new Vector3(xRotate, yRotate, 0);

                yield return null;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void Update()
        {
            GetInput();
        }

        protected virtual void GetInput()
        {
            inputX = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            inputY = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;

#if DEBUG
            InputActive = Input.GetMouseButton(1);
#endif
        }

        /*
        protected virtual void SetRotation()
        {
            xRotate += inputX;
            xRotate = Mathf.Clamp(xRotate, -90, 90);

            yRotate += inputY;

            CameraTransform.localEulerAngles = new Vector3(xRotate, yRotate, 0);
        }
        */
    }
}