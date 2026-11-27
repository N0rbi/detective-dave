using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class PlayerSettingsAsset
    {
        public static PlayerMovementSettings EnsureWithSprites(Sprite[] walkFrames, Sprite[] climbFrames, Sprite deathSprite, Sprite[] decorations)
        {
            var settings = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(PlayerSetupPaths.SettingsPath);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<PlayerMovementSettings>();
                AssetDatabase.CreateAsset(settings, PlayerSetupPaths.SettingsPath);
            }
            var serialized = new SerializedObject(settings);
            AssignFrames(serialized.FindProperty("walkFrames"), walkFrames);
            AssignFrames(serialized.FindProperty("climbFrames"), climbFrames);
            serialized.FindProperty("deathSprite").objectReferenceValue = deathSprite;
            serialized.FindProperty("startGrate").objectReferenceValue = decorations[0];
            serialized.FindProperty("goalIcon").objectReferenceValue = decorations[1];
            serialized.FindProperty("goalBurst").objectReferenceValue = decorations[2];
            serialized.ApplyModifiedProperties();
            return settings;
        }

        static void AssignFrames(SerializedProperty property, Sprite[] frames)
        {
            property.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }
    }
}
