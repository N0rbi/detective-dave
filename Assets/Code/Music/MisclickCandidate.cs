namespace DetectiveDave.MusicPlay
{
    public sealed class MisclickCandidate
    {
        public MisclickCandidate(NoteKind kind, int clipIndex, string name, PitchClassSet pitches)
        {
            Kind = kind;
            ClipIndex = clipIndex;
            Name = name;
            Pitches = pitches;
        }

        public NoteKind Kind { get; }

        public int ClipIndex { get; }

        public string Name { get; }

        public PitchClassSet Pitches { get; }
    }
}
