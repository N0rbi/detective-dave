using System.IO;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>The Level Tiles palette: the Tile Palette that holds every <see cref="TileType"/>, so designers find all kinds of key in one place.</summary>
    public static class LevelPalette
    {
        /// <summary>Where the palette lives.</summary>
        public const string DefaultPath = "Assets/Tiles/Level Tiles.prefab";

        static readonly Vector3 PaletteCellSize = new Vector3(1f, 1f, 0f);

        /// <summary>The palette prefab at <paramref name="path"/>, or null when there is none.</summary>
        public static GameObject Find(string path = DefaultPath) => AssetDatabase.LoadAssetAtPath<GameObject>(path);

        /// <summary>True when the palette has <paramref name="tile"/> painted somewhere.</summary>
        public static bool Contains(GameObject palette, TileBase tile)
        {
            var tilemap = palette ? palette.GetComponentInChildren<Tilemap>(true) : null;
            return tilemap && tile && tilemap.ContainsTile(tile);
        }

        /// <summary>
        /// Paints <paramref name="tile"/> into the palette, to the right of what is there, and saves the palette. Does nothing when the
        /// palette already has it. The palette asset's tilemap is edited in place: no prefab round-trip, so its palette settings stay untouched.
        /// </summary>
        public static void Add(GameObject palette, TileBase tile)
        {
            var tilemap = palette ? palette.GetComponentInChildren<Tilemap>(true) : null;
            if (!tilemap || !tile || tilemap.ContainsTile(tile)) return;
            tilemap.CompressBounds();
            var bounds = tilemap.cellBounds;
            var cell = tilemap.GetUsedTilesCount() == 0 ? Vector3Int.zero : new Vector3Int(bounds.xMax, bounds.yMin, 0);
            Undo.RecordObject(tilemap, "Add Tile To Level Tiles");
            tilemap.SetTile(cell, tile);
            EditorUtility.SetDirty(tilemap);
            AssetDatabase.SaveAssetIfDirty(palette);
        }

        /// <summary>Creates an empty palette at <paramref name="path"/> with Unity's own palette maker and returns it.</summary>
        public static GameObject Create(string path = DefaultPath)
        {
            var folder = Path.GetDirectoryName(path)?.Replace('\\', '/');
            LevelAssets.EnsureFolder(folder);
            return GridPaletteUtility.CreateNewPalette(folder, Path.GetFileNameWithoutExtension(path),
                GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Automatic, PaletteCellSize, GridLayout.CellSwizzle.XYZ);
        }
    }
}
