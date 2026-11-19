using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardInput;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// The keyboard layouts the game ships (the JSON files in <see cref="LevelSettings.LayoutsFolder"/>), parsed once and cached
    /// until the project changes. Gizmos, the Key brush and the checks run often: they must not re-parse JSON.
    /// </summary>
    public static class KeyboardLayouts
    {
        /// <summary>The preview layout until the user picks another: UK (ISO).</summary>
        public const string DefaultPreviewId = "uk";

        static List<KeyboardLayout> cache;

        static KeyboardLayouts() => EditorApplication.projectChanged += Clear;

        /// <summary>Every layout, in picker order (by digit).</summary>
        public static IReadOnlyList<KeyboardLayout> All => cache ??= Load();

        /// <summary>The layout with this id, or null.</summary>
        public static KeyboardLayout Find(string id)
        {
            foreach (var layout in All)
                if (layout.Id == id) return layout;
            return null;
        }

        /// <summary>The layout picked in the Levels window (per computer), else UK, else the first one. Null when there are no layouts.</summary>
        public static KeyboardLayout Preview => Find(LevelPreferences.PreviewLayoutId) ?? Find(DefaultPreviewId) ?? All.FirstOrDefault();

        /// <summary>Forgets the parsed layouts; the next call reads the files again.</summary>
        public static void Clear() => cache = null;

        static List<KeyboardLayout> Load()
        {
            var layouts = new List<KeyboardLayout>();
            var folder = LevelSettings.instance.LayoutsFolder;
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"Keyboard layout folder '{folder}' does not exist (Project Settings > Detective Dave > Levels).");
                return layouts;
            }
            foreach (var guid in AssetDatabase.FindAssets("t:TextAsset", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".json")) continue;
                try { layouts.Add(KeyboardLayout.Parse(AssetDatabase.LoadAssetAtPath<TextAsset>(path))); }
                catch (System.ArgumentException exception) { Debug.LogWarning($"Not a keyboard layout: {path} ({exception.Message})"); }
            }
            layouts.Sort((a, b) => a.Digit.CompareTo(b.Digit));
            return layouts;
        }
    }
}
