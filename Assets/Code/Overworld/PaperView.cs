using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class PaperView : MonoBehaviour
    {
        const int SheetSortingOrder = 20;
        const int TextSortingOrder = 21;
        const int FontSize = 64;
        const float EmScale = 0.1f;
        const char MeasuredCharacter = '0';

        static readonly FontSlot[] AllSlots = (FontSlot[])Enum.GetValues(typeof(FontSlot));

        readonly Dictionary<FontSlot, Font> fonts = new Dictionary<FontSlot, Font>();
        readonly Dictionary<FontSlot, float> advancePixels = new Dictionary<FontSlot, float>();
        readonly Dictionary<int, float> glyphWidthPixels = new Dictionary<int, float>();
        readonly List<GlyphMesh> glyphMeshes = new List<GlyphMesh>();

        OverworldSettings settings;
        Font settingsRequestFont;
        SpriteRenderer sheet;
        Vector2 rest;
        Vector3 textOrigin;
        TextGrid grid;
        TextColorSet colors;

        public void Configure(Sprite paper, OverworldSettings overworldSettings)
        {
            settings = overworldSettings;
            sheet = new GameObject("Sheet").AddComponent<SpriteRenderer>();
            sheet.transform.SetParent(transform, false);
            sheet.sprite = paper;
            sheet.sortingOrder = SheetSortingOrder;
            colors = settings.TextColors;
            settingsRequestFont = settings.FontFor(FontSlot.Request);
            foreach (var slot in AllSlots) AddFont(slot, settings.FontFor(slot));
            gameObject.SetActive(false);
        }

        public float Aspect => sheet.sprite.bounds.size.y / sheet.sprite.bounds.size.x;

        public Vector2 TextArea(float paperWidth, float paddingShare)
        {
            float aspect = sheet.sprite.bounds.size.y / sheet.sprite.bounds.size.x;
            float inset = paperWidth * paddingShare * 2f;
            return new Vector2(paperWidth - inset, paperWidth * aspect - inset);
        }

        public void Place(PaperSpot spot, TextGrid textGrid, float paddingShare)
        {
            rest = spot.Centre;
            grid = textGrid;
            transform.position = new Vector3(spot.Centre.x, spot.Centre.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, spot.RotationDegrees);
            float scale = spot.Width / sheet.sprite.bounds.size.x;
            sheet.transform.localScale = new Vector3(scale, scale, 1f);
            var size = (Vector2)sheet.sprite.bounds.size * scale;
            textOrigin = new Vector3(-size.x * 0.5f + size.x * paddingShare, size.y * 0.5f - size.x * paddingShare, 0f);
        }

        public void SlideFromRight(float distance) => transform.position = new Vector3(rest.x + distance, rest.y, transform.position.z);

        public void UseColors(TextColorSet textColors) => colors = textColors;

        public void SetText(string richText, FontSlot[] slots) => SetText(richText, slots, LineScales.Uniform);

        public void SetText(string richText, FontSlot[] slots, LineScales scales)
        {
            var glyphs = GlyphPlan.Build(RichRuns.Parse(richText), slots);
            for (int index = 0; index < glyphs.Count; index++) DrawGlyph(MeshAt(index), glyphs[index], scales);
            for (int index = glyphs.Count; index < glyphMeshes.Count; index++) glyphMeshes[index].Hide();
        }

        public void UseRequestFont(Font caseOverride) => ChangeFont(FontSlot.Request, RequestFontChoice.For(caseOverride, settingsRequestFont));

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        void AddFont(FontSlot slot, Font font)
        {
            fonts[slot] = font;
            advancePixels[slot] = MeasureAdvance(font);
        }

        void ChangeFont(FontSlot slot, Font font)
        {
            if (fonts[slot] == font) return;
            AddFont(slot, font);
            glyphWidthPixels.Clear();
        }

        static float MeasureAdvance(Font font)
        {
            font.RequestCharactersInTexture(MeasuredCharacter.ToString(), FontSize);
            font.GetCharacterInfo(MeasuredCharacter, out var info, FontSize);
            if (info.advance <= 0) throw new InvalidOperationException("Font has no letter width: " + font.name);
            return info.advance;
        }

        float WidthPixelsOf(FontSlot slot, char character)
        {
            int key = ((int)slot << 16) | character;
            if (glyphWidthPixels.TryGetValue(key, out float known)) return known;
            var font = fonts[slot];
            font.RequestCharactersInTexture(character.ToString(), FontSize);
            font.GetCharacterInfo(character, out var info, FontSize);
            float width = Mathf.Max(info.advance, info.maxX - info.minX);
            glyphWidthPixels[key] = width;
            return width;
        }

        GlyphMesh MeshAt(int index)
        {
            while (glyphMeshes.Count <= index) glyphMeshes.Add(NewGlyphMesh(glyphMeshes.Count));
            return glyphMeshes[index];
        }

        GlyphMesh NewGlyphMesh(int index)
        {
            var textObject = new GameObject("Glyph " + index);
            textObject.transform.SetParent(transform, false);
            var mesh = textObject.AddComponent<TextMesh>();
            mesh.fontSize = FontSize;
            mesh.richText = false;
            mesh.anchor = TextAnchor.UpperCenter;
            mesh.alignment = TextAlignment.Center;
            textObject.GetComponent<MeshRenderer>().sortingOrder = TextSortingOrder;
            return new GlyphMesh(mesh);
        }

        void DrawGlyph(GlyphMesh glyphMesh, Glyph glyph, LineScales scales)
        {
            var color = ColorOf(glyph.ColorHex, colors.ForSlot(glyph.Slot));
            if (color.a <= 0f)
            {
                glyphMesh.Hide();
                return;
            }
            float cellWidth = grid.LetterWidth * scales.ShareOf(glyph.Row);
            float characterSize = GlyphCell.CharacterSize(cellWidth, grid.LineStep * scales.ShareOf(glyph.Row), advancePixels[glyph.Slot], FontSize, EmScale);
            float glyphWidth = WidthPixelsOf(glyph.Slot, glyph.Character) * characterSize * EmScale;
            float x = textOrigin.x + GlyphCell.CentreX(glyph.Column, cellWidth);
            float y = textOrigin.y + GlyphCell.TopY(scales.OffsetOf(glyph.Row), grid.LineStep, settings.BaselineOffsetShareFor(glyph.Slot), scales.ShareOf(glyph.Row));
            glyphMesh.Show(glyph.Character, fonts[glyph.Slot], color, characterSize, GlyphCell.HorizontalScale(glyphWidth, cellWidth), new Vector3(x, y, textOrigin.z));
        }

        static Color ColorOf(string hex, Color slotColor)
        {
            if (hex == null) return slotColor;
            if (!ColorUtility.TryParseHtmlString("#" + hex, out var color)) throw new FormatException("Not a colour: " + hex);
            return color;
        }

        sealed class GlyphMesh
        {
            readonly TextMesh mesh;
            readonly MeshRenderer meshRenderer;
            char character;

            public GlyphMesh(TextMesh textMesh)
            {
                mesh = textMesh;
                meshRenderer = textMesh.GetComponent<MeshRenderer>();
            }

            public void Hide() => mesh.gameObject.SetActive(false);

            public void Show(char shown, Font font, Color color, float characterSize, float horizontalScale, Vector3 localPosition)
            {
                mesh.gameObject.SetActive(true);
                if (mesh.font != font)
                {
                    mesh.font = font;
                    meshRenderer.sharedMaterial = font.material;
                }
                if (character != shown)
                {
                    character = shown;
                    mesh.text = shown.ToString();
                }
                mesh.characterSize = characterSize;
                mesh.color = color;
                mesh.transform.localScale = new Vector3(horizontalScale, 1f, 1f);
                mesh.transform.localPosition = localPosition;
            }
        }
    }
}
