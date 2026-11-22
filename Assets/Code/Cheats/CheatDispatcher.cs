using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.CheatPlay
{
    public sealed class CheatDispatcher
    {
        readonly CheatCodeMatcher matcher;
        readonly Dictionary<string, IGameCheat> cheatsByCode;
        readonly bool enabled;

        public CheatDispatcher(IReadOnlyList<CheatBinding> bindings, float timeoutSeconds, bool enabled)
        {
            cheatsByCode = bindings
                .Where(binding => !string.IsNullOrEmpty(binding.Code) && binding.Cheat != null)
                .GroupBy(binding => binding.Code.ToLowerInvariant())
                .ToDictionary(group => group.Key, group => group.First().Cheat);
            matcher = new CheatCodeMatcher(cheatsByCode.Keys, timeoutSeconds);
            this.enabled = enabled;
        }

        public void OnLetterTyped(char letter, float nowSeconds)
        {
            if (!enabled) return;
            var code = matcher.TypeLetter(letter, nowSeconds);
            if (code != null) cheatsByCode[code].Run();
        }

        public void OnBackspace(float nowSeconds)
        {
            if (enabled) matcher.Backspace(nowSeconds);
        }
    }
}
