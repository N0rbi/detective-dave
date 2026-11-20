using System.Collections.Generic;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// The keyboard as a game board: which keys touch, and where a key is in the world. A key is the Input System <see cref="Key"/> (its physical position),
    /// and for the player a key is a tile. Everything here is about the locked layout.
    /// Key events are not here: listen to the <see cref="IKeyEventSource"/> (the <see cref="KeyboardKeySource"/> on the same object), which only produces events.
    /// What is ON a key (terrain, paths) is level code, not part of the board.
    /// </summary>
    public interface IKeyboardBoard
    {
        /// <summary>Do the two keys touch? Directions and the key you stand on do not matter. False for keys the layout does not have.</summary>
        bool IsNeighbour(Key key, Key other);

        /// <summary>All keys touching this one: at most 6, fewer on the edges.</summary>
        IReadOnlyCollection<Key> GetNeighbours(Key key);

        /// <summary>Where the player stands on a key, in world space (a hair above-right of the center, so it never sits on a tile border). False for a key the layout does not have.</summary>
        bool TryGetPosition(Key key, out Vector2 world);

        /// <summary>World units of one key at the current screen fit.</summary>
        float KeyWorldSize { get; }
    }
}
