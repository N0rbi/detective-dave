using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>Put in a scene that needs a locked layout. Sends the player to the layout scene when there is none.</summary>
    public class KeyboardLayoutGate : MonoBehaviour
    {
        [SerializeField] GameScene redirectTo = GameScene.KeyboardSelect;

        void Start()
        {
            if (!KeyboardSession.IsLocked) SceneLoader.Load(redirectTo);
        }
    }
}
