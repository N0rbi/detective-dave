using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// One kind of terrain a key can be: floor, gap, wall, start, goal, or anything a designer adds.
    /// Types are assets, not code: create one with Assets > Create > Detective Dave > Level > Tile Type,
    /// paint it with a <see cref="TerrainTile"/> or give it to a whole <see cref="TileMapLayer"/>,
    /// and give it key sprites in each <see cref="LevelDefinition"/>.
    /// Game code that needs a particular type keeps a reference to its asset and compares references.
    /// Subclass it to attach game-specific data.
    /// </summary>
    [CreateAssetMenu(menuName = "Detective Dave/Level/Tile Type", fileName = "NewTileType", order = 1)]
    public class TileType : ScriptableObject
    {
        [Tooltip("Colour used in editor previews, gizmos and debug markers. The in-game look of a key comes from the Level Definition.")]
        [SerializeField] Color editorColor = Color.white;

        [Tooltip("Can the player and enemies walk on keys of this type? Pathfinding uses this unless the caller passes its own rule.")]
        [SerializeField] bool walkable;

        [Tooltip("Notes for the team: what this type means in the game. Not shown in game.")]
        [SerializeField, TextArea(2, 5)] string description;

        /// <summary>Colour for previews, gizmos and debug markers. Not the in-game key colour.</summary>
        public Color EditorColor => editorColor;

        /// <summary>True when the player and enemies can stand on keys of this type.</summary>
        public bool IsWalkable => walkable;

        /// <summary>The designer's notes about the type.</summary>
        public string Description => description ?? string.Empty;
    }
}
