Shader "Custom/URPTextureArray" {
    Properties {
        [MainTexture] _MainTex("Albedo", 2DArray) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Slice("Array Slice", Float) = 0
        _SpecGlossMap("Specular", 2DArray) = "white" {}
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        [Normal] _BumpMap("Normal Map", 2DArray) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2DArray) = "black" {}
    }
    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
        }
        LOD 300
        Pass {
            Name "GBuffer"
            Tags { "LightMode" = "UniversalGBuffer" }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _EMISSION
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"

            #ifndef GBUFFEROUTPUT_DEFINED
            struct GBufferOutput {
                float4 GBuffer0 : SV_Target0;
                float4 GBuffer1 : SV_Target1;
                float4 GBuffer2 : SV_Target2;
                float4 GBuffer3 : SV_Target3;
            };
            #define GBUFFEROUTPUT_DEFINED 1
            #endif
            TEXTURE2D_ARRAY(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D_ARRAY(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);
            TEXTURE2D_ARRAY(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D_ARRAY(_EmissionMap); SAMPLER(sampler_EmissionMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _Slice;
                float _BumpScale;
                float _Smoothness;
                float4 _EmissionColor;
            CBUFFER_END
            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings vert(Attributes input) {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs vpos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs vnorm = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                o.positionCS = vpos.positionCS;
                o.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                o.positionWS = vpos.positionWS;
                o.normalWS = vnorm.normalWS;
                o.tangentWS = float4(vnorm.tangentWS, input.tangentOS.w);
                return o;
            }
            GBufferOutput frag(Varyings input) {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.uv;
                float slice = _Slice;
                // Albedo
                float4 albedoAlpha = SAMPLE_TEXTURE2D_ARRAY(_MainTex, sampler_MainTex, uv, slice) * _BaseColor;
                // Specular
                float4 specGloss = SAMPLE_TEXTURE2D_ARRAY(_SpecGlossMap, sampler_SpecGlossMap, uv, slice);
                // Normal
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D_ARRAY(_BumpMap, sampler_BumpMap, uv, slice), _BumpScale);
                float3 normalWS = input.normalWS;
                float3 bitangent = input.tangentWS.w * cross(input.normalWS, input.tangentWS.xyz);
                normalWS = TransformTangentToWorld(normalTS, float3x3(input.tangentWS.xyz, bitangent, input.normalWS));
                normalWS = normalize(normalWS);
                // Emission
                float3 emission = 0;
                emission = SAMPLE_TEXTURE2D_ARRAY(_EmissionMap, sampler_EmissionMap, uv, slice).rgb * _EmissionColor.rgb;
                GBufferOutput o;
                o.GBuffer0 = float4(albedoAlpha.rgb, 1);
                o.GBuffer1 = float4(specGloss.rgb, specGloss.a * _Smoothness);
                o.GBuffer2 = float4(normalWS * 0.5 + 0.5, 1);
                o.GBuffer3 = float4(emission, 1);
                return o;
            }
            ENDHLSL
        }
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings vert(Attributes input) {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return o;
            }
            float4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }
        Pass {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull[_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings vert(Attributes input) {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return o;
            }
            float4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
