#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpellScalingConfig))]
public class SpellScalingConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var config = (SpellScalingConfig)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("预览（技能体积倍率）", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "每颗豆：sizeMultiplier += amount（默认 0.1）。\n" +
            "技能倍率 = 1 + (sizeMultiplier-1) × 转化率，最高「上限」。\n" +
            "默认 0.5 转化率 + 0.1/豆 → 约 20 颗到 2×。自爆 BOOM 范围同样吃该倍率。",
            MessageType.Info);

        int toMax = 0;
        for (int n = 1; n <= 40; n++)
        {
            if (config.PreviewSpellScaleAfterPickups(n) >= config.maxSizeScale - 0.001f)
            {
                toMax = n;
                break;
            }
        }
        if (toMax > 0)
            EditorGUILayout.LabelField("约到上限需要", $"{toMax} 颗 Size 豆");

        for (int n = 1; n <= 8; n++)
        {
            float scale = config.PreviewSpellScaleAfterPickups(n);
            EditorGUILayout.LabelField($"{n} 颗 Size 豆", $"{scale:F3}×  ({(scale - 1f) * 100f:+0.#;-0.#}%)");
        }
    }
}
#endif
