using UnityEngine;

namespace _KMH_Framework._Internal_Sound
{
    [System.Serializable]
    public class BlendableSound
    {
        [SerializeField]
        [Unity.Cinemachine.MinMaxRangeSlider(0f, BlendableSoundManager.MAX_DISTANCE)] // => you can use anothor MinMaxRangeSlider attribute or not
        private Vector2 range;
        [SerializeField]
        private AnimationCurve curve = AnimationCurve.Linear(0f, 1f, 1f, 0f); // time & value MUST in range (0 ~ 1)

        [Space(10)]
        [SerializeField]
        private AudioClip[] clips;

        public bool TryEvaluate(float distance, out (AudioClip, float) audioClipAndVolumePair)
        {
            if (distance >= range.x && distance <= range.y)
            {
                float normal = Mathf.InverseLerp(range.x, range.y, distance);
                float evaluated = curve.Evaluate(normal);

                int randomizedIndex = Random.Range(0, clips.Length);
                AudioClip randomizedClip = clips[randomizedIndex];

                audioClipAndVolumePair = (randomizedClip, evaluated);

                return true;
            }
            else
            {
                audioClipAndVolumePair = (null, -1f);
                return false;
            }
        }
    }
}