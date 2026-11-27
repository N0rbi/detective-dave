using System.Linq;
using DetectiveDave.MusicPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class DeathHookSetup
    {
        public static void Apply(MusicSettings settings)
        {
            var property = new SerializedObject(settings).FindProperty("deathHook.hookClip");
            if (property.objectReferenceValue) return;
            var clip = FindHookClip();
            if (!clip) return;
            clip = MoveToStingers(clip);
            property.objectReferenceValue = clip;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        static AudioClip MoveToStingers(AudioClip clip)
        {
            var path = AssetDatabase.GetAssetPath(clip);
            if (!path.StartsWith(MusicSetupPaths.StagingFolder + "/", System.StringComparison.Ordinal)) return clip;
            if (!AssetDatabase.IsValidFolder(MusicSetupPaths.StingerFolder)) AssetDatabase.CreateFolder(MusicSetupPaths.MusicFolder, "Stingers");
            var target = MusicSetupPaths.StingerFolder + "/" + System.IO.Path.GetFileName(path);
            return string.IsNullOrEmpty(AssetDatabase.MoveAsset(path, target)) ? AssetDatabase.LoadAssetAtPath<AudioClip>(target) : clip;
        }

        static AudioClip FindHookClip()
        {
            var folders = new[] { MusicSetupPaths.StingerFolder, MusicSetupPaths.StagingFolder }.Where(AssetDatabase.IsValidFolder).ToArray();
            if (folders.Length == 0) return null;
            return AssetDatabase.FindAssets("t:AudioClip", folders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.ToLowerInvariant().Contains(MusicSetupPaths.DeathHookKeyword))
                .OrderBy(path => path.StartsWith(MusicSetupPaths.StingerFolder) ? 0 : 1)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
                .FirstOrDefault();
        }
    }
}
