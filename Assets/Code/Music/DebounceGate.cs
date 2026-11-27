using System;

namespace DetectiveDave.MusicPlay
{
    public sealed class DebounceGate
    {
        readonly float debounceSeconds;
        readonly bool singlesExtendDebounce;
        float lastNoteSeconds = float.NegativeInfinity;

        public DebounceGate(float debounceSeconds, bool singlesExtendDebounce)
        {
            this.debounceSeconds = Math.Max(debounceSeconds, 0f);
            this.singlesExtendDebounce = singlesExtendDebounce;
        }

        public NoteKind KindAt(float nowSeconds) =>
            nowSeconds - lastNoteSeconds >= debounceSeconds ? NoteKind.Chord : NoteKind.Single;

        public void NotePlayed(NoteKind kind, float nowSeconds)
        {
            if (kind == NoteKind.Chord || singlesExtendDebounce) lastNoteSeconds = nowSeconds;
        }
    }
}
