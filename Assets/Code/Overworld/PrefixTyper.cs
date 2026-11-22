using System;
using System.Collections.Generic;

namespace DetectiveDave.OverworldPlay
{
    public sealed class PrefixTyper
    {
        readonly string[] targets;
        readonly TypingRules rules;
        readonly List<char> typed = new List<char>();

        public PrefixTyper(IReadOnlyList<string> targets, TypingRules rules)
        {
            if (targets == null || targets.Count == 0) throw new ArgumentException("At least one target text is needed.", nameof(targets));
            this.targets = new string[targets.Count];
            for (int index = 0; index < targets.Count; index++) this.targets[index] = targets[index] ?? string.Empty;
            this.rules = rules;
            TypeFreeCharacters();
        }

        public int TypedCount => typed.Count;

        public int TargetCount => targets.Length;

        public int CompletedTargetIndex
        {
            get
            {
                for (int index = 0; index < targets.Length; index++)
                {
                    if (targets[index].Length == typed.Count && IsAlive(index)) return index;
                }
                return -1;
            }
        }

        public bool IsComplete => CompletedTargetIndex >= 0;

        public bool IsAlive(int targetIndex)
        {
            var target = targets[targetIndex];
            if (target.Length < typed.Count) return false;
            for (int position = 0; position < typed.Count; position++)
            {
                if (CharacterMatch.Canonical(target[position]) != typed[position]) return false;
            }
            return true;
        }

        public bool TryType(char character)
        {
            if (IsComplete) return false;
            var canonical = CharacterMatch.Canonical(character);
            if (!AnyAliveTargetExpects(canonical)) return false;
            typed.Add(canonical);
            TypeFreeCharacters();
            return true;
        }

        public bool Backspace()
        {
            if (typed.Count == 0) return false;
            int before = typed.Count;
            while (typed.Count > 0 && rules.IsAutoTyped(typed[typed.Count - 1])) RemoveLast();
            if (typed.Count > 0) RemoveLast();
            if (typed.Count == 0) TypeFreeCharacters();
            return typed.Count != before;
        }

        bool AnyAliveTargetExpects(char canonical)
        {
            for (int index = 0; index < targets.Length; index++)
            {
                if (IsAlive(index) && typed.Count < targets[index].Length && CharacterMatch.Canonical(targets[index][typed.Count]) == canonical) return true;
            }
            return false;
        }

        void TypeFreeCharacters()
        {
            while (TryGetSharedNextCharacter(out var next) && rules.IsAutoTyped(next)) typed.Add(next);
        }

        bool TryGetSharedNextCharacter(out char next)
        {
            next = default;
            bool found = false;
            for (int index = 0; index < targets.Length; index++)
            {
                if (!IsAlive(index)) continue;
                if (typed.Count >= targets[index].Length) return false;
                var candidate = CharacterMatch.Canonical(targets[index][typed.Count]);
                if (found && candidate != next) return false;
                next = candidate;
                found = true;
            }
            return found;
        }

        void RemoveLast() => typed.RemoveAt(typed.Count - 1);
    }
}
