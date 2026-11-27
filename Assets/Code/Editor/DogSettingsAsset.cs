using DetectiveDave.DogPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class DogSettingsAsset
    {
        public static DogSettings EnsureWithSprites(Sprite[] angryFrames, Sprite[] calmFrames)
        {
            var settings = AssetDatabase.LoadAssetAtPath<DogSettings>(DogSetupPaths.SettingsPath);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<DogSettings>();
                AssetDatabase.CreateAsset(settings, DogSetupPaths.SettingsPath);
            }
            var serialized = new SerializedObject(settings);
            AssignFrames(serialized.FindProperty("angryFrames"), angryFrames);
            AssignFrames(serialized.FindProperty("calmFrames"), calmFrames);
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
