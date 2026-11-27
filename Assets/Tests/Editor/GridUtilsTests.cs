using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class GridUtilsTests
    {
        const string LayoutJson = @"{
  ""id"": ""grid-utils"", ""name"": ""Grid Utils"", ""form"": ""iso"", ""digit"": 9, ""unitsWide"": 15, ""unitsHigh"": 4,
  ""keys"": [
    { ""code"": ""A"", ""label"": ""A"", ""rects"": [ { ""x"": 0, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 0.5, ""y"": 0.5 } }
  ]
}";

        KeyboardLayout layout;

        [SetUp]
        public void CreateLayout() => layout = KeyboardLayout.Parse(LayoutJson);

        [Test]
        public void ToLocalWithoutStretchIsTheOldMapping()
        {
            Assert.AreEqual(new Vector2(-7f, 1.5f), GridUtils.ToLocal(layout, 1f, new Vector2(0.5f, 0.5f)));
        }

        [Test]
        public void ToLocalStretchesOnlyHeight()
        {
            Assert.AreEqual(new Vector2(-7f, 3f), GridUtils.ToLocal(layout, 1f, new Vector2(0.5f, 0.5f), 2f));
        }

        [Test]
        public void ToLocalSizeStretchesOnlyHeight()
        {
            Assert.AreEqual(new Vector2(1.5f, 3f), GridUtils.ToLocalSize(new Vector2(1.5f, 1.5f), 1f, 2f));
        }

        [Test]
        public void FitScaleIgnoresTheStretch()
        {
            Assert.AreEqual(1f, GridUtils.FitScale(layout, new Vector2(15f, 4f)));
        }

        [Test]
        public void DefaultStretchIsTwo()
        {
            Assert.AreEqual(2f, GridUtils.DefaultHeightStretch);
        }
    }
}
