Shader "Custom/Geometry/Wireframe" {
    Properties {
        [PowerSlider(3.0)]
        _WireframeVal ("Wireframe width", Range(0., 0.5)) = 0.05
        _Color ("Wire color", color) = (1., 1., 1., 1.)
        _CenterAlpha ("CenterAlpha", Range(0.,1.0)) = 0.02
        [Toggle] _RemoveDiag("Remove diagonals?", Float) = 0.
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Pass {
            Cull Front
            Blend SrcAlpha One
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma multi_compile_instancing // Добавляем поддержку инстансинга

            // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            // Объявляем буфер для свойств инстансинга
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _WireframeVal)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _CenterAlpha)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct v2g {
                float4 worldPos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса для фрагментного шейдера
            };

            v2g vert(appdata_base v) {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                float3 param = float3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;
                UNITY_SETUP_INSTANCE_ID(IN[0]); // Устанавливаем ID инстанса для первого вершины
                UNITY_TRANSFER_INSTANCE_ID(IN[0], o); // Передаем ID инстанса
                
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                triStream.Append(o);
                
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                triStream.Append(o);
                
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                triStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i); // Устанавливаем ID инстанса
                
                // Используем инстансированные свойства
                float wireframeVal = UNITY_ACCESS_INSTANCED_PROP(Props, _WireframeVal);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                float centerAlpha = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterAlpha);
                
                if(!any(bool3(i.bary.x < wireframeVal, i.bary.y < wireframeVal, i.bary.z < wireframeVal)))
                    return fixed4(color.r, color.g, color.b, centerAlpha);

                return color;
            }
            ENDCG
        }

        Pass {
            Cull Back
            Blend SrcAlpha One
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma multi_compile_instancing // Добавляем поддержку инстансинга

            // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            // Объявляем буфер для свойств инстансинга
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _WireframeVal)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _CenterAlpha)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct v2g {
                float4 worldPos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса для фрагментного шейдера
            };

            v2g vert(appdata_base v) {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                float3 param = float3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;
                UNITY_SETUP_INSTANCE_ID(IN[0]); // Устанавливаем ID инстанса для первого вершины
                UNITY_TRANSFER_INSTANCE_ID(IN[0], o); // Передаем ID инстанса
                
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                triStream.Append(o);
                
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                triStream.Append(o);
                
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                triStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i); // Устанавливаем ID инстанса
                
                // Используем инстансированные свойства
                float wireframeVal = UNITY_ACCESS_INSTANCED_PROP(Props, _WireframeVal);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                float centerAlpha = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterAlpha);
                
                if(!any(bool3(i.bary.x <= wireframeVal, i.bary.y <= wireframeVal, i.bary.z <= wireframeVal)))
                    return fixed4(color.r, color.g, color.b, centerAlpha);

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}