Shader "Mobile/Bumped Specular Custom Alpha Support" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Color ("Main Color", Color) = (1,1,1,1)
    [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}

    _SpecCustomColor ("Specular Color", Color) = (1,1,1,1)
    _SpecGlossMap ("Specular", 2D) = "white" {}
    _Shininess ("Shininess", Range (0, 1)) = 0
    
    // Emission properties with toggle
    [Toggle(USE_EMISSION)] _UseEmission ("Enable Emission", Float) = 0
    [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    _EmissionMap ("Emission", 2D) = "white" {}

    // Rendering options
    [Enum(Opaque,0,Transparent,1,Cutout,2,Fade,3)] _RenderType("Render Type", Float) = 0
    _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    _Transparency("Transparency", Range(0,1)) = 1.0
}

SubShader {
    Tags { 
        "RenderType" = "Opaque"
        "PerformanceChecks" = "False"
        "Queue" = "Geometry"
    }
    
    LOD 250
    
    CGPROGRAM
    #pragma surface surf MobileBlinnPhong alpha exclude_path:prepass halfasview novertexlights
    #pragma shader_feature USE_EMISSION
    #pragma shader_feature _ALPHATEST_ON
    #pragma shader_feature _ALPHABLEND_ON
    
    half4 _Color;
    half4 _SpecCustomColor;
    
    sampler2D _MainTex;
    sampler2D _BumpMap;
    sampler2D _EmissionMap;
    sampler2D _SpecGlossMap;    
    half _Shininess;
    half4 _EmissionColor;
    half _Cutoff;
    half _Transparency;

    struct Input {
        float2 uv_MainTex;
    };

    inline half4 LightingMobileBlinnPhong (SurfaceOutput s, half3 lightDir, half3 halfDir, half atten)
    {
        half diff = max(0, dot(s.Normal, lightDir));
        half nh = max(0, dot(s.Normal, halfDir));

        half specPower = exp2(10 * _Shininess + 4);   // 16-1024
        half spec = pow(nh, specPower) * s.Gloss;
        
        half4 c;
        c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
        
        // Поддержка Fade рендеринга
        #if defined(_ALPHABLEND_ON)
            c.a = s.Alpha * _Transparency;
        #else
            c.a = s.Alpha;
        #endif
        
        return c;
    }
    
    void surf (Input IN, inout SurfaceOutput o) {
        half4 texColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        half4 specGloss = tex2D(_SpecGlossMap, IN.uv_MainTex);
        
        specGloss.rgb *= _SpecCustomColor.rgb;
        
        #if defined(_ALPHATEST_ON)
            clip(texColor.a - _Cutoff);
        #endif

        o.Albedo = texColor.rgb;
        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
        o.Specular = specGloss.rgb;
        o.Gloss = specGloss.a * _Shininess;
        o.Alpha = texColor.a;

        #if defined(USE_EMISSION)
            half4 e = tex2D(_EmissionMap, IN.uv_MainTex);
            o.Emission = e.rgb * _EmissionColor.rgb;
        #else
            o.Emission = half3(0,0,0);
        #endif
    }
    ENDCG
}

CustomEditor "MobileBumpedSpecularGUI"
FallBack "Mobile/VertexLit"
}