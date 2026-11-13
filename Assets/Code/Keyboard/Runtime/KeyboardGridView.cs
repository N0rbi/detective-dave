using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// The view of the locked layout: one group of sprites per key inside a rectangle (the gizmo in the Scene view),
    /// tinted from each cell's Active state. Knows nothing about input. Needs an orthographic camera tagged MainCamera.
    /// Stays empty when no layout is locked yet.
    /// </summary>
    public class KeyboardGridView : MonoBehaviour
    {
        const float BackgroundMargin = 0.4f;            // world units of backdrop around the keys
        const int BackgroundSortingOrder = -1;
        const int CellSortingOrder = 0;

        [SerializeField] Vector2 size = new Vector2(16f, 5f);       // the rectangle the layout is fitted into, in world units
        [SerializeField] KeyboardPalette palette;                    // idle = key, lit = accent, backdrop = panel
        [SerializeField] Material cellMaterial;                      // flat unlit sprite material (Art/KeyboardCell.mat)
        [SerializeField, Range(0f, 0.3f)] float gap = 0.08f;         // between cells, in key units

        readonly Dictionary<Key, KeyCell> cells = new Dictionary<Key, KeyCell>();
        Sprite square;
        SpriteRenderer background;

        public KeyboardLayout Layout { get; private set; }           // null until built
        public KeyboardPalette Palette { get; private set; }         // the palette in use
        public float Scale { get; private set; }                     // key units to world units
        public IReadOnlyDictionary<Key, KeyCell> Cells => cells;

        void Start()
        {
            if (!KeyboardSession.IsLocked)
            {
                enabled = false;
                return;
            }
            Layout = KeyboardSession.Layout;
            Palette = palette ? palette : ScriptableObject.CreateInstance<KeyboardPalette>();
            Build();
        }

        // Late, so the Active flags set by the input component in Update are what gets drawn.
        void LateUpdate()
        {
            if (Layout == null) return;
            background.color = Palette.panel;
            foreach (var cell in cells.Values)
            {
                var color = cell.Active ? Palette.accent : Palette.key;
                foreach (var part in cell.Parts) part.color = color;
            }
        }

        /// <summary>The cell under a screen position, or null.</summary>
        public KeyCell CellAt(Vector2 screenPosition)
        {
            var cam = Camera.main;
            if (Layout == null || !cam) return null;
            var local = GridUtils.ScreenToLocal(cam, transform, screenPosition);
            return GridUtils.FindCellAt(cells.Values, GridUtils.ToKeyUnits(Layout, Scale, local));
        }

        public Vector3 LabelWorldPosition(KeyCell cell) => transform.TransformPoint(cell.LabelPosition);

        void Build()
        {
            Scale = GridUtils.FitScale(Layout, size);
            square = SpriteFactory.CreateUnitSquare();
            background = NewPart("Background", Vector2.zero, Layout.Size * Scale + Vector2.one * (BackgroundMargin * 2f), BackgroundSortingOrder);

            foreach (var unknown in Layout.keys.Where(candidate => !candidate.InputKey.HasValue))
                Debug.LogWarning($"Layout '{Layout.id}': '{unknown.code}' is not an Input System key name.", this);

            var firstOfEachKey = Layout.keys
                .Where(candidate => candidate.InputKey.HasValue)
                .GroupBy(candidate => candidate.InputKey.Value)
                .Select(group => group.First());
            foreach (var def in firstOfEachKey)
                cells.Add(def.InputKey.Value, CreateCell(def));
        }

        KeyCell CreateCell(KeyDef def)
        {
            var parts = def.rects
                .Select(rect => NewPart(def.code, GridUtils.ToLocal(Layout, Scale, rect.Center), GridUtils.CellSize(rect, Scale, gap), CellSortingOrder))
                .ToArray();
            return new KeyCell(def, def.InputKey.Value, parts, GridUtils.ToLocal(Layout, Scale, def.center));
        }

        SpriteRenderer NewPart(string partName, Vector2 localPosition, Vector2 localSize, int sortingOrder)
        {
            var go = new GameObject(partName);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(localSize.x, localSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.sortingOrder = sortingOrder;
            if (cellMaterial) sr.sharedMaterial = cellMaterial;
            return sr;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0f));
        }
    }
}
