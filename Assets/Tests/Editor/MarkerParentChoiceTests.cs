using System.Collections.Generic;
using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class MarkerParentChoiceTests
    {
        static readonly string[] None = new string[0];

        [Test]
        public void TheRightClickedObjectWinsOverEverythingElse() =>
            Assert.AreEqual("context", MarkerParentChoice.Pick("context", "stage", "selected", new[] { "scene" }));

        [Test]
        public void TheDeskOpenInPrefabModeWinsOverTheSelection() =>
            Assert.AreEqual("stage", MarkerParentChoice.Pick(null, "stage", "selected", new[] { "scene" }));

        [Test]
        public void TheSelectedDeskWinsOverTheDeskInTheScene() =>
            Assert.AreEqual("selected", MarkerParentChoice.Pick(null, null, "selected", new[] { "scene", "other" }));

        [Test]
        public void TheOnlyDeskInTheSceneIsUsed() =>
            Assert.AreEqual("scene", MarkerParentChoice.Pick(null, null, null, new[] { "scene" }));

        [Test]
        public void SeveralDesksInTheSceneAreNotGuessed() =>
            Assert.IsNull(MarkerParentChoice.Pick(null, null, null, new[] { "one", "two" }));

        [Test]
        public void NoDeskAnywhereGivesNothing() =>
            Assert.IsNull(MarkerParentChoice.Pick<string>(null, null, null, None));
    }
}
