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

        sealed class FixedTiming : INoteTiming
        {
            public bool Plays = true;
            public double Delay;
            public ChordMoment Moment = new ChordMoment(2, 1, false, false);

            public bool TryPlan(out double delaySeconds, out ChordMoment moment)
            {
                delaySeconds = Delay;
                moment = Moment;
                return Plays;
            }
        }

        sealed class RecordingVoices : IPianoVoices
        {
            public readonly List<(int Clip, float Volume, double Delay)> Played = new List<(int, float, double)>();

            public void Play(int clipIndex, float volume, double delaySeconds) => Played.Add((clipIndex, volume, delaySeconds));
        }

        RecordingVoices voices;
        FixedTiming timing;
        float now;

        [SetUp]
        public void SetUp()
        {
            voices = new RecordingVoices();
            timing = new FixedTiming();
            now = 0f;
        }

        PianoReaction Build(IPianoPicker picker, float minSecondsBetween = 0f) =>
            new PianoReaction(picker, new PianoNoteVolume(new FixedRandom(), BaseVolume, 0.2f, BlockedShare), new ClickGate(minSecondsBetween), voices, () => now, timing);

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
        public void The_delay_of_the_timing_is_passed_to_the_voice()
        {
            timing.Delay = 0.35;
            Build(new ScriptedPicker(0)).OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(0.35, voices.Played[0].Delay, 1e-9);
        }

        [Test]
        public void The_picker_gets_the_moment_of_the_timing()
        {
            var picker = new ScriptedPicker(0);
            Build(picker).OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(2, picker.LastMoment.SegmentIndex);
            Assert.AreEqual(1, picker.LastMoment.BeatInBar);
        }

        [Test]
        public void When_the_timing_drops_the_note_the_picker_is_not_asked()
        {
            timing.Plays = false;
            var picker = new ScriptedPicker(0);
            Build(picker).OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(0, picker.Calls);
            Assert.IsEmpty(voices.Played);
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
