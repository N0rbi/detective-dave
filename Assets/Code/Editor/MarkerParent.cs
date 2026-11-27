using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class MarkerParent
    {
        public static Transform Find(GameObject context)
        {
            return MarkerParentChoice.Pick(
                context ? context.transform : null,
                OpenDesk(),
                SelectedDesk(),
                Object.FindObjectsByType<DeskLayout>(FindObjectsSortMode.None).Select(desk => desk.transform).ToArray());
        }

        static Transform OpenDesk()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (!stage) return null;
            var desk = stage.prefabContentsRoot.GetComponent<DeskLayout>();
            return desk ? desk.transform : null;
        }

        static Transform SelectedDesk()
        {
            var selected = Selection.activeGameObject;
            if (!selected || EditorUtility.IsPersistent(selected)) return null;
            var desk = selected.GetComponentInParent<DeskLayout>();
            return desk ? desk.transform : null;
        }
    }
}
