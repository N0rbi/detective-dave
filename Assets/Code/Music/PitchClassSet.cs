using System.Collections.Generic;

namespace DetectiveDave.MusicPlay
{
    public readonly struct PitchClassSet
    {
        const int PitchClassesPerOctave = 12;

        readonly int mask;

        PitchClassSet(int mask) => this.mask = mask;

        public bool IsEmpty => mask == 0;

        public static PitchClassSet Of(IEnumerable<int> notes)
        {
            var mask = 0;
            if (notes != null)
                foreach (var note in notes) mask |= BitOf(note);
            return new PitchClassSet(mask);
        }

        public int Count => BitCount(mask);

        public IEnumerable<int> Notes
        {
            get
            {
                for (var note = 0; note < PitchClassesPerOctave; note++)
                    if ((mask & (1 << note)) != 0) yield return note;
            }
        }

        public bool Contains(int note) => (mask & BitOf(note)) != 0;

        public bool ContainsAll(PitchClassSet other) => (other.mask & ~mask) == 0;

        public int CountIn(PitchClassSet scale) => BitCount(mask & scale.mask);

        static int BitCount(int bits)
        {
            var count = 0;
            for (; bits != 0; bits &= bits - 1) count++;
            return count;
        }

        static int BitOf(int note) => 1 << (((note % PitchClassesPerOctave) + PitchClassesPerOctave) % PitchClassesPerOctave);
    }
}
