using System;
using UnityEngine;


namespace _KMH_Framework._Internal_KeyCode
{
    public enum EventType
    {
        Click_Down = 0,
        Toggle_Down = 1,
    }

    [Serializable]
    public class KeyCodeData
    {
        [SerializeField]
        private KeyCode keyCode = KeyCode.None;
        public string KeyCodeName
        {
            get
            {
                return keyCode.ToString();
            }
        }

        [SerializeField]
        private EventType eventType = EventType.Click_Down;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        private bool _isInput = false;
        internal bool IsInput
        {
            get
            {
                return _isInput;
            }
            set
            {
                if (_isInput != value && _isLock == false)
                {
                    _isInput = value;

                    if (eventType == EventType.Click_Down)
                    {
                        OnClick?.Invoke(value);
                    }
                    else if (eventType == EventType.Toggle_Down)
                    {
                        if (value == true)
                        {
                            _toggleValue = !_toggleValue;
                            OnValueChanged?.Invoke(_toggleValue);
                        }
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }

        internal bool IsInputDown
        {
            get
            {
                return Input.GetKeyDown(keyCode);
            }
        }

        [ReadOnly]
        [SerializeField]
        private bool _toggleValue = false;
        internal bool ToggleValue
        {
            get
            {
                return _toggleValue;
            }
        }

        private bool _isLock = false;

        private Action<bool> OnClick;
        private Action<bool> OnValueChanged;

        internal void OnAwake()
        {
            _isInput = false;
            _toggleValue = false;
            _isLock = false;
        }

        internal void OnUpdate()
        {
            IsInput = Input.GetKey(keyCode);
        }

        internal void RegisterEvent(Action<bool> action)
        {
            if (eventType == EventType.Click_Down)
            {
                OnClick += action;
            }
            else if (eventType == EventType.Toggle_Down)
            {
                OnValueChanged += action;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        internal void UnregisterEvent(Action<bool> action)
        {
            if (eventType == EventType.Click_Down)
            {
                OnClick -= action;
            }
            else if (eventType == EventType.Toggle_Down)
            {
                OnValueChanged -= action;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        internal void SetToggleValue(bool isOn)
        {
            if (eventType == EventType.Toggle_Down)
            {
                bool isChanged = _toggleValue != isOn;
                _toggleValue = isOn;

                if (isChanged == true)
                {
                    OnValueChanged?.Invoke(isOn);
                }
            }
            else
            {
                Debug.Assert(false, "cannot set value to non-toggle type event");
            }
        }

        internal void UpdateLock(bool isLock)
        {
            this._isLock = isLock;
        }
    }
}