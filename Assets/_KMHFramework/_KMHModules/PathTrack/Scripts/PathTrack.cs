using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _KMH_Framework
{
    public class PathTrack : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[PathTrack]</b></color> {0}";

        private const float MAX_APPROXIMATE_LENGTH = 0.1f;
        private const float MAX_APPROXIMATE_ANGLE = 1f;

        [Header("=== PathTrack ===")]
        [SerializeField]
        protected Transform targetTransform; // 시네머신 브레인을 따라다니는 Transform

        [Space(10)]
        [SerializeField]
        protected float samplingThreshold = 1f; // 샘플링할 베지어 포인트의 임계값, 값이 커질수록 베지어가 정밀해지지만 그만큼 리소스 사용량이 높아짐

        [Space(10)]
        [SerializeField]
        protected bool isRideOnAwake = false;
        [SerializeField]
        protected float speed = 1f;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int currentPathIndex = 0;
        [ReadOnly]
        [SerializeField]
        protected float pathNormalProgress = 0f;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int bezierPointIndex = 0;
        [ReadOnly]
        [SerializeField]
        protected float currentBezierPointProgress = 0f;
        [ReadOnly]
        [SerializeField]
        protected float bezierPointNormalProgress = 0f;
     
        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float normalPathProgress = 0f;

        [Space(10)]
        [SerializeField]
        protected List<PathPoint> pathPointList = new List<PathPoint>();
        [ReadOnly]
        [SerializeField]
        protected List<CubicBezierCurve> cubicBezierCurveList = new List<CubicBezierCurve>();
        [ReadOnly]
        [SerializeField]
        protected List<Vector3> bezierPointList = new List<Vector3>();

        protected bool _isApproximatelyPositionDone;
        protected bool _isApproximatelyRotationDone;

        public bool IsApproximatelyDone
        {
            get
            {
                return _isApproximatelyPositionDone == true && _isApproximatelyRotationDone == true;
            }
            protected set
            {
                _isApproximatelyPositionDone = value;
                _isApproximatelyRotationDone = value;
            }
        }

        protected virtual void Awake()
        {
            if (isRideOnAwake == true)
            {
                Execute();
            }
        }

        public void Execute()
        {
            Debug.LogFormat(LOG_FORMAT, "Execute()");

            IsApproximatelyDone = false;
            StartCoroutine(ExecutePositionRoutine());
            StartCoroutine(ExecuteRotationRoutine());
        }

        public void ExecuteImmediately()
        {
            Debug.LogFormat(LOG_FORMAT, "ExecuteImmediately()");

            StopAllCoroutines();

            targetTransform.position = pathPointList[pathPointList.Count - 1].Position;
            targetTransform.rotation = pathPointList[pathPointList.Count - 1].Rotation;

            currentBezierPointProgress = 1f;
        }

        protected IEnumerator ExecutePositionRoutine()
        {
            while (currentBezierPointProgress < bezierPointList.Count - 1)
            {
                currentBezierPointProgress += Time.deltaTime * speed;
                bezierPointIndex = (int)currentBezierPointProgress;
                bezierPointNormalProgress = currentBezierPointProgress - bezierPointIndex;

                if (bezierPointIndex < bezierPointList.Count - 1)
                {
                    targetTransform.position = Vector3.Lerp(bezierPointList[bezierPointIndex], bezierPointList[bezierPointIndex + 1], bezierPointNormalProgress);
                }

                yield return null;
            }
            currentBezierPointProgress = bezierPointList.Count - 1;
            bezierPointIndex = (int)currentBezierPointProgress;
            bezierPointNormalProgress = 0f;

            normalPathProgress = 0f;

            targetTransform.position = bezierPointList[bezierPointList.Count - 1];
            _isApproximatelyPositionDone = true;
        }

        protected IEnumerator ExecuteRotationRoutine()
        {
            currentPathIndex = 0;
            while (bezierPointIndex != bezierPointList.Count - 1)
            {
                pathNormalProgress = (currentBezierPointProgress - pathPointList[currentPathIndex].BezierIndex) / (pathPointList[currentPathIndex + 1].BezierIndex - pathPointList[currentPathIndex].BezierIndex);
                float evaluatedValue = pathPointList[currentPathIndex].RotationCurve.Evaluate(pathNormalProgress);
                targetTransform.rotation = Quaternion.Lerp(pathPointList[currentPathIndex].Rotation, pathPointList[currentPathIndex + 1].Rotation, evaluatedValue);
                
                if (pathPointList[currentPathIndex + 1].BezierIndex <= currentBezierPointProgress)
                {
                    currentPathIndex++;
                }

                yield return null;
            }

            currentPathIndex = pathPointList.Count - 1;
            pathNormalProgress = 0f;

            _isApproximatelyRotationDone = true;
        }

        [ContextMenu("Find And Attatch")]
        protected void FindAndAttatchTransforms() // 잘 모르시겠다면 사용법에 대해서 곽명환에게 문의해주세요
        {
            Debug.LogFormat(LOG_FORMAT, "FindAndAttatchTransforms()");

            for (int i = 0; i < this.transform.childCount; i++)
            {
                GameObject childObj = this.transform.GetChild(i).gameObject;
                string _name = childObj.name;
                if (_name.Contains("PathPoint_") == true)
                {
                    string numberText = _name.Replace("PathPoint_", "");
                    int number = int.Parse(numberText);

                    while(pathPointList.Count < number + 1)
                    {
                        pathPointList.Add(new PathPoint());
                    }

                    Transform innerHandleT = childObj.transform.Find("InnerHandleObj");
                    if (innerHandleT == null)
                    {
                        innerHandleT = new GameObject("InnerHandleObj").transform;
                        innerHandleT.transform.parent = childObj.transform;
                        innerHandleT.transform.localPosition = Vector3.zero;
                    }

                    Transform outerHandleT = childObj.transform.Find("OuterHandleObj");
                    if (outerHandleT == null)
                    {
                        outerHandleT = new GameObject("OuterHandleObj").transform;
                        outerHandleT.transform.parent = childObj.transform;
                        outerHandleT.transform.localPosition = Vector3.zero;
                    }

                    pathPointList[number].This = childObj.transform;
                    pathPointList[number].InnerHandleTransform = innerHandleT;
                    pathPointList[number].OuterHandleTransform = outerHandleT;
                }
            }

            while (cubicBezierCurveList.Count < pathPointList.Count - 1)
            {
                cubicBezierCurveList.Add(new CubicBezierCurve());
            }

            for (int i = 0; i < cubicBezierCurveList.Count; i++)
            {
                cubicBezierCurveList[i].StartPointT = pathPointList[i].This;
                cubicBezierCurveList[i].StartHandlePointT = pathPointList[i].OuterHandleTransform;

                cubicBezierCurveList[i].EndPointT = pathPointList[i + 1].This;
                cubicBezierCurveList[i].EndHandlePointT = pathPointList[i + 1].InnerHandleTransform;              
            }

            GetBezierPointList();
        }

        protected void GetBezierPointList()
        {
            bezierPointList.Clear();
            for (int i = 0; i < cubicBezierCurveList.Count; i++)
            {
                cubicBezierCurveList[i]._BezierCurve = new BezierCurve(cubicBezierCurveList[i].StartPointT.position, cubicBezierCurveList[i].StartHandlePointT.position, cubicBezierCurveList[i].EndPointT.position, cubicBezierCurveList[i].EndHandlePointT.position, 30);
            
                float _length = cubicBezierCurveList[i]._BezierCurve.GetLength();
                int _count = (int)(_length * samplingThreshold);

                // ReInstantiate Curve to progress speed to Equally
                cubicBezierCurveList[i]._BezierCurve = new BezierCurve(cubicBezierCurveList[i].StartPointT.position, cubicBezierCurveList[i].StartHandlePointT.position, cubicBezierCurveList[i].EndPointT.position, cubicBezierCurveList[i].EndHandlePointT.position, _count);

                Vector3[] curvedPoints = cubicBezierCurveList[i]._BezierCurve.GetCurvedPoints();
                for (int j = -1; j < curvedPoints.Length; j++)
                {
                    if (j == -1)
                    {
                        pathPointList[i].BezierIndex = bezierPointList.Count;
                        bezierPointList.Add(cubicBezierCurveList[i].StartPointT.position);
                    }
                    else
                    {
                        bezierPointList.Add(curvedPoints[j]);
                    }
                }
            }

            pathPointList[pathPointList.Count - 1].BezierIndex = bezierPointList.Count;
            bezierPointList.Add(pathPointList[pathPointList.Count - 1].This.position);
        }

#if UNITY_EDITOR

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected PathTrackConfig pathTrackConfig;

        protected void OnDrawGizmos()
        {
            if (pathTrackConfig == null)
            {
                pathTrackConfig = Resources.Load("ScriptableObject/PathTrackConfig") as PathTrackConfig;
            }

            if (samplingThreshold < 0.001f)
            {
                samplingThreshold = 0.001f;
            }

            for (int i = 0; i < pathPointList.Count; i++)
            {
                Gizmos.color = pathTrackConfig.BezierColor;
                Gizmos.DrawSphere(pathPointList[i].InnerHandlePos, pathTrackConfig.BezierHandleSize);
                Gizmos.DrawSphere(pathPointList[i].OuterHandlePos, pathTrackConfig.BezierHandleSize);

                Gizmos.color = pathTrackConfig.PointMeshColor;
                Gizmos.DrawMesh(pathTrackConfig.PointMesh, 0, pathPointList[i].Position, pathPointList[i].Rotation);

                Gizmos.DrawLine(pathPointList[i].Position, pathPointList[i].InnerHandlePos);
                Gizmos.DrawLine(pathPointList[i].Position, pathPointList[i].OuterHandlePos);

                if (Selection.activeTransform == this.transform)
                {
                    GetBezierPointList();
                }
                else if (Selection.activeTransform == pathPointList[i].This)
                {
                    GetBezierPointList();
                }
                else if (Selection.activeTransform == pathPointList[i].InnerHandleTransform)
                {
                    pathPointList[i].OuterHandleTransform.position = Vector3.LerpUnclamped(pathPointList[i].InnerHandlePos, pathPointList[i].Position, 2f);

                    GetBezierPointList();
                }
                else if (Selection.activeTransform == pathPointList[i].OuterHandleTransform)
                {
                    pathPointList[i].InnerHandleTransform.position = Vector3.LerpUnclamped(pathPointList[i].OuterHandlePos, pathPointList[i].Position, 2f);

                    GetBezierPointList();
                }
            }

            Gizmos.color = pathTrackConfig.PathTrackGizmoColor;
            for (int i = 0; i < cubicBezierCurveList.Count; i++)
            {
                try
                {
                    Vector3[] curvedPoints = cubicBezierCurveList[i]._BezierCurve.GetCurvedPoints();
                }
                catch(Exception _e)
                {
                    if (_e is NullReferenceException)
                    {
                        GetBezierPointList();
                    }
                }
                finally
                {
                    Vector3[] curvedPoints = cubicBezierCurveList[i]._BezierCurve.GetCurvedPoints();

                    Gizmos.DrawLine(cubicBezierCurveList[i].StartPointT.position, curvedPoints[0]);
                    Gizmos.DrawLine(cubicBezierCurveList[i].EndPointT.position, curvedPoints[curvedPoints.Length - 1]);

                    for (int j = 0; j < curvedPoints.Length - 1; j++)
                    {
                        Gizmos.DrawLine(curvedPoints[j], curvedPoints[j + 1]);
                    }
                }
            }

            Gizmos.color = pathTrackConfig.SamplingPointColor;
            for (int i = 0; i < bezierPointList.Count; i++)
            {
                Gizmos.DrawSphere(bezierPointList[i], 0.1f);
            }
        }
#endif
    }

    [System.Serializable]
    public class PathPoint
    {
        public Transform This;

        [Space(10)]
        public Transform InnerHandleTransform;
        public Transform OuterHandleTransform;

        [Space(10)]
        public AnimationCurve RotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Space(10)]
        [ReadOnly]
        public int BezierIndex;

        public Vector3 Position
        {
            get
            {
                return This.position;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return This.rotation;
            }
        }

        public Vector3 InnerHandlePos
        {
            get
            {
                return InnerHandleTransform.position;
            }
        }

        public Vector3 OuterHandlePos
        {
            get
            {
                return OuterHandleTransform.position;
            }
        }
    }

    [System.Serializable]
    public class CubicBezierCurve
    {
        public Transform StartPointT;
        public Transform StartHandlePointT;

        [Space(10)]
        public Transform EndPointT;
        public Transform EndHandlePointT;

        public BezierCurve _BezierCurve;
    }
}