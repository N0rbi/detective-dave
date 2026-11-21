using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class ShiftKeycapsView : MonoBehaviour
    {
        const int KeycapSortingOrder = 1;

        public void Configure(KeyboardGridView grid, Sprite sprite, Color color, float sizeShare)
        {
            Place(grid, Key.LeftShift, sprite, color, sizeShare);
            Place(grid, Key.RightShift, sprite, color, sizeShare);
        }

        void Place(KeyboardGridView grid, Key key, Sprite sprite, Color color, float sizeShare)
        {
            if (!grid.Cells.TryGetValue(key, out var cell) || cell.Parts.Length == 0) return;
            var bounds = cell.Parts[0].bounds;
            for (int index = 1; index < cell.Parts.Length; index++) bounds.Encapsulate(cell.Parts[index].bounds);
            var keycap = new GameObject(key + " Keycap").AddComponent<SpriteRenderer>();
            keycap.transform.SetParent(transform, false);
            keycap.sprite = sprite;
            keycap.color = color;
            keycap.sortingOrder = KeycapSortingOrder;
            var spriteSize = sprite.bounds.size;
            keycap.transform.position = new Vector3(bounds.center.x, bounds.center.y, 0f);
            keycap.transform.localScale = new Vector3(bounds.size.x * sizeShare / spriteSize.x, bounds.size.y * sizeShare / spriteSize.y, 1f);
        }
    }
}
