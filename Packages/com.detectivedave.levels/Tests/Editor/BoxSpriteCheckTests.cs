using NUnit.Framework;
using UnityEngine;
using DetectiveDave.Levels.Editor;

namespace DetectiveDave.Levels.Tests
{
    public class BoxSpriteCheckTests
    {
        [Test]
        public void ASlicedFullRectSpriteHasNoProblem() =>
            Assert.AreEqual(BoxSpriteProblem.None, BoxSpriteCheck.Classify(new Vector4(8f, 8f, 8f, 8f), SpriteMeshType.FullRect));

        [Test]
        public void AZeroBorderIsReported() =>
            Assert.AreEqual(BoxSpriteProblem.NoBorder, BoxSpriteCheck.Classify(Vector4.zero, SpriteMeshType.FullRect));

        [Test]
        public void ATightMeshIsReported() =>
            Assert.AreEqual(BoxSpriteProblem.NotFullRect, BoxSpriteCheck.Classify(new Vector4(8f, 8f, 8f, 8f), SpriteMeshType.Tight));

        [Test]
        public void BothProblemsAreReportedTogether() =>
            Assert.AreEqual(BoxSpriteProblem.NoBorder | BoxSpriteProblem.NotFullRect, BoxSpriteCheck.Classify(Vector4.zero, SpriteMeshType.Tight));

        [Test]
        public void ABorderOnOneSideCounts() =>
            Assert.IsTrue(BoxSpriteCheck.HasBorder(new Vector4(0f, 0f, 0f, 2f)));

        [TestCase(24, 24, 8)]
        [TestCase(24, 30, 8)]
        [TestCase(2, 2, 1)]
        [TestCase(0, 0, 1)]
        public void SuggestedBorderIsAThirdOfTheSmallerSide(int width, int height, int expected) =>
            Assert.AreEqual(expected, BoxSpriteCheck.SuggestedBorderPixels(width, height));

        [Test]
        public void SettingTheSameBorderTwiceChangesNothing() =>
            Assert.IsFalse(BoxSpriteCheck.NeedsChange(new Vector4(8f, 8f, 8f, 8f), SpriteMeshType.FullRect, 8));

        [Test]
        public void ADifferentBorderNeedsAChange() =>
            Assert.IsTrue(BoxSpriteCheck.NeedsChange(new Vector4(4f, 4f, 4f, 4f), SpriteMeshType.FullRect, 8));

        [Test]
        public void ATightMeshNeedsAChangeEvenWithTheRightBorder() =>
            Assert.IsTrue(BoxSpriteCheck.NeedsChange(new Vector4(8f, 8f, 8f, 8f), SpriteMeshType.Tight, 8));

        [Test]
        public void ProblemTextNamesEachProblem()
        {
            Assert.AreEqual(string.Empty, BoxSpriteCheck.Describe(BoxSpriteProblem.None));
            StringAssert.Contains("9-slice border", BoxSpriteCheck.Describe(BoxSpriteProblem.NoBorder));
            StringAssert.Contains("Full Rect", BoxSpriteCheck.Describe(BoxSpriteProblem.NotFullRect));
        }

        [Test]
        public void ASpriteThatIsNotAnImportedFileIsOnlyJudgedByItsBorder()
        {
            using var make = new LevelTestUtility();
            Assert.AreEqual(BoxSpriteProblem.None, BoxSpriteCheck.ProblemOf(make.SpriteWithBorder("box", 8f)));
            Assert.AreEqual(BoxSpriteProblem.NoBorder, BoxSpriteCheck.ProblemOf(make.SpriteWithBorder("flat", 0f)));
        }
    }
}
