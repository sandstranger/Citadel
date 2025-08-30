Shader "Custom/URP3DTextShader" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Font Texture", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass {
            Name "ForwardPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(float3(0, 0, -1));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                half4 texSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                Light mainLight = GetMainLight();
                half3 lambert = LightingLambert(mainLight.color, mainLight.direction, IN.normalWS);
                half3 ambient = SampleSH(IN.normalWS);
                half3 finalColor = _Color.rgb * (lambert + ambient);
                return half4(finalColor, texSample.a * _Color.a);
            }
            ENDHLSL
        }
    }
}