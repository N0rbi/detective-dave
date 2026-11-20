using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// The view of the locked layout: one group of sprites per key inside a rectangle (the gizmo in the Scene view),
    /// tinted from each cell's Active state. Knows nothing about input. Needs an orthographic camera tagged MainCamera.
    /// Stays empty when no layout is locked yet. Builds in Awake.
    /// </summary>
    public class KeyboardGridView : MonoBehaviour
    {
        const float BackgroundMargin = 0.4f;            // world units of backdrop around the keys
        const int BackgroundSortingOrder = -1;
        const int CellSortingOrder = 0;

        [SerializeField] Vector2 size = new Vector2(15f, 4f);       // the rectangle the layout is fitted into, in world units (15 x 4: one key is one world unit)
        [SerializeField] KeyboardPalette palette;                    // idle = key, lit = accent, backdrop = panel
        [SerializeField] Material cellMaterial;                      // flat unlit sprite material (Art/KeyboardCell.mat)
        [SerializeField, Tooltip("Off: the keys and the backdrop are not drawn, but the board and the key positions still work (the overworld desk).")] bool drawKeys = true;
        [SerializeField, Range(0f, 0.3f)] float gap = 0.08f;         // between cells, in key units
        [SerializeField, Min(1f), Tooltip("How many times taller than wide the keys are drawn. The climbing player needs room above a gutter. Keep equal to Key Height Stretch in Project Settings > Detective Dave > Levels.")]
        float keyHeightStretch = GridUtils.DefaultHeightStretch;

        readonly Dictionary<Key, KeyCell> cells = new Dictionary<Key, KeyCell>();
        Sprite square;
        SpriteRenderer background;

        public KeyboardLayout Layout { get; private set; }           // null until built
        public KeyboardPalette Palette { get; private set; }         // the palette in use
        public float Scale { get; private set; }                     // key units to world units
        public float HeightStretch => keyHeightStretch;
        public IReadOnlyDictionary<Key, KeyCell> Cells => cells;

        /// <summary>The width of the keys plus the backdrop around them, in the grid object's local units.</summary>
        public float ContentWidth => Layout == null ? 0f : Layout.UnitsWide * Scale + BackgroundMargin * 2f;

        public float ContentHeight => Layout == null ? 0f : Layout.UnitsHigh * Scale * HeightStretch + BackgroundMargin * 2f;

        // Built in Awake so that every Start can already ask for the layout and the cells.
        void Awake()
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
                var color = cell.Active ? Palette.accent : cell.Neighbour ? Palette.edge : cell.Background ?? Palette.key;
                foreach (var part in cell.Parts) part.color = color;
            }
        }

        /// <summary>Draws one rectangle of a key with a sprite, stretched to fill it. Null goes back to the flat square.</summary>
        public void SetPartSprite(KeyCell cell, int part, Sprite sprite)
        {
            if (!sprite) sprite = square;
            var renderer = cell.Parts[part];
            var drawn = GridUtils.PartRect(cell.Def.Rects, part, gap);
            renderer.enabled = true;
            renderer.sprite = sprite;
            renderer.transform.localPosition = GridUtils.ToLocal(Layout, Scale, drawn.center, HeightStretch);
            renderer.transform.localScale = GridUtils.ScaleToFit(GridUtils.ToLocalSize(drawn.size, Scale, HeightStretch), sprite.bounds.size);
        }

        /// <summary>Draws a key of several rectangles with one sprite over the whole of it (the L-shaped Enter). The first part carries it, the others are hidden.</summary>
        public void SetKeySprite(KeyCell cell, Sprite sprite)
        {
            var bounds = GridUtils.KeyBounds(cell.Def.Rects, gap);
            var renderer = cell.Parts[0];
            renderer.enabled = true;
            renderer.sprite = sprite;
            renderer.transform.localPosition = GridUtils.ToLocal(Layout, Scale, bounds.center, HeightStretch);
            renderer.transform.localScale = GridUtils.ScaleToFit(GridUtils.ToLocalSize(bounds.size, Scale, HeightStretch), sprite.bounds.size);
            for (int part = 1; part < cell.Parts.Length; part++) cell.Parts[part].enabled = false;
        }

        public Vector3 CenterWorldPosition(KeyCell cell) => transform.TransformPoint(cell.Center);

        /// <summary>Where the player stands on a key, in world space. False for a key the layout does not have.</summary>
        public bool TryGetWorldCenter(Key key, out Vector2 world)
        {
            bool found = cells.TryGetValue(key, out var cell);
            world = found ? (Vector2)CenterWorldPosition(cell) : default;
            return found;
        }

        void Build()
        {
            Scale = GridUtils.FitScale(Layout, size);
            square = SpriteFactory.CreateUnitSquare();
            background = NewPart("Background", Vector2.zero, GridUtils.ToLocalSize(Layout.Size, Scale, HeightStretch) + Vector2.one * (BackgroundMargin * 2f), BackgroundSortingOrder);

            foreach (var unknown in Layout.Keys.Where(candidate => !candidate.InputKey.HasValue))
                Debug.LogWarning($"Layout '{Layout.Id}': '{unknown.Code}' is not an Input System key name.", this);

            foreach (var def in Layout.UniqueKeys())
                cells.Add(def.InputKey.Value, CreateCell(def));
        }

        KeyCell CreateCell(KeyDef def)
        {
            var parts = Enumerable.Range(0, def.Rects.Count)
                .Select(index =>
                {
                    var drawn = GridUtils.PartRect(def.Rects, index, gap);
                    return NewPart(def.Code, GridUtils.ToLocal(Layout, Scale, drawn.center, HeightStretch), GridUtils.ToLocalSize(drawn.size, Scale, HeightStretch), CellSortingOrder);
                })
                .ToArray();
            return new KeyCell(def, def.InputKey.Value, parts, GridUtils.ToLocal(Layout, Scale, def.Center, HeightStretch));
        }

        SpriteRenderer NewPart(string partName, Vector2 localPosition, Vector2 localSize, int sortingOrder)
        {
            var go = new GameObject(partName);
            go.AddComponent<KeyTag>();
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(localSize.x, localSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.sortingOrder = sortingOrder;
            sr.enabled = drawKeys;
            if (cellMaterial) sr.sharedMaterial = cellMaterial;
            return sr;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y * keyHeightStretch, 0f));
        }
    }
}
