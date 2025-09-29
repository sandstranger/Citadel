namespace Citadel.Game
{
    internal sealed class AndroidConfig
    {
        public static AndroidConfig Default { get; } = new();
		
        private readonly PlayerPrefsBoolValue _enableLightsCulling = new("enable_lights_culling", true);
        private readonly PlayerPrefsFloatValue _lightsCullingMaxDistance = new("lighs_culling_max_distance", LightDistanceCuller.MaxDistance);

        public bool EnableLightsCulling
        {
            get => _enableLightsCulling.Value; 
            set => _enableLightsCulling.Value = value;
        }

        public float LightsCullingMaxDistance
        {
            get => _lightsCullingMaxDistance.Value;
            set => _lightsCullingMaxDistance.Value = value;
        }
    }
}