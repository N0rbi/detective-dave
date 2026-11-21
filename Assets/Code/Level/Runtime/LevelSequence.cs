using System.Collections.Generic;
using DetectiveDave.Levels;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// The levels of the game, in the order they are played. Edited in Window > Detective Dave > Levels (drag to reorder).
    /// The game finds it through the <see cref="LevelLoader"/> in the game scene, so it ships with the build.
    /// </summary>
    [CreateAssetMenu(menuName = "Detective Dave/Level Sequence", fileName = "Level Sequence.config.asset", order = 2)]
    public sealed class LevelSequence : ScriptableObject
    {
        [Tooltip("Level prefabs in play order. Empty entries are skipped.")]
        [SerializeField] List<Level> levels = new List<Level>();

        /// <summary>The entries in play order, empty ones included.</summary>
        public IReadOnlyList<Level> Levels => levels;

        /// <summary>The first level that is set, or null when there is none.</summary>
        public Level First => Next(-1);

        /// <summary>The position of <paramref name="level"/> in the sequence, or -1.</summary>
        public int IndexOf(Level level) => level ? levels.IndexOf(level) : -1;

        /// <summary>The first set level after position <paramref name="index"/>, or null at the end.</summary>
        public Level Next(int index)
        {
            for (int i = Mathf.Max(index + 1, 0); i < levels.Count; i++)
                if (levels[i]) return levels[i];
            return null;
        }

        /// <summary>The level after <paramref name="level"/> in play order, or null when it is the last one or not in the sequence.</summary>
        public Level After(Level level)
        {
            int index = IndexOf(level);
            return index < 0 ? null : Next(index);
        }

        /// <summary>The level after <paramref name="level"/>, or the first level again after the last one (or when it is not in the sequence).</summary>
        public Level AfterWrapping(Level level) => After(level) ?? First;
    }
}
