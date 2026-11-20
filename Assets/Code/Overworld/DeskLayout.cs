using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DeskLayout : MonoBehaviour
    {
        const string TopName = "PLACEHOLDER Desk Top";
        const int TopSortingOrder = -50;

        CaseMarker[] markers;

        public IReadOnlyList<CaseMarker> Markers => markers ?? (markers = GetComponentsInChildren<CaseMarker>(true));

        public void DrawTop(Vector2 size, Color color)
        {
            var top = new GameObject(TopName);
            top.transform.SetParent(transform, false);
            top.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = top.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.CreateUnitSquare();
            renderer.color = color;
            renderer.sortingOrder = TopSortingOrder;
        }
    }
}
