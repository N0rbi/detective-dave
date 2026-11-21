using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class DeskRoot
    {
        public static bool NeedsReset(DeskLayout desk) =>
            IsPrefabRoot(desk) && !DeskRootCheck.IsAtOriginWithUnitScale(desk.transform.localPosition, desk.transform.localScale);

        public static void Reset(DeskLayout desk)
        {
            var serialized = new SerializedObject(desk.transform);
            serialized.FindProperty("m_LocalPosition").vector3Value = Vector3.zero;
            serialized.FindProperty("m_LocalScale").vector3Value = Vector3.one;
            serialized.ApplyModifiedProperties();
        }

        static bool IsPrefabRoot(DeskLayout desk)
        {
            if (EditorUtility.IsPersistent(desk)) return true;
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            return stage && stage.prefabContentsRoot == desk.gameObject;
        }
    }
}
