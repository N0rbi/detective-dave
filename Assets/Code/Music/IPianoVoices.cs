namespace DetectiveDave.MusicPlay
{
    public interface IPianoVoices
    {
        void Play(NoteKind kind, int clipIndex, float volume);
    }
}
