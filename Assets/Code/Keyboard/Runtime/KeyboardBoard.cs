using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// The scene's <see cref="IKeyboardBoard"/>: answers neighbour and position questions about the locked layout,
    /// and starts the <see cref="KeyboardKeySource"/> on the same object with that layout. The source turns the keyboard into key events;
    /// listen to it for presses. The board holds no game state.
    /// </summary>
    [DefaultExecutionOrder(KeyboardKeySource.ExecutionOrder)]    // ready before any other Start asks for neighbours
    [RequireComponent(typeof(KeyboardGridView), typeof(KeyboardKeySource))]
    public class KeyboardBoard : MonoBehaviour, IKeyboardBoard
    {
        // A key center usually lies exactly on a tile border (a key is 4 x 4 tiles). Nudging the point keeps every answer
        // stable: a key stands, and reads its tile, just above and right of its center.
        static readonly Vector2 CenterNudge = new Vector2(0.01f, 0.01f);

        KeyboardGridView view;
        KeyboardKeySource source;
        KeyGraph graph;

        void Awake()
        {
            view = GetComponent<KeyboardGridView>();
            source = GetComponent<KeyboardKeySource>();
        }

        void Start()
        {
            if (view.Layout == null)
            {
                enabled = false;
                return;
            }
            graph = new KeyGraph(view.Layout);
            source.Begin(view.Layout);
        }

        public float KeyWorldSize => view.Scale * view.transform.lossyScale.x;

        public bool IsNeighbour(Key key, Key other) => graph != null && graph.AreNeighbours(key, other);

        public IReadOnlyCollection<Key> GetNeighbours(Key key) => graph != null ? graph.Of(key) : Array.Empty<Key>();

        public bool TryGetPosition(Key key, out Vector2 world)
        {
            bool found = view.TryGetWorldCenter(key, out world);
            world += CenterNudge;
            return found;
        }
    }
}
