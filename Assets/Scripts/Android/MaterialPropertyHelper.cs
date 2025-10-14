using System;
using UnityEngine;

namespace Citadel.Game
{
    public sealed class MaterialPropertyHelper
    {
        private readonly Renderer _renderer;
        private readonly MaterialPropertyBlock _materialPropertyBlock = new();

        public MaterialPropertyHelper(Renderer renderer) => _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

        public Texture GetTexture(string textureName) => HasValue(textureName) ? _materialPropertyBlock.GetTexture(textureName) : null;
        
        public bool HasValue (string propertyName) => _materialPropertyBlock.HasProperty(propertyName);

        public void SetFloat(string propertyName, float value)
        {
            _materialPropertyBlock.SetFloat(propertyName, value);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void SetColor(string propertyName, Color color)
        {
            _materialPropertyBlock.SetColor(propertyName, color);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }

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
        
        public static void SetEmissionColor (this MaterialPropertyHelper helper, Color value) => helper.SetColor("_EmissionColor", value);
    }
}