using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// Everything that makes one level, as data: its name, its background, how each <see cref="TileType"/> looks on the keys,
    /// and its shape (which keys are floor, gap, wall, ...), baked from a <see cref="TileMapGroup"/>.
    /// Create one per level with Assets > Create > Detective Dave > Level > Level Definition and assign it; no code per level.
    /// <para>
    /// Styles can come from a <see cref="StyleTemplate"/>: anything this level leaves empty (a style, a single shape sprite,
    /// the background) is taken from the template, then from the template's template, and so on.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Lookups (<see cref="FindStyle"/>, <see cref="FindSprite"/>, <see cref="Background"/>) walk the template chain and allocate a little:
    /// call them when a level is set up or redrawn, not every frame.
    /// </remarks>
    [CreateAssetMenu(menuName = "Detective Dave/Level/Level Definition", fileName = "NewLevel", order = 0)]
    public sealed class LevelDefinition : ScriptableObject
    {
        const int MaxTemplateDepth = 16;

        [Header("Level")]
        [Tooltip("The name players see. Leave empty to use the asset name.")]
        [SerializeField] string displayName;

        [Header("Background")]
        [Tooltip("Drawn behind the keyboard. Leave empty to use the style template's background.")]
        [SerializeField] Sprite background;

        [Tooltip("Multiplied with the background sprite. White keeps the art's own colours.")]
        [SerializeField] Color backgroundTint = Color.white;

        [Header("Key Art")]
        [Tooltip("Optional. Another level whose key art and background fill in whatever this level leaves empty. " +
                 "Make one \"style\" level and point new levels at it to restyle them all in one place.")]
        [SerializeField] LevelDefinition styleTemplate;

        [Tooltip("How each tile type looks on the keys: a tint and a sprite per key shape. One entry per type.")]
        [SerializeField] TileStyle[] tileStyles = Array.Empty<TileStyle>();

        [Header("Shape")]
        [Tooltip("Baked from a Tile Map Group: press Bake on the group (or let it bake before Play). Not edited by hand.")]
        [SerializeField] LevelTerrain terrain = new LevelTerrain();

        [SerializeField, HideInInspector] string bakedFrom;

        /// <summary>The name players see: the display name, or the asset name when that is empty.</summary>
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        /// <summary>The background sprite of this level, or of the nearest template that has one. Null when none has.</summary>
        public Sprite Background
        {
            get
            {
                var owner = FindInChain(level => level.background);
                return owner ? owner.background : null;
            }
        }

        /// <summary>The tint that goes with <see cref="Background"/> (it comes from the same level as the sprite).</summary>
        public Color BackgroundTint
        {
            get
            {
                var owner = FindInChain(level => level.background);
                return owner ? owner.backgroundTint : backgroundTint;
            }
        }

        /// <summary>The level whose art fills in what this one leaves empty, or null.</summary>
        public LevelDefinition StyleTemplate => styleTemplate;

        /// <summary>This level's own styles, in inspector order (template styles not included).</summary>
        public IReadOnlyList<TileStyle> TileStyles => Array.AsReadOnly(tileStyles ?? Array.Empty<TileStyle>());

        /// <summary>The level's shape. Empty until baked.</summary>
        public LevelTerrain Terrain => terrain ??= new LevelTerrain();

        /// <summary>What the terrain was last baked from (scene and object path), for display. Empty when never baked.</summary>
        internal string BakedFrom => bakedFrom ?? string.Empty;

        /// <summary>The style for <paramref name="type"/> from this level, or else from the nearest template that has one. Null when none has.</summary>
        public TileStyle FindStyle(TileType type)
        {
            if (!type) return null;
            foreach (var level in Chain())
            {
                var style = level.OwnStyle(type);
                if (style != null) return style;
            }
            return null;
        }

        /// <summary>
        /// The sprite for a key shape of <paramref name="type"/>: the first match in this level, then in each template in turn.
        /// <paramref name="size"/> is in key units; <paramref name="wholeKey"/> asks for a sprite covering a whole multi-rectangle key
        /// (then <paramref name="size"/> is the box around the key). Null when no level in the chain has one.
        /// </summary>
        public Sprite FindSprite(TileType type, Vector2 size, bool wholeKey = false)
        {
            if (!type) return null;
            foreach (var level in Chain())
            {
                var sprite = level.OwnStyle(type)?.FindSprite(size, wholeKey);
                if (sprite) return sprite;
            }
            return null;
        }

        /// <summary>This level, then its template, then that one's template: each once, even when templates loop.</summary>
        internal IEnumerable<LevelDefinition> Chain()
        {
            var seen = new HashSet<LevelDefinition>();
            for (var level = this; level && seen.Count < MaxTemplateDepth && seen.Add(level); level = level.styleTemplate)
                yield return level;
        }

        /// <summary>True when following the templates from this level comes back to a level already visited.</summary>
        internal bool HasTemplateLoop()
        {
            var seen = new HashSet<LevelDefinition>();
            for (var level = this; level; level = level.styleTemplate)
                if (!seen.Add(level)) return true;
            return false;
        }

        internal void SetTerrain(LevelTerrain baked, string source)
        {
            terrain = baked ?? new LevelTerrain();
            bakedFrom = source ?? string.Empty;
        }

        TileStyle OwnStyle(TileType type)
        {
            foreach (var style in tileStyles ?? Array.Empty<TileStyle>())
                if (style != null && style.Type == type) return style;
            return null;
        }

        LevelDefinition FindInChain(Func<LevelDefinition, bool> has)
        {
            foreach (var level in Chain())
                if (has(level)) return level;
            return null;
        }

        void OnValidate()
        {
            tileStyles ??= Array.Empty<TileStyle>();
            terrain ??= new LevelTerrain();
        }
    }
}
