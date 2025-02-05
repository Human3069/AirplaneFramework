using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class MeshDragSelector : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[MeshDragSelector]</b></color> {0}";

        private const float Mesh_Safety_Multiply = 100f;
        private const float Mesh_Safety_Rect_Size = 10f;
        private const float Mesh_Max_Distance = 10000;

        protected MeshCollider thisMeshCollider;
        protected Rigidbody thisRigidbody;

        [SerializeField]
        protected Camera _camera;

        [Space(10)]
        [SerializeField]
        protected RectTransform selectUIRect; // this Rect Anchor Must Be Left-Top !!!

        [Header("Read Only")]
        [ReadOnly]
        [SerializeField]
        protected bool _isOn;
        public bool IsOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                if (_isOn != value)
                {
                    // Debug.LogFormat(LOG_FORMAT, "IsOn : <color=white><b>" + IsOn + "</b></color>");

                    _isOn = value;
                    if (value == true)
                    {
                        TriggeredList.Clear();
                        StartCoroutine(PostValueSetTrue());
                    }
                    else
                    {
                        DoneValueSetFalse();
                    }
                }
            }
        }

        protected virtual IEnumerator PostValueSetTrue()
        {
            dragStartPoint = Input.mousePosition;

            selectUIRect.anchoredPosition = GetAnchoredPositionFromPoint(dragStartPoint);

            selectUIRect.sizeDelta = Vector2.zero;
            selectUIRect.gameObject.SetActive(true);

            while (IsOn == true)
            {
                dragEndPoint = Input.mousePosition;

                SetUIRect();
                CreateMeshUsing4Ray();

                yield return new WaitForFixedUpdate();
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected Vector2 dragStartPoint;
        [ReadOnly]
        [SerializeField]
        protected Vector2 dragEndPoint;

        protected Vector2 leftDownAnchor
        {
            get
            {
                return new Vector2(dragStartPoint.x, dragEndPoint.y);
            }
        }
        protected Vector2 rightUpAnchor
        {
            get
            {
                return new Vector2(dragEndPoint.x, dragStartPoint.y);
            }
        }

        protected List<Collider> _triggeredList = new List<Collider>();
        public List<Collider> TriggeredList
        {
            get
            {
                return _triggeredList;
            }
        }

        public delegate void SelectColliders(List<Collider> colliderList);
        public event SelectColliders OnSelectColliders;

        protected Coroutine _postOnTrigger;

        protected bool _isTriggering = false;
        public bool IsTriggering
        {
            get
            {
                return _isTriggering;
            }
            protected set
            {
                if (_isTriggering != value)
                {
                    _isTriggering = value;
                }
            }
        }

        protected virtual void Awake()
        {
            thisMeshCollider = this.GetComponent<MeshCollider>();
            thisRigidbody = this.GetComponent<Rigidbody>();

            Debug.Assert(thisMeshCollider.convex == true);
            Debug.Assert(thisMeshCollider.isTrigger == true);

            Debug.Assert(thisRigidbody.isKinematic == true);
            Debug.Assert(thisRigidbody.interpolation == RigidbodyInterpolation.Interpolate || thisRigidbody.interpolation == RigidbodyInterpolation.Extrapolate);
            Debug.Assert(thisRigidbody.collisionDetectionMode == CollisionDetectionMode.Continuous);
        }

        protected virtual void OnTriggerEnter(Collider _collider)
        {
            if (TriggeredList.Contains(_collider) == false)
            {
                TriggeredList.Insert(TriggeredList.Count, _collider);
            }

            if (_postOnTrigger != null)
            {
                StopCoroutine(_postOnTrigger);
            }
            _postOnTrigger = StartCoroutine(PostOnTrigger());
        }

        protected virtual IEnumerator PostOnTrigger()
        {
            yield return null;

            if (OnSelectColliders != null)
            {
                OnSelectColliders(TriggeredList);
            }
            TriggeredList.Clear();
        }

        protected virtual void DoneValueSetFalse()
        {
            dragStartPoint = Vector2.zero;
            dragEndPoint = Vector2.zero;

            selectUIRect.sizeDelta = Vector2.zero;
            selectUIRect.gameObject.SetActive(false);

            Destroy(thisMeshCollider.sharedMesh);
            thisMeshCollider.sharedMesh = null;
        }

        protected virtual void SetUIRect()
        {
            float diffX = dragEndPoint.x - dragStartPoint.x;
            float diffY = dragStartPoint.y - dragEndPoint.y;

            float signDiffX = Mathf.Sign(diffX);
            float signDiffY = Mathf.Sign(diffY);

            int unsignedXToSign = (int)((-signDiffX / 2) + 0.5f);
            int unsignedYToSign = (int)((signDiffY / 2) + 0.5f);

            selectUIRect.pivot = new Vector2(unsignedXToSign, unsignedYToSign);
            selectUIRect.sizeDelta = new Vector2(diffX * signDiffX, diffY * signDiffY);
        }

        protected virtual Vector2 GetAnchoredPositionFromPoint(Vector2 point)
        {
            return new Vector2(point.x, point.y - Screen.height);
        }

        protected virtual Mesh GeneratePyramidMesh(Vector3[] vertices)
        {
            int[] pyramidTriangles = new int[] { 0, 2, 1, 0, 1, 4, 0, 3, 2, 0, 4, 3, 1, 2, 3, 1, 3, 4 };

            Mesh generatedMesh = new Mesh();
            generatedMesh.vertices = vertices;
            generatedMesh.triangles = pyramidTriangles;

            return generatedMesh;
        }

        protected virtual bool IsMeshManufacturableSafe()
        {
            float diffX = Mathf.Abs(dragStartPoint.x - dragEndPoint.x);
            float diffY = Mathf.Abs(dragStartPoint.y - dragEndPoint.y);

            if ((diffX / diffY) < Mesh_Safety_Multiply && (diffY / diffX) < Mesh_Safety_Multiply)
            {
                if (selectUIRect.sizeDelta.magnitude > Mesh_Safety_Rect_Size)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void CreateMeshUsing4Ray()
        {
            Vector3[] rayRearPoints = new Vector3[4];

            Ray _ray = _camera.ScreenPointToRay(dragStartPoint);
            rayRearPoints[0] = _ray.direction * Mesh_Max_Distance;                      
                                                                                        
            _ray = _camera.ScreenPointToRay(leftDownAnchor);                            
            rayRearPoints[1] = _ray.direction * Mesh_Max_Distance;                       
                                                                                         
            _ray = _camera.ScreenPointToRay(dragEndPoint);                               
            rayRearPoints[2] = _ray.direction * Mesh_Max_Distance;                          
                                                                                            
            _ray = _camera.ScreenPointToRay(rightUpAnchor);          
            rayRearPoints[3] = _ray.direction * Mesh_Max_Distance;

            if (IsMeshManufacturableSafe() == true)
            {
                Vector3[] _vertices = { _camera.transform.position, rayRearPoints[0], rayRearPoints[1], rayRearPoints[2], rayRearPoints[3] };

                Mesh _mesh = GeneratePyramidMesh(_vertices);
                thisMeshCollider.sharedMesh = _mesh;

                for (int i = 0; i < 4; i++)
                {
                    Debug.DrawLine(_camera.transform.position, rayRearPoints[i], Color.blue);
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Debug.DrawLine(_camera.transform.position, rayRearPoints[i], Color.red);
                }
            }
        }
    }
}