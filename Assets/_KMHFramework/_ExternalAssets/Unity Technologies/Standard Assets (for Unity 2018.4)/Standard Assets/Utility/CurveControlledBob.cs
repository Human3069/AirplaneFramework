using System;
using System.Xml.XPath;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;


namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class CurveControlledBob
    {
        public float HorizontalBobRange = 0.33f;
        public float VerticalBobRange = 0.33f;
        public float AxialBobRange = 0.33f;

        [Space(10)]
        public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                            new Keyframe(2f, 0f)); // sin curve for head bob

        [Space(10)]
        public float VerticaltoHorizontalRatio = 1f;
        public float AxialToHorizontalRatio = 1f;

        protected float m_CyclePositionX;
        protected float m_CyclePositionY;
        protected float m_CyclePositionZ;

        protected float m_BobBaseInterval;
        protected Vector3 m_OriginalCameraPosition;
        protected float m_Time;

        public virtual void Setup(Camera camera, float bobBaseInterval)
        {
            m_BobBaseInterval = bobBaseInterval;
            m_OriginalCameraPosition = camera.transform.localPosition;

            // get the length of the curve in time
            m_Time = Bobcurve[Bobcurve.length - 1].time;
        }

        public virtual Vector3 DoHeadBob(float speed)
        {
            float xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRange);
            float yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRange);

            m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
            m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticaltoHorizontalRatio;

            if (m_CyclePositionX >= VerticaltoHorizontalRatio)
            {
                m_CyclePositionX -= VerticaltoHorizontalRatio;
            }
            if (m_CyclePositionY >= VerticaltoHorizontalRatio)
            {
                m_CyclePositionY -= VerticaltoHorizontalRatio;
            }

            return new Vector3(xPos, yPos, 0f);
        }

        public virtual Vector3[] DoHeadBobEx(float speed)
        {
            float xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRange);
            float yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRange);
            float zPos = m_OriginalCameraPosition.z + (Bobcurve.Evaluate(m_CyclePositionZ) * AxialBobRange);

            m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
            m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticaltoHorizontalRatio;
            m_CyclePositionZ += ((speed * Time.deltaTime) / m_BobBaseInterval) * AxialToHorizontalRatio;

            if (m_CyclePositionX >= 2.0f)
            {
                m_CyclePositionX -= 2.0f;
            }
            if (m_CyclePositionY >= 2.0f)
            {
                m_CyclePositionY -= 2.0f;
            }
            if (m_CyclePositionZ >= 2.0f)
            {
                m_CyclePositionZ -= 2.0f;
            }

            Vector3[] posAndAngle = { new Vector3(xPos, yPos, zPos), new Vector3(xPos, yPos, zPos) };
            return posAndAngle;
        }
    }
}
