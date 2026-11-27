using System;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    /// <summary>
    /// Plays the player's motion. Contract for every method taking <c>done</c>:
    /// done is invoked at most once, and only when the animation finishes naturally; never synchronously inside the call;
    /// never after <see cref="CancelAllWithoutCallbacks"/>, <see cref="ShowResetAt"/>, a newer animation, or OnDisable/OnDestroy.
    /// Starting any animation cancels a running one without its callback.
    /// Breathing is a loop with no callback: <see cref="StartBreathing"/> does nothing when it already runs, and it stops (scale back to <see cref="PlayerView.BaseLocalScale"/>) on <see cref="StopBreathing"/>, <see cref="CancelAllWithoutCallbacks"/>, <see cref="ShowResetAt"/> and before any other animation.
    /// <see cref="BumpWall"/> walks to <c>towards</c>, squashes, and ends back exactly at <c>home</c> with the base scale; <c>returning</c> follows the same rules as <c>done</c> and is invoked when the way back starts.
    /// <see cref="ShowResetAt"/> fully resets position, localScale (to <see cref="PlayerView.BaseLocalScale"/>), rotation (identity) and alpha (1).
    /// </summary>
    public interface IPlayerAnimations
    {
        void ShowResetAt(Vector2 world);

        void MoveTo(Vector2 world, Action done);

        void Stun(Action done);

        void BumpWall(Vector2 towards, Vector2 home, Action returning, Action done);

        void FallFlat(Action done);

        void StartBreathing();

        void StopBreathing();

        void CancelAllWithoutCallbacks();
    }
}
