using DetectiveDave.Levels.Editor;
using NUnit.Framework;

namespace DetectiveDave.Levels.Tests
{
    public class BackgroundNamesTests
    {
        [Test]
        public void AFileNamedPlaceholderIsAPlaceholder()
        {
            Assert.IsTrue(BackgroundNames.IsPlaceholder("placeholder_background"));
        }

        [Test]
        public void TheCheckIgnoresTheCase()
        {
            Assert.IsTrue(BackgroundNames.IsPlaceholder("Placeholder_Sky"));
        }

        [Test]
        public void ARealBackgroundIsNotAPlaceholder()
        {
            Assert.IsFalse(BackgroundNames.IsPlaceholder("new_york_skyline_moody"));
        }

        [Test]
        public void AnEmptyNameIsNotAPlaceholder()
        {
            Assert.IsFalse(BackgroundNames.IsPlaceholder(""));
        }
    }
}
