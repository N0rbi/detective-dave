using System.Collections.Generic;
using System.Linq;
using DetectiveDave.GuardPlay;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public sealed class GuardLayoutChecks : ILevelCheck
    {
        public void Check(Level level, List<LevelIssue> issues)
        {
            if (!level || !level.Shape) return;
            foreach (var patrol in level.GetComponentsInChildren<GuardPatrol>(true))
                foreach (var message in Describe(level, patrol))
                    issues.Add(new LevelIssue(MessageType.Warning, message, level));
        }

        public static List<string> Describe(Level level, GuardPatrol patrol)
        {
            var messages = new List<string>();
            var layoutsByText = new Dictionary<string, List<string>>();
            foreach (var layout in KeyboardLayouts.All)
            {
                var placement = GuardPlacements.Judge(level, patrol, layout);
                if (placement.IsFine) continue;
                var text = string.Join(" ", placement.Problems.Select(kind => GuardProblemText.Of(kind, patrol.StartKey, patrol.EndKey)));
                if (!layoutsByText.TryGetValue(text, out var names)) layoutsByText[text] = names = new List<string>();
                names.Add(layout.Name);
            }
            foreach (var entry in layoutsByText) messages.Add($"{patrol.name} ({string.Join(", ", entry.Value)}): {entry.Key}");
            return messages;
        }
    }
}
