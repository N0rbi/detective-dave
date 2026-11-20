using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class PlayerFacingTests
    {
        readonly PlayerFacing facing = new PlayerFacing();

        [Test]
        public void MovingLeftFacesLeft()
        {
            facing.ResolveMove(new Vector2(2f, 0f), new Vector2(1f, 0f));
            Assert.AreEqual(Facing.Left, facing.Current);
        }

        [Test]
        public void VerticalMoveKeepsFacing()
        {
            facing.Set(Facing.Left);
            facing.ResolveMove(new Vector2(1f, 0f), new Vector2(1.005f, 1f));
            Assert.AreEqual(Facing.Left, facing.Current);
        }

        [Test]
        public void BackwardsIsOppositeAlsoAfterVerticalMove()
        {
            facing.Set(Facing.Right);
            facing.ResolveMove(Vector2.zero, Vector2.up);
            Assert.AreEqual(Facing.Left, facing.Backwards);
        }

        [Test]
        public void BackwardsOfRightIsLeft()
        {
            facing.Set(Facing.Right);
            Assert.AreEqual(Facing.Left, facing.Backwards);
        }

        [Test]
        public void BackwardsOfLeftIsRight()
        {
            facing.Set(Facing.Left);
            Assert.AreEqual(Facing.Right, facing.Backwards);
        }

        [Test]
        public void DefaultFacesTowardsGoal()
        {
            facing.SetTowardsGoal(new Vector2(5f, 0f), new Vector2(1f, 0f));
            Assert.AreEqual(Facing.Left, facing.Current);
        }

        [Test]
        public void DefaultIsRightWhenGoalIsAbove()
        {
            facing.Set(Facing.Left);
            facing.SetTowardsGoal(new Vector2(1f, 0f), new Vector2(1f, 3f));
            Assert.AreEqual(Facing.Right, facing.Current);
        }

        [Test]
        public void ResetToDefaultRestoresTheGoalFacing()
        {
            facing.SetTowardsGoal(new Vector2(5f, 0f), new Vector2(1f, 0f));
            facing.ResolveMove(Vector2.zero, Vector2.right);
            facing.ResetToDefault();
            Assert.AreEqual(Facing.Left, facing.Current);
        }

        [Test]
        public void BackwardsDirectionOppositeToFacing()
        {
            facing.Set(Facing.Right);
            Assert.AreEqual(Vector2.left, facing.BackwardsDirection());
            facing.Set(Facing.Left);
            Assert.AreEqual(Vector2.right, facing.BackwardsDirection());
        }
    }
}
