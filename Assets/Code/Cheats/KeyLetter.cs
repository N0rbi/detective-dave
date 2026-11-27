namespace DetectiveDave.CheatPlay
{
    public static class KeyLetter
    {
        public static bool TryFrom(string displayName, out char letter)
        {
            letter = default;
            if (string.IsNullOrEmpty(displayName) || displayName.Length != 1 || !char.IsLetter(displayName[0])) return false;
            letter = char.ToLowerInvariant(displayName[0]);
            return true;
        }
    }
}
