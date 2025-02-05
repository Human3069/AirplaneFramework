using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public static class Vector3Ex
    {
        #region Get Float Value...
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 betweenLength = b - a;
            Vector3 currentLength = value - a;

            return Vector3.Dot(currentLength, betweenLength) / Vector3.Dot(betweenLength, betweenLength);
        }

        public static float GetAngleFromThreePositions(Vector3 pos1, Vector3 pos2, Vector3 pos3)
        {
            Vector3 L1 = pos1 - pos2;
            Vector3 L2 = pos3 - pos2;

            float dotProducts = (L1.normalized.x * L2.normalized.x) + (L1.normalized.y * L2.normalized.y) + (L1.normalized.z * L2.normalized.z);
            double angle = (Mathf.Acos(dotProducts) * 180.0) / Mathf.PI;
            float result = Mathf.Round((float)angle * 1000) / 1000;

            return result;
        }

        public static float GetDistanceTwoLine(Vector3 aStartPoint, Vector3 aEndPoint, Vector3 bStartPoint, Vector3 bEndPoint)
        {
            Vector3 _aLength = aStartPoint - aEndPoint;
            Vector3 _bLength = bStartPoint - bEndPoint;
            Vector3 _startToStartLength = aStartPoint - bStartPoint;

            Vector3 line = Vector3.Cross(_aLength, _bLength);

            return Mathf.Abs(Vector3.Dot(_startToStartLength, line)) / line.magnitude;
        }

        // horizontal => 1, vertical => 0, 45 degree => 0.5
        public static float GetHorizontalNormal(float eulerAngleX)
        {
            float angleX = eulerAngleX;
            angleX = angleX % 360f;

            if (angleX < 0f)
            {
                angleX += 360f;
            }

            if (angleX <= 90f)
            {
                return 1f - (angleX / 90f);
            }
            else if (angleX <= 270f)
            {
                return 1f - (angleX - 90f) / 180f;
            }
            else
            {
                return (angleX - 270f) / 90f;
            }
        }
        #endregion

        #region Get One Point...
        public static Vector3 RotateToAngle(Vector3 originDir, float angle)
        {
            return Vector3.RotateTowards(originDir, -originDir, angle * Mathf.PI / 180f, 0f);
        }

        public static Vector3 GetClosestPointOnFiniteLine(Vector3 targetPoint, Vector3 startPoint, Vector3 endPoint)
        {
            Vector3 _lineDirection = endPoint - startPoint;

            float _lineLength = _lineDirection.magnitude;
            _lineDirection.Normalize();

            float _dot = Vector3.Dot(targetPoint - startPoint, _lineDirection);
            float _projectLength = Mathf.Clamp(_dot, 0f, _lineLength);
            return startPoint + _lineDirection * _projectLength;
        }

        public static Vector3 GetPredictPosition(Vector3 myPos, Vector3 targetPos, Vector3 targetVelocity, float projectileSpeed)
        {
            float _magnitude = (targetPos - myPos).magnitude;
            Vector3 targetPredict = targetPos + (targetVelocity * _magnitude / projectileSpeed);

            return targetPredict;
        }
        #endregion

        #region Get Two Points...
        public static (Vector3 pointA, Vector3 pointB) GetShortestTwoPoint(Vector3 aStartPoint, Vector3 aEndPoint, Vector3 bStartPoint, Vector3 bEndPoint, bool isUnclamped = false)
        {
            Vector3 _aLength = aStartPoint - aEndPoint;
            Vector3 _bLength = bStartPoint - bEndPoint;

            // get right angle between two Line
            Vector3 _rightAngleOfAB = Vector3.Cross(_aLength, _bLength);

            Vector3 _crossedLineA = Vector3.Cross(_rightAngleOfAB, _aLength);
            Vector3 _crossedLineB = Vector3.Cross(_rightAngleOfAB, _bLength);

            Vector3 _pointADir = (aEndPoint - aStartPoint).normalized;
            Vector3 _pointBDir = (bEndPoint - bStartPoint).normalized;

            // Find intersection points
            Vector3 _pointA = aStartPoint + _pointADir * Vector3.Dot(_crossedLineB, bStartPoint - aStartPoint) / Vector3.Dot(_crossedLineB, _pointADir);
            Vector3 _pointB = bStartPoint + _pointBDir * Vector3.Dot(_crossedLineA, aStartPoint - bStartPoint) / Vector3.Dot(_crossedLineA, _pointBDir);

            if (isUnclamped == false)
            {
                // Clamp points over endPoints
                _pointA = Vector3.ClampMagnitude(_pointA - aStartPoint, _aLength.magnitude) + aStartPoint;
                _pointB = Vector3.ClampMagnitude(_pointB - bStartPoint, _bLength.magnitude) + bStartPoint;

                // Clamp points over startPoints
                _pointA = Vector3.ClampMagnitude(_pointA - aEndPoint, _aLength.magnitude) + aEndPoint;
                _pointB = Vector3.ClampMagnitude(_pointB - bEndPoint, _bLength.magnitude) + bEndPoint;
            }

            return (_pointA, _pointB);
        }

        public static (Vector3 pointA, Vector3 pointB) GetShortestTwoPoint(Ray rayA, Ray rayB)
        {
            Vector3 aStartPoint = rayA.origin;
            Vector3 aEndPoint = rayA.origin + rayA.direction;

            Vector3 bStartPoint = rayB.origin;
            Vector3 bEndPoint = rayB.origin + rayB.direction;

            return GetShortestTwoPoint(aStartPoint, aEndPoint, bStartPoint, bEndPoint, true);
        }
        #endregion
    }
}