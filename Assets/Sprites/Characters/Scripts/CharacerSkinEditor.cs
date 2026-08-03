using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterSkin))]
public class CharacterSkinEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CharacterSkin skin = (CharacterSkin)target;

        if (GUILayout.Button("Extract Lobby Sprite from Body Controller"))
        {
            if (skin.bodyController == null)
            {
                Debug.LogWarning("No body controller assigned.");
                return;
            }

            Sprite extracted = ExtractFirstSprite(skin.bodyController);
            if (extracted != null)
            {
                skin.lobbySprite = extracted;
                EditorUtility.SetDirty(skin);
                AssetDatabase.SaveAssets();
                Debug.Log($"Extracted lobby sprite: {extracted.name}");
            }
            else
            {
                Debug.LogWarning("Could not extract sprite from body controller.");
            }
        }
    }

    static Sprite ExtractFirstSprite(RuntimeAnimatorController controller)
    {
        if (controller.animationClips.Length == 0) return null;

        foreach (var clip in controller.animationClips)
        {
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in bindings)
            {
                var frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                if (frames.Length > 0 && frames[0].value is Sprite sprite)
                    return sprite;
            }
        }
        return null;
    }
}