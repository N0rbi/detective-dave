namespace DetectiveDave.LevelPlay
{
    public static class KeyboardBackdropRule
    {
        public static bool IsVisible(bool hideOverBackground, bool levelShowsBackground) => !(hideOverBackground && levelShowsBackground);
    }
}
