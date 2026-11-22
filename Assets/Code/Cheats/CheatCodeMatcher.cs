using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DetectiveDave.CheatPlay
{
    public sealed class CheatCodeMatcher
    {
        readonly string[] codes;
        readonly float timeoutSeconds;
        readonly int bufferLength;
        readonly StringBuilder buffer = new StringBuilder();
        float lastKeySeconds;

        public CheatCodeMatcher(IEnumerable<string> codes, float timeoutSeconds)
        {
            this.codes = codes
                .Where(code => !string.IsNullOrEmpty(code))
                .Select(code => code.ToLowerInvariant())
                .Distinct()
                .OrderByDescending(code => code.Length)
                .ToArray();
            this.timeoutSeconds = timeoutSeconds;
            bufferLength = this.codes.Length == 0 ? 0 : this.codes[0].Length;
        }

        public string TypedSoFar => buffer.ToString();

        public string TypeLetter(char letter, float nowSeconds)
        {
            ForgetWhenTooSlow(nowSeconds);
            if (bufferLength == 0) return null;
            buffer.Append(char.ToLowerInvariant(letter));
            if (buffer.Length > bufferLength) buffer.Remove(0, buffer.Length - bufferLength);
            var matched = codes.FirstOrDefault(code => EndsWithCode(code));
            if (matched != null) buffer.Clear();
            return matched;
        }

        public void Backspace(float nowSeconds)
        {
            ForgetWhenTooSlow(nowSeconds);
            if (buffer.Length > 0) buffer.Remove(buffer.Length - 1, 1);
        }

        void ForgetWhenTooSlow(float nowSeconds)
        {
            if (buffer.Length > 0 && nowSeconds - lastKeySeconds > timeoutSeconds) buffer.Clear();
            lastKeySeconds = nowSeconds;
        }

        bool EndsWithCode(string code)
        {
            if (buffer.Length < code.Length) return false;
            return buffer.ToString(buffer.Length - code.Length, code.Length).Equals(code, StringComparison.Ordinal);
        }
    }
}
