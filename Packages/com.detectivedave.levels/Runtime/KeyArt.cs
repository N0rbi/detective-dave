using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// How keys look in game: for every <see cref="TileType"/> a tint and a sprite per key shape, plus a default background.
    /// A <see cref="Level"/> references one Key Art; to give a level a different look, duplicate the Key Art and point the level at the copy.
    /// Lives in Assets/Key Art (Assets > Create > Detective Dave > Key Art).
    /// </summary>
    [CreateAssetMenu(menuName = "Detective Dave/Key Art", fileName = "New Key Art.config.asset", order = 2)]
    [HelpURL(LevelsDocs.GuideUrl)]
    public sealed class KeyArt : ScriptableObject
    {
        // Field names are kept from LevelDefinition (same script GUID), so existing Key Art assets load unchanged.
        [Header("Background")]
        [Tooltip("Drawn behind the keyboard in levels whose own Background has no sprite.")]
        [SerializeField] Sprite background;

        [Tooltip("Multiplied with the default background. White keeps the art's own colours.")]
        [SerializeField] Color backgroundTint = Color.white;

        [Header("Keys")]
        [Tooltip("How each kind of key looks: a tint and a sprite per key shape. One entry per Tile Type.")]
        [SerializeField] TileStyle[] tileStyles = Array.Empty<TileStyle>();

        /// <summary>
        /// Raised when the asset is edited in the inspector (also while playing), so views can redraw.
        /// Only raised from <c>OnValidate</c>: it never fires in a build.
        /// </summary>
        public event Action Changed;

        /// <summary>The background for levels that have none of their own, or null.</summary>
        public Sprite DefaultBackground => background;

        /// <summary>The tint that goes with <see cref="DefaultBackground"/>.</summary>
        public Color DefaultBackgroundTint => backgroundTint;

        /// <summary>The styles in inspector order.</summary>
        public IReadOnlyList<TileStyle> TileStyles => Array.AsReadOnly(tileStyles ?? Array.Empty<TileStyle>());

        /// <summary>The first style for <paramref name="type"/>, or null when there is none.</summary>
        public TileStyle FindStyle(TileType type)
        {
            if (!type) return null;
            foreach (var style in tileStyles ?? Array.Empty<TileStyle>())
                if (style != null && style.Type == type) return style;
            return null;
        }

        /// <summary>
        /// The sprite for a key shape of <paramref name="type"/>, or null. <paramref name="size"/> is in key units; <paramref name="wholeKey"/>
        /// asks for a sprite covering a whole multi-rectangle key (then <paramref name="size"/> is the box around the key).
        /// </summary>
        public Sprite FindSprite(TileType type, Vector2 size, bool wholeKey = false) => FindStyle(type)?.FindSprite(size, wholeKey);

        void OnValidate()
        {
            tileStyles ??= Array.Empty<TileStyle>();
            Changed?.Invoke();
        }
    }
}
