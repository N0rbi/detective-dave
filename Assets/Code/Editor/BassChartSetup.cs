using System.IO;
using DetectiveDave.MusicPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class BassChartSetup
    {
        const string DialogTitle = "Set Up Bass Chart";
        const string HowToMakeTheFile = "Drop the upright bass export (.aif or .wav, starting at the start of the loop) into Assets/Staging, move it to Assets/Audio/Music/Bass, and run in a terminal:\npython3 tools/bass_onsets.py \"Assets/Audio/Music/Bass/<file>\" Assets/Audio/Music/Bass/bass_onsets.json";

        [MenuItem("Window/Detective Dave/Set Up Bass Chart")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the bass chart. Nothing was changed.");
                return;
            }
            AssetDatabase.Refresh();
            var settings = AssetDatabase.LoadAssetAtPath<MusicSettings>(MusicSetupPaths.SettingsPath);
            if (!settings)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Music Settings does not exist yet. Run Window > Detective Dave > Set Up Music first.");
                return;
            }
            if (!File.Exists(MusicSetupPaths.BassOnsetsPath))
            {
                PlayerSetup.ShowProblem(DialogTitle, $"Nothing was changed. {MusicSetupPaths.BassOnsetsPath} is missing.\n\n{HowToMakeTheFile}");
                return;
            }
            var error = Apply(settings, ChooseTempo);
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error.Text);
                return;
            }
            AssetDatabase.SaveAssets();
            var chart = AssetDatabase.LoadAssetAtPath<BassChart>(MusicSetupPaths.BassChartPath);
            Selection.activeObject = chart;
            EditorUtility.DisplayDialog(DialogTitle, $"The Bass Chart has {chart.OnsetBeats.Count} bass notes and is linked on Music Settings. {chart.TempoNote}", "OK");
        }

        public static string ApplyIfFileExists(MusicSettings settings)
        {
            if (!File.Exists(MusicSetupPaths.BassOnsetsPath)) return null;
            var failure = Apply(settings, (_, loopTempo) => loopTempo);
            return failure?.Text;
        }

        sealed class Failure
        {
            public readonly string Text;

            public Failure(string text) => Text = text;
        }

        delegate TempoChoice Chooser(TempoChoice audioTempo, TempoChoice loopTempo);

        sealed class TempoChoice
        {
            public readonly double SecondsPerBeat;
            public readonly string Note;

            public TempoChoice(double secondsPerBeat, string note)
            {
                SecondsPerBeat = secondsPerBeat;
                Note = note;
            }
        }

        static Failure Apply(MusicSettings settings, Chooser choose)
        {
            var chordChart = settings.ChordChart;
            if (!chordChart) return new Failure("Nothing was changed. Music Settings has no Chord Chart. Run Window > Detective Dave > Set Up Chord Chart first.");
            if (!settings.LoopClip) return new Failure("Nothing was changed. Music Settings has no Loop Clip.");
            var file = ReadFile(out var failure);
            if (file == null) return failure;
            var chart = EnsureChart();
            var loopSeconds = settings.LoopClip.length;
            var loopTempo = new TempoChoice(loopSeconds / chordChart.LoopLengthBeats, $"Seconds became beats at the loop's own tempo ({TempoBpm(loopSeconds / chordChart.LoopLengthBeats):0.0} BPM, {chordChart.LoopLengthBeats} beats in {loopSeconds:0.00} s).");
            var audioSecondsPerBeat = BassOnsetBeats.SecondsPerBeatForDuration(file.durationSeconds, chordChart.LoopLengthBeats);
            var audioTempo = new TempoChoice(audioSecondsPerBeat, $"Seconds became beats at the bass file's tempo ({TempoBpm(audioSecondsPerBeat):0.0} BPM, the whole file is the {chordChart.LoopLengthBeats} beat loop).");
            var chosen = BassOnsetBeats.DurationFitsLoop(file.durationSeconds, loopSeconds, loopTempo.SecondsPerBeat) ? loopTempo : Mismatch(file, loopSeconds, chordChart, choose, audioTempo, loopTempo);
            var serialized = new SerializedObject(chart);
            var offset = serialized.FindProperty("bassStartOffsetBeats").floatValue;
            var quantize = serialized.FindProperty("quantizeBeats").floatValue;
            var beats = BassOnsetBeats.ToLoopBeats(file.onsetsSeconds, chosen.SecondsPerBeat, offset, chordChart.LoopLengthBeats, quantize);
            chart.Replace(beats, file.sourceFile, file.durationSeconds, chosen.Note + " Assumes the bass file starts at the start of the loop; Bass Start Offset Beats corrects it.");
            EditorUtility.SetDirty(chart);
            Link(settings, chart);
            return beats.Length == 0 ? new Failure("The bass file has no notes, so the Bass Chart is empty.") : null;
        }

        static TempoChoice Mismatch(BassOnsetFile file, float loopSeconds, ChordChart chordChart, Chooser choose, TempoChoice audioTempo, TempoChoice loopTempo)
        {
            Debug.LogWarning($"[Music] The bass file is {file.durationSeconds:0.00} s but the loop is {loopSeconds:0.00} s ({chordChart.TempoBpm:0.#} BPM, {chordChart.LoopLengthBars} bars). The bass file does not fit the loop.");
            return choose(audioTempo, loopTempo);
        }

        static TempoChoice ChooseTempo(TempoChoice audioTempo, TempoChoice loopTempo)
        {
            var useAudio = EditorUtility.DisplayDialog(DialogTitle,
                $"The bass file does not have the length of the loop.\n\nLoop tempo: {TempoBpm(loopTempo.SecondsPerBeat):0.0} BPM.\nBass file tempo (file length = loop length): {TempoBpm(audioTempo.SecondsPerBeat):0.0} BPM.\n\nWhich tempo places the bass notes?",
                "Use the audio tempo", "Keep the loop tempo");
            return useAudio ? audioTempo : loopTempo;
        }

        static double TempoBpm(double secondsPerBeat) => 60.0 / secondsPerBeat;

        static BassOnsetFile ReadFile(out Failure failure)
        {
            failure = null;
            try
            {
                return BassOnsetFile.Parse(File.ReadAllText(MusicSetupPaths.BassOnsetsPath));
            }
            catch (System.ArgumentException exception)
            {
                failure = new Failure($"Nothing was changed. {MusicSetupPaths.BassOnsetsPath} cannot be read: {exception.Message}\n\n{HowToMakeTheFile}");
                return null;
            }
        }

        static BassChart EnsureChart()
        {
            var chart = AssetDatabase.LoadAssetAtPath<BassChart>(MusicSetupPaths.BassChartPath);
            if (chart) return chart;
            chart = ScriptableObject.CreateInstance<BassChart>();
            AssetDatabase.CreateAsset(chart, MusicSetupPaths.BassChartPath);
            return chart;
        }

        static void Link(MusicSettings settings, BassChart chart)
        {
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("bassChart").objectReferenceValue = chart;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
