using UnityEngine;

namespace DetectiveDave.EditorTools
{
    public static class DeskRootCheck
    {
        public const string Warning = "The desk root is not at position 0 with scale 1. Case files are measured from the keyboard grid, so the desk shown here will not match the keys in the game.";

        const float Tolerance = 0.0001f;

        public static bool IsAtOriginWithUnitScale(Vector3 position, Vector3 scale) =>
            position.sqrMagnitude < Tolerance * Tolerance && (scale - Vector3.one).sqrMagnitude < Tolerance * Tolerance;
    }
}
