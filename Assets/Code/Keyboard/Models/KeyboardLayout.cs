using System;
using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>One layout file from Assets/Game/Keyboard/Data. Field names match the JSON.</summary>
    [Serializable]
    public class KeyboardLayout
    {
        public string id;
        public string name;
        public string form;          // ansi, iso or jis
        public int digit;            // number-row key that selects this layout
        public float unitsWide;
        public float unitsHigh;
        public KeyDef[] keys;

        /// <summary>Board size in key units.</summary>
        public Vector2 Size => new Vector2(unitsWide, unitsHigh);

        public static KeyboardLayout Parse(TextAsset json) => JsonUtility.FromJson<KeyboardLayout>(json.text);
    }
}
