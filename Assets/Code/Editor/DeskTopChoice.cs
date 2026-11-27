namespace DetectiveDave.EditorTools
{
    public static class DeskTopChoice
    {
        const string PlaceholderPrefix = "placeholder_";

        public static bool ShouldReplace(string currentSpriteName) =>
            string.IsNullOrEmpty(currentSpriteName) || currentSpriteName.StartsWith(PlaceholderPrefix);
    }
}
