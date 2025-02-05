using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public static class Predictor
    {
        public static List<Vector3> PredictWithoutCollision(float drag, Vector3 velocity, out float lineDistance, Vector3 startPos, Vector3? gravity = null, float accuracy = 0.9f, int iterationLimit = 150)
        {
            Vector3 directionWithLength = Vector3.zero;
            Vector3 toPos;
;
            Vector3 _gravity = gravity == null ? Physics.gravity : gravity.Value;

            bool isDone = false;

            int iterated = 0;
            lineDistance = 0f;

            float compAcc = 1f - accuracy;
            Vector3 gravAdd = _gravity * compAcc;
            float dragMult = Mathf.Clamp01(1f - drag * compAcc);
            List<Vector3> predictionPointList = new List<Vector3>();

            while (isDone == false && iterated < iterationLimit)
            {
                velocity += gravAdd;
                velocity *= dragMult;

                toPos = startPos + velocity * compAcc;
                directionWithLength = toPos - startPos;
                predictionPointList.Add(startPos);

                float distance = Vector3.Distance(startPos, toPos);
                lineDistance += distance;

                startPos = toPos;
                iterated++;
            }

            return predictionPointList;
        }

        public static Vector3 PredictWithSingleCollision(float drag, Vector3 velocity, LayerMask raycastMask, out float lineDistance, Vector3 startPos, Vector3? gravity = null, float accuracy = 0.9f, int iterationLimit = 150, bool isDrawRay = false)
        {
            Vector3 directionWithLength = Vector3.zero;
            Vector3 toPos;

            Vector3 _gravity = gravity == null ? Physics.gravity : gravity.Value;

            bool isDone = false;

            int iterated = 0;
            lineDistance = 0f;

            float compAcc = 1f - accuracy;
            Vector3 gravAdd = _gravity * compAcc;
            float dragMult = Mathf.Clamp01(1f - drag * compAcc);

            Vector3 hitPoint = Vector3.zero;
            while (isDone == false && iterated < iterationLimit)
            {
                velocity += gravAdd;
                velocity *= dragMult;

                toPos = startPos + velocity * compAcc;
                directionWithLength = toPos - startPos;

                hitPoint = startPos;

                float distance = Vector3.Distance(startPos, toPos);
                lineDistance += distance;

                Ray ray = new Ray(startPos, directionWithLength);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, distance, raycastMask) == true)
                {
                    return hit.point;
                }

                if (isDrawRay == true)
                {
                    Debug.DrawRay(startPos, directionWithLength, Color.red, Time.deltaTime);
                }
                
                startPos = toPos;
                iterated++;
            }

            return hitPoint;
        }

        public static void Predict(Rigidbody rigidbody, Vector3 _velocity, LayerMask raycastMask, out float lineDistance, Vector3 startPos, Vector3? gravity = null, bool isCheckForCollision = true, bool isBounceOnCollision = false, float accuracy = 0.98f, int iterationLimit = 150)
        {
            Vector3 directionWithLength = Vector3.zero;
            Vector3 toPos;

            float _drag = rigidbody.linearDamping;
            Vector3 _gravity = gravity == null ? Physics.gravity : gravity.Value;

            bool isDone = false;

            int iterated = 0;
            lineDistance = 0f;

            float compAcc = 1f - accuracy;
            Vector3 gravAdd = _gravity * compAcc;
            float dragMult = Mathf.Clamp01(1f - _drag * compAcc);
            List<Vector3> predictionPointList = new List<Vector3>();

            while (isDone == false && iterated < iterationLimit)
            {
                _velocity += gravAdd;
                _velocity *= dragMult;

                toPos = startPos + _velocity * compAcc;
                directionWithLength = toPos - startPos;
                predictionPointList.Add(startPos);

                float distance = Vector3.Distance(startPos, toPos);
                lineDistance += distance;

                if (isCheckForCollision == true)
                {
                    Ray ray = new Ray(startPos, directionWithLength);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, distance, raycastMask) == true)
                    {
                        predictionPointList.Add(hit.point);
                        if (isBounceOnCollision == true)
                        {
                            Collider col = rigidbody.GetComponent<Collider>();
                            if (col != null)
                            {
                                PhysicsMaterial physicsMaterial = col.sharedMaterial;
                                if (physicsMaterial != null)
                                {
                                    toPos = hit.point;
                                    _velocity = Vector3.Reflect(_velocity, hit.normal) * physicsMaterial.bounciness;
                                }
                            }
                        }
                        else
                        {
                            isDone = true;
                        }
                    }
                }

                Debug.DrawRay(startPos, directionWithLength, Color.red, Time.deltaTime);

                startPos = toPos;
                iterated++;
            }
        }
    }
}