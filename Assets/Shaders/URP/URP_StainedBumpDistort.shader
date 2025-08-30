Shader "Custom/URPStained BumpDistort"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _BumpAmt ("Distortion", Range(0, 128)) = 10
        _MainTex ("Tint Color (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "BumpDistort"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D_X_FLOAT(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float _BumpAmt;
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                float4 _Color; 
            CBUFFER_END

            
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Calculate distorted UV coordinates
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                // Get normal from bump map and calculate offset
                half3 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv));
                float2 offset = bump.xy * _BumpAmt * 0.01; // Adjusted for URP scale
                
                // Apply offset to screen UV
                screenUV += offset;
                
                // Sample the screen texture with distortion
                half4 col = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV);
                
                // Apply tint color
                half4 tint = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                col *= tint * _Color;
                return col;
            }
            ENDHLSL
        }
    }
}