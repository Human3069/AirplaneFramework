using UnityEngine;

namespace _KMH_Framework
{
    [RequireComponent(typeof(Rigidbody))]
    public class LocalDraggable : MonoBehaviour
    {
        protected Rigidbody _rigidbody;

        [SerializeField]
        protected Vector3 localLinearDrag;
        [SerializeField]
        protected float tiltSpeed = 0.1f;

        protected void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected void Update()
        {
            float timeFactor = Time.deltaTime * 200;

            // get local linear drag force
            Vector3 localLinearVelocity = this.transform.InverseTransformDirection(_rigidbody.linearVelocity);
            Vector3 localLinearDragForce = new Vector3(-localLinearDrag.x * localLinearVelocity.x * Mathf.Abs(localLinearVelocity.x),
                                                       -localLinearDrag.y * localLinearVelocity.y * Mathf.Abs(localLinearVelocity.y),
                                                       -localLinearDrag.z * localLinearVelocity.z * Mathf.Abs(localLinearVelocity.z)) * timeFactor;

            Vector3 worldDragForce = this.transform.TransformDirection(localLinearDragForce);
            _rigidbody.AddForce(worldDragForce);

            // tilt from center of mass
            Vector3 diffWithCOM = _rigidbody.worldCenterOfMass - this.transform.position;
            Vector3 cross = Vector3.Cross(-diffWithCOM.normalized, Vector3.up) * diffWithCOM.sqrMagnitude * timeFactor;
            _rigidbody.AddTorque(cross * tiltSpeed);
        }
    }
}