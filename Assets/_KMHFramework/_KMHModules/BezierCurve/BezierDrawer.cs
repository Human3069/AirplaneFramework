using UnityEngine;

namespace _KMH_Framework
{
    public class BezierDrawer : MonoBehaviour
    {
        [SerializeField]
        protected int pointCount = 10;

        [Space(10)]
        [SerializeField]
        protected Transform startTransform;
        [SerializeField]
        protected Transform startHandleTransform;
        [SerializeField]
        protected Transform endTransform;
        [SerializeField]
        protected Transform endHandleTransform;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float approximateLength;

        protected void OnDrawGizmos()
        {
            if (pointCount < 1)
            {
                pointCount = 1;
            }

            Vector3 startPoint = startTransform.position;
            Vector3 startHandlePoint = startHandleTransform.position;
            Vector3 endPoint = endTransform.position;
            Vector3 endHandlePoint = endHandleTransform.position;

            BezierCurve _bezierCurve = new BezierCurve(startPoint, startHandlePoint, endPoint, endHandlePoint, pointCount);

            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(startPoint, 1f);
            Gizmos.DrawSphere(startHandlePoint, 0.5f);

            Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
            Gizmos.DrawSphere(endPoint, 1f);
            Gizmos.DrawSphere(endHandlePoint, 0.5f);

            Gizmos.color = new Color(0f, 1f, 0f, 1f);
            Vector3[] _points = _bezierCurve.GetCurvedPoints();

            Gizmos.DrawLine(startPoint, _points[0]);
            Gizmos.DrawLine(endPoint, _points[_points.Length - 1]);
            for (int i = 0; i < _points.Length - 1; i++)
            {
                Gizmos.DrawLine(_points[i], _points[i + 1]);
            }

            approximateLength = _bezierCurve.GetLength();
        }
    }
}