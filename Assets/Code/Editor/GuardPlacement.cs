using System;
using System.Collections.Generic;
using DetectiveDave.GuardPlay;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.PlayerPlay;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    public sealed class GuardPlacement
    {
        public IReadOnlyList<GuardProblemKind> Problems { get; }

        public IReadOnlyList<Key> Route { get; }

        public bool IsFine => Problems.Count == 0;

        GuardPlacement(List<GuardProblemKind> problems, IReadOnlyList<Key> route)
        {
            Problems = problems;
            Route = route;
        }

        public static GuardPlacement Judge(Key start, Key end, Func<Key, bool> existsOnKeyboard, Func<Key, CellKind> kindOf, IKeyboardBoard board, Key playerStart)
        {
            var problems = new List<GuardProblemKind>();
            bool startUsable = JudgeEnd(start, existsOnKeyboard, kindOf, GuardProblemKind.StartNotPlaced, GuardProblemKind.StartNotOnKeyboard, GuardProblemKind.StartOnBlockedKey, problems);
            bool endUsable = JudgeEnd(end, existsOnKeyboard, kindOf, GuardProblemKind.EndNotPlaced, GuardProblemKind.EndNotOnKeyboard, GuardProblemKind.EndOnBlockedKey, problems);
            bool startOnPlayer = start != Key.None && start == playerStart;
            if (startOnPlayer) problems.Add(GuardProblemKind.StartOnPlayerShift);
            var route = Array.Empty<Key>();
            if (startUsable && endUsable && !startOnPlayer)
            {
                var pathfinder = new KeyPathfinder(board, key => key != playerStart && kindOf(key) == CellKind.Floor);
                route = Copy(pathfinder.FindPath(start, end));
                if (route.Length == 0) problems.Add(GuardProblemKind.NoPathBetweenStartAndEnd);
            }
            return new GuardPlacement(problems, route);
        }

        static bool JudgeEnd(Key key, Func<Key, bool> existsOnKeyboard, Func<Key, CellKind> kindOf,
            GuardProblemKind notPlaced, GuardProblemKind notOnKeyboard, GuardProblemKind blocked, List<GuardProblemKind> problems)
        {
            if (key == Key.None)
            {
                problems.Add(notPlaced);
                return false;
            }
            if (!existsOnKeyboard(key))
            {
                problems.Add(notOnKeyboard);
                return false;
            }
            if (kindOf(key) != CellKind.Floor)
            {
                problems.Add(blocked);
                return false;
            }
            return true;
        }

        static Key[] Copy(IReadOnlyList<Key> route)
        {
            var copy = new Key[route.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = route[i];
            return copy;
        }
    }
}
