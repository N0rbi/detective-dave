using System;
using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>Scene wiring for one layout on the selection screen.</summary>
    [Serializable]
    public class LayoutOption
    {
        public TextAsset layout;      // one of Assets/Game/Keyboard/Data/*.json, its "digit" is the key
        public SpriteRenderer glyph;  // shown in the scene, dimmed when not chosen
    }
}
