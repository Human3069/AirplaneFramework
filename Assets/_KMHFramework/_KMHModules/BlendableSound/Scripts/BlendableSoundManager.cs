using _KMH_Framework._Internal_Sound;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace _KMH_Framework
{
    public enum SoundType
    {
        Explosion,
        EnemyAirplaneExplosion,
        BulletRicochet,
        EnemyGunFire
    }

    public class BlendableSoundManager : MonoSingleton<BlendableSoundManager>
    {
        public const float SOUND_UNITY_PER_SEC = 340f; 
        public const float MAX_DISTANCE = 5000f;

        [SerializeField]
        protected Transform playerT;
        [SerializeField]
        protected AudioMixer mixer;

        [SerializeField]
        [SerializedDictionary("SoundType", "Controller")]
        protected SerializedDictionary<SoundType, BlendableSoundController> controllerDic = new SerializedDictionary<SoundType, BlendableSoundController>();

        protected void Awake()
        {
            if (playerT == null)
            {
                AudioListener listener = (AudioListener)FindFirstObjectByType(typeof(AudioListener));
                playerT = listener.transform;
            }

            foreach (KeyValuePair<SoundType, BlendableSoundController> pair in controllerDic)
            {
                GameObject childObj = new GameObject("Parent_" + pair.Key);
                childObj.transform.SetParent(this.transform);

                pair.Value.OnAwake(mixer, playerT, childObj.transform, pair.Key);
            }
        }

        public void PlaySound(SoundType type, Vector3 position)
        {
            StartCoroutine(controllerDic[type].PlaySoundRoutine(position));
        }
    }
}