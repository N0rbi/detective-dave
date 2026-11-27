namespace DetectiveDave.MusicPlay
{
    public readonly struct PickedNote
    {
        public PickedNote(NoteKind kind, int clipIndex)
        {
            Kind = kind;
            ClipIndex = clipIndex;
        }

        public NoteKind Kind { get; }

        public int ClipIndex { get; }

        public bool IsSilent => ClipIndex == PianoPicker.NoClip;
    }
}
