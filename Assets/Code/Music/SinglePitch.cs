using System.Text.RegularExpressions;

namespace DetectiveDave.MusicPlay
{
    public static class SinglePitch
    {
        public const string FilePrefix = "Piano Single";

        const int PitchClassesPerOctave = 12;
        const int HighestMidiPitch = 127;
        const int OctaveOfMidiZero = -1;

        static readonly Regex NamePattern = new Regex(@"^\s*piano\s+single\s*-\s*([a-g])(s?)(-?[0-9])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        static readonly int[] PitchClassOfLetter = { 9, 11, 0, 2, 4, 5, 7 };

        public static bool TryParse(string fileName, out int midiPitch, out string problem)
        {
            midiPitch = 0;
            var match = NamePattern.Match(fileName ?? string.Empty);
            if (!match.Success)
            {
                problem = $"\"{fileName}\" does not look like \"Piano Single - <Note><Octave>\", for example \"Piano Single - A3\" or \"Piano Single - Gs3\" (s is the sharp, flats are not used).";
                return false;
            }
            var letter = char.ToUpperInvariant(match.Groups[1].Value[0]) - 'A';
            var sharp = match.Groups[2].Length;
            var octave = int.Parse(match.Groups[3].Value);
            midiPitch = (octave - OctaveOfMidiZero) * PitchClassesPerOctave + PitchClassOfLetter[letter] + sharp;
            if (midiPitch >= 0 && midiPitch <= HighestMidiPitch)
            {
                problem = null;
                return true;
            }
            problem = $"\"{fileName}\" is outside the piano range: the octave must be -1 to 9 and the highest note is G9.";
            return false;
        }

        public static int PitchClassOf(int midiPitch) => ((midiPitch % PitchClassesPerOctave) + PitchClassesPerOctave) % PitchClassesPerOctave;

        public static string DisplayName(int midiPitch) => NoteNames[PitchClassOf(midiPitch)] + OctaveOf(midiPitch);

        public static string FileNameOf(int midiPitch) => DisplayName(midiPitch).Replace('#', 's');

        static int OctaveOf(int midiPitch) => (int)System.Math.Floor(midiPitch / (double)PitchClassesPerOctave) + OctaveOfMidiZero;
    }
}
