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
        readonly Dictionary<FontSlot, List<TextMesh>> layers = new Dictionary<FontSlot, List<TextMesh>>();

        SpriteRenderer sheet;
        Vector2 rest;
        Vector3 textOrigin;
        TextGrid grid;

        public void Configure(Sprite paper, OverworldSettings settings)
        {
            sheet = new GameObject("Sheet").AddComponent<SpriteRenderer>();
            sheet.transform.SetParent(transform, false);
            sheet.sprite = paper;
            sheet.sortingOrder = SheetSortingOrder;
            foreach (var slot in AllSlots) AddLayer(slot, settings.FontFor(slot));
            gameObject.SetActive(false);
        }

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

        public void SetText(string richText, FontSlot[] slots, Color baseColor)
        {
            var runs = RichRuns.Parse(richText);
            foreach (var slot in AllSlots) ShowLayer(slot, LayerLines.Build(runs, slots, slot), baseColor);
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        void AddLayer(FontSlot slot, Font font)
        {
            fonts[slot] = font;
            advancePixels[slot] = MeasureAdvance(font);
            layers[slot] = new List<TextMesh>();
        }

        static float MeasureAdvance(Font font)
        {
            font.RequestCharactersInTexture(MeasuredCharacter.ToString(), FontSize);
            font.GetCharacterInfo(MeasuredCharacter, out var info, FontSize);
            if (info.advance <= 0) throw new InvalidOperationException("Font has no letter width: " + font.name);
            return info.advance;
        }

        void ShowLayer(FontSlot slot, string[] lines, Color baseColor)
        {
            var meshes = layers[slot];
            for (int index = 0; index < lines.Length; index++)
            {
                var mesh = MeshAt(slot, meshes, index);
                bool visible = lines[index] != null;
                mesh.gameObject.SetActive(visible);
                if (visible) Draw(mesh, slot, lines[index], index, baseColor);
            }
            for (int index = lines.Length; index < meshes.Count; index++) meshes[index].gameObject.SetActive(false);
        }

        TextMesh MeshAt(FontSlot slot, List<TextMesh> meshes, int index)
        {
            while (meshes.Count <= index) meshes.Add(NewMesh(slot, meshes.Count));
            return meshes[index];
        }

        TextMesh NewMesh(FontSlot slot, int index)
        {
            var textObject = new GameObject("Text " + slot + " " + index);
            textObject.transform.SetParent(transform, false);
            var mesh = textObject.AddComponent<TextMesh>();
            mesh.font = fonts[slot];
            mesh.fontSize = FontSize;
            mesh.richText = true;
            mesh.anchor = TextAnchor.UpperLeft;
            mesh.alignment = TextAlignment.Left;
            var renderer = textObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = fonts[slot].material;
            renderer.sortingOrder = TextSortingOrder;
            return mesh;
        }

        void Draw(TextMesh mesh, FontSlot slot, string line, int index, Color baseColor)
        {
            mesh.characterSize = grid.LetterWidth / (advancePixels[slot] * EmScale);
            mesh.color = baseColor;
            mesh.text = line;
            mesh.transform.localPosition = textOrigin + Vector3.down * (index * grid.LineStep);
        }
    }
}
