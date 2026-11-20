using System;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    [RequireComponent(typeof(LevelLoader), typeof(KeyboardGridView))]
    public sealed class LevelCellClassifier : MonoBehaviour, IPlayerLevel
    {
        [SerializeField] TileType gap;
        [SerializeField] TileType wallGutter;

        LevelLoader loader;
        KeyboardGridView view;

        public event Action LevelChanged;

        void Awake()
        {
            loader = GetComponent<LevelLoader>();
            view = GetComponent<KeyboardGridView>();
        }

        void OnEnable() => loader.LevelChanged += ForwardLevelChanged;

        void OnDisable() => loader.LevelChanged -= ForwardLevelChanged;

        public bool HasLevel => loader.Current;

        public bool TryGetEnds(out Key start, out Key goal)
        {
            start = default;
            goal = default;
            var level = loader.Current;
            if (!level) return false;
            ShiftEnds.Of(level.StartKey, out start, out goal);
            return view.Cells.ContainsKey(start) && view.Cells.ContainsKey(goal);
        }

        public CellKind KindOf(Key key) => IsShift(key) ? CellKind.Floor : Classify(loader.GetTile(key), gap, wallGutter);

        public static CellKind Classify(MapTile tile, TileType gap, TileType wallGutter)
        {
            if (tile.IsEmpty) return CellKind.Wall;
            if (tile.Type == gap) return CellKind.Gap;
            if (tile.Type == wallGutter) return CellKind.Gutter;
            return tile.IsWalkable ? CellKind.Floor : CellKind.Wall;
        }

        static bool IsShift(Key key) => key == Key.LeftShift || key == Key.RightShift;

        void ForwardLevelChanged() => LevelChanged?.Invoke();
    }
}
