using DetectiveDave.LevelPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class KeyboardBackdropRuleTests
    {
        [Test]
        public void TheBackdropIsHiddenWhenALevelBackgroundShows()
        {
            Assert.IsFalse(KeyboardBackdropRule.IsVisible(true, true));
        }

        [Test]
        public void TheBackdropStaysWhenTheLevelHasNoBackground()
        {
            Assert.IsTrue(KeyboardBackdropRule.IsVisible(true, false));
        }

        [Test]
        public void TheBackdropStaysWhenHidingIsTurnedOff()
        {
            Assert.IsTrue(KeyboardBackdropRule.IsVisible(false, true));
        }

        [Test]
        public void TheBackdropStaysWithNothingToShowAndNoHiding()
        {
            Assert.IsTrue(KeyboardBackdropRule.IsVisible(false, false));
        }
    }
}
