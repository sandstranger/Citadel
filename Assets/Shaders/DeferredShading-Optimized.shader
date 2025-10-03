Shader "Hidden/Internal-DeferredShading-Optimized"
{
    Properties
    {
        _LightTexture0 ("", any) = "" {}
        _LightTextureB0 ("", 2D) = "" {}
        _ShadowMapTexture ("", any) = "" {}
        _SrcBlend ("", Float) = 1
        _DstBlend ("", Float) = 1
    }
    SubShader
    {

        Pass
        {
            ZWrite Off
            Blend [_SrcBlend] [_DstBlend]

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_deferred
            #pragma fragment frag
            #pragma multi_compile_lightpass
            #pragma exclude_renderers nomrt
            #pragma multi_compile ___ UNITY_HDR_ON

            #include "UnityCG.cginc"
            #include "UnityDeferredLibrary.cginc"
            #include "UnityLightingCommon.cginc"

            sampler2D _CameraGBufferTexture0;
            sampler2D _CameraGBufferTexture2;

            half4 CalculateLightMobile(unity_v2f_deferred i)
            {
                half3 wpos;
                half2 uv;
                half atten, fadeDist;
                UnityLight light;
                UNITY_INITIALIZE_OUTPUT(UnityLight, light);
                UnityDeferredCalculateLightParams(i, wpos, uv, light.dir, atten, fadeDist);
                light.color = _LightColor.rgb * atten;
                half4 gbuffer0 = tex2D(_CameraGBufferTexture0, uv);
                half4 gbuffer2 = tex2D(_CameraGBufferTexture2, uv);
                half3 diffuseColor = gbuffer0.rgb;
                half3 normalWorld = gbuffer2.rgb * 2 - 1;
                half ndotl = saturate(dot(normalWorld, light.dir));
                half3 diffuse = diffuseColor * ndotl;
                return half4(diffuse * light.color, 1);
            }

            #ifdef UNITY_HDR_ON
half4
            #else
            fixed4
            #endif
            frag(unity_v2f_deferred i) : SV_Target
            {
                half4 c = CalculateLightMobile(i);
                #ifdef UNITY_HDR_ON
    return c;
                #else
                return exp2(-c);
                #endif
            }
            ENDCG
        }

        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Stencil
            {
                ref [_StencilNonBackground]
                readmask [_StencilNonBackground]
                compback equal
                compfront equal
            }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers nomrt

            #include "UnityCG.cginc"

            sampler2D _LightBuffer;

            struct v2f
            {
                half4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
            };

            v2f vert(float4 vertex : POSITION, float2 texcoord : TEXCOORD0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.texcoord = texcoord.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 lightBuffer = tex2D(_LightBuffer, i.texcoord);
                return -log2(lightBuffer);
            }
            ENDCG
        }

    }
    Fallback Off
}