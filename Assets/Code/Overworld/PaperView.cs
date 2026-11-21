using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class PaperView : MonoBehaviour
    {
        const int SheetSortingOrder = 20;
        const int TextSortingOrder = 21;
        const int FontSize = 64;
        const float EmScale = 0.1f;

        SpriteRenderer sheet;
        TextMesh text;
        Vector2 rest;

        public void Configure(Sprite paper, Font font)
        {
            sheet = new GameObject("Sheet").AddComponent<SpriteRenderer>();
            sheet.transform.SetParent(transform, false);
            sheet.sprite = paper;
            sheet.sortingOrder = SheetSortingOrder;
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(transform, false);
            text = textObject.AddComponent<TextMesh>();
            text.font = font;
            text.fontSize = FontSize;
            text.richText = true;
            text.anchor = TextAnchor.UpperLeft;
            text.alignment = TextAlignment.Left;
            var renderer = textObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingOrder = TextSortingOrder;
            gameObject.SetActive(false);
        }

        public Vector2 TextArea(float paperWidth, float paddingShare)
        {
            float aspect = sheet.sprite.bounds.size.y / sheet.sprite.bounds.size.x;
            float inset = paperWidth * paddingShare * 2f;
            return new Vector2(paperWidth - inset, paperWidth * aspect - inset);
        }

        public void Place(PaperSpot spot, float lineHeight, float paddingShare)
        {
            rest = spot.Centre;
            transform.position = new Vector3(spot.Centre.x, spot.Centre.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, spot.RotationDegrees);
            float scale = spot.Width / sheet.sprite.bounds.size.x;
            sheet.transform.localScale = new Vector3(scale, scale, 1f);
            var size = (Vector2)sheet.sprite.bounds.size * scale;
            text.characterSize = lineHeight / (FontSize * EmScale);
            text.transform.localPosition = new Vector3(-size.x * 0.5f + size.x * paddingShare, size.y * 0.5f - size.x * paddingShare, 0f);
        }

        public void SlideFromRight(float distance) => transform.position = new Vector3(rest.x + distance, rest.y, transform.position.z);

        public void SetText(string richText, Color baseColor)
        {
            text.color = baseColor;
            text.text = richText;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);
    }
}
