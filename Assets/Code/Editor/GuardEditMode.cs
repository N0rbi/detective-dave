namespace DetectiveDave.EditorTools
{
    public enum PatrolTarget
    {
        Start,
        End
    }

    public sealed class GuardEditMode
    {
        public bool IsActive { get; private set; }

        public int GuardId { get; private set; }

        public PatrolTarget Target { get; private set; }

        public void Enter(int guardId, bool guardHasStart)
        {
            IsActive = true;
            GuardId = guardId;
            Target = TargetFor(guardHasStart);
        }

        public void SwitchGuard(int guardId, bool guardHasStart)
        {
            if (!IsActive || guardId == GuardId) return;
            Enter(guardId, guardHasStart);
        }

        public void ChooseTarget(PatrolTarget target)
        {
            if (IsActive) Target = target;
        }

        public PatrolTarget? Click(bool endModifierHeld)
        {
            if (!IsActive) return null;
            var placed = endModifierHeld ? PatrolTarget.End : Target;
            Target = PatrolTarget.End;
            return placed;
        }

        public void Exit() => IsActive = false;

        static PatrolTarget TargetFor(bool guardHasStart) => guardHasStart ? PatrolTarget.End : PatrolTarget.Start;
    }
}
