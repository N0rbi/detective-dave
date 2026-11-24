using DetectiveDave.KeyboardGrid;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class SlicedKeyGeometryTests
    {
        static readonly Vector4 EightPixels = new Vector4(8f, 8f, 8f, 8f);

        [Test]
        public void TheBorderIsDrawnAtTheRequestedThickness()
        {
            var shape = SlicedKeyGeometry.Fit(new Vector2(1f, 2f), EightPixels, 16f, 0.12f);

            Assert.AreEqual(0.24f, shape.Scale, 1e-6f);
            Assert.AreEqual(0.12f, 8f / 16f * shape.Scale, 1e-6f);
        }

        [Test]
        public void TheRendererSizeFillsTheKeyAfterTheScale()
        {
            var target = new Vector2(2.25f, 2f);

            var shape = SlicedKeyGeometry.Fit(target, EightPixels, 16f, 0.12f);

            Assert.AreEqual(target.x, shape.RendererSize.x * shape.Scale, 1e-5f);
            Assert.AreEqual(target.y, shape.RendererSize.y * shape.Scale, 1e-5f);
        }

        [Test]
        public void TheHeightStretchDoesNotChangeTheBorderThickness()
        {
            var plain = SlicedKeyGeometry.Fit(new Vector2(1f, 1f), EightPixels, 16f, 0.12f);
            var stretched = SlicedKeyGeometry.Fit(new Vector2(1f, 2f), EightPixels, 16f, 0.12f);

            Assert.AreEqual(plain.Scale, stretched.Scale, 1e-6f);
            Assert.AreEqual(plain.RendererSize.x, stretched.RendererSize.x, 1e-6f);
            Assert.AreEqual(plain.RendererSize.y * 2f, stretched.RendererSize.y, 1e-5f);
        }

        [Test]
        public void AKeyTooNarrowForTwoBordersShrinksTheBorderToFit()
        {
            var target = new Vector2(0.1f, 1f);

            var shape = SlicedKeyGeometry.Fit(target, EightPixels, 16f, 0.12f);

            Assert.Less(shape.Scale, 0.24f);
            float borderAcross = 16f / 16f * shape.Scale;
            Assert.AreEqual(target.x, borderAcross, 1e-5f);
            Assert.GreaterOrEqual(shape.RendererSize.x * 16f, 16f - 1e-3f);
        }

        [Test]
        public void AKeyTooShortForTwoBordersShrinksTheBorderToFit()
        {
            var shape = SlicedKeyGeometry.Fit(new Vector2(1f, 0.1f), EightPixels, 16f, 0.12f);

            Assert.AreEqual(0.1f, 16f / 16f * shape.Scale, 1e-5f);
        }

        [Test]
        public void TheThinnestBorderSetsTheScaleSoEveryTileOfOneArtShareTheSamePixelSize()
        {
            var shape = SlicedKeyGeometry.Fit(new Vector2(2f, 2f), new Vector4(4f, 8f, 12f, 8f), 16f, 0.2f);

            Assert.AreEqual(0.2f * 16f / 4f, shape.Scale, 1e-6f);
        }

        [Test]
        public void ATallerTopRimKeepsTheScaleOfTheOtherRims()
        {
            var plain = SlicedKeyGeometry.Fit(new Vector2(1f, 2f), new Vector4(65f, 65f, 65f, 65f), 16f, 0.1875f);
            var withPipe = SlicedKeyGeometry.Fit(new Vector2(1f, 2f), new Vector4(65f, 65f, 65f, 97f), 16f, 0.1875f);

            Assert.AreEqual(plain.Scale, withPipe.Scale, 1e-6f);
        }

        [Test]
        public void TheReferenceBorderIgnoresSidesWithoutABorder()
        {
            Assert.AreEqual(6f, SlicedKeyGeometry.ReferenceBorderPixels(new Vector4(0f, 6f, 0f, 9f)));
            Assert.AreEqual(float.MaxValue, SlicedKeyGeometry.ReferenceBorderPixels(Vector4.zero));
        }

        [Test]
        public void ASpriteWithoutABorderIsDrawnAtItsTargetSize()
        {
            var shape = SlicedKeyGeometry.Fit(new Vector2(1f, 2f), Vector4.zero, 16f, 0.12f);

            Assert.AreEqual(1f, shape.Scale);
            Assert.AreEqual(new Vector2(1f, 2f), shape.RendererSize);
        }

        [Test]
        public void AZeroPixelsPerUnitFallsBackToTheTargetSize()
        {
            var shape = SlicedKeyGeometry.Fit(new Vector2(1f, 2f), EightPixels, 0f, 0.12f);

            Assert.AreEqual(1f, shape.Scale);
            Assert.AreEqual(new Vector2(1f, 2f), shape.RendererSize);
        }

        [Test]
        public void ABorderOnOneSideOnlyStillFits()
        {
            var shape = SlicedKeyGeometry.Fit(new Vector2(1f, 1f), new Vector4(0f, 0f, 0f, 6f), 16f, 0.1f);

            Assert.AreEqual(0.1f * 16f / 6f, shape.Scale, 1e-6f);
        }

        [Test]
        public void TheBorderCheckSeesAnySide()
        {
            Assert.IsFalse(SlicedKeyGeometry.HasBorder(Vector4.zero));
            Assert.IsTrue(SlicedKeyGeometry.HasBorder(new Vector4(0f, 1f, 0f, 0f)));
        }
    }
}
