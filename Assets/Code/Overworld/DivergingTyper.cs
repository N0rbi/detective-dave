using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DivergingTyper
    {
        const int AcceptIndex = 0;
        const int DeclineIndex = 1;

        readonly PrefixTyper typer;

        public DivergingTyper(string acceptLetter, string declineLetter, TypingRules rules)
        {
            AcceptLetter = acceptLetter ?? string.Empty;
            DeclineLetter = declineLetter ?? string.Empty;
            typer = new PrefixTyper(new[] { AcceptLetter, DeclineLetter }, rules);
        }

        public string AcceptLetter { get; }

        public string DeclineLetter { get; }

        public int TypedCount => typer.TypedCount;

        public LetterPath Path
        {
            get
            {
                bool accept = typer.IsAlive(AcceptIndex);
                bool decline = typer.IsAlive(DeclineIndex);
                if (accept && !decline) return LetterPath.Accept;
                if (decline && !accept) return LetterPath.Decline;
                return LetterPath.Undecided;
            }
        }

        public LetterOutcome Outcome
        {
            get
            {
                switch (typer.CompletedTargetIndex)
                {
                    case AcceptIndex: return LetterOutcome.Accepted;
                    case DeclineIndex: return LetterOutcome.Declined;
                    default: return LetterOutcome.None;
                }
            }
        }

        public bool TryType(char character) => typer.TryType(character);

        public bool Backspace() => typer.Backspace();

        public static int DivergenceIndex(string first, string second)
        {
            first = first ?? string.Empty;
            second = second ?? string.Empty;
            int shared = Math.Min(first.Length, second.Length);
            for (int position = 0; position < shared; position++)
            {
                if (char.ToLowerInvariant(first[position]) != char.ToLowerInvariant(second[position])) return position;
            }
            return shared;
        }

        public static bool Diverge(string accept, string decline)
        {
            if (string.IsNullOrEmpty(accept) || string.IsNullOrEmpty(decline)) return false;
            int divergence = DivergenceIndex(accept, decline);
            return divergence < accept.Length && divergence < decline.Length;
        }
    }
}
