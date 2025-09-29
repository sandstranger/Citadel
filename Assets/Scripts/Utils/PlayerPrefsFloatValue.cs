using System;
using PlayerPrefs = Citadel.Game.PlayerPrefsExtensions;

namespace Citadel.Game
{
    internal sealed class PlayerPrefsFloatValue : PlayerPrefsValue<float>
    {
        public PlayerPrefsFloatValue(string playerPrefsKey, float defaultValue = 0.0f) : base(playerPrefsKey, defaultValue)
        {
        }

        public PlayerPrefsFloatValue(string playerPrefsKey, Action<float> onPlayerPrefsValueChanged, float defaultValue = 0.0f) : base(playerPrefsKey, onPlayerPrefsValueChanged, defaultValue)
        {
        }

        protected override float GetValue(string playerPrefsKey, float defaultValue) => PlayerPrefs.GetFloat(playerPrefsKey, defaultValue);

        protected override void SetValue(string playerPrefsKey, float value) => PlayerPrefs.SetFloat(playerPrefsKey, value);
    }
}