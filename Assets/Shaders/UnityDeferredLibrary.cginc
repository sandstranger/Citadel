// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_DEFERRED_LIBRARY_INCLUDED
#define UNITY_DEFERRED_LIBRARY_INCLUDED

// Deferred shading helpers

// --------------------------------------------------------
// Vertex shader

struct unity_v2f_deferred {
    float4 pos : SV_POSITION;
    half4 uv : TEXCOORD0;
    half3 ray : TEXCOORD1; // Изменено на half3
};

half _LightAsQuad; // Изменено на half

unity_v2f_deferred vert_deferred (float4 vertex : POSITION, half3 normal : NORMAL) // normal изменен на half3
{
    unity_v2f_deferred o;
    o.pos = UnityObjectToClipPos(vertex);
    o.uv = (half4) ComputeScreenPos(o.pos);
    o.ray = (half3)UnityObjectToViewPos(vertex) * half3(-1,-1,1);
    o.ray = lerp(o.ray, normal, _LightAsQuad);
    return o;
}

// --------------------------------------------------------
// Shared uniforms

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

half4 _LightDir; // Изменено на half4
half4 _LightPos; // Изменено на half4
half4 _LightColor; 
half4 unity_LightmapFade; // Изменено на half4
half4x4 unity_WorldToLight; // Оставлен half4x4 (матрицы)
sampler2D_half _LightTextureB0;

#if defined (POINT_COOKIE)
samplerCUBE_half _LightTexture0;
#else
sampler2D_half _LightTexture0;
#endif

#if defined (SHADOWS_SCREEN)
sampler2D _ShadowMapTexture;
#endif

#if defined (SHADOWS_SHADOWMASK)
sampler2D _CameraGBufferTexture4;
#endif

// --------------------------------------------------------
// Shadow/fade helpers

// Receiver plane depth bias create artifacts when depth is retrieved from
// the depth buffer. see UnityGetReceiverPlaneDepthBias in UnityShadowLibrary.cginc
#ifdef UNITY_USE_RECEIVER_PLANE_BIAS
    #undef UNITY_USE_RECEIVER_PLANE_BIAS
#endif

#include "UnityShadowLibrary.cginc"

//Note :
// SHADOWS_SHADOWMASK + LIGHTMAP_SHADOW_MIXING -> ShadowMask mode
// SHADOWS_SHADOWMASK only -> Distance shadowmask mode

// --------------------------------------------------------
half UnityDeferredSampleShadowMask(half2 uv) // uv изменен на half2
{
    half shadowMaskAttenuation = half(1.0);

    #if defined (SHADOWS_SHADOWMASK)
        half4 shadowMask = tex2D(_CameraGBufferTexture4, uv);
        shadowMaskAttenuation = saturate(dot(shadowMask, (half4)unity_OcclusionMaskSelector));
    #endif

    return shadowMaskAttenuation;
}

// --------------------------------------------------------
half UnityDeferredSampleRealtimeShadow(half fade, half3 vec, half2 uv) // Параметры изменены на half
{
    half shadowAttenuation = half(1.0);

    #if defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
        #if defined(SHADOWS_SCREEN)
            shadowAttenuation = (half)tex2D(_ShadowMapTexture, uv).r;
        #endif
    #endif

    #if defined(UNITY_FAST_COHERENT_DYNAMIC_BRANCHING) && defined(SHADOWS_SOFT) && !defined(LIGHTMAP_SHADOW_MIXING)
    UNITY_BRANCH
    if (fade < (half)(1.0 - 1e-2f))
    {
    #endif

        #if defined(SPOT)
            #if defined(SHADOWS_DEPTH)
                half4 shadowCoord = mul(unity_WorldToShadow[0], half4(vec, 1)); // Оставлен float для матричной операции
                shadowAttenuation = (half)UnitySampleShadowmap(shadowCoord);
            #endif
        #endif

        #if defined (POINT) || defined (POINT_COOKIE)
            #if defined(SHADOWS_CUBE)
                shadowAttenuation = (half)UnitySampleShadowmap(vec);
            #endif
        #endif

    #if defined(UNITY_FAST_COHERENT_DYNAMIC_BRANCHING) && defined(SHADOWS_SOFT) && !defined(LIGHTMAP_SHADOW_MIXING)
    }
    #endif

    return shadowAttenuation;
}

// --------------------------------------------------------
half UnityDeferredComputeShadow(half3 vec, half fadeDist, half2 uv) // Параметры изменены на half
{
    half fade = (half)UnityComputeShadowFade(fadeDist);
    half shadowMaskAttenuation = UnityDeferredSampleShadowMask(uv);
    half realtimeShadowAttenuation = UnityDeferredSampleRealtimeShadow(fade, vec, uv);

    return UnityMixRealtimeAndBakedShadows(realtimeShadowAttenuation, shadowMaskAttenuation, fade);
}

// --------------------------------------------------------
// Common lighting data calculation (direction, attenuation, ...)
void UnityDeferredCalculateLightParams (
    unity_v2f_deferred i,
    out half3 outWorldPos,
    out half2 outUV,
    out half3 outLightDir,
    out half outAtten,
    out half outFadeDist)
{
    i.ray = i.ray * ((half)_ProjectionParams.z / i.ray.z);
    half2 uv = (half2)(i.uv.xy / i.uv.w);

    // read depth and reconstruct world position
    half depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); 
    depth = Linear01Depth (depth);
    half4 vpos = half4(i.ray * depth,1);
    half3 wpos = (half3)mul (unity_CameraToWorld, vpos).xyz; // Результат можно привести к half3

    half fadeDist = (half)UnityComputeShadowFadeDistance(wpos, vpos.z);

    // spot light case
    #if defined (SPOT)
        half3 tolight = (half3)(_LightPos.xyz - wpos);
        half3 lightDir = normalize (tolight);

        half4 uvCookie = mul (unity_WorldToLight, half4(wpos,1)); // Матричные операции оставляем в half
        half atten = (half)tex2Dbias (_LightTexture0, half4(uvCookie.xy / uvCookie.w, 0, -8)).w;
        atten *= (half)(uvCookie.w < 0);
        half att = (half)dot(tolight, tolight) * _LightPos.w;
        atten *= (half)tex2D (_LightTextureB0, att.rr).r;

        atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);

    // directional light case
    #elif defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
        half3 lightDir = -(half3)_LightDir.xyz;
        half atten = half(1.0);

        atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);

        #if defined (DIRECTIONAL_COOKIE)
        atten *= (half)tex2Dbias (_LightTexture0, half4(mul(unity_WorldToLight, half4(wpos,1)).xy, 0, -8)).w;
        #endif //DIRECTIONAL_COOKIE

    // point light case
    #elif defined (POINT) || defined (POINT_COOKIE)
        half3 tolight = (half3)(wpos - _LightPos.xyz);
        half3 lightDir = -normalize (tolight);

        half att = (half)dot(tolight, tolight) * _LightPos.w;
        half atten = (half)tex2D (_LightTextureB0, att.rr).r;

        atten *= UnityDeferredComputeShadow (tolight, fadeDist, uv);

        #if defined (POINT_COOKIE)
        atten *= (half)texCUBEbias(_LightTexture0, half4(mul(unity_WorldToLight, half4(wpos,1)).xyz, -8)).w;
        #endif //POINT_COOKIE
    #else
        half3 lightDir = half3(0,0,0);
        half atten = half(0);
    #endif

    outWorldPos = wpos;
    outUV = uv;
    outLightDir = lightDir;
    outAtten = atten;
    outFadeDist = fadeDist;
}

#endif // UNITY_DEFERRED_LIBRARY_INCLUDED