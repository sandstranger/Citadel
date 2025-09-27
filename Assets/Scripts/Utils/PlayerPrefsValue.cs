using System;

namespace Citadel.Game
{
    internal abstract class PlayerPrefsValue <T>
    {
        private readonly string _playerPrefsKey;
        private readonly Action<T> _onPlayerPrefsValueChanged;
        private readonly T _defaultValue;

        private T _savedValue;
        private bool _hasValue;

        protected PlayerPrefsValue(string playerPrefsKey, T defaultValue = default )
        {
            _playerPrefsKey = playerPrefsKey;
            _defaultValue = defaultValue;
        }
        
        protected PlayerPrefsValue(string playerPrefsKey, Action<T> onPlayerPrefsValueChanged, T defaultValue = default)
        {
            _playerPrefsKey = playerPrefsKey;
            _onPlayerPrefsValueChanged = onPlayerPrefsValueChanged;
            _defaultValue = defaultValue;
        }

        public T Value
        {
            get
            {
                if (!_hasValue)
                {
                    _savedValue = GetValue(_playerPrefsKey, _defaultValue);
                }

                return _savedValue;
            }
            set
            {
                _hasValue = true;
                _savedValue = value;
                SetValue(_playerPrefsKey, value);
                _onPlayerPrefsValueChanged?.Invoke(value);
            }
        }

        protected abstract T GetValue(string playerPrefsKey, T defaultValue);
        protected abstract void SetValue(string playerPrefsKey, T value);
    }
}