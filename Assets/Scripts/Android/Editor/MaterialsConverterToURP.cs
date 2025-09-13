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
        private const string MobileDiffuseShaderName = "Mobile/Diffuse";
        
        private static readonly Lazy<IReadOnlyDictionary<string,Shader>> _urpShaders = new( () =>
            {
                var urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                var urpSimpleLitShader = Shader.Find("Universal Render Pipeline/Simple Lit");
                var urpUnlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                var urpParticleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                
                return new Dictionary<string, Shader>
                {
                    { StandardShaderName, urpLitShader },
                    { StandardSpecularShaderName, urpLitShader },
                    { Gui3DTextShaderName, Shader.Find("Custom/URP3DTextShader") },
                    { AutoMapMaskingShaderName, Shader.Find("Custom/URPAutomapMaskingFoW") },
                    { StandardTextureArrayShaderName, Shader.Find("Custom/URPTextureArray") },
                    { CustomTransparentCutoutShaderName, urpUnlitShader },
                    { CustomTransparentCutoutOverlayShaderName, urpUnlitShader },
                    { TwoSidedSpecularShaderName, Shader.Find("Custom/URPTwoSidedTransparentSpecular") },
                    { GrassShaderName, Shader.Find("Custom/URPGrass") },
                    { HighlightShaderName, Shader.Find("Custom/URPHighlightShader") },
                    { ViewWeaponsShaderName, Shader.Find("Custom/URPViewWeapons") },
                    { WireframeShaderName, Shader.Find("Custom/URPWireframe") },
                    { WireframeOverlayShaderName, Shader.Find("Custom/URPWireframeOverlay") },
                    { BumpDistortShaderName, Shader.Find("Custom/URPStained BumpDistort") },
                    { UnlitTransparentShaderName, urpUnlitShader },
                    { UnlitTextureShaderName, urpUnlitShader },
                    { AlphaPremultiplyParticleShaderName, urpParticleShader },
                    { AdditiveParticleShaderName, urpParticleShader },
                    { AlphaBlendedParticleShaderName, urpParticleShader },
                    { VertexLitBlendedParticleShaderName, urpParticleShader },
                    { UnlitTransparentCutoutShaderName, urpUnlitShader },
                    { MultiplyParticleShaderName, urpParticleShader },
                    { MobileParticleAdditiveShaderName, urpParticleShader },
                    { MobileParticleMultiplyShaderName, urpParticleShader },
                    { StandardSurfaceParticleShaderName, urpParticleShader },
                    { TransparentBumpedDiffuseShaderName, urpSimpleLitShader },
                    { MobileDiffuseShaderName, urpSimpleLitShader },
                };
            });

        [MenuItem("Tools/Convert all materials to URP")]
        private static void ConvertAllMaterialsToURP()
        {
            var allMaterials = FindAllComponentsInProject<Material>();
            List<string> convertedMaterials = new(allMaterials.Count);
            
            foreach (var material in allMaterials)
            {
                var enableGpuInstancing = material.enableInstancing;
                var oldShaderName = material.shader.name;
                
                switch (material.shader.name)
                {
                    case MobileDiffuseShaderName:
                        var texture = material.GetTexture("_MainTex");
                        material.shader = _urpShaders.Value[MobileDiffuseShaderName];
                        material.SetTexture("_BaseMap", texture);
                        material.DisableEmission();
                        break;
                    case TransparentBumpedDiffuseShaderName:
                        ConvertLegacyBumpedDiffuseToURP(material);
                        break;
                    case StandardShaderName:
                        ConvertStandardShaderToURP(material);
                        break;
                    case StandardSpecularShaderName:
                        ConvertStandartSpecularShaderToURP(material);
                        break;
                    case Gui3DTextShaderName:
                    case AutoMapMaskingShaderName:
                    case StandardTextureArrayShaderName:   
                    case ViewWeaponsShaderName:
                    case WireframeOverlayShaderName:
                        material.shader = _urpShaders.Value[oldShaderName];
                        break;
                    case HighlightShaderName:
                        var rimPower = material.GetFloat("_RimPower") - 1.0f;
                        material.shader = _urpShaders.Value[oldShaderName];
                        material.SetFloat("_RimPower", rimPower);
                        break;
                    case GrassShaderName:
                        ConvertGrassShaderToURP(material);
                        break;
                    default:
                        break;
                }
                
                if (_urpShaders.Value.ContainsKey(oldShaderName))
                {
                    convertedMaterials.Add(material.name);
                    material.enableInstancing = enableGpuInstancing;
                    try
                    {
                        material.SetFloat("_XRMotionVectorsPass", 0.0f);
                    }
                    catch
                    {
                    }
                }
            }

            if (convertedMaterials.Count > 0)
            {
                Debug.Log($"Converted {string.Join(",", convertedMaterials)} materials to URP");
                AssetDatabase.Refresh();
            }
        }
        
        [MenuItem("Tools/Disable GPU Instancing on all materials")]
        private static void DisableGPUInstancing()
        {
            foreach (var material in FindAllComponentsInProject<Material>())
            {
                material.enableInstancing = false;
            }
        }
        
        [MenuItem("Tools/Change Global Illumination from Realtime to Baked on all materials")]
        private static void ChangeGlobalIlluminationFromRealtimeToBaked()
        {
            foreach (var material in FindAllComponentsInProject<Material>())
            {
                if (material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.RealtimeEmissive))
                {
                    material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.BakedEmissive;
                }
            }
        }

        private static void ConvertGrassShaderToURP(Material material)
        {
            var grassBaseColor = material.GetColor("_Color");
            var minBladeWidth = material.GetFloat("_BladeWidthRandom");
            var maxBladeWidth = material.GetFloat("_BladeWidth");
            var minBladeHeight = material.GetFloat("_BladeHeightRandom");
            var maxBladeHeight = material.GetFloat("_BladeHeight");
            var bladeForward = material.GetFloat("_BladeForward");
            var bladeCurve = material.GetFloat("_BladeCurve");
            var bendRotation = material.GetFloat("_BendRotationRandom");
            
            material.shader = _urpShaders.Value[material.shader.name];
            material.SetColor("_BaseColor", grassBaseColor);
            material.SetColor("_TipColor", grassBaseColor);
            material.SetFloat("_BladeWidthMin", minBladeWidth);
            material.SetFloat("_BladeWidthMax", maxBladeWidth);
            material.SetFloat("_BladeHeightMin", minBladeHeight);
            material.SetFloat("_BladeHeightMax", maxBladeHeight);
            material.SetFloat("_BladeBendDistance", bladeForward);
            material.SetFloat("_BladeBendCurve", bladeCurve);
            material.SetFloat("_BendDelta", bendRotation);
            material.SetFloat("_TessellationGrassDistance", 0.2f);
            material.SetFloat("_WindFrequency", 0.0f);
        }
        
        private static void ConvertLegacyBumpedDiffuseToURP(Material material)
        {
            var texture = material.GetTexture("_MainTex");
            var color = material.GetColor("_Color");
            var bumpMap = material.GetTexture("_BumpMap");
            material.shader = _urpShaders.Value[material.shader.name];
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", color);
            material.SetTexture("_BumpMap", bumpMap);
            material.SetFloat("_Surface", 1.0f);
            material.SetFloat("_BlendModePreserveSpecular", 0.0f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableEmission();
        }
        
        private static void ConvertStandardShaderToURP(Material material)
        {
            StandardShaderRenderingMode renderingMode = (StandardShaderRenderingMode)Convert.ToUInt32(material.GetFloat("_Mode"));
                    
            var materialIllumination = material.globalIlluminationFlags;
            var texture = material.GetTexture("_MainTex");
            var normalMap = material.GetTexture("_BumpMap");
            var color = material.GetColor("_Color");
            var metallic = material.GetFloat("_Metallic");
            var smoothness = material.GetFloat("_Glossiness");
            var emissionColor = material.GetColor("_EmissionColor");
            var glossyReflections = material.GetFloat("_GlossyReflections");
            var specularHighlights = material.GetFloat("_SpecularHighlights");
            var cutoff = material.GetFloat("_Cutoff");
            
            material.shader = _urpShaders.Value[StandardShaderName];
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", emissionColor);
            material.SetFloat("_GlossyReflections", glossyReflections);
            material.SetFloat("_SpecularHighlights", specularHighlights);
            material.SetTexture("_BumpMap", normalMap);
            material.globalIlluminationFlags = materialIllumination;
            
            if (normalMap != null)
            {
                material.SetFloat("_BumpScale", 1.0f);
            }
            
            if (renderingMode is StandardShaderRenderingMode.Cutout)
            {
                material.SetFloat("_Surface", 1.0f);
                material.SetFloat("_AlphaClip", 1.0f);
                material.SetFloat("_Blend", 0.0f);
                material.SetFloat("_Cutoff", cutoff); 
                material.EnableKeyword("_ALPHATEST_ON");
            }
            else if (renderingMode is StandardShaderRenderingMode.Fade or StandardShaderRenderingMode.Transparent)
            {
                material.SetFloat("_Surface", 1.0f); 
                material.SetFloat("_Blend", 0.0f);
                material.EnableKeyword("_ALPHATEST_ON");
            }
        }
        
        private static void ConvertStandartSpecularShaderToURP(Material material)
        {
            StandardShaderRenderingMode renderingMode = (StandardShaderRenderingMode)Convert.ToUInt32(material.GetFloat("_Mode"));
                    
            var materialIllumination = material.globalIlluminationFlags;            
            var texture = material.GetTexture("_MainTex");
            var normalMap = material.GetTexture("_BumpMap");
            var specularMap = material.GetTexture("_SpecGlossMap");
            var color = material.GetColor("_Color");
            var specularColor = material.GetColor("_SpecColor");
            var smoothness = material.GetFloat("_Glossiness");
            var emissionColor = material.GetColor("_EmissionColor");
            var glossyReflections = material.GetFloat("_GlossyReflections");
            var specularHighlights = material.GetFloat("_SpecularHighlights");
            var cutoff = material.GetFloat("_Cutoff");
                    
            material.shader = _urpShaders.Value[StandardSpecularShaderName];
            material.SetFloat("_WorkflowMode", 0.0f); 
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", color);
            material.SetColor("_SpecColor", specularColor);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", emissionColor);
            material.SetFloat("_GlossyReflections", glossyReflections);
            material.SetFloat("_SpecularHighlights", specularHighlights);
            material.SetTexture("_BumpMap", normalMap);
            material.SetTexture("_SpecGlossMap", specularMap);
            material.globalIlluminationFlags = materialIllumination;

            if (normalMap != null)
            {
                material.SetFloat("_BumpScale", 1.0f);
            }
            
            if (renderingMode is StandardShaderRenderingMode.Cutout)
            {
                material.SetFloat("_Surface", 1.0f);
                material.SetFloat("_AlphaClip", 1.0f);
                material.SetFloat("_Blend", 0.0f);
                material.SetFloat("_Cutoff", cutoff); 
                material.EnableKeyword("_ALPHATEST_ON");
            }
            else if (renderingMode is StandardShaderRenderingMode.Fade or StandardShaderRenderingMode.Transparent)
            {
                material.SetFloat("_Surface", 1.0f); 
                material.SetFloat("_Blend", 0.0f);
                material.EnableKeyword("_ALPHATEST_ON");
            }
        }
        
        private static void DisableEmission(this Material material)
        {
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        
        private enum StandardShaderRenderingMode
        {
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3
        }
    }
}
