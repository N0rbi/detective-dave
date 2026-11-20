using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>Finding and creating level assets, and the Create menu items.</summary>
    public static class LevelAssets
    {
        /// <summary>The name the default Key Art goes by.</summary>
        public const string DefaultKeyArtName = "Default Key Art";

        const string CreateMenu = "Assets/Create/Detective Dave/";

        static readonly (string name, Color color, bool walkable, string description)[] StandardTypes =
        {
            ("Walkable", new Color(0.35f, 0.85f, 0.45f), true, "Floor: the player and enemies walk here."),
            ("Gap", new Color(0.6f, 0.4f, 0.95f), false, "A hole: nobody walks here."),
            ("Wall", new Color(0.9f, 0.35f, 0.3f), false, "Blocks the way."),
            ("WallGutter", new Color(0.95f, 0.65f, 0.25f), false, "A wall with a gutter the player can climb."),
        };

        /// <summary>Every Tile Type asset in the project, by name. Scans the project: cache the result.</summary>
        public static TileType[] FindAllTileTypes() => FindAll<TileType>().OrderBy(type => type.name).ToArray();

        /// <summary>The Key Art named <see cref="DefaultKeyArtName"/>, else the only Key Art in the project, else null.</summary>
        public static KeyArt FindDefaultKeyArt()
        {
            var all = FindAll<KeyArt>().ToList();
            return all.FirstOrDefault(art => art.name == DefaultKeyArtName) ?? (all.Count == 1 ? all[0] : null);
        }

        /// <summary>Creates the folder (and its parents) when it does not exist. <paramref name="folder"/> is a project path like "Assets/Levels".</summary>
        public static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder)) return;
            var parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }

        static System.Collections.Generic.IEnumerable<T> FindAll<T>() where T : Object =>
            AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset)
                .Distinct();

        [MenuItem(CreateMenu + "Standard Tile Types", false, 20)]
        static void CreateStandardTileTypes()
        {
            var folder = SelectedFolder();
            int created = 0;
            foreach (var (typeName, color, walkable, description) in StandardTypes)
            {
                var path = $"{folder}/{typeName}.asset";
                if (AssetDatabase.LoadAssetAtPath<Object>(path)) continue;
                var type = ScriptableObject.CreateInstance<TileType>();
                type.color = color;
                AssetDatabase.CreateAsset(type, path);
                var serialized = new SerializedObject(type);
                serialized.FindProperty("editorColor").colorValue = color;
                serialized.FindProperty("walkable").boolValue = walkable;
                serialized.FindProperty("description").stringValue = description;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                created++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log(created > 0 ? $"Created {created} tile type(s) in {folder}." : $"{folder} already has every standard tile type.");
        }

        static string SelectedFolder()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) return "Assets";
            return AssetDatabase.IsValidFolder(path) ? path : System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
        }
    }
}
