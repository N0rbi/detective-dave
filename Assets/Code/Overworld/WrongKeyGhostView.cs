using DetectiveDave.KeyboardGrid;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class WrongKeyGhostView : MonoBehaviour
    {
        const int GhostSortingOrder = 2;

        KeyboardGridView grid;
        SpriteRenderer ghost;
        float sizeShare;
        Color color;
        float fadeSeconds;

        public void Configure(KeyboardGridView gridView, OverworldSettings settings)
        {
            grid = gridView;
            sizeShare = settings.ShiftKeycapSizeShare;
            color = settings.StartHintWrongKeyColor;
            fadeSeconds = settings.StartHintWrongKeyFadeSeconds;
            ghost = new GameObject("Wrong Key Ghost").AddComponent<SpriteRenderer>();
            ghost.transform.SetParent(transform, false);
            ghost.sprite = settings.ShiftKeycapSprite;
            ghost.sortingOrder = GhostSortingOrder;
            ghost.enabled = false;
        }

        public void Show(Key key)
        {
            if (!grid.Cells.TryGetValue(key, out var cell) || cell.Parts.Length == 0) return;
            var bounds = cell.Parts[0].bounds;
            for (int index = 1; index < cell.Parts.Length; index++) bounds.Encapsulate(cell.Parts[index].bounds);
            var spriteSize = ghost.sprite.bounds.size;
            ghost.transform.position = new Vector3(bounds.center.x, bounds.center.y, 0f);
            ghost.transform.localScale = new Vector3(bounds.size.x * sizeShare / spriteSize.x, bounds.size.y * sizeShare / spriteSize.y, 1f);
            ghost.DOKill();
            ghost.color = color;
            ghost.enabled = true;
            DOTween.ToAlpha(() => ghost.color, tint => ghost.color = tint, 0f, fadeSeconds).SetEase(Ease.InQuad).SetTarget(ghost).OnComplete(() => ghost.enabled = false);
        }

        void OnDestroy()
        {
            if (ghost) ghost.DOKill();
        }
    }
}
