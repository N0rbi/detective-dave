namespace DetectiveDave.CheatPlay
{
    public readonly struct CheatBinding
    {
        public CheatBinding(string code, IGameCheat cheat)
        {
            Code = code;
            Cheat = cheat;
        }

        public string Code { get; }

        public IGameCheat Cheat { get; }
    }
}
