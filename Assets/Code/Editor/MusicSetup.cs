using System.Linq;
using DetectiveDave.MusicPlay;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    static class MusicSetup
    {
        const string DialogTitle = "Set Up Music";
        const string SavedSceneFirst = "{0} is open with unsaved changes and the music is not set up in it yet. Save it (Ctrl+S) and try again.";

        [MenuItem("Window/Detective Dave/Set Up Music")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the music. Nothing was changed.");
                return;
            }
            AssetDatabase.Refresh();
            var error = ApplyOrError(out var singles);
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(MusicSetupPaths.SettingsPath);
            EditorUtility.DisplayDialog(DialogTitle, "The jazz loop and the piano notes are on Music Settings, and the game and the overworld play them.\n\n" + singles.Text, "OK");
        }

        public static string ApplyOrError() => ApplyOrError(out _);

        public static string ApplyOrError(out SingleNoteReport singles)
        {
            singles = SingleNoteReport.Empty;
            var pianoPaths = PianoPaths();
            var missing = MissingAssets(pianoPaths);
            if (missing.Length > 0) return "Nothing was changed. Missing assets:\n" + string.Join("\n", missing);
            MusicLoopImport.MakeGapless(MusicSetupPaths.LoopPath);
            var settings = EnsureSettings(pianoPaths);
            var chartError = ChordChartSetup.ApplyOrError(settings, false);
            if (chartError != null) return chartError;
            singles = SingleNoteSetup.Apply(settings);
            var bassError = BassChartSetup.ApplyIfFileExists(settings);
            if (bassError != null) return bassError;
            AssetDatabase.SaveAssets();
            return WireScene(PlayerSetupPaths.ScenePath, settings) ?? WireScene(OverworldSetupPaths.ScenePath, settings);
        }

        static string[] PianoPaths() =>
            NaturalOrder.Sort(AssetDatabase.FindAssets("t:AudioClip", new[] { MusicSetupPaths.PianoFolder }).Select(AssetDatabase.GUIDToAssetPath).Where(path => !MusicSetupPaths.IsSingleNotePath(path)).ToArray());

        static string[] MissingAssets(string[] pianoPaths)
        {
            var missingLoop = AssetImporter.GetAtPath(MusicSetupPaths.LoopPath) ? new string[0] : new[] { MusicSetupPaths.LoopPath };
            var missingPiano = pianoPaths.Length > 0 ? new string[0] : new[] { MusicSetupPaths.PianoFolder + "/ (no audio files)" };
            return missingLoop.Concat(missingPiano).ToArray();
        }

        static MusicSettings EnsureSettings(string[] pianoPaths)
        {
            var settings = AssetDatabase.LoadAssetAtPath<MusicSettings>(MusicSetupPaths.SettingsPath);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<MusicSettings>();
                AssetDatabase.CreateAsset(settings, MusicSetupPaths.SettingsPath);
            }
            var serialized = new SerializedObject(settings);
            var loop = serialized.FindProperty("loopClip");
            if (!loop.objectReferenceValue) loop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicSetupPaths.LoopPath);
            FillPianoIfEmpty(serialized.FindProperty("pianoClips"), pianoPaths);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }

        static void FillPianoIfEmpty(SerializedProperty clips, string[] pianoPaths)
        {
            if (Enumerable.Range(0, clips.arraySize).Any(index => clips.GetArrayElementAtIndex(index).objectReferenceValue)) return;
            clips.arraySize = pianoPaths.Length;
            for (var index = 0; index < pianoPaths.Length; index++)
                clips.GetArrayElementAtIndex(index).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(pianoPaths[index]);
        }

        static string WireScene(string scenePath, MusicSettings settings)
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)) return null;
            var scene = EditorSceneManager.GetSceneByPath(scenePath);
            var wasLoaded = scene.isLoaded;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                return Wire(scene, scenePath, settings);
            }
            catch (System.Exception exception)
            {
                return $"Setting up the music in {scenePath} failed: {exception.Message}";
            }
            finally
            {
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        static string Wire(Scene scene, string scenePath, MusicSettings settings)
        {
            var host = FindHost(scene);
            if (!host) return null;
            var link = host.GetComponent<MusicLink>();
            if (link && IsLinked(link, settings)) return null;
            if (scene.isDirty) return string.Format(SavedSceneFirst, scenePath);
            if (!link) link = host.AddComponent<MusicLink>();
            var serialized = new SerializedObject(link);
            serialized.FindProperty("settings").objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
            return null;
        }

        static GameObject FindHost(Scene scene) =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<IMoveAttemptSource>(true)).OfType<Component>().Select(component => component.gameObject).FirstOrDefault();

        static bool IsLinked(MusicLink link, MusicSettings settings) => new SerializedObject(link).FindProperty("settings").objectReferenceValue == settings;
    }
}
