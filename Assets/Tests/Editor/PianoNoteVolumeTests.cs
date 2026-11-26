using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PianoNoteVolumeTests
    {
        sealed class EdgeRandom : IRandomSource
        {
            readonly bool upper;

            public EdgeRandom(bool upper) => this.upper = upper;

            public float Range(float min, float max) => upper ? max : min;

            public int Index(int count) => 0;
        }

        [Test]
        public void A_move_uses_the_base_volume()
        {
            var volume = new PianoNoteVolume(new EdgeRandom(true), 0.8f, 0f, 0.5f);
            Assert.AreEqual(0.8f, volume.For(MoveAttempt.Moved), 0.0001f);
        }

        [Test]
        public void A_blocked_move_uses_the_blocked_share()
        {
            var volume = new PianoNoteVolume(new EdgeRandom(true), 0.8f, 0f, 0.5f);
            Assert.AreEqual(0.4f, volume.For(MoveAttempt.Blocked), 0.0001f);
        }

        [Test]
        public void Variance_moves_the_volume_both_ways()
        {
            var low = new PianoNoteVolume(new EdgeRandom(false), 0.5f, 0.2f, 0.5f);
            var high = new PianoNoteVolume(new EdgeRandom(true), 0.5f, 0.2f, 0.5f);
            Assert.AreEqual(0.4f, low.For(MoveAttempt.Moved), 0.0001f);
            Assert.AreEqual(0.6f, high.For(MoveAttempt.Moved), 0.0001f);
        }

        [Test]
        public void Volume_never_goes_above_one()
        {
            var volume = new PianoNoteVolume(new EdgeRandom(true), 1f, 0.5f, 1f);
            Assert.AreEqual(1f, volume.For(MoveAttempt.Moved), 0.0001f);
        }

        [Test]
        public void A_blocked_move_with_variance_stays_half_of_a_moved_one()
        {
            var moved = new PianoNoteVolume(new EdgeRandom(true), 0.6f, 0.1f, 0.5f);
            var blocked = new PianoNoteVolume(new EdgeRandom(true), 0.6f, 0.1f, 0.5f);
            Assert.AreEqual(moved.For(MoveAttempt.Moved) * 0.5f, blocked.For(MoveAttempt.Blocked), 0.0001f);
        }
    
        [Test]
        public void A_wrong_key_uses_the_misclick_share()
        {
            var volume = new PianoNoteVolume(new EdgeRandom(false), 0.8f, 0f, 0.5f, 0.25f);
            Assert.AreEqual(0.2f, volume.For(MoveAttempt.WrongKey), 0.0001f);
            Assert.AreEqual(0.4f, volume.For(MoveAttempt.Blocked), 0.0001f);
        }
}
}
