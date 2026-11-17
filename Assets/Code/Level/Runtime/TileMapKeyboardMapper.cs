using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// The level on the keyboard: what tile each key is, and paths between keys, read from the assigned level's baked shape.
    /// Put it on the keyboard object (next to <see cref="KeyboardGridView"/>) and assign a <see cref="LevelDefinition"/>:
    /// that is all the wiring a level needs. <see cref="KeyTerrainView"/> and <see cref="LevelBackgroundView"/> draw it.
    /// <para>
    /// Board space is this object's local space, which is where a <see cref="TileMapGroup"/> parented to this object bakes.
    /// Keys are looked up in the view; nothing here talks to the input package.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView))]
    public sealed class TileMapKeyboardMapper : MonoBehaviour
    {
        // A key center usually lies exactly on a tile border (a key is 4 x 4 tiles). Reading just above and right of it keeps every answer stable.
        static readonly Vector2 CenterNudge = new Vector2(0.01f, 0.01f);

        [Tooltip("The level to play. Its baked shape decides what every key is; its key art and background are drawn by the views on this object.")]
        [SerializeField] LevelDefinition level;

        KeyboardGridView view;

        /// <summary>Raised after <see cref="Load"/> (or an inspector change while playing) switches the level.</summary>
        public event Action LevelChanged;

        /// <summary>The level being played, or null.</summary>
        public LevelDefinition Level => level;

        void Awake() => view = GetComponent<KeyboardGridView>();

        /// <summary>Switches to another level (null for none) and tells the views to redraw.</summary>
        public void Load(LevelDefinition next)
        {
            level = next;
            LevelChanged?.Invoke();
        }

        /// <summary>The tile under the center of a key. Empty when nothing is painted there, no level is set, or the layout has no such key.</summary>
        public MapTile GetTile(Key key) => TryGetBoardPosition(key, out var board) ? TileAtBoard(board) : default;

        /// <summary>The tile at a world position. Empty when nothing is painted there or no level is set.</summary>
        public MapTile GetTile(Vector2 world) => TileAtBoard(transform.InverseTransformPoint(world));

        /// <summary>
        /// World positions from <paramref name="start"/> to <paramref name="goal"/>, both included, empty when there is no way.
        /// Walkable tiles only, unless <paramref name="passable"/> says otherwise. Allocates: call it when a path is needed, not every frame.
        /// </summary>
        public IReadOnlyList<Vector2> GetPath(Vector2 start, Vector2 goal, Func<MapTile, bool> passable = null)
        {
            if (!level) return Array.Empty<Vector2>();
            var path = level.Terrain.FindPath(transform.InverseTransformPoint(start), transform.InverseTransformPoint(goal), passable);
            var world = new Vector2[path.Count];
            for (int i = 0; i < world.Length; i++) world[i] = transform.TransformPoint(path[i]);
            return world;
        }

        bool TryGetBoardPosition(Key key, out Vector2 board)
        {
            if (view && view.Cells.TryGetValue(key, out var cell))
            {
                board = cell.Center + CenterNudge;
                return true;
            }
            board = default;
            return false;
        }

        MapTile TileAtBoard(Vector2 board) => level ? level.Terrain.GetTile(board) : default;

        // Swapping the level in the inspector during Play redraws the keys.
        void OnValidate()
        {
            if (Application.isPlaying && view) LevelChanged?.Invoke();
        }
    }
}
