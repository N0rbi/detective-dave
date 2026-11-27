using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DeskTopChoiceTests
    {
        [Test]
        public void An_empty_desk_top_is_replaced()
        {
            Assert.IsTrue(DeskTopChoice.ShouldReplace(null));
            Assert.IsTrue(DeskTopChoice.ShouldReplace(""));
        }

        [Test]
        public void The_placeholder_desk_top_is_replaced()
        {
            Assert.IsTrue(DeskTopChoice.ShouldReplace("placeholder_desk_top"));
            Assert.IsTrue(DeskTopChoice.ShouldReplace("placeholder_desk_top_0"));
        }

        [Test]
        public void A_real_picture_stays()
        {
            Assert.IsFalse(DeskTopChoice.ShouldReplace("keyboard_stealth_board"));
            Assert.IsFalse(DeskTopChoice.ShouldReplace("my_own_desk"));
        }
    }
}
