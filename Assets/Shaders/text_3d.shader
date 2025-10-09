Shader "GUI/3D Text Shader" { 
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Font Texture",2D) = "white" {}
    }

    SubShader {
        Tags {"RenderType"="Transparent" "Queue"="Transparent"}
        Cull Back
        
        CGPROGRAM
        #pragma surface SS_Main Lambert vertex:VS_Main alpha:blend
        #pragma multi_compile_instancing // Добавляем поддержку инстансинга
        #pragma target 3.5
        
        // Объявляем буфер для свойств инстансинга
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)
        
        uniform sampler2D _MainTex;
        
        struct Output {
            float4 vertex : POSITION;
            half3 normal : NORMAL;
            half4 tangent : TANGENT;
            half4 texcoord : TEXCOORD0;
            half4 texcoord1 : TEXCOORD1;
            half4 texcoord2 : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
        };
        
        struct Input {
            half2 uv_MainTex;
            UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса для фрагментного шейдера
        };
        
        void VS_Main (inout Output o) {
            UNITY_SETUP_INSTANCE_ID(o); // Устанавливаем ID инстанса
            
            o.normal = half3 (0, 0, -1);
            o.tangent = half4(1, 0, 0, 1);
        }
        
        void SS_Main (Input IN, inout SurfaceOutput o) {
            UNITY_SETUP_INSTANCE_ID(IN); // Устанавливаем ID инстанса для доступа к свойствам
            
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            
            // Используем инстансированное свойство _Color
            fixed4 instancedColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            
            o.Albedo = instancedColor.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    
    // Добавляем fallback шейдер для случаев, когда инстансинг не поддерживается
    FallBack "Transparent/VertexLit"
}