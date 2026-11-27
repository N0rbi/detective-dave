using System.IO;
using System.Linq;
using DetectiveDave.MusicPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class ChordChartFileTests
    {
        static readonly string MusicFolder = Path.Combine(Application.dataPath, "Audio", "Music");

        static ChordChartFile Load() => ChordChartFile.Parse(File.ReadAllText(Path.Combine(MusicFolder, "chord_chart.json")));

        [Test]
        public void The_file_describes_the_75_bpm_loop_of_eight_bars()
        {
            var file = Load();
            Assert.AreEqual(75f, file.tempoBpm);
            Assert.AreEqual(4, file.beatsPerBar);
            Assert.AreEqual(8, file.loopLengthBars);
            Assert.AreEqual(25.6, file.loopLengthBars * file.beatsPerBar * 60.0 / file.tempoBpm, 1e-6);
        }

        [Test]
        public void The_chords_follow_each_other_without_gaps_and_fill_the_loop()
        {
            var segments = Load().segments;
            Assert.AreEqual(8, segments.Length);
            for (var index = 0; index < segments.Length; index++)
            {
                Assert.AreEqual(index * 4f, segments[index].StartBeat, 1e-6, segments[index].ChordName);
                Assert.AreEqual(4f, segments[index].LengthBeats, 1e-6);
                Assert.IsTrue(segments[index].ScaleTones.ContainsAll(segments[index].ChordTones), segments[index].ChordName);
            }
        }

        [Test]
        public void Every_piano_clip_file_has_notes_in_the_file()
        {
            var file = Load();
            var pianoFiles = Directory.GetFiles(Path.Combine(MusicFolder, "Piano"), "*.wav").Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
            Assert.AreEqual(pianoFiles.Count, file.clips.Length);
            foreach (var entry in file.clips)
            {
                CollectionAssert.Contains(pianoFiles, entry.clipName);
                Assert.IsNotEmpty(entry.pitchClasses, entry.clipName);
            }
        }

        [Test]
        public void Every_chord_has_at_least_one_clip_in_its_scale()
        {
            var file = Load();
            var palette = new ChordPalette(file.segments, file.clips.Select(entry => PitchClassSet.Of(entry.pitchClasses)).ToList());
            for (var index = 0; index < file.segments.Length; index++)
                Assert.GreaterOrEqual(palette.Allowed(index).Count, 1, file.segments[index].ChordName);
        }

        [Test]
        public void Every_chord_has_at_least_one_clip_made_of_its_own_notes()
        {
            var file = Load();
            var palette = new ChordPalette(file.segments, file.clips.Select(entry => PitchClassSet.Of(entry.pitchClasses)).ToList());
            for (var index = 0; index < file.segments.Length; index++)
                Assert.IsTrue(Enumerable.Range(0, file.clips.Length).Any(clip => palette.IsChordTone(index, clip)), file.segments[index].ChordName);
        }
    }
}
