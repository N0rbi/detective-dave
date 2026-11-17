using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Tests
{
    /// <summary>Builds throwaway types, tiles, levels and tilemaps for the tests, and destroys them all in <see cref="Dispose"/>.</summary>
    internal sealed class LevelTestUtility : System.IDisposable
    {
        public const float CellSize = 0.25f;

        readonly List<Object> created = new List<Object>();

        public TileType Type(string typeName, bool walkable = true)
        {
            var type = Track(ScriptableObject.CreateInstance<TileType>());
            type.name = typeName;
            var serialized = new SerializedObject(type);
            serialized.FindProperty("walkable").boolValue = walkable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return type;
        }

        public TerrainTile Tile(TileType type)
        {
            var tile = Track(ScriptableObject.CreateInstance<TerrainTile>());
            var serialized = new SerializedObject(tile);
            serialized.FindProperty("type").objectReferenceValue = type;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return tile;
        }

        public Tile PlainTile() => Track(ScriptableObject.CreateInstance<Tile>());

        public Sprite Sprite(string spriteName)
        {
            var texture = Track(new Texture2D(4, 4));
            var sprite = Track(UnityEngine.Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f));
            sprite.name = spriteName;
            return sprite;
        }

        public LevelDefinition Level(string levelName = "Level")
        {
            var level = Track(ScriptableObject.CreateInstance<LevelDefinition>());
            level.name = levelName;
            return level;
        }

        /// <summary>Adds a style to <paramref name="level"/>: one sprite per (size, wholeKey) entry.</summary>
        public void AddStyle(LevelDefinition level, TileType type, params (Vector2 size, bool wholeKey, Sprite sprite)[] shapes)
        {
            var serialized = new SerializedObject(level);
            var styles = serialized.FindProperty("tileStyles");
            styles.arraySize++;
            var style = styles.GetArrayElementAtIndex(styles.arraySize - 1);
            style.FindPropertyRelative("type").objectReferenceValue = type;
            style.FindPropertyRelative("tint").colorValue = Color.white;
            var entries = style.FindPropertyRelative("shapes");
            entries.arraySize = shapes.Length;
            for (int i = 0; i < shapes.Length; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("size").vector2Value = shapes[i].size;
                entry.FindPropertyRelative("wholeKey").boolValue = shapes[i].wholeKey;
                entry.FindPropertyRelative("sprite").objectReferenceValue = shapes[i].sprite;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public void SetTemplate(LevelDefinition level, LevelDefinition template) => Set(level, "styleTemplate", template);

        public void SetBackground(LevelDefinition level, Sprite background) => Set(level, "background", background);

        /// <summary>A Grid (cell 0.25) with a TileMapGroup, under a board object at the origin, and one child Tilemap per name.</summary>
        public TileMapGroup Group(LevelDefinition level, params string[] tilemapNames)
        {
            var board = Track(new GameObject("Board"));
            var root = new GameObject("Grid", typeof(Grid), typeof(TileMapGroup));
            root.transform.SetParent(board.transform, false);
            root.GetComponent<Grid>().cellSize = new Vector3(CellSize, CellSize, 0f);
            foreach (var tilemapName in tilemapNames)
            {
                var child = new GameObject(tilemapName, typeof(Tilemap));
                child.transform.SetParent(root.transform, false);
            }
            var group = root.GetComponent<TileMapGroup>();
            Set(group, "level", level);
            return group;
        }

        public Tilemap Tilemap(TileMapGroup group, string tilemapName) => group.transform.Find(tilemapName).GetComponent<Tilemap>();

        /// <summary>Lists the group's layers explicitly: (tilemap name, layer type or null).</summary>
        public void SetLayers(TileMapGroup group, params (string tilemap, TileType type)[] layers)
        {
            var serialized = new SerializedObject(group);
            var array = serialized.FindProperty("layers");
            array.arraySize = layers.Length;
            for (int i = 0; i < layers.Length; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("tilemap").objectReferenceValue = Tilemap(group, layers[i].tilemap);
                element.FindPropertyRelative("type").objectReferenceValue = layers[i].type;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Paint(Tilemap tilemap, TileBase tile, RectInt area)
        {
            foreach (var cell in area.allPositionsWithin)
                tilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), tile);
        }

        public void Dispose()
        {
            for (int i = created.Count - 1; i >= 0; i--)
                if (created[i]) Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        T Track<T>(T item) where T : Object
        {
            created.Add(item);
            return item;
        }

        static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
