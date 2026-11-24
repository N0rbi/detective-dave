using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class KeyArtSetupTests
    {
        [Test]
        public void The_rim_of_the_painted_box_is_a_share_of_its_short_side()
        {
            Assert.AreEqual(65, KeyArtBorder.RimPixels(872, 872));
        }

        [Test]
        public void The_rim_follows_the_shorter_side_of_a_tall_sprite()
        {
            Assert.AreEqual(49, KeyArtBorder.RimPixels(654, 1744));
        }

        [Test]
        public void The_rim_is_never_less_than_one_pixel()
        {
            Assert.AreEqual(1, KeyArtBorder.RimPixels(4, 4));
        }

        [Test]
        public void A_file_only_in_staging_needs_to_move()
        {
            Assert.AreEqual(KeyArtSlotState.NeedsMove, KeyArtSlot.Classify(true, false));
        }

        [Test]
        public void A_file_only_in_the_target_folder_is_already_in_place()
        {
            Assert.AreEqual(KeyArtSlotState.InPlace, KeyArtSlot.Classify(false, true));
        }

        [Test]
        public void A_file_nowhere_is_missing()
        {
            Assert.AreEqual(KeyArtSlotState.Missing, KeyArtSlot.Classify(false, false));
        }

        [Test]
        public void A_file_in_both_places_is_a_conflict()
        {
            Assert.AreEqual(KeyArtSlotState.Conflict, KeyArtSlot.Classify(true, true));
        }
    }
}
