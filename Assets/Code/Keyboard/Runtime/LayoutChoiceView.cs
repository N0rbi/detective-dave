using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>Dims the glyphs of the layouts that were not chosen.</summary>
    public class LayoutChoiceView : MonoBehaviour
    {
        [SerializeField] KeyboardLayoutSelector selector;
        [SerializeField, Range(0f, 1f)] float dimmedAlpha = 0.25f;

        void OnEnable() => selector.LayoutLocked += Highlight;
        void OnDisable() => selector.LayoutLocked -= Highlight;

        void Highlight(KeyboardLayout chosen)
        {
            foreach (var choice in selector.Choices)
            {
                if (!choice.Glyph) continue;
                choice.Glyph.color = new Color(1f, 1f, 1f, choice.Layout.id == chosen.id ? 1f : dimmedAlpha);
            }
        }
    }
}
