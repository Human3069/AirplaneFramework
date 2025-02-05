using _KMH_Framework;
using _TW_Framework;
using FPS_Framework.Pool;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PredictDataBundle", menuName = "Scriptable Objects/PredictDataBundle")]
public class PredictDataBundle : ScriptableObject
{
    [SerializeField]
    private List<PredictableData> dataList = new List<PredictableData>();

    public static PredictDataBundle GetPredictData(ProjectileType type)
    {
        string resourceName = type.ToString() + "PredictData";
        PredictDataBundle predictData = Resources.Load<PredictDataBundle>(resourceName);

        return predictData;
    }

    private (PredictableData?, PredictableData?) GetPredictableTwoPointDatas(float distance)
    {
        if (distance > 3000f || distance < 0f)
        {
            return (null, null);
        }
        else
        {
            PredictableData overData = dataList.Find(data => data.Distance > distance);

            int moreThanIndex = dataList.IndexOf(overData);
            PredictableData lessData = dataList[moreThanIndex - 1];

            return (lessData, overData);
        }
    }

    public float? GetLinearProjectileSpeed(float distance)
    {
        if (distance > 3000f || distance < 0f)
        {
            return null;
        }
        else
        {
            (PredictableData? lessData, PredictableData? overData) = GetPredictableTwoPointDatas(distance);
            if (lessData == null || overData == null)
            {
                return null;
            }

            PredictableData firstData = dataList[0];

            float distanceNormal = Mathf.InverseLerp(lessData.Value.Distance, overData.Value.Distance, distance);
            float linearProjectileSpeed = (Mathf.Lerp(lessData.Value.SpeedOnHit, overData.Value.SpeedOnHit, distanceNormal) + firstData.SpeedOnHit) * 0.5f;

            return linearProjectileSpeed;
        }
    }

    public float? GetPredictedGravityHeightAmount(float firingAngleX, float distance)
    {
        (PredictableData? lessData, PredictableData? overData) = GetPredictableTwoPointDatas(distance);
        if (lessData == null || overData == null)
        {
            return null;
        }

        float distanceNormal = Mathf.InverseLerp(lessData.Value.Distance, overData.Value.Distance, distance);
        float heightAmount = Mathf.Lerp(lessData.Value.AdditionalHeight, overData.Value.AdditionalHeight, distanceNormal);

        float hNormal = Vector3Ex.GetHorizontalNormal(firingAngleX);

        return heightAmount * hNormal;
    }

    // Predicts velocity and gravity
    public Vector3? GetPredictedPosition(Vector3 firePos, float firingAngleX, Rigidbody targetRigidbody)
    {
        float distance = (targetRigidbody.centerOfMass - firePos).magnitude;
        float? lerpedSpeed = GetLinearProjectileSpeed(distance);
        if (lerpedSpeed == null)
        {
            return null;
        }

        Vector3 predictedVelocity = Vector3Ex.GetPredictPosition(firePos, targetRigidbody.centerOfMass, targetRigidbody.linearVelocity, lerpedSpeed.Value);
        float? heightAmount = GetPredictedGravityHeightAmount(firingAngleX, distance);
        if (heightAmount == null)
        {
            return null;
        }

        Vector3 predictedGravity = new Vector3(0f, heightAmount.Value, 0f);
        Vector3 predictPos = predictedVelocity + predictedGravity;
        return predictPos;
    }

    // Only Gravity Prediction
    public Vector3? GetPredictedPosition(Vector3 firePos, float firingAngleX, Vector3 targetPos)
    {
        float distance = (targetPos - firePos).magnitude;
        float? heightAmount = GetPredictedGravityHeightAmount(firingAngleX, distance);
        if (heightAmount == null)
        {
            return null;
        }
        
        Vector3 predictedGravity = new Vector3(0f, heightAmount.Value, 0f);
        Vector3 predictPos = targetPos + predictedGravity;
        return predictPos;
    }
}