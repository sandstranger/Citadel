using UnityEditor;
using UnityEngine;

public class MobileBumpedDiffuseGUI : ShaderGUI
{
    private enum RenderMode
    {
        Opaque,
        Transparent,
        Cutout,
        Fade
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;
        
        // Находим свойства
        MaterialProperty mainTex = FindProperty("_MainTex", properties);
        MaterialProperty color = FindProperty("_Color", properties);
        MaterialProperty bumpMap = FindProperty("_BumpMap", properties);
        
        MaterialProperty useEmission = FindProperty("_UseEmission", properties);
        MaterialProperty emissionMap = FindProperty("_EmissionMap", properties);
        MaterialProperty emissionColor = FindProperty("_EmissionColor", properties);
        
        MaterialProperty renderType = FindProperty("_RenderType", properties);
        MaterialProperty cutoff = FindProperty("_Cutoff", properties);
        MaterialProperty transparency = FindProperty("_Transparency", properties);

        // Основные свойства
        materialEditor.ShaderProperty(color, "Color");
        materialEditor.ShaderProperty(mainTex, "Base Texture");
        materialEditor.ShaderProperty(bumpMap, "Normal Map");
        
        // Эмиссия
        EditorGUILayout.Space();
        materialEditor.ShaderProperty(useEmission, "Enable Emission");
        if (useEmission.floatValue > 0)
        {
            materialEditor.ShaderProperty(emissionMap, "Emission Map");
            materialEditor.ShaderProperty(emissionColor, "Emission Color");
        }
        
        // Настройки рендеринга
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rendering Options", EditorStyles.boldLabel);
        
        // Режим рендеринга
        RenderMode mode = (RenderMode)renderType.floatValue;
        EditorGUI.BeginChangeCheck();
        mode = (RenderMode)EditorGUILayout.EnumPopup("Rendering Mode", mode);
        
        if (EditorGUI.EndChangeCheck())
        {
            renderType.floatValue = (float)mode;
            
            // Устанавливаем ключевые слова и параметры рендеринга
            switch (mode)
            {
                case RenderMode.Opaque:
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    break;
                    
                case RenderMode.Transparent:
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    break;
                    
                case RenderMode.Cutout:
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    break;
                    
                case RenderMode.Fade:
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    break;
            }
        }
        
        // Дополнительные параметры для разных режимов
        switch (mode)
        {
            case RenderMode.Cutout:
                materialEditor.ShaderProperty(cutoff, "Alpha Cutoff");
                break;
                
            case RenderMode.Transparent:
            case RenderMode.Fade:
                materialEditor.ShaderProperty(transparency, "Transparency");
                break;
        }
        
        // Кнопка сброса
        EditorGUILayout.Space();
        if (GUILayout.Button("Reset to Defaults"))
        {
            color.colorValue = Color.white;
            emissionColor.colorValue = Color.black;
            transparency.floatValue = 1.0f;
        }
    }
}