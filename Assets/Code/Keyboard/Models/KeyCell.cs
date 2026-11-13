using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>A key as it exists in the grid scene: its definition, its sprites and its lit state.</summary>
    public class KeyCell
    {
        public KeyDef Def { get; }
        public Key InputKey { get; }
        public SpriteRenderer[] Parts { get; }   // one per rectangle of the key
        public Vector2 LabelPosition { get; }    // local space of the grid object

        public bool Active { get; set; }
        public float PulseUntil { get; set; }    // toggle keys stay lit until this unscaled time

        public KeyCell(KeyDef def, Key inputKey, SpriteRenderer[] parts, Vector2 labelPosition)
        {
            Def = def;
            InputKey = inputKey;
            Parts = parts;
            LabelPosition = labelPosition;
        }
    }
}
