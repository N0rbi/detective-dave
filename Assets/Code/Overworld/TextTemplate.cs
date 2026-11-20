using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DetectiveDave.OverworldPlay
{
    public static class TextTemplate
    {
        const string ClientPlaceholder = "client";
        static readonly Regex PlaceholderPattern = new Regex(@"\{[^{}\s]+\}", RegexOptions.Compiled);

        public static string Resolve(string text, string client, IReadOnlyList<SuspectEntry> suspects)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var resolved = Replace(text, ClientPlaceholder, client);
            if (suspects == null) return resolved;
            foreach (var suspect in suspects) resolved = Replace(resolved, suspect.Key, suspect.Value);
            return resolved;
        }

        public static IReadOnlyList<string> UnresolvedPlaceholders(string resolvedText)
        {
            if (string.IsNullOrEmpty(resolvedText)) return new string[0];
            return PlaceholderPattern.Matches(resolvedText).Cast<Match>().Select(match => match.Value).Distinct().ToArray();
        }

        static string Replace(string text, string name, string value)
        {
            if (string.IsNullOrEmpty(name)) return text;
            return text.Replace("{" + name + "}", value ?? string.Empty);
        }
    }
}
