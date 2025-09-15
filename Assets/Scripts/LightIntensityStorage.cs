using UnityEngine;

namespace Citadel.Game
{
    internal sealed class LightIntensityStorage : MonoBehaviour
    {
        public float Intensity { get; set; }
        public bool WasEnabled { get; set; } = true;
        public bool Saved { get; set; }
    }
}