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
        LOD 100
        
        Cull Off
        
        // Main surface pass with GPU Instancing
        CGPROGRAM
        #pragma surface surf BlinnPhong alpha:fade
        #pragma target 3.0
        #pragma multi_compile_instancing

        sampler2D _MainTex;
        half _Shininess;
        half _Gloss;
        fixed _Alpha;

        UNITY_INSTANCING_BUFFER_START(Props)
            // Убрано _SpecColor из буфера инстансинга
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 instanceColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * instanceColor;
            o.Albedo = c.rgb;
            o.Specular = _Shininess;
            o.Gloss = _Gloss;
            o.Alpha = c.a * _Alpha;
        }
        ENDCG

        // Shadow caster pass с GPU Instancing (без изменений)
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
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct v2f {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _Alpha;
            float _DitherScale;

            UNITY_INSTANCING_BUFFER_START(ShadowProps)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(ShadowProps)

            v2f vert(appdata_base v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.screenPos = ComputeScreenPos(o.pos);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                return o;
            }

            float DitherClip(float alpha, float2 screenPos) {
                float4x4 thresholdMatrix = float4x4(
                    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
                );
                float2 pos = screenPos * _ScreenParams.xy * _DitherScale;
                float dither = thresholdMatrix[fmod(pos.x, 4)][fmod(pos.y, 4)];
                return alpha - dither * 0.5;
            }

            fixed4 frag(v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                
                fixed4 instanceColor = UNITY_ACCESS_INSTANCED_PROP(ShadowProps, _Color);
                fixed4 texcol = tex2D(_MainTex, i.uv) * instanceColor;
                fixed alpha = texcol.a * _Alpha;
                
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                clip(DitherClip(alpha, screenUV));
                
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}