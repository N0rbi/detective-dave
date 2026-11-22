using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    public enum GuardActionKind
    {
        Stay,
        StepTo,
        Redirect,
        Catch
    }

    public readonly struct GuardAction
    {
        public GuardActionKind Kind { get; }

        public Key Target { get; }

        public float Seconds { get; }

        public bool IsChasing { get; }

        GuardAction(GuardActionKind kind, Key target, float seconds, bool isChasing)
        {
            Kind = kind;
            Target = target;
            Seconds = seconds;
            IsChasing = isChasing;
        }

        public static GuardAction Stay() => new GuardAction(GuardActionKind.Stay, default, 0f, false);

        public static GuardAction StepTo(Key target, float seconds, bool isChasing) => new GuardAction(GuardActionKind.StepTo, target, seconds, isChasing);

        public static GuardAction Redirect(Key target, float seconds, bool isChasing) => new GuardAction(GuardActionKind.Redirect, target, seconds, isChasing);

        public static GuardAction Catch() => new GuardAction(GuardActionKind.Catch, default, 0f, true);
    }
}
