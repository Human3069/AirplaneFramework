using _KMH_Framework;
using UnityEngine;

namespace AFramework
{
    public class AirplaneData
    {
        public AirplaneData(bool isShowOnGUIField, AirplaneInputHandler input, AirplanePhysicsHandler physics, Transform transform, Rigidbody rigidbody)
        {
            DEBUG_isShowOnGUIField = isShowOnGUIField;

            GUIStyle = new GUIStyle();
            GUIStyle.normal.textColor = Color.red;
            GUIStyle.fontSize = 20;

            Input = input;
            Physics = physics;

            Transform = transform;
            Rigidbody = rigidbody;
        }

        public bool DEBUG_isShowOnGUIField;
        public GUIStyle GUIStyle;

        public AirplaneInputHandler Input;
        public AirplanePhysicsHandler Physics;

        public Transform Transform;
        public Rigidbody Rigidbody;
    }

    [RequireComponent(typeof(Rigidbody))]
    public class AirplaneHandlerEx : MonoSingleton<AirplaneHandlerEx>
    {
        protected Rigidbody _rigidbody;

        [SerializeField]
        protected AirplaneInputHandler input;
        [SerializeField]
        protected AirplanePhysicsHandler physics;
        [SerializeField]
        protected AirplaneCamHandlerEx cam;

        protected bool DEBUG_isNoclip = false;
        protected bool DEBUG_isShowOnGUIField = true;

        protected void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();

            AirplaneData data = new AirplaneData(DEBUG_isShowOnGUIField, input, physics, this.transform, this._rigidbody);

            input.OnAwake(data);
            physics.OnAwake(data);
            cam.OnAwake(data);
        }

        protected void Update()
        {
            float factor = Time.deltaTime * 200;

            input.OnUpdate(factor);
            physics.OnUpdate(factor);
            cam.OnUpdate(factor);

#if DEBUG
            if (Input.GetKeyDown(KeyCode.V) == true)
            {
                DEBUG_isNoclip = !DEBUG_isNoclip;
                
                input.OnNoclipModeChanged(DEBUG_isNoclip);
                physics.OnNoclipModeChanged(DEBUG_isNoclip);
            }
#endif
        }

        protected void FixedUpdate()
        {
            float factor = Time.fixedDeltaTime * 50;

            input.OnFixedUpdate(factor);
            physics.OnFixedUpdate(factor);
            cam.OnFixedUpdate(factor);
        }

        private GUIStyle _guiStyle = new GUIStyle();

#if DEBUG
        protected void OnGUI()
        {
            input.OnDrawGUI();
            physics.OnDrawGUI();
            cam.OnDrawGUI();
        }
#endif

        protected void OnDrawGizmos()
        {

        }
    }
}