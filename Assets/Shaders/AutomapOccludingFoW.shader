Shader "Custom/AutomapMaskingFoW" {
Properties {
    _MainTex("MainTexture",2D) = "black"{}
    _Stencil ("Stencil ID", Float) = 0
    _StencilComp ("Stencil Comparison", Float) = 8
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("stencil Read Mask", Float) = 255
    _ColorMask ("Color Mask", Float) = 15
    
    [PerRendererData] _InstanceColor("Instance Color", Color) = (1,1,1,1)
}
    SubShader {
        Tags { 
            "Queue" = "Background-1"
            "RenderType" = "Opaque" 
        }

        Pass {
            ColorMask 0
            
            // Добавляем теги для инстансинга
            Tags { "LightMode" = "ForwardBase" }
       
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5            
            #pragma multi_compile_instancing // Добавляем поддержку инстансинга
            #include "UnityCG.cginc"
 
            struct appdata
            {
               float4 vertex : POSITION;
               UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };
     
            struct v2f
            {
               float4 vertex : SV_POSITION;
               float size : PSIZE;
               UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса для фрагментного шейдера
            };
            
            // Объявляем буфер для свойств инстансинга, если нужно
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half4, _InstanceColor)
            UNITY_INSTANCING_BUFFER_END(Props)
         
            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
                UNITY_TRANSFER_INSTANCE_ID(v, o); // Передаем ID инстанса в фрагментный шейдер
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.size = 30;
                return o;
            }
         
            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // Устанавливаем ID инстанса для доступа к свойствам
                
                return fixed4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
    
    // Добавляем fallback шейдер для случаев, когда инстансинг не поддерживается
    FallBack "Transparent/VertexLit"
    CustomEditor "ShaderGUI"
}