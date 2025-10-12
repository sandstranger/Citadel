using UnityEngine;

namespace Citadel.Game
{
    internal sealed class MaterialPropertyHelper
    {
        private readonly Renderer _renderer;
        private readonly MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();

        public MaterialPropertyHelper(Renderer renderer) => _renderer = renderer;

        public Texture GetTexture(string textureName) => HasValue(textureName) ? _materialPropertyBlock.GetTexture(textureName) : null;
        
        public bool HasValue (string propertyName) => _materialPropertyBlock.HasProperty(propertyName);

        public void SetTexture(string propertyName, Texture value)
        {
            _materialPropertyBlock.SetTexture(propertyName, value);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }

    internal static class MaterialPropertyHelperExtensions
    {
        public static Texture GetMainTexture (this MaterialPropertyHelper helper) => helper.GetTexture("_MainTex");

        public static void SetMainTexture(this MaterialPropertyHelper helper, Texture texture) => helper.SetTexture("_MainTex", texture);
        
        public static void SetEmissionTexture (this MaterialPropertyHelper helper, Texture texture) => 
            helper.SetTexture("_EmissionMap", texture);
    }
}