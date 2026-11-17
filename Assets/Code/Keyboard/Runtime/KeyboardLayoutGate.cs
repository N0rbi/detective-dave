using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Put in a scene that needs a locked layout. Sends the player to the layout scene when there is none,
    /// and the layout scene sends them back here once they have picked.
    /// </summary>
    public class KeyboardLayoutGate : MonoBehaviour
    {
        [SerializeField] GameScene redirectTo = GameScene.KeyboardSelect;

        void Start()
        {
            if (KeyboardSession.IsLocked) return;
            KeyboardSession.ReturnTo = Enum.TryParse(SceneManager.GetActiveScene().name, out GameScene current) ? current : (GameScene?)null;
            SceneLoader.Load(redirectTo);
        }
    }
}
