Shader "Hidden/URPBerserk Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SwapTex ("Color Data", 2D) = "transparent" {}
        _EffectStrength ("Effect Strength", Float) = 0.2
        _LowThreshold ("Low Threshold", Float) = 0.0
        _HighThreshold ("High Threshold", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SwapTex);
            SAMPLER(sampler_SwapTex);

            CBUFFER_START(UnityPerMaterial)
                float _EffectStrength;
                float _LowThreshold;
                float _HighThreshold;
            CBUFFER_END
            
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 final;
                half4 thresholdBlend;
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 swapCol = SAMPLE_TEXTURE2D(_SwapTex, sampler_SwapTex, float2(c.r, 0));
                
                thresholdBlend = clamp(c, 
                    half4(_LowThreshold, _LowThreshold, _LowThreshold, 0.0), 
                    half4(_HighThreshold, _HighThreshold, _HighThreshold, 0.0));
                
                final = lerp(c, swapCol, swapCol.a * _EffectStrength * thresholdBlend);
                return final;
            }
            ENDHLSL
        }
    }
    FallBack Off
}