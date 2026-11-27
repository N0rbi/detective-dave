using DetectiveDave.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class DeskRootCheckTests
    {
        [Test]
        public void ARootAtTheOriginWithUnitScaleIsClean() =>
            Assert.IsTrue(DeskRootCheck.IsAtOriginWithUnitScale(Vector3.zero, Vector3.one));

        [Test]
        public void AMovedRootIsNotClean() =>
            Assert.IsFalse(DeskRootCheck.IsAtOriginWithUnitScale(new Vector3(0f, 2f, 0f), Vector3.one));

        [Test]
        public void AScaledRootIsNotClean() =>
            Assert.IsFalse(DeskRootCheck.IsAtOriginWithUnitScale(Vector3.zero, new Vector3(2f, 2f, 1f)));

        [Test]
        public void TinyRoundingIsIgnored() =>
            Assert.IsTrue(DeskRootCheck.IsAtOriginWithUnitScale(new Vector3(0.00001f, 0f, 0f), new Vector3(1.00001f, 1f, 1f)));
    }
}
