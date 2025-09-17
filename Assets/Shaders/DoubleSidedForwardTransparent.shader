Shader "Custom/TwoSidedTransparentSpecular" {
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
        }
        LOD 200
        
        Cull Off
        
        // Main surface pass
        CGPROGRAM
        #pragma surface surf BlinnPhong alpha:fade
        #pragma target 3.0
        #pragma multi_compile_instancing // Добавляем поддержку инстансинга

        sampler2D _MainTex;
        half _Shininess;
        half _Gloss;

        // Объявляем буфер для свойств инстансинга
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _Alpha)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            // Используем инстансированные свойства
            fixed4 instancedColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed instancedAlpha = UNITY_ACCESS_INSTANCED_PROP(Props, _Alpha);
            
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * instancedColor;
            o.Albedo = c.rgb;
            o.Specular = _Shininess;
            o.Gloss = _Gloss;
            o.Alpha = c.a * instancedAlpha;
        }
        ENDCG

        // Shadow caster pass with finer dithered transparency
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing // Добавляем поддержку инстансинга
            #include "UnityCG.cginc"

            struct v2f {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DitherScale;

            // Объявляем буфер для свойств инстансинга
            UNITY_INSTANCING_BUFFER_START(ShadowProps)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(fixed, _Alpha)
            UNITY_INSTANCING_BUFFER_END(ShadowProps)

            v2f vert(appdata_base v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

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

            fixed4 frag(v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i); // Устанавливаем ID инстанса
                
                // Используем инстансированные свойства
                fixed4 instancedColor = UNITY_ACCESS_INSTANCED_PROP(ShadowProps, _Color);
                fixed instancedAlpha = UNITY_ACCESS_INSTANCED_PROP(ShadowProps, _Alpha);
                
                fixed4 texcol = tex2D(_MainTex, i.uv) * instancedColor;
                fixed alpha = texcol.a * instancedAlpha;
                
                // Apply finer dithering
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                clip(DitherClip(alpha, screenUV));
                
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}