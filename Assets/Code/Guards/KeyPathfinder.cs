using System;
using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardGrid;
using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    public sealed class KeyPathfinder
    {
        static readonly IComparer<Key> ByKeyOrder = Comparer<Key>.Create((a, b) => ((int)a).CompareTo((int)b));

        readonly IKeyboardBoard board;
        readonly Func<Key, bool> passable;

        public KeyPathfinder(IKeyboardBoard board, Func<Key, bool> passable)
        {
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.passable = passable ?? throw new ArgumentNullException(nameof(passable));
        }

        public IReadOnlyList<Key> FindPath(Key from, Key to)
        {
            return PathSearch.Find(from, to, InKeyOrder, passable, NoEstimate, ByKeyOrder);
        }

        public bool HasPath(Key from, Key to) => FindPath(from, to).Count > 0;

        IEnumerable<Key> InKeyOrder(Key key) => board.GetNeighbours(key).OrderBy(neighbour => (int)neighbour);

        static float NoEstimate(Key key) => 0f;
    }
}
