using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

namespace _KMH_Framework
{
    public enum KeyType
    {
        // Pitch_Up,
        // Pitch_Down,
        // Yaw_Left,
        // Yaw_Right,
        Roll_Left,
        Roll_Right,

        Throttle_Up,
        Throttle_Down,

        Toggle_Boot,
        Toggle_Flap,
        Toggle_Landing_Gear,

        Look_Around,

        Fire_Weapon,
    }

    public static class KeyTypeUtility
    {
        public static bool IsInput(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return false;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).IsInput;
            }
        }

        public static bool IsInputDown(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return false;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).IsInputDown;
            }
        }

        public static bool ToggleValue(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return false;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).ToggleValue;
            }
        }

        public static void RegisterEvent(this KeyType type, Action<bool> action)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).RegisterEvent(action);
            }
        }

        public static IEnumerator RegisterEventRoutine(this KeyType type, Action<bool> action)
        {
            yield return new WaitWhile(() => KeyCodeManager.Instance == null);

            KeyCodeManager.Instance.GetData(type).RegisterEvent(action);
        }

        public static async UniTask RegisterEventAsync(this KeyType type, Action<bool> action)
        {
            await UniTask.WaitWhile(() => KeyCodeManager.Instance == null);

            KeyCodeManager.Instance.GetData(type).RegisterEvent(action);
        }

        public static void UnregisterEvent(this KeyType type, Action<bool> action)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).UnregisterEvent(action);
            }
        }

        public static void UpdateLock(this KeyType type, bool isLock)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).UpdateLock(isLock);
            }
        }

        public static void SetToggleValue(this KeyType type, bool isOn)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).SetToggleValue(isOn);
            }
        }

        public static string GetKeyName(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return null;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).KeyCodeName;
            }
        }
    }
}