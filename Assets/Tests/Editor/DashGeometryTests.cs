using System.Linq;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public sealed class DashGeometryTests
    {
        static readonly Vector2[] Straight = { new Vector2(0f, 0f), new Vector2(10f, 0f) };

        [Test]
        public void DashesAndGapsAlternateAlongALine()
        {
            var dashes = DashGeometry.Dashes(Straight, 2f, 1f, 0f);
            Assert.That(dashes.Count, Is.EqualTo(4));
            Assert.That(dashes[0].First(), Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(dashes[0].Last(), Is.EqualTo(new Vector2(2f, 0f)));
            Assert.That(dashes[1].First(), Is.EqualTo(new Vector2(3f, 0f)));
        }

        [Test]
        public void TheLastDashIsCutAtTheEndOfTheLine()
        {
            var dashes = DashGeometry.Dashes(Straight, 2f, 1f, 0f);
            Assert.That(dashes.Last().Last(), Is.EqualTo(new Vector2(10f, 0f)));
            Assert.That(dashes.Last().First(), Is.EqualTo(new Vector2(9f, 0f)));
        }

        [Test]
        public void FlowOffsetSlidesTheDashesAlongTheLine()
        {
            var dashes = DashGeometry.Dashes(Straight, 2f, 1f, 1f);
            Assert.That(dashes[0].First(), Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(dashes[0].Last(), Is.EqualTo(new Vector2(1f, 0f)));
            Assert.That(dashes[1].First(), Is.EqualTo(new Vector2(2f, 0f)));
        }

        [Test]
        public void ADashAcrossACornerFollowsTheCorner()
        {
            var bent = new[] { new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f) };
            var dashes = DashGeometry.Dashes(bent, 3f, 1f, 0f);
            CollectionAssert.AreEqual(new[] { new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 1f) }, dashes[0]);
        }

        [Test]
        public void NothingIsDrawnForOnePointOrZeroDashLength()
        {
            Assert.That(DashGeometry.Dashes(new[] { Vector2.zero }, 1f, 1f, 0f), Is.Empty);
            Assert.That(DashGeometry.Dashes(Straight, 0f, 1f, 0f), Is.Empty);
        }

        [Test]
        public void LengthAddsEverySegment()
        {
            var bent = new[] { new Vector2(0f, 0f), new Vector2(3f, 0f), new Vector2(3f, 4f) };
            Assert.That(DashGeometry.LengthOf(bent), Is.EqualTo(7f).Within(0.0001f));
        }
    }
}
