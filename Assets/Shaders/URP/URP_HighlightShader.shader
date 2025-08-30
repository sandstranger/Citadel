Shader "Custom/URPHighlightShader" {
    Properties {
        _ColorTint("Color Tint", Color) = (1, 1, 1, 1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(1.0, 6.0)) = 3.0
        _HSVAAdjust("HSVA Adjust", Vector) = (0,0,0,0)
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        CBUFFER_START(UnityPerMaterial)
            float4 _ColorTint;
            float4 _RimColor;
            float _RimPower;
            float4 _HSVAAdjust;
            float4 _MainTex_ST;
        CBUFFER_END
        ENDHLSL

        Pass {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(IN.positionOS.xyz));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                // Sample main texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // Apply color tint and HSVA adjustment
                half4 baseColor = texColor * _ColorTint;
                baseColor.rgb += _HSVAAdjust.rgb;
                baseColor.a += _HSVAAdjust.a;
                
                // Get main light
                Light mainLight = GetMainLight();
                
                // Calculate Lambert lighting :cite[5]
                half3 diffuse = LightingLambert(mainLight.color, mainLight.direction, IN.normalWS);
                
                // Add ambient lighting
                half3 ambient = SampleSH(IN.normalWS);
                
                // Combine lighting
                half3 finalColor = baseColor.rgb * (diffuse + ambient);
                
                // Calculate rim lighting
                half rim = 1.0 - saturate(dot(normalize(IN.viewDirWS), IN.normalWS));
                half3 rimEffect = _RimColor.rgb * pow(rim, _RimPower);
                
                // Add rim effect to final color
                finalColor += rimEffect;
                
                return half4(finalColor, baseColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}