using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.Levels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// Puts a level on the keyboard: instantiates the level prefab under this object (the keyboard), builds its terrain once,
    /// and answers what tile each key is and how to get from one place to another.
    /// <para>
    /// A normal run plays the first level of <see cref="Sequence"/> (in <see cref="Start"/>). A <see cref="Load"/> before then
    /// (the editor's Play This Level, or a test) wins, even <c>Load(null)</c>, and the sequence is not used. <see cref="Reload"/> rebuilds only the level
    /// instance: the keyboard, its board and its key source are never touched.
    /// </para>
    /// <para>
    /// Board space is this object's local space; the level root is placed at its origin, so the level's own local space is the board.
    /// Keys are looked up in the view; nothing here talks to the input package.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView))]
    public sealed class LevelLoader : MonoBehaviour
    {
        // A key center usually lies exactly on a tile border (a key is 4 x 4 tiles). Reading just above and right of it keeps every answer stable.
        static readonly Vector2 CenterNudge = new Vector2(0.01f, 0.01f);

        [Tooltip("The levels of the game, in order. A normal run starts with the first one.")]
        [SerializeField] LevelSequence sequence;

        [Tooltip("Draw the level's authoring tilemaps and its Background renderer while playing (debug). Off: only the keys and the background view show the level.")]
        [SerializeField] bool showAuthoringRenderers;

        KeyboardGridView view;
        Level source;
        Level instance;
        bool loadRequested;
        bool playsSequence;
        LevelTerrain terrain = new LevelTerrain();

        /// <summary>Raised after <see cref="Load"/> or <see cref="Reload"/> put a level (or none) on the keyboard.</summary>
        public event Action LevelChanged;

        /// <summary>The level sequence a normal run plays.</summary>
        public LevelSequence Sequence => sequence;

        /// <summary>The level prefab being played, or null.</summary>
        public Level Source => source;

        /// <summary>The instance of <see cref="Source"/> under the keyboard, or null.</summary>
        public Level Current => instance;

        /// <summary>The current level's terrain, in board space. Empty when no level is loaded.</summary>
        public LevelTerrain Terrain => terrain;

        void Awake() => view = GetComponent<KeyboardGridView>();

        void Start()
        {
            if (loadRequested || !sequence) return;
            var first = sequence.First;
            if (first)
            {
                Show(first);
                playsSequence = true;
            }
            else Debug.LogWarning($"Level sequence '{sequence.name}' has no levels: nothing to play.", this);
        }

        /// <summary>Plays <paramref name="level"/> (a level prefab; null for none) and tells the views to redraw.</summary>
        public void Load(Level level)
        {
            playsSequence = false;
            Show(level);
        }

        /// <summary>Plays the next level of the sequence (the first again after the last one). A level handed to <see cref="Load"/> is replayed instead.</summary>
        public void Advance() => Show(playsSequence && sequence ? sequence.AfterWrapping(source) : source);

        void Show(Level level)
        {
            loadRequested = true;
            source = level;
            Reload();
        }

        /// <summary>Rebuilds the current level from its prefab (after it was edited), then tells the views to redraw. The keyboard is not rebuilt.</summary>
        public void Reload()
        {
            if (instance) DestroyObject(instance.gameObject);
            instance = null;
            terrain = new LevelTerrain();
            if (source)
            {
                instance = Instantiate(source, transform, false);
                instance.name = source.name;
                ShowAuthoringRenderers(instance, showAuthoringRenderers);
                terrain = instance.BuildTerrain() ?? new LevelTerrain();
            }
            LevelChanged?.Invoke();
        }

        /// <summary>The tile under the center of a key. Empty when nothing is painted there, no level is loaded, or the layout has no such key.</summary>
        public MapTile GetTile(Key key) => TryGetBoardPosition(key, out var board) ? terrain.GetTile(board) : default;

        /// <summary>The tile at a world position. Empty when nothing is painted there or no level is loaded.</summary>
        public MapTile GetTile(Vector2 world) => terrain.GetTile((Vector2)transform.InverseTransformPoint(world));

        /// <summary>
        /// World positions from <paramref name="start"/> to <paramref name="goal"/>, both included, empty when there is no way.
        /// Walkable tiles only, unless <paramref name="passable"/> says otherwise. Allocates: call it when a path is needed, not every frame.
        /// </summary>
        public IReadOnlyList<Vector2> GetPath(Vector2 start, Vector2 goal, Func<MapTile, bool> passable = null)
        {
            var path = terrain.FindPath(transform.InverseTransformPoint(start), transform.InverseTransformPoint(goal), passable);
            var world = new Vector2[path.Count];
            for (int i = 0; i < world.Length; i++) world[i] = transform.TransformPoint(path[i]);
            return world;
        }

        bool TryGetBoardPosition(Key key, out Vector2 board)
        {
            if (view && view.Cells.TryGetValue(key, out var cell))
            {
                board = cell.Center + CenterNudge;
                return true;
            }
            board = default;
            return false;
        }

        static void ShowAuthoringRenderers(Level level, bool show)
        {
            foreach (var renderer in level.GetComponentsInChildren<TilemapRenderer>(true)) renderer.enabled = show;
            if (level.BackgroundRenderer) level.BackgroundRenderer.enabled = show;     // LevelBackgroundView draws the background, fitted to the camera
        }

        // Edit Mode tests load levels too: Destroy only works in Play Mode.
        static void DestroyObject(GameObject target)
        {
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }

        // Toggling the debug switch while playing applies at once.
        void OnValidate()
        {
            if (Application.isPlaying && instance) ShowAuthoringRenderers(instance, showAuthoringRenderers);
        }
    }
}
