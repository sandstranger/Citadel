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

        // Base pass (ambient + first light)
        Pass {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing // Добавляем поддержку инстансинга
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Объявляем буфер для свойств инстансинга
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
                UNITY_DEFINE_INSTANCED_PROP(float, _DepthOffset)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct VertexData {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };

            struct Interpolators {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };

            Interpolators MyVertexProgram (VertexData v) {
                Interpolators i;
                UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
                UNITY_TRANSFER_INSTANCE_ID(v, i); // Передаем ID инстанса
                
                i.position = UnityObjectToClipPos(v.position);
                
                // Используем инстансированное свойство
                float depthOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _DepthOffset);
                i.position.z -= depthOffset;
                
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                i.worldNormal = UnityObjectToWorldNormal(v.normal);
                i.worldPos = mul(unity_ObjectToWorld, v.position).xyz;
                return i;
            }

            float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(i); // Устанавливаем ID инстанса
                
                // Используем инстансированные свойства
                float4 tint = UNITY_ACCESS_INSTANCED_PROP(Props, _Tint);
                
                float4 color = tex2D(_MainTex, i.uv) * tint;
                float3 normal = normalize(i.worldNormal);
                float3 worldPos = i.worldPos;

                // Ambient lighting
                float3 lighting = UNITY_LIGHTMODEL_AMBIENT.rgb;

                // Handle main light (if directional, else zero)
                float3 lightDir;
                float3 lightColor = _LightColor0.rgb;
                if (_WorldSpaceLightPos0.w == 0) { // Directional
                    lightDir = normalize(_WorldSpaceLightPos0.xyz);
                } else { // Point light as main light
                    lightDir = normalize(_WorldSpaceLightPos0.xyz - worldPos);
                }
                float diff = max(0, dot(normal, lightDir));
                lighting += diff * lightColor;

                color.rgb *= lighting;
                return color;
            }
            ENDCG
        }

        // Additional pass for point lights
        Pass {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
            CGPROGRAM
            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram
            #pragma multi_compile_fwdadd
            #pragma multi_compile_instancing // Добавляем поддержку инстансинга
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Объявляем буфер для свойств инстансинга
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
                UNITY_DEFINE_INSTANCED_PROP(float, _DepthOffset)
                UNITY_DEFINE_INSTANCED_PROP(float, _UnlitFac)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct VertexData {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };

            struct Interpolators {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };

            Interpolators MyVertexProgram (VertexData v) {
                Interpolators i;
                UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
                UNITY_TRANSFER_INSTANCE_ID(v, i); // Передаем ID инстанса
                
                i.position = UnityObjectToClipPos(v.position);
                
                // Используем инстансированное свойство
                float depthOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _DepthOffset);
                i.position.z -= depthOffset;
                
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                i.worldNormal = UnityObjectToWorldNormal(v.normal);
                i.worldPos = mul(unity_ObjectToWorld, v.position).xyz;
                return i;
            }

            float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(i); // Устанавливаем ID инстанса
                
                // Используем инстансированные свойства
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
                float attenuation = 1.0 / (1.0 + distance * distance);
                float diff = max(0, dot(normal, lightDir));
                color.rgb *= diff * lightColor * attenuation;
                color.rgb += albedo * unlitFac;
                return color;
            }
            ENDCG
        }
	}
}