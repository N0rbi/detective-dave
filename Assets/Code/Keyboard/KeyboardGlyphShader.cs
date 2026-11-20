using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Code mirror of KeyboardGlyph.shader: every shader property has exactly one entry here.
    /// Change a property in one place, change it in the other.
    /// </summary>
    public static class KeyboardGlyphShader
    {
        public const string Name = "Game/Keyboard Glyph";

        public static readonly int MainTex = Shader.PropertyToID("_MainTex");           // the mask, set by the SpriteRenderer
        public static readonly int Panel = Shader.PropertyToID("_Panel");               // background
        public static readonly int Key = Shader.PropertyToID("_Key");                   // keycaps
        public static readonly int Edge = Shader.PropertyToID("_Edge");                 // keycap outline
        public static readonly int Text = Shader.PropertyToID("_Text");                 // labels and layout name
        public static readonly int Accent = Shader.PropertyToID("_Accent");             // number badge
        public static readonly int CornerRadius = Shader.PropertyToID("_CornerRadius"); // panel corners, texture pixels

        /// <summary>Writes the five palette colors into a property block.</summary>
        public static void SetColors(MaterialPropertyBlock block, KeyboardPalette palette)
        {
            block.SetColor(Panel, palette.panel);
            block.SetColor(Key, palette.key);
            block.SetColor(Edge, palette.edge);
            block.SetColor(Text, palette.text);
            block.SetColor(Accent, palette.accent);
        }
    }
}
