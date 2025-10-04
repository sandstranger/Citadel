Shader "Custom/Geometry/WireframeOverlay" {
    Properties {
        [PowerSlider(3.0)]
        _WireframeVal ("Wireframe width", Range(0., 0.5)) = 0.05
        _Color ("Wire color", color) = (1., 1., 1., 1.)
        [Toggle] _RemoveDiag("Remove diagonals?", Float) = 0.
    }
    SubShader {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }

        Pass {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma target 4.0
            #pragma multi_compile_instancing

            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half, _WireframeVal)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct v2g {
                float4 worldPos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct g2f {
                float4 pos : SV_POSITION;
                half3 bary : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2g vert(appdata_base v) {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); // КРИТИЧЕСКИ ВАЖНО: передаем ID
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                UNITY_SETUP_INSTANCE_ID(IN[0]);
                UNITY_SETUP_INSTANCE_ID(IN[1]);
                UNITY_SETUP_INSTANCE_ID(IN[2]);

                half3 param = half3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                half EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                half EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                half EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;
                UNITY_TRANSFER_INSTANCE_ID(IN[0], o);
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = half3(1., 0., 0.) + param;
                triStream.Append(o);
                
                UNITY_TRANSFER_INSTANCE_ID(IN[1], o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = half3(0., 0., 1.) + param;
                triStream.Append(o);
                
                UNITY_TRANSFER_INSTANCE_ID(IN[2], o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = half3(0., 1., 0.) + param;
                triStream.Append(o);
                
                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                
                half wireframeVal = UNITY_ACCESS_INSTANCED_PROP(Props, _WireframeVal);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                
                if(!any(bool3(i.bary.x < wireframeVal, i.bary.y < wireframeVal, i.bary.z < wireframeVal)))
                    discard;

                return color;
            }
            ENDCG
        }

        Pass {
            Cull Back
            // Убрал ZTest GEqual для более предсказуемого поведения
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma target 4.0
            #pragma multi_compile_instancing

            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half, _WireframeVal)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct v2g {
                float4 worldPos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct g2f {
                float4 pos : SV_POSITION;
                half3 bary : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2g vert(appdata_base v) {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); // КРИТИЧЕСКИ ВАЖНО: передаем ID
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                UNITY_SETUP_INSTANCE_ID(IN[0]);
                UNITY_SETUP_INSTANCE_ID(IN[1]);
                UNITY_SETUP_INSTANCE_ID(IN[2]);

                half3 param = half3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                half EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                half EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                half EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;
                
                // Первая вершина
                UNITY_TRANSFER_INSTANCE_ID(IN[0], o);
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = half3(1., 0., 0.) + param;
                triStream.Append(o);
                
                // Вторая вершина
                UNITY_TRANSFER_INSTANCE_ID(IN[1], o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = half3(0., 0., 1.) + param;
                triStream.Append(o);
                
                // Третья вершина
                UNITY_TRANSFER_INSTANCE_ID(IN[2], o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = half3(0., 1., 0.) + param;
                triStream.Append(o);
                
                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                
                half wireframeVal = UNITY_ACCESS_INSTANCED_PROP(Props, _WireframeVal);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                
                if(!any(bool3(i.bary.x <= wireframeVal, i.bary.y <= wireframeVal, i.bary.z <= wireframeVal)))
                    discard;

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}