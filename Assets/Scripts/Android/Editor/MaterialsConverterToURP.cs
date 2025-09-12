using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Citadel.Editor.Utils;

namespace Citadel.Editor
{
    internal static class MaterialsConverterToURP
    {
        private const string StandardShaderName = "Standard";
        private const string StandardSpecularShaderName = "Standard (Specular setup)";
        private const string Gui3DTextShaderName = "GUI/3D Text Shader";
        private const string AutoMapMaskingShaderName = "Custom/AutomapMaskingFoW";
        private const string StandardTextureArrayShaderName = "Custom/StandardTextureArray";
        private const string CustomTransparentCutoutShaderName = "Custom/Unlit/Transparent Cutout";
        private const string CustomTransparentCutoutOverlayShaderName = "Custom/Unlit/Transparent Cutout Overlay";
        private const string TwoSidedSpecularShaderName = "Custom/TwoSidedTransparentSpecular";
        private const string GrassShaderName = "Deferred/Grass";
        private const string HighlightShaderName = "Custom/HighlightShader";
        private const string ViewWeaponsShaderName = "Custom/ViewWeapons";
        private const string WireframeShaderName = "Custom/Geometry/Wireframe";
        private const string WireframeOverlayShaderName = "Custom/Geometry/WireframeOverlay";
        private const string BumpDistortShaderName = "FX/Glass/Stained BumpDistort";
        private const string UnlitTransparentShaderName = "Unlit/Transparent";
        private const string UnlitTextureShaderName = "Unlit/Texture";
        private const string AlphaPremultiplyParticleShaderName = "Legacy Shaders/Particles/Alpha Blended Premultiply";
        private const string AdditiveParticleShaderName = "Legacy Shaders/Particles/Additive";
        private const string AlphaBlendedParticleShaderName = "Legacy Shaders/Particles/Alpha Blended";
        private const string VertexLitBlendedParticleShaderName = "Legacy Shaders/Particles/VertexLit Blended";
        private const string UnlitTransparentCutoutShaderName = "Unlit/Transparent Cutout";
        private const string MultiplyParticleShaderName = "Legacy Shaders/Particles/Multiply";
        private const string MobileParticleAdditiveShaderName = "Mobile/Particles/Additive";
        private const string MobileParticleMultiplyShaderName = "Mobile/Particles/Multiply";
        private const string StandardSurfaceParticleShaderName = "Particles/Standard Surface";
        private const string TransparentBumpedDiffuseShaderName = "Legacy Shaders/Transparent/Bumped Diffuse";
        
        private static readonly Lazy<IReadOnlyDictionary<string,Shader>> _urpShaders = new Lazy<IReadOnlyDictionary<string,Shader>> (
            () =>
            {
                var urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                var urpSimpleLitShader = Shader.Find("Universal Render Pipeline/Simple Lit");
                
                return new Dictionary<string, Shader>()
                {
                    { "Standard", urpLitShader },
                    { "Standard (Specular setup)", urpLitShader },
                    { "Unlit/Texture", Shader.Find("Universal Render Pipeline/Unlit") },
                    { "Unlit/Color", Shader.Find("Universal Render Pipeline/Unlit") },
                    { "Unlit/Transparent", Shader.Find("Universal Render Pipeline/Unlit") },
                    { "Unlit/Transparent Cutout", Shader.Find("Universal Render Pipeline/Unlit") },
                    { "Legacy Shaders/Diffuse", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Specular", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Bumped Diffuse", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Bumped Specular", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Parallax Diffuse", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Parallax Specular", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Transparent/Diffuse", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Transparent/Cutout/Diffuse", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Transparent/Specular", Shader.Find("Universal Render Pipeline/Lit") },
                    { "Legacy Shaders/Transparent/Cutout/Specular", Shader.Find("Universal Render Pipeline/Lit") }
                };
            });

        [MenuItem("Tools/Convert all materials to URP")]
        private static void ConvertAllMaterialsToURP()
        {
            var allMaterials = FindAllComponentsInProject<Material>();
            int convertedCount = 0;

            foreach (var material in allMaterials)
            {
                Debug.Log(material.shader.name);
                
                if (material.shader.name == "Standard")
                {
//                    material.shader = Shader.Find("Universal Render Pipeline/Lit");
                    convertedCount++;
                }
            }

            Debug.Log($"Converted {convertedCount} materials to URP");
        }
    }
}
