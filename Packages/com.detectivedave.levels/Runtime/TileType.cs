using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// One kind of key (floor, gap, wall, start, goal, or anything a designer adds), and the tile you paint it with:
    /// pick it in the Level Tiles palette and paint the Shape tilemap of a <see cref="Level"/>.
    /// Its <c>Sprite</c> and <c>Color</c> are what the tile looks like while painting; the in-game look comes from the level's <see cref="KeyArt"/>.
    /// Create one with Assets > Create > Detective Dave > Tile Type, in Assets/Tiles. Game code that needs a particular kind
    /// keeps a reference to its asset and compares references. Subclass it to attach game-specific data.
    /// </summary>
    [CreateAssetMenu(menuName = "Detective Dave/Tile Type", fileName = "New Tile Type", order = 1)]
    [HelpURL(LevelsDocs.GuideUrl)]
    public class TileType : Tile
    {
        [Tooltip("Colour used in editor previews, check lists and debug markers. The in-game look of a key comes from the Key Art.")]
        [SerializeField] Color editorColor = Color.white;

        [Tooltip("Can the player and enemies walk on keys of this kind? Pathfinding uses this unless the caller passes its own rule.")]
        [SerializeField] bool walkable;

        [Tooltip("Notes for the team: what this kind of key means in the game. Not shown in game.")]
        [SerializeField, TextArea(2, 5)] string description;

        /// <summary>Colour for previews, check lists and debug markers. Not the in-game key colour.</summary>
        public Color EditorColor => editorColor;

        /// <summary>True when the player and enemies can stand on keys of this kind.</summary>
        public bool IsWalkable => walkable;

        /// <summary>The designer's notes about the kind.</summary>
        public string Description => description ?? string.Empty;
    }
}
