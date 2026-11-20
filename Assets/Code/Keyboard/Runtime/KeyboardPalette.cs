using System;
using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>The five colors of the keyboard glyphs. Create via Assets > Create > Game > Keyboard > Palette.</summary>
    [CreateAssetMenu(menuName = "Game/Keyboard/Palette", fileName = "KeyboardPalette.config")]
    public class KeyboardPalette : ScriptableObject
    {
        public Color panel = new Color32(20, 23, 32, 255);    // background behind the keys and the header
        public Color key = new Color32(48, 54, 70, 255);      // keycaps, modifiers included
        public Color edge = new Color32(84, 92, 116, 255);    // keycap outline
        public Color text = new Color32(232, 236, 244, 255);  // key labels and layout name
        public Color accent = new Color32(242, 184, 75, 255); // number badge

        /// <summary>Raised when a color is edited in the Inspector, so glyphs repaint live.</summary>
        public event Action Changed;

#if UNITY_EDITOR
        void OnValidate() => Changed?.Invoke();
#endif
    }
}
