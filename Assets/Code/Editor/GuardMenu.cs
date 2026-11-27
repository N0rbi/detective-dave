using DetectiveDave.GuardPlay;
using DetectiveDave.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    public static class GuardMenu
    {
        [MenuItem("GameObject/Detective Dave/Guard", false, 11)]
        static void CreateGuard(MenuCommand command)
        {
            var level = LevelOf(command.context as GameObject);
            if (!level)
            {
                EditorUtility.DisplayDialog("Guard", "Open a level in Prefab Mode, or select a level in the scene, so the guard has a level to patrol.", "OK");
                return;
            }
            Add(level);
        }

        public static GameObject Add(Level level)
        {
            int number = level.GetComponentsInChildren<GuardPatrol>(true).Length + 1;
            var guard = new GameObject($"Guard {number}");
            Undo.RegisterCreatedObjectUndo(guard, "Create Guard");
            guard.transform.SetParent(level.transform, false);
            guard.AddComponent<GuardPatrol>();
            Selection.activeGameObject = guard;
            SceneView.RepaintAll();
            return guard;
        }

        static Level LevelOf(GameObject context)
        {
            if (context) return context.GetComponentInParent<Level>();
            var selected = Selection.activeGameObject ? Selection.activeGameObject.GetComponentInParent<Level>() : null;
            return selected ? selected : OpenStageLevel();
        }

        static Level OpenStageLevel()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            return stage ? stage.prefabContentsRoot.GetComponent<Level>() : null;
        }
    }
}
