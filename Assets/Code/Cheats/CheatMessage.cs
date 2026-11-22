namespace DetectiveDave.CheatPlay
{
    public sealed class CheatMessage
    {
        string text;
        float hideAtSeconds;

        public void Show(string shownText, float nowSeconds, float durationSeconds)
        {
            text = shownText;
            hideAtSeconds = nowSeconds + durationSeconds;
        }

        public bool TryGet(float nowSeconds, out string shownText)
        {
            shownText = text;
            return !string.IsNullOrEmpty(text) && nowSeconds < hideAtSeconds;
        }
    }
}
