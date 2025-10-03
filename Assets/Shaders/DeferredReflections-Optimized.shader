Shader "Hidden/Internal-DeferredReflections-Optimized"
{
    Properties
    {
        _SrcBlend ("", Float) = 1
        _DstBlend ("", Float) = 1
    }
    SubShader
    {

        Pass
        {
            ZWrite Off
            ZTest LEqual
            Blend [_SrcBlend] [_DstBlend]
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_deferred
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityDeferredLibrary.cginc"

            sampler2D _CameraGBufferTexture1;
            sampler2D _CameraGBufferTexture2;

            fixed3 distanceFromAABB(fixed3 p, fixed3 aabbMin, fixed3 aabbMax)
            {
                return max(max(p - aabbMax, aabbMin - p), fixed3(0.0, 0.0, 0.0));
            }

            fixed4 frag(unity_v2f_deferred i) : SV_Target
            {
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float2 uv = i.uv.xy / i.uv.w;

                fixed depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                depth = Linear01Depth(depth);
                fixed4 viewPos = fixed4(i.ray * depth, 1);
                fixed3 worldPos = mul(unity_CameraToWorld, viewPos).xyz;

                fixed4 gbuffer1 = tex2D(_CameraGBufferTexture1, uv);
                fixed4 gbuffer2 = tex2D(_CameraGBufferTexture2, uv);

                fixed3 normalWorld = gbuffer2.rgb * 2.0 - 1.0;
                fixed smoothness = gbuffer1.a;
                fixed3 specColor = gbuffer1.rgb;

                fixed3 eyeVec = normalize(worldPos - _WorldSpaceCameraPos);
                fixed3 worldNormalRefl = reflect(eyeVec, normalWorld);
                fixed blendDistance = unity_SpecCube1_ProbePosition.w;

                half3 env0 = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldNormalRefl).rgb;
                fixed3 rgb = env0 * specColor * smoothness;
                fixed3 distance = distanceFromAABB(worldPos, unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz);
                fixed falloff = saturate(1.0 - length(distance) / blendDistance);

                return fixed4(rgb, falloff);
            }
            ENDCG
        }

        Pass
        {
            ZWrite Off
            ZTest Always
            Blend [_SrcBlend] [_DstBlend]

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile ___ UNITY_HDR_ON

            #include "UnityCG.cginc"

            sampler2D _CameraReflectionsTexture;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.uv = ComputeScreenPos(o.pos).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_CameraReflectionsTexture, i.uv);
                #ifdef UNITY_HDR_ON
            return fixed4(c.rgb, 0.0);
                #else
                return fixed4(exp2(-c.rgb), 0.0);
                #endif
            }
            ENDCG
        }

    }
    Fallback Off
}