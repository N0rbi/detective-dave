using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave
{
    /// <summary>
    /// The only place that loads scenes. Scenes are named by <see cref="GameScene"/>, never by a string.
    /// A scene missing from the build list stops the run when it starts, not when the player reaches it.
    /// In the editor, BuildSceneRegistrar adds missing scenes to the list before Play Mode starts.
    /// </summary>
    public static class SceneLoader
    {
        public static bool IsInBuild(GameScene scene) => Application.CanStreamedLevelBeLoaded(scene.ToString());

        public static void Load(GameScene scene)
        {
            if (!IsInBuild(scene)) throw new InvalidOperationException(MissingMessage(new[] { scene }));
            SceneManager.LoadScene(scene.ToString());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void VerifyBuildList()
        {
            var missing = Enum.GetValues(typeof(GameScene)).Cast<GameScene>().Where(scene => !IsInBuild(scene)).ToArray();
            if (missing.Length == 0) return;

            Debug.LogError(MissingMessage(missing));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static string MissingMessage(IEnumerable<GameScene> missing) =>
            $"Scenes missing from the build list: {string.Join(", ", missing)}. Add them under File > Build Profiles.";
    }
}
