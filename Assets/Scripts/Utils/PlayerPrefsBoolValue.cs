using System;
using PlayerPrefs = Citadel.Game.PlayerPrefsExtensions;

namespace Citadel.Game
{
    internal sealed class PlayerPrefsBoolValue : PlayerPrefsValue<bool>
    {
        public PlayerPrefsBoolValue(string playerPrefsKey, bool defaultValue = false) : base(playerPrefsKey, defaultValue)
        {
        }

        public PlayerPrefsBoolValue(string playerPrefsKey, Action<bool> onPlayerPrefsValueChanged, bool defaultValue = false) :
            base(playerPrefsKey, onPlayerPrefsValueChanged, defaultValue)
        {
        }

        protected override bool GetValue(string playerPrefsKey, bool defaultValue) => PlayerPrefs.GetBool(playerPrefsKey, defaultValue);

        protected override void SetValue(string playerPrefsKey, bool value) => PlayerPrefs.SetBool(playerPrefsKey, value);
    }
}