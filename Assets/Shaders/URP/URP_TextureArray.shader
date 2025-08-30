Shader "Custom/URPTextureArray"
{
    Properties
    {
        _MainTex("Albedo", 2DArray) = "" {}
        _SpecGlossMap("Specular", 2DArray) = "" {}
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2DArray) = "" {}
        _EmissionColor("Color", Color) = (1,1,1)
        _EmissionMap("Emission", 2DArray) = "" {}

        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        // ------------------------------------------------------------------
        //  GBuffer pass (for deferred rendering)
        // ------------------------------------------------------------------
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            ZWrite [_ZWrite]
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // -------------------------------------
            // Universal Render Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float3 texcoord : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                float4 color : COLOR; // Add vertex color input
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float3 uv : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

                float3 positionWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float4 tangentWS : TEXCOORD4;
                float3 viewDirWS : TEXCOORD5;

                half4 fogFactorAndVertexLight : TEXCOORD6;
                float4 shadowCoord : TEXCOORD7;

                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D_ARRAY(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D_ARRAY(_SpecGlossMap);
            SAMPLER(sampler_SpecGlossMap);
            TEXTURE2D_ARRAY(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D_ARRAY(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float _BumpScale;
                float4 _MainTex_ST;
                half4 _EmissionColor;
            CBUFFER_END

            float4 TexCoords(Attributes v)
            {
                float4 texcoord;
                texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex); // Always source from uv0
                texcoord.z = floor(v.color.r * 255);
                return texcoord;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.uv = TexCoords(input);
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS.xyz, input.tangentOS.w * GetOddNegativeScale());
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.positionCS = vertexInput.positionCS;

                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

                output.shadowCoord = GetShadowCoord(vertexInput);

                return output;
            }

            GBufferFragOutput frag(Varyings input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Get texture array index (you'll need to define how to get this)
                float arrayIndex = input.uv.z; // Replace with your logic to get array index
                float2 position = input.uv.xy;
                // Sample textures from array
                half4 albedo = SAMPLE_TEXTURE2D_ARRAY(_MainTex, sampler_MainTex, position, arrayIndex);
                half4 specGloss = SAMPLE_TEXTURE2D_ARRAY(_SpecGlossMap, sampler_SpecGlossMap, position, arrayIndex);
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D_ARRAY(_BumpMap, sampler_BumpMap, position, arrayIndex), _BumpScale);
                half3 emission = SAMPLE_TEXTURE2D_ARRAY(_EmissionMap, sampler_EmissionMap, position, arrayIndex).rgb
                    * _EmissionColor.rgb;

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz,
                                                             cross(input.normalWS, input.tangentWS.xyz) * input.
                                                             tangentWS.w,
                                                             input.normalWS));
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = SafeNormalize(input.viewDirWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);

                SurfaceData surfaceData;
                surfaceData.albedo = albedo.rgb;
                surfaceData.specular = specGloss.rgb;
                surfaceData.metallic = 0.0;
                surfaceData.smoothness = specGloss.a;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = emission;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = albedo.a;
                surfaceData.clearCoatMask = 0.0;
                surfaceData.clearCoatSmoothness = 0.0;

                return SurfaceDataToGbuffer(surfaceData, inputData, surfaceData.smoothness,
              surfaceData.emission + inputData.bakedGI * surfaceData.albedo);
            }
            ENDHLSL
        }

        // Shadow Caster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            Cull Off
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.uv = input.texcoord;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}