using System;

namespace Citadel.Game
{
    internal sealed class PlayerPrefsEnumValue<T> : PlayerPrefsValue<T> where T : struct
    {
        public PlayerPrefsEnumValue(string playerPrefsKey, T defaultValue = default) : base(playerPrefsKey, defaultValue)
        {
        }

        public PlayerPrefsEnumValue(string playerPrefsKey, Action<T> onPlayerPrefsValueChanged, T defaultValue = default) : 
            base(playerPrefsKey, onPlayerPrefsValueChanged, defaultValue)
        {
        }

        protected override T GetValue(string playerPrefsKey, T defaultValue) => PlayerPrefsExtensions.GetEnum(playerPrefsKey, defaultValue);

        protected override void SetValue(string playerPrefsKey, T value) => PlayerPrefsExtensions.SetEnum(playerPrefsKey, value);
    }
}