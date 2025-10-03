Shader "Deferred/Grass" {
    Properties {
        _Color ("Color Tint", Color) = (1,1,1,1)
        _BladeWidth("Blade Width", Float) = 0.05
        _BladeWidthRandom("Blade Width Random", Float) = 0.02
        _BladeHeight("Blade Height", Float) = 0.5
        _BladeHeightRandom("Blade Height Random", Float) = 0.3
        _BladeForward("Blade Forward Amount", Float) = 0.38
        _BladeCurve("Blade Curvature Amount", Range(0, 4)) = 2
        _BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #include "UnityPBSLighting.cginc"
    
    float _BladeHeight;
    float _BladeHeightRandom;
    float _BladeWidthRandom;
    float _BladeWidth;
    float _BladeForward;
    float _BladeCurve;
    float _BendRotationRandom;

    // Добавляем поддержку GPU Instancing для свойства _Color
    UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
    UNITY_INSTANCING_BUFFER_END(Props)

    struct vertexInput {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float4 tangent : TANGENT;
        UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
    };

    vertexInput vert(vertexInput v) {
        vertexInput o;
        UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
        UNITY_TRANSFER_INSTANCE_ID(v, o); // Передаем в вывод
        o.vertex = v.vertex;
        o.normal = v.normal;
        o.tangent = v.tangent;
        return o;
    }

    struct geometryOutput {
        float4 pos : SV_POSITION;
        float3 normal : TEXCOORD1;
        float4 tangent : TANGENT;
        UNITY_VERTEX_INPUT_INSTANCE_ID // Передаем ID в геометрический шейдер
    };

    float rand(float3 co) {
        return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
    }

    float3x3 AngleAxis3x3(float angle, float3 axis) {
        float c, s;
        sincos(angle, s, c);
        float t = 1 - c;
        float x = axis.x;
        float y = axis.y;
        float z = axis.z;
        return float3x3(
            t * x * x + c, t * x * y - s * z, t * x * z + s * y,
            t * x * y + s * z, t * y * y + c, t * y * z - s * x,
            t * x * z - s * y, t * y * z + s * x, t * z * z + c
        );
    }

    // ИСПРАВЛЕНИЕ: Добавляем параметр input для передачи данных инстанса
    geometryOutput GenerateGrassVertex(float3 vertexPosition, float width, float height, float forward, float3x3 transformMatrix, float4 tangent, vertexInput input) {
        geometryOutput o;
        float3 tangentPoint = float3(width, forward, height);
        float3 tangentNormal = float3(0,-1,0);
        float3 localNormal = normalize(mul(transformMatrix, tangentNormal));
        float3 localOffset = mul(transformMatrix, tangentPoint);
        float3 localPosition = vertexPosition + localOffset;
        o.pos = UnityObjectToClipPos(localPosition);
        o.normal = UnityObjectToWorldNormal(localNormal);
        o.tangent = float4(UnityObjectToWorldDir(tangent.xyz), tangent.w);
        
        // ИСПРАВЛЕНИЕ: Правильно передаем ID инстанса
        UNITY_TRANSFER_INSTANCE_ID(input, o);
        return o;
    }

    #define BLADE_SEGMENTS 3
    [maxvertexcount(BLADE_SEGMENTS * 2 + 4)]
    void geo(triangle vertexInput IN[3], inout TriangleStream<geometryOutput> triStream) {
        // Устанавливаем ID инстанса для геометрического шейдера
        UNITY_SETUP_INSTANCE_ID(IN[0]);
        
        float3 pos0 = IN[0].vertex.xyz;
        float3 pos1 = IN[1].vertex.xyz;
        float3 pos2 = IN[2].vertex.xyz;
        float3 pos = pos0;
        if (pos1.x < pos.x) pos = pos1;
        if (pos2.x < pos.x) pos = pos2;

        float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
        float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));

        float3 vNormal = IN[0].normal;
        float4 vTangent = IN[0].tangent;
        float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;
        float3x3 tangentToLocal = float3x3(
            vTangent.x, vBinormal.x, vNormal.x,
            vTangent.y, vBinormal.y, vNormal.y,
            vTangent.z, vBinormal.z, vNormal.z
        );

        float3x3 transformationMatrix = mul(mul(tangentToLocal, facingRotationMatrix), bendRotationMatrix);
        float3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotationMatrix);

        float height = abs(rand(pos) * 2 - 1) * _BladeHeightRandom + _BladeHeight;
        float width = abs(rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth;
        float forward = rand(pos.yyz) * _BladeForward;

        for (int i = 0; i < BLADE_SEGMENTS - 1; i++) {
            float t = i / (float)(BLADE_SEGMENTS - 1);

            float segmentHeight = height * t;
            float segmentWidth = width * (1 - t * 0.5);
            float segmentForward = pow(t, _BladeCurve) * forward;

            float3x3 transformMatrix = i == 0 ? transformationMatrixFacing : transformationMatrix;

            // ИСПРАВЛЕНИЕ: Передаем IN[0] как параметр input
            triStream.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, transformMatrix, vTangent, IN[0]));
            triStream.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, transformMatrix, vTangent, IN[0]));
        }

        float baseT = (BLADE_SEGMENTS - 1) / (float)(BLADE_SEGMENTS - 1);
        float baseHeight = height * baseT * 0.9;
        float baseWidth = width * (1 - baseT * 0.5);
        float baseForward = pow(baseT, _BladeCurve) * forward;

        float tipHeight = height;
        float tipWidth = width * 0.3;
        float tipForward = pow(1.0, _BladeCurve) * forward * 2;

        // ИСПРАВЛЕНИЕ: Передаем IN[0] как параметр input
        triStream.Append(GenerateGrassVertex(pos, baseWidth, baseHeight, baseForward, transformationMatrix, vTangent, IN[0]));
        triStream.Append(GenerateGrassVertex(pos, -baseWidth, baseHeight, baseForward, transformationMatrix, vTangent, IN[0]));
        triStream.Append(GenerateGrassVertex(pos, tipWidth, tipHeight, tipForward, transformationMatrix, vTangent, IN[0]));
        triStream.Append(GenerateGrassVertex(pos, -tipWidth, tipHeight, tipForward, transformationMatrix, vTangent, IN[0]));
        triStream.RestartStrip();
    }
    ENDCG

    SubShader {
        Pass {
            Tags {"LightMode"="Deferred"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment pixel_shader
            #pragma geometry geo
            #pragma target 4.0
            #pragma exclude_renderers nomrt
            #pragma multi_compile ___ UNITY_HDR_ON
            #pragma multi_compile_instancing // Включаем GPU Instancing
            
            #include "UnityPBSLighting.cginc"

            struct structurePS {
                half4 albedo : SV_Target0;
                half4 specular : SV_Target1;
                half4 normal : SV_Target2;
                half4 emission : SV_Target3;
            };
            
            structurePS pixel_shader(geometryOutput vs) {
                // Устанавливаем ID инстанса для фрагментного шейдера
                UNITY_SETUP_INSTANCE_ID(vs);
                
                structurePS ps;
                float3 normalDirection = normalize(vs.normal);
                
                // Получаем цвет через GPU Instancing
                float4 instanceColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                
                ps.albedo = instanceColor;
                ps.specular = 0;
                ps.normal = float4(normalDirection * 0.5 + 0.5, 1.0);
                ps.emission = instanceColor * 0.25;
                ps.emission.a = 1;
                #ifndef UNITY_HDR_ON
                    ps.emission.rgb = exp2(-ps.emission.rgb);
                #endif
                return ps;
            }
            ENDCG
        }

        // Shadow Caster Pass
        Pass {
            Tags {"LightMode" = "ShadowCaster"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment shadow_fragment
            #pragma geometry geo
            #pragma target 4.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing // Включаем GPU Instancing для теней

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct v2f {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 shadow_fragment(v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}