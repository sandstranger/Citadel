Shader "Deferred/Grass"
{
    Properties
    {
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
    #include "UnityShaderVariables.cginc"

    UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
        UNITY_DEFINE_INSTANCED_PROP(half, _BladeHeight)
        UNITY_DEFINE_INSTANCED_PROP(half, _BladeHeightRandom)
        UNITY_DEFINE_INSTANCED_PROP(half, _BladeWidthRandom)
        UNITY_DEFINE_INSTANCED_PROP(half, _BladeWidth)
        UNITY_DEFINE_INSTANCED_PROP(half, _BladeForward)
        UNITY_DEFINE_INSTANCED_PROP(half, _BladeCurve)
        UNITY_DEFINE_INSTANCED_PROP(half, _BendRotationRandom)
    UNITY_INSTANCING_BUFFER_END(Props)

    struct vertexInput
    {
        float4 vertex : POSITION;
        half3 normal : NORMAL;
        half4 tangent : TANGENT;
        UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
    };

    vertexInput vert(vertexInput v)
    {
        vertexInput o;
        UNITY_SETUP_INSTANCE_ID(v); // Устанавливаем ID инстанса
        UNITY_TRANSFER_INSTANCE_ID(v, o); // Передаем в вывод
        o.vertex = v.vertex;
        o.normal = v.normal;
        o.tangent = v.tangent;
        return o;
    }

    struct geometryOutput
    {
        float4 pos : SV_POSITION;
        half3 normal : TEXCOORD1;
        half4 tangent : TANGENT;
        UNITY_VERTEX_INPUT_INSTANCE_ID // Передаем ID в геометрический шейдер
    };

    half rand(half3 co)
    {
        return frac(sin(dot(co.xyz, half3(12.9898, 78.233, 53.539))) * 43758.5453);
    }

    half3x3 AngleAxis3x3(half angle, half3 axis)
    {
        half c, s;
        sincos(angle, s, c);
        half t = 1 - c;
        half x = axis.x;
        half y = axis.y;
        half z = axis.z;
        return half3x3(
            t * x * x + c, t * x * y - s * z, t * x * z + s * y,
            t * x * y + s * z, t * y * y + c, t * y * z - s * x,
            t * x * z - s * y, t * y * z + s * x, t * z * z + c
        );
    }

    // ИСПРАВЛЕНИЕ: Добавляем параметр input для передачи данных инстанса
    geometryOutput GenerateGrassVertex(half3 vertexPosition, half width, half height, half forward,
                                                       half3x3 transformMatrix, half4 tangent, vertexInput input)
    {
        geometryOutput o;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, o);
        half3 tangentPoint = half3(width, forward, height);
        half3 tangentNormal = half3(0,-1,0);
        half3 localNormal = normalize(mul(transformMatrix, tangentNormal));
        half3 localOffset = mul(transformMatrix, tangentPoint);
        half3 localPosition = vertexPosition + localOffset;
        o.pos = UnityObjectToClipPos(localPosition);
        o.normal = UnityObjectToWorldNormal(localNormal);
        o.tangent = half4(UnityObjectToWorldDir(tangent.xyz), tangent.w);
        return o;
    }

    #define BLADE_SEGMENTS 3

    [maxvertexcount(BLADE_SEGMENTS * 2 + 4)]
    void geo(triangle vertexInput IN[3], inout TriangleStream<geometryOutput> triStream)
    {
        // Устанавливаем ID инстанса для геометрического шейдера
        UNITY_SETUP_INSTANCE_ID(IN[0]);
        UNITY_SETUP_INSTANCE_ID(IN[1]);
        UNITY_SETUP_INSTANCE_ID(IN[2]);

        half3 pos0 = IN[0].vertex.xyz;
        half3 pos1 = IN[1].vertex.xyz;
        half3 pos2 = IN[2].vertex.xyz;
        half3 pos = pos0;
        if (pos1.x < pos.x) pos = pos1;
        if (pos2.x < pos.x) pos = pos2;

        half3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, half3(0, 0, 1));
        half3x3 bendRotationMatrix =
            AngleAxis3x3(rand(pos.zzx) * UNITY_ACCESS_INSTANCED_PROP(Props, _BendRotationRandom) * UNITY_PI * 0.5, half3(-1, 0, 0));

        half3 vNormal = IN[0].normal;
        half4 vTangent = IN[0].tangent;
        half3 vBinormal = cross(vNormal, vTangent) * vTangent.w;
        half3x3 tangentToLocal = half3x3(
            vTangent.x, vBinormal.x, vNormal.x,
            vTangent.y, vBinormal.y, vNormal.y,
            vTangent.z, vBinormal.z, vNormal.z
        );

        half3x3 transformationMatrix = mul(mul(tangentToLocal, facingRotationMatrix), bendRotationMatrix);
        half3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotationMatrix);

        half bladeHeightRandom = UNITY_ACCESS_INSTANCED_PROP(Props, _BladeHeightRandom);
        half bladeWidth = UNITY_ACCESS_INSTANCED_PROP(Props, _BladeWidth);
        half bladeWidthRandom = UNITY_ACCESS_INSTANCED_PROP(Props, _BladeWidthRandom);
        half bladeHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _BladeHeight);
        half bladeForward = UNITY_ACCESS_INSTANCED_PROP(Props, _BladeForward);
        half height = abs(rand(pos) * 2 - 1) * bladeHeightRandom + bladeHeight;
        half width = abs(rand(pos.xzy) * 2 - 1) * bladeWidthRandom + bladeWidth;
        half forward = rand(pos.yyz) * bladeForward;
        half bladeCurve = UNITY_ACCESS_INSTANCED_PROP(Props, _BladeCurve);
        
        for (int i = 0; i < BLADE_SEGMENTS - 1; i++)
        {
            half t = i / (half)(BLADE_SEGMENTS - 1);

            half segmentHeight = height * t;
            half segmentWidth = width * (1 - t * 0.5);
            half segmentForward = pow(t, bladeCurve) * forward;

            half3x3 transformMatrix = i == 0 ? transformationMatrixFacing : transformationMatrix;

            // ИСПРАВЛЕНИЕ: Передаем IN[0] как параметр input
            triStream.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, transformMatrix,
                                   vTangent, IN[0]));
            triStream.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, transformMatrix,
                                                          vTangent, IN[0]));
        }

        half baseT = (BLADE_SEGMENTS - 1) / (half)(BLADE_SEGMENTS - 1);
        half baseHeight = height * baseT * 0.9;
        half baseWidth = width * (1 - baseT * 0.5);
        half baseForward = pow(baseT, bladeCurve) * forward;

        half tipHeight = height;
        half tipWidth = width * 0.3;
        half tipForward = pow(1.0, bladeCurve) * forward * 2;

        // ИСПРАВЛЕНИЕ: Передаем IN[0] как параметр input
        triStream.Append(GenerateGrassVertex(pos, baseWidth, baseHeight, baseForward, transformationMatrix, vTangent,
    IN[0]));
        triStream.Append(GenerateGrassVertex(pos, -baseWidth, baseHeight, baseForward, transformationMatrix, vTangent,
           IN[0]));
        triStream.Append(GenerateGrassVertex(pos, tipWidth, tipHeight, tipForward, transformationMatrix, vTangent,
                         IN[0]));
        triStream.Append(GenerateGrassVertex(pos, -tipWidth, tipHeight, tipForward, transformationMatrix, vTangent,
                                             IN[0]));
        triStream.RestartStrip();
    }
    ENDCG

    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode"="Deferred"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment pixel_shader
            #pragma geometry geo
            #pragma target 4.0
            #pragma exclude_renderers nomrt
            #pragma multi_compile ___ UNITY_HDR_ON
            #pragma multi_compile_instancing // Включаем GPU Instancing

            #include "UnityPBSLighting.cginc"

            struct structurePS
            {
                float4 albedo : SV_Target0;
                float4 specular : SV_Target1;
                float4 normal : SV_Target2;
                float4 emission : SV_Target3;
            };

            structurePS pixel_shader(geometryOutput vs)
            {
                UNITY_SETUP_INSTANCE_ID(vs);

                structurePS ps;
                half3 normalDirection = normalize(vs.normal);

                // Получаем цвет через GPU Instancing
                half4 instanceColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                ps.albedo = instanceColor;
                ps.specular = 0;
                ps.normal = half4(normalDirection * 0.5 + 0.5, 1.0);
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
        Pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment shadow_fragment
            #pragma geometry geo
            #pragma target 4.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing // Включаем GPU Instancing для теней

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            half4 shadow_fragment(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                    SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}