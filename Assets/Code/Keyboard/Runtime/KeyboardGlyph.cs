using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Put on a SpriteRenderer that shows one of the keyboard_*.png masks with the KeyboardGlyph material.
    /// Pushes the palette colors into the renderer. Without a palette the shader's default colors are used.
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(SpriteRenderer))]
    public class KeyboardGlyph : MonoBehaviour
    {
        [SerializeField] KeyboardPalette palette;

        SpriteRenderer sr;
        MaterialPropertyBlock block;
        KeyboardPalette bound;

        public void SetPalette(KeyboardPalette value)
        {
            palette = value;
            Bind();
            Apply();
        }

        void OnEnable() { Bind(); Apply(); }

        void OnDisable()
        {
            if (bound) bound.Changed -= Apply;
            bound = null;
        }

        void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            Bind();
            Apply();
        }

        void Bind()
        {
            if (bound == palette) return;
            if (bound) bound.Changed -= Apply;
            bound = palette;
            if (bound) bound.Changed += Apply;
        }

        void Apply()
        {
            if (!palette) return;
            if (!sr) sr = GetComponent<SpriteRenderer>();
            block ??= new MaterialPropertyBlock();
            sr.GetPropertyBlock(block);
            KeyboardGlyphShader.SetColors(block, palette);
            sr.SetPropertyBlock(block);
        }
    }
}
