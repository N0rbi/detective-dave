using DetectiveDave.LevelPlay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.OverworldPlay
{
    public static class CaseLevelHandoff
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!CaseSession.IsActive || scene.name != nameof(GameScene.Game)) return;
            var loader = Object.FindAnyObjectByType<LevelLoader>();
            if (!loader)
            {
                Debug.LogError("The case cannot start: the Game scene has no Level Loader.");
                return;
            }
            loader.Load(CaseSession.Current.Level);
        }
    }
}
