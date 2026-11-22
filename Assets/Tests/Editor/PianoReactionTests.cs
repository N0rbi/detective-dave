using System.Collections.Generic;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PianoReactionTests
    {
        const float BaseVolume = 0.8f;
        const float BlockedShare = 0.5f;

        sealed class FixedRandom : IRandomSource
        {
            public float Range(float min, float max) => (min + max) / 2f;

            public int Index(int count) => 0;
        }

        sealed class ScriptedPicker : IPianoPicker
        {
            readonly Queue<int> clips;

            public ScriptedPicker(params int[] clips) => this.clips = new Queue<int>(clips);

            public int Calls { get; private set; }

            public ChordMoment LastMoment { get; private set; }

            public int Next(ChordMoment moment)
            {
                Calls++;
                LastMoment = moment;
                return clips.Count > 0 ? clips.Dequeue() : PianoPicker.NoClip;
            }
        }

        sealed class RecordingVoices : IPianoVoices
        {
            public readonly List<(int Clip, float Volume)> Played = new List<(int, float)>();

            public void Play(int clipIndex, float volume) => Played.Add((clipIndex, volume));
        }

        RecordingVoices voices;
        ChordMoment moment;
        float now;

        [SetUp]
        public void SetUp()
        {
            voices = new RecordingVoices();
            moment = new ChordMoment(2, 1, false, false);
            now = 0f;
        }

        PianoReaction Build(IPianoPicker picker, float minSecondsBetween = 0f) =>
            new PianoReaction(picker, new PianoNoteVolume(new FixedRandom(), BaseVolume, 0.2f, BlockedShare), new ClickGate(minSecondsBetween), voices, () => now, () => moment);

        [Test]
        public void A_move_plays_the_picked_clip_at_full_volume()
        {
            Build(new ScriptedPicker(2)).OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(1, voices.Played.Count);
            Assert.AreEqual(2, voices.Played[0].Clip);
            Assert.AreEqual(BaseVolume, voices.Played[0].Volume, 0.0001f);
        }

        [Test]
        public void A_blocked_move_plays_at_half_volume()
        {
            Build(new ScriptedPicker(1)).OnMoveAttempted(MoveAttempt.Blocked);
            Assert.AreEqual(BaseVolume * BlockedShare, voices.Played[0].Volume, 0.0001f);
        }

        [Test]
        public void Without_a_clip_nothing_plays()
        {
            Build(new ScriptedPicker()).OnMoveAttempted(MoveAttempt.Moved);
            Assert.IsEmpty(voices.Played);
        }

        [Test]
        public void The_picker_gets_the_chord_moment_of_the_press()
        {
            var picker = new ScriptedPicker(0, 0);
            var reaction = Build(picker);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(2, picker.LastMoment.SegmentIndex);
            Assert.AreEqual(1, picker.LastMoment.BeatInBar);
            moment = new ChordMoment(5, 3, false, false);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(5, picker.LastMoment.SegmentIndex);
        }

        [Test]
        public void Notes_closer_than_the_minimum_time_are_skipped()
        {
            var reaction = Build(new ScriptedPicker(0, 0, 0), 0.5f);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.2f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.6f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(2, voices.Played.Count);
        }

        [Test]
        public void A_skipped_note_does_not_use_up_the_pick()
        {
            var reaction = Build(new ScriptedPicker(1, 0), 0.5f);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.1f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 1f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(2, voices.Played.Count);
            Assert.AreEqual(0, voices.Played[1].Clip);
        }
    }
}
