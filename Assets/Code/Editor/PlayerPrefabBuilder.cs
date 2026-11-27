using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class PlayerPrefabBuilder
    {
        public static PlayerView Ensure()
        {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(PlayerSetupPaths.PrefabPath)) return Create();
            var root = PrefabUtility.LoadPrefabContents(PlayerSetupPaths.PrefabPath);
            try
            {
                if (AddOrRepairPieces(root)) PrefabUtility.SaveAsPrefabAsset(root, PlayerSetupPaths.PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerSetupPaths.PrefabPath).GetComponent<PlayerView>();
        }

        static PlayerView Create()
        {
            var root = new GameObject("Player");
            AddOrRepairPieces(root);
            var saved = PrefabUtility.SaveAsPrefabAsset(root, PlayerSetupPaths.PrefabPath);
            Object.DestroyImmediate(root);
            return saved.GetComponent<PlayerView>();
        }

        static bool AddOrRepairPieces(GameObject root)
        {
            var changed = AddIfMissing<SpriteRenderer>(root);
            changed |= AddIfMissing<PlayerView>(root);
            changed |= AddIfMissing<PlayerAnimations>(root);
            if (!root.activeSelf)
            {
                root.SetActive(true);
                changed = true;
            }
            return changed;
        }

        static bool AddIfMissing<T>(GameObject target) where T : Component
        {
            if (target.GetComponent<T>()) return false;
            target.AddComponent<T>();
            return true;
        }
    }
}
