// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ViewWeapons" {
	Properties {
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Texture", 2D) = "white" {}
        _DepthOffset ("Depth Offset",Range(-0.5,0)) = -0.2
		_UnlitFac ("Unlit Factor",Range(0,1)) = 0.1
	}

	SubShader {
		Tags {"RenderType"="Opaque"}
		ZWrite On ZTest LEqual
		Lighting On
        Cull Back

        // Base pass (ambient + first light + vertex lights)
        Pass {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
                UNITY_DEFINE_INSTANCED_PROP(float, _DepthOffset)
                UNITY_DEFINE_INSTANCED_PROP(float, _UnlitFac)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct VertexData {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 vertexLighting : TEXCOORD3; // Добавили для вертексного освещения
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Interpolators MyVertexProgram (VertexData v) {
                Interpolators i;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, i);
                
                i.position = UnityObjectToClipPos(v.position);
                
                float depthOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _DepthOffset);
                i.position.z -= depthOffset;
                
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                i.worldNormal = UnityObjectToWorldNormal(v.normal);
                i.worldPos = mul(unity_ObjectToWorld, v.position).xyz;
                
                // ВЕРТЕКСНОЕ ОСВЕЩЕНИЕ ДЛЯ "NOT IMPORTANT" LIGHTS
                i.vertexLighting = float3(0,0,0);
                
                // Обрабатываем до 4 вертексных источников света
                #ifdef VERTEXLIGHT_ON
                    i.vertexLighting += Shade4PointLights(
                        unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                        unity_LightColor[0].rgb, unity_LightColor[1].rgb, 
                        unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                        unity_4LightAtten0, i.worldPos, i.worldNormal
                    );
                #endif
                
                return i;
            }

            float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(i);
                
                float4 tint = UNITY_ACCESS_INSTANCED_PROP(Props, _Tint);
                float unlitFac = UNITY_ACCESS_INSTANCED_PROP(Props, _UnlitFac);
                
                float4 color = tex2D(_MainTex, i.uv) * tint;
                float3 normal = normalize(i.worldNormal);
                float3 worldPos = i.worldPos;

                // Ambient lighting
                float3 lighting = UNITY_LIGHTMODEL_AMBIENT.rgb;

                // Добавляем вертексное освещение от "Not Important" lights
                lighting += i.vertexLighting;

                // Основной направленный свет
                float3 lightDir;
                float3 lightColor = _LightColor0.rgb;
                if (_WorldSpaceLightPos0.w == 0) { // Directional
                    lightDir = normalize(_WorldSpaceLightPos0.xyz);
                } else { // Point light как основной
                    lightDir = normalize(_WorldSpaceLightPos0.xyz - worldPos);
                }
                float diff = max(0, dot(normal, lightDir));
                lighting += diff * lightColor;

                // Добавляем постоянную подсветку чтобы не было совсем черно
                lighting += unlitFac;
                
                color.rgb *= lighting;
                return color;
            }
            ENDCG
        }

        // Additional pass для per-pixel lights
        Pass {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
            CGPROGRAM
            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram
            #pragma multi_compile_fwdadd
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
                UNITY_DEFINE_INSTANCED_PROP(float, _DepthOffset)
                UNITY_DEFINE_INSTANCED_PROP(float, _UnlitFac)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct VertexData {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Interpolators MyVertexProgram (VertexData v) {
                Interpolators i;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, i);
                
                i.position = UnityObjectToClipPos(v.position);
                
                float depthOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _DepthOffset);
                i.position.z -= depthOffset;
                
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                i.worldNormal = UnityObjectToWorldNormal(v.normal);
                i.worldPos = mul(unity_ObjectToWorld, v.position).xyz;
                return i;
            }

            float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(i);
                
                float4 tint = UNITY_ACCESS_INSTANCED_PROP(Props, _Tint);
                float unlitFac = UNITY_ACCESS_INSTANCED_PROP(Props, _UnlitFac);
                
                float4 albedo = tex2D(_MainTex, i.uv) * tint;
                float4 color = albedo;
                float3 normal = normalize(i.worldNormal);
                float3 worldPos = i.worldPos;

                // Point light calculation
                float3 lightDir = _WorldSpaceLightPos0.xyz - worldPos;
                float distance = length(lightDir);
                lightDir = normalize(lightDir);
                float3 lightColor = _LightColor0.rgb;
                
                // Более правильная аттенюация
                float attenuation = 1.0 / (1.0 + _LightColor0.a * distance * distance);
                attenuation *= lerp(attenuation, 1, _WorldSpaceLightPos0.w); // для направленного света
                
                float diff = max(0, dot(normal, lightDir));
                color.rgb *= diff * lightColor * attenuation;
                
                return color;
            }
            ENDCG
        }
	}
}