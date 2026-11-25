using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class ShiftKeycapsView : MonoBehaviour
    {
        const int KeycapSortingOrder = 1;
        const int PunchVibrato = 6;
        const float PunchElasticity = 0.5f;

        readonly Dictionary<Key, Transform> keycaps = new Dictionary<Key, Transform>();

        public void Configure(KeyboardGridView grid, Sprite sprite, Color color, float sizeShare)
        {
            Place(grid, Key.LeftShift, sprite, color, sizeShare);
            Place(grid, Key.RightShift, sprite, color, sizeShare);
        }

        public void Punch(Key key, float scaleAmount, float seconds)
        {
            if (!keycaps.TryGetValue(key, out var keycap)) return;
            keycap.DOComplete();
            keycap.DOPunchScale(keycap.localScale * scaleAmount, seconds, PunchVibrato, PunchElasticity);
        }

        void OnDestroy()
        {
            foreach (var keycap in keycaps.Values) keycap.DOKill();
        }

        void Place(KeyboardGridView grid, Key key, Sprite sprite, Color color, float sizeShare)
        {
            if (!grid.Cells.TryGetValue(key, out var cell) || cell.Parts.Length == 0) return;
            var bounds = cell.Parts[0].bounds;
            for (int index = 1; index < cell.Parts.Length; index++) bounds.Encapsulate(cell.Parts[index].bounds);
            var keycap = new GameObject(key + " Keycap").AddComponent<SpriteRenderer>();
            keycaps[key] = keycap.transform;
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
