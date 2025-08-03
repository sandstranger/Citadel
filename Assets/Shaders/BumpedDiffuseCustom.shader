Shader "Mobile/Bumped Diffuse Custom" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Color ("Main Color", Color) = (1,1,1,1)
    [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
    
    // Emission properties with toggle
    [Toggle(USE_EMISSION)] _UseEmission ("Enable Emission", Float) = 0
    [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    _EmissionMap ("Emission", 2D) = "white" {}

    // Rendering options
    [Enum(Opaque,0,Transparent,1,Cutout,2)] _RenderType("Render Type", Float) = 0
    _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
}

SubShader {
    Tags { 
        "RenderType" = "Opaque"
        "PerformanceChecks" = "False"
    }
    LOD 250

    CGPROGRAM

    #pragma surface surf Lambert noforwardadd
    #pragma shader_feature USE_EMISSION
    #pragma shader_feature _ALPHATEST_ON
    
    sampler2D _MainTex;
    sampler2D _BumpMap;
    sampler2D _EmissionMap;
    half4 _Color;
    half4 _EmissionColor;
    half _Cutoff;

    struct Input {
        float2 uv_MainTex;
    };
    
    void surf (Input IN, inout SurfaceOutput o) {
        half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

        #if defined(_ALPHATEST_ON)
            clip(c.a - _Cutoff);
        #endif
        
        o.Albedo = c.rgb;
        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
        
        // Эмиссия
        #if defined(USE_EMISSION)
            half4 e = tex2D(_EmissionMap, IN.uv_MainTex);
            o.Emission = e.rgb * _EmissionColor.rgb;
        #else
            o.Emission = half3(0,0,0);
        #endif
        
        o.Alpha = c.a;
    }
    ENDCG
}

CustomEditor "MobileBumpedDiffuseGUI"
FallBack "Mobile/VertexLit"
}