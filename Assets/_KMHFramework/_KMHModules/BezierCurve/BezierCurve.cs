using UnityEngine;

namespace _KMH_Framework
{
    public class BezierCurve
    {
        protected Vector3 _startPos;
        protected Vector3 _startHandlePos;
        protected Vector3 _endPos;
        protected Vector3 _endHandlePos;
        protected int _pointCount;

        protected Vector3[] resultPoints;
        protected int normalizeCount;

        public BezierCurve(Vector3 startPos, Vector3 startHandlePos, Vector3 endPos, Vector3 endHandlePos, int pointCount = 10)
        {
            this._startPos = startPos;
            this._startHandlePos = startHandlePos;
            this._endPos = endPos;
            this._endHandlePos = endHandlePos;
            this._pointCount = pointCount;
            if (_pointCount < 1)
            {
                _pointCount = 1;
                Debug.LogError("pointCount parameter Cannot Less then 1, pointCount set forcelly to 1");
            }

            resultPoints = new Vector3[pointCount];
            normalizeCount = _pointCount + 1;

            for (int i = 1; i < normalizeCount; i++)
            {
                float _normalized = (float)i / (float)normalizeCount;

                Vector3 basedStartPoint = Vector3.Lerp(_startPos, _startHandlePos, _normalized);
                Vector3 basedMiddlePoint = Vector3.Lerp(_startHandlePos, _endHandlePos, _normalized);
                Vector3 basedEndPoint = Vector3.Lerp(_endHandlePos, _endPos, _normalized);

                Vector3 intermediateStartPoint = Vector3.Lerp(basedStartPoint, basedMiddlePoint, _normalized);
                Vector3 intermediateEndPoint = Vector3.Lerp(basedMiddlePoint, basedEndPoint, _normalized);

                resultPoints[i - 1] = Vector3.Lerp(intermediateStartPoint, intermediateEndPoint, _normalized);
            }
        }

        public Vector3[] GetCurvedPoints()
        {
            return resultPoints;
        }

        // as it increases pointCount, able to get more accurate approximate length but increases computation
        public float GetLength()
        {
            float _length = 0f;
            for (int i = 0; i < normalizeCount; i++)
            {
                float pointToPointDistance;
                if (i == 0)
                {
                    pointToPointDistance = (_startPos - resultPoints[i]).magnitude;
                }
                else if (i == resultPoints.Length)
                {
                    pointToPointDistance = (resultPoints[i - 1] - _endPos).magnitude;
                }
                else
                {
                    pointToPointDistance = (resultPoints[i - 1] - resultPoints[i]).magnitude;
                }

                _length += pointToPointDistance;
            }

            return _length;
        }
    }
}