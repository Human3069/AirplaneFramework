using _KMH_Framework;
using UnityEngine;

namespace _TW_Framework
{
    [System.Serializable]
    public struct PredictableData
    {
        public float Distance;
        public float Time;
        public float SpeedOnHit;

        [Space(10)]
        public float AdditionalHeight;

        public PredictableData(float distance, float time, float speedOnHit, float additionalHeight)
        {
            Distance = distance;
            Time = time;
            SpeedOnHit = speedOnHit;

            AdditionalHeight = additionalHeight;
        }
    }
}