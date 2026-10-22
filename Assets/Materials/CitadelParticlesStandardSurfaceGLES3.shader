Shader "Citadel/Particles/Standard Surface GLES3"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [HideInInspector] _SoftParticleFadeParams ("Soft Particle Fade Params", Vector) = (0,1,0,0)
        [HideInInspector] _CameraFadeParams ("Camera Fade Params", Vector) = (0,1,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
        }

        LOD 100
        Cull [_Cull]

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Standard alpha:fade noforwardadd nolightmap nometa keepalpha
        #pragma multi_compile __ SOFTPARTICLES_ON
        #pragma shader_feature_local _FADING_ON

        #include "UnityCG.cginc"

        sampler2D _MainTex;
        fixed4 _Color;
        half _Metallic;
        half _Glossiness;
        float4 _SoftParticleFadeParams;
        float4 _CameraFadeParams;

        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color : COLOR;
            float4 screenPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color * _Color;

            #if defined(SOFTPARTICLES_ON) && defined(_FADING_ON)
                float sceneZ = LinearEyeDepth(
                    SAMPLE_DEPTH_TEXTURE_PROJ(
                        _CameraDepthTexture,
                        UNITY_PROJ_COORD(IN.screenPos)
                    )
                );
                float particleZ = LinearEyeDepth(IN.screenPos.z / IN.screenPos.w);
                float softFade = saturate(
                    _SoftParticleFadeParams.y *
                    ((sceneZ - _SoftParticleFadeParams.x) - particleZ)
                );
                c.a *= softFade;
            #endif

            #if defined(_FADING_ON)
                float cameraZ = LinearEyeDepth(IN.screenPos.z / IN.screenPos.w);
                float cameraFade = saturate(
                    (cameraZ - _CameraFadeParams.x) * _CameraFadeParams.y
                );
                c.a *= cameraFade;
            #endif

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Occlusion = 1.0;
            o.Emission = 0.0;
            o.Alpha = c.a;
        }
        ENDCG
    }
}
