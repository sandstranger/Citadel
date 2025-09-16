// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_DEFERRED_LIBRARY_INCLUDED
#define UNITY_DEFERRED_LIBRARY_INCLUDED
// Deferred lighting / shading helpers

// --------------------------------------------------------
// Vertex shader

struct unity_v2f_deferred {
    half4 pos : SV_POSITION;
    half4 uv : TEXCOORD0;
    half3 ray : TEXCOORD1;
};

half _LightAsQuad;

unity_v2f_deferred vert_deferred (half4 vertex : POSITION, half3 normal : NORMAL)
{
    unity_v2f_deferred o;
    o.pos = UnityObjectToClipPos(vertex);
    o.uv = ComputeScreenPos(o.pos);
    o.ray = UnityObjectToViewPos(vertex) * half3(-1,-1,1);

    // normal contains a ray pointing from the camera to one of near plane's
    // corners in camera space when we are drawing a full screen quad.
    // Otherwise, when rendering 3D shapes, use the ray calculated here.
    o.ray = lerp(o.ray, normal, _LightAsQuad);

    return o;
}


// --------------------------------------------------------
// Shared uniforms


UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

half4 _LightDir;
half4 _LightPos;
half4 _LightColor;
half4 unity_LightmapFade;
half4x4 unity_WorldToLight;
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
half UnityDeferredSampleShadowMask(half2 uv)
{
    half shadowMaskAttenuation = 1.0f;

    #if defined (SHADOWS_SHADOWMASK)
        half4 shadowMask = tex2D(_CameraGBufferTexture4, uv);
        shadowMaskAttenuation = saturate(dot(shadowMask, unity_OcclusionMaskSelector));
    #endif

    return shadowMaskAttenuation;
}

// --------------------------------------------------------
half UnityDeferredSampleRealtimeShadow(half fade, half3 vec, half2 uv)
{
    half shadowAttenuation = 1.0f;

    #if defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
        #if defined(SHADOWS_SCREEN)
            shadowAttenuation = tex2D(_ShadowMapTexture, uv).r;
        #endif
    #endif

    #if defined(UNITY_FAST_COHERENT_DYNAMIC_BRANCHING) && defined(SHADOWS_SOFT) && !defined(LIGHTMAP_SHADOW_MIXING)
    //avoid expensive shadows fetches in the distance where coherency will be good
    UNITY_BRANCH
    if (fade < (1.0f - 1e-2f))
    {
    #endif

        #if defined(SPOT)
            #if defined(SHADOWS_DEPTH)
                half4 shadowCoord = mul(unity_WorldToShadow[0], half4(vec, 1));
                shadowAttenuation = UnitySampleShadowmap(shadowCoord);
            #endif
        #endif

        #if defined (POINT) || defined (POINT_COOKIE)
            #if defined(SHADOWS_CUBE)
                shadowAttenuation = UnitySampleShadowmap(vec);
            #endif
        #endif

    #if defined(UNITY_FAST_COHERENT_DYNAMIC_BRANCHING) && defined(SHADOWS_SOFT) && !defined(LIGHTMAP_SHADOW_MIXING)
    }
    #endif

    return shadowAttenuation;
}

// --------------------------------------------------------
half UnityDeferredComputeShadow(half3 vec, half fadeDist, half2 uv)
{

    half fade                      = UnityComputeShadowFade(fadeDist);
    half shadowMaskAttenuation     = UnityDeferredSampleShadowMask(uv);
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
    i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
    half2 uv = i.uv.xy / i.uv.w;

    // read depth and reconstruct world position
    half depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
    depth = Linear01Depth (depth);
    half4 vpos = half4(i.ray * depth,1);
    half3 wpos = mul (unity_CameraToWorld, vpos).xyz;

    half fadeDist = UnityComputeShadowFadeDistance(wpos, vpos.z);

    // spot light case
    #if defined (SPOT)
        half3 tolight = _LightPos.xyz - wpos;
        half3 lightDir = normalize (tolight);

        half4 uvCookie = mul (unity_WorldToLight, half4(wpos,1));
        // negative bias because http://aras-p.info/blog/2010/01/07/screenspace-vs-mip-mapping/
        half atten = tex2Dbias (_LightTexture0, half4(uvCookie.xy / uvCookie.w, 0, -8)).w;
        atten *= uvCookie.w < 0;
        half att = dot(tolight, tolight) * _LightPos.w;
        atten *= tex2D (_LightTextureB0, att.rr).r;

        atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);

    // directional light case
    #elif defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
        half3 lightDir = -_LightDir.xyz;
        half atten = 1.0;

        atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);

        #if defined (DIRECTIONAL_COOKIE)
        atten *= tex2Dbias (_LightTexture0, half4(mul(unity_WorldToLight, half4(wpos,1)).xy, 0, -8)).w;
        #endif //DIRECTIONAL_COOKIE

    // point light case
    #elif defined (POINT) || defined (POINT_COOKIE)
        half3 tolight = wpos - _LightPos.xyz;
        half3 lightDir = -normalize (tolight);

        half att = dot(tolight, tolight) * _LightPos.w;
        half atten = tex2D (_LightTextureB0, att.rr).r;

        atten *= UnityDeferredComputeShadow (tolight, fadeDist, uv);

        #if defined (POINT_COOKIE)
        atten *= texCUBEbias(_LightTexture0, half4(mul(unity_WorldToLight, half4(wpos,1)).xyz, -8)).w;
        #endif //POINT_COOKIE
    #else
        half3 lightDir = 0;
        half atten = 0;
    #endif

    outWorldPos = wpos;
    outUV = uv;
    outLightDir = lightDir;
    outAtten = atten;
    outFadeDist = fadeDist;
}


#endif // UNITY_DEFERRED_LIBRARY_INCLUDED