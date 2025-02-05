using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    [RequireComponent(typeof(AudioSource))]
    public class FxSoundManager : MonoBehaviour // This Instance Dependant On 'FxSoundPoolController'
    {
        private const string LOG_FORMAT = "<color=white><b>[FxManager]</b></color> {0}";

        protected static FxSoundManager _instance;
        public static FxSoundManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected AudioSource thisAudioSource;

        [SerializeField]
        protected float _globalVolume = 1f;
        public float GlobalVolume
        {
            get
            {
                return _globalVolume;
            }
            set
            {
                _globalVolume = value;
            }
        }

        public delegate void ClipEnd();
        protected ClipEnd OnClipEnd;

        protected Dictionary<ClipEnd, Coroutine> coroutineDictionary = new Dictionary<ClipEnd, Coroutine>();

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Singleton Instance Overlapped!");
                Destroy(this.gameObject);
                return;
            }

            Debug.Assert(GlobalVolume == Mathf.Clamp01(GlobalVolume));

            thisAudioSource = this.gameObject.GetComponent<AudioSource>();
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public virtual void PlayOneShot(AudioClip clip, ClipEnd _onClipEnd = null)
        {
            PlayOneShot(clip, 1f, _onClipEnd);
        }

        public virtual void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            PlayOneShot(clip, volume, null);
        }

        public virtual void PlayOneShot(AudioClip clip, float volume, ClipEnd _onClipEnd = null)
        {
            thisAudioSource.PlayOneShot(clip, GlobalVolume * volume);

            if (_onClipEnd != null)
            {
                Coroutine newCoroutine = StartCoroutine(PostOnPlayOneShot(clip, _onClipEnd));
                coroutineDictionary.Add(_onClipEnd, newCoroutine);
            }
        }

        protected virtual IEnumerator PostOnPlayOneShot(AudioClip clip, ClipEnd _onClipEnd)
        {
            yield return new WaitForSeconds(clip.length);

            OnClipEnd = _onClipEnd;
            OnClipEnd();
            OnClipEnd = null;

            coroutineDictionary.Remove(_onClipEnd);
        }

        public virtual void PlayOneShotAtPoint(Vector3 position, float volume = 1f, ClipEnd _onClipEnd = null)
        {

        }
    }
}