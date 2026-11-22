using System.Linq;
using DetectiveDave.MusicPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class SingleNoteOrderTests
    {
        AudioClip first;
        AudioClip second;
        AudioClip third;

        [SetUp]
        public void SetUp()
        {
            first = NewClip("Piano Single - A3");
            second = NewClip("Piano Single - C4");
            third = NewClip("Piano Single - C4 copy");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(third);
        }

        static AudioClip NewClip(string name) => AudioClip.Create(name, 1, 1, 44100, false);

        [Test]
        public void Clips_are_sorted_by_pitch()
        {
            var sorted = SingleNoteOrder.Sorted(new[] { new SingleNoteClip(second, 60), new SingleNoteClip(first, 57) });
            CollectionAssert.AreEqual(new[] { first, second }, sorted.Select(single => single.Clip));
        }

        [Test]
        public void Clips_with_the_same_pitch_are_sorted_by_name()
        {
            var sorted = SingleNoteOrder.Sorted(new[] { new SingleNoteClip(third, 60), new SingleNoteClip(second, 60) });
            CollectionAssert.AreEqual(new[] { second, third }, sorted.Select(single => single.Clip));
        }

        [Test]
        public void Empty_entries_and_entries_without_a_clip_are_dropped()
        {
            var sorted = SingleNoteOrder.Sorted(new[] { null, new SingleNoteClip(null, 50), new SingleNoteClip(first, 57) });
            Assert.AreEqual(1, sorted.Count);
        }

        [Test]
        public void No_entries_gives_an_empty_list()
        {
            Assert.IsEmpty(SingleNoteOrder.Sorted(new SingleNoteClip[0]));
        }

        [Test]
        public void An_entry_knows_its_pitch_class_and_note_name()
        {
            var single = new SingleNoteClip(first, 56);
            Assert.AreEqual(8, single.PitchClass);
            Assert.AreEqual("G#3", single.NoteName);
        }
    }
}
