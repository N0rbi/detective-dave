using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Tests
{
    /// <summary>Builds throwaway kinds of key, key art and level hierarchies for the tests, and destroys them all in <see cref="Dispose"/>.</summary>
    internal sealed class LevelTestUtility : System.IDisposable
    {
        public const float CellSize = 0.25f;

        readonly List<Object> created = new List<Object>();

        public TileType Type(string typeName, bool walkable = true)
        {
            var type = Track(ScriptableObject.CreateInstance<TileType>());
            type.name = typeName;
            Set(type, "walkable", walkable);
            return type;
        }

        public Tile PlainTile() => Track(ScriptableObject.CreateInstance<Tile>());

        public Sprite Sprite(string spriteName)
        {
            var texture = Track(new Texture2D(4, 4));
            var sprite = Track(UnityEngine.Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f));
            sprite.name = spriteName;
            return sprite;
        }

        public KeyArt KeyArt(string artName = "Key Art")
        {
            var art = Track(ScriptableObject.CreateInstance<KeyArt>());
            art.name = artName;
            return art;
        }

        /// <summary>Adds a style to <paramref name="art"/>: one sprite per (size, wholeKey) entry.</summary>
        public void AddStyle(KeyArt art, TileType type, params (Vector2 size, bool wholeKey, Sprite sprite)[] shapes)
        {
            var serialized = new SerializedObject(art);
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

        public void SetDefaultBackground(KeyArt art, Sprite background) => Set(art, "background", background);

        /// <summary>
        /// A board object holding a level laid out like a level prefab: Level root, Grid child (cell 0.25) at <paramref name="gridPosition"/>,
        /// Shape tilemap under it, Background renderer. <paramref name="keyArt"/> may be null.
        /// </summary>
        public Level Level(string levelName = "Level 01", KeyArt keyArt = null, Vector3 gridPosition = default, bool underBoard = true)
        {
            var root = new GameObject(levelName, typeof(Level));
            if (underBoard) root.transform.SetParent(Track(new GameObject("Board")).transform, false);
            else Track(root);
            var grid = new GameObject("Grid", typeof(Grid));
            grid.GetComponent<Grid>().cellSize = new Vector3(CellSize, CellSize, 0f);
            grid.transform.SetParent(root.transform, false);
            grid.transform.localPosition = gridPosition;
            var shape = new GameObject("Shape", typeof(Tilemap));
            shape.transform.SetParent(grid.transform, false);
            var background = new GameObject("Background", typeof(SpriteRenderer));
            background.transform.SetParent(root.transform, false);
            var level = root.GetComponent<Level>();
            Set(level, "keyArt", keyArt);
            Set(level, "shape", shape.GetComponent<Tilemap>());
            Set(level, "background", background.GetComponent<SpriteRenderer>());
            return level;
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

        public static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Set(Object target, string property, bool value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
