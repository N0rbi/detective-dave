using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class PlayerFacing
    {
        const float HorizontalEpsilon = 0.01f;

        Facing defaultFacing = Facing.Right;

        public Facing Current { get; private set; } = Facing.Right;

        public void ResolveMove(Vector2 from, Vector2 to)
        {
            float dx = to.x - from.x;
            if (dx > HorizontalEpsilon) Current = Facing.Right;
            else if (dx < -HorizontalEpsilon) Current = Facing.Left;
        }

        public Vector2 ForwardDirection() => Current == Facing.Right ? Vector2.right : Vector2.left;

        public void SetTowardsGoal(Vector2 start, Vector2 goal)
        {
            defaultFacing = goal.x < start.x ? Facing.Left : Facing.Right;
            Current = defaultFacing;
        }

        public void ResetToDefault() => Current = defaultFacing;

        public void Set(Facing facing) => Current = facing;
    }
}
