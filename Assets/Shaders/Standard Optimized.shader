// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Оптимизированная версия для мобильных устройств с поддержкой Deferred Rendering

Shader "Standard Optimized"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        // Эти параметры оставлены для совместимости с редактором, но не используются в мобильной версии
        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // Эти параметры оставлены для совместимости с редактором, но не используются в мобильной версии
        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
//            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
          //  #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _PARALLAXMAP_OFF
            #pragma shader_feature_local _ _OCCLUSIONMAP_OFF
            #pragma shader_feature_local _ _DETAIL_MULX2_OFF
          
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBase
            #pragma fragment fragBase
            #define MOBILE_OPTIMIZED
            #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
        
        // ------------------------------------------------------------------
        //  ТОЛЬКО Deferred pass (оптимизированная версия для мобильных)
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt

            // Упрощенные настройки для мобильных устройств
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            // УБРАНЫ детали и параллакс для мобильных устройств
            // #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local _ _PARALLAXMAP_OFF
            #pragma shader_feature_local _ _OCCLUSIONMAP_OFF
            #pragma shader_feature_local _ _DETAIL_MULX2_OFF
            
            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            
            #pragma vertex vertDeferred
            #pragma fragment fragDeferred

            // МОБИЛЬНАЯ ОПТИМИЗАЦИЯ: использование упрощенной библиотеки
            #define MOBILE_OPTIMIZED
            #include "UnityStandardCore.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        //  Shadow rendering pass (минимальная версия)
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // Упрощенные настройки для мобильных устройств
            #pragma shader_feature_local _ _ALPHATEST_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // УБРАН параллакс для мобильных устройств
            #pragma shader_feature_local _ _PARALLAXMAP_OFF
            #pragma shader_feature_local _ _OCCLUSIONMAP_OFF
            #pragma shader_feature_local _ _DETAIL_MULX2_OFF
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #define MOBILE_OPTIMIZED
            #include "UnityStandardShadow.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        //  META pass для совместимости с редактором
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // УБРАНЫ детали для мобильных устройств
            // #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local _ _DETAIL_MULX2_OFF

            #define MOBILE_OPTIMIZED
            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    // ------------------------------------------------------------------
    //  Fallback для случаев, когда deferred недоступен
    FallBack "Mobile/VertexLit"
    CustomEditor "StandardShaderGUI"
}