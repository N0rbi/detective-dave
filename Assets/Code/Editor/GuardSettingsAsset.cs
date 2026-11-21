using DetectiveDave.GuardPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class GuardSettingsAsset
    {
        public static GuardSettings EnsureWithSprites(Sprite[] walkFrames, Sprite[] chaseFrames)
        {
            var settings = AssetDatabase.LoadAssetAtPath<GuardSettings>(GuardSetupPaths.SettingsPath);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<GuardSettings>();
                AssetDatabase.CreateAsset(settings, GuardSetupPaths.SettingsPath);
            }
            var serialized = new SerializedObject(settings);
            AssignFrames(serialized.FindProperty("walkFrames"), walkFrames);
            AssignFrames(serialized.FindProperty("chaseFrames"), chaseFrames);
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
