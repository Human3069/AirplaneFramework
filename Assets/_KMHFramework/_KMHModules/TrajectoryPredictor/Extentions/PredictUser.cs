using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictUser : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody _rigidbody;
    [SerializeField]
    protected LayerMask _raycastMask;

    [Space(10)]
    [ReadOnly]
    [SerializeField]
    protected float _distance;
    [SerializeField]
    protected float accuracy = 0.9f;
    [SerializeField]
    protected int iterationLimit = 150;

    protected void OnDrawGizmos()
    {
        Vector3 predictPos = Predictor.PredictWithSingleCollision(_rigidbody.linearDamping, this.transform.forward * 20f, _raycastMask, out _distance, this.transform.position, accuracy : accuracy, iterationLimit : iterationLimit);
        List<Vector3> predictPosList = Predictor.PredictWithoutCollision(_rigidbody.linearDamping, this.transform.forward * 20f, out _, this.transform.position, accuracy : accuracy, iterationLimit: iterationLimit);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(predictPos, 1f);

        Gizmos.color = Color.red;
        for (int i = 0; i < predictPosList.Count; i++)
        {
            if (i != 0)
            {
                Gizmos.DrawLine(predictPosList[i - 1], predictPosList[i]);
            }
        }
    }
}
