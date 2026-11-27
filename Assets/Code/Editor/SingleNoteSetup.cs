using System.Collections.Generic;
using System.IO;
using System.Linq;
using DetectiveDave.MusicPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class SingleNoteSetup
    {
        public static SingleNoteReport Apply(MusicSettings settings)
        {
            var problems = new List<string>();
            var moved = MoveFromStaging(problems);
            var scanned = Scan(problems);
            AddNewEntries(settings, scanned);
            return new SingleNoteReport(scanned.Count, moved, problems);
        }

        static List<string> MoveFromStaging(List<string> problems)
        {
            var moved = new List<string>();
            if (!AssetDatabase.IsValidFolder(MusicSetupPaths.StagingFolder)) return moved;
            foreach (var path in StagedSinglePaths())
            {
                EnsureSingleFolder();
                var target = MusicSetupPaths.SingleFolder + "/" + Path.GetFileName(path);
                var error = AssetDatabase.MoveAsset(path, target);
                if (string.IsNullOrEmpty(error)) moved.Add(Path.GetFileName(path));
                else problems.Add($"{Path.GetFileName(path)} could not be moved: {error}");
            }
            return moved;
        }

        static IEnumerable<string> StagedSinglePaths() =>
            AssetDatabase.FindAssets("t:AudioClip", new[] { MusicSetupPaths.StagingFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetFileNameWithoutExtension(path).StartsWith(SinglePitch.FilePrefix, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

        static void EnsureSingleFolder()
        {
            if (!AssetDatabase.IsValidFolder(MusicSetupPaths.SingleFolder)) AssetDatabase.CreateFolder(MusicSetupPaths.PianoFolder, "Single");
        }

        static IReadOnlyList<SingleNoteClip> Scan(List<string> problems)
        {
            var found = new List<SingleNoteClip>();
            if (!AssetDatabase.IsValidFolder(MusicSetupPaths.SingleFolder)) return found;
            foreach (var path in AssetDatabase.FindAssets("t:AudioClip", new[] { MusicSetupPaths.SingleFolder }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (SinglePitch.TryParse(Path.GetFileNameWithoutExtension(path), out var midiPitch, out var problem)) found.Add(new SingleNoteClip(clip, midiPitch));
                else problems.Add(problem);
            }
            return SingleNoteOrder.Sorted(found);
        }

        static void AddNewEntries(MusicSettings settings, IReadOnlyList<SingleNoteClip> scanned)
        {
            var serialized = new SerializedObject(settings);
            var entries = serialized.FindProperty("singleNoteClips");
            var known = new HashSet<Object>(Enumerable.Range(0, entries.arraySize).Select(index => entries.GetArrayElementAtIndex(index).FindPropertyRelative("clip").objectReferenceValue));
            foreach (var single in scanned.Where(single => !known.Contains(single.Clip)))
            {
                entries.arraySize++;
                var entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                entry.FindPropertyRelative("clip").objectReferenceValue = single.Clip;
                entry.FindPropertyRelative("midiPitch").intValue = single.MidiPitch;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }
    }
}
