using System.Linq;
using DetectiveDave.Levels;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    public static class OverworldSetup
    {
        const string DialogTitle = "Set Up Overworld";

        [MenuItem("Window/Detective Dave/Set Up Overworld")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the overworld. Nothing was changed.");
                return;
            }
            var error = ApplyOrError();
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(OverworldSetupPaths.SampleDeskPath);
            EditorUtility.DisplayDialog(DialogTitle, Summary(), "OK");
        }

        static readonly System.Collections.Generic.List<string> changes = new System.Collections.Generic.List<string>();

        static string Summary() =>
            changes.Count == 0
                ? "Nothing needed changing. Everything was already set up.\nUse Window > Detective Dave > Play Overworld (Shift+F5)."
                : "Set up:\n- " + string.Join("\n- ", changes) + "\n\nUse Window > Detective Dave > Play Overworld (Shift+F5).";

        static void Note(string text) => changes.Add(text);

        public static string ApplyOrError()
        {
            changes.Clear();
            var playerError = PlayerSetup.ApplyOrError();
            if (playerError != null) return "The player cannot be set up:\n" + playerError;
            AssetDatabase.Refresh();
            var missing = OverworldSetupPaths.SpritePaths.Concat(OverworldSetupPaths.PlaceholderPaths).Concat(OverworldSetupPaths.FontPaths).Where(path => !AssetImporter.GetAtPath(path)).ToArray();
            if (missing.Length > 0) return "Nothing was changed. Missing assets:\n" + string.Join("\n", missing);
            var level = AssetDatabase.LoadAssetAtPath<Level>(OverworldSetupPaths.LevelPath);
            var sprites = OverworldSetupPaths.SpritePaths.Select(PlayerSpriteImport.ImportDecoration).ToArray();
            EnsureFolder(OverworldSetupPaths.OverworldFolder);
            EnsureFolder(OverworldSetupPaths.CasesFolder);
            var settings = EnsureSettings(sprites[1], sprites[2]);
            AssignPlaceholders(settings);
            var magnifier = EnsureMagnifierSettings(sprites[0]);
            var sampleCase = EnsureSampleCase(level);
            var stamped = CaseIds.StampAll();
            if (stamped > 0) Note("locked the id of " + stamped + " cases");
            var desk = EnsureSampleDesk(sampleCase, sprites[1]);
            AssignDeskIfEmpty(settings, desk);
            AssetDatabase.SaveAssets();
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerSetupPaths.PrefabPath).GetComponent<PlayerView>();
            return OverworldSceneBuilder.WireOrError(playerPrefab, magnifier, settings);
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'), System.IO.Path.GetFileName(path));
        }

        static OverworldSettings EnsureSettings(Sprite folder, Sprite solved)
        {
            var settings = AssetDatabase.LoadAssetAtPath<OverworldSettings>(OverworldSetupPaths.SettingsPath);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<OverworldSettings>();
                AssetDatabase.CreateAsset(settings, OverworldSetupPaths.SettingsPath);
                Note("created Overworld Settings");
            }
            var serialized = new SerializedObject(settings);
            FillIfEmpty(serialized, "folderSprite", folder);
            FillIfEmpty(serialized, "solvedSprite", solved);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }

        static void FillIfEmpty(SerializedObject serialized, string field, Object value)
        {
            var property = serialized.FindProperty(field);
            if (property.objectReferenceValue) return;
            property.objectReferenceValue = value;
            Note("assigned " + field);
        }

        static void AssignDeskIfEmpty(OverworldSettings settings, DeskLayout desk)
        {
            var serialized = new SerializedObject(settings);
            FillIfEmpty(serialized, "desk", desk);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssignPlaceholders(OverworldSettings settings)
        {
            var serialized = new SerializedObject(settings);
            FillIfEmpty(serialized, "deskTopSprite", PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.DeskTopPath));
            FillIfEmpty(serialized, "descriptionPaperSprite", PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.DescriptionPaperPath));
            FillIfEmpty(serialized, "letterPaperSprite", PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.LetterPaperPath));
            FillIfEmpty(serialized, "ringSprite", PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.RingPath));
            FillIfEmpty(serialized, "shiftKeycapSprite", PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.KeycapPath));
            FillIfEmpty(serialized, "requestFont", AssetDatabase.LoadAssetAtPath<Font>(OverworldSetupPaths.RequestFontPath));
            FillIfEmpty(serialized, "placeholderFont", AssetDatabase.LoadAssetAtPath<Font>(OverworldSetupPaths.PlaceholderFontPath));
            FillIfEmpty(serialized, "typedFont", AssetDatabase.LoadAssetAtPath<Font>(OverworldSetupPaths.TypedFontPath));
            FillIfEmpty(serialized, "findingsFont", AssetDatabase.LoadAssetAtPath<Font>(OverworldSetupPaths.FindingsFontPath));
            FillIfEmpty(serialized, "noticeFont", AssetDatabase.LoadAssetAtPath<Font>(OverworldSetupPaths.NoticeFontPath));
            FillIfEmpty(serialized, "backspaceClip", AssetDatabase.LoadAssetAtPath<AudioClip>(OverworldSetupPaths.BackspacePath));
            var clicks = serialized.FindProperty("typewriterClicks");
            if (clicks.arraySize == 0)
            {
                clicks.arraySize = OverworldSetupPaths.ClickPaths.Length;
                for (var index = 0; index < clicks.arraySize; index++)
                    clicks.GetArrayElementAtIndex(index).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(OverworldSetupPaths.ClickPaths[index]);
                Note("assigned typewriterClicks");
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static PlayerMovementSettings EnsureMagnifierSettings(Sprite glass)
        {
            var magnifier = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(OverworldSetupPaths.MagnifierSettingsPath);
            if (!magnifier)
            {
                var player = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(PlayerSetupPaths.SettingsPath);
                magnifier = Object.Instantiate(player);
                magnifier.name = "Magnifier Movement Settings.config";
                AssetDatabase.CreateAsset(magnifier, OverworldSetupPaths.MagnifierSettingsPath);
                Note("created Magnifier Movement Settings");
                var created = new SerializedObject(magnifier);
                created.FindProperty("forgivingMoves").boolValue = true;
                created.FindProperty("flipsWithFacing").boolValue = false;
                created.FindProperty("dashGustEnabled").boolValue = false;
                created.FindProperty("heightInKeys").floatValue = 1.6f;
                created.ApplyModifiedPropertiesWithoutUndo();
            }
            var serialized = new SerializedObject(magnifier);
            SetFrames(serialized.FindProperty("walkFrames"), glass);
            SetFrames(serialized.FindProperty("climbFrames"), glass);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return magnifier;
        }

        static void SetFrames(SerializedProperty property, Sprite sprite)
        {
            property.arraySize = 1;
            property.GetArrayElementAtIndex(0).objectReferenceValue = sprite;
        }

        static CaseFile EnsureSampleCase(Level level)
        {
            var file = AssetDatabase.LoadAssetAtPath<CaseFile>(OverworldSetupPaths.SampleCasePath);
            if (file) return file;
            file = ScriptableObject.CreateInstance<CaseFile>();
            AssetDatabase.CreateAsset(file, OverworldSetupPaths.SampleCasePath);
            var serialized = new SerializedObject(file);
            serialized.FindProperty("caseId").stringValue = "sample-case";
            serialized.FindProperty("clientName").stringValue = "Mrs. Pennywhistle";
            var suspects = serialized.FindProperty("suspects");
            suspects.arraySize = 1;
            suspects.GetArrayElementAtIndex(0).FindPropertyRelative("key").stringValue = "husband";
            suspects.GetArrayElementAtIndex(0).FindPropertyRelative("value").stringValue = "Gerald Pennywhistle";
            serialized.FindProperty("description").stringValue = "{client} writes that her husband, {husband}, comes home late every night and smells of cheap cigars. She wants to know where he goes.";
            serialized.FindProperty("acceptMessage").stringValue = "I am sorry to hear that you have doubts in your husband. I will look into this.";
            serialized.FindProperty("declineMessage").stringValue = "I cannot take on any new cases in my current capacity.";
            serialized.FindProperty("findings").stringValue = "* 19:30 - {husband} entered the casino\n* 20:00 - {husband} lost all his money and his car\n* 20:10 - {husband} was pushed down from the roof of the casino in a staged suicide";
            serialized.FindProperty("response").stringValue = "{husband} was at the casino all along. There is nothing to worry about, your husband was faithful.";
            serialized.FindProperty("level").objectReferenceValue = level;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return file;
        }

        static DeskLayout EnsureSampleDesk(CaseFile sampleCase, Sprite folder)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(OverworldSetupPaths.SampleDeskPath);
            if (!existing) existing = OverworldDeskBuilder.SaveDesk(OverworldSetupPaths.SampleDeskPath, sampleCase, folder, OverworldDeskBuilder.CentredScale(folder));
            return existing.GetComponent<DeskLayout>();
        }
    }
}
