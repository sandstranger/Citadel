Shader "Custom/URPTwoSidedTransparentSpecular" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _Shininess ("Shininess", Range(0.03, 1)) = 0.078125
        _Gloss ("Gloss", Range(0,1)) = 0.4
        _Alpha ("Transparency", Range(0,1)) = 1.0
        _DitherScale ("Dither Scale", Range(0.1, 100)) = 1.0
    }
    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }
        LOD 200
        
        Cull Off // Renders both sides
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        // Main forward pass
        Pass {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _SpecColor;
                float _Shininess;
                float _Gloss;
                float _Alpha;
            CBUFFER_END

            Varyings vert(Attributes IN) {
                Varyings OUT;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS);
                
                OUT.positionHCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.normalWS = normalInput.normalWS;
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                return OUT;
            }

            // Custom Blinn-Phong lighting calculation
            half3 CalculateBlinnPhong(Light light, half3 normal, half3 viewDir, half3 albedo, half gloss, half shininess) {
                half3 halfDir = normalize(light.direction + viewDir);
                half NdotH = max(0, dot(normal, halfDir));
                half specular = pow(NdotH, shininess * 128.0) * gloss;
                
                half NdotL = max(0, dot(normal, light.direction));
                half3 diffuse = light.color * NdotL * albedo;
                half3 spec = light.color * specular * _SpecColor.rgb;
                
                return diffuse + spec;
            }

            half4 frag(Varyings IN) : SV_Target {
                // Sample texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 baseColor = texColor * _Color;
                
                // Get lighting information
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                half3 ambient = SampleSH(IN.normalWS);
                
                // Calculate Blinn-Phong lighting
                half3 lighting = CalculateBlinnPhong(
                    mainLight, 
                    normalize(IN.normalWS), 
                    normalize(IN.viewDirWS), 
                    baseColor.rgb, 
                    _Gloss, 
                    _Shininess
                );
                
                // Combine lighting with ambient
                half3 finalColor = lighting + ambient * baseColor.rgb;
                
                return half4(finalColor, baseColor.a * _Alpha);
            }
            ENDHLSL
        }

        // Shadow caster pass with dithering
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Alpha;
                float _DitherScale;
            CBUFFER_END

            float3 _LightDirection;

            // Dithering function with adjustable scale
            float DitherClip(float alpha, float2 screenPos) {
                float4x4 thresholdMatrix = float4x4(
                    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
                );
                // Increase resolution by scaling screen position
                float2 pos = screenPos * _ScreenParams.xy * _DitherScale;
                float dither = thresholdMatrix[fmod(pos.x, 4)][fmod(pos.y, 4)];
                return alpha - dither * 0.5;
            }

            Varyings vert(Attributes IN) {
                Varyings OUT;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(ApplyShadowBias(vertexInput.positionWS, 
                    GetVertexNormalInputs(float3(0, 0, 1)).normalWS, _LightDirection));
                
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                half4 texcol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                half alpha = texcol.a * _Alpha;
                
                // Apply dithering
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                clip(DitherClip(alpha, screenUV));
                
                return 0;
            }
            ENDHLSL
        }

        // Depth-only pass for sorting
        Pass {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Alpha;
            CBUFFER_END

            Varyings vert(Attributes IN) {
                Varyings OUT;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = vertexInput.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                half4 texcol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                clip(texcol.a * _Alpha - 0.5);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}