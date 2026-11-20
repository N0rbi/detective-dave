using System;

namespace DetectiveDave.PlayerPlay
{
    public sealed class PlayerStateMachine
    {
        public PlayerState State { get; private set; } = PlayerState.NotStarted;

        public bool AcceptsMoves => State == PlayerState.Idle;

        public event Action<PlayerState> Changed;

        public void Enter(PlayerState next)
        {
            if (State == PlayerState.Goal || State == next) return;
            Set(next);
        }

        public void Begin()
        {
            if (State == PlayerState.NotStarted) Enter(PlayerState.Idle);
        }

        public void Reset()
        {
            if (State != PlayerState.NotStarted) Set(PlayerState.NotStarted);
        }

        void Set(PlayerState next)
        {
            State = next;
            Changed?.Invoke(next);
        }
    }
}
