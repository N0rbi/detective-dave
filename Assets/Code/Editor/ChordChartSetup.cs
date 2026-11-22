using System.Collections.Generic;
using System.IO;
using System.Linq;
using DetectiveDave.MusicPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class ChordChartSetup
    {
        const string DialogTitle = "Set Up Chord Chart";

        [MenuItem("Window/Detective Dave/Set Up Chord Chart")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the chord chart. Nothing was changed.");
                return;
            }
            AssetDatabase.Refresh();
            var settings = AssetDatabase.LoadAssetAtPath<MusicSettings>(MusicSetupPaths.SettingsPath);
            if (!settings)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Music Settings does not exist yet. Run Window > Detective Dave > Set Up Music first.");
                return;
            }
            var error = ApplyOrError(settings, AskToReplaceExisting());
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(MusicSetupPaths.ChartPath);
            EditorUtility.DisplayDialog(DialogTitle, "The Chord Chart is filled from chord_chart.json and linked on Music Settings.", "OK");
        }

        public static string ApplyOrError(MusicSettings settings, bool replaceExisting)
        {
            var file = ReadFile(out var error);
            if (file == null) return error;
            var chart = EnsureChart();
            error = FillIfAllowed(chart, file, replaceExisting);
            if (error != null) return error;
            LinkToSettings(settings, chart);
            AssetDatabase.SaveAssets();
            return null;
        }

        static bool AskToReplaceExisting()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ChordChart>(MusicSetupPaths.ChartPath);
            return existing && existing.HasData
                && EditorUtility.DisplayDialog(DialogTitle, "The Chord Chart already has chords and clip notes. Replace them with the ones in chord_chart.json? Your edits on the asset are lost.", "Replace", "Keep");
        }

        static ChordChartFile ReadFile(out string error)
        {
            error = null;
            if (!File.Exists(MusicSetupPaths.ChartFilePath))
            {
                error = $"Nothing was changed. {MusicSetupPaths.ChartFilePath} is missing.";
                return null;
            }
            try
            {
                return ChordChartFile.Parse(File.ReadAllText(MusicSetupPaths.ChartFilePath));
            }
            catch (System.ArgumentException exception)
            {
                error = $"Nothing was changed. {MusicSetupPaths.ChartFilePath} cannot be read: {exception.Message}";
                return null;
            }
        }

        static ChordChart EnsureChart()
        {
            var chart = AssetDatabase.LoadAssetAtPath<ChordChart>(MusicSetupPaths.ChartPath);
            if (chart) return chart;
            chart = ScriptableObject.CreateInstance<ChordChart>();
            AssetDatabase.CreateAsset(chart, MusicSetupPaths.ChartPath);
            return chart;
        }

        static string FillIfAllowed(ChordChart chart, ChordChartFile file, bool replaceExisting)
        {
            if (chart.HasData && !replaceExisting) return null;
            var clips = PianoClipsByName();
            var missing = file.clips.Select(entry => entry.clipName).Where(name => !clips.ContainsKey(name)).ToArray();
            if (missing.Length > 0) return $"Nothing was changed. These clips from chord_chart.json are not in {MusicSetupPaths.PianoFolder}:\n" + string.Join("\n", missing);
            var pitches = file.clips.Select(entry => new PianoClipPitch(clips[entry.clipName], entry.pitchClasses, entry.label));
            chart.Replace(file.tempoBpm, file.beatsPerBar, file.loopLengthBars, file.chartBeatAtLoopStart, file.segments, pitches);
            EditorUtility.SetDirty(chart);
            return null;
        }

        static Dictionary<string, AudioClip> PianoClipsByName() =>
            AssetDatabase.FindAssets("t:AudioClip", new[] { MusicSetupPaths.PianoFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
                .GroupBy(clip => clip.name)
                .ToDictionary(group => group.Key, group => group.First());

        static void LinkToSettings(MusicSettings settings, ChordChart chart)
        {
            var serialized = new SerializedObject(settings);
            var link = serialized.FindProperty("chordChart");
            if (!link.objectReferenceValue)
            {
                link.objectReferenceValue = chart;
                serialized.FindProperty("pianoOrder").enumValueIndex = (int)PianoOrder.ChordAware;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
