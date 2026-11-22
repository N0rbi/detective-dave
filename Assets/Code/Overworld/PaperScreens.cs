using System;

namespace DetectiveDave.OverworldPlay
{
    public enum PaperScreen
    {
        Letter,
        Report,
        Review,
        Notice
    }

    public readonly struct ScreenRoles
    {
        public ScreenRoles(TextRole leftSettled, TextRole leftRevealed, TextRole rightPending, TextRole rightTyped)
        {
            LeftSettled = leftSettled;
            LeftRevealed = leftRevealed;
            RightPending = rightPending;
            RightTyped = rightTyped;
        }

        public TextRole LeftSettled { get; }

        public TextRole LeftRevealed { get; }

        public TextRole RightPending { get; }

        public TextRole RightTyped { get; }
    }

    public static class PaperScreens
    {
        public static ScreenRoles RolesOf(PaperScreen screen)
        {
            switch (screen)
            {
                case PaperScreen.Letter:
                    return new ScreenRoles(TextRole.ClientMessage, TextRole.ClientMessage, TextRole.Placeholder, TextRole.Typed);
                case PaperScreen.Report:
                    return new ScreenRoles(TextRole.ClientMessage, TextRole.Findings, TextRole.Placeholder, TextRole.Typed);
                case PaperScreen.Review:
                    return new ScreenRoles(TextRole.ClientMessage, TextRole.Findings, TextRole.Typed, TextRole.Typed);
                case PaperScreen.Notice:
                    return new ScreenRoles(TextRole.ClientMessage, TextRole.ClientMessage, TextRole.Notice, TextRole.Notice);
                default:
                    throw new ArgumentOutOfRangeException(nameof(screen), screen, "No roles for this screen");
            }
        }
    }
}
