using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    public sealed class KeyGraphBoard : IKeyboardBoard
    {
        readonly KeyGraph graph;

        public KeyGraphBoard(KeyboardLayout layout)
        {
            graph = new KeyGraph(layout);
        }

        public float KeyWorldSize => 1f;

        public bool IsNeighbour(Key key, Key other) => graph.AreNeighbours(key, other);

        public IReadOnlyCollection<Key> GetNeighbours(Key key) => graph.Of(key);

        public bool TryGetPosition(Key key, out Vector2 world)
        {
            world = default;
            return false;
        }
    }
}
