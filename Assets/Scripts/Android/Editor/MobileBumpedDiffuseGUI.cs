using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Android.Tools
{
    public class MobileBumpedDiffuseGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            // Основные свойства
            MaterialProperty mainTex = FindProperty("_MainTex", properties);
            MaterialProperty bumpMap = FindProperty("_BumpMap", properties);
            var mainColor = FindProperty("_Color", properties);

            // Настройки рендера
            MaterialProperty renderType = FindProperty("_RenderType", properties);
            MaterialProperty cutoff = FindProperty("_Cutoff", properties);

            // Свойства эмиссии
            MaterialProperty useEmission = FindProperty("_UseEmission", properties);
            MaterialProperty emissionColor = FindProperty("_EmissionColor", properties);
            MaterialProperty emissionMap = FindProperty("_EmissionMap", properties);

            EditorGUI.BeginChangeCheck();

            // Секция текстур
            editor.TexturePropertySingleLine(new GUIContent("Main Texture"), mainTex);
            editor.ShaderProperty(mainColor, "Main Color");
            editor.TexturePropertySingleLine(new GUIContent("Normal Map"), bumpMap);

            // Настройка типа рендера
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rendering Settings", EditorStyles.boldLabel);
            int mode = (int)renderType.floatValue;
            mode = EditorGUILayout.Popup("Render Type", mode, new[] { "Opaque", "Transparent", "Cutout" });
            renderType.floatValue = mode;

            if (mode == 2) // Cutout
            {
                editor.ShaderProperty(cutoff, "Alpha Cutoff");
            }

            // Секция эмиссии
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
            editor.ShaderProperty(useEmission, "Enable Emission");

            if (useEmission.floatValue > 0.5)
            {
                editor.TexturePropertyWithHDRColor(new GUIContent("Emission Map"), emissionMap, emissionColor, true);
            }
            else
            {
                editor.ShaderProperty(emissionColor, "Emission Color");
            }

            // Применяем изменения
            if (EditorGUI.EndChangeCheck())
            {
                // Обновляем теги рендера и ключевые слова
                foreach (Material mat in editor.targets)
                {
                    // Управление ключевыми словами для эмиссии
                    if (useEmission.floatValue > 0.5)
                        mat.EnableKeyword("USE_EMISSION");
                    else
                        mat.DisableKeyword("USE_EMISSION");

                    // Управление ключевыми словами для альфа-теста
                    if (mode == 2)
                        mat.EnableKeyword("_ALPHATEST_ON");
                    else
                        mat.DisableKeyword("_ALPHATEST_ON");

                    // Настройка блендинга
                    if (mode == 1) // Transparent
                    {
                        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        mat.renderQueue = 3000;
                    }
                    else if (mode == 2) // Cutout
                    {
                        mat.SetInt("_SrcBlend", (int)BlendMode.One);
                        mat.SetInt("_DstBlend", (int)BlendMode.Zero);
                        mat.renderQueue = 2450;
                    }
                    else // Opaque
                    {
                        mat.SetInt("_SrcBlend", (int)BlendMode.One);
                        mat.SetInt("_DstBlend", (int)BlendMode.Zero);
                        mat.renderQueue = 2000;
                    }

                    // Обновление тега RenderType
                    string renderTag = mode == 0 ? "Opaque" :
                        mode == 1 ? "Transparent" : "TransparentCutout";
                    mat.SetOverrideTag("RenderType", renderTag);
                }
            }
        }
    }
}