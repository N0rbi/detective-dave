using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// One finding of the level checks, worded for designers: what is wrong and what to do about it, with an optional
    /// one-click fix. Shown in the Level and Key Art inspectors (and in any tool that calls <see cref="LevelValidator"/>).
    /// </summary>
    public sealed class LevelIssue
    {
        /// <summary>How bad it is: Error (the level will not play right), Warning (it plays, but something is missing), Info.</summary>
        public MessageType Severity { get; }

        /// <summary>What is wrong and what to do, in one or two sentences.</summary>
        public string Message { get; }

        /// <summary>The object the issue is about (for pinging/selecting), or null.</summary>
        public Object Context { get; }

        /// <summary>The label of the fix button, or null when there is no fix.</summary>
        public string FixLabel { get; }

        /// <summary>The fix, or null. Fixes record Undo where they change objects.</summary>
        public Action Fix { get; }

        /// <summary>An issue; pass <paramref name="fixLabel"/> and <paramref name="fix"/> together, or neither.</summary>
        public LevelIssue(MessageType severity, string message, Object context = null, string fixLabel = null, Action fix = null)
        {
            Severity = severity;
            Message = message ?? string.Empty;
            Context = context;
            FixLabel = fix != null ? fixLabel ?? "Fix" : null;
            Fix = fix;
        }
    }

    /// <summary>
    /// A check on a <see cref="Level"/>. Every concrete class implementing this (with a parameterless constructor), in any editor
    /// assembly, is found through <see cref="TypeCache"/> and run by <see cref="LevelValidator.Validate(Level)"/>: game code adds
    /// its own checks (for example keyboard-layout checks) without registering anything. A check that throws is reported as an
    /// error issue and does not stop the others.
    /// </summary>
    public interface ILevelCheck
    {
        /// <summary>Adds the issues this check finds on <paramref name="level"/> to <paramref name="issues"/>.</summary>
        void Check(Level level, System.Collections.Generic.List<LevelIssue> issues);
    }
}
