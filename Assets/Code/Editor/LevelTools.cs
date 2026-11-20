using System.Collections.Generic;
using System.Linq;
using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    /// <summary>The operations behind the Levels window and Play This Level, one public Unity API each. No scene is ever edited.</summary>
    public static class LevelTools
    {
        const string TemplatesFolderName = "Templates";

        /// <summary>The level open in Prefab Mode (the stage's editable root), or null.</summary>
        public static Level OpenLevel
        {
            get
            {
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                return stage != null && stage.prefabContentsRoot ? stage.prefabContentsRoot.GetComponent<Level>() : null;
            }
        }

        /// <summary>The asset path of a level prefab, or of the prefab an open stage edits. Empty for anything else.</summary>
        public static string AssetPath(Level level)
        {
            if (!level) return string.Empty;
            var stage = PrefabStageUtility.GetPrefabStage(level.gameObject);
            if (stage != null) return stage.assetPath;
            return PrefabUtility.IsPartOfPrefabAsset(level) ? AssetDatabase.GetAssetPath(level) : string.Empty;
        }

        /// <summary>The prefab asset of a level (the level itself when it already is one, the edited prefab for a stage root).</summary>
        public static Level Asset(Level level)
        {
            var path = AssetPath(level);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Level>(path);
        }

        /// <summary>What Play This Level plays: the open level, else a selected level prefab, else the first of the sequence.</summary>
        public static Level LevelToPlay()
        {
            if (OpenLevel) return OpenLevel;
            if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent(out Level selected) && PrefabUtility.IsPartOfPrefabAsset(selected)) return selected;
            var sequence = FindSequence();
            return sequence ? sequence.First : null;
        }

        /// <summary>Saves the level open in Prefab Mode when it has unsaved changes (public API: save the stage contents, then clear its dirty flag).</summary>
        public static void SaveOpenLevel()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null || !stage.scene.isDirty || !stage.prefabContentsRoot.GetComponent<Level>()) return;
            PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.assetPath, out bool saved);
            if (saved) stage.ClearDirtiness();
            else Debug.LogError($"Could not save '{stage.assetPath}'.");
        }

        /// <summary>The game's level sequence: the one Level Sequence asset in the project. Null (with a warning for several) otherwise.</summary>
        public static LevelSequence FindSequence()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(LevelSequence));
            if (guids.Length > 1) Debug.LogWarning($"{guids.Length} Level Sequence assets found; the Levels window uses {AssetDatabase.GUIDToAssetPath(guids[0])}.");
            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<LevelSequence>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>Every level prefab in the levels folder (templates excluded), by path.</summary>
        public static List<Level> FindLevels()
        {
            var folder = LevelSettings.instance.LevelsFolder;
            if (!AssetDatabase.IsValidFolder(folder)) return new List<Level>();
            return AssetDatabase.FindAssets("t:Prefab", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !path.Contains($"/{TemplatesFolderName}/"))
                .OrderBy(path => path)
                .Select(path => AssetDatabase.LoadAssetAtPath<Level>(path))
                .Where(level => level)
                .ToList();
        }

        /// <summary>
        /// New Level: copies the template to "Level NN.prefab" in the levels folder, gives it the default Key Art, appends it to the sequence,
        /// opens it in Prefab Mode and gets the Tile Palette ready. Returns the new prefab, or null.
        /// </summary>
        public static Level CreateLevel(LevelSequence sequence)
        {
            var settings = LevelSettings.instance;
            var template = settings.LevelTemplate;
            if (!template)
            {
                Debug.LogError("No level template set (Project Settings > Detective Dave > Levels > Level Template).");
                return null;
            }
            EnsureFolder(settings.LevelsFolder);
            int number = FindLevels().Count + 1;
            var path = AssetDatabase.GenerateUniqueAssetPath($"{settings.LevelsFolder}/Level {number:00}.prefab");
            if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(template), path))
            {
                Debug.LogError($"Could not copy the level template to '{path}'.");
                return null;
            }
            var level = AssetDatabase.LoadAssetAtPath<Level>(path);
            if (settings.DefaultKeyArt && level.KeyArt != settings.DefaultKeyArt) EditAsset(level, edited => SetKeyArtField(edited, settings.DefaultKeyArt));
            if (sequence) AddToSequence(sequence, level);
            Open(level);
            return level;
        }

        /// <summary>Appends a level prefab to the sequence (undoable) and saves the sequence.</summary>
        public static void AddToSequence(LevelSequence sequence, Level level)
        {
            if (!sequence || !level || sequence.IndexOf(level) >= 0) return;
            var serialized = new SerializedObject(sequence);
            var levels = serialized.FindProperty("levels");
            levels.arraySize++;
            levels.GetArrayElementAtIndex(levels.arraySize - 1).objectReferenceValue = level;
            serialized.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(sequence);
        }

        /// <summary>Opens a level prefab in Prefab Mode and gets the Tile Palette ready for painting it.</summary>
        public static void Open(Level level)
        {
            var asset = Asset(level) ?? level;
            if (!asset) return;
            AssetDatabase.OpenAsset(asset.gameObject);
            ReadyPalette();
        }

        /// <summary>
        /// Opens the Tile Palette with the level palette, the open level's Shape tilemap as the paint target and the Key brush.
        /// Uses the public <see cref="GridPaintingState"/>; logs when the window cannot be opened.
        /// </summary>
        public static void ReadyPalette()
        {
            if (!EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette"))
                Debug.LogWarning("Could not open Window > 2D > Tile Palette: open it by hand, pick the 'Level Tiles' palette and the Key brush.");
            ApplyPaletteState();
            EditorApplication.delayCall += ApplyPaletteState;     // again once the window has initialised itself
        }

        static void ApplyPaletteState()
        {
            var palette = LevelSettings.instance.Palette;
            if (!palette) Debug.LogWarning("No level tile palette set (Project Settings > Detective Dave > Levels > Palette).");
            else if (GridPaintingState.palettes.Contains(palette)) GridPaintingState.palette = palette;     // the setter throws for anything else
            else Debug.LogWarning($"'{palette.name}' is not a Tile Palette: pick the level palette in the Tile Palette window.", palette);
            var brush = GridPaintingState.brushes.OfType<KeyBrush>().FirstOrDefault();
            if (brush) GridPaintingState.gridBrush = brush;
            else Debug.LogWarning("The Key brush is not in the Tile Palette's brush list.");
            var level = OpenLevel;
            if (level && level.Shape) GridPaintingState.scenePaintTarget = level.Shape.gameObject;
        }

        /// <summary>Sets the background sprite of a level: in the open stage (Auto Save saves it), or in the prefab asset.</summary>
        public static void SetBackground(Level level, Sprite sprite) => Edit(level, edited =>
        {
            var renderer = edited.BackgroundRenderer;
            if (!renderer)
            {
                Debug.LogWarning($"Level '{edited.name}' has no Background renderer.", edited);
                return;
            }
            Undo.RecordObject(renderer, "Set Level Background");
            renderer.sprite = sprite;
            EditorUtility.SetDirty(renderer);
        });

        /// <summary>Sets the Key Art of a level: in the open stage, or in the prefab asset.</summary>
        public static void SetKeyArt(Level level, KeyArt art) => Edit(level, edited => SetKeyArtField(edited, art));

        /// <summary>Sets which Shift the player starts on: in the open stage, or in the prefab asset.</summary>
        public static void SetStartKey(Level level, StartSide side) => Edit(level, edited =>
        {
            var serialized = new SerializedObject(edited);
            serialized.FindProperty("startKey").enumValueIndex = (int)side;
            serialized.ApplyModifiedProperties();
        });

        static void SetKeyArtField(Level level, KeyArt art)
        {
            var serialized = new SerializedObject(level);
            serialized.FindProperty("keyArt").objectReferenceValue = art;
            serialized.ApplyModifiedProperties();
        }

        // Edits the open stage copy when the level is open (live, undoable, saved by Auto Save), else the asset.
        static void Edit(Level level, System.Action<Level> change)
        {
            var open = OpenLevel;
            if (open && AssetPath(open) == AssetPath(level))
            {
                change(open);
                return;
            }
            var asset = Asset(level);
            if (asset) EditAsset(asset, change);
        }

        static void EditAsset(Level asset, System.Action<Level> change)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                change(root.GetComponent<Level>());
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            LiveLevelReload.Schedule(path);
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
