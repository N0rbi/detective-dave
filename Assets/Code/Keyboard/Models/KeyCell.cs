using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>A key as it exists in the grid scene: its definition, its sprites and its lit state.</summary>
    public class KeyCell
    {
        public KeyDef Def { get; }
        public Key InputKey { get; }
        public SpriteRenderer[] Parts { get; }   // one per rectangle of the key
        public Vector2 Center { get; }           // where the player stands: local space of the grid object

        public bool Active { get; set; }
        public bool Neighbour { get; set; }      // touches a lit key (debug tint)
        public Color? Background { get; set; }   // the idle color when the key is not lit, null for the palette's key color

        public KeyCell(KeyDef def, Key inputKey, SpriteRenderer[] parts, Vector2 center)
        {
            Def = def;
            InputKey = inputKey;
            Parts = parts;
            Center = center;
        }
    }
}
