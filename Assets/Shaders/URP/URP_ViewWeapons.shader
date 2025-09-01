Shader "Custom/URPViewWeapons"
{
    Properties
    {
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
        _DepthOffset("Depth Offset", Range(-0.5,0)) = -0.2
        _UnlitFac("Unlit Factor", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        ZWrite On ZTest LEqual
		Lighting On
        ZWrite On 
        ZTest LEqual
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float4 _MainTex_ST;
                float _DepthOffset;
                float _UnlitFac;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Преобразование позиции в пространство отсечения
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                
                // Применение смещения глубины
                OUT.positionHCS.z += _DepthOffset;
                
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample texture
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Tint;
                half4 color = albedo;
                
                // Normal calculation
                float3 normalWS = normalize(IN.normalWS);
                
                // Get main light
                Light mainLight = GetMainLight();
                float3 mainLightColor = mainLight.color;
                float3 mainLightDirection = mainLight.direction;
                
                // Calculate main light contribution
                float NdotL = saturate(dot(normalWS, mainLightDirection));
                float3 mainLighting = NdotL * mainLightColor;
                
                // Additional lights
                int additionalLightsCount = GetAdditionalLightsCount();
                float3 additionalLighting = float3(0, 0, 0);
                
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    Light light = GetAdditionalLight(i, IN.positionWS);
                    float3 lightColor = light.color;
                    float3 lightDirection = light.direction;
                    float distance = length(light.direction); // For point lights
                    float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                    
                    float addNdotL = saturate(dot(normalWS, lightDirection));
                    additionalLighting += addNdotL * lightColor * attenuation;
                }
                
                // Combine lighting
                color.rgb *= (mainLighting + additionalLighting);
                
                // Add unlit factor
                color.rgb += albedo.rgb * _UnlitFac;
                
                return color;
            }
            ENDHLSL
        }
    }
}