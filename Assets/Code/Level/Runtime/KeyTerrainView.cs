using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// Draws the level on the keys: every key takes the tint and the sprite of its tile type for its key shape, from the level's
    /// <see cref="LevelDefinition"/> (and its style template). Keys where the level has nothing painted are hidden, so the keyboard
    /// takes the shape of the level. A view only: it asks <see cref="TileMapKeyboardMapper"/> what each key is and writes only to
    /// <see cref="KeyboardGridView"/> and <see cref="KeyCell.Background"/>. Lit and neighbour tints still win over the tint.
    /// Redraws when the mapper switches level; call <see cref="Refresh"/> for anything else.
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView), typeof(TileMapKeyboardMapper))]
    public sealed class KeyTerrainView : MonoBehaviour
    {
        [Tooltip("Hide keys where the level has nothing painted, so the keyboard takes the shape of the level. " +
                 "Ignored while the level has no baked shape (all keys stay visible).")]
        [SerializeField] bool hideKeysOutsideLevel = true;

        readonly HashSet<string> reported = new HashSet<string>();
        KeyboardGridView view;
        TileMapKeyboardMapper mapper;

        void Awake()
        {
            view = GetComponent<KeyboardGridView>();
            mapper = GetComponent<TileMapKeyboardMapper>();
        }

        void OnEnable() => mapper.LevelChanged += Refresh;

        void OnDisable() => mapper.LevelChanged -= Refresh;

        void Start() => Refresh();

        /// <summary>Redraws every key from the current level.</summary>
        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (view.Layout == null) return;
            var level = mapper.Level;
            if (level && level.Terrain.IsEmpty) ReportOnce($"Level '{level.name}' has no baked shape yet: every key reads as empty. Bake its Tile Map Group.");
            bool hideEmpty = hideKeysOutsideLevel && level && !level.Terrain.IsEmpty;

            foreach (var cell in view.Cells.Values)
            {
                var type = mapper.GetTile(cell.InputKey).Type;
                var style = level ? level.FindStyle(type) : null;
                cell.Background = style?.Tint;
                Draw(cell, level, type, style != null);
                if (hideEmpty && !type)
                    foreach (var part in cell.Parts) part.enabled = false;
            }
        }

        void Draw(KeyCell cell, LevelDefinition level, TileType type, bool styled)
        {
            var rects = cell.Def.Rects;
            if (rects.Count > 1 && level)
            {
                var whole = level.FindSprite(type, BoundsSize(rects), wholeKey: true);
                if (whole)
                {
                    view.SetKeySprite(cell, whole);
                    return;
                }
            }
            for (int part = 0; part < cell.Parts.Length; part++)
            {
                var size = new Vector2(rects[part].Width, rects[part].Height);
                var sprite = level ? level.FindSprite(type, size) : null;
                if (styled && !sprite) ReportOnce($"Level '{level.name}' has no '{type.name}' sprite for a {size.x} x {size.y} key: drawn as a flat square.");
                view.SetPartSprite(cell, part, sprite);
            }
        }

        static Vector2 BoundsSize(IReadOnlyList<KeyRect> rects)
        {
            float left = float.MaxValue, top = float.MaxValue, right = float.MinValue, bottom = float.MinValue;
            foreach (var rect in rects)
            {
                left = Mathf.Min(left, rect.X);
                top = Mathf.Min(top, rect.Y);
                right = Mathf.Max(right, rect.X + rect.Width);
                bottom = Mathf.Max(bottom, rect.Y + rect.Height);
            }
            return new Vector2(right - left, bottom - top);
        }

        // One warning per message: a missing sprite is a hole in the art, not a crash.
        void ReportOnce(string message)
        {
            if (reported.Add(message)) Debug.LogWarning(message, this);
        }
    }
}
