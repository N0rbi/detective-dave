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
            readonly Queue<int> indexes;

            public FixedRandom(params int[] indexes) => this.indexes = new Queue<int>(indexes);

            public float Range(float min, float max) => (min + max) / 2f;

            public int Index(int count) => indexes.Dequeue();
        }

        sealed class RecordingVoices : IPianoVoices
        {
            public readonly List<(int Clip, float Volume)> Played = new List<(int, float)>();

            public void Play(int clipIndex, float volume) => Played.Add((clipIndex, volume));
        }

        RecordingVoices voices;
        float now;

        [SetUp]
        public void SetUp()
        {
            voices = new RecordingVoices();
            now = 0f;
        }

        PianoReaction Build(int clipCount, float minSecondsBetween, params int[] indexes)
        {
            var random = new FixedRandom(indexes);
            return new PianoReaction(new PianoPicker(random, clipCount), new PianoNoteVolume(random, BaseVolume, 0.2f, BlockedShare), new ClickGate(minSecondsBetween), voices, () => now);
        }

        [Test]
        public void A_move_plays_the_picked_clip_at_full_volume()
        {
            var reaction = Build(3, 0f, 2);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(1, voices.Played.Count);
            Assert.AreEqual(2, voices.Played[0].Clip);
            Assert.AreEqual(BaseVolume, voices.Played[0].Volume, 0.0001f);
        }

        [Test]
        public void A_blocked_move_plays_at_half_volume()
        {
            var reaction = Build(3, 0f, 1);
            reaction.OnMoveAttempted(MoveAttempt.Blocked);
            Assert.AreEqual(BaseVolume * BlockedShare, voices.Played[0].Volume, 0.0001f);
        }

        [Test]
        public void Without_clips_nothing_plays()
        {
            var reaction = Build(0, 0f);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.IsEmpty(voices.Played);
        }

        [Test]
        public void Notes_closer_than_the_minimum_time_are_skipped()
        {
            var reaction = Build(3, 0.5f, 0, 0);
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
            var reaction = Build(3, 0.5f, 1, 0);
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
