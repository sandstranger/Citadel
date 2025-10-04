Shader "Custom/HighlightShader" {
 Properties 
 {
  _ColorTint("Color Tint", Color) = (1, 1, 1, 1)
  _MainTex("Base (RGB)", 2D) = "white" {}
  _RimColor("Rim Color", Color) = (1, 1, 1, 1)
  _RimPower("Rim Power", Range(1.0, 6.0)) = 3.0
  _HSVAAdjust("HSVA Adjust", Vector) = (0,0,0,0)
 }
 SubShader {

  Tags { "RenderType"="Opaque" }

  CGPROGRAM
  #pragma surface surf Lambert
  #pragma target 3.5
  #pragma multi_compile_instancing // Добавляем поддержку инстансинга

  struct Input {
   half4 color : Color;
   half2 uv_MainTex;
   half3 viewDir;
   UNITY_VERTEX_INPUT_INSTANCE_ID // Добавляем ID инстанса
  };

  // Объявляем буфер для свойств инстансинга
  UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(half4, _ColorTint)
    UNITY_DEFINE_INSTANCED_PROP(half4, _RimColor)
    UNITY_DEFINE_INSTANCED_PROP(half, _RimPower)
    UNITY_DEFINE_INSTANCED_PROP(half4, _HSVAAdjust)
  UNITY_INSTANCING_BUFFER_END(Props)

  sampler2D _MainTex;

  void surf (Input IN, inout SurfaceOutput o) 
  {
    UNITY_SETUP_INSTANCE_ID(IN); // Устанавливаем ID инстанса
    
    // Используем инстансированные свойства
    half4 instancedColorTint = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTint);
    half4 instancedRimColor = UNITY_ACCESS_INSTANCED_PROP(Props, _RimColor);
    half instancedRimPower = UNITY_ACCESS_INSTANCED_PROP(Props, _RimPower);
    half4 instancedHSVAAdjust = UNITY_ACCESS_INSTANCED_PROP(Props, _HSVAAdjust);

    IN.color = instancedColorTint;
    IN.color.x += instancedHSVAAdjust.x;
    IN.color.y += instancedHSVAAdjust.y;
    IN.color.z += instancedHSVAAdjust.z;
    IN.color.a += instancedHSVAAdjust.a;
    
    o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * IN.color;

    half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
    o.Emission = instancedRimColor.rgb * pow(rim, instancedRimPower);
  }
  ENDCG
 } 
 FallBack "Diffuse"
}