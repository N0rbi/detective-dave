using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// Draws the level on the keys: every key takes the tint and the sprite of its tile type for its key shape, from the level's
    /// <see cref="KeyArt"/>. Keys where the level has nothing painted are hidden, so the keyboard takes the shape of the level.
    /// A view only: it asks <see cref="LevelLoader"/> what each key is and writes only to <see cref="KeyboardGridView"/> and
    /// <see cref="KeyCell.Background"/>. Lit and neighbour tints still win over the tint.
    /// Redraws when the loader puts another level on the keyboard and when the level's key art is edited.
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView), typeof(LevelLoader))]
    public sealed class KeyTerrainView : MonoBehaviour
    {
        [Tooltip("Hide keys where the level has nothing painted, so the keyboard takes the shape of the level. " +
                 "Ignored while the level's shape is empty (all keys stay visible).")]
        [SerializeField] bool hideKeysOutsideLevel = true;

        readonly HashSet<string> reported = new HashSet<string>();
        KeyboardGridView view;
        LevelLoader loader;
        KeyArt watchedArt;

        void Awake()
        {
            view = GetComponent<KeyboardGridView>();
            loader = GetComponent<LevelLoader>();
        }

        void OnEnable()
        {
            loader.LevelChanged += Refresh;
            Watch(CurrentArt);
        }

        void OnDisable()
        {
            loader.LevelChanged -= Refresh;
            Watch(null);
        }

        void Start() => Refresh();

        KeyArt CurrentArt => loader.Current ? loader.Current.KeyArt : null;

        /// <summary>Redraws every key from the current level.</summary>
        [ContextMenu("Refresh")]
        public void Refresh()
        {
            Watch(CurrentArt);
            if (view.Layout == null) return;
            var level = loader.Current;
            var art = CurrentArt;
            if (level && !art) ReportOnce($"Level '{level.DisplayName}' has no Key Art: keys are drawn as flat squares.");
            bool hideEmpty = hideKeysOutsideLevel && level && !loader.Terrain.IsEmpty;

            foreach (var cell in view.Cells.Values)
            {
                var type = loader.GetTile(cell.InputKey).Type;
                var style = art ? art.FindStyle(type) : null;
                cell.Background = style?.Tint;
                Draw(cell, level, art, type, style != null);
                if (hideEmpty && !type)
                    foreach (var part in cell.Parts) part.enabled = false;
            }
        }

        // The key art is an asset: editing it (in the inspector, even while playing) redraws the keys.
        void Watch(KeyArt art)
        {
            if (watchedArt == art) return;
            if (watchedArt) watchedArt.Changed -= Refresh;
            watchedArt = art;
            if (watchedArt) watchedArt.Changed += Refresh;
        }

        void Draw(KeyCell cell, Level level, KeyArt art, TileType type, bool styled)
        {
            var rects = cell.Def.Rects;
            if (rects.Count > 1 && art)
            {
                var whole = art.FindSprite(type, BoundsSize(rects), wholeKey: true);
                if (whole)
                {
                    view.SetKeySprite(cell, whole);
                    return;
                }
            }
            for (int part = 0; part < cell.Parts.Length; part++)
            {
                var size = new Vector2(rects[part].Width, rects[part].Height);
                var sprite = art ? art.FindSprite(type, size) : null;
                if (styled && !sprite) ReportOnce($"Key Art '{art.name}' (level '{level.DisplayName}') has no '{type.name}' sprite for a {size.x} x {size.y} key: drawn as a flat square.");
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
