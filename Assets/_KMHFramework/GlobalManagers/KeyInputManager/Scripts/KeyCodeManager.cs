using _KMH_Framework._Internal_KeyCode;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class KeyCodeManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[KeyCodeManager]</b></color> {0}";

        protected static KeyCodeManager _instance;
        public static KeyCodeManager Instance
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

        [SerializeField]
        protected KeyCodeDataBundle keyCodeDataBundle;

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Instance already exists. Destroying this instance.");
                Destroy(this.gameObject);
                return;
            }

            keyCodeDataBundle.OnAwake();
            AwakeAsync().Forget();
        }

        protected async UniTaskVoid AwakeAsync()
        {
            while (true)
            {
                foreach (KeyValuePair<KeyType, KeyCodeData> pair in keyCodeDataBundle.KeySettingDic)
                {
                    pair.Value.OnUpdate();
                }

                await UniTask.Yield();
            }
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        internal KeyCodeData GetData(KeyType type)
        {
            Dictionary<KeyType, KeyCodeData> settingDic = keyCodeDataBundle.KeySettingDic;
            if (settingDic.ContainsKey(type) == false)
            {
                Debug.Assert(false);
                return null;
            }
            else
            {
                return settingDic[type];
            }
        }
    }
}