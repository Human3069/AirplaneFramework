using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace _KMH_Framework._Internal_Sound
{
    [System.Serializable]
    public class BlendableSoundController
    {
        [SerializeField]
        private int poolCount = 100;

        [Space(10)]
        [SerializeField]
        private BlendableSound[] blendableSounds;

        private AudioMixer mixer;
        private Transform playerT;
        private Transform baseT;

        private Queue<AudioSource> poolQueue = new Queue<AudioSource>();
        private SoundType soundType;

        internal void OnAwake(AudioMixer _mixer, Transform _playerT, Transform _baseT, SoundType _soundType)
        {
            this.mixer = _mixer;
            this.playerT = _playerT;
            this.baseT = _baseT;
            this.soundType = _soundType;

            // create a pool of audio sources for later use amount of poolCount
            for (int i = 0; i < poolCount; i++)
            {
                AudioSource poolAudioSource = InstantiatePool();
                poolAudioSource.gameObject.SetActive(false);

                poolQueue.Enqueue(poolAudioSource);
            }
        }

        internal IEnumerator PlaySoundRoutine(Vector3 targetPosition)
        {
            float timer = 0f;
            float distance = Vector3.Distance(playerT.position, targetPosition);
            float delay = distance / BlendableSoundManager.SOUND_UNITY_PER_SEC;

            while (timer <= delay)
            {
                timer += Time.deltaTime;

                distance = Vector3.Distance(playerT.position, targetPosition);
                delay = distance / BlendableSoundManager.SOUND_UNITY_PER_SEC;
                
                yield return null;
            }

            // if the pool is empty, create a new audio source and enable it
            if (poolQueue.TryDequeue(out AudioSource pooledAudioSource) == false)
            {
                pooledAudioSource = InstantiatePool();
            }
            pooledAudioSource.gameObject.SetActive(true);

            List<(AudioClip, float)> clipNormalList = EvaluateClipAndVolumeList(distance);
            float longestClipLength = 0f;

            // play each audio clip in the list and find the longest clip length
            foreach ((AudioClip clip, float normal) in clipNormalList)
            {
                pooledAudioSource.PlayOneShot(clip, normal);
                if (clip.length > longestClipLength)
                {
                    longestClipLength = clip.length;
                }
            }

            // wait for the longest clip to finish playing before returning the audio source to the pool
            yield return new WaitForSeconds(longestClipLength);

            // return the audio source to the pool
            pooledAudioSource.gameObject.SetActive(false);
            poolQueue.Enqueue(pooledAudioSource);
        }

        private AudioSource InstantiatePool()
        {
            GameObject poolObj = new GameObject("Controller_" + this.soundType);
            poolObj.transform.SetParent(baseT);

            AudioSource poolAudioSource = poolObj.AddComponent<AudioSource>();
            poolAudioSource.outputAudioMixerGroup = this.mixer.FindMatchingGroups("Fx")[0];
            
            return poolAudioSource;
        }

        private List<(AudioClip, float)> EvaluateClipAndVolumeList(float distance)
        {
            List<(AudioClip, float)> audioClipAndVolumePairList = new List<(AudioClip, float)>();
            for (int i = 0; i < blendableSounds.Length; i++)
            {
                if (blendableSounds[i].TryEvaluate(distance, out (AudioClip, float) audioClipAndVolumePair) == true)
                {
                    audioClipAndVolumePairList.Add(audioClipAndVolumePair);
                }
            }

            return audioClipAndVolumePairList;
        }
    }
}