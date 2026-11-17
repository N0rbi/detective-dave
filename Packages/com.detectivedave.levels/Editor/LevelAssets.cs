using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>Creating level assets and objects with sensible defaults, and the menu items that do it.</summary>
    internal static class LevelAssets
    {
        const string CreateMenu = "Assets/Create/Detective Dave/Level/";
        const string GameObjectMenu = "GameObject/Detective Dave/Level/";
        const float CellsPerKey = 4f;

        // The terrain kinds the game started with. Start and goal are floor the player stands on, so they are walkable.
        static readonly (string name, Color color, bool walkable, string description)[] StandardTypes =
        {
            ("Walkable", new Color(0.35f, 0.85f, 0.45f), true, "Floor: the player and enemies walk here."),
            ("Gap", new Color(0.6f, 0.4f, 0.95f), false, "A hole: nobody walks here."),
            ("Wall", new Color(0.9f, 0.35f, 0.3f), false, "Blocks the way."),
            ("WallGutter", new Color(0.95f, 0.65f, 0.25f), false, "A wall with a gutter the player can climb."),
            ("Start", new Color(0.25f, 0.5f, 0.85f), true, "Where the player starts."),
            ("Goal", new Color(1f, 0.85f, 0.2f), true, "Where the player has to get to."),
        };

        /// <summary>Every Tile Type asset in the project, by name.</summary>
        public static TileType[] FindAllTileTypes() =>
            AssetDatabase.FindAssets("t:" + nameof(TileType))
                .Select(guid => AssetDatabase.LoadAssetAtPath<TileType>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(type => type)
                .Distinct()
                .OrderBy(type => type.name)
                .ToArray();

        /// <summary>
        /// Creates a Level Definition at <paramref name="path"/>. It uses the project's usual style template (the level most other levels
        /// use as their template) when there is one; otherwise it gets an empty style, with the standard key shapes, for every Tile Type.
        /// </summary>
        public static LevelDefinition CreateLevel(string path)
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            AssetDatabase.CreateAsset(level, path);
            var serialized = new SerializedObject(level);
            var template = FindUsualTemplate();
            if (template && template != level)
                serialized.FindProperty("styleTemplate").objectReferenceValue = template;
            else
            {
                var styles = serialized.FindProperty("tileStyles");
                foreach (var type in FindAllTileTypes())
                    LevelDefinitionEditor.ResetStyle(LevelDefinitionEditor.AppendElement(styles), type);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(level);
            return level;
        }

        static LevelDefinition FindUsualTemplate() =>
            AssetDatabase.FindAssets("t:" + nameof(LevelDefinition))
                .Select(guid => AssetDatabase.LoadAssetAtPath<LevelDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(level => level && level.StyleTemplate)
                .GroupBy(level => level.StyleTemplate)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .FirstOrDefault();

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
                AssetDatabase.CreateAsset(type, path);
                var serialized = new SerializedObject(type);
                serialized.FindProperty("editorColor").colorValue = color;
                serialized.FindProperty("walkable").boolValue = walkable;
                serialized.FindProperty("description").stringValue = description;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                created++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log(created > 0
                ? $"Created {created} tile type(s) in {folder}."
                : $"{folder} already has every standard tile type.");
        }

        [MenuItem(GameObjectMenu + "Tile Map Group", false, 10)]
        static void CreateTileMapGroup(MenuCommand command)
        {
            var root = new GameObject("Level Shape", typeof(Grid), typeof(TileMapGroup));
            root.GetComponent<Grid>().cellSize = new Vector3(1f / CellsPerKey, 1f / CellsPerKey, 0f);
            var tilemap = new GameObject("Terrain", typeof(Tilemap), typeof(TilemapRenderer));
            tilemap.transform.SetParent(root.transform, false);
            GameObjectUtility.SetParentAndAlign(root, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(root, "Create Tile Map Group");
            Selection.activeObject = root;
        }

        static string SelectedFolder()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) return "Assets";
            return AssetDatabase.IsValidFolder(path) ? path : System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
        }
    }
}
